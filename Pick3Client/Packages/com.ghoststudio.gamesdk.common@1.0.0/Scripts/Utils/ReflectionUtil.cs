using System;
using System.Reflection;

namespace AppBase.Utils
{
    /// <summary>
    /// 反射工具
    /// </summary>
    public static class ReflectionUtil
    {
        /// <summary>
        /// 热更程序集名字
        /// </summary>
        public const string HotfixAsm = "HotfixAsm";
        
        /// <summary>
        /// 获取类型
        /// </summary>
        public static Type GetType(string assembly, string typeName)
        {
            return Assembly.Load(assembly)?.GetType(typeName);
        }
        
        /// <summary>
        /// 获取静态方法
        /// </summary>
        public static MethodInfo GetStaticMethod(string assembly, string typeName, string methodName)
        {
            var type = GetType(assembly, typeName);
            return type?.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        }
        
        /// <summary>
        /// 调用静态方法
        /// </summary>
        public static object InvokeStaticMethod(string assembly, string typeName, string methodName, params object[] args)
        {
            var method = GetStaticMethod(assembly, typeName, methodName);
            return method?.Invoke(null, args);
        }
        
        /// <summary>
        /// 获取实例方法
        /// </summary>
        public static MethodInfo GetMethod(string assembly, string typeName, string methodName)
        {
            var type = GetType(assembly, typeName);
            return type?.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }
        
        /// <summary>
        /// 获取静态属性值
        /// </summary>
        public static T GetStaticProperty<T>(string assembly, string typeName, string propertyName)
        {
            var type = GetType(assembly, typeName);
            var property = type?.GetProperty(propertyName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            return (T) property?.GetValue(null);
        }
        
        /// <summary>
        /// 设置静态属性值
        /// </summary>
        public static bool SetStaticProperty(string assembly, string typeName, string propertyName, object value)
        {
            var type = GetType(assembly, typeName);
            var property = type?.GetProperty(propertyName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (property == null) return false;
            property.SetValue(null, value);
            return true;
        }
        
        /// <summary>
        /// 获取静态字段值
        /// </summary>
        public static T GetStaticField<T>(string assembly, string typeName, string fieldName)
        {
            var type = GetType(assembly, typeName);
            var field = type?.GetField(fieldName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            return (T) field?.GetValue(null);
        }
        
        /// <summary>
        /// 设置静态字段值
        /// </summary>
        public static bool SetStaticField(string assembly, string typeName, string fieldName, object value)
        {
            var type = GetType(assembly, typeName);
            var field = type?.GetField(fieldName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null) return false;
            field.SetValue(null, value);
            return true;
        }
        
        /// <summary>
        /// 调用实例方法
        /// </summary>
        public static object InvokeMethod(object obj, string methodName, params object[] args)
        {
            var type = obj?.GetType();
            var method = type?.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return method?.Invoke(obj, args);
        }
        
        /// <summary>
        /// 获取实例属性值
        /// </summary>
        public static T GetProperty<T>(object obj, string propertyName)
        {
            var type = obj?.GetType();
            var property = type?.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return (T) property?.GetValue(obj);
        }
        
        /// <summary>
        /// 设置实例属性值
        /// </summary>
        public static bool SetProperty(object obj, string propertyName, object value)
        {
            var type = obj?.GetType();
            var property = type?.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property == null) return false;
            property.SetValue(obj, value);
            return true;
        }
        
        /// <summary>
        /// 获取实例字段值
        /// </summary>
        public static T GetField<T>(object obj, string fieldName)
        {
            var type = obj?.GetType();
            var field = type?.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return (T) field?.GetValue(obj);
        }
        
        /// <summary>
        /// 设置实例字段值
        /// </summary>
        public static bool SetField(object obj, string fieldName, object value)
        {
            var type = obj?.GetType();
            var field = type?.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null) return false;
            field.SetValue(obj, value);
            return true;
        }
        
        /// <summary>
        /// 获取二级实例属性值
        /// </summary>
        public static T GetStaticProperty<T>(string assembly, string typeName, string subProperty, string propertyName)
        {
            var obj = GetStaticProperty<object>(assembly, typeName, subProperty);
            return GetProperty<T>(obj, propertyName);
        }
        
        /// <summary>
        /// 设置二级实例属性值
        /// </summary>
        public static bool SetStaticProperty(string assembly, string typeName, string subProperty, string propertyName, object value)
        {
            var obj = GetStaticProperty<object>(assembly, typeName, subProperty);
            return SetProperty(obj, propertyName, value);
        }
    }
}