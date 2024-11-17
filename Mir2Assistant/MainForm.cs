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
            HotKeyUtils.RegisterHotKey(Handle, 200, 0, Keys.Delete); // ע���ȼ�
        }

        protected override void WndProc(ref Message m)//����Windows��Ϣ
        {
            const int WM_HOTKEY = 0x0312; //���m.Msg��ֵΪ0x0312��ô��ʾ�û��������ȼ� 
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
            base.WndProc(ref m); //��ϵͳ��Ϣ�����Ը����WndProc 

        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            HotKeyUtils.UnregisterHotKey(Handle, 200);
        }
    }
}
