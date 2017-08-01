using Assets.Script.Behaviour;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Script.Managers {
    public class UIManager : MonoBehaviour {

        private UIManager _instance; //todo: finish singlethon

        [SerializeField] private Sprite _backgroundSprite;
        private Image _bgImage;
        private TreasurePanelBehaviour _treasurePanelBehaviour;
        //todo: for managin open/close treauser menu
        
        private void Awake() {
            Init();
        }

        private void Init() {

            _instance = this;
            _treasurePanelBehaviour = gameObject.GetComponentInChildren<TreasurePanelBehaviour>();

            InitBackground();
        }

        private void InitBackground() {
            _bgImage = gameObject.GetComponentInChildren<Image>();
            if (!_bgImage)
            {
                Debug.LogWarning("BG image is not found");
                return;
            }
            _bgImage.sprite = _backgroundSprite;
            _bgImage.transform.SetAsFirstSibling();
        }

        private void OnDestroy() {
            _instance = null;
        }
    }
}
