using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace IDevTrack.Update
{
    public static class UTools
    {
        private delegate void delegateExistUpdate();

        private delegate void delegateUpdateFiles();

        public delegate void delegateCallBack(IAsyncResult iar);

        private static string Remote = "";                  //资源更新源
        private static readonly List<string> WaitFiles = new List<string>();
        private static string ForceDel = "";              //强制删除的内容
        private static string rVersion = "";              //更新源程序版本号

        public static string[] UpdateList()
        {
            string[] tmp = new string[WaitFiles.Count + 1];
            tmp[0] = rVersion;
            WaitFiles.CopyTo(tmp, 1);
            return tmp;
        }

        //<示例>
        //IDevTrack.Update.UTools.UpdateFiles(new IDevTrack.Update.UTools.delegateCallBack(CallBack_UpdateFiles));
        //private void CallBack_UpdateFiles(IAsyncResult iar)
        //{
        //    MessageBox.Show("更新成功");
        //    System.Diagnostics.Process.Start("DevTrack.exe");
        //    Close();
        //    Application.Exit();
        //}

        public static void ExistUpdate(delegateCallBack callback)
        {
            delegateExistUpdate mc = new delegateExistUpdate(existupdate);
            mc.BeginInvoke(new AsyncCallback(callback), null);
        }

        public static void UpdateFiles(delegateCallBack callback)
        {
            delegateUpdateFiles mc = new delegateUpdateFiles(updatefiles);
            mc.BeginInvoke(new AsyncCallback(callback), null);
        }

        private static void existupdate()  //检查更新
        {
            Remote = rVersion = ForceDel = "";
            WaitFiles.Clear();

            string rcog = IDevTrack.Update.FilesMap.GetRemoteList(IDevTrack.Update.Settings.RemoteXml);
            if (rcog is "") { return; }

            //版本更新检查
            rVersion = IDevTrack.Update.FilesMap.GetNode(rcog, "VERSION");
            if (rVersion.CompareTo(Application.ProductVersion) < 0) { return; }

            Dictionary<string, string> lfmap = IDevTrack.Update.FilesMap.GetLocalFiles();

            Remote = IDevTrack.Update.FilesMap.GetNode(rcog, "REMOTE");
            if (Remote is "") { Remote = IDevTrack.Update.Settings.RemoteRes; }
            ForceDel = IDevTrack.Update.FilesMap.GetNode(rcog, "FORCEDEL");
            string toupdate = IDevTrack.Update.FilesMap.GetNode(rcog, "TOUPDATE").Replace("/", "\\");
            string[] rfiles = toupdate.Replace("\r\n", "\n").Split('\n');

            foreach (string rfile in rfiles)
            {
                string rf = Regex.Match(rfile, @"\[(.*)\]", RegexOptions.Singleline).Groups[1].Value.Trim();
                string rmd5 = Regex.Match(rfile, @"\((.*)\)", RegexOptions.Singleline).Groups[1].Value.Trim();
                if (rf is "") { continue; }

                if (!lfmap.ContainsKey(rf))
                {
                    if (Array.IndexOf(IDevTrack.Update.Settings.nFiles, rf) == -1)
                    {
                        WaitFiles.Add(rf);
                    }
                }
                else if (lfmap[rf] != rmd5) { WaitFiles.Add(rf); }
            }
        }

        private static void updatefiles()
        {
            string curdir = System.AppDomain.CurrentDomain.BaseDirectory; //本地根目录

            //强制清除文件/文件夹
            if (!(ForceDel is ""))
            {
                string[] items = ForceDel.Replace("\r\n", "\n").Split('\n');
                foreach (string item in items)
                {
                    string it = Regex.Match(item.Trim(), @"\[(.*)\]", RegexOptions.Singleline).Groups[1].Value;
                    string tp = Regex.Match(item, @"\((.*)\)", RegexOptions.Singleline).Groups[1].Value;
                    if (it is "") { continue; }
                    if (tp == "d")
                    {
                        if (Directory.Exists(it)) { Directory.Delete(curdir + it, true); }
                    }
                    else if (tp == "f")
                    {
                        if (File.Exists(it)) { File.Delete(curdir + it); }
                    }
                }
            }
            //从更新源下载到本地
            for (int i = 0; i < WaitFiles.Count; i++)
            {
                string url = Remote + WaitFiles[i];
                string filepath = curdir + (WaitFiles[i].Contains(IDevTrack.Update.Settings.UpdateExe)
                    ? IDevTrack.Update.Settings.Temp : "") + WaitFiles[i];

                if (File.Exists(filepath)) { File.Delete(filepath); }  //首先删除需要更新的文件
                else
                {
                    string filedir = Path.GetDirectoryName(filepath); //可能不存在目录
                    if (!Directory.Exists(filedir)) { Directory.CreateDirectory(filedir); }
                }

                using (WebClient client = new WebClient())
                {
                    try
                    {
                        string sfx = IDevTrack.Update.Extra.GetFileSuffix(filepath);
                        if ((Array.IndexOf(IDevTrack.Update.Settings.CRLF, sfx) != -1))
                        {
                            string data = Encoding.UTF8.GetString(client.DownloadData(url));
                            IDevTrack.Update.Extra.WriteOut(filepath, data);  //覆盖写入文件
                        }
                        else { client.DownloadFile(url, filepath); }
                    }
                    catch { };
                }
            }
            Remote = "";
            WaitFiles.Clear();
            ForceDel = "";
        }

        public static void MoveUpdateExe()
        {
            string curdir = System.AppDomain.CurrentDomain.BaseDirectory;
            string exesrc = curdir + IDevTrack.Update.Settings.Temp + IDevTrack.Update.Settings.UpdateExe;
            string exedst = curdir + IDevTrack.Update.Settings.UpdateExe;

            if (File.Exists(exesrc))
            {
                try
                {
                    System.Diagnostics.Process[] process = System.Diagnostics.Process.GetProcessesByName(IDevTrack.Update.Settings.UpdateExe);
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
                try
                {
                    File.Copy(exesrc, exedst, true);
                    Directory.Delete(Path.GetDirectoryName(exesrc), true);
                }
                catch { }
            }
        }
    }
}