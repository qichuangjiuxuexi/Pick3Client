using System;
using Newtonsoft.Json;
using UnityEngine;

namespace AppBase.Utils
{
    /// <summary>
    /// Json工具
    /// </summary>
    public static class JsonUtil
    {
        /// <summary>
        /// 序列化参数
        /// </summary>
        private static JsonSerializerSettings settings = new()
        {
            TypeNameHandling = TypeNameHandling.Auto
        };
        
        /// <summary>
        /// 省流的配置
        /// </summary>
        private static JsonSerializerSettings settingsSaving = new()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
        };

        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="o">需要序列化的普通类</param>
        /// <returns>序列化的字符串</returns>
        public static string SerializeObject(object o)
        {
            try
            {
                return JsonConvert.SerializeObject(o, settings);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return null;
            }
        }

        /// <summary>
        /// 序列化存档，省流版，并记录根类型，方便反序列化时推断出正确的子类型
        /// </summary>
        /// <param name="o">需要序列化的对象</param>
        /// <param name="baseType">基类型，反序列化时使用这个基类型也可以获得正确的子类型</param>
        /// <returns>序列化的字符串</returns>
        public static string SerializeArchive(object o, Type baseType)
        {
            try
            {
                return JsonConvert.SerializeObject(o, baseType, settingsSaving);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return null;
            }
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T">反序列化的类型</typeparam>
        /// <param name="data">需要序列化的数据</param>
        /// <returns>返回的类</returns>
        public static T DeserializeObject<T>(string data)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(data, settings);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return default;
            }
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T">反序列化的类型</typeparam>
        /// <param name="data">需要序列化的数据</param>
        /// <returns>返回的类</returns>
        public static object DeserializeObject(string data, Type type)
        {
            try
            {
                return JsonConvert.DeserializeObject(data, type, settings);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return default;
            }
        }
    }
}