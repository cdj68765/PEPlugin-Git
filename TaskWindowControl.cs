using MetroFramework;
using MetroFramework.Forms;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace PE多功能信息处理插件
{
    public partial class TaskWindowControl : UserControl
    {
        public TaskWindowControl()
        {
            this.Location = new System.Drawing.Point(100, 100);
            InitializeComponent();
        }

        private void metroLink2_Click(object sender, EventArgs e)
        {
            Clipboard.SetData(DataFormats.Text, "cdj68765@gmail.com");
            MetroMessageBox.Show(this, "已经复制到剪切板", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void metroLink1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/cdj68765/PmxEditor_plugin");
        }

        private void metroLink3_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://thielj.github.io/MetroFramework/");
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem((object state) =>
             {
                 try
                 {
                     var Update = new Regex(@"((?<!\d)((\d{2,4}(\.|年|\/|\-))((((0?[13578]|1[02])(\.|月|\/|\-))((3[01])|([12][0-9])|(0?[1-9])))|(0?2(\.|月|\/|\-)((2[0-8])|(1[0-9])|(0?[1-9])))|(((0?[469]|11)(\.|月|\/|\-))((30)|([12][0-9])|(0?[1-9]))))|((([0-9]{2})((0[48]|[2468][048]|[13579][26])|((0[48]|[2468][048]|[3579][26])00))(\.|年|\/|\-))0?2(\.|月|\/|\-)29))日?(?!\d))").Match(System.Text.Encoding.UTF8.GetString(new System.Net.WebClient().DownloadData("https://bowlroll.net/file/95442")));
                     if (Update.Value != Resource1.UpdateData)
                     {
                         Class2.newopen.BeginInvoke(new MethodInvoker(() =>
                         {
                             MetroTaskWindow.ShowTaskWindow(Class2.newopen.Parent, "检测到更新", new TaskWindowControl2(), 10);
                         }));
                     }
                     else
                     {
                         Class2.newopen.BeginInvoke(new MethodInvoker(() =>
                        {
                            MetroTaskWindow.ShowTaskWindow(Class2.newopen.Parent, "已经是最新版", new TaskWindowControl3(), 10);
                        }));
                     }
                 }
                 catch (Exception)
                 {
                 }
             });
        }
    }
}