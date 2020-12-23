using System;
using System.Windows.Forms;

namespace CCS
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

#if !DEBUG
            if (CCS.Catcher.Utils.Licence.Check()) //许可证检查
            {
                Application.Run(new Form1());
            }
            else
            {
                Application.Exit();
            }
#else
            Application.Run(new Form1());
#endif
        }
    }
}