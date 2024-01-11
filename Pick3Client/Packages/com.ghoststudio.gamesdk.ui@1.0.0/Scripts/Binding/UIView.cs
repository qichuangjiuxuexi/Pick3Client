using System.Collections.Generic;

namespace AppBase.UI
{
    /// <summary>
    /// UI框架基类
    /// </summary>
    public class UIView : UIBinding
    {
        /// <summary>
        /// 绑定缓存
        /// </summary>
        private Dictionary<string, UIBinding> bindings;

        /// <summary>
        /// 查找UIBinding并缓存
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns>组件</returns>
        public T FindUIBinding<T>(string path) where T : UIBinding
        {
            if (bindings != null && bindings.TryGetValue(path, out var binding) && binding != null) return (T)binding;
            binding = transform.Find(path)?.GetComponent<T>();
            if (binding != null)
            {
                bindings ??= new Dictionary<string, UIBinding>();
                bindings[path] = binding;
            }
            return (T)binding;
        }
    }
}
