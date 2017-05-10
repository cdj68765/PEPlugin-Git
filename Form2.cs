using MetroFramework;
using MetroFramework.Forms;
using System;
using System.Windows.Forms;

namespace PE多功能信息处理插件
{
    public partial class Form2 : MetroForm
    {
        internal string Password = "";

        public Form2()
        {
            InitializeComponent();
        }

        private void metroTextBox1_ButtonClick(object sender, EventArgs e)
        {
            this.Password = metroTextBox1.Text;
            if (Password == "")
            {
                MetroMessageBox.Show(this, "密码不能为空", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                this.Close();
            }
        }
    }
}