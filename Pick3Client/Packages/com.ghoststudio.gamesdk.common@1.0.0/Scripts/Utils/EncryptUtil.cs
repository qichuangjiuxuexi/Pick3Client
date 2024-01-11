using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace AppBase.Utils
{
    /// <summary>
    /// 加密工具，带数据校验功能
    /// </summary>
    public static class EncryptUtil
    {
        private static readonly byte[] headArray = { (byte)'G', (byte)'S', (byte)'D', 0x00 };
        private static MD5CryptoServiceProvider _md5;
        private static MD5CryptoServiceProvider md5 => _md5 ??= new MD5CryptoServiceProvider();
        private static SymmetricAlgorithm _aes;

        private static SymmetricAlgorithm aes => _aes ??= new AesManaged
        {
            Padding = PaddingMode.PKCS7,
            Mode = CipherMode.CBC,
            KeySize = 128,
            BlockSize = 128,
            Key = md5.ComputeHash(new[]{(byte)'G',(byte)'h',(byte)'o',(byte)'s',(byte)'t',(byte)'S',(byte)'t',(byte)'u',(byte)'d',(byte)'i',(byte)'o',(byte)2,(byte)0,(byte)2,(byte)3})
        };

        /// <summary>
        /// 加密字节
        /// </summary>
        public static byte[] EncryptBytes(byte[] bytes)
        {
            if (bytes == null) return null;
            //使用md5哈希做IV，可兼顾校验功能
            var md5Bytes = md5.ComputeHash(bytes);
            aes.IV = md5Bytes;
            byte[] encryptedBytes;
            using (var encryptor = aes.CreateEncryptor())
                encryptedBytes = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
            return headArray.Concat(md5Bytes).Concat(encryptedBytes).ToArray();
        }

        /// <summary>
        /// 解密字节
        /// </summary>
        public static byte[] DecryptBytes(byte[] bytes)
        {
            if (!CheckIsEncrypted(bytes)) return null;
            var iv = bytes.Skip(4).Take(16).ToArray();
            aes.IV = iv;
            byte[] decryptedBytes;
            using (var decryptor = aes.CreateDecryptor())
                decryptedBytes = decryptor.TransformFinalBlock(bytes, 20, bytes.Length - 20);
            //校验md5
            var md5Bytes = md5.ComputeHash(decryptedBytes);
            return md5Bytes.SequenceEqual(iv) ? decryptedBytes : null;
        }

        /// <summary>
        /// 检查加密头部
        /// </summary>
        public static bool CheckIsEncrypted(byte[] bytes)
        {
            return bytes != null &&
                   bytes.Length > 20 &&
                   bytes[0] == headArray[0] &&
                   bytes[1] == headArray[1] &&
                   bytes[2] == headArray[2] &&
                   bytes[3] == headArray[3];
        }

        /// <summary>
        /// 加密字符串
        /// </summary>
        public static byte[] EncryptString(string text)
        {
            if (text == null) return null;
            return EncryptBytes(Encoding.UTF8.GetBytes(text));
        }

        /// <summary>
        /// 解密字符串
        /// </summary>
        public static string DecryptString(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return null;
            var result = DecryptBytes(bytes);
            return result != null ? Encoding.UTF8.GetString(result) : null;
        }
    }
}
