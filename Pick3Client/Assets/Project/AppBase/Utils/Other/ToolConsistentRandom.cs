using System;
using System.Collections.Generic;

namespace WordGame.Utils
{
    /// <summary>
    /// 给定种子, 按固定算法生成, 固定的假随机数列
    /// </summary>
    public class ToolConsistentRandom
    {
        private static int seed;

        public static int Seed
        {
            get => seed;
            set => seed = value;
        }

        
        /// <summary>
        /// 得到下一个随机数[0~1]
        /// 1. 正弦值小数点后5~8位的值.
        /// </summary>
        /// <returns></returns>
        public static double Random()
        {
            double temp = Math.Sin(seed);
            seed++;
            temp *= 10000;
            temp -= Math.Floor(temp);
            return temp;
        }

        
        /// <summary>
        /// 根据随机种子, Shuffle一个数组
        /// </summary>
        /// <param name="items"></param>
        /// <typeparam name="T"></typeparam>
        public static void ArrayShuffle<T>(List<T> items)
        {
            //有序的个数
            int itemWithOrderCount = items.Count;
            while (itemWithOrderCount>0)
            {
                int randomIndex = (int)Math.Floor(Random() * itemWithOrderCount);
                itemWithOrderCount--;

                var temp = items[itemWithOrderCount];
                items[itemWithOrderCount] = items[randomIndex];
                items[randomIndex] = temp;
            }
        }
        
    }
}