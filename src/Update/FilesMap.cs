using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace IDevTrack.Update
{
    public static class FilesMap
    {
        //System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase //本地根目录

        private static readonly Dictionary<string, string> fmap = new Dictionary<string, string>();
        private static readonly string CurPath = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

        private static void GetFilesMap(DirectoryInfo dir)
        {
            if (dir == null) { return; }
            FileInfo[] allFile = dir.GetFiles();
            foreach (FileInfo fi in allFile)
            {
                if (Array.IndexOf(IDevTrack.Update.Settings.nFiles, fi.Name) == -1)
                {
                    fmap.Add(fi.FullName.Substring(CurPath.Length).Replace("/", "\\"),
                         IDevTrack.Update.Extra.MD5Encrypt(fi.FullName));
                }
            }
            DirectoryInfo[] allDir = dir.GetDirectories();
            foreach (DirectoryInfo d in allDir)
            {
                if (Array.IndexOf(IDevTrack.Update.Settings.nDirs,
                    d.FullName.Substring(CurPath.Length).Replace("/", "\\")) == -1)
                {
                    GetFilesMap(d);
                }
            }
        }

        public static Dictionary<string, string> GetLocalFiles()
        {
            fmap.Clear();
            GetFilesMap(new System.IO.DirectoryInfo(CurPath));
            return fmap;
        }

        public static string GetRemoteList(string urll)
        {
            string cog = "";
            using (WebClient client = new WebClient())
            {
                client.Encoding = Encoding.Default;
                try
                {
                    cog = client.DownloadString(urll);
                }
                catch (Exception)
                {
                    //MessageBox.Show("无法获取更新源~");
                    return "";
                }
            }
            return cog;
        }

        /// <summary>
        /// 获取资源列表中的节点值
        /// </summary>
        /// <param name="cog">资源列表内容</param>
        /// <param name="node">REMOTE-false;FORCEDEL-true;TOUPDATE-true;</param>
        /// <returns>节点内容</returns>
        public static string GetNode(string cog, string node)
        {
            if (cog == null) { return ""; }
            try
            {
                string rule = node + @"\{(.*)\}" + node;
                return Regex.Match(cog, rule, RegexOptions.Singleline).Groups[1].Value;
            }
            catch (Exception) { return cog; }
        }

        /// <summary>
        /// 创建资源列表并写入到磁盘
        /// </summary>
        /// <returns>true:创建成功</returns>
        public static bool WriteToLocal()
        {
            string reslist_name = CurPath + IDevTrack.Update.Settings.UpdateXml;

            fmap.Clear();
            GetFilesMap(new System.IO.DirectoryInfo(CurPath)); //获取本地资源列表

            //FORCEDEL(
            //[canls/dev](d)
            //[canls/dev/camera.h](f)
            //)FORCEDEL
            string cog = "VERSION{" + Application.ProductVersion + "}VERSION\n" +
                "REMOTE{" + IDevTrack.Update.Settings.RemoteRes + "}REMOTE\n\n" +
                "FORCEDEL{\n[" + IDevTrack.Update.Settings.Temp + "](d)\n" + "}FORCEDEL\n\n" +
                "TOUPDATE{\n";
            foreach (KeyValuePair<string, string> kvp in fmap)
            {
                cog += ("[" + kvp.Key + "]" + "(" + kvp.Value + ")\n");
            }
            cog += "}TOUPDATE\n";

            if (File.Exists(reslist_name)) { File.Delete(reslist_name); }
            IDevTrack.Update.Extra.WriteOut(reslist_name, cog); //覆盖方式写出XML

            return File.Exists(reslist_name) ? true : false;
        }
    }
}