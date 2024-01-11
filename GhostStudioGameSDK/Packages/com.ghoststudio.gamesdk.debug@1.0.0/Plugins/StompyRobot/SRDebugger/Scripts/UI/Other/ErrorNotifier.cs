using UnityEngine;

namespace SRDebugger.UI.Other
{
    public class ErrorNotifier : MonoBehaviour
    {
        public bool IsVisible
        {
            get { return _isShowing; }
        }

        [SerializeField]
        private Animator _animator = null;

        private int _triggerHash;

        private bool _isShowing;

        void Awake()
        {
            _triggerHash = Animator.StringToHash("Display");
        }

        public void ShowErrorWarning()
        {
            ToggleAnim(true);
        }

        public void HideErrorWarning()
        {
            ToggleAnim(false);
        }
        
        private void ToggleAnim(bool isShow)
        {
            _animator.SetBool(_triggerHash, isShow);
            _isShowing = isShow;
        }
    }
}