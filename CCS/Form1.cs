using System.Drawing;
using System.Windows.Forms;

namespace CCS
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            pageBox1.RUN(Catcher.Controls.COMMOND.Load);
        }

        private void Form1_Load(object sender, System.EventArgs e)
        {
            Catcher.Utils.Notify.Register(this); //通知显示地址注册

            Catcher.Internal.Canls.LoadInit(istScope1.AddData);//初始化数据中转站
            Catcher.Internal.Canls.MatchOBJ(istScope1.AddItems, ToolBar_AddLineData); //匹配传送数据对象
            Text = Text + " - PKG:[" + Catcher.Internal.Canls.PkgName + "]";
        }

        private void ToolBar_AddLineData(string lname, Color clr, int idx)
        {
            //ToolAnls.DropDownItems.Add(lname, null, ToolBar_AnlsClick);
            //foreach (ToolStripMenuItem ele in ToolAnls.DropDownItems)
            //{
            //    if (ele.Text == lname)
            //    {
            //        ele.Checked = true;
            //        ele.ForeColor = clr;
            //        ele.Tag = idx;
            //        break;
            //    }
            //}
        }
    }
}