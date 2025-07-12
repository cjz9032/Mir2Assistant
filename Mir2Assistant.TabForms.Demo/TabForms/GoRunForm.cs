using Mir2Assistant.Common.Functions;
using Mir2Assistant.Common.Models;
using Mir2Assistant.Common.TabForms;
using Mir2Assistant.Utils;
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
            if (oldX != GameInstance!.CharacterStatus!.X!.Value || oldY != GameInstance.CharacterStatus.Y!)
            {
                GoRunFunction.GoRun(GameInstance, GameInstance.CharacterStatus.X!.Value, GameInstance.CharacterStatus.Y!.Value, direct, type);
                oldX = GameInstance!.CharacterStatus.X!.Value;
                oldY = GameInstance.CharacterStatus.Y!.Value;
            }

        }



        [DllImport("user32.dll")]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private void button10_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            GoRunFunction.FindPath(GameInstance!, int.Parse(textBox1.Text), int.Parse(textBox2.Text));

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
    }
}
