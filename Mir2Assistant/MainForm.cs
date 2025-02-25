using Mir2Assistant.Common.Models;
using Mir2Assistant.Common.Utils;
using System.Diagnostics;

namespace Mir2Assistant
{
    public partial class MainForm : Form
    {
        private Dictionary<int, MirGameInstanceModel> GameInstances = new Dictionary<int, MirGameInstanceModel>();
        private string currentProcessName = Process.GetCurrentProcess().ProcessName;
        public MainForm()
        {
            InitializeComponent();
        }

        HotKeyUtils hotKeyUtils = new HotKeyUtils();

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

                    HotKeyUtils.UnregisterHotKey(Handle, 200);
                    SendKeys.Send("{DEL}");
                    HotKeyUtils.RegisterHotKey(Handle, 200, 0, Keys.Delete); // 注册热键
                    var hwnd = WindowUtils.GetForegroundWindow();
                    var pid = WindowUtils.GetProcessId(hwnd);
                    var process = Process.GetProcessById(pid);

                    if (process.ProcessName == "mir1.dat")
                    {

                        if (GameInstances.ContainsKey(pid))
                        {
                            if (GameInstances[pid].AssistantForm!.Visible)
                            {
                                GameInstances[pid].AssistantForm!.Hide();
                            }
                            else
                            {
                                GameInstances[pid].AssistantForm!.Show();
                                GameInstances[pid].AssistantForm!.WindowState=FormWindowState.Normal;
                            }
                        }
                        else
                        {
                            var rect = WindowUtils.GetClientRect(hwnd);
                            var gameInstance = new MirGameInstanceModel();
                            GameInstances.Add(pid, gameInstance);
                            gameInstance.AssistantForm = new AssistantForm(gameInstance);
                            gameInstance.MirHwnd = hwnd;
                            gameInstance.MirPid = pid;
                            gameInstance.MirBaseAddress = process.MainModule!.BaseAddress;
                            gameInstance.mirVer = process.MainModule?.FileVersionInfo?.FileVersion;
                            gameInstance.MirThreadId = (uint)process.Threads[0].Id;
                            DllInject.loadDll(gameInstance);
                            gameInstance.AssistantForm.Show();
                            gameInstance.AssistantForm.Location = new Point(rect.Left, rect.Top);
                            gameInstance.AssistantForm.Disposed += (sender, args) =>
                            {
                                GameInstances.Remove(gameInstance.MirPid);
                            };

                        }
                    }
                    else if (process.ProcessName == currentProcessName)
                    {
                        GameInstances.Values.FirstOrDefault(o => o.AssistantForm?.Handle == hwnd)?.AssistantForm!.Hide();
                    }
                    break;

            }
            base.WndProc(ref m); //将系统消息传递自父类的WndProc 


        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            HotKeyUtils.UnregisterHotKey(Handle, 200);
            Task.Run(() =>
            {
                foreach (var gameInstance in GameInstances.Values)
                {

                    DllInject.Unhook(gameInstance);
                }
            });
            Thread.Sleep(200);


        }


    }
}
