using System.Diagnostics;

namespace Mir2Assistant
{
    public partial class MainForm : Form
    {
        private Dictionary<int, AssiastantForm> AssiatantWindows = new Dictionary<int, AssiastantForm>();

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            HotKeyUtils.RegisterHotKey(Handle, 200, 0, Keys.Delete); // 注册热键
        }

        protected override void WndProc(ref Message m)//监视Windows消息
        {
            const int WM_HOTKEY = 0x0312; //如果m.Msg的值为0x0312那么表示用户按下了热键 
            switch (m.Msg)
            {
                case WM_HOTKEY:
                    var hwnd = WindowUtils.GetForegroundWindow();
                    var pid = WindowUtils.GetProcessId(hwnd);
                    var process = Process.GetProcessById(pid);
                    if (process.ProcessName == "mir1.dat")
                    {
                        if (AssiatantWindows.ContainsKey(pid))
                        {
                            if (AssiatantWindows[pid].Visible)
                            {
                                AssiatantWindows[pid].Hide();
                            }
                            else
                            {
                                AssiatantWindows[pid].Show();
                            }
                        }
                        else
                        {
                            var rect = WindowUtils.GetClientRect(hwnd);
                            var assiastant = new AssiastantForm();
                            AssiatantWindows.Add(pid, assiastant);
                            assiastant.hwnd = hwnd;
                            assiastant.pid = pid;
                            assiastant.baseAddress = process.MainModule.BaseAddress;
                            assiastant.Show();
                            assiastant.Location = new Point(rect.Left, rect.Top);
                        }
                    }
                    else
                    {
                        var form = AssiatantWindows.Values.Where(o => o.Handle == hwnd);
                        if (form.Any())
                        {
                            form.First().Hide();
                        }
                    }
                    break;

            }
            base.WndProc(ref m); //将系统消息传递自父类的WndProc 

        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            HotKeyUtils.UnregisterHotKey(Handle, 200);
        }
    }
}
