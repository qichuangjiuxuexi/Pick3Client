using UnityEngine;

namespace AppBase.UI.Dialog
{
    /// <summary>
    /// Dialog生命周期助手
    /// </summary>
    public class DialogRuntimeComponent : MonoBehaviour
    {
        UIDialog dialog;

        protected virtual void Awake()
        {
            dialog = GetComponent<UIDialog>();
            if (dialog != null)
            {
                dialog.OnBindComponents();
                dialog.OnAwake();
            }
        }

        protected virtual void Start()
        {
            if (dialog != null)
            {
                dialog.OnStart();
                dialog.PlayOpenAnim(() =>
                {
                    dialog.dialogData?.OnOpenCallback(dialog);
                });
            }
        }
    }
}
