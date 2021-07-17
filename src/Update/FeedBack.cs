using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Windows.Forms;
using System.Net;

namespace IDevTrack.Update
{
    public static class FeedBack
    {
        public const string FeedbackMail = "DevTrack@163.com";
        public const string AuthorizationCode = "rays2020stvumn";

        //默认的反馈信息目标邮箱
        //"xxx@qq.com;xxx@163.com;xxx@xxx.com" 分号隔开即可
        public const string ReceiveEmail = "2930589025@qq.com";

        public static void Notify(string user, string upVer)
        {
            string MMail = FeedbackMail;
            string AC = AuthorizationCode;
            string To = ReceiveEmail;

            using (SmtpClient client = new SmtpClient())
            {
                string[] addressor = MMail.Trim().Split(new Char[] { '@', '.' });
                switch (addressor[1])
                {
                    case "163": client.Host = "smtp.163.com"; break;
                    case "126": client.Host = "smtp.126.com"; break;
                    case "qq": client.Host = "smtp.qq.com"; break;
                    case "gmail": client.Host = "smtp.gmail.com"; break;
                    case "foxmail": client.Host = "smtp.foxmail.com"; break;
                    case "sina": client.Host = "smtp.sina.com.cn"; break;
                }
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential(MMail, AC);

                using (MailMessage message = new MailMessage())
                {
                    message.From = new MailAddress(MMail, "DevTrack", Encoding.UTF8); //From
                    List<string> ToMiallist = To.Split(';').ToList();
                    for (int i = 0; i < ToMiallist.Count; i++)
                    {
                        message.To.Add(new MailAddress(ToMiallist[i]));
                    }
                    message.SubjectEncoding = Encoding.GetEncoding(936);
                    message.IsBodyHtml = false;
                    message.Subject = "User Info";
                    message.BodyEncoding = Encoding.GetEncoding(936);
                    message.Body = "USER:" + user + "\r\n" + "VERSION:v" + Application.ProductVersion + " => " + upVer + "\r\n" + GetIP() + "\r\n";

                    try { client.Send(message); }
                    catch (Exception) { return; }
                }
            }
        }

        public static string GetIP()
        {
            string res = "";

            IPAddress[] ips;    //定义IPAddress类数组对象ips用于存放获取出来的IP

            res += Dns.GetHostName();
            ips = Dns.GetHostAddresses(Dns.GetHostName());    //取得计算机内网IP，其中Dns.GetHostName()是取得计算机名称

            foreach (IPAddress ip in ips)    //遍历数组，取出第一个IPV4的地址
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    res += (ip.ToString() + "\r\n");
                }
            }
            return res;
        }
    }
}