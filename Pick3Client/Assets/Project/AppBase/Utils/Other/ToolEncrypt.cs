using System;
using System.Text;

namespace WordGame.Utils
{
    /// <summary>
    /// 加密解密工具
    /// </summary>
    public static class ToolEncrypt
    {
        //加密头
        private static char[] charHeadArray = new char[] {'E', 'N', 'C', 'O', 'D', 'E', '1', '6'};

        private static string EncodeKey = "owkqidlwizyeneos";

        /// <summary>文件解密</summary>
        /// <param name="input"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string CharXorDecrypt(string input, char[] key)
        {
            char[] inputArray = input.ToCharArray();

            bool isXorEncrypt = true;
            if (inputArray.Length < charHeadArray.Length)
            {
                isXorEncrypt = false;
            }
            else
            {
                for (int i = 0; i < charHeadArray.Length; i++)
                {
                    if (inputArray[i] != charHeadArray[i])
                    {
                        isXorEncrypt = false;
                        break;
                    }
                }
            }

            if (isXorEncrypt)
            {
                char[] encryptArray = new char[inputArray.Length - charHeadArray.Length];
                char[] decryptArray = new char[encryptArray.Length];
                Buffer.BlockCopy(inputArray, charHeadArray.Length * sizeof(char), encryptArray, 0,
                    encryptArray.Length * sizeof(char));

                for (int i = 0; i < encryptArray.Length; i++)
                {
                    decryptArray[i] = (char) (encryptArray[i] ^ key[i & 15]);
                }

                return new string(decryptArray);
            }
            else
            {
                return input;
            }
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="rawStrContent"></param>
        /// <returns></returns>
        public static byte[] Encrypt(string rawStrContent)
        {
            if (string.IsNullOrEmpty(rawStrContent)) //长度不够处理
            {
                return new byte[0];
            }


            byte[] bytes = Encoding.UTF8.GetBytes(rawStrContent); //信息改为utf8 bytes

            uint[] intForData = ToolString.Byte2Uint(bytes, true); //数据 转成uint数组
            int dataLen = intForData.Length - 1; //最后1位存储 数据长度
            if (dataLen < 1) //长度为0 返回空数组
            {
                return new byte[0];
            }

            uint[] intForKey = ToolString.Byte2Uint(Encoding.UTF8.GetBytes(EncodeKey), false); //密钥转为uint数组

            for (int i = 0, iMax = intForData.Length; i < iMax; i++)
            {
                intForData[i] = (intForData[i] + intForKey[i % 4]) % uint.MaxValue;
            }

            return ToolString.Uint2Byte(intForData, false);
        }

        /// <summary>
        /// 加密, 返回字符串
        /// </summary>
        /// <param name="rawStrContent"></param>
        /// <returns></returns>
        public static string EncryptWithStringResult(string rawStrContent)
        {
            byte[] bytesResult = Encrypt(rawStrContent);
            return Convert.ToBase64String(bytesResult);
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string Decrypt(byte[] bytes)
        {
            string info = string.Empty;
            if (bytes == null || bytes.Length < 1) //长度不够处理
            {
                return info;
            }


            uint[] intForData = ToolString.Byte2Uint(bytes, false);
            uint[] intForKey = ToolString.Byte2Uint(Encoding.UTF8.GetBytes(EncodeKey), false);

            for (int i = 0, iMax = intForData.Length; i < iMax; i++)
            {
                uint middleValue = intForKey[i % 4];
                if (intForData[i] < middleValue)
                {
                    intForData[i] += uint.MaxValue - middleValue;
                }
                else
                {
                    intForData[i] -= middleValue;
                }
            }

            return ToolString.GetStringFromUTF8Bytes(ToolString.Uint2Byte(intForData, true));
        }

        /// <summary>
        /// 解密, 以字符串为参数
        /// </summary>
        /// <param name="rawStrContent"></param>
        /// <returns></returns>
        public static string DecryptWithStringContent(string encrypStrContent)
        {
            byte[] bytesResult = Convert.FromBase64String(encrypStrContent);
            return Decrypt(bytesResult);
        }


        /// <summary>
        /// 混合运算
        /// </summary>
        /// <param name="sum"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="p"></param>
        /// <param name="e"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        private static uint DoRun(uint sum, uint y, uint z, int p, uint e, uint[] k)
        {
            uint num0 = (z >> 5 ^ y << 2);
            uint num1 = (y >> 3 ^ z << 4) ^ (sum ^ y);
            long num2 = unchecked(p & 3 ^ (long) ((ulong) e));
            int num3 = (int) (checked(num2));
            //Debug.LogError("num3  " + num3.ToString());
            uint num4 = k[num3];

            return num0 + num1 + (num4 ^ z);
        }
    }
}