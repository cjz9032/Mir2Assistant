using Mir2Assistant.Common.Functions;
using Mir2Assistant.Common.Models;
using Mir2Assistant.Common.Utils;
using Serilog; // 新增Serilog引用
using Serilog.Sinks.Debug; // 添加Debug sink引用
using System.Diagnostics;
using System.Text.Json;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
using Mir2Assistant.Common;

namespace Mir2Assistant
{
    public partial class MainForm : Form
    {
        private string currentProcessName = Process.GetCurrentProcess().ProcessName;
        private List<GameAccountModel> accountList = new List<GameAccountModel>();
        private string configFilePath = Path.Combine(Directory.GetCurrentDirectory(), "accounts.json");
        private string gamePath = "ZC.H"; // 游戏路径，需要配置
        private string gameDirectory = @"G:\cq\cd"; // 游戏所在目录
        //private string encodeArg = @"CqJHmIicTR62d+9dSOk58cTKgI7HznPbN6DJTvZprSAELnz5oDnMgNt5wBjuFk1qAxqanu//vSGjdVAYZn1QVPj5eBNHqu8r4kAsaof5+vscS8Zg1/EMOLMUwqpHS2YUm5UOhb889exVRhd96hTJFIy2GNszQaAk4ncba891PYNsKBxPjF9PeD+eX8L/GJU6ZIHi/GdjTYUxiBMd20cBkivf1tc3SbFSLabzUQjMLFYEX0f3dTqz8T3pAGRR5eGdIC5UiEbcGJowco1ftRbqqTIKMqyHUk3ie7SiX0uDpRK3DjxXN2dVyAqfucJ6HiftllA5LrVZb77XUr4JOt631s2Ku6zEgZhYDYRH9Kip8qxiRMPXSzzNfaP5gCaNQfxPfePxDlcHectMqT+XV2LzEkJWEEEnF79SyHKT6Uiz99UeHtZ11MDST5NVTwie/bOnFyu2CT7xScxoWT+yyuw3d7tNGAkt1fqlTtXTlc2/BT5ps9phS154s8TsyjcWNDoXWhXPgrknoFSJVhtQbl0qxBUvXFVYE2wDh6D+pe5kBMLOF8PRc82m4PcpGy+vimoRVJCkGamn757CLu4Bg2G4sda4RxQmk2dtPSu17irMsc/Cxmebkv3W76xPKdUeNgFYF+oWJDCkwfj7iFvzolHb1KaSuCHRnOb6O+mvW51BGB5tkxKzrXt2XIcNr1DWwQrkZT3HKQsyY1vILGR0U2xfE+Z9wQCVwwCOsERTh9RxZt2Iht6vKWV0DDn2TWfVevPPl5Du3QU+Y7lp3WmIe+GwLZjueViLnJIL46EmpMKXA1/s+zSTW9s1No5eSoVoRmYN6FH7Wds1xvMSn2JovV3OE7zo+DMLC69xLopScUHBKVeQQOLHVkLvqVepYGdQ0fg3tc17vxphP046OdSzdxxJ1OB8xNtK+dgH9nxaYBPZOCWb7Wutz8kWJaO8lziTLMHD6jfR77lkt2HmO0ENJb36REyfeWaCQ5cnlLnlhSy9MK5HH3P95dreEORsNZnNWBYN";
        private string encodeArg = @"HeUVbIiNKoqdx3Zd1P9/K6FKxF5LcquVlUFQp5UVWzCvmS0DqoZ+ntxDNFDM+ZMn9sSfNmZCDBxL1olW7iv2WcrVaSANQBXI5kOSz2yaQK2jnDbXqWDKtMSj45/rQd6KQh0bbpITYwgO8kCMXp8zurfBYKYf8TSsOemSc4Dwrup5bmCG/81Ai3HUNN8aj7Rxr46sGB2xqQRTaN8H/+tKZPLUMVYj4M0E7fMCRB3SM8uVuUuQ0KVEiF1wi9cleqxvHCQ4eOmmxgJYRderur3esPNpBV36NxJhl98eLuFcfCOsgQGshTstqeTWPezkX68EKnP/JSQd4r536vLh9vI8fY56tbF3ErtSRdSi7RVDdhnwllrrG2QTrh6ILOxNo4yrfdpc3InYQfAVD70iqFcN5L6goMW/NoGw9oo8GTIMy5pVzhUlp5+TxI6C9I5+mm+2F15rGw07jgav5+J+G+9wLpibn1jlYqbjWBwDHU4GXSIh40OrKoOkcTsz/tN4EhdFZU2prfu+/UHKC5XK9dUR3wYUVsMEU7JUUizN3r56Xmu+ImZxAD4LKFVbCTJICg17TREsY1Si57JQsyN/JvEgX4yHQMxRZj5/Lx8sQWbziQr/xUvtYRldHX1jP5HBgYafUVYlWKDqxpwKJa6Z9CuAjaHGuSYUC/Lw+TlSiOTOZ3a1UcujkYSkD/vZnlpy1t2cZnU4ag98wACbemBA3hefD/YyawKUhQ/+6obMVFwudmG6Vq1t/k6Nzkuk6bbtSOoGjNusckNzdRjEQgeoiQKeoPmQQPOVCf9LXgAEiL2ew4Z67T+KQc/N0V6iYhxphV1+tmKJX4FUA/pjU5YBXV3cMKq309NXPN87AdoX6OTDXnxVBHTHf1spTD1XC03OF+zObYij1aAeQhZktt3VxkCflzB3WeAtpVCdXvTUvA099cvBNxYBB49EblQxZwTNV+y0SQ97EpagWUKlReeNhrffhco6D41aQyYpYhTb2JnmuSrtJUkhMazBpaAXIvuNNh7v";
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
                GameState.GameInstances.Add(gameInstance);
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
            RefreshDataGrid();
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
                MessageBox.Show($"加载账号列表失败: {ex.Message}");
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
               MessageBox.Show($"保存账号列表失败: {ex.Message}");
            }
        }

        private void RefreshDataGrid()
        {
            Log.Debug("刷新数据网格");
            // 更新进程状态
            foreach (var account in accountList)
            {
                if (account.ProcessId.HasValue)
                {
                    try
                    {
                        Process.GetProcessById(account.ProcessId.Value);
                    }
                    catch
                    {
                        Log.Information("进程 {ProcessId} 已不存在，重置账号 {Account} 的进程ID", account.ProcessId, account.Account);
                        account.ProcessId = null;
                    }
                }
            }

            // 保存当前列顺序和位置
            Dictionary<string, int> columnOrder = new Dictionary<string, int>();
            if (dataGridViewAccounts.Columns.Count > 0)
            {
                foreach (DataGridViewColumn col in dataGridViewAccounts.Columns)
                {
                    columnOrder[col.Name] = col.DisplayIndex;
                }
            }

            dataGridViewAccounts.DataSource = null;
            dataGridViewAccounts.DataSource = new BindingSource { DataSource = accountList };
            
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

        private async void StartGameProcess(GameAccountModel account)
        {
            try
            {
                string arguments = $"{account.encodeArg}";
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
                    Log.Information("PowerShell已直接启动游戏进程，账号: {Account}, PID: {Pid}", account.Account, pid);
                    account.ProcessId = pid;
                    // 通过PID获取进程对象
                    var gameProcess = Process.GetProcessById(pid);
                    // 后续绑定DLL等逻辑
                    await AttachToGameProcess(gameProcess, account);
                }
                else
                {
                    Log.Warning("未能获取到新进程PID，账号: {Account}", account.Account);
                    MessageBox.Show("无法获取新进程PID，请手动启动游戏。");
                }

                RefreshDataGrid();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "启动游戏过程中发生异常，账号: {Account}", account.Account);
                MessageBox.Show($"启动游戏失败: {ex.Message}");
            }
        }

        private void KillGameProcess(GameAccountModel account)
        {
            if (account.ProcessId.HasValue)
            {
                try
                {
                    Log.Information("准备关闭游戏进程，账号: {Account}, PID: {ProcessId}", account.Account, account.ProcessId);
                    Process process = Process.GetProcessById(account.ProcessId.Value);
                    
                    // 如果有关联的辅助窗口，先解除挂钩并关闭
                    if (GameState.GameInstances.Any(o => o.MirPid == account.ProcessId.Value))
                    {
                        Log.Debug("解除DLL挂钩并关闭辅助窗口");
                        var gameInstance = GameState.GameInstances.First(o => o.MirPid == account.ProcessId.Value);
                        DllInject.Unhook(gameInstance);
                        if (gameInstance.AssistantForm != null)
                        {
                            gameInstance.AssistantForm.Invoke(new Action(() => gameInstance.AssistantForm.Close()));
                        }
                        gameInstance.Clear();
                    }
                    
                    process.Kill();
                    account.ProcessId = null;
                    Log.Information("游戏进程已关闭，账号: {Account}", account.Account);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "关闭游戏进程失败，账号: {Account}", account.Account);
                    // MessageBox.Show($"关闭游戏进程失败: {ex.Message}");
                }
            }
        }

        private void RestartGameProcess(GameAccountModel account)
        {
            Log.Information("重启游戏进程，账号: {Account}", account.Account);
            KillGameProcess(account);
            StartGameProcess(account);
        }

        private async Task AttachToGameProcess(Process process, GameAccountModel account)
        {
            try
            {
                if (process.ProcessName == "ZC.H")
                {
                    var pid = process.Id;
                    var hwnd = process.MainWindowHandle;
                    
                    Log.Information("准备绑定游戏进程，账号: {Account}, PID: {ProcessId}", account.Account, pid);
                    
                    if (!GameState.GameInstances.Any(o => o.MirPid == pid))
                    {
                        var rect = WindowUtils.GetClientRect(hwnd);
                        var gameInstance = new MirGameInstanceModel();
                        GameState.GameInstances.Add(gameInstance);
                        gameInstance.AssistantForm = new AssistantForm(gameInstance, account.Account, account.CharacterName);
                        gameInstance.MirHwnd = hwnd;
                        gameInstance.MirPid = pid;
                        gameInstance.MirBaseAddress = process.MainModule!.BaseAddress;
                        gameInstance.mirVer = process.MainModule?.FileVersionInfo?.FileVersion;
                        gameInstance.MirThreadId = (uint)process.Threads[0].Id;
                        
                        // 设置账号信息
                        gameInstance.AccountInfo = account;
                        
                        Log.Debug("加载DLL到游戏进程");
                        await DllInject.loadDll(gameInstance);

                 
                        // TODO 会导致不刷新 , 需要重新搞个不依赖tab的
                        gameInstance.AssistantForm.Show();
                        gameInstance.AssistantForm.Location = new Point(rect.Left, rect.Top);
                               // 如果是主控，显示辅助窗口
                        if (!account.IsMainControl)
                        {
                            gameInstance.AssistantForm.WindowState = FormWindowState.Minimized;
                        }
                        Log.Information("辅助窗口已显示，账号: {Account}", account.Account);
                
                        await Task.Delay( Environment.ProcessorCount <=4 ?  13_000 : 10000);
                        SendMirCall.Send(gameInstance!, 9099, new nint[] { });
                        await Task.Delay( Environment.ProcessorCount <=4 ?  10_000 : 6000);

                        if (gameInstance.CharacterStatus.CurrentHP == 0)
                        {
                            RestartGameProcess(account);
                        }
                        
                        gameInstance.AssistantForm.Disposed += (sender, args) =>
                        {
                            Log.Debug("辅助窗口已关闭，移除游戏实例，PID: {ProcessId}", gameInstance.MirPid);
                            gameInstance.Clear();
                        };

                        // 添加进程退出事件监听
                        process.EnableRaisingEvents = true;
                        process.Exited += (sender, e) =>
                        {
                            try
                            {
                                if (GameState.GameInstances.Any(o => o.MirPid == process.Id) && gameInstance.AssistantForm != null)
                                {
                                    if (!gameInstance.AssistantForm.IsDisposed)
                                    {
                                        gameInstance.AssistantForm.Invoke(new Action(() => gameInstance.AssistantForm.Close()));
                                    }
                                    gameInstance.Clear();
                                }
                            }
                            catch { }
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "绑定游戏进程失败，账号: {Account}", account.Account);
                MessageBox.Show($"绑定游戏进程失败: {ex.Message}");
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
                            if (GameState.GameInstances.First(o => o.MirPid == pid).AssistantForm!.Visible)
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
                            // 查找对应的账号
                            var account = accountList.FirstOrDefault(a => a.ProcessId == pid);
                           if (account == null)
                            {
                                account = accountList[0];
                            }

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
                RestartGameProcess(account);
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
                RestartGameProcess(account);
                await Task.Delay( Environment.ProcessorCount <=4 ?  10_000 : 7000);
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
            // 重新开始所有任务
            // processTasks();
            autoAtBackground();
            autoForeGround();
            autoAtBackgroundFast();
        }
        private async Task BuyLZ(MirGameInstanceModel instanceValue, CancellationToken _cancellationToken)
        {
            // 没买蜡烛先买, 背包蜡烛小于5
            if (instanceValue.Items.Where(o => o.Name == "蜡烛").Count() < 3)
            {
                bool pathFound = await GoRunFunction.PerformPathfinding(_cancellationToken, instanceValue!, 640, 613, "", 6);
                if (pathFound)
                {
                    await NpcFunction.ClickNPC(instanceValue!, "陈家铺老板");
                    await NpcFunction.BuyLZ(instanceValue!, "蜡烛", 3);
                }
            }
        }

        private async Task repairBasicWeaponClothes(MirGameInstanceModel instanceValue, CancellationToken _cancellationToken)
        {
            await NpcFunction.RefreshPackages(instanceValue);
            // 修理装备
            var CharacterStatus = instanceValue.CharacterStatus!;
            var isLeftAlive = CharacterStatus.X < 400;
            var repairTasks = new[] {
                (npc: !isLeftAlive ? "精武馆老板" : "边界村铁匠铺", pos: EquipPosition.Weapon, x: !isLeftAlive ? 649 : 295, y: !isLeftAlive ? 602 : 608),
                (npc: !isLeftAlive ? "高家店老板" : "白家服装老板", pos: EquipPosition.Dress, x: !isLeftAlive ? 649 : 298, y: !isLeftAlive ? 602 : 607),
                (npc: !isLeftAlive ? "高家店老板" : "白家服装老板", pos: EquipPosition.Helmet, x: !isLeftAlive ? 649 : 298, y: !isLeftAlive ? 602 : 607)
            };
        
            foreach (var task in repairTasks)
            {
                await NpcFunction.RepairEquipment(instanceValue!, task.npc, task.pos, task.x, task.y);
            }
        }

        private async Task buyBasicWeaponClothes(MirGameInstanceModel instanceValue, CancellationToken _cancellationToken)
        {
            await NpcFunction.RefreshPackages(instanceValue);
            var CharacterStatus = instanceValue.CharacterStatus!;
            var isLeftAlive = CharacterStatus.X < 400;
            var tasks  = new[] {
                (npc: !isLeftAlive ? "精武馆老板" : "边界村铁匠铺", pos: EquipPosition.Weapon, x: !isLeftAlive ? 649 : 295, y: !isLeftAlive ? 602 : 608),
                (npc: !isLeftAlive ? "高家店老板" : "白家服装老板", pos: EquipPosition.Dress, x: !isLeftAlive ? 649 : 298, y: !isLeftAlive ? 602 : 607),
                //(npc: !isLeftAlive ? "高家店老板" : "白家服装老板", pos: EquipPosition.Helmet, x: !isLeftAlive ? 649 : 298, y: !isLeftAlive ? 602 : 607)
            };
        
            foreach (var task in tasks)
            {
                await NpcFunction.BuyEquipment(instanceValue!, task.npc, task.pos, task.x, task.y);
            }
        }
        private async Task sellMeat(MirGameInstanceModel instanceValue, CancellationToken _cancellationToken, bool keepMeat = true)
        {
            ItemFunction.ReadBag(instanceValue);

            var meats = instanceValue.Items.Where(o => o.Name == "肉");
            var chickens = instanceValue.Items.Where(o => o.Name == "鸡肉");
            var expressMeats = meats.Skip(1).ToList();
            var expressChickens = chickens.Skip(1).ToList();
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
                    await Task.Delay(400);
                }

                await Task.Delay(500);
                await NpcFunction.RefreshPackages(instanceValue);
            }

        }
        private async Task prepareBags(MirGameInstanceModel instanceValue, CancellationToken _cancellationToken)
        {
            var CharacterStatus = instanceValue.CharacterStatus!;
            var isLeftAlive = CharacterStatus.X < 400;
            // 卖肉
            await sellMeat(instanceValue, _cancellationToken);
            if (!isLeftAlive)
            {
                // 蜡烛检测
                await BuyLZ(instanceValue, _cancellationToken);
            }
            // 买衣服和武器
            await buyBasicWeaponClothes(instanceValue, _cancellationToken);
            // trigger takeon 
            await NpcFunction.autoReplaceEquipment(instanceValue);
            // 修衣服和武器
            await repairBasicWeaponClothes(instanceValue, _cancellationToken);
        }

        private static async Task findNoobNpc(MirGameInstanceModel instanceValue, CancellationToken _cancellationToken)
        {
            var CharacterStatus = instanceValue.CharacterStatus!;
            var isLeftAlive = CharacterStatus.X < 400;
            Log.Information($"找助手 {isLeftAlive}");

            bool pathFound = await GoRunFunction.PerformPathfinding(_cancellationToken, instanceValue!, !isLeftAlive ? 630 : 283, !isLeftAlive ? 603 : 608, "", 6);
            if (pathFound)
            {
                await NpcFunction.ClickNPC(instanceValue!, !isLeftAlive ? "助手小敏" : "助手阿妍");
            }
        }

      
        
         private static async Task findMeatNpc(MirGameInstanceModel instanceValue, CancellationToken _cancellationToken)
        {
            var CharacterStatus = instanceValue.CharacterStatus!;
            var isLeftAlive = CharacterStatus.X < 400;
            Log.Information($"找屠夫 {isLeftAlive}");
            bool pathFound = await GoRunFunction.PerformPathfinding(_cancellationToken, instanceValue!, !isLeftAlive ? 647 : 287, !isLeftAlive ? 595 : 595, "", 6);
            if (pathFound)
            {
                await NpcFunction.ClickNPC(instanceValue!, "屠夫");
            }
        }

        private static async Task findWeaponNpc(MirGameInstanceModel instanceValue, CancellationToken _cancellationToken)
        {
            var CharacterStatus = instanceValue.CharacterStatus!;
            var isLeftAlive = CharacterStatus.X < 400;
            Log.Information($"找武器 {isLeftAlive}");

            bool pathFound = await GoRunFunction.PerformPathfinding(_cancellationToken, instanceValue!, !isLeftAlive ? 635 : 295, !isLeftAlive ? 611 : 608, "", 6);
            if (pathFound)
            {
                await NpcFunction.ClickNPC(instanceValue!, !isLeftAlive ? "精武馆老板" : "边界村铁匠铺");
            }
        }
     
        private async void processTasks()
        {
            var instances = GameState.GameInstances;
            instances.ForEach(async instance =>
            {
                // todo cancel
                // 查看当前出生点
                var instanceValue = instance;
                var CharacterStatus = instanceValue.CharacterStatus!;
                var isLeftAlive = CharacterStatus.X < 400 || CharacterStatus.Level > 10;
                var fixedPoints = new List<(int, int)>();
                var patrolSteps = 10;
                var portalStartX = isLeftAlive ? 200 : 550;
                var portalEndX = isLeftAlive ? 300 : 620;
                var portalStartY = 550;
                var portalEndY = 620;
                // 生成矩形区域内的所有点位
                for (int x = portalStartX; x <= portalEndX; x += patrolSteps)
                {
                    // 根据x的奇偶性决定y的遍历方向，形成蛇形路线
                    var yStart = (x - portalStartX) / patrolSteps % 2 == 0 ? portalStartY : portalEndY;
                    var yEnd = (x - portalStartX) / patrolSteps % 2 == 0 ? portalEndY : portalStartY;
                    var yStep = (x - portalStartX) / patrolSteps % 2 == 0 ? patrolSteps : -patrolSteps;

                    for (int y = yStart; yStep > 0 ? y <= yEnd : y >= yEnd; y += yStep)
                    {
                        // 生成点位
                        fixedPoints.Add((x, y));
                        Log.Debug($"生成巡逻点: ({x}, {y})");
                    }
                }

                Log.Information($"共生成 {fixedPoints.Count} 个巡逻点");

                // 转换为数组
                var patrolPairs = fixedPoints.ToArray();

                if (CharacterStatus.CurrentHP > 0)
                {
                    var act = instanceValue.AccountInfo!;
                    var _cancellationTokenSource = new CancellationTokenSource();

                    await prepareBags(instanceValue, _cancellationTokenSource.Token);
                    // 新手任务
                    // todo 目前是5
                    if (CharacterStatus.Level <= 8 && act.TaskMain0Step < 6)
                    {
                        // 主线
                        if (act.TaskMain0Step == 0)
                        {
                            // 主线4级
                            // click 助手小敏 630 603
                            // @QUEST
                            // 没有 屠夫正在找
                            // 有
                            // npc给你的任务 <做/@QUEST1_1_1> 
                            // npc给你的任务 <不做/@QUEST1_1_2>

                            await findNoobNpc(instanceValue, _cancellationTokenSource.Token);
                            // todo check cmd
                            await NpcFunction.Talk2(instanceValue!, "@QUEST");
                            await NpcFunction.Talk2(instanceValue!, "@QUEST1_1_1");
                            act.TaskMain0Step = 1;
                            // todo json
                            SaveAccountList();
                        }

                        if (act.TaskMain0Step == 1)
                        {
                            // 主线4级 
                            // click 屠夫 
                            // <新手任务对话/@QUEST> 
                            // <继续/@QUEST1_1_1>
                            await findMeatNpc(instanceValue, _cancellationTokenSource.Token);
                            await NpcFunction.Talk2(instanceValue!, "@QUEST");
                            await NpcFunction.Talk2(instanceValue!, "@QUEST1_1_1");

                            await GoRunFunction.NormalAttackPoints(instanceValue, _cancellationTokenSource.Token, patrolPairs, (instanceValue) =>
                            {
                                // 检查背包的肉
                                var meat = instanceValue.Items.Where(o => o.Name == "肉").FirstOrDefault();
                                return meat != null;
                            });

                            act.TaskMain0Step = 2;
                            SaveAccountList();
                        }
                        if (act.TaskMain0Step == 2)
                        {
                            // 找肉

                            // 主线4级 
                            // click 屠夫 
                            // <新手任务对话/@QUEST> 
                            // <继续/@QUEST1_1_1>
                            await findMeatNpc(instanceValue, _cancellationTokenSource.Token);
                            await NpcFunction.Talk2(instanceValue!, "@QUEST");
                            await NpcFunction.Talk2(instanceValue!, "@QUEST1_2_1");

                            // await NormalAttackPoints(instanceValue, _cancellationTokenSource, [(625, 580), (625, 560)], (instanceValue) =>
                            // {
                            //     // 检查背包的肉
                            //     var meat = instanceValue.Items.Where(o => o.Name == "肉").FirstOrDefault();
                            //     return meat != null;
                            // });

                            act.TaskMain0Step = 3;
                            SaveAccountList();
                        }
                        if (act.TaskMain0Step == 3)
                        {
                            await findNoobNpc(instanceValue, _cancellationTokenSource.Token);
                            await NpcFunction.Talk2(instanceValue!, "@QUEST");
                            // todo 保存json
                            act.TaskMain0Step = 4;
                            SaveAccountList();
                        }
                        if (act.TaskMain0Step == 4)
                        {
                            // 升级到5
                            // 抽象到巡逻, 然后能退出
                            await GoRunFunction.NormalAttackPoints(instanceValue, _cancellationTokenSource.Token, patrolPairs, (instanceValue) =>
                            {
                                return instanceValue.CharacterStatus!.Level >= 5;
                            });
                            act.TaskMain0Step = 5;
                            SaveAccountList();
                        }
                        if (act.TaskMain0Step == 5)
                        {

                            await prepareBags(instanceValue, _cancellationTokenSource.Token);
                            await findNoobNpc(instanceValue, _cancellationTokenSource.Token);
                            await NpcFunction.Talk2(instanceValue!, "@QUEST");
                            await NpcFunction.Talk2(instanceValue!, "@QUEST1_1_1");
                            // 精武馆老板
                            await findWeaponNpc(instanceValue, _cancellationTokenSource.Token);
                            await NpcFunction.Talk2(instanceValue!, "@QUEST");
                            await NpcFunction.Talk2(instanceValue!, "@exit");
                            await findWeaponNpc(instanceValue, _cancellationTokenSource.Token);
                            await NpcFunction.Talk2(instanceValue!, "@QUEST");
                            await NpcFunction.Talk2(instanceValue!, "@exit");
                            // 回复会得到乌木剑, 会自动带
                            // 再次找助手 会让你接着走 武士之家
                            await findNoobNpc(instanceValue, _cancellationTokenSource.Token);
                            await NpcFunction.Talk2(instanceValue!, "@QUEST");
                            // <下一页/@Q705_1_1_1>
                            await NpcFunction.Talk2(instanceValue!, "@Q705_1_1_1");
                            await NpcFunction.Talk2(instanceValue!, "@exit");
                            // 查询所有号的等级>=5, 再出去
                            // 逛街
                            // await GoRunFunction.NormalAttackPoints(instanceValue, _cancellationTokenSource.Token, patrolPairs, (instanceValue) =>
                            // {
                            //     var instances = GameState.GameInstances.ToList();
                            //     var allLevel5 = instances.Where(o => o.Value.CharacterStatus!.Level < 5).ToList();
                            //     if (allLevel5.Count == 0)
                            //     {
                            //         return true;
                            //     }
                            //     return false;
                            // });
                            act.TaskMain0Step = 6;
                            SaveAccountList();

                        }
                    }
                    // 支线8级, 没啥调用 先不要了
                    if (CharacterStatus.Level >= 999 && act.TaskSub0Step < 99)
                    {
                        // 1、7级前从新手村助手阿妍处接介绍信任务
                        // 2、给屠夫1块鹿肉、1块鸡肉（得到太阳水1瓶）
                        // 3、得到佛牌，然后找书店老板（15级前完成）
                        // 4、给边界村书店或银杏村药店5块12以上肉和5块5以上的鸡肉，对话选择“助人为快乐之本,我不能要”可得祈福项链。
                        // 5、祈福项链拿给比奇黄飞龙可升到10级。
                        if (act.TaskSub0Step == 0)
                        {
                            // click 助手阿妍 630 603
                            await findNoobNpc(instanceValue, _cancellationTokenSource.Token);
                            await NpcFunction.Talk2(instanceValue!, "@next");
                            await NpcFunction.Talk2(instanceValue!, "@next1");
                            await NpcFunction.Talk2(instanceValue!, "@new01");
                            act.TaskSub0Step = 1;
                            SaveAccountList();
                        }
                        // 2 屠夫
                        if (act.TaskSub0Step == 1)
                        {
                            // click 屠夫 647 595
                            await findMeatNpc(instanceValue, _cancellationTokenSource.Token);
                            await NpcFunction.Talk2(instanceValue!, "@main1");
                            act.TaskSub0Step = 2;
                            SaveAccountList();
                        }
                        if (act.TaskSub0Step == 2)
                        {
                            // 再次开始找肉和鸡肉
                            await GoRunFunction.NormalAttackPoints(instanceValue, _cancellationTokenSource.Token, patrolPairs, (instanceValue) =>
                            {
                                var meats = instanceValue.Items.Where(o => o.Name == "肉").ToList();
                                var chickens = instanceValue.Items.Where(o => o.Name == "鸡肉").ToList();
                                return meats.Count > 0 && chickens.Count > 0;
                            });
                            act.TaskSub0Step = 3;
                            SaveAccountList();
                        }
                        if (act.TaskSub0Step == 3)
                        {
                            // 回屠夫给肉他
                            await findMeatNpc(instanceValue, _cancellationTokenSource.Token);
                            await NpcFunction.Talk2(instanceValue!, "@main1");
                            // <确定/@newnew1_1>
                            await NpcFunction.Talk2(instanceValue!, "@newnew1_1");
                            // <真的吗？太好了！/@job>
                            await NpcFunction.Talk2(instanceValue!, "@job");
                            act.TaskSub0Step = 4;
                            SaveAccountList();
                        }
                        // 一点经验没啥用
                        // if (act.TaskSub0Step == 4)
                        // {
                        //     // 去银杏村药店 药剂师
                        //     bool pathFound = await GoRunFunction.PerformPathfinding(_cancellationTokenSource.Token, instanceValue!, 9, 13, "", 6);
                        //     if (pathFound)
                        //     {
                        //         await NpcFunction.ClickNPC(instanceValue!, "药剂师");
                        //         // 介绍信任务/@news
                        //         await NpcFunction.Talk2(instanceValue!, "@news");
                        //         // < 可以 / @new2_21 >,
                        //         await NpcFunction.Talk2(instanceValue!, "@new2_21");
                        //         // <接受/@new2_211>
                        //         await NpcFunction.Talk2(instanceValue!, "@new2_211");
                        //     }
                        //     act.TaskSub0Step = 5;
                        //     SaveAccountList();
                        // }
                        //  if (act.TaskSub0Step == 5)
                        // {
                        //     // 继续找肉 5 5
                        //     await GoRunFunction.NormalAttackPoints(instanceValue, _cancellationTokenSource.Token, patrolPairs, (instanceValue) =>
                        //     {
                        //         var meats = instanceValue.Items.Where(o => o.Name == "肉").ToList();
                        //         var chickens = instanceValue.Items.Where(o => o.Name == "鸡肉").ToList();
                        //         return meats.Count > 4 && chickens.Count > 4;
                        //     });
                        //     act.TaskSub0Step = 6;
                        //     SaveAccountList();
                        // }
                    }
                    // 主线
                    // 去道士武士魔法之家, todo 跨地图寻路
                    // 道士 544, 560
                    // 武士 107, 316
                    // 魔法 314, 474
                    if (CharacterStatus.Level >= 3)
                    {
                        // todo 跨地图
                        // bool pathFound = await GoRunFunction.PerformPathfinding(_cancellationTokenSource.Token, instanceValue!, 282, 636, "", 10);
                        // if (pathFound)
                        // {
                        // }

                        // 目前死循环
                        while (true)
                        {
                            await GoRunFunction.NormalAttackPoints(instanceValue, _cancellationTokenSource.Token, patrolPairs, (instanceValue) =>
                            {
                                // go home
                                var meats = instanceValue.Items.Where(o => !o.IsEmpty).ToList();
                                return meats.Count > 32;
                            });
                            await prepareBags(instanceValue, _cancellationTokenSource.Token);
                        }
                        // act.TaskSub0Step = 6;
                        // SaveAccountList();

                    }
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

        private async void autoAtBackground(){
            while(true){
                await Task.Delay(15_000);
                var instances = GameState.GameInstances;
                instances.ForEach(instance =>
                {
                    instance.RefreshAll();
                });

                // 其他中断并行需要考虑 
                instances.ForEach(async instance =>
                {
                    if (!instance.IsAttached)
                    {
                        return;
                    }
                    // todo ref方法 避免重复调用
                    var CharacterStatus = instance.CharacterStatus;
                    // 死亡
                    if (CharacterStatus.CurrentHP <= 0)
                    {
                        // 复活 重启
                        // 尝试小退
                        await GoRunFunction.RestartByToSelectScene(instance);
                        //await Task.Delay(3000);
                        //CharacterStatusFunction.GetInfo(instance);
                        //// check hp -- 其实还不起作用
                        //if (CharacterStatus.CurrentHP == 0)
                        //{
                        //    RestartGameProcess(instance.AccountInfo!);
                        //}
                        return;
                    }
                    if (CharacterStatus.CurrentHP > 0)
                    {

                        // 组队
                        if (CharacterStatus.groupMemCount < GameState.GameInstances.Count)
                        {
                            if (instance.AccountInfo.IsMainControl)
                            {
                                // GameInstances 除了自己
                                var members = GameState.GameInstances.Where(o => o.MirPid != instance.MirPid).Select(o => o.CharacterStatus.Name).ToList();
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
                    }
                });
            }
        }
        
          private async void autoAtBackgroundFast(){
            // 其他中断并行需要考虑 
            while (true)
            {
        
                var instances = GameState.GameInstances;
                instances.ForEach(instance =>
                {
                    if (!instance.IsAttached)
                    {
                        return;
                    }
                    instance.RefreshAll();

                    if (instance.CharacterStatus.CurrentHP > 0)
                    {
                        GoRunFunction.TryEatDrug(instance);
                        GoRunFunction.TryHealPeople(instance);
                    }
                });
                await Task.Delay(200);
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
                DllInject.Unhook(gameInstance);
                if (gameInstance.AssistantForm != null && !gameInstance.AssistantForm.IsDisposed)
                {
                    gameInstance.AssistantForm.Close();
                }
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
