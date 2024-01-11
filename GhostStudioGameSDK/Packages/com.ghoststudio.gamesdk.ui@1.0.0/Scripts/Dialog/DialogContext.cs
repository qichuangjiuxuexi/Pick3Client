using System.Collections.Generic;

namespace AppBase.UI.Dialog
{
    /// <summary>
    /// 对话框上下文数据
    /// </summary>
    public abstract class DialogContext
    {
        public virtual void OnLoadedCallback(UIDialog obj)
        {
        }

        public virtual void OnOpenCallback(UIDialog obj)
        {
        }

        public virtual void OnCloseCallback(UIDialog obj)
        {
        }
    }
    
    /// <summary>
    /// 获取并移除DialogContext的扩展方法
    /// </summary>
    public static class DialogContextExtension
    {
        /// <summary>
        /// 获取并移除DialogContext，用于将上下文传递给下一个对话框
        /// </summary>
        public static T GetAndRemove<T>(this List<DialogContext> list) where T : DialogContext
        {
            if (list == null || list.Count == 0) return null;
            var dialogContext = (T)list.Find(x => x is T);
            if (dialogContext == null) return null;
            list.Remove(dialogContext);
            return dialogContext;
        }
        
        /// <summary>
        /// 传递DialogContext给下一个对话框
        /// </summary>
        public static T MoveFrom<T>(this List<DialogContext> newList, List<DialogContext> oldList) where T : DialogContext
        {
            if (oldList == null || oldList.Count == 0 || newList == null) return null;
            var dialogContext = oldList.GetAndRemove<T>();
            if (dialogContext == null) return null;
            newList.Add(dialogContext);
            return dialogContext;
        }
        
        /// <summary>
        /// 传递DialogContext给下一个对话框
        /// </summary>
        public static T MoveTo<T>(this List<DialogContext> oldList, List<DialogContext> newList) where T : DialogContext
        {
            return newList.MoveFrom<T>(oldList);
        }
    }
}
