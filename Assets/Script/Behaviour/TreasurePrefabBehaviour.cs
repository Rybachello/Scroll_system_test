using UnityEngine;
using UnityEngine.UI;

namespace Assets.Script.Behaviour {
    public class TreasurePrefabBehaviour : MonoBehaviour {

        private Animator _animator;
        private void Awake() {
            Init();
        }
        //private Image 
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
