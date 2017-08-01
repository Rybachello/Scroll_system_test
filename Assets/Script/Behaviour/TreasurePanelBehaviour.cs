using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Script.Behaviour {
    public class TreasurePanelBehaviour : MonoBehaviour {

        [SerializeField] private List<TreasurePrefabBehaviour> _treasurePrefabList;
        private ScrollSnapRectBehaviour _scrollSnapRectBehaviour;
        private RectTransform _pageIcons;
        private GameObject _treasurePrefab;
        [SerializeField] private Sprite _iconSprite; // todo: move it and all loading to another script

        private void Awake() {
            Init();
        }

        private void Init() {
            _scrollSnapRectBehaviour = gameObject.GetComponentInChildren<ScrollSnapRectBehaviour>();
            if (!_scrollSnapRectBehaviour) {
                Debug.LogWarning("Cannot find Scroll Rect");
            }

            _treasurePrefab = Resources.Load("Prefabs/treasure") as GameObject;
            if (!_treasurePrefab) {
                Debug.LogWarning("Cannot find treasure prefab");
            }

            _treasurePrefabList = CreateTreasurePrefabs(_scrollSnapRectBehaviour.ScrollContent);
            PlayAnimation(0, "Selected"); //initial
            _pageIcons = CreatePageSelectionIcons();
        }

        private RectTransform CreatePageSelectionIcons(string childName = "Page Icons") {
            var pageIconsGameOb = gameObject.transform.FindChild(childName).GetComponent<RectTransform>();
            if (!pageIconsGameOb) {
                Debug.LogWarning("Cannot find pageIconsGameOb gameobject");
            }
            for (var i = 0; i < ListSize; i++) {
                var iconGameOb = new GameObject();
                iconGameOb.gameObject.transform.SetParent(pageIconsGameOb);
                var rectTrasform = iconGameOb.AddComponent<RectTransform>();
                iconGameOb.name = "icon_" + i;
                rectTrasform.sizeDelta = new Vector2(40, 40);
                rectTrasform.anchoredPosition = new Vector2(-512 + i * 55, 0);
                var iconImage = iconGameOb.AddComponent<Image>();
                iconImage.sprite = _iconSprite;
                iconImage.color = new Color32(44, 90, 113, 255);
            }
            return pageIconsGameOb;
        }

        private const int ListSize = 20;

        private List<TreasurePrefabBehaviour> CreateTreasurePrefabs(RectTransform contentTransform) {

            var treasureList = new List<TreasurePrefabBehaviour>(ListSize);
            for (int i = 0; i < ListSize; i++) {
                var treasurePrefabGameOb = Instantiate(_treasurePrefab, contentTransform);
                treasurePrefabGameOb.name = "treasure_" + i;
                treasurePrefabGameOb.gameObject.SetActive(false);
                var treasurePrefabBehaviour = treasurePrefabGameOb.GetComponent<TreasurePrefabBehaviour>();
                treasureList.Add(treasurePrefabBehaviour);
            }
            return treasureList;
        }

        public void PlayAnimation(int index, string animationName) {
            var treasurePrefabBehaviour = _treasurePrefabList[index];
            if (treasurePrefabBehaviour.gameObject.activeInHierarchy == false)
                treasurePrefabBehaviour.gameObject.SetActive(true);
            UpdateTreasureGameObjects(index);
            treasurePrefabBehaviour.PlayAnim(animationName);
        }

        private void UpdateTreasureGameObjects(int index) {
            if (index != 0 && index != 1) {
                _treasurePrefabList[index - 2].gameObject.SetActive(false);
            }
            if (index != 0) {
                _treasurePrefabList[index - 1].gameObject.SetActive(true);
            }
            if (index != ListSize - 1) {
                _treasurePrefabList[index + 1].gameObject.SetActive(true);
            }
            if (index != ListSize - 2 && index != ListSize - 1) {
                _treasurePrefabList[index + 2].gameObject.SetActive(false);
            }
        }

        public List<TreasurePrefabBehaviour> TreasurePrefabList {
            get { return _treasurePrefabList; }
        }
    }
}
