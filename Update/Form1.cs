using System;
using System.IO;
using System.Windows.Forms;

namespace Update
{
    public partial class Form1 : Form
    {
        //[DllImport("user32.dll")]
        //public static extern bool ReleaseCapture();

        //[DllImport("user32.dll")]
        //public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

        //private void PicBox_MouseDown(object sender, MouseEventArgs e)
        //{
        //    ReleaseCapture();
        //    SendMessage(Handle, 0x0112, 0xF010 + 0x0002, 0);
        //}

        private static string ToVersion = "v---"; //将要更新到的版本号

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if ((pictureBox1.Image != null))
            {
                pictureBox1.Image.Dispose();
                pictureBox1.Image = null;
            }
            pictureBox1.Image = IUpdate.Properties.Resources.sun;

            try
            {
                //检查更新
                IDevTrack.Update.UTools.ExistUpdate(new IDevTrack.Update.UTools.delegateCallBack(CallBack_ExistUpdate));
            }
            catch (Exception) { Application.Exit(); }
        }

        private void CallBack_ExistUpdate(IAsyncResult iar)
        {
            string[] Lst = IDevTrack.Update.UTools.UpdateList(); //待更新清单
            ToVersion = Lst[0];
            if (Lst.Length > 1)
            {
                CloseMainApplication(); //关闭 DevTrack.exe
                try
                {
                    IDevTrack.Update.UTools.UpdateFiles(new IDevTrack.Update.UTools.delegateCallBack(CallBack_UpdateFiles));
                }
                catch (Exception) { Application.Exit(); }
            }
            else  //没有更新,则关闭
            {
                Application.Exit();
            }
        }

        private void CallBack_UpdateFiles(IAsyncResult iar)
        {
            //#if !DEBUG
            IUpdate.FeedBack.Notify(Environment.UserName, "v" + ToVersion);
            //#endif

            if (File.Exists("DevTrack.exe"))
            {
                System.Diagnostics.Process.Start("DevTrack.exe");
            }
            Close();
            Application.Exit();
        }

        private static void CloseMainApplication()
        {
            try
            {
                System.Diagnostics.Process[] process = System.Diagnostics.Process.GetProcessesByName("DevTrack");
                foreach (System.Diagnostics.Process pro in process)
                {
                    if (!pro.CloseMainWindow())
                    {
                        pro.Kill();
                    }
                }
            }
            catch (Exception)
            {
                Application.Exit();
            }
        }
    }
}