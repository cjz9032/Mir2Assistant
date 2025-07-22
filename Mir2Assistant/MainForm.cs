using Mir2Assistant.Common.Functions;
using Mir2Assistant.Common.Models;
using Mir2Assistant.Common.Utils;
using Serilog; // 新增Serilog引用
using Serilog.Sinks.Debug; // 添加Debug sink引用
using System.Diagnostics;
using System.Text.Json;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace Mir2Assistant
{
    public partial class MainForm : Form
    {
        static private Dictionary<int, MirGameInstanceModel> GameInstances = new Dictionary<int, MirGameInstanceModel>();
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
            LoadAccountList();
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
                string arguments = $"{encodeArg}";
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
                await Task.Delay(5000);

                string output = process.StandardOutput.ReadLine(); // 只读一行即可
                // 不需要 WaitForExit
                if (int.TryParse(output.Trim(), out int pid))
                {
                    Log.Information("PowerShell已直接启动游戏进程，账号: {Account}, PID: {Pid}", account.Account, pid);
                    account.ProcessId = pid;
                    // 通过PID获取进程对象
                    var gameProcess = Process.GetProcessById(pid);
                    // 后续绑定DLL等逻辑
                    AttachToGameProcess(gameProcess, account);
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
                    if (GameInstances.ContainsKey(account.ProcessId.Value))
                    {
                        Log.Debug("解除DLL挂钩并关闭辅助窗口");
                        var gameInstance = GameInstances[account.ProcessId.Value];
                        DllInject.Unhook(gameInstance);
                        gameInstance.AssistantForm?.Close();
                        GameInstances.Remove(account.ProcessId.Value);
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

        private void AttachToGameProcess(Process process, GameAccountModel account)
        {
            try
            {
                if (process.ProcessName == "ZC.H")
                {
                    var pid = process.Id;
                    var hwnd = process.MainWindowHandle;
                    
                    Log.Information("准备绑定游戏进程，账号: {Account}, PID: {ProcessId}", account.Account, pid);
                    
                    if (!GameInstances.ContainsKey(pid))
                    {
                        var rect = WindowUtils.GetClientRect(hwnd);
                        var gameInstance = new MirGameInstanceModel();
                        GameInstances.Add(pid, gameInstance);
                        gameInstance.AssistantForm = new AssistantForm(gameInstance, account.Account, account.CharacterName);
                        gameInstance.MirHwnd = hwnd;
                        gameInstance.MirPid = pid;
                        gameInstance.MirBaseAddress = process.MainModule!.BaseAddress;
                        gameInstance.mirVer = process.MainModule?.FileVersionInfo?.FileVersion;
                        gameInstance.MirThreadId = (uint)process.Threads[0].Id;
                        
                        // 设置账号信息
                        gameInstance.AccountInfo = account;
                        
                        Log.Debug("加载DLL到游戏进程");
                        DllInject.loadDll(gameInstance);
                        
                        // 如果是主控，显示辅助窗口
                        // if (account.IsMainControl)
                        // {
                       
                        // }
                        gameInstance.AssistantForm.Show();
                        gameInstance.AssistantForm.Location = new Point(rect.Left, rect.Top);
                        Log.Information("辅助窗口已显示，账号: {Account}", account.Account);
                        
                        gameInstance.AssistantForm.Disposed += (sender, args) =>
                        {
                            Log.Debug("辅助窗口已关闭，移除游戏实例，PID: {ProcessId}", gameInstance.MirPid);
                            GameInstances.Remove(gameInstance.MirPid);
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
                        if (GameInstances.ContainsKey(pid))
                        {
                            if (GameInstances[pid].AssistantForm!.Visible)
                            {
                                GameInstances[pid].AssistantForm!.Hide();
                            }
                            else
                            {
                                GameInstances[pid].AssistantForm!.Show();
                                GameInstances[pid].AssistantForm!.WindowState = FormWindowState.Normal;
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
                        GameInstances.Values.FirstOrDefault(o => o.AssistantForm?.Handle == hwnd)?.AssistantForm!.Hide();
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
            // 先杀死所有ZC.H进程
            KillAllGameProcess();

                foreach (var account in accountList)
                {
                    RestartGameProcess(account);
                    await Task.Delay(7_000); // 异步延迟5秒
                }
             
                // sllep 
                // await Task.Delay(20_000);
             
        }

        private async void btnRestartTask_Click(object sender, EventArgs e)
        {
            Log.Information("重启所有游戏任务");
            // 重新开始所有任务
            // processTasks();
               autoAtBackground();
                autoForeGround();
        }

        private static async Task<bool> NormalAttackPoints(MirGameInstanceModel instanceValue, CancellationTokenSource _cancellationTokenSource, (int, int)[] patrolPairs, Func<MirGameInstanceModel, bool> checker)
        {
            var allowMonsters = new string[]  {"鸡", "鹿", "羊", "稻草人", "多钩猫", "钉耙猫", "半兽人", "半兽战士", "半兽勇士",
                "森林雪人", "蛤蟆", "蝎子",
                "毒蜘蛛", "洞蛆", "蝙蝠", "骷髅","骷髅战将", "掷斧骷髅", "骷髅战士", "僵尸","山洞蝙蝠"};
            var allowButch = new string[]  {"鸡", "鹿", "蝎子", "蜘蛛", "洞蛆"};
            // >=5级 排除掉鹿先 但是少肉
            // if (instanceValue.CharacterStatus!.Level >= 5)
            // {
            //     allowMonsters = allowMonsters.Where(o => o != "鹿").ToArray();
            // }
            // 当前巡回
            var curP = 0;
            var CharacterStatus = instanceValue.CharacterStatus!;
            while (true)
            {
                // 主从模式
                // 主人是点位
                var (px, py) = (0, 0);
                // 从是跟随
                if (instanceValue.AccountInfo!.IsMainControl)
                {
                    // 主人是点位
                    (px, py) = patrolPairs[curP];
                }
                else
                {
                    // 从是跟随
                    var instances = GameInstances.ToList();
                    var mainInstance = instances.FirstOrDefault(o => o.Value.AccountInfo!.IsMainControl);
                    if (mainInstance.Key != 0)
                    {
                        (px, py) = (mainInstance.Value.CharacterStatus!.X!, mainInstance.Value.CharacterStatus!.Y!);
                    }
                }
                

                bool _whateverPathFound = await GoRunFunction.PerformPathfinding(_cancellationTokenSource.Token, instanceValue!, px, py, "", 5);
                // 如果是跟随
                if (!instanceValue.AccountInfo!.IsMainControl)
                {
                    // 从是跟随 -- 这是重复代码 先放着
                    var instances = GameInstances.ToList();
                    var mainInstance = instances.FirstOrDefault(o => o.Value.AccountInfo!.IsMainControl);
                    if (mainInstance.Key != 0)
                    {
                        (px, py) = (mainInstance.Value.CharacterStatus!.X!, mainInstance.Value.CharacterStatus!.Y!);
                    }
                    // 检测距离
                    if (Math.Max(Math.Abs(px - CharacterStatus.X), Math.Abs(py - CharacterStatus.Y)) > 9)
                    {
                        // 跟随
                        await GoRunFunction.PerformPathfinding(_cancellationTokenSource.Token, instanceValue!, px, py, "", 1);
                    }
                }

                // 无怪退出
                while (true)
                {
                    // 查看存活怪物 并且小于距离10个格子
                    var ani = instanceValue.Monsters.Values.Where(o => !o.isDead &&
                    allowMonsters.Contains(o.Name) &&
                     // 还要看下是不是距离巡逻太远了, 就不要
                     Math.Max(Math.Abs(o.X - px), Math.Abs(o.Y - py)) < 16
                     && Math.Max(Math.Abs(o.X - CharacterStatus.X), Math.Abs(o.Y - CharacterStatus.Y)) < 13)
                    .OrderBy(o => Math.Max(Math.Abs(o.X - CharacterStatus.X), Math.Abs(o.Y - CharacterStatus.Y)))
                    .FirstOrDefault();
                    if (ani != null)
                    {
                        // 如果距离超过1 就需要寻路
                        if (Math.Max(Math.Abs(ani.X - px), Math.Abs(ani.Y - py)) > 1)
                        {
                            // 暂时就给1了
                            await GoRunFunction.PerformPathfinding(_cancellationTokenSource.Token, instanceValue!, ani.X, ani.Y, "", 1);
                        }
                        // 攻击
                        // 持续攻击, 超过就先放弃
                        var monTried = 0;
                        while (true)
                        {
                            monTried++;
                            MonsterFunction.SlayingMonster(instanceValue!, ani.Addr);
                            // 注意判断距离 可能会跑
                            if (Math.Max(Math.Abs(ani.X - CharacterStatus.X), Math.Abs(ani.Y - CharacterStatus.Y)) > 1)
                            {
                                MonsterFunction.SlayingMonsterCancel(instanceValue!);
                                await GoRunFunction.PerformPathfinding(_cancellationTokenSource.Token, instanceValue!, ani.X, ani.Y, "", 1);
                            }
                            await Task.Delay(200);
                            if (ani.isDead || monTried > 150)
                            {
                                MonsterFunction.SlayingMonsterCancel(instanceValue!);
                                break;
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }

               
             
                // 没怪了 可以捡取东西 或者挖肉
                // 捡取
                // 按距离, 且没捡取过
                var drops = instanceValue.DropsItems.Where(o => !instanceValue.pickupItemIds.Contains(o.Value.Id))
                .OrderBy(o => Math.Max(Math.Abs(o.Value.X - CharacterStatus.X), Math.Abs(o.Value.Y - CharacterStatus.Y)));
                foreach (var drop in drops)
                {
                    bool pathFound2 = await GoRunFunction.PerformPathfinding(_cancellationTokenSource.Token, instanceValue!, drop.Value.X, drop.Value.Y, "", 0);
                    if (pathFound2)
                    {
                        ItemFunction.Pickup(instanceValue!);
                        // 加捡取过的名单,
                        instanceValue.pickupItemIds.Add(drop.Value.Id);
                    }
                }
                // 屠挖肉
                var bodys = instanceValue.Monsters.Values.Where(o => o.isDead && allowButch.Contains(o.Name) && !o.isButched && Math.Max(Math.Abs(o.X - CharacterStatus.X), Math.Abs(o.Y - CharacterStatus.Y)) < 13)
                .OrderBy(o => Math.Max(Math.Abs(o.X - CharacterStatus.X), Math.Abs(o.Y - CharacterStatus.Y)));
                foreach (var body in bodys)
                {
                    bool pathFound2 = await GoRunFunction.PerformPathfinding(_cancellationTokenSource.Token, instanceValue!, body.X, body.Y, "", 2);
                    if (pathFound2)
                    {
                        // 要持续屠宰, 直到尸体消失, 最大尝试 30次
                        var tried = 0;
                        while (tried < 20)
                        {
                            SendMirCall.Send(instanceValue!, 3030, new nint[] { (nint)body.X, (nint)body.Y, 0, body.Id });
                            await Task.Delay(150);
                            MonsterFunction.ReadMonster(instanceValue!);
                            if (body.isButched)
                            {
                                break;
                            }
                            tried++;
                        }
                    }
                }
                // checker 满足条件就跳出循环, checker是参数
                if (checker(instanceValue!))
                {
                    break;
                }
                curP++;
                curP = curP % patrolPairs.Length;
                continue;
            }
            return true;

        }
        private async void processTasks()
        {
            var instances = GameInstances.ToList();
            instances.ForEach(async instance =>
            {
                var instanceValue = instance.Value;
                var CharacterStatus = instanceValue.CharacterStatus!;
                var fixedPoints = new List<(int, int)>();
                var patrolSteps = 15;
                var portalStartX = 580;
                var portalEndX = 660;
                var portalStartY = 580;
                var portalEndY = 660;
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
                    // 新手任务
                    var _cancellationTokenSource = new CancellationTokenSource();
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

                            bool pathFound = await GoRunFunction.PerformPathfinding(_cancellationTokenSource.Token, instanceValue!, 630, 603, "", 6);
                            if (pathFound)
                            {
                                await NpcFunction.ClickNPC(instanceValue!, "助手小敏");
                                // todo check cmd
                                await NpcFunction.Talk2(instanceValue!, "@QUEST");
                                await NpcFunction.Talk2(instanceValue!, "@QUEST1_1_1");
                                act.TaskMain0Step = 1;
                                // todo json
                                SaveAccountList();
                            }

                        }

                        if (act.TaskMain0Step == 1)
                        {
                            // 主线4级 
                            // click 屠夫 
                            // <新手任务对话/@QUEST> 
                            // <继续/@QUEST1_1_1>
                            bool pathFound = await GoRunFunction.PerformPathfinding(_cancellationTokenSource.Token, instanceValue!, 647, 595, "", 6);
                            if (pathFound)
                            {
                                await NpcFunction.ClickNPC(instanceValue!, "屠夫");
                                await NpcFunction.Talk2(instanceValue!, "@QUEST");
                                await NpcFunction.Talk2(instanceValue!, "@QUEST1_1_1");

                                await NormalAttackPoints(instanceValue, _cancellationTokenSource, patrolPairs, (instanceValue) =>
                                {
                                    // 检查背包的肉
                                    var meat = instanceValue.Items.Where(o => o.Name == "肉").FirstOrDefault();
                                    return meat != null;
                                });

                                act.TaskMain0Step = 2;
                                SaveAccountList();
                            }
                        }
                        if (act.TaskMain0Step == 2)
                        {
                            // 找肉

                            // 主线4级 
                            // click 屠夫 
                            // <新手任务对话/@QUEST> 
                            // <继续/@QUEST1_1_1>
                            bool pathFound = await GoRunFunction.PerformPathfinding(_cancellationTokenSource.Token, instanceValue!, 647, 595, "", 6);
                            if (pathFound)
                            {
                                await NpcFunction.ClickNPC(instanceValue!, "屠夫");
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
                        }
                        if (act.TaskMain0Step == 3)
                        {
                            // click 助手小敏 630 603
                            bool pathFound = await GoRunFunction.PerformPathfinding(_cancellationTokenSource.Token, instanceValue!, 630, 603, "", 6);
                            if (pathFound)
                            {
                                await NpcFunction.ClickNPC(instanceValue!, "助手小敏");
                                await NpcFunction.Talk2(instanceValue!, "@QUEST");
                                // todo 保存json
                                act.TaskMain0Step = 4;
                                SaveAccountList();
                            }
                        }
                        if (act.TaskMain0Step == 4)
                        {
                            // 升级到5
                            // 抽象到巡逻, 然后能退出
                            await NormalAttackPoints(instanceValue, _cancellationTokenSource, patrolPairs, (instanceValue) =>
                            {
                                return instanceValue.CharacterStatus!.Level >= 5;
                            });
                            act.TaskMain0Step = 5;
                            SaveAccountList();
                        }
                        if (act.TaskMain0Step == 5)
                        {
                            Debugger.Break();

                            // 屠夫 647 595 // todo 屠夫记录NPC
                            bool pathFound = await GoRunFunction.PerformPathfinding(_cancellationTokenSource.Token, instanceValue!, 647, 595, "", 6);
                            if (pathFound)
                            {
                                // 只要点一次就够
                                await NpcFunction.ClickNPC(instanceValue!, "屠夫");
                            }
                            // 修理装备 -- 应该还不需要 以后再加
                            // 穿先不管 自己会穿
                            // 先卖掉多余的肉和鸡肉, 保留5个 没意义
                            ItemFunction.ReadBag(instanceValue);
                            var meats = instanceValue.Items.Where(o => o.Name == "肉");
                            var chickens = instanceValue.Items.Where(o => o.Name == "鸡肉");
                            var expressMeats = meats.Skip(5).ToList(); 
                            var expressChickens = chickens.Skip(5).ToList(); 
                            var allMeats = expressMeats.Concat(expressChickens).ToList();
                            foreach (var meat in allMeats)
                            {
                                // 卖肉 TODO 抽象方法
                                nint[] data = StringUtils.GenerateCompactStringData(meat.Name);
                                Array.Resize(ref data, data.Length + 1);
                                data[data.Length - 1] = meat.Id;
                                SendMirCall.Send(instanceValue!, 3011, data);
                                await Task.Delay(200);
                            }
                            await Task.Delay(500);
                            SendMirCall.Send(instanceValue!, 9010, new nint[] { });
                            // 先找助手
                            bool pathFound1 = await GoRunFunction.PerformPathfinding(_cancellationTokenSource.Token, instanceValue!, 630, 603, "", 6);
                            if (pathFound1)
                            {
                                // TODO 不确定对不对 先debug下这里
                                await NpcFunction.ClickNPC(instanceValue!, "助手小敏");
                                await NpcFunction.Talk2(instanceValue!, "@QUEST");
                            

                                await NpcFunction.Talk2(instanceValue!, "@QUEST1_1_1");
                            }
                            // 精武馆老板
                            bool pathFound2 = await GoRunFunction.PerformPathfinding(_cancellationTokenSource.Token, instanceValue!, 649, 602, "", 6);
                            if (pathFound2)
                            {

                                await NpcFunction.ClickNPC(instanceValue!, "精武馆老板");
                                await NpcFunction.Talk2(instanceValue!, "@QUEST");
                                // 这里也是 感觉不太对 先debug下这里

                                await NpcFunction.Talk2(instanceValue!, "@exit");
                                await NpcFunction.ClickNPC(instanceValue!, "精武馆老板");
                                await NpcFunction.Talk2(instanceValue!, "@QUEST");
                                await NpcFunction.Talk2(instanceValue!, "@exit");
                                // 回复会得到乌木剑, 会自动带
                            }
                            // 再次找助手 会让你接着走 武士之家
                            bool pathFound3 = await GoRunFunction.PerformPathfinding(_cancellationTokenSource.Token, instanceValue!, 630, 603, "", 6);
                            if (pathFound3)
                            {

                                await NpcFunction.ClickNPC(instanceValue!, "助手小敏");
                                // 这里也是 感觉不太对 先debug下这里
                                // todo 这里也要记录下 这个不对

                                await NpcFunction.Talk2(instanceValue!, "@QUEST");
                                await NpcFunction.Talk2(instanceValue!, "@QUEST1_1_1");
                            }
                            // 查询所有号的等级>=5, 再出去
                            // 逛街
                            await NormalAttackPoints(instanceValue, _cancellationTokenSource, patrolPairs, (instanceValue) =>
                            {
                                var instances = GameInstances.ToList();
                                var allLevel5 = instances.Where(o => o.Value.CharacterStatus!.Level < 5).ToList();
                                if (allLevel5.Count == 0)
                                {
                                    return true;
                                }
                                return false;
                            });
                            act.TaskMain0Step = 6;
                            SaveAccountList();

                        }
                    }
                    // 支线8级, 主线先做一半到支线一起做
                    if (CharacterStatus.Level <= 8 && act.TaskSub0Step < 99)
                    {
                        // 1、7级前从新手村助手阿妍处接介绍信任务
                        // 2、给屠夫1块鹿肉、1块鸡肉（得到太阳水1瓶）
                        // 3、得到佛牌，然后找书店老板（15级前完成）
                        // 4、给边界村书店或银杏村药店5块12以上肉和5块5以上的鸡肉，对话选择“助人为快乐之本,我不能要”可得祈福项链。
                        // 5、祈福项链拿给比奇黄飞龙可升到10级。
                        if (act.TaskSub0Step == 0)
                        {
                            // click 助手阿妍 630 603
                            bool pathFound = await GoRunFunction.PerformPathfinding(_cancellationTokenSource.Token, instanceValue!, 630, 603, "", 6);
                            if (pathFound)
                            {
                                await NpcFunction.ClickNPC(instanceValue!, "助手小敏");
                                await NpcFunction.Talk2(instanceValue!, "@next");
                                await NpcFunction.Talk2(instanceValue!, "@next1");
                                await NpcFunction.Talk2(instanceValue!, "@new01");
                                act.TaskSub0Step = 1;
                                SaveAccountList();
                            }
                        }
                        // 2 屠夫
                        if (act.TaskSub0Step == 1)
                        {
                            // click 屠夫 647 595
                            bool pathFound = await GoRunFunction.PerformPathfinding(_cancellationTokenSource.Token, instanceValue!, 647, 595, "", 6);
                            if (pathFound)
                            {
                                await NpcFunction.ClickNPC(instanceValue!, "屠夫");
                                await NpcFunction.Talk2(instanceValue!, "@main1");
                                act.TaskSub0Step = 2;
                                SaveAccountList();
                            }
                        }
                        if (act.TaskSub0Step == 2)
                        {
                            // 再次开始找肉和鸡肉
                            await NormalAttackPoints(instanceValue, _cancellationTokenSource, patrolPairs, (instanceValue) =>
                            {
                                var meats = instanceValue.Items.Where(o => o.Name == "肉").ToList();
                                var chickens = instanceValue.Items.Where(o => o.Name == "鸡肉").ToList();
                                return meats.Count > 0 || chickens.Count > 0;
                            });
                            act.TaskSub0Step = 3;
                            SaveAccountList();
                        }
                        if (act.TaskSub0Step == 3)
                        {
                            // 回屠夫给肉他
                            bool pathFound = await GoRunFunction.PerformPathfinding(_cancellationTokenSource.Token, instanceValue!, 647, 595, "", 6);
                            if (pathFound)
                            {
                                await NpcFunction.ClickNPC(instanceValue!, "屠夫");
                                await NpcFunction.Talk2(instanceValue!, "@main1");
                                await NpcFunction.Talk2(instanceValue!, "@next");
                            }
                        }
                    }
                    // 去道士武士之家, todo 跨地图寻路
                    if (CharacterStatus.Level <= 15 && act.TaskSub0Step < 99)
                    {
                        // 书店老板
                        bool pathFound = await GoRunFunction.PerformPathfinding(_cancellationTokenSource.Token, instanceValue!, 649, 602, "", 6);
                        if (pathFound)
                        {
                        }
                        return;
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
                await Task.Delay(15000);

                // 其他中断并行需要考虑 
                var instances = GameInstances.ToList();
                foreach (var instance in instances)
                {
                    // todo ref方法 避免重复调用
                    CharacterStatusFunction.GetInfo(instance.Value);
                    CharacterStatusFunction.GetUsedItemInfo(instance.Value);
                    ItemFunction.ReadBag(instance.Value);

                    var CharacterStatus = instance.Value.CharacterStatus;
                    // 死亡
                    if (CharacterStatus.CurrentHP <= 0)
                    {
                        // 复活 重启
                        RestartGameProcess(instance.Value.AccountInfo!);
                        continue;
                    }
                    if (CharacterStatus.CurrentHP > 0)
                    {

                        // 组队
                        if (CharacterStatus.groupMemCount < GameInstances.Count)
                        {
                            if (instance.Value.AccountInfo.IsMainControl)
                            {
                                // GameInstances 除了自己
                                var members = GameInstances.Where(o => o.Key != instance.Key).Select(o => o.Value.CharacterStatus.Name).ToList();
                                foreach (var member in members)
                                {
                                    nint[] data = StringUtils.GenerateCompactStringData(member);
                                    SendMirCall.Send(instance.Value, 9004, data);
                                    await Task.Delay(300);
                                }
                            }
                            else
                            {
                                if (!instance.Value.CharacterStatus.allowGroup)
                                {
                                    SendMirCall.Send(instance.Value, 9005, new nint[] { 1 });
                                }
                            }
                        }
                        // 替换装备, 如果失败就不做 
                        // todo 后续修理要避免冲突
                        var bagItems = instance.Value.Items;
                        foreach (var itemWithIndex in CharacterStatus.useItems.Select((item, index) => new { item, index }))
                        {
                            var item = itemWithIndex.item;
                            var index = itemWithIndex.index;
                            // TODO 装备评分
                            // 先非常简略从背包找乌木剑, 并且手上是木剑, 后面再优化
                            if (item.Name == "木剑")
                           {
                                var final = bagItems.Where(o => o.Name == "乌木剑" && !o.IsLowDurability).FirstOrDefault();
                                if (final != null)
                                {   
                                    nint toIndex = index;
                                    nint bagGridIndex = final.Index;
                                    SendMirCall.Send(instance.Value, 3021, new nint[] { bagGridIndex, toIndex });
                                    await Task.Delay(800);
                                }
                            }
                            if (item.IsEmpty)
                            {
                                // 从bags里找装备, 要符合条件
                                // 1.index->stdmode
                                // 2.not IsLowDurability
                                // 3.能携带, 目前只看等级, reqType  // todo 更多类型,以及携带策略可能要搭配
                                // 4.TODO 负重腕力等, 先不管
                                var final = bagItems.Where(o => o.stdModeToUseItemIndex.Contains((byte)index)
                                && !o.IsLowDurability
                                && o.reqType == 0
                                && o.reqPoints <= CharacterStatus.Level
                                ).FirstOrDefault();
                                if (final != null)
                                {
                                    // 装回检查的位置
                                    nint toIndex = index;
                                    nint bagGridIndex = final.Index;
                                    SendMirCall.Send(instance.Value, 3021, new nint[] { bagGridIndex, toIndex });
                                    await Task.Delay(800);
                                }
                            }
                        }
                    }
                }
                await Task.Delay(60_000);
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Log.Information("应用程序正在关闭");
            HotKeyUtils.UnregisterHotKey(Handle, 200);
            Log.Debug("已注销热键");
            // SaveAccountList();
            
            Log.Debug("正在解除所有DLL挂钩，游戏实例数量: {InstanceCount}", GameInstances.Count);
            Task.Run(() =>
            {
                foreach (var gameInstance in GameInstances.Values)
                {
                    DllInject.Unhook(gameInstance);
                }
            });
            Thread.Sleep(200);
            Log.Information("应用程序已关闭");
            Log.CloseAndFlush();
        }
    }
}
