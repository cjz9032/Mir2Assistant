using Mir2Assistant.Common.Functions;
using Mir2Assistant.Common.Models;
using Mir2Assistant.Common.Utils;
using System.Diagnostics;
using System.Text.Json;
using Serilog; // ����Serilog����
using Serilog.Sinks.Debug; // ���Debug sink����

namespace Mir2Assistant
{
    public partial class MainForm : Form
    {
        private Dictionary<int, MirGameInstanceModel> GameInstances = new Dictionary<int, MirGameInstanceModel>();
        private string currentProcessName = Process.GetCurrentProcess().ProcessName;
        private List<GameAccountModel> accountList = new List<GameAccountModel>();
        private string configFilePath = Path.Combine(Directory.GetCurrentDirectory(), "accounts.json");
        private string gamePath = "ZC.H"; // ��Ϸ·������Ҫ����
        private string gameDirectory = @"G:\cq\cd"; // ��Ϸ����Ŀ¼
        private string encodeArg = @"CqJHmIicTR62d+9dSOk58cTKgI7HznPbN6DJTvZprSAELnz5oDnMgNt5wBjuFk1qAxqanu//vSGjdVAYZn1QVPj5eBNHqu8r4kAsaof5+vscS8Zg1/EMOLMUwqpHS2YUm5UOhb889exVRhd96hTJFIy2GNszQaAk4ncba891PYNsKBxPjF9PeD+eX8L/GJU6ZIHi/GdjTYUxiBMd20cBkivf1tc3SbFSLabzUQjMLFYEX0f3dTqz8T3pAGRR5eGdIC5UiEbcGJowco1ftRbqqTIKMqyHUk3ie7SiX0uDpRK3DjxXN2dVyAqfucJ6HiftllA5LrVZb77XUr4JOt631s2Ku6zEgZhYDYRH9Kip8qxiRMPXSzzNfaP5gCaNQfxPfePxDlcHectMqT+XV2LzEkJWEEEnF79SyHKT6Uiz99UeHtZ11MDST5NVTwie/bOnFyu2CT7xScxoWT+yyuw3d7tNGAkt1fqlTtXTlc2/BT5ps9phS154s8TsyjcWNDoXWhXPgrknoFSJVhtQbl0qxBUvXFVYE2wDh6D+pe5kBMLOF8PRc82m4PcpGy+vimoRVJCkGamn757CLu4Bg2G4sda4RxQmk2dtPSu17irMsc/Cxmebkv3W76xPKdUeNgFYF+oWJDCkwfj7iFvzolHb1KaSuCHRnOb6O+mvW51BGB5tkxKzrXt2XIcNr1DWwQrkZT3HKQsyY1vILGR0U2xfE+Z9wQCVwwCOsERTh9RxZt2Iht6vKWV0DDn2TWfVevPPl5Du3QU+Y7lp3WmIe+GwLZjueViLnJIL46EmpMKXA1/s+zSTW9s1No5eSoVoRmYN6FH7Wds1xvMSn2JovV3OE7zo+DMLC69xLopScUHBKVeQQOLHVkLvqVepYGdQ0fg3tc17vxphP046OdSzdxxJ1OB8xNtK+dgH9nxaYBPZOCWb7Wutz8kWJaO8lziTLMHD6jfR77lkt2HmO0ENJb36REyfeWaCQ5cnlLnlhSy9MK5HH3P95dreEORsNZnNWBYN";
        public MainForm()
        {
            InitializeComponent();
        }

        HotKeyUtils hotKeyUtils = new HotKeyUtils();

        private void MainForm_Load(object sender, EventArgs e)
        {
            // ����Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(Path.Combine(Directory.GetCurrentDirectory(), "logs\\app_.log"), rollingInterval: RollingInterval.Day)
                .WriteTo.Debug()
                .CreateLogger();
                
            Log.Information("Ӧ�ó�������");
            HotKeyUtils.RegisterHotKey(Handle, 200, 0, Keys.Delete); // ע���ȼ�
            Log.Debug("��ע���ȼ�: Delete");
            LoadAccountList();
            RefreshDataGrid();
        }

        private void LoadAccountList()
        {
            try
            {
                Log.Debug("��ʼ�����˺��б������ļ�·��: {ConfigFilePath}", configFilePath);
                if (File.Exists(configFilePath))
                {
                    string json = File.ReadAllText(configFilePath);
                    accountList = JsonSerializer.Deserialize<List<GameAccountModel>>(json) ?? new List<GameAccountModel>();
                    Log.Information("�ɹ������˺��б��� {AccountCount} ���˺�", accountList.Count);
                }
                else
                {
                    // ����ʾ���˺�
                    Log.Information("�����ļ������ڣ�����ʾ���˺�");
                    accountList = new List<GameAccountModel>
                    {
                        new GameAccountModel { Account = "adad", Password = "adad", CharacterName = "sad13", IsMainControl = true },
                        new GameAccountModel { Account = "acac", Password = "acac", CharacterName = "sad14", IsMainControl = false }
                    };
                    SaveAccountList();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "�����˺��б�ʧ��");
                MessageBox.Show($"�����˺��б�ʧ��: {ex.Message}");
            }
        }

        private void SaveAccountList()
        {
            try
            {
                Log.Debug("��ʼ�����˺��б��� {AccountCount} ���˺�", accountList.Count);
                string json = JsonSerializer.Serialize(accountList, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(configFilePath, json);
                Log.Information("�˺��б���ɹ���·��: {ConfigFilePath}", configFilePath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "�����˺��б�ʧ��");
                MessageBox.Show($"�����˺��б�ʧ��: {ex.Message}");
            }
        }

        private void RefreshDataGrid()
        {
            Log.Debug("ˢ����������");
            // ���½���״̬
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
                        Log.Information("���� {ProcessId} �Ѳ����ڣ������˺� {Account} �Ľ���ID", account.ProcessId, account.Account);
                        account.ProcessId = null;
                    }
                }
            }

            // ���浱ǰ��˳���λ��
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
            
            // �ָ���˳���λ��
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

        // �޸�StartGameProcess������ʹ��PowerShell�ű�������Ϸ
        // �޸�StartGameProcess���������⸸�ӽ��̹�ϵ
        private void StartGameProcess(GameAccountModel account)
        {
            try
            {
                string arguments = $"{encodeArg}";
                // ʹ�� -PassThru ��ȡ���̶��󣬲����PID
                string psCommand = $"cd '{gameDirectory}'; $p=Start-Process -FilePath './{gamePath}' -ArgumentList '{arguments}' -NoNewWindow -PassThru; $p.Id";

                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{psCommand}\"",
                    UseShellExecute = false, // ����Ϊfalse�����ض������
                    RedirectStandardOutput = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                var process = System.Diagnostics.Process.Start(psi);
                string output = process.StandardOutput.ReadLine(); // ֻ��һ�м���
                // ����Ҫ WaitForExit
                if (int.TryParse(output.Trim(), out int pid))
                {
                    Log.Information("PowerShell��ֱ��������Ϸ���̣��˺�: {Account}, PID: {Pid}", account.Account, pid);
                    account.ProcessId = pid;
                    // ͨ��PID��ȡ���̶���
                    var gameProcess = Process.GetProcessById(pid);
                    // ������DLL���߼�
                    AttachToGameProcess(gameProcess, account);
                }
                else
                {
                    Log.Warning("δ�ܻ�ȡ���½���PID���˺�: {Account}", account.Account);
                    MessageBox.Show("�޷���ȡ�½���PID�����ֶ�������Ϸ��");
                }

                RefreshDataGrid();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "������Ϸ�����з����쳣���˺�: {Account}", account.Account);
                MessageBox.Show($"������Ϸʧ��: {ex.Message}");
            }
        }

        private void KillGameProcess(GameAccountModel account)
        {
            if (account.ProcessId.HasValue)
            {
                try
                {
                    Log.Information("׼���ر���Ϸ���̣��˺�: {Account}, PID: {ProcessId}", account.Account, account.ProcessId);
                    Process process = Process.GetProcessById(account.ProcessId.Value);
                    
                    // ����й����ĸ������ڣ��Ƚ���ҹ����ر�
                    if (GameInstances.ContainsKey(account.ProcessId.Value))
                    {
                        Log.Debug("���DLL�ҹ����رո�������");
                        var gameInstance = GameInstances[account.ProcessId.Value];
                        DllInject.Unhook(gameInstance);
                        gameInstance.AssistantForm?.Close();
                        GameInstances.Remove(account.ProcessId.Value);
                    }
                    
                    process.Kill();
                    account.ProcessId = null;
                    Log.Information("��Ϸ�����ѹرգ��˺�: {Account}", account.Account);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "�ر���Ϸ����ʧ�ܣ��˺�: {Account}", account.Account);
                    MessageBox.Show($"�ر���Ϸ����ʧ��: {ex.Message}");
                }
            }
        }

        private void RestartGameProcess(GameAccountModel account)
        {
            Log.Information("������Ϸ���̣��˺�: {Account}", account.Account);
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
                    
                    Log.Information("׼������Ϸ���̣��˺�: {Account}, PID: {ProcessId}", account.Account, pid);
                    
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
                        
                        // �����˺���Ϣ
                        gameInstance.AccountInfo = account;
                        
                        Log.Debug("����DLL����Ϸ����");
                        DllInject.loadDll(gameInstance);
                        
                        // ��������أ���ʾ��������
                        // if (account.IsMainControl)
                        // {
                       
                        // }
                        gameInstance.AssistantForm.Show();
                        gameInstance.AssistantForm.Location = new Point(rect.Left, rect.Top);
                        Log.Information("������������ʾ���˺�: {Account}", account.Account);
                        
                        gameInstance.AssistantForm.Disposed += (sender, args) =>
                        {
                            Log.Debug("���������ѹرգ��Ƴ���Ϸʵ����PID: {ProcessId}", gameInstance.MirPid);
                            GameInstances.Remove(gameInstance.MirPid);
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "����Ϸ����ʧ�ܣ��˺�: {Account}", account.Account);
                MessageBox.Show($"����Ϸ����ʧ��: {ex.Message}");
            }
        }

        protected override void WndProc(ref Message m)//����Windows��Ϣ
        {
            const int WM_HOTKEY = 0x0312; //���m.Msg��ֵΪ0x0312��ô��ʾ�û��������ȼ� 
            switch (m.Msg)
            {
                case WM_HOTKEY:
                    Log.Debug("���յ��ȼ���Ϣ");
                    HotKeyUtils.UnregisterHotKey(Handle, 200);
                    SendKeys.Send("{DEL}");
                    HotKeyUtils.RegisterHotKey(Handle, 200, 0, Keys.Delete); // ע���ȼ�
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
                            // ���Ҷ�Ӧ���˺�
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
            base.WndProc(ref m); //��ϵͳ��Ϣ�����Ը����WndProc 
        }

        private void dataGridViewAccounts_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // ����Ƿ�����"����"��ť
            if (e.ColumnIndex == colRestart.Index && e.RowIndex >= 0)
            {
                var account = accountList[e.RowIndex];
                RestartGameProcess(account);
            }
        }

        private void btnRestartAll_Click(object sender, EventArgs e)
        {
            Log.Information("����������Ϸ���̣��˺�����: {AccountCount}", accountList.Count);
            foreach (var account in accountList)
            {
                RestartGameProcess(account);
                // ����ӳ٣�����ͬʱ�����������
                Thread.Sleep(2000);
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Log.Information("Ӧ�ó������ڹر�");
            HotKeyUtils.UnregisterHotKey(Handle, 200);
            Log.Debug("��ע���ȼ�");
            SaveAccountList();
            
            Log.Debug("���ڽ������DLL�ҹ�����Ϸʵ������: {InstanceCount}", GameInstances.Count);
            Task.Run(() =>
            {
                foreach (var gameInstance in GameInstances.Values)
                {
                    DllInject.Unhook(gameInstance);
                }
            });
            Thread.Sleep(200);
            Log.Information("Ӧ�ó����ѹر�");
            Log.CloseAndFlush();
        }
    }
}
