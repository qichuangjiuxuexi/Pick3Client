using System;
using System.Collections;
using UnityEngine;


namespace WordGame.Utils
{
    public class AnimatorCoroutineMono : MonoBehaviour
    {
        private Animator animator;

        [SerializeField]
        private string animName;
        public string AnimName => animName;
        
        private void Awake()
        {
            animator = GetComponent<Animator>();
        }
        public void DelayAni(string animName,Action callback)
        {
            this.animName = animName;
            if (!gameObject.activeInHierarchy)
            {
                ClearSelf();
                return;
            }
            StartCoroutine(_DelayAni(callback));
        }
        private IEnumerator _DelayAni(Action callback)
        {
            float length = animator.GetCurrentAnimatorStateInfo(0).length;
            if (animator.updateMode == AnimatorUpdateMode.UnscaledTime)
            {
                yield return new WaitForSecondsRealtime(length);
            }
            else
            {
                yield return new WaitForSeconds(length);
            }
            callback?.Invoke();
            ClearSelf();
        }
        private void ClearSelf()
        {
            Destroy(this);
        }
    }
}
