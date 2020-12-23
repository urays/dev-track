using System.Collections.Generic;
using System.Windows.Forms;

namespace CCS.Catcher.Utils
{
    public static class Notify
    {
        //Success = 0,
        //Warning = 1,
        //info = 2,
        //Error = 3

        private const int KeepNotice = 2500;   //ms
        private const int X0 = 20;    //显示初始横坐标
        private const int Y0 = 20;    //显示初始纵坐标
        private const int BY = 80;    //离窗口底部的距离
        private const int space = 10; //通知间隔距离

        private static List<Catcher.Controls.NotifyBox> MsgList = null;
        private static Control DEST = null;
        private static int Y = Y0;

        /// <summary>
        /// 消息发送目的地注册
        /// </summary>
        /// <param name="dest">目的控件</param>
        public static void Register(Control dest)
        {
            MsgList = new List<Catcher.Controls.NotifyBox>();
            DEST = dest;
        }

        /// <summary>
        /// 向目标控件发送消息
        /// </summary>
        /// <param name="type">消息类型</param>
        /// <param name="text">消息内容</param>
        public static void Send(int type, string text)
        {
            if (DEST == null) { return; }

            for (int i = 0; i < MsgList.Count; i++)
            {
                if (MsgList[i].Visible == false)
                {
                    DEST.Controls.Remove(MsgList[i]);
                    MsgList.RemoveAt(i);
                    i--;
                }
            }
            if (MsgList.Count == 0) { Y = Y0; }

            MsgList.Add(new Catcher.Controls.NotifyBox());
            int index = MsgList.Count - 1;
            MsgList[index].Location = new System.Drawing.Point(X0, Y);

            if (Y + MsgList[index].Height + space < DEST.ClientSize.Height - BY)
            {
                Y = Y + MsgList[index].Height + space;
            }
            else { Y = Y0; }

            DEST.Controls.Add(MsgList[index]);
            MsgList[index].Notify(type, text, KeepNotice);
        }
    }
}