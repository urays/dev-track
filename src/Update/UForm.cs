using System;
using System.Windows.Forms;

namespace IDevTrack.Update
{
    public partial class UForm : Form
    {
        private readonly string[] Lst = Array.Empty<string>();

        public UForm(string[] lst)
        {
            Lst = lst;

            InitializeComponent();
            button1.DialogResult = DialogResult.Cancel;
            button2.DialogResult = DialogResult.OK;
            button1.TabIndex = 0;
            button2.TabIndex = 1;
        }

        private void updateForm_Load(object sender, EventArgs e)
        {
            //listBox1.Items.Add("---------------------------");
            if (Application.ProductVersion == Lst[0])
            {
                listBox1.Items.Add("v" + Application.ProductVersion + " <= (OPT)");
            }
            else
            {
                listBox1.Items.Add("v" + Application.ProductVersion + " => v" + Lst[0]);
            }
            listBox1.Items.Add("---------------------------");
            for (int i = 1; i < Lst.Length; i++)
            {
                listBox1.Items.Add(Lst[i]);
            }
        }
    }
}