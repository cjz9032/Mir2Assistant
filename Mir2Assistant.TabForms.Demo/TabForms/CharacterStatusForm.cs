using Mir2Assistant.Common.Models;
using Mir2Assistant.Common.TabForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mir2Assistant.TabForms.Demo
{
    /// <summary>
    /// 角色状态
    /// </summary>
    [Order(1)]
    public partial class CharacterStatusForm : Form, ITabForm
    {
        public string Title => "角色状态";
        public MirGameInstanceModel? GameInstance { get; set; }
        private ObservableCollection<ItemModel> items = new ObservableCollection<ItemModel>();
        private BindingSource bindingSource1 = new BindingSource();

        public CharacterStatusForm()
        {
            InitializeComponent();
            button1.Click += button1_Click;
        }

        private void CharacterStatusForm_Load(object sender, EventArgs e)
        {
            bindingSource1.DataSource = items;
            listBox1.DataSource = bindingSource1;
            listBox1.DisplayMember = "Display";
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            var info = GameInstance!.CharacterStatus;
            label1.Text=$"{info!.Name}\n{info.MapName}：{info.X} {info.Y} 血：{info.CurrentHP}/{info.MaxHP} 蓝：{info.CurrentMP}/{info.MaxMP}";
            
            var currentItems = GameInstance!.CharacterStatus!.useItems.Where(o => o != null).ToList();
            items.Clear();
            foreach (var item in currentItems)
            {
                items.Add(item);
            }
            bindingSource1.DataSource = items;
            bindingSource1.ResetBindings(false);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // 脱按钮空实现
        }
    }
}
