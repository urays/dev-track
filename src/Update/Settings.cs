namespace IDevTrack.Update
{
    public static class Settings //Encoding.UTF8 no BOM;CRLF
    {
        //程序更新清单文件名
        public const string UpdateXml = "update.xml";

        //更新程序名
        public const string UpdateExe = "Update.exe";

        //远程资源库
        public const string RemoteRes = @"https://gitee.com/OTRACK/devtrack-release/raw/master/";

        //远程程序更新清单
        public const string RemoteXml = @"https://gitee.com/OTRACK/devtrack-release/raw/master/update.xml";

        //临时文件夹
        public const string Temp = @"~temp\";

        //不被更新的文件
        public static readonly string[] nFiles = { "update.xml", "idkey.pub", "license.lic" };

        //不被访问的文件夹
        public static readonly string[] nDirs = { ".git" };

        //LF->CRLF
        public static readonly string[] CRLF = { "txt", "c", "h", "md", "xml", "lic", "cpp", "hpp" };
    }
}