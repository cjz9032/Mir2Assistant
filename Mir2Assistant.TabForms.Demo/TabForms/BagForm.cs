using Mir2Assistant.Common.Functions;
using Mir2Assistant.Common.Models;
using Mir2Assistant.Common.TabForms;
using Mir2Assistant.Common.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mir2Assistant.TabForms.Demo.TabForms
{
    /// <summary>
    /// 读背包
    /// </summary>
    [Order(4)]
    public partial class BagForm : Form, ITabForm
    {
        public BagForm()
        {
            InitializeComponent();
        }

        public string Title => "背包列表";

        public MirGameInstanceModel? GameInstance { get; set; }

        private ObservableCollection<ItemModel> items = new ObservableCollection<ItemModel>();

        private void BagForm_Load(object sender, EventArgs e)
        {
            bindingSource1.DataSource = items;
            listBox1.DataSource = bindingSource1;
            listBox1.DisplayMember = "Display";
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            var scrollOffset = listBox1.TopIndex;
            listBox1.BeginUpdate();
            bindingSource1.DataSource = GameInstance!.Items.Values.Where(o => !o.IsEmpty).OrderBy(o => o.Index);
            listBox1.EndUpdate();
            listBox1.TopIndex = scrollOffset;
        }
    }
}