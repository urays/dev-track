using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace IDevTrack.Update
{
    public static class Extra
    {
        /// <summary>
        /// 获取文件的MD5
        /// </summary>
        /// <param name="fullpath">文件全路径地址</param>
        /// <returns>16位MD5</returns>
        public static string MD5Encrypt(string fullpath)
        {
            string data = "";
            string md5c = "";
            if (File.Exists(fullpath))
            {
                using (StreamReader f = new StreamReader(fullpath, Encoding.UTF8))
                {
                    data = f.ReadToEnd();
                    f.Close();
                }
                //不考虑CRLF和LF的区别
                data = Regex.Replace(data, "[\r\n]", string.Empty, RegexOptions.Compiled);
                var buffer = Encoding.UTF8.GetBytes(data); //将输入字符串转换成字节数组
#pragma warning disable CA5351 // 不要使用损坏的加密算法
                using (var MD5 = System.Security.Cryptography.MD5.Create())
#pragma warning restore CA5351 // 不要使用损坏的加密算法
                {
                    var Hash = MD5.ComputeHash(buffer);
                    foreach (var t in Hash)
                    {
                        md5c += t.ToString("x2");
                    }
                }
            }
            return md5c;
        }

        /// <summary>
        /// 覆盖方式写出文件(CRLF,UTF8_no_BOM)
        /// </summary>
        /// <param name="path">输出路径</param>
        /// <param name="cog">写入内容</param>
        public static void WriteOut(string path, string cog)
        {
            if (cog == null) { return; }

            cog = cog.Replace("\r\n", "\n").Replace("\n", "\r\n");
            try
            {
                using (var sink = new StreamWriter(path, false, new System.Text.UTF8Encoding(false)))
                {
                    sink.Write(cog);
                    sink.Close();
                }
            }
            catch { }
        }

        /// <summary>
        /// 获取文件名后缀
        /// </summary>
        /// <param name="fullpath">全路径地址</param>
        /// <returns>后缀字符串</returns>
        public static string GetFileSuffix(string fullpath)
        {
            if (fullpath == null) return "";
            string[] arr = fullpath.Split('.');
            return arr[arr.Length - 1].ToLower(System.Globalization.CultureInfo.GetCultureInfo("en-US"));
        }
    }
}