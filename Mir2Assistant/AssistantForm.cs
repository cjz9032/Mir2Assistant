using Mir2Assistant.Common.Functions;
using Mir2Assistant.Common.Models;
using Mir2Assistant.Common.TabForms;
using Mir2Assistant.Common.Utils;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Mir2Assistant
{
    public partial class AssistantForm : Form
    {
        public MirGameInstanceModel? gameInstance { get; set; }



        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();


        public CharacterStatusModel CharacterStatus = new CharacterStatusModel();
        public AssistantForm()
        {
            InitializeComponent();
        }

        public AssistantForm(MirGameInstanceModel instance)
        {
            gameInstance = instance;
            InitializeComponent();
        }

        private void AssistantForm_Load(object sender, EventArgs e)
        {
            gameInstance!.MemoryUtils = new MemoryUtils(gameInstance);
            var configPath = Path.Combine(Directory.GetCurrentDirectory(), "config", $"{gameInstance.mirVer}.ini");
            if (!File.Exists(configPath))
            {
                MessageBox.Show($"当前版本{gameInstance.mirVer},暂无该版本的配置文件");
                return;
            }
            var cfgStr = File.ReadAllText(configPath, System.Text.Encoding.GetEncoding("gb2312"));
            foreach (var cfg in cfgStr.Split('\n'))
            {
                if (cfg?.Contains("=") ?? false)
                {
                    gameInstance.MirConfig.Add(cfg.Split('=')[0].Trim(), Convert.ToInt32(cfg.Split('=')[1].Trim(), 16));
                }
            }

            //加载tab页
            string currentDirectory = Directory.GetCurrentDirectory();
            string[] dllFiles = Directory.GetFiles(currentDirectory, "*.dll");

            var tabForms = new List<Form>();
            foreach (string dllPath in dllFiles)
            {

                try
                {
                    var assembly = Assembly.LoadFrom(dllPath);
                    var forms = assembly.GetTypes().Where(o => o.IsClass && o.IsAssignableTo(typeof(ITabForm))).OrderBy(o => o.GetCustomAttribute<OrderAttribute>()?.Order);
                    foreach (var form in forms)
                    {
                        var fm = (Form)assembly.CreateInstance(form.FullName!)!;
                        ((ITabForm)fm).GameInstance = gameInstance;
                        fm.FormBorderStyle = FormBorderStyle.None;
                        fm.TopLevel = false;
                        fm.ControlBox = false;
                        fm.Dock = DockStyle.Fill;
                        fm.Show();
                        tabForms.Add(fm);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to load " + dllPath + ": " + ex.Message);
                }
            }
            foreach (var fm in tabForms.OrderBy(o => (o.GetType()?.GetCustomAttribute<OrderAttribute>()?.Order ?? 9999)).ToList())
            {
                var tp = new TabPage();
                tp.Text = ((ITabForm)fm).Title;
                fm.Parent = tp;
                tabControl.TabPages.Add(tp);
            }
            //读状态
            Task.Run(() =>
            {
                try
                {
                    while (gameInstance.LibIpdl > 0)
                    {
                        Task.Delay(500).Wait();
                        if (cancellationTokenSource.Token.IsCancellationRequested)
                        {
                            cancellationTokenSource.Token.ThrowIfCancellationRequested();
                            break;
                        }
                        CharacterStatusFunction.GetInfo(gameInstance);
                        if (gameInstance.CharacterStatus!.MaxMP > 0 && gameInstance.CharacterStatus.MaxHP > 0)
                        {
                            if (gameInstance.Skills.Count == 0)
                            {
                                this.Invoke(() => SendMirCall.Send(gameInstance, 9001, [gameInstance.MirConfig["写屏CALL地址"], this.Handle]));

                                SkillFunction.ReadSkills(gameInstance);
                            }
                            MonsterFunction.ReadMonster(gameInstance);
                        }
                        else //已小退
                        {
                            gameInstance.Skills.Clear();
                            gameInstance.Monsters.Clear();
                            this.Invoke(() => SendMirCall.Send(gameInstance, 9002, new nint[] { }));

                        }
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.EndsWith("is not running."))
                    {
                        //游戏退出了
                        this.Invoke(() =>
                        {
                            DllInject.Unhook(gameInstance);
                            this.Dispose();
                        });

                    }
                    //将异常抛给主线程
                    //this.BeginInvoke(new EventHandler(delegate { throw ex; }));
                }
            });
        }

        private void AssistantForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }



        protected override void WndProc(ref Message m)
        {
            const int WM_COPYDATA = 0x004A;

            if (m.Msg == WM_COPYDATA)
            {
                // Extract the string from the COPYDATASTRUCT
                COPYDATASTRUCT cds = (COPYDATASTRUCT)m.GetLParam(typeof(COPYDATASTRUCT))!;
                string? msg = Marshal.PtrToStringAnsi(cds.lpData);
                var flag = m.WParam;
                gameInstance!.InvokeSysMsg(flag, msg);
            }

            base.WndProc(ref m);
        }

        // Define the COPYDATASTRUCT for C#
        private struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            public IntPtr lpData;
        }
    }
}
