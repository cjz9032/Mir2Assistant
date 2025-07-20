using Mir2Assistant.Common.Functions;
using Mir2Assistant.Common.Models;
using Mir2Assistant.Common.TabForms;
using Serilog;
using System.DirectoryServices.ActiveDirectory;
using System.Runtime.InteropServices;
using RadioButton = System.Windows.Forms.RadioButton;

namespace Mir2Assistant.TabForms.Demo
{
    /// <summary>
    /// 走路、跑路、寻路
    /// </summary>
    [Order(2)]
    public partial class GoRunForm : Form, ITabForm
    {
        public GoRunForm()
        {
            InitializeComponent();
        }

        public string Title => "走路跑路";
        public MirGameInstanceModel? GameInstance { get; set; }



        byte type { get; set; }
        byte direct { get; set; }




        int oldX = 0;
        int oldY = 0;

        private void GoRun_Load(object sender, EventArgs e)
        {
        }

        private void goRun(object sender, EventArgs e)
        {
            oldX = 0;
            oldY = 0;
            foreach (RadioButton radioButton in groupBox1.Controls.OfType<RadioButton>())
            {
                if (radioButton.Checked)
                {
                    type = byte.Parse(radioButton.Tag!.ToString()!);
                    break;
                }
            }
            direct = byte.Parse((sender as Button)!.Text);
            if (direct == 5)
            {
                timer1.Enabled = false;
            }
            else
            {
                timer1.Enabled = true;
            }
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            //if (oldX != GameInstance!.CharacterStatus!.X!.Value || oldY != GameInstance.CharacterStatus.Y!)
            //{
            //    GoRunFunction.GoRun(GameInstance, GameInstance.CharacterStatus.X!.Value, GameInstance.CharacterStatus.Y!.Value, direct, type);
            //    oldX = GameInstance!.CharacterStatus.X!.Value;
            //    oldY = GameInstance.CharacterStatus.Y!.Value;
            //}

        }



        [DllImport("user32.dll")]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private CancellationTokenSource? _cancellationTokenSource;

        private void button10_Click(object sender, EventArgs e)
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(async () =>{
                try
                {
                    bool pathFound = await PerformPathfinding(_cancellationTokenSource.Token);
                    if (pathFound)
                    {
                        Log.Information("寻路任务成功完成。");
                    } else {
                        Log.Warning("寻路任务未成功完成。");
                    }
                }
                catch (OperationCanceledException)
                {
                    Log.Information("寻路任务已取消。");
                }
                finally
                {
                    _cancellationTokenSource = null;
                }
            }, _cancellationTokenSource.Token);
        }

        private async Task<bool> PerformPathfinding(CancellationToken cancellationToken)
        {
            // todo 寻找路径目的地需要容错处理

            CharacterStatusFunction.GetInfo(GameInstance!);
            MonsterFunction.ReadMonster(GameInstance!);

            var stopwatchTotal = new System.Diagnostics.Stopwatch();
            stopwatchTotal.Start();
            
            // gameInstance.Monsters -- 额外的怪物也是障碍点
            var monsterCount = GameInstance!.Monsters.Count;
            int[][] monsPos = new int[monsterCount][];
            int index = 0;
            foreach (var monster in GameInstance!.Monsters)
            {
                monsPos[index++] = new int[] {
                    monster.Value.X.Value,
                    monster.Value.Y.Value
                };
            }

            var goNodes = GoRunFunction.genGoPath(GameInstance!, int.Parse(textBox1.Text), int.Parse(textBox2.Text), monsPos, 3, false);
            stopwatchTotal.Stop();
            Log.Debug($"寻路: {stopwatchTotal.ElapsedMilliseconds} 毫秒");
            if (goNodes.Count == 0)
            {
                return false;
            }
            while (goNodes.Count > 0)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }
                var node = goNodes[0];
                goNodes.RemoveAt(0);

                var oldX = GameInstance!.CharacterStatus.X.Value;
                var oldY = GameInstance!.CharacterStatus.Y.Value;


                var (nextX, nextY) = GoRunFunction.getNextPostion(oldX, oldY, node.dir, node.steps);

                GoRunFunction.GoRunAlgorithm(GameInstance, oldX, oldY, node.dir, node.steps);

                var tried = 0;
                while(true)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return false;
                    }
                    await Task.Delay(100, cancellationToken);
                    CharacterStatusFunction.FastUpdateXY(GameInstance!);
                    MonsterFunction.ReadMonster(GameInstance!);

                    // 执行后发生了变更
                    var newX = GameInstance!.CharacterStatus.X.Value;
                    var newY = GameInstance!.CharacterStatus.Y.Value;

                    tried++;
                    if (tried > 20)
                    {
                        return await PerformPathfinding(cancellationToken);
                    }

                    if (oldX != newX || oldY != newY)
                    {
                        if (nextX == newX && nextY == newY)
                        {
                            break;
                        } else {
                            // 遇新障了,导致位置不能通过,或偏移，重新执行寻路逻辑
                            return await PerformPathfinding(cancellationToken);
                        }
                    }
                    // 否则继续等待
                }
            }

            return true;
        }

        private void button11_Click(object sender, EventArgs e)
        {

            SendMirCall.Send(GameInstance!, 1, new nint[] {});

            //SendMirCall.Send(GameInstance!, 9999,
            //AsmUtils.Init(GameInstance!.MirPid)
            //   .Push6A(0)
            //   .Push6A(0)
            //   .Push6A(0)
            //   .Push6A(0)
            //   .Mov_EAX((int)GameInstance.MirConfig["通用参数"])
            //   .Mov_EAX_DWORD_Ptr_EAX()
            //   .Mov_EAX_DWORD_Ptr_EAX()
            //   .Mov_ECX(0)
            //   .Mov_EDX(0x11B2)
            //   .Mov_EBX((int)GameInstance!.MirConfig["对话CALL地址"])
            //   .Call_EBX()
            //   .GetAsmCode());
        }

        private void buttonGroup_Click(object sender, EventArgs e)
        {
            if (GameInstance == null) return;
            string member = textBoxMember.Text.Trim();
            if (string.IsNullOrEmpty(member)) return;

            nint[] data = Mir2Assistant.Common.Utils.StringUtils.GenerateCompactStringData(member);
            SendMirCall.Send(GameInstance, 9004, data);
        }
    }
}
