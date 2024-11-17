using Mir2Assistant.Functions;
using Mir2Assistant.Models;
using Mir2Assistant.TabForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mir2Assistant
{
    public partial class AssiastantForm : Form
    {
        public int pid; //游戏pid
        public IntPtr baseAddress;
        public IntPtr hwnd;//游戏窗体句柄

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public int 角色基址 = 0x87AD6C;


        public MemoryUtils? memoryUtils;

        public CharacerStatusModel CharacerStatus = new CharacerStatusModel();
        public AssiastantForm()
        {
            InitializeComponent();
        }

        private void AssiastantForm_Load(object sender, EventArgs e)
        {
            memoryUtils = new MemoryUtils(pid);

            var assembly = Assembly.GetExecutingAssembly();
            var forms = assembly.GetTypes().Where(o => o.IsClass && o.IsAssignableTo(typeof(ITabForm)));
            foreach (var form in forms)
            {
                var tp = new TabPage();
                var fm = (Form)assembly.CreateInstance(form.FullName!)!;
                tp.Text = ((ITabForm)fm).Title;
                ((ITabForm)fm).AssiastantForm = this;
                fm.FormBorderStyle = FormBorderStyle.None;
                fm.TopLevel = false;
                fm.Parent = tp;
                fm.ControlBox = false;
                fm.Dock = DockStyle.Fill;
                fm.Show();
                tabControl.TabPages.Add(tp);
            }

            Task.Run(() =>
            {
                try
                {
                    while (true)
                    {
                        if (cancellationTokenSource.Token.IsCancellationRequested)
                        {
                            cancellationTokenSource.Token.ThrowIfCancellationRequested();
                            break;
                        }
                        CharacerStatusFunction.GetInfo(this);
                        Thread.Sleep(100);
                    }
                }
                catch (Exception ex)
                {
                    //将异常抛给主线程
                    this.BeginInvoke(new EventHandler(delegate { throw ex; }));
                }
            });
        }
    }
}
