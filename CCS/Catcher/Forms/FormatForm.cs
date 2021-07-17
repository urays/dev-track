using System;
using System.Drawing;
using System.Windows.Forms;

namespace CCS.Catcher.Forms
{
    public partial class FormatForm : System.Windows.Forms.Form
    {
        public int FormatWidth { get; private set; } = Catcher.Internal.OPSwcode.DEFAULT_WIDTH;

        public int FormatHeight { get; private set; } = Catcher.Internal.OPSwcode.DEFAULT_HEIGHT;

        public int FormatIGs { get; private set; } = 0;

        public string FileName { get => Text; set => Text = value; }

        public FormatForm()
        {
            InitializeComponent();
        }

        private void FormatForm_Load(object sender, EventArgs e)
        {
            FormatWidth = Catcher.Internal.Canls.SupW;
            widthBox.Text = FormatWidth.ToString();
            FormatHeight = Catcher.Internal.Canls.SupH;
            heightBox.Text = FormatHeight.ToString();

            FormatIGs = 0;
            comboBox1.SelectedIndex = 0;

            button1.Enabled = true;
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            if (widthBox.TextLength == 0 || heightBox.TextLength == 0)
            {
                button1.Enabled = false;
            }
            else
            {
                button1.Enabled = true;
            }
        }

        private void textBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (e.KeyChar != 8 && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            widthBox.Focus();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FormatWidth = Convert.ToInt32(widthBox.Text);
            FormatHeight = Convert.ToInt32(heightBox.Text);
            FormatIGs = Convert.ToInt32(comboBox1.SelectedItem.ToString());
            DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}