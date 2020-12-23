using IDevTrack.IControls;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace IDevTrack.Utils
{
    public static class Notify
    {
        private const int KeepNotice = 2500;   //ms
        private const int X0 = 50;    //显示初始横坐标
        private const int Y0 = 50;    //显示初始纵坐标
        private const int BY = 80;    //离窗口底部的距离
        private const int space = 10; //通知间隔距离

        private static List<NotifyBox> MsgList = null;
        private static Form Destination = null;
        private static int MaxY = 0;

        private static int Y = Y0;

        public static void register(Form dest)
        {
            MsgList = new List<NotifyBox>();
            Destination = dest;
            MaxY = Destination.ClientSize.Height - BY;
        }

        public static void Send(int type, string text)
        {
            if (Destination == null) { return; }
            Debug.WriteLine(MsgList.Count.ToString());
            for (int i = 0; i < MsgList.Count; i++)
            {
                if (MsgList[i].Visible == false)
                {
                    Destination.Controls.Remove(MsgList[i]);
                    MsgList.RemoveAt(i);
                    i--;
                }
            }
            if (MsgList.Count == 0) { Y = Y0; }

            MsgList.Add(new NotifyBox());
            int index = MsgList.Count - 1;
            MsgList[index].Location = new System.Drawing.Point(X0, Y);

            if (Y + MsgList[index].Height + space >= MaxY) { Y = Y0; }
            else { Y = Y + MsgList[index].Height + space; }

            Destination.Controls.Add(MsgList[index]);
            MsgList[index].Notify(type, text, KeepNotice);
        }
    }
}