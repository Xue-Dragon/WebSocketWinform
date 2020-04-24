using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataSocket
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void CheckBox1_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                Busi.SetInit();
                Busi.JobStartAll();
                Busi.DataRuning = true;
                Tool.ShowStat("任务全部启动！");
            }
            else
            {
                Busi.JobCloseAll();
                Busi.DataRuning = false;
                Tool.ShowStat("任务全部关闭！");
            }
        }

        private void CheckBox2_Click(object sender, EventArgs e)
        {
            Const.IsDebug = checkBox2.Checked;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Const.Main = this;
        }

        private void CheckBox3_Click(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
            {
                WebSocket.Start();
            }
            else
            {
                WebSocket.End();
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            Busi.SetInit();
            Tool.ShowStat("完成车辆配置信息更新");
        }
    }
}
