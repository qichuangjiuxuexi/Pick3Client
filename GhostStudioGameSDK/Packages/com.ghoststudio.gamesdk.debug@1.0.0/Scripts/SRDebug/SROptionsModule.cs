using System;
using System.Collections.Generic;
using System.Reflection;
using AppBase.Module;
using SRDebugger;
using SRF.Helpers;
using UnityEngine;
using UnityEngine.Scripting;

namespace AppBase.Debugging
{
    /// <summary>
    /// SROption基类，各业务模块的SROption继承此类
    /// </summary>
    [Preserve]
    public abstract class SROptionsModule : ModuleBase, IOptionContainer
    {
        /// <summary>
        /// 分组名称
        /// </summary>
        protected abstract string CatalogName { get; }

        /// <summary>
        /// 分组优先级，越小越靠前
        /// </summary>
        protected abstract SROptionsPriority CatalogPriority { get; }

        protected sealed override void OnInternalInit()
        {
            base.OnInternalInit();
            GameBase.Instance.GetModule<DebugManager>().SRDebug.RegisterSROptions(this);
        }

        protected sealed override void OnInternalDestroy()
        {
            base.OnInternalDestroy();
            GameBase.Instance.GetModule<DebugManager>().SRDebug.UnregisterSROptions(this);
        }
        
        private List<OptionDefinition> options;
        public IEnumerable<OptionDefinition> GetOptions()
        {
            if (options != null) return options;
            options = ScanForOptions();
            return options;
        }
        
        //扫描属性
        private List<OptionDefinition> ScanForOptions()
        {
            var options = new List<OptionDefinition>();
            var members = GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.InvokeMethod);
            var ignoreAssembly = typeof(MonoBehaviour).Assembly;
            foreach (var memberInfo in members)
            {
                // Skip any properties that are from built-in Unity types (e.g. Behaviour, MonoBehaviour)
                if (memberInfo.DeclaringType != null && memberInfo.DeclaringType.Assembly == ignoreAssembly) continue;

                // Find user-specified display name from attribute
                var nameAttribute = SRReflection.GetAttribute<DisplayNameAttribute>(memberInfo);
                //没有指定名字的一律不显示
                if (nameAttribute == null) continue;
                var name = nameAttribute.DisplayName;

                // Find user-specified category name from attribute
                var categoryAttribute = SRReflection.GetAttribute<CategoryAttribute>(memberInfo);
                var category = categoryAttribute == null ? CatalogName : categoryAttribute.Category;

                // Find user-specified sorting priority from attribute
                var sortPriority = nameAttribute.SortPriority;

                if (memberInfo is PropertyInfo propertyInfo)
                {
                    if (propertyInfo.GetGetMethod() == null) continue;
                    // Ignore static members
                    if ((propertyInfo.GetGetMethod().Attributes & MethodAttributes.Static) != 0) continue;
                    
                    //转义属性类型
                    var attributes = new List<Attribute>();
                    var numberRangeAttribute = SRReflection.GetAttribute<NumberRangeAttribute>(memberInfo);
                    if (numberRangeAttribute != null) attributes.Add(new SRDebugger.NumberRangeAttribute(numberRangeAttribute.Min, numberRangeAttribute.Max));
                    var incrementAttribute = SRReflection.GetAttribute<IncrementAttribute>(memberInfo);
                    if (incrementAttribute != null) attributes.Add(new SRDebugger.IncrementAttribute(incrementAttribute.Increment));

                    var option = new OptionDefinition(name, category, sortPriority, new PropertyReference(propertyInfo.PropertyType,
                        propertyInfo.GetGetMethod() != null ? () => SRReflection.GetPropertyValue(this, propertyInfo) : null,
                        propertyInfo.GetSetMethod() != null ? v => SRReflection.SetPropertyValue(this, propertyInfo, v) : null,
                        attributes.ToArray()
                    ));
                    option.CatalogPriority = (int)CatalogPriority;
                    options.Add(option);
                }
                else if (memberInfo is MethodInfo methodInfo)
                {
                    if (methodInfo.IsStatic) continue;
                    // Skip methods with parameters or non-void return type
                    if (methodInfo.ReturnType != typeof(void) || methodInfo.GetParameters().Length > 0) continue;

                    var option = new OptionDefinition(name, category, sortPriority, new MethodReference(this, methodInfo));
                    option.CatalogPriority = (int)CatalogPriority;
                    options.Add(option);
                }
            }
            return options;
        }

        public bool IsDynamic => false;
        public event Action<OptionDefinition> OptionAdded;
        public event Action<OptionDefinition> OptionRemoved;
    }
}