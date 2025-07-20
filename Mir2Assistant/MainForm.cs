using Mir2Assistant.Common.Functions;
using Mir2Assistant.Common.Models;
using Mir2Assistant.Common.Utils;
using Serilog; // 新增Serilog引用
using Serilog.Sinks.Debug; // 添加Debug sink引用
using System.Diagnostics;
using System.Text.Json;

namespace Mir2Assistant
{
    public partial class MainForm : Form
    {
        private Dictionary<int, MirGameInstanceModel> GameInstances = new Dictionary<int, MirGameInstanceModel>();
        private string currentProcessName = Process.GetCurrentProcess().ProcessName;
        private List<GameAccountModel> accountList = new List<GameAccountModel>();
        private string configFilePath = Path.Combine(Directory.GetCurrentDirectory(), "accounts.json");
        private string gamePath = "ZC.H"; // 游戏路径，需要配置
        private string gameDirectory = @"G:\cq\cd"; // 游戏所在目录
        private string encodeArg = @"CqJHmIicTR62d+9dSOk58cTKgI7HznPbN6DJTvZprSAELnz5oDnMgNt5wBjuFk1qAxqanu//vSGjdVAYZn1QVPj5eBNHqu8r4kAsaof5+vscS8Zg1/EMOLMUwqpHS2YUm5UOhb889exVRhd96hTJFIy2GNszQaAk4ncba891PYNsKBxPjF9PeD+eX8L/GJU6ZIHi/GdjTYUxiBMd20cBkivf1tc3SbFSLabzUQjMLFYEX0f3dTqz8T3pAGRR5eGdIC5UiEbcGJowco1ftRbqqTIKMqyHUk3ie7SiX0uDpRK3DjxXN2dVyAqfucJ6HiftllA5LrVZb77XUr4JOt631s2Ku6zEgZhYDYRH9Kip8qxiRMPXSzzNfaP5gCaNQfxPfePxDlcHectMqT+XV2LzEkJWEEEnF79SyHKT6Uiz99UeHtZ11MDST5NVTwie/bOnFyu2CT7xScxoWT+yyuw3d7tNGAkt1fqlTtXTlc2/BT5ps9phS154s8TsyjcWNDoXWhXPgrknoFSJVhtQbl0qxBUvXFVYE2wDh6D+pe5kBMLOF8PRc82m4PcpGy+vimoRVJCkGamn757CLu4Bg2G4sda4RxQmk2dtPSu17irMsc/Cxmebkv3W76xPKdUeNgFYF+oWJDCkwfj7iFvzolHb1KaSuCHRnOb6O+mvW51BGB5tkxKzrXt2XIcNr1DWwQrkZT3HKQsyY1vILGR0U2xfE+Z9wQCVwwCOsERTh9RxZt2Iht6vKWV0DDn2TWfVevPPl5Du3QU+Y7lp3WmIe+GwLZjueViLnJIL46EmpMKXA1/s+zSTW9s1No5eSoVoRmYN6FH7Wds1xvMSn2JovV3OE7zo+DMLC69xLopScUHBKVeQQOLHVkLvqVepYGdQ0fg3tc17vxphP046OdSzdxxJ1OB8xNtK+dgH9nxaYBPZOCWb7Wutz8kWJaO8lziTLMHD6jfR77lkt2HmO0ENJb36REyfeWaCQ5cnlLnlhSy9MK5HH3P95dreEORsNZnNWBYN";
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
               string json = JsonSerializer.Serialize(accountList, new JsonSerializerOptions { WriteIndented = true });
               File.WriteAllText(configFilePath, json);
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
                    await Task.Delay(10_000); // 异步延迟5秒
                }
             
                // sllep 
                await Task.Delay(20_000);
                autoAtBackground();
                autoForeGround();
        }
        private async void processTasks(){
                var instances = GameInstances.ToList();
                instances.ForEach(async instance =>
                {
                    var instanceValue = instance.Value;

                    var CharacterStatus = instanceValue.CharacterStatus;
                    if (CharacterStatus.CurrentHP > 0)
                    {
                        var act = instanceValue.AccountInfo!;
                        // 新手任务
                        var _cancellationTokenSource = new CancellationTokenSource();
                        if (CharacterStatus.Level <= 4 && act.TaskMain0Step < 99)
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

                                bool pathFound = await GoRunFunction.PerformPathfinding(_cancellationTokenSource.Token, instanceValue!,630,603, "", 3);
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
                                bool pathFound = await GoRunFunction.PerformPathfinding(_cancellationTokenSource.Token, instanceValue!,647,595, "", 3);
                                if (pathFound)
                                {
                                    await NpcFunction.ClickNPC(instanceValue!, "屠夫");
                                    await NpcFunction.Talk2(instanceValue!, "@QUEST");
                                    await NpcFunction.Talk2(instanceValue!, "@QUEST1_1_1");
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
                                bool pathFound = await GoRunFunction.PerformPathfinding(_cancellationTokenSource.Token, instanceValue!,647,595, "", 3);
                                if (pathFound)
                                {
                                    await NpcFunction.ClickNPC(instanceValue!, "屠夫");
                                    await NpcFunction.Talk2(instanceValue!, "@QUEST");
                                    await NpcFunction.Talk2(instanceValue!, "@QUEST1_2_1");
                                    act.TaskMain0Step = 3;
                                    SaveAccountList();
                                }
                            }
                            if (act.TaskMain0Step == 3)
                            {
                                // click 助手小敏 630 603
                                bool pathFound = await GoRunFunction.PerformPathfinding(_cancellationTokenSource.Token, instanceValue!,630,603, "", 3);
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
                                // click 助手小敏 630 603
                                // 升级到3
                                while (CharacterStatus.Level < 3)
                                {
                                    // todo 抽象到巡逻, 然后能退出
                                    // 巡回升级 625 580->560->540
                                    (int, int)[] patrolPairs = { (625, 580), (625, 560), (625, 540) }; // 3 对巡回升级数值
                                    // 当前巡回
                                    var curP =  0;
                                    while(true){
                                        var (px,py) = patrolPairs[curP];
                                        bool pathFound = await GoRunFunction.PerformPathfinding(_cancellationTokenSource.Token, instanceValue!,px,py, "", 3);

                                        // 查看存活怪物 并且小于距离10个格子
                                        var ani = instanceValue.Monsters.Values.Where(o => !o.isDead &&
                                            Math.Max(Math.Abs(o.X - CharacterStatus.X), Math.Abs(o.Y - CharacterStatus.Y)) < 10)
                                        .OrderBy(o => Math.Max(Math.Abs(o.X - CharacterStatus.X), Math.Abs(o.Y - CharacterStatus.Y)))
                                        .FirstOrDefault();
                                        if (ani != null)
                                        {
                                            // 攻击
                                            MonsterFunction.SlayingMonster(instanceValue!, ani.Addr);
                                        } else {
                                            // 没怪了 可以捡取东西 或者挖肉
                                            // 捡取
                                            // 按距离, 且没捡取过
                                            var drops = instanceValue.DropsItems.Where(o => !instanceValue.pickupItemIds.Contains(o.Value.Id))
                                            .OrderBy(o => Math.Max(Math.Abs(o.Value.X - CharacterStatus.X), Math.Abs(o.Value.Y - CharacterStatus.Y)));
                                            foreach(var drop in drops){
                                                bool pathFound2 = await GoRunFunction.PerformPathfinding(_cancellationTokenSource.Token, instanceValue!,drop.Value.X,drop.Value.Y, "", 0);
                                                if(pathFound2){
                                                    ItemFunction.Pickup(instanceValue!);
                                                    // 加捡取过的名单,
                                                    instanceValue.pickupItemIds.Add(drop.Value.Id);
                                                }
                                            }

                                            // 挖
                                            curP++;
                                            curP = curP % patrolPairs.Length;
                                            continue;
                                        }
                                    }


                                }
                            }
                        }
                        // todo 可以先做4级的
                        if (CharacterStatus.Level <= 8 && act.TaskSub0Step < 99)
                        {
                            // sub
                            //支线8级
                            //click 助手小敏 630 603
                            // @next
                            // @next1
                            // @new01 (提示：点击此处可接支线任务)

                            // 2 屠夫
                            // <介绍信任务对话/@main1> 

                        }
                        return;
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
            // processTasks();
        }

        private async void autoAtBackground(){
            while(true){
                // 其他中断并行需要考虑
                var instances = GameInstances.ToList();
                foreach (var instance in instances)
                {
                    var CharacterStatus = instance.Value.CharacterStatus;
                    if (CharacterStatus.CurrentHP > 0)
                    {
                        // 组队
                        if (CharacterStatus.groupMemCount < GameInstances.Count) { 
                            if(instance.Value.AccountInfo.IsMainControl){
                                // GameInstances 除了自己
                                var members = GameInstances.Where(o => o.Key != instance.Key).Select(o => o.Value.CharacterStatus.Name).ToList();
                                foreach (var member in members)
                                {
                                    nint[] data = StringUtils.GenerateCompactStringData(member);
                                    SendMirCall.Send(instance.Value, 9004, data);
                                }
                            } else {
                                if (!instance.Value.CharacterStatus.allowGroup)
                                {
                                    SendMirCall.Send(instance.Value, 9005, new nint[]{1});
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
                await Task.Delay(30_000);
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
