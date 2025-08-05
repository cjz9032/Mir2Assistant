using Mir2Assistant.Common.Functions;
using Mir2Assistant.Common.Models;
using Mir2Assistant.Common.Utils;
using Serilog; // 新增Serilog引用
using Serilog.Sinks.Debug; // 添加Debug sink引用
using System.Diagnostics;
using System.Text.Json;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
using Mir2Assistant.Common;
using Mir2Assistant.Common.Constants;

namespace Mir2Assistant
{
    public partial class MainForm : Form
    {
        private string currentProcessName = Process.GetCurrentProcess().ProcessName;
        private List<GameAccountModel> accountList = new List<GameAccountModel>();
        private string configFilePath = Path.Combine(Directory.GetCurrentDirectory(), "accounts.json");
        private string gamePath = "ZC.H"; // 游戏路径，需要配置
        private string gameDirectory = @"G:\cq\cd"; // 游戏所在目录
        private string encodeArgMainLarge = @"kfOoahYhb3fG+gA4fSangPnDCxTkr4MFKyoW9LH4W28SVhlJvRGmm3gS9BTJKj1XXKvbBY2jXq7Oijy5Ndyrl26OkaL1VZhHlhVKhSCUYve1TH9GgI4qZyM4JQPJeGhjP37oFxARRsxzev4HbOsulCkXoRO0uL5VGX7T5vIiCZRE9nhbDDW4H34zLNXwBQGoml/S2EcHXUyIndJnOMyddNYzS1zAJFKTPwhE6mOZXoIhO2cOuNhGVepTwwsJDAiPg68l5nHGkAG7SGqD2JP5GALD/LP7noJKKjFT3Y+fo4gvF4YR+U0fMwyDPu8kkpZ3/V3AOUpFHJvBoAd878ebSCCKxEYvgKFN/W0J1UzoN4wzeoX9zR/GjiENNDH/EYf9/GutWxwxMm9IcXHWnnTnffdoxxLaWX95NEyCLtEi7+7fF2gM1YTIAwG3Nyd26oj3WkabRDygpttgLbVpLKuffRFiuvoJdGIYkcjF2uC/mK4lXGupfWN9Ieh1Mpr4lk8O90FMLHwVOVTdGxYva+N91uCw3hzpRrJaojwCWLVfPHJznl+7emqylU0l9/4dzdNfdpSKDryBlGg1hqHGGXgQ5RJ02VMNeQSHpqflOx/XuSjaNvKUTsqnl55iP2gQ4ntEM4hM9WBjeX6/guFChBXVDet6rwXMf7GZJvQIpg3btjFjcMKrXqyWhdFaNmz9uabWf4neA8C06+y8aBcPDgEQX7Uin98WLKTHKKb00AZYxNuT2yfTOzzJavnCn2kSGmj8sk5I1kZr8dTcVo2gdC77o8kCuY4dtGbqOExFv6RFNtrIwywUv6pSFN9DsSvKWzwiJwjn+2YqzZxJitB1t07y8ByRygWbvpCqmb9jqS+MWX9zn/ZGnb61JjJVFZDdHUyq1URBz+Q1tOj8bJ1o5aGi3cQbG0v00oH66+uOXzVXNlHbkDy6Y5nyUBY39TLHFelJDlBgI68D6QLx21rwk3nzV2HqTbQX+4JaJMCkH0xd3GLbh8QV2M5u6gDWfr+MXyWM";
        private string encodeArgOtherSmall = @"4V/1bwu8lqrZMQVVO1BIYDHyVWUSBU7O08Cx4QUu4TRozkhjtCiplv/eCIDhcjJ9ZbfM2+f7XHWNpGdCVOgKjqnEHHweuCVM3AldA0HoF0IPD70w9Xd8WYRe3SnhSlS409969oAOQRrSOqNvCAWjZYt/ICyYM6igVotv9RzLSL3azgJZN/qdECOu7KCnxeva/XBAixA7h3XBvMggke6e698XwXFs0ISyNrSj+oFvw8rk02uRodUagCowmh8cGib9QpcFxrK1TuUJKgRbqK03QcN+pLSxrq8e+UwtamASEFM3wnQPB6K+pMtOZOkcrr8+Kd7klYlXBRrYbT8hvYQ8eF0WgbOEN4KzThDOXMa0INxVX94UxemyUhXIGJGOAN3yIWhy9jYBR9PbZ+yFvtTo0Alz5GmAvQV2zONMZeeI9RleWewtwRBjMwraS8XCwJfHgyS6nZNmEfVQmULQmCC2Ow9MSFbYBloawb9aNqBgS2esl6djUA3y4+ozQjcW9AdN+WJMeWYUAQGIkQ7GVYQFi5LiXYmmev7WvezzjXin7NtYoRKkTMPtnfqLTwHBueeO5adtlz2RlgN5cte47l6fBgsTh23OHcJqT8cuJv99C9deLIDWFNY1/+8HFw8X2fPJvVgJNRJKMULWV4iQ3KgmU75ZLp+QAxTGJrQA5bicxCU30mjIaY+vCD2HPznaUugskIBLSWk4q47lDR0e8DtqH/Bf00rW0s/ymd9CouAufsDWw2tRIHD+iFaFZc5UoLCHpuZK77neQNjrydJyg6077Hrxf5yZOfdlUzUWKTzdgblCAMLXApPX1TSNedsrwgBpP55O0CLaEgr4l5xPFwPtYAX289hd6JLmopEehIzrFPyJc2awmn5A/ST2wukLdySweiKjShHk2BWRXYYkxxI0xM4pIFEIkPujKWimzNeXfSMeKrjb+TGbIQDauvVMhUhMHqZ8mu33XeaQezUyfSttDjpTjzGpaxVEcoRtb/QLHGBGY9GrpLMWpu1FbeyJHeUv";
        public MainForm()
        {
            InitializeComponent();
            LoadAccountList();

            // 为每个账号创建一个游戏实例
            foreach (var account in accountList)
            {
                var gameInstance = new MirGameInstanceModel
                {
                    AccountInfo = account
                };
                gameInstance.AccountInfo = account;

                GameState.GameInstances.Add(gameInstance);
            }

            var configPath = Path.Combine(Directory.GetCurrentDirectory(), "config", "2.1.0.1226.ini");
            var cfgStr = File.ReadAllText(configPath, System.Text.Encoding.GetEncoding("gb2312"));

            foreach (var cfg in cfgStr.Split('\n'))
            {
                if (cfg?.Contains("=") ?? false)
                {
                    GameState.MirConfig.Add(cfg.Split('=')[0].Trim(), Convert.ToInt32(cfg.Split('=')[1].Trim(), 16));
                }
            }

        }

        HotKeyUtils hotKeyUtils = new HotKeyUtils();

        private void MainForm_Load(object sender, EventArgs e)
        {
            // 配置Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(Path.Combine(Directory.GetCurrentDirectory(), "logs\\app_.log"), rollingInterval: RollingInterval.Day)
                .WriteTo.Debug()
                .CreateLogger();

            Log.Information("应用程序启动");
            HotKeyUtils.RegisterHotKey(Handle, 200, 0, Keys.Delete); // 注册热键
            Log.Debug("已注册热键: Delete");
            // todo 目前还好, 就是自动的runner对所有生效
            autoAtBackgroundFast();
            autoAtBackground();
            // RefreshDataGrid();
            Task.Run(async () => {
               while (true)
               {
                   this.Invoke(new Action(() => RefreshDataGrid()));
                   await Task.Delay(30_000);
               }
            });
          
        }

        private void LoadAccountList()
        {
            try
            {
                Log.Debug("开始加载账号列表，配置文件路径: {ConfigFilePath}", configFilePath);
                if (File.Exists(configFilePath))
                {
                    string json = File.ReadAllText(configFilePath);
                    accountList = JsonSerializer.Deserialize<List<GameAccountModel>>(json) ?? new List<GameAccountModel>();
                    Log.Information("成功加载账号列表，共 {AccountCount} 个账号", accountList.Count);
                }
                else
                {
                    // 创建示例账号
                    // Log.Information("配置文件不存在，创建示例账号");
                    // accountList = new List<GameAccountModel>
                    // {
                    //     new GameAccountModel { Account = "adad", Password = "adad", CharacterName = "sad13", IsMainControl = true },
                    //     new GameAccountModel { Account = "acac", Password = "acac", CharacterName = "sad14", IsMainControl = false }
                    // };
                    // SaveAccountList();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "加载账号列表失败");
                // MessageBox.Show($"加载账号列表失败: {ex.Message}");
            }
        }

        private void SaveAccountList()
        {
            try
            {
               Log.Debug("开始保存账号列表，共 {AccountCount} 个账号", accountList.Count);
               string json = JsonSerializer.Serialize(accountList, new JsonSerializerOptions { 
                   WriteIndented = true,
                   Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
               });
               File.WriteAllText(configFilePath, json, System.Text.Encoding.UTF8);
               Log.Information("账号列表保存成功，路径: {ConfigFilePath}", configFilePath);
            }
            catch (Exception ex)
            {
               Log.Error(ex, "保存账号列表失败");
            //    MessageBox.Show($"保存账号列表失败: {ex.Message}");
            }
        }

        private void RefreshDataGrid()
        {
            // 保存当前列顺序和位置
            Dictionary<string, int> columnOrder = new Dictionary<string, int>();
            if (dataGridViewAccounts.Columns.Count > 0)
            {
                foreach (DataGridViewColumn col in dataGridViewAccounts.Columns)
                {
                    columnOrder[col.Name] = col.DisplayIndex;
                }
            }


            var accountViews = accountList.Select(a => new
            {
                Account = a.Account,
                Password = a.Password,
                CharacterName = a.CharacterName,
                RoleType = a.role,
                IsMainControl = a.IsMainControl,
                ProcessId = GameState.GameInstances.FirstOrDefault(g => g.AccountInfo?.Account == a.Account)?.MirPid ?? 0,
                Level = GameState.GameInstances.FirstOrDefault(g => g.AccountInfo?.Account == a.Account)?.CharacterStatus?.Level ?? 0,
                HP = GameState.GameInstances.FirstOrDefault(g => g.AccountInfo?.Account == a.Account)?.CharacterStatus?.CurrentHP ?? 0,
                TaskMain0Step = a.TaskMain0Step,
                TaskSub0Step = a.TaskSub0Step
            }).ToList();

            dataGridViewAccounts.DataSource = null;
            dataGridViewAccounts.DataSource = new BindingSource { DataSource = accountViews };
            
            // 恢复列顺序和位置
            if (columnOrder.Count > 0)
            {
                foreach (DataGridViewColumn col in dataGridViewAccounts.Columns)
                {
                    if (columnOrder.ContainsKey(col.Name))
                    {
                        col.DisplayIndex = columnOrder[col.Name];
                    }
                }
            }
        }

        private async Task<bool> StartGameProcess(MirGameInstanceModel gameInstance)
        {
            var account = gameInstance.AccountInfo;
            try
            {
                string arguments = gameInstance.AccountInfo.IsMainControl ? encodeArgMainLarge : encodeArgOtherSmall;
                // 使用 -PassThru 获取进程对象，并输出PID
                string psCommand = $"cd '{gameDirectory}'; $p=Start-Process -FilePath './{gamePath}' -ArgumentList '{arguments}' -NoNewWindow -PassThru; $p.Id";

                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{psCommand}\"",
                    UseShellExecute = false, // 必须为false才能重定向输出
                    RedirectStandardOutput = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                var process = System.Diagnostics.Process.Start(psi);
                await Task.Delay( Environment.ProcessorCount <=4 ?  10_000 : 6000);

                string output = process.StandardOutput.ReadLine(); // 只读一行即可
                // 不需要 WaitForExit
                if (int.TryParse(output.Trim(), out int pid))
                {
                    gameInstance.GameInfo("PowerShell已直接启动游戏进程，账号: {Account}, PID: {Pid}", account.Account, pid);
                    // no attach前不能有 gameInstance.MirPid = pid;
                    // 通过PID获取进程对象
                    var gameProcess = Process.GetProcessById(pid);
                    // 后续绑定DLL等逻辑
                    return await AttachToGameProcess(gameProcess, account);
                }
                else
                {
                    gameInstance.GameWarning("未能获取到新进程PID，账号: {Account}", account.Account);
                    // MessageBox.Show("无法获取新进程PID，请手动启动游戏。");
                }

            }
            catch (Exception ex)
            {
                gameInstance.GameError("启动游戏进程失败: {Error}", ex.Message);
                // MessageBox.Show($"启动游戏失败: {ex.Message}");
            }
            return false;
        }

        private void beforeClose(MirGameInstanceModel gameInstance){
             // 如果有关联的辅助窗口，先解除挂钩并关闭
            gameInstance.GameInfo("准备关闭游戏资源，账号: {Account}, PID: {ProcessId}", gameInstance.AccountInfo.Account, gameInstance.MirPid);
            gameInstance.GameDebug("解除DLL挂钩并关闭辅助窗口");
            DllInject.Unhook(gameInstance);
            if (gameInstance.AssistantForm != null && gameInstance.AssistantForm.IsHandleCreated && !gameInstance.AssistantForm.IsDisposed)
            {
                try 
                {
                    if (gameInstance.AssistantForm.InvokeRequired)
                    {
                        gameInstance.AssistantForm.Invoke(new Action(() => gameInstance.AssistantForm.Close()));
                    }
                    else
                    {
                        gameInstance.AssistantForm.Close();
                    }
                }
                catch (Exception ex)
                {
                    gameInstance.GameWarning("关闭辅助窗口失败: {Error}", ex.Message);
                }
            }
        }

        private void KillGameProcess(MirGameInstanceModel gameInstance)
        {
            if (gameInstance.MirPid != 0)
            {
                var account = gameInstance.AccountInfo;
                Process process = null;

                try
                {
                    // 先尝试获取进程
                    try
                    {
                        process = Process.GetProcessById(gameInstance.MirPid);
                    }
                    catch (ArgumentException)
                    {
                        // 进程已经不存在了，直接清理资源
                        gameInstance.GameWarning("游戏进程已不存在，PID: {ProcessId}", gameInstance.MirPid);
                    }

                    beforeClose(gameInstance);
                }
                catch (Exception ex)
                {
                    gameInstance.GameError("关闭游戏资源失败: {Error}", ex.Message);
                }
                finally
                {
                    gameInstance.Clear();
                    if (process != null && !process.HasExited)
                    {
                        try
                        {
                            process.Kill();
                        }
                        catch (Exception ex)
                        {
                            gameInstance.GameError("强制结束进程失败: {Error}", ex.Message);
                        }
                    }
                    gameInstance.GameInfo("游戏进程已关闭，账号: {Account}", account.Account);
                }
            }
        }

        async private void RestartGameProcess(MirGameInstanceModel gameInstance)
        {
            if (gameInstance.isRestarting)
            {
                return;
            }
            gameInstance.isRestarting = true;
            gameInstance.GameInfo("重启游戏进程，账号: {Account}", gameInstance.AccountInfo.Account);
            KillGameProcess(gameInstance);
            var isSuccess = await StartGameProcess(gameInstance);
            gameInstance.isRestarting = false;
            if(!isSuccess){
                gameInstance.GameWarning("重启游戏进程失败，账号: {Account}", gameInstance.AccountInfo.Account);
                RestartGameProcess(gameInstance);
            }
        }

        private async Task<bool> AttachToGameProcess(Process process, GameAccountModel account)
        {
            try
            {
                if (process.ProcessName == "ZC.H")
                {
                    var pid = process.Id;
                    var hwnd = process.MainWindowHandle;

                    var gameInstance = GameState.GameInstances.First(o => o.AccountInfo.Account == account.Account);
                    gameInstance.GameInfo("准备绑定游戏进程，账号: {Account}, PID: {ProcessId}", account.Account, pid);

                    if (!GameState.GameInstances.Any(o => o.MirPid == pid))
                    {
                        var rect = WindowUtils.GetClientRect(hwnd);
                        gameInstance.AssistantForm = new AssistantForm(gameInstance, account.Account, account.CharacterName);
                        gameInstance.MirHwnd = hwnd;
                        gameInstance.MirPid = pid;
                        gameInstance.MirBaseAddress = process.MainModule!.BaseAddress;
                        gameInstance.mirVer = process.MainModule?.FileVersionInfo?.FileVersion;
                        gameInstance.MirThreadId = (uint)process.Threads[0].Id;
                        gameInstance.memoryUtils = new MemoryUtils(gameInstance);

                        gameInstance.GameDebug("加载DLL到游戏进程");
                        DllInject.loadDll(gameInstance);
                        // 不知道加载多久 随便写个
                        await Task.Delay(100);
                        // todo 挪走到外面
                        nint[] data = MemoryUtils.PackStringsToData(gameInstance.AccountInfo.Account, gameInstance.AccountInfo.Password);
                        // auto login 
                        SendMirCall.Send(gameInstance, 9003, data);


                        // TODO 会导致不刷新 , 需要重新搞个不依赖tab的
                        gameInstance.AssistantForm.Location = new Point(rect.Left, rect.Top);
                        // 如果是主控，显示辅助窗口
                        if (account.IsMainControl)
                        {
                            gameInstance.AssistantForm.Show();
                            gameInstance.GameInfo("辅助窗口已显示，账号: {Account}", account.Account);
                        }

                        gameInstance.AssistantForm.Disposed += (sender, args) =>
                        {
                            gameInstance.GameDebug("辅助窗口已关闭，移除游戏实例，PID: {ProcessId}", gameInstance.MirPid);
                            gameInstance.Clear();
                        };

                        // 添加进程退出事件监听
                        process.EnableRaisingEvents = true;
                        process.Exited += (sender, e) =>
                        {
                            try
                            {
                                var gameInstance = GameState.GameInstances.First(o => o.MirPid == process.Id);
                                if (gameInstance != null)
                                {
                                    beforeClose(gameInstance);
                                    gameInstance.Clear();
                                }
                            }
                            catch
                            {
                                gameInstance.Clear();
                            }
                        };

                        await Task.Delay(8000);
                        // todo 挪走到外面
                        SendMirCall.Send(gameInstance!, 9099, new nint[] { });
                        await Task.Delay(6000);
                        SendMirCall.Send(gameInstance!, 9100, new nint[] { });
                        await Task.Delay(3000);
                        return gameInstance.CharacterStatus.CurrentHP > 0;
                    }else{
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                var gameInstance = GameState.GameInstances.First(o => o.AccountInfo.Account == account.Account);
                gameInstance.GameError("绑定游戏进程失败: {Error}", ex.Message);
                return false;
                // MessageBox.Show($"绑定游戏进程失败: {ex.Message}");
            }
        }

        protected override void WndProc(ref Message m)//监视Windows消息
        {
            const int WM_HOTKEY = 0x0312; //如果m.Msg的值为0x0312那么表示用户按下了热键 
            switch (m.Msg)
            {
                case WM_HOTKEY:
                    Log.Debug("接收到热键消息");
                    HotKeyUtils.UnregisterHotKey(Handle, 200);
                    SendKeys.Send("{DEL}");
                    HotKeyUtils.RegisterHotKey(Handle, 200, 0, Keys.Delete); // 注册热键
                    var hwnd = WindowUtils.GetForegroundWindow();
                    var pid = WindowUtils.GetProcessId(hwnd);
                    var process = Process.GetProcessById(pid);

                    if (process.ProcessName == "ZC.H")
                    {
                        if (GameState.GameInstances.Any(o => o.MirPid == pid))
                        {
                            if (GameState.GameInstances.First(o => o.MirPid == pid).AssistantForm.Visible)
                            {
                                GameState.GameInstances.First(o => o.MirPid == pid).AssistantForm.Hide();
                            }
                            else
                            {
                                GameState.GameInstances.First(o => o.MirPid == pid).AssistantForm.Show();
                            }
                        }
                        else
                        {
                            // 通过窗口标题匹配账号
                            var title = process.MainWindowTitle;
                            var account = accountList.FirstOrDefault(a => title.Contains($"<{a.CharacterName}>"));
                            if (account != null)
                            {
                                AttachToGameProcess(process, account);
                            }
                        }
                    }
                    else if (process.ProcessName == currentProcessName)
                    {
                        GameState.GameInstances.FirstOrDefault(o => o.AssistantForm?.Handle == hwnd)?.AssistantForm!.Hide();
                    }
                    break;
            }
            base.WndProc(ref m); //将系统消息传递自父类的WndProc 
        }

        private void dataGridViewAccounts_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // 检查是否点击了"重启"按钮
            if (e.ColumnIndex == colRestart.Index && e.RowIndex >= 0)
            {
                var account = accountList[e.RowIndex];
                RestartGameProcess(GameState.GameInstances.FirstOrDefault(g => g.AccountInfo?.Account == account.Account)!);
            }
        }

        private void KillAllGameProcess()
        {
            // 先杀死所有ZC.H进程
            foreach (var process in System.Diagnostics.Process.GetProcessesByName("ZC.H"))
            {
                try
                {
                    process.Kill();
                    process.WaitForExit();
                }
                catch (System.InvalidOperationException) { }
            }
        }

        private async void btnRestartAll_Click(object sender, EventArgs e)
        {
            Log.Information("重启所有游戏进程，账号数量: {AccountCount}", accountList.Count);


            foreach (var account in accountList)
            {
                RestartGameProcess(GameState.GameInstances.FirstOrDefault(g => g.AccountInfo?.Account == account.Account)!);
                await Task.Delay( Environment.ProcessorCount <=4 ?  10_000 : 15000);
                }
             
                // sllep 
                // await Task.Delay(20_000);
             
        }

        private void btnCloseAll_Click(object sender, EventArgs e)
        {
                    //   先杀死所有ZC.H进程 有bug, 会导致丢失
            KillAllGameProcess();
            // Empty implementation for closing all instances
        }

        private async void btnRestartTask_Click(object sender, EventArgs e)
        {
            Log.Information("重启所有游戏任务");
            autoForeGround();
        }
        // private async Task BuyLZ(MirGameInstanceModel instanceValue, CancellationToken _cancellationToken)
        // {
        //     // 没买蜡烛先买, 背包蜡烛小于5
        //     if (instanceValue.Items.Where(o => o.Name == "蜡烛").Count() < 3)
        //     {
        //         bool pathFound = await GoRunFunction.PerformPathfinding(_cancellationToken, instanceValue!, 640, 613, "0", 6);
        //         if (pathFound)
        //         {
        //             await NpcFunction.ClickNPC(instanceValue!, "陈家铺老板");
        //             await NpcFunction.BuyLZ(instanceValue!, "蜡烛", 3);
        //         }
        //     }
        // }

     
        private async Task buyDrugs(MirGameInstanceModel instanceValue, CancellationToken _cancellationToken)
        {
            // 目前只有道士买魔法, 
            // TODO法师
            if (instanceValue.AccountInfo.role == RoleType.taoist && instanceValue.CharacterStatus!.Level > 6)
            {
                var items = GameConstants.Items.MegaPotions;
                var exitsQuan = 0;
                items.ForEach(item =>
                {
                    var bags = GoRunFunction.findIdxInAllItems(instanceValue!, item);
                    if (bags != null)
                    {
                        exitsQuan += bags.Length;
                    }
                });

                if (exitsQuan < GameConstants.Items.megaBuyCount)
                {
                    await NpcFunction.BuyDrugs(instanceValue!, GameConstants.Items.MegaPotions[0], GameConstants.Items.megaBuyCount - exitsQuan);
                }
            }
            else
            {
                // 其他就预备血6
                var items2 = GameConstants.Items.HealPotions;
                var exitsQuan2= 0;
                items2.ForEach(item =>
                    {
                        var bags = GoRunFunction.findIdxInAllItems(instanceValue!, item);
                        if (bags != null)
                        {
                            exitsQuan2 += bags.Length;
                        }
                    });
                if (exitsQuan2 < GameConstants.Items.healBuyCount)
                {
                    await NpcFunction.BuyDrugs(instanceValue!, GameConstants.Items.HealPotions[0], GameConstants.Items.healBuyCount - exitsQuan2);
                }
            }
           
        }
        private async Task sellMeat(MirGameInstanceModel instanceValue, CancellationToken _cancellationToken)
        {
            ItemFunction.ReadBag(instanceValue);

            var meats = instanceValue.Items.Where(o => o.Name == "肉");
            var chickens = instanceValue.Items.Where(o => o.Name == "鸡肉");
            var keepOneMeat = instanceValue.CharacterStatus!.Level < 7;
            var expressMeats = keepOneMeat ? meats.Skip(1).ToList() : meats.ToList();
            var expressChickens = keepOneMeat ? chickens.Skip(1).ToList() : chickens.ToList();
            var allMeats = expressMeats.Concat(expressChickens).ToList();
            if (allMeats.Count > 0)
            {
                Log.Information("卖肉");
                // 屠夫 647 595 // todo 屠夫记录NPC
                await findMeatNpc(instanceValue, _cancellationToken);
                foreach (var meat in allMeats)
                {
                    // 卖肉 TODO 抽象方法
                    nint[] data = StringUtils.GenerateCompactStringData(meat.Name);
                    Array.Resize(ref data, data.Length + 1);
                    data[data.Length - 1] = meat.Id;
                    SendMirCall.Send(instanceValue!, 3011, data);
                    await Task.Delay(500);
                }

                await Task.Delay(500);
                await NpcFunction.RefreshPackages(instanceValue);
            }

        }
        private async Task prepareBags(MirGameInstanceModel instanceValue, CancellationToken _cancellationToken)
        {
            var CharacterStatus = instanceValue.CharacterStatus!;
            // 要先回家, 如果不在的话, 
            // TODO 用地牢飞, 先走

            var nearHome = NpcFunction.PickNearHomeMap(instanceValue);
            if (nearHome != CharacterStatus.MapId)
            {
                var (npcMap, npcName, x, y) = NpcFunction.PickDrugNpcByMap(instanceValue, nearHome);
                // 先随便选个NPC位置
                await GoRunFunction.PerformPathfinding(_cancellationToken, instanceValue!, x, y, npcMap, 6);
            }
            // 卖肉
            await sellMeat(instanceValue, _cancellationToken);
            // 蜡烛检测
            await NpcFunction.BuyLZ(instanceValue, _cancellationToken);
            // 确保到家了, 到家先把极品穿起来, 这样不会被购买替换了, 然后被维修
            // trigger takeon 
            await NpcFunction.autoReplaceEquipment(instanceValue, false);
            // 买所有
            // 卖所有
            await NpcFunction.sellLJEquipment(instanceValue, _cancellationToken);
            // 买了再修
            await NpcFunction.buyAllEquipment(instanceValue, _cancellationToken);
            // 重复修 保证没被换
            // 修所有
            await NpcFunction.RepairAllEquipment(instanceValue, _cancellationToken);
            // 保留装可能还在, 所以修背包内的
            await NpcFunction.RepairAllBagsEquipment(instanceValue, _cancellationToken);
            // 修所有
            await NpcFunction.RepairAllEquipment(instanceValue, _cancellationToken);
            // save, 该带的极品已经带了, 可以存了, 书记也存
            await NpcFunction.SaveItem(instanceValue, "远程老板", 0, 0, instanceValue.Items.Where(o => !o.IsEmpty && (o.IsGodly || o.stdMode == 4)).ToArray());
            // 存完, 然后可以卖多余的装备了, 暂时不考虑其他, 书一般先存, 药直接不会捡取多余的
            await buyDrugs(instanceValue, _cancellationToken);
            // 修沪深只有道士
            await NpcFunction.BuyRepairAllFushen(instanceValue, _cancellationToken);

        }

        private static async Task findNoobNpc(MirGameInstanceModel instanceValue, CancellationToken _cancellationToken)
        {
            var CharacterStatus = instanceValue.CharacterStatus!;
            var isLeftAlive = CharacterStatus.X < 400;

            bool pathFound = await GoRunFunction.PerformPathfinding(_cancellationToken, instanceValue!, !isLeftAlive ? 630 : 283, !isLeftAlive ? 603 : 608, "0", 6,true, 10);
            if (pathFound)
            {
                await NpcFunction.ClickNPC(instanceValue!, !isLeftAlive ? "助手小敏" : "助手阿妍");
            }
        }

      
        
         private static async Task findMeatNpc(MirGameInstanceModel instanceValue, CancellationToken _cancellationToken)
        {
            var CharacterStatus = instanceValue.CharacterStatus!;
            var isLeftAlive = CharacterStatus.X < 400;
            bool pathFound = await GoRunFunction.PerformPathfinding(_cancellationToken, instanceValue!, !isLeftAlive ? 647 : 293, !isLeftAlive ? 595 : 604, "0", 6, true, 10);
            if (pathFound)
            {
                await NpcFunction.ClickNPC(instanceValue!, "屠夫");
            }
        }

        private static async Task findWeaponNpc(MirGameInstanceModel instanceValue, CancellationToken _cancellationToken)
        {
            var CharacterStatus = instanceValue.CharacterStatus!;
            var isLeftAlive = CharacterStatus.X < 400;

            bool pathFound = await GoRunFunction.PerformPathfinding(_cancellationToken, instanceValue!, !isLeftAlive ? 635 : 295, !isLeftAlive ? 611 : 613, "0", 6,true, 10);
            if (pathFound)
            {
                await NpcFunction.ClickNPC(instanceValue!, !isLeftAlive ? "精武馆老板" : "边界村铁匠铺");
            }
        }
     
        private async void processTasks()
        {
            var instances = GameState.GameInstances;
            //Log.Information("开始处理任务，实例数量: {Count}", instances.Count);

            instances.ForEach(async instance =>
            {
                if (instance.IsBotRunning)
                {
                    Log.Debug("实例 {Account} 正在运行中，跳过", instance.AccountInfo?.Account);
                    return;
                }
                try
                {
                    instance.IsBotRunning = true;   
                    if (!instance.IsAttached)
                    {
                        //Log.Debug("实例 {Account} 未附加，跳过", instance.AccountInfo?.Account);
                        return;
                    }
                    Log.Information("开始处理实例 {Account} 的任务", instance.AccountInfo?.Account);
                    var CharacterStatus = instance.CharacterStatus!;

                    if (CharacterStatus.CurrentHP > 0)
                    {
                        var instanceValue = instance;
                        var act = instanceValue.AccountInfo;
                        var _cancellationTokenSource = new CancellationTokenSource();
                        if (instanceValue.AccountInfo!.role == RoleType.blade)
                        {
                             CharacterStatusFunction.AdjustAttackSpeed(instanceValue, 1100);
                        }
                        // 只有城中才初始准备, 或者旁边有NPC说明是城里, 但是要排除掉一些特殊的野外NPC 再说, 还有个思路 可以看是不是战斗地图
                        if (new string[] { "0", "1", "2", "3" }.Contains(CharacterStatus.MapId) || instanceValue.Monsters.FirstOrDefault(o => o.Value.TypeStr == "NPC").Value != null)
                            await prepareBags(instanceValue, _cancellationTokenSource.Token);
                        // 新手任务
                        // todo 目前是5
                        // if (CharacterStatus.Level <= 8 && act.TaskMain0Step < 6)
                        // {
                        //     // 主线
                        //     if (act.TaskMain0Step == 0)
                        //     {
                        //         // 主线4级
                        //         // click 助手小敏 630 603
                        //         // @QUEST
                        //         // 没有 屠夫正在找
                        //         // 有
                        //         // npc给你的任务 <做/@QUEST1_1_1> 
                        //         // npc给你的任务 <不做/@QUEST1_1_2>

                        //         await findNoobNpc(instanceValue, _cancellationTokenSource.Token);
                        //         // todo check cmd
                        //         await NpcFunction.Talk2(instanceValue!, "@QUEST");
                        //         await NpcFunction.Talk2(instanceValue!, "@QUEST1_1_1");
                        //         act.TaskMain0Step = 1;
                        //         // todo json
                        //         SaveAccountList();
                        //     }

                        //     if (act.TaskMain0Step == 1)
                        //     {
                        //         // 主线4级 
                        //         // click 屠夫 
                        //         // <新手任务对话/@QUEST> 
                        //         // <继续/@QUEST1_1_1>
                        //         await findMeatNpc(instanceValue, _cancellationTokenSource.Token);
                        //         await NpcFunction.Talk2(instanceValue!, "@QUEST");
                        //         await NpcFunction.Talk2(instanceValue!, "@QUEST1_1_1");

                        //         await GoRunFunction.NormalAttackPoints(instanceValue, _cancellationTokenSource.Token, false, (instanceValue) =>
                        //         {
                        //             // 检查背包的肉
                        //             var meat = instanceValue.Items.Where(o => o.Name == "肉").FirstOrDefault();
                        //             return meat != null;
                        //         });

                        //         act.TaskMain0Step = 2;
                        //         SaveAccountList();
                        //     }
                        //     if (act.TaskMain0Step == 2)
                        //     {
                        //         // 找肉

                        //         // 主线4级 
                        //         // click 屠夫 
                        //         // <新手任务对话/@QUEST> 
                        //         // <继续/@QUEST1_1_1>
                        //         await findMeatNpc(instanceValue, _cancellationTokenSource.Token);
                        //         await NpcFunction.Talk2(instanceValue!, "@QUEST");
                        //         await NpcFunction.Talk2(instanceValue!, "@QUEST1_2_1");

                        //         // await NormalAttackPoints(instanceValue, _cancellationTokenSource, [(625, 580), (625, 560)], (instanceValue) =>
                        //         // {
                        //         //     // 检查背包的肉
                        //         //     var meat = instanceValue.Items.Where(o => o.Name == "肉").FirstOrDefault();
                        //         //     return meat != null;
                        //         // });

                        //         act.TaskMain0Step = 3;
                        //         SaveAccountList();
                        //     }
                        //     if (act.TaskMain0Step == 3)
                        //     {
                        //         await findNoobNpc(instanceValue, _cancellationTokenSource.Token);
                        //         await NpcFunction.Talk2(instanceValue!, "@QUEST");
                        //         // todo 保存json
                        //         act.TaskMain0Step = 4;
                        //         SaveAccountList();
                        //     }
                        //     if (act.TaskMain0Step == 4)
                        //     {
                        //         // 升级到5
                        //         // 抽象到巡逻, 然后能退出
                        //         await GoRunFunction.NormalAttackPoints(instanceValue, _cancellationTokenSource.Token, false, (instanceValue) =>
                        //         {
                        //             return instanceValue.CharacterStatus!.Level >= 5;
                        //         });
                        //         act.TaskMain0Step = 5;
                        //         SaveAccountList();
                        //     }
                        //     if (act.TaskMain0Step == 5)
                        //     {

                        //         await prepareBags(instanceValue, _cancellationTokenSource.Token);
                        //         await findNoobNpc(instanceValue, _cancellationTokenSource.Token);
                        //         await NpcFunction.Talk2(instanceValue!, "@QUEST");
                        //         await NpcFunction.Talk2(instanceValue!, "@QUEST1_1_1");
                        //         // 精武馆老板
                        //         await findWeaponNpc(instanceValue, _cancellationTokenSource.Token);
                        //         await NpcFunction.Talk2(instanceValue!, "@QUEST");
                        //         await NpcFunction.Talk2(instanceValue!, "@exit");
                        //         await findWeaponNpc(instanceValue, _cancellationTokenSource.Token);
                        //         await NpcFunction.Talk2(instanceValue!, "@QUEST");
                        //         await NpcFunction.Talk2(instanceValue!, "@exit");
                        //         // 回复会得到乌木剑, 会自动带
                        //         // 再次找助手 会让你接着走 武士之家
                        //         await findNoobNpc(instanceValue, _cancellationTokenSource.Token);
                        //         await NpcFunction.Talk2(instanceValue!, "@QUEST");
                        //         // <下一页/@Q705_1_1_1>
                        //         await NpcFunction.Talk2(instanceValue!, "@Q705_1_1_1");
                        //         await NpcFunction.Talk2(instanceValue!, "@exit");
                        //         // 查询所有号的等级>=5, 再出去
                        //         // 逛街
                        //         // await GoRunFunction.NormalAttackPoints(instanceValue, _cancellationTokenSource.Token, patrolPairs, (instanceValue) =>
                        //         // {
                        //         //     var instances = GameState.GameInstances.ToList();
                        //         //     var allLevel5 = instances.Where(o => o.Value.CharacterStatus!.Level < 5).ToList();
                        //         //     if (allLevel5.Count == 0)
                        //         //     {
                        //         //         return true;
                        //         //     }
                        //         //     return false;
                        //         // });
                        //         act.TaskMain0Step = 6;
                        //         SaveAccountList();

                        //     }
                        // }
                        // 支线8级, 没啥调用 先不要了
                        // if (CharacterStatus.Level >= 999 && act.TaskSub0Step < 99)
                        // {
                        //     // 1、7级前从新手村助手阿妍处接介绍信任务
                        //     // 2、给屠夫1块鹿肉、1块鸡肉（得到太阳水1瓶）
                        //     // 3、得到佛牌，然后找书店老板（15级前完成）
                        //     // 4、给边界村书店或银杏村药店5块12以上肉和5块5以上的鸡肉，对话选择“助人为快乐之本,我不能要”可得祈福项链。
                        //     // 5、祈福项链拿给比奇黄飞龙可升到10级。
                        //     if (act.TaskSub0Step == 0)
                        //     {
                        //         // click 助手阿妍 630 603
                        //         await findNoobNpc(instanceValue, _cancellationTokenSource.Token);
                        //         await NpcFunction.Talk2(instanceValue!, "@next");
                        //         await NpcFunction.Talk2(instanceValue!, "@next1");
                        //         await NpcFunction.Talk2(instanceValue!, "@new01");
                        //         act.TaskSub0Step = 1;
                        //         SaveAccountList();
                        //     }
                        //     // 2 屠夫
                        //     if (act.TaskSub0Step == 1)
                        //     {
                        //         // click 屠夫 647 595
                        //         await findMeatNpc(instanceValue, _cancellationTokenSource.Token);
                        //         await NpcFunction.Talk2(instanceValue!, "@main1");
                        //         act.TaskSub0Step = 2;
                        //         SaveAccountList();
                        //     }
                        //     if (act.TaskSub0Step == 2)
                        //     {
                        //         // 再次开始找肉和鸡肉
                        //         await GoRunFunction.NormalAttackPoints(instanceValue, _cancellationTokenSource.Token, false, (instanceValue) =>
                        //         {
                        //             var meats = instanceValue.Items.Where(o => o.Name == "肉").ToList();
                        //             var chickens = instanceValue.Items.Where(o => o.Name == "鸡肉").ToList();
                        //             return meats.Count > 0 && chickens.Count > 0;
                        //         });
                        //         act.TaskSub0Step = 3;
                        //         SaveAccountList();
                        //     }
                        //     if (act.TaskSub0Step == 3)
                        //     {
                        //         // 回屠夫给肉他
                        //         await findMeatNpc(instanceValue, _cancellationTokenSource.Token);
                        //         await NpcFunction.Talk2(instanceValue!, "@main1");
                        //         // <确定/@newnew1_1>
                        //         await NpcFunction.Talk2(instanceValue!, "@newnew1_1");
                        //         // <真的吗？太好了！/@job>
                        //         await NpcFunction.Talk2(instanceValue!, "@job");
                        //         act.TaskSub0Step = 4;
                        //         SaveAccountList();
                        //     }
                        //     // 一点经验没啥用
                        //     // if (act.TaskSub0Step == 4)
                        //     // {
                        //     //     // 去银杏村药店 药剂师
                        //     //     bool pathFound = await GoRunFunction.PerformPathfinding(_cancellationTokenSource.Token, instanceValue!, 9, 13, "", 6);
                        //     //     if (pathFound)
                        //     //     {
                        //     //         await NpcFunction.ClickNPC(instanceValue!, "药剂师");
                        //     //         // 介绍信任务/@news
                        //     //         await NpcFunction.Talk2(instanceValue!, "@news");
                        //     //         // < 可以 / @new2_21 >,
                        //     //         await NpcFunction.Talk2(instanceValue!, "@new2_21");
                        //     //         // <接受/@new2_211>
                        //     //         await NpcFunction.Talk2(instanceValue!, "@new2_211");
                        //     //     }
                        //     //     act.TaskSub0Step = 5;
                        //     //     SaveAccountList();
                        //     // }
                        //     //  if (act.TaskSub0Step == 5)
                        //     // {
                        //     //     // 继续找肉 5 5
                        //     //     await GoRunFunction.NormalAttackPoints(instanceValue, _cancellationTokenSource.Token, patrolPairs, (instanceValue) =>
                        //     //     {
                        //     //         var meats = instanceValue.Items.Where(o => o.Name == "肉").ToList();
                        //     //         var chickens = instanceValue.Items.Where(o => o.Name == "鸡肉").ToList();
                        //     //         return meats.Count > 4 && chickens.Count > 4;
                        //     //     });
                        //     //     act.TaskSub0Step = 6;
                        //     //     SaveAccountList();
                        //     // }
                        // }
                        // 主线
                        // 去道士武士魔法之家, todo 跨地图寻路
                        // 道士 544, 560
                        // 武士 107, 316
                        // 魔法 314, 474
                        if (CharacterStatus.Level >= 1)
                        {
                            // todo 跨地图
                            // bool pathFound = await GoRunFunction.PerformPathfinding(_cancellationTokenSource.Token, instanceValue!, 282, 636, "", 10);
                            // if (pathFound)
                            // {
                            // }

                            // 目前死循环
                            while (true)
                            {
                                // 挑选挂机点, 
                                // 13级以前比起
                                // >=13 自然骷髅
                                // >=20 毒蛇矿
                                var hangMapId = "0";
                                // 如果有朋友等级太低, 先到5
                                // 检查朋友的等级
                                var friends = GameState.GameInstances.Where(o => o.IsAttached && o.CharacterStatus!.Level > 0 && o.CharacterStatus!.Level < 4).ToList();
                                if (friends.Count > 0)
                                {
                                    hangMapId = "0";
                                }
                                else  if (CharacterStatus.Level >= 22)
                                {
                                    hangMapId = "D421";
                                }
                                else if (CharacterStatus.Level >= 13)
                                {
                                    hangMapId = "D002";
                                }
                                instanceValue.isHomePreparing = false;
                                await GoRunFunction.NormalAttackPoints(instanceValue, _cancellationTokenSource.Token, false, (instanceValue) =>
                                {
                                    // 小号跟随回家
                                    if (!instanceValue.AccountInfo.IsMainControl && instances[0].isHomePreparing)
                                    {
                                        return false;
                                    }
                                    // go home
                                    // 排除药品, 
                                    // todo 扔掉红
                                    var isConsumer = GoRunFunction.whoIsConsumer(instanceValue!);
                                    var miscs = instanceValue.Items.Where(o => !o.IsEmpty).ToList();
                                    var realLowEq = false;
                                    if (isConsumer == 2)
                                    {
                                        var specificItems = new List<EquipPosition>() { EquipPosition.Weapon, EquipPosition.Dress };
                                        foreach (var item in specificItems)
                                        {
                                            var useItem = instanceValue.CharacterStatus.useItems[(int)item];
                                            // 手上不管是JP还是普通, 只要包里还有就可以
                                            if (useItem.IsEmpty || useItem.IsLowDurability)
                                            {
                                                // check 包里有没有就回家, 趁着还没爆, 当然也有可能已经爆了
                                                var replacement = NpcFunction.checkReplacementInBag(instanceValue, item, false);
                                                if (replacement == null)
                                                {
                                                    Log.Debug("没替换装备了装备{Name}低耐久, 需要替换, 回家", useItem.Name);
                                                    realLowEq = true;
                                                    break;
                                                }
                                            }
                                        }
                                        // 其他部分空不要紧, 但是不能爆极品 策略是不一样的的, 而且其他也不容易磨损
                                        var otherItems = new List<EquipPosition>() { EquipPosition.Helmet, EquipPosition.RingLeft, EquipPosition.RingRight,
                                        EquipPosition.Necklace, EquipPosition.ArmRingLeft, EquipPosition.ArmRingRight };
                                        foreach (var item in otherItems)
                                        {
                                            var useItem = instanceValue.CharacterStatus.useItems[(int)item];
                                            if (useItem.IsGodly && useItem.IsLowDurability)
                                            {
                                                var replacement = NpcFunction.checkReplacementInBag(instanceValue, item, false);
                                                if (replacement == null)
                                                {
                                                    Log.Debug("杂货极品{Name}没替换装备了低耐久, 需要替换, 回家", useItem.Name);
                                                    realLowEq = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    // 7级以下不配
                                    var isLowHpMP = instanceValue.AccountInfo.role == RoleType.taoist
                                    && (isConsumer == 2)
                                    && (instanceValue.CharacterStatus.CurrentHP < instanceValue.CharacterStatus.MaxHP * 0.3
                                    && instanceValue.CharacterStatus.CurrentMP < instanceValue.CharacterStatus.MaxMP * 0.2);
                                    var isLowFushen = false;
                                    if (instanceValue.AccountInfo.role == RoleType.taoist)
                                    {
                                        // 总计只剩50 跑路足够了
                                        var usedFushen = instanceValue.CharacterStatus.useItems.Where(o => !o.IsEmpty && o.stdMode == 25 && o.Name == "护身符").ToList();
                                        var items = instanceValue.Items.Where(o => !o.IsEmpty && o.stdMode == 25 && o.Name == "护身符").ToList();
                                        var allFushen = usedFushen.Concat(items).Sum(o => o.Duration);
                                        if (allFushen < 50)
                                        {
                                            isLowFushen = true;
                                        }
                                    }
                                    var final = miscs.Count > 38 || realLowEq || isLowHpMP || isLowFushen;
                                    return final;
                                }, hangMapId);
                                // 考虑到可能手上没东西了, 先强制把low极品穿上, 跑路回家
                                await NpcFunction.autoReplaceEquipment(instanceValue, false);
                                instanceValue.isHomePreparing = true;
                                await prepareBags(instanceValue, _cancellationTokenSource.Token);
                            }
                            // act.TaskSub0Step = 6;
                            // SaveAccountList();

                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "处理任务出错，账号: {Account}", instance.AccountInfo?.Account);
                }
                finally
                {
                    Log.Information("完成处理实例 {Account} 的任务", instance.AccountInfo?.Account);
                    await Task.Delay(10_000);
                    instance.IsBotRunning = false;
                    processTasks();
                }
            });
        }
        // TODO behavior tree for the main level up loop
        private async void autoForeGround(){
            // while(true){
            //     // 先写2个新手任务
            //     // 避免太频繁 先写1000
            //     await Task.Delay(1000);
            // }
            // 先全写一起了不然没法写了
            processTasks();
        }

        private async void autoAtBackground()
        {
            while (true)
            {
                try 
                {
                    Log.Debug("开始后台自动处理");
                    await Task.Delay(20_000);
                    var instances = GameState.GameInstances;

                    instances.ForEach(async instance =>
                    {
                        try
                        {
                            if (!instance.IsAttached)
                            {
                                //Log.Debug("实例 {Account} 未附加，跳过后台处理", instance.AccountInfo?.Account);
                                return;
                            }
                            Log.Debug("刷新实例 {Account} 状态", instance.AccountInfo?.Account);
                            instance.RefreshAll();
                            // todo ref方法 避免重复调用
                            var CharacterStatus = instance.CharacterStatus;
                            // 死亡 判断有没怪物
                            if (CharacterStatus.CurrentHP <= 0 && instance.Monsters.Count > 0)
                            {
                                // 复活 重启
                                // 尝试小退
                                await GoRunFunction.RestartByToSelectScene(instance);
                                await Task.Delay(2000);
                                CharacterStatusFunction.GetInfo(instance);
                                // check hp -- 其实还不起作用
                                if (CharacterStatus.CurrentHP == 0)
                                {
                                   RestartGameProcess(instance);
                                }
                                return;
                            }
                            if (CharacterStatus.CurrentHP > 0)
                            {
                                if (CharacterStatus.CurrentHP == instance.lastHP)
                                {
                                    instance.sameHPtimes++;
                                    if (instance.sameHPtimes > 30)
                                    {
                                        // 掉线
                                        RestartGameProcess(instance);
                                        return;
                                    }
                                }
                                else
                                {
                                    instance.sameHPtimes = 0;
                                }
                                instance.lastHP = CharacterStatus.CurrentHP;

                                // 组队
                                if (CharacterStatus.groupMemCount < GameState.GameInstances.Count)
                                {
                                    if (instance.AccountInfo.IsMainControl)
                                    {
                                        // GameInstances 除了自己
                                        var members = GameState.GameInstances.Where(o => o.IsAttached && o.MirPid != instance.MirPid).Select(o => o.AccountInfo.CharacterName).ToList();
                                        foreach (var member in members)
                                        {
                                            nint[] data = StringUtils.GenerateCompactStringData(member);
                                            SendMirCall.Send(instance, 9004, data);
                                            await Task.Delay(300);
                                        }
                                    }
                                    else
                                    {
                                        if (!instance.CharacterStatus.allowGroup)
                                        {
                                            SendMirCall.Send(instance, 9005, new nint[] { 1 });
                                        }
                                    }
                                }
                                await NpcFunction.autoReplaceEquipment(instance);
                                await GoRunFunction.TryAliveRecallMob(instance);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "后台处理出错，账号: {Account}", instance.AccountInfo?.Account);
                            await Task.Delay(3000);
                            //autoAtBackground();
                            //return;
                        }
                    });
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "后台处理主循环出错");
                    await Task.Delay(3000);
                }
            }
        }
        
          private async void autoAtBackgroundFast(){
            // 其他中断并行需要考虑 
            while (true)
            {
        
                var instances = GameState.GameInstances;
                instances.ForEach(instance =>
                {
                    try 
                    {
                        if (!instance.IsAttached)
                        {
                            return;
                        }
                        // Log.Debug("快速刷新实例 {Account} 状态", instance.AccountInfo?.Account);
                        instance.RefreshAll();

                        if (instance.CharacterStatus.CurrentHP > 0)
                        {
                            GoRunFunction.TryEatDrug(instance);
                            GoRunFunction.TryHealPeople(instance);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "快速后台处理出错，账号: {Account}", instance.AccountInfo?.Account);
                    }
                });
                await Task.Delay(300);
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Log.Information("应用程序正在关闭");

            // 注销热键
            HotKeyUtils.UnregisterHotKey(Handle, 200);

            // 同步关闭所有资源
            foreach (var gameInstance in GameState.GameInstances)
            {
                beforeClose(gameInstance);
            }
            GameState.GameInstances.Clear();

            Log.Information("应用程序已关闭");
            Log.CloseAndFlush();

            // 强制退出进程
            Process.GetCurrentProcess().Kill();
        }

        private void btnAttachAll_Click(object sender, EventArgs e)
        {
            // 获取所有运行中的游戏进程
            foreach (var process in Process.GetProcessesByName("ZC.H"))
            {
                try
                {
                    // 通过窗口标题匹配账号
                    var title = process.MainWindowTitle;
                    var account = accountList.FirstOrDefault(a => title.Contains($"<{a.CharacterName}>"));
                    if (account != null)
                    {
                        AttachToGameProcess(process, account);
                    }

                }
                catch (Exception ex)
                {
                    Log.Error(ex, "附加到进程失败，PID: {ProcessId}", process.Id);
                }
            }
        }
    }
}
