using UnityEngine;
using UnityEngine.UI;

namespace Assets.Script.Behaviour {
    public class TreasurePrefabBehaviour : MonoBehaviour {
        //todo: maybe add text and other staff later
        private Animator _animator;

        private void Awake() {
            Init();
        }

        private void Init() {
            _animator = gameObject.GetComponent<Animator>();
        }

        public void PlayAnim(string animationName) {
            if (!_animator) {
                Init();
            }
            _animator.Play(animationName);
        }
    }
}
