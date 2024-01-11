/**********************************************

Copyright(c) 2020 by com.me2zen
All right reserved

Author : Terrence Rao 
Date : 2020-07-18 19:30:13
Ver : 1.0.0
Description : 
ChangeLog :
**********************************************/

namespace WordGame.Utils
{
    /// <summary>
    /// 单例类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Singleton<T> where T : new()
    {
        /// <summary>
        /// 实例
        /// </summary>
        private static T instance;

        /// <summary>
        /// 获取单例
        /// </summary>
        public static T Instance
        {
            get
            {
                if (instance == null) instance = new T();
                return instance;
            }
        }
    }
}