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
using Serilog; // 新增Serilog引用
using Serilog.Sinks.Debug; // 添加Debug sink引用

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
        private HashSet<int> lastItemIds = new HashSet<int>();

        private List<ItemModel> GetSelectedItems()
        {
            return listBox1.SelectedItems.Cast<ItemModel>().ToList();
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
           
            
            var selectedItems = GetSelectedItems();
            foreach (var item in selectedItems)
            {
                var data = Common.Utils.StringUtils.GenerateMixedData(
                    item.Name,
                    item.Id
                );

                SendMirCall.Send(GameInstance!, 3015, data);
                await Task.Delay(200);
            }

            await Task.Delay(500);
            SendMirCall.Send(GameInstance!, 9010, new nint[] { 1 });

        }

        private async void btnTake_Click(object sender, EventArgs e)
        {
            await NpcFunction.Talk2(GameInstance!, "@storage");
            // SendMirCall.Send(GameInstance!, 9010, new nint[] { 1 });
        }

        private async void btnSell_Click(object sender, EventArgs e)
        {


            var selectedItems = GetSelectedItems();
            foreach (var item in selectedItems)
            {
                var data = Common.Utils.StringUtils.GenerateMixedData(
                    item.Name,
                    item.Id
                );

                SendMirCall.Send(GameInstance!, 3011, data);
                await Task.Delay(500);
            }

            await Task.Delay(500);
            SendMirCall.Send(GameInstance!, 9010, new nint[] { });
        }

        private async void btnRepair_Click(object sender, EventArgs e)
        {

            var selectedItems = GetSelectedItems();
            foreach (var item in selectedItems) {
                var data = Common.Utils.StringUtils.GenerateMixedData(
                    item.Name,
                    item.Id
                );

                SendMirCall.Send(GameInstance!, 3012, data);
                await Task.Delay(200);
            }

            await Task.Delay(500);
            SendMirCall.Send(GameInstance!, 9010, new nint[] { });
        }

        private async void btnTakeOn_Click(object sender, EventArgs e)
        {
            SendMirCall.Send(GameInstance!, 9010, new nint[] {  });
            await Task.Delay(500);
            var selectedItems = GetSelectedItems();
            foreach (var item in selectedItems)
            {
               
                // 先随便 左手了
                byte toIndex = item.stdModeToUseItemIndex[0];
                if(toIndex == -1)
                {
                    Log.Error($"物品{item.Name}无法装备");
                    continue;
                }
      
                var data = Common.Utils.StringUtils.GenerateMixedData(
                    item.Name,    
                    toIndex, 
                    item.Id   
                );
                SendMirCall.Send(GameInstance!, 3023, data);
            }

            await Task.Delay(500);
            SendMirCall.Send(GameInstance!, 9010, new nint[] { });
        }

        private void BagForm_Load(object sender, EventArgs e)
        {
            bindingSource1.DataSource = items;
            listBox1.DataSource = bindingSource1;
            listBox1.DisplayMember = "Display";
            btnSave.Click += btnSave_Click;
            btnTake.Click += btnTake_Click;
            btnSell.Click += btnSell_Click;
            btnRepair.Click += btnRepair_Click;
            btnTakeOn.Click += btnTakeOn_Click;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            var currentItems = GameInstance!.Items.Where(o => !o.IsEmpty).OrderBy(o => o.Index).ToList();
            var currentIds = new HashSet<int>(currentItems.Select(item => item.Id));

            if (!currentIds.SetEquals(lastItemIds))
            {
                var scrollOffset = listBox1.TopIndex;
                var selectedIndices = listBox1.SelectedIndices.Cast<int>().ToList();
                listBox1.BeginUpdate();
                
                items.Clear();
                foreach (var item in currentItems)
                {
                    items.Add(item);
                }
                
                bindingSource1.DataSource = items;
                bindingSource1.ResetBindings(false);

                listBox1.EndUpdate();
                listBox1.TopIndex = scrollOffset;
                
                foreach (var index in selectedIndices)
                {
                    if (index < listBox1.Items.Count)
                    {
                        listBox1.SetSelected(index, true);
                    }
                }

                lastItemIds = currentIds;
            }
        }
    }
}