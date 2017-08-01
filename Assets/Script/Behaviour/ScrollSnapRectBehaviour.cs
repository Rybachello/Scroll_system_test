using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Script.Behaviour {
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(Mask))]
    [RequireComponent(typeof(ScrollRect))]
    public class ScrollSnapRectBehaviour : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler {

        public int StartingPage = 0;
        [Tooltip("Threshold time for fast swipe in seconds")] public float FastSwipeThresholdTime = 0.3f;
        [Tooltip("Threshold time for fast swipe in (unscaled) pixels")] public int FastSwipeThresholdDistance = 100;
        [Tooltip("How fast will page lerp to target position")] public float DecelerationRate = 10f;
        [Tooltip("Sprite for unselected page (optional)")] public Color UnselectedPageColor;
        [Tooltip("Sprite for selected page (optional)")] public Color SelectedPageColor;
        [Tooltip("Container with page images (optional)")] public Transform PageSelectionIcons;

        private int _fastSwipeThresholdMaxLimit;
        // fast swipes should be fast and short. If too long, then it is not fast swipe

        private ScrollRect _scrollRectComponent;
        private RectTransform _scrollRectRect;
        private RectTransform _scrollContainer;

        private bool _horizontal;

        // number of pages in container
        private int _pageCount;
        private int _currentPage;

        // whether lerping is in progress and target lerp position
        private bool _lerp;
        private Vector2 _lerpTo;

        // target position of every page
        private readonly List<Vector2> _pagePositions = new List<Vector2>();

        // in draggging, when dragging started and where it started
        private bool _dragging;
        private float _timeStamp;
        private Vector2 _startPosition;

        private int _previousPageSelectionIndex;
        // container with Image components - one Image for each page
        private List<Image> _pageSelectionImages;

        private TreasurePanelBehaviour _treasurePanelBehaviour;

        private void Start() {
            Init();
        }

        private void Init() {
            _scrollRectComponent = gameObject.GetComponent<ScrollRect>();
            if (!_scrollRectComponent) {
                Debug.LogWarning("Cannot find ScrollRect");
            }
            _scrollContainer = _scrollRectComponent.content;
            _scrollRectRect = gameObject.GetComponent<RectTransform>();
            if (!_scrollRectRect) {
                Debug.LogWarning("Cannot find ScrollRect Transform");
            }

            _treasurePanelBehaviour = gameObject.GetComponentInParent<TreasurePanelBehaviour>();
            if (!_treasurePanelBehaviour) {
                Debug.LogWarning("Cannot find TreasurePanelBehaviour");
            }
            _pageCount = _scrollContainer.childCount;

            // is it horizontal or vertical scrollrect
            if (_scrollRectComponent.horizontal && !_scrollRectComponent.vertical) {
                _horizontal = true;
            }
            else if (!_scrollRectComponent.horizontal && _scrollRectComponent.vertical) {
                _horizontal = false;
            }
            else {
                Debug.LogWarning("Confusing setting of horizontal/vertical direction. Default set to horizontal.");
                _horizontal = true;
            }

            _lerp = false;

            SetPagePositions();
            SetPage(StartingPage);
            InitPageSelection();
            SetPageSelection(StartingPage);
        }

        private void SetPagePositions() {
            var width = 0;
            var height = 0;
            var offsetX = 0;
            var offsetY = 0;
            var containerWidth = 0;
            var containerHeight = 0;

            if (_horizontal) {
                // screen width in pixels of scrollrect window
                // width = (int)_scrollRectRect.rect.width/4;
                width = (int) _scrollRectRect.rect.width / 2;
                // center position of all pages
                offsetX = width / 2;
                // total width
                containerWidth = width * _pageCount;
                // limit fast swipe length - beyond this length it is fast swipe no more
                _fastSwipeThresholdMaxLimit = width;
            }
            else {
                height = (int) _scrollRectRect.rect.height;
                offsetY = height / 2;
                containerHeight = height * _pageCount;
                _fastSwipeThresholdMaxLimit = height;
            }

            // set width of container
            var newSize = new Vector2(containerWidth, containerHeight);
            _scrollContainer.sizeDelta = newSize;
            var newPosition = new Vector2(containerWidth / 2f, containerHeight / 2f);
            _scrollContainer.anchoredPosition = newPosition;

            _pagePositions.Clear();

            for (var i = 0; i < _pageCount; i++) {
                var child = _scrollContainer.GetChild(i).GetComponent<RectTransform>();
                var childPosition = _horizontal
                    ? new Vector2(i * width - containerWidth / 2 + offsetX, 0f)
                    : new Vector2(0f, -(i * height - containerHeight / 2 + offsetY));
                child.anchoredPosition = childPosition;
                _pagePositions.Add(-childPosition);
            }
        }

        private void Update() {
            // if moving to target position
            if (_lerp) {
                // prevent overshooting with values greater than 1
                float decelerate = Mathf.Min(DecelerationRate * Time.deltaTime, 1f);
                _scrollContainer.anchoredPosition = Vector2.Lerp(_scrollContainer.anchoredPosition, _lerpTo, decelerate);
                // time to stop lerping?
                if (Vector2.SqrMagnitude(_scrollContainer.anchoredPosition - _lerpTo) < 0.25f) {
                    // snap to target and stop lerping
                    _scrollContainer.anchoredPosition = _lerpTo;
                    _lerp = false;
                    // clear also any scrollrect move that may interfere with our lerping
                    _scrollRectComponent.velocity = Vector2.zero;
                }
                SetPageSelection(GetNearestPage());
            }
        }

        private void SetPage(int aPageIndex) {
            aPageIndex = Mathf.Clamp(aPageIndex, 0, _pageCount - 1);
            _scrollContainer.anchoredPosition = _pagePositions[aPageIndex];
            CurrentPage = aPageIndex;
        }

        private void LerpToPage(int aPageIndex) {
            aPageIndex = Mathf.Clamp(aPageIndex, 0, _pageCount - 1);
            if (aPageIndex != _currentPage) {
                _treasurePanelBehaviour.PlayAnimation(_currentPage, "Unselected");
            }
            _lerpTo = _pagePositions[aPageIndex];
            _lerp = true;
            CurrentPage = aPageIndex;
        }

        private void InitPageSelection() {
            _previousPageSelectionIndex = -1;
            _pageSelectionImages = new List<Image>();

            // cache all Image components into list
            for (int i = 0; i < PageSelectionIcons.childCount; i++) {
                Image image = PageSelectionIcons.GetChild(i).GetComponent<Image>();
                if (image == null) {
                    Debug.LogWarning("Page selection icon at position " + i + " is missing Image component");
                }
                _pageSelectionImages.Add(image);
            }
        }

        private void SetPageSelection(int aPageIndex) {
            // nothing to change
            if (_previousPageSelectionIndex == aPageIndex) {
                return;
            }
            // unselect old
            if (_previousPageSelectionIndex >= 0) {
                _pageSelectionImages[_previousPageSelectionIndex].color = UnselectedPageColor;
            }
            // select new
            _pageSelectionImages[aPageIndex].color = SelectedPageColor;

            _previousPageSelectionIndex = aPageIndex;
        }

        private void NextScreen() {
            LerpToPage(_currentPage + 1);
        }

        private void PreviousScreen() {
            LerpToPage(_currentPage - 1);
        }

        private int GetNearestPage() {
            // based on distance from current position, find nearest page
            Vector2 currentPosition = _scrollContainer.anchoredPosition;

            float distance = float.MaxValue;
            int nearestPage = _currentPage;

            for (int i = 0; i < _pagePositions.Count; i++) {
                float testDist = Vector2.SqrMagnitude(currentPosition - _pagePositions[i]);
                if (testDist < distance) {
                    distance = testDist;
                    nearestPage = i;
                }
            }
            return nearestPage;
        }

        #region Dragging

        public void OnBeginDrag(PointerEventData aEventData) {
            _lerp = false; // if currently lerping, then stop it as user is draging
            _dragging = false; // not dragging yet
        }

        public void OnEndDrag(PointerEventData aEventData) {
            float difference; // how much was container's content dragged
            if (_horizontal) {
                difference = _startPosition.x - _scrollContainer.anchoredPosition.x;
            }
            else {
                difference = -(_startPosition.y - _scrollContainer.anchoredPosition.y);
            }

            // test for fast swipe - swipe that moves only +/-1 item
            if (Time.unscaledTime - _timeStamp < FastSwipeThresholdTime &&
                Mathf.Abs(difference) > FastSwipeThresholdDistance &&
                Mathf.Abs(difference) < _fastSwipeThresholdMaxLimit) {
                if (difference > 0) {
                    NextScreen();
                }
                else {
                    PreviousScreen();
                }
            }
            else {
                // if not fast time, look to which page we got to
                LerpToPage(GetNearestPage());
            }
            _dragging = false;
        }

        public void OnDrag(PointerEventData aEventData) {
            if (!_dragging) {
                // dragging started
                _dragging = true;
                // save time - unscaled so pausing with Time.scale should not affect it
                _timeStamp = Time.unscaledTime;
                _startPosition = _scrollContainer.anchoredPosition;
            }
            else {
                SetPageSelection(GetNearestPage());
            }
        }

        #endregion

        #region buttons

        private int CurrentPage {
            get { return _currentPage; }
            set {
                if (value == _currentPage) {
                    return;
                }
                var index = Mathf.Clamp(value, 0, _pageCount - 1);
                _treasurePanelBehaviour.PlayAnimation(index, "Selected");
                _currentPage = value;
            }
        }

        public void OnPrevButtonClick() {
            PreviousScreen();
        }

        public void OnNextButtonClick() {
            NextScreen();
        }

        #endregion

        public RectTransform ScrollContent {
            get { return !_scrollContainer ? GetComponent<ScrollRect>().content : _scrollContainer; }
        }
    }
}
