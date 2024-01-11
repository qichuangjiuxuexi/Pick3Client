using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace WordGame.Utils
{
    public class ToolJson
    {
        /// <summary>
        /// 序列化参数
        /// </summary>
        private static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings()
            {TypeNameHandling = TypeNameHandling.Auto};
        //混合类特殊的序列化参数
        private static JsonSerializerSettings unityJsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Error = (se, ev) => { ev.ErrorContext.Handled = true; },
            Converters = new List<JsonConverter> { new UnityObjectConverter() }
        };

        /// <summary>
        /// 序列化普通类
        /// </summary>
        /// <param name="o">需要序列化的普通类</param>
        /// <returns>序列化的字符串</returns>
        public static string SerializeObject(object o)
        {
            try
            {
                return JsonConvert.SerializeObject(o, jsonSerializerSettings);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return null;
            }
        }
        
        /// <summary>
        /// 序列化包含UnityObject的混合类，作为调试时使用
        /// </summary>
        /// <param name="o">需要序列化的混合类</param>
        /// <returns>序列化的字符串</returns>
        public static string SerializeUnityObject(object o)
        {
            try
            {
                return JsonConvert.SerializeObject(o, unityJsonSerializerSettings);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return null;
            }
        }

        /// <summary>
        /// 反序列化普通类
        /// </summary>
        /// <typeparam name="T">反序列化的类型</typeparam>
        /// <param name="data">需要序列化的数据</param>
        /// <returns>返回的类</returns>
        public static T DeserializeObject<T>(string data)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(data, jsonSerializerSettings);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return default;
            }
        }
        
        /// <summary>
        /// 反序列化普通类
        /// </summary>
        /// <typeparam name="T">反序列化的类型</typeparam>
        /// <param name="data">需要序列化的数据</param>
        /// <returns>返回的类</returns>
        public static object DeserializeObject(string data,Type type)
        {
            return JsonConvert.DeserializeObject(data, type,jsonSerializerSettings);
        }

        /// <summary>
        /// 将json串排版后输出
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Convert2PrettyPrint(string str)
        {
            //格式化json字符串
            JsonSerializer serializer = new JsonSerializer();
            TextReader tr = new StringReader(str);
            JsonTextReader jtr = new JsonTextReader(tr);
            object obj = serializer.Deserialize(jtr);
            if (obj != null)
            {
                StringWriter textWriter = new StringWriter();
                JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
                {
                    Formatting = Formatting.Indented,
                    Indentation = 4,
                    IndentChar = ' '
                };
                serializer.Serialize(jsonWriter, obj);
                return textWriter.ToString();
            }
            else
            {
                return str;
            }
        }
        
        /// <summary>
        /// UnityObject Json解析器
        /// </summary>
        private class UnityObjectConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return typeof(UnityEngine.Object).IsAssignableFrom(objectType);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null)
                    return null;
                var jsonObject = JObject.Load(reader);
                string json = jsonObject.ToString();
                return JsonUtility.FromJson(json, objectType);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                if (value is UnityEngine.Object unityObject)
                {
                    string json = JsonUtility.ToJson(unityObject);
                    writer.WriteRawValue(json);
                }
                else
                {
                    string json = JsonConvert.SerializeObject(value, jsonSerializerSettings);
                    writer.WriteRawValue(json);
                }
            }
        }
    }
}