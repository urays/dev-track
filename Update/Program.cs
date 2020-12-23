using System;
using System.Windows.Forms;

namespace Update
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
#if DEBUG
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
#else
            if (args.Length > 0)
            {
                if (args[0].Trim() == "u")
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new Form1());
                }
                else if (args[0].Trim() == "xml")
                {
                    IDevTrack.Update.FilesMap.WriteToLocal();
                }
            }
#endif
        }
    }
}