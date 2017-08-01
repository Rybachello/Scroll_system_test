using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Script.Behaviour {
    public class TreasurePanelBehaviour : MonoBehaviour {

        [SerializeField] private List<TreasurePrefabBehaviour> _treasurePrefabList;
        private ScrollSnapRectBehaviour _scrollSnapRectBehaviour;
        private RectTransform _pageIcons; //todo:implement later
        private GameObject _treasurePrefab;

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
        }

        private const int ListSize = 20;

        private List<TreasurePrefabBehaviour> CreateTreasurePrefabs(RectTransform contentTransform) {

            var treasureList = new List<TreasurePrefabBehaviour>(ListSize);
            for (int i = 0; i < ListSize; i++) {
                var treasurePrefabGameOb = Instantiate(_treasurePrefab,contentTransform);
                treasurePrefabGameOb.name = "treasure_" + i;

                var treasurePrefabBehaviour = treasurePrefabGameOb.GetComponent<TreasurePrefabBehaviour>();
                treasureList.Add(treasurePrefabBehaviour);
            }
            treasureList[0].PlayAnim("Selected"); //initial
            return treasureList;
        }

        public void PlayAnimation(int index, string animationName)
        {
            var treasurePrefabBehaviour = _treasurePrefabList[index];
            treasurePrefabBehaviour.PlayAnim(animationName);
        }
        public List<TreasurePrefabBehaviour> TreasurePrefabList {
            get { return _treasurePrefabList; }
        }

       
    }
}
