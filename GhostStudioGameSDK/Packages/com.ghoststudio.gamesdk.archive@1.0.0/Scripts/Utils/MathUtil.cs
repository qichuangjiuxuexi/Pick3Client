namespace AppBase.Utils
{
    internal static class MathUtil
    {
        /// <summary>
        /// 安全增加资产数量，防止溢出 [0, long.MaxValue]
        /// </summary>
        internal static long SafeIncrement(this long a, long b)
        {
            if (a > 0 && b > long.MaxValue - a)
            {
                // a和b的总和超过long.MaxValue
                return long.MaxValue;
            }

            if (a < 0 && b < -a)
            {
                // a和b的总和小于0
                return 0;
            }

            return a + b;
        }
    }
}