using Mir2Assistant.Common.Functions;
using Mir2Assistant.Common.Models;
using Mir2Assistant.Common.TabForms;
using Mir2Assistant.Common.Utils;
using System.Data;
using System.Reflection;

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

            foreach (string dllPath in dllFiles)
            {
                try
                {
                    var assembly = Assembly.LoadFrom(dllPath);
                    var forms = assembly.GetTypes().Where(o => o.IsClass && o.IsAssignableTo(typeof(ITabForm))).OrderBy(o => o.GetCustomAttribute<OrderAttribute>()?.Order);
                    foreach (var form in forms)
                    {
                        var tp = new TabPage();
                        var fm = (Form)assembly.CreateInstance(form.FullName!)!;
                        tp.Text = ((ITabForm)fm).Title;
                        ((ITabForm)fm).GameInstance = gameInstance;
                        fm.FormBorderStyle = FormBorderStyle.None;
                        fm.TopLevel = false;
                        fm.Parent = tp;
                        fm.ControlBox = false;
                        fm.Dock = DockStyle.Fill;
                        fm.Show();
                        tabControl.TabPages.Add(tp);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to load " + dllPath + ": " + ex.Message);
                }

            }
            //读状态
            Task.Run(() =>
            {
                Thread.Sleep(100);
                try
                {
                    while (gameInstance.LibIpdl > 0)
                    {
                        if (cancellationTokenSource.Token.IsCancellationRequested)
                        {
                            cancellationTokenSource.Token.ThrowIfCancellationRequested();
                            break;
                        }
                        CharacterStatusFunction.GetInfo(gameInstance);
                        Thread.Sleep(100);
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
    }
}
