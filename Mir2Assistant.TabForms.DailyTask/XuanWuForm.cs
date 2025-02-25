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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

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
            doTask(listBox1.SelectedItem.ToString());
        }

        CancellationTokenSource cts = new CancellationTokenSource();
        private void button2_Click(object sender, EventArgs e)
        {
            cts.Cancel();
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
            GoRunFunction.GoXuanWu(GameInstance!);
        }

        private string? getTaskMonster(string? str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }
            var regex = new Regex("只(.*?)，|。");
            Match match = regex.Match(str);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 领奖领任务
        /// </summary>
        /// <returns></returns>
        private async Task<string?> getTask()
        {
            await NpcFunction.ClickNPC(GameInstance!, "玄武任务使者");
            await NpcFunction.Talk2Text(GameInstance!, "日常任务");
            await NpcFunction.Talk2Text(GameInstance!, "玄武岛除妖任务");
            await NpcFunction.Talk2Text(GameInstance!, "领取奖励");
            await NpcFunction.Talk2Text(GameInstance!, "领取奖励");
            await NpcFunction.ClickNPC(GameInstance!, "玄武任务使者");
            await NpcFunction.Talk2Text(GameInstance!, "日常任务");
            await NpcFunction.Talk2Text(GameInstance!, "玄武岛除妖任务");
            string str = await NpcFunction.Talk2Text(GameInstance!, "领取任务");
            return getTaskMonster(str);
        }

        private async Task doTask(string taskMonster)
        {
            dt!.FindPath.Clear();
            switch (taskMonster)
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
            await dt.RunTask();
            await GoRunFunction.GoXuanWu(GameInstance!);
        }

        /// <summary>
        /// 兑主号钻石凭证
        /// </summary>
        /// <returns></returns>
        private async Task HumanExp()
        {
            await NpcFunction.ClickNPC(GameInstance!, "修为兑换大使");
            await NpcFunction.Talk2Text(GameInstance!, "兑换主号修为");
            while (await NpcFunction.Talk2Text(GameInstance!, "使用10张白金修为凭证兑换10000点主号修为") != "请查看背包中兑换材料是否足够。\\")
            {
                Task.Delay(100).Wait();
            }
            await NpcFunction.ClickNPC(GameInstance!, "修为兑换大使");
            await NpcFunction.Talk2Text(GameInstance!, "兑换主号修为");
            while (await NpcFunction.Talk2Text(GameInstance!, "使用1张白金修为凭证兑换1000点主号修为") != "请查看背包中兑换材料是否足够。\\")
            {
                Task.Delay(100).Wait();
            }
            await NpcFunction.ClickNPC(GameInstance!, "修为兑换大使");
            await NpcFunction.Talk2Text(GameInstance!, "兑换主号修为");
            await NpcFunction.Talk2Text(GameInstance!, "下一页");
            while (await NpcFunction.Talk2Text(GameInstance!, "使用10张钻石修为凭证兑换100000点主号修为") != "请查看背包中兑换材料是否足够。\\")
            {
                Task.Delay(100).Wait();
            }
            await NpcFunction.ClickNPC(GameInstance!, "修为兑换大使");
            await NpcFunction.Talk2Text(GameInstance!, "兑换主号修为");
            await NpcFunction.Talk2Text(GameInstance!, "下一页");
            while (await NpcFunction.Talk2Text(GameInstance!, "使用1张钻石修为凭证兑换10000点主号修为") != "请查看背包中兑换材料是否足够。\\")
            {
                Task.Delay(100).Wait();
            }
        }

        /// <summary>
        /// 秘境天罡正气
        /// </summary>
        /// <returns></returns>
        private async Task tgzq()
        {
            await NpcFunction.ClickNPC(GameInstance!, "玄武老兵");
            await NpcFunction.Talk2Text(GameInstance!, "我要去玄武洞天");
            await NpcFunction.WaitNPC(GameInstance!, "狐人族长老");
            await NpcFunction.ClickNPC(GameInstance!, "狐人族长老");
            await NpcFunction.Talk2Text(GameInstance!, "关于转生神技");
            await NpcFunction.Talk2Text(GameInstance!, "传送至玄武石窟");
            await NpcFunction.WaitNPC(GameInstance!, "修行接引人");
            await NpcFunction.ClickNPC(GameInstance!, "修行接引人");
            await NpcFunction.Talk2Text(GameInstance!, "挑战石窟秘境");
            await NpcFunction.WaitNPC(GameInstance!, "修行教头");
            await NpcFunction.ClickNPC(GameInstance!, "修行教头");
            if (GameInstance!.TalkCmds.Any(o => o.StartsWith("免费接受挑战")))
            {
                await NpcFunction.Talk2Text(GameInstance!, "免费接受挑战");
                dt!.FindPath.Clear();
                dt.AddTaskMonster("秘境幽冥毒牙", 1);
                dt.FindPath.Enqueue(new Point(22, 30));
                dt.FindPath.Enqueue(new Point(33, 37));
                await dt.RunTask();
            }
        }

        private async void button5_Click(object sender, EventArgs e)
        {
            cts.Dispose();
            cts = new CancellationTokenSource();
            try
            {
                await Task.Run(async () =>
                {
                    await GoRunFunction.GoXuanWu(GameInstance!);
                    cts.Token.ThrowIfCancellationRequested();
                    var taskMonster = await getTask();
                    while (!string.IsNullOrEmpty(taskMonster))
                    {
                        cts.Token.ThrowIfCancellationRequested();
                        this.Invoke(() => listBox1.Text = taskMonster);
                        await doTask(taskMonster);
                        taskMonster = await getTask();
                    }
                    await HumanExp();
                    cts.Token.ThrowIfCancellationRequested();
                    await tgzq();
                    cts.Token.ThrowIfCancellationRequested();
                    await GoRunFunction.BackTu(GameInstance!);
                }, cts.Token);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("任务被取消");
            }
        }

    }
}

