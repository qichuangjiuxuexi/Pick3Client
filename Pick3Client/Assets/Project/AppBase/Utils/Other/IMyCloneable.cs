using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


namespace WordGame.Utils
{
    /// <summary>
    /// 深度克隆接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IMyCloneable<T> : ICloneable where T : class
    {
        /// <summary>
        /// 克隆
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        /// <summary>
        /// 深度克隆
        /// </summary>
        /// <returns></returns>
        public T DeepClone()
        {
            using (Stream objectStream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(objectStream, this);
                objectStream.Seek(0, SeekOrigin.Begin);
                return formatter.Deserialize(objectStream) as T;
            }
        }

        /// <summary>
        /// 浅度克隆
        /// </summary>
        /// <returns></returns>
        public T ShallowClone()
        {
            return Clone() as T;
        }
    }
}