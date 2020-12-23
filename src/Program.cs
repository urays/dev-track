using System;
using System.Windows.Forms;

namespace DevTrack
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        private static void Main()
        {
            IDevTrack.Update.UTools.MoveUpdateExe(); //更新Update.exe(如果有更新的话)

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
#if DEBUG
            using (Form ST = new IDevTrack.StartUp.mainForm())
            {
                Application.Run(ST);
            }
#else
            if (IDevTrack.Utils.Licence.Check()) //许可证检查
            {
                using (Form ST = new IDevTrack.StartUp.mainForm())
                {
                    Application.Run(ST);
                }
            }
            else
            {
                Application.Exit();
            }
#endif
        }
    }
}