using MetroFramework;
using System;
using System.Windows.Forms;

namespace PE多功能信息处理插件
{
    public partial class TaskWindowControl3 : UserControl
    {
        public TaskWindowControl3()
        {
            this.Location = new System.Drawing.Point(100, 100);
            InitializeComponent();
            if (Program.bootstate.ShowUpdata == 0)
            {
                metroCheckBox1.Checked = true;
            }
            else
            {
                metroCheckBox1.Checked = false;
            }
        }

        private void metroLink2_Click(object sender, EventArgs e)
        {
            Clipboard.SetData(DataFormats.Text, "https://bowlroll.net/file/95442");
            System.Diagnostics.Process.Start("https://bowlroll.net/file/95442");
            MetroMessageBox.Show(this, "已经复制到剪切板", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void metroCheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (metroCheckBox1.Checked)
            {
                Program.bootstate.ShowUpdata = 0;
            }
            else
            {
                Program.bootstate.ShowUpdata = 1;
            }
        }
    }
}