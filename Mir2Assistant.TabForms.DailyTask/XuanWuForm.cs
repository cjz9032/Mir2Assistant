using Mir2Assistant.Common.Functions;
using Mir2Assistant.Common.Models;
using Mir2Assistant.Common.TabForms;
using Mir2Assistant.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mir2Assistant.TabForms.DailyTask
{
    /// <summary>
    /// 玄武岛任务
    /// </summary>
    [Order(100)]
    public partial class XuanWuForm : Form, ITabForm
    {
        public XuanWuForm()
        {
            InitializeComponent();
        }

        public string Title => "玄武任务";

        public MirGameInstanceModel? GameInstance { get; set; }

        private DailyTaskService? dt;
        private void XuanWuForm_Load(object sender, EventArgs e)
        {
            dt = new DailyTaskService(GameInstance!);
            dt.CompleteEvent += () => this.Invoke(() => button1.Enabled = true);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null)
            {
                return;
            }
            button1.Enabled = false;
            dt!.NPCPoint = new Point(210, 250);
            dt.FindPath.Clear();
            switch (listBox1.SelectedItem?.ToString())
            {
                case "噬魂毒牙":
                    dt.AddTaskMonster("噬魂毒牙", 3);
                    dt.FindPath.Enqueue(new Point(319, 146));
                    dt.FindPath.Enqueue(new Point(280, 202));
                    dt.FindPath.Enqueue(new Point(292, 302));
                    dt.FindPath.Enqueue(new Point(343, 330));
                    dt.FindPath.Enqueue(new Point(409, 273));
                    break;
                case "圣山竹鼠":
                    dt.AddTaskMonster("圣山竹鼠", 3);
                    dt.FindPath.Enqueue(new Point(142, 296));
                    dt.FindPath.Enqueue(new Point(270, 281));
                    dt.FindPath.Enqueue(new Point(263, 200));
                    dt.FindPath.Enqueue(new Point(360, 325));
                    dt.FindPath.Enqueue(new Point(409, 273));
                    break;
                case "幽冥毒牙":
                    dt.AddTaskMonster("幽冥毒牙", 3);
                    dt.FindPath.Enqueue(new Point(157, 164));
                    dt.FindPath.Enqueue(new Point(218, 359));
                    dt.FindPath.Enqueue(new Point(333, 330));
                    dt.FindPath.Enqueue(new Point(409, 273));
                    break;
                case "赤狐弩手":
                    dt.AddTaskMonster("赤狐弩手", 3);
                    dt.FindPath.Enqueue(new Point(289, 135));
                    dt.FindPath.Enqueue(new Point(255, 361));
                    dt.FindPath.Enqueue(new Point(328, 299));
                    dt.FindPath.Enqueue(new Point(365, 315));
                    break;
                case "赤狐劫掠者":
                    dt.AddTaskMonster("赤狐劫掠者", 3);
                    dt.FindPath.Enqueue(new Point(270, 130));
                    dt.FindPath.Enqueue(new Point(282, 302));
                    dt.FindPath.Enqueue(new Point(358, 322));
                    dt.FindPath.Enqueue(new Point(233, 361));
                    break;
                case "白狐刀客":
                    dt.AddTaskMonster("白狐刀客", 3);
                    dt.FindPath.Enqueue(new Point(270, 130));
                    dt.FindPath.Enqueue(new Point(253, 359));
                    dt.FindPath.Enqueue(new Point(358, 322));
                    dt.FindPath.Enqueue(new Point(233, 361));
                    break;
                case "浪客帮拳师":
                    dt.AddTaskMonster("浪客帮拳师", 3);
                    dt.FindPath.Enqueue(new Point(142, 68));
                    dt.FindPath.Enqueue(new Point(110, 96));
                    dt.FindPath.Enqueue(new Point(149, 138));
                    dt.FindPath.Enqueue(new Point(375, 465));
                    break;
                case "浪客帮香女":
                    dt.AddTaskMonster("浪客帮香女", 3);
                    dt.FindPath.Enqueue(new Point(142, 68));
                    dt.FindPath.Enqueue(new Point(110, 96));
                    dt.FindPath.Enqueue(new Point(149, 138));
                    dt.FindPath.Enqueue(new Point(375, 465));
                    break;
                case "浪客帮斧手":
                    dt.AddTaskMonster("浪客帮斧手", 3);
                    dt.FindPath.Enqueue(new Point(142, 68));
                    dt.FindPath.Enqueue(new Point(110, 96));
                    dt.FindPath.Enqueue(new Point(149, 138));
                    dt.FindPath.Enqueue(new Point(375, 465));
                    break;
                case "黑狐滚刀手":
                    dt.AddTaskMonster("黑狐滚刀手", 1);
                    dt.FindPath.Enqueue(new Point(233, 179));
                    dt.FindPath.Enqueue(new Point(272, 304));
                    dt.FindPath.Enqueue(new Point(238, 419));
                    dt.FindPath.Enqueue(new Point(385, 335));
                    break;
                case "浪客帮舵主":
                    dt.AddTaskMonster("浪客帮舵主", 1);
                    dt.FindPath.Enqueue(new Point(392, 452));
                    dt.FindPath.Enqueue(new Point(142, 68));
                    dt.FindPath.Enqueue(new Point(110, 96));
                    dt.FindPath.Enqueue(new Point(149, 138));
                    break;
                case "狱焰巨蛛":
                    dt.AddTaskMonster("狱焰巨蛛", 1);
                    dt.FindPath.Enqueue(new Point(314, 128));
                    dt.FindPath.Enqueue(new Point(322, 98));
                    break;
                case "野狐统领":
                    dt.AddTaskMonster("野狐统领", 1);
                    dt.FindPath.Enqueue(new Point(307, 289));
                    dt.FindPath.Enqueue(new Point(330, 289));
                    break;
                case "圣域猎人":
                    dt.AddTaskMonster("圣域猎人", 1);
                    dt.FindPath.Enqueue(new Point(349, 281));
                    dt.FindPath.Enqueue(new Point(365, 294));
                    break;
                case "时红夜蛇":
                    dt.AddTaskMonster("时红夜蛇", 1);
                    dt.FindPath.Enqueue(new Point(329, 335));
                    dt.FindPath.Enqueue(new Point(326, 362));
                    break;
                case "虎头海雕":
                    dt.AddTaskMonster("虎头海雕", 1);
                    dt.FindPath.Enqueue(new Point(93, 429));
                    dt.FindPath.Enqueue(new Point(72, 441));
                    break;
            }
            dt.RunTask();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            dt!.Complete = true;
            button1.Enabled = true;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            dt!.Pause = checkBox1.Checked;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            GoRunFunction.FlyCY(GameInstance!);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (GameInstance!.CharacterStatus!.MapName == "玄武岛")
            {
                GoRunFunction.FindPath(GameInstance, 210, 249);
            }
        }
    }
}
