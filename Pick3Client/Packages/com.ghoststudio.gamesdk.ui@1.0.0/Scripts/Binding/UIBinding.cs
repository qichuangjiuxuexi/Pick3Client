using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AppBase.UI
{
    /// <summary>
    /// 通用UI绑定组件
    /// </summary>
    public class UIBinding : MonoBehaviour
    {
        /// <summary>
        /// 绑定的变量名
        /// </summary>
        public string BindingName;
        private Dictionary<Type, Component> components;
        
        public RectTransform RectTransform => Get<RectTransform>();
        public Button Button => Get<Button>();
        public Toggle Toggle => Get<Toggle>();
        public Image Image => Get<Image>();
        public SpriteRenderer SpriteRenderer => Get<SpriteRenderer>();
        public InputField InputField => Get<InputField>();
        public TextMeshProUGUI TextMeshProUGUI => Get<TextMeshProUGUI>();
        public TextMeshPro TextMeshPro => Get<TextMeshPro>();
        public Animator Animator => Get<Animator>();
        public LocalizationTextBinder LocalizationTextBinder => GetOrAdd<LocalizationTextBinder>();
        
        /// <summary>
        /// 获取组件并缓存
        /// </summary>
        public T Get<T>() where T : Component
        {
            if (components != null && components.TryGetValue(typeof(T), out var component) && component != null)
            {
                return (T)component;
            }
            component = GetComponent<T>();
            if (component == null)
            {
                Debugger.LogError(nameof(UIBinding), $"UIBinding GetComponent failed, {typeof(T).Name} not found");
                return null;
            }
            components ??= new Dictionary<Type, Component>();
            components[typeof(T)] = component;
            return (T)component;
        }
        
        /// <summary>
        /// 获取或添加组件并缓存
        /// </summary>
        public T GetOrAdd<T>() where T : Component
        {
            var component = Get<T>();
            if (component != null) return component;
            component = gameObject.AddComponent<T>();
            if (component == null)
            {
                Debugger.LogError(nameof(UIBinding), $"UIBinding AddComponent failed: {typeof(T).Name}");
                return null;
            }
            components ??= new Dictionary<Type, Component>();
            components[typeof(T)] = component;
            return component;
        }
    }
}