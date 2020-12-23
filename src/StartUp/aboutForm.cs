using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace IDevTrack.StartUp
{
    public partial class aboutForm : Form
    {
        /// <summary>
        /// 接收反馈信息的邮箱账号们(发送给多个邮箱时,用";"隔开)
        /// </summary>
        public string MailSendTo { get; set; } = "";

        private string _mailtype = "";

        public aboutForm()
        {
            InitializeComponent();
            _mailtype = radioButton1.Text;
            radioButton1.Checked = true;
            radioButton2.Checked = false;
            radioButton3.Checked = false;
            button1.DialogResult = DialogResult.OK;
        }

        private void AboutForm_Load(object sender, EventArgs e)
        {
            string ownername = "admin";
            label1.Text = Application.ProductName + " v" + Application.ProductVersion;
            label2.Text = "Copyright © 2020 " + Application.ProductName + " Software. All Rights Reserved.";
            label3.Text = "许可期限:" + Utils.Licence.GetValidDayMark(); //获取许可信息
            label4.Text = "注册给 " + ownername;

            textBox2.Lines = Properties.Resources.history.Replace("\r\n", "\n").Split('\n'); //兼容LF,CRLF
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 2)
            {
                textBox1.Text = "";
                textBox3.Text = "";
                textBox4.Text = "";
                label6.Text = "0/" + textBox1.MaxLength.ToString();
                button1.Text = "发送";
                button1.DialogResult = DialogResult.None;
            }
            else
            {
                button1.Text = "确认";
                button1.DialogResult = DialogResult.OK;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 2)
            {
                string CTO = Regex.Replace(textBox3.Text, @"\s", "");
                bool checkok = false;
                if (CTO.Length != 0)
                {
                    if (Regex.IsMatch(CTO, "^[\\w-]+@[\\w-]+\\.(com|net|org|edu|mil|tv|biz|info)$"))
                    {
                        CTO = "Email:" + CTO;   //验证邮箱
                        checkok = true;
                    }
                    else if (Regex.IsMatch(CTO, @"^[1]+[3,5]+\d{9}"))
                    {
                        CTO = "Tel:" + CTO;     //验证电话号码
                        checkok = true;
                    }
                    else if (Regex.IsMatch(CTO, @"^[1-9]\d{4,11}$"))
                    {
                        CTO = "QQ:" + CTO;     //验证QQ
                        checkok = true;
                    }
                }
                if (Regex.Replace(textBox1.Text, @"\s", "").Length != 0 && checkok == true)
                {
                    SendMail("User:" + textBox4.Text + "    " + CTO + "\r\n\r\n" +
                        "[" + _mailtype + "]" + "\r\n" + textBox1.Text, _mailtype);
                }
                else
                {
                    IDevTrack.Utils.Notify.Send(1, "发送内容为空、联系方式未输入或为无效值");
                }
            }
        }

        public void SendMail(string cog, string sub)
        {
            Enabled = false; //禁止发送端
            string MMail = Config.Cfg.FeedbackMail;
            string AC = Config.Cfg.AuthorizationCode;

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
                    message.From = new MailAddress(MMail, Application.ProductName + " v" + Application.ProductVersion, Encoding.UTF8);
                    List<string> ToMiallist = MailSendTo.Split(';').ToList();
                    for (int i = 0; i < ToMiallist.Count; i++)
                    {
                        message.To.Add(new MailAddress(ToMiallist[i]));
                    }
                    message.SubjectEncoding = Encoding.GetEncoding(936);
                    message.IsBodyHtml = false;
                    message.Subject = sub;
                    message.BodyEncoding = Encoding.GetEncoding(936);
                    message.Body = cog;

                    try
                    {
                        client.Send(message); //发送消息

                        textBox1.Text = "";
                        label6.Text = "0/" + textBox1.MaxLength.ToString();
                        IDevTrack.Utils.Notify.Send(0, "发送成功,感谢反馈 ^_^");
                    }
                    catch (Exception)
                    {
                        IDevTrack.Utils.Notify.Send(3, "由于某些原因发送失败了 >_<");
                    }
                }
            }
            Enabled = true;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            label6.Text = textBox1.TextLength.ToString() + "/" + textBox1.MaxLength.ToString();
        }

        private void radioButton_MouseClick(object sender, MouseEventArgs e)
        {
            radioButton1.Checked = false;
            radioButton2.Checked = false;
            radioButton3.Checked = false;
            _mailtype = (sender as RadioButton).Text;
            (sender as RadioButton).Checked = true;
        }
    }
}