using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CCS.Catcher.Utils
{
    /// <summary>
    /// 本地基础类支持许可证检查
    /// </summary>
    internal static class Licence
    {
        #region Constants

#if DEBUG
        private const string PATHKEY = "../src/Libs/rcm.dll";
#else
        private const string PATHKEY = "./rcm.dll";
#endif

        private const string LICENSE = "license.lic";
        private const string PUBLICKEY = "idkey.pub";

        #endregion Constants

        #region Methods

        /// <summary>
        /// 私钥核对检查
        /// </summary>
        /// <param name="local">许可证路径</param>
        /// <returns>有效期限</returns>
        ///  0 - 验证失败,密钥无效
        /// -1 - 验证成功,永久密钥
        /// >0 - 验证成功,期限密钥(秒)
        [DllImport(PATHKEY, EntryPoint = "askey_check", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int askey_check(string local);

        /// <summary>
        /// 根据公钥创建私钥
        /// </summary>
        /// <param name="due">创建密钥的有效期(天)</param>
        ///      = 0 - 将创建永久密钥
        ///      > 0 - 将创建期限密钥(3 - 3天使用期...)
        [DllImport(PATHKEY, EntryPoint = "askey_private", CallingConvention = CallingConvention.Cdecl)]
        private static extern void askey_private(short due);

        /// <summary>
        /// 创建公钥
        /// </summary>
        [DllImport(PATHKEY, EntryPoint = "askey_public", CallingConvention = CallingConvention.Cdecl)]
        private static extern void askey_public();

        /// <summary>
        /// 许可证检查
        /// </summary>
        /// <returns>许可证是否有效</returns>
        public static bool Check()
        {
            try
            {
                int rmsec = askey_check("./" + LICENSE); //许可剩余时间  -1:永久  0:无效许可
                if (rmsec == 0 || rmsec < -1)
                {
                    Licence.askey_public(); //创建公钥
                    if (File.Exists("./" + LICENSE)) { File.Delete("./" + LICENSE); } //删除无效许可
                    MessageBox.Show("许可证<" + LICENSE + ">丢失或已失效!\n请将<" + PUBLICKEY + "> 发送至管理员获取<" + LICENSE + ">",
                        "许可证检查 <" + LICENSE + ">", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    return false;
                }
                else
                {
                    if (File.Exists("./" + PUBLICKEY)) { File.Delete("./" + PUBLICKEY); }
                }
            }
            catch (Exception e) //抛出异常提示
            {
                MessageBox.Show(string.Format("{0}", e.GetBaseException().Message), "Exception Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 获取有效天数标记信息条
        /// </summary>
        /// <returns>字符串信息条</returns>
        public static string GetValidDayMark()
        {
            string res = "";
#if DEBUG
            res = "DEBUG";
#else
            int rmsec = askey_check("./" + LICENSE); //许可剩余时间  -1:永久  0:无效许可
            if (rmsec > 0)
            {
                TimeSpan ts = new TimeSpan(0, 0, rmsec);
                res = (ts.Days + 1).ToString() + "天";
            }
            else if (rmsec == -1)
            {
                res = "永久";
            }
#endif
            return res;
        }

        #endregion Methods
    }
}