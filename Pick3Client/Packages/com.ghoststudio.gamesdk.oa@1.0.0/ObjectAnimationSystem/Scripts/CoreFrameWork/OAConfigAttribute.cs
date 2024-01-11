using System;

namespace AppBase.OA
{
    [AttributeUsage(AttributeTargets.Class)]
    public class OAConfigAttribute : Attribute
    {
        public string displayName = "";
        public int sort = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">展示名</param>
        /// <param name="sort">排序，越小越靠前</param>
        public OAConfigAttribute(string name,int sort)
        {
            this.displayName = name;
            this.sort = sort;
        }
    }
}