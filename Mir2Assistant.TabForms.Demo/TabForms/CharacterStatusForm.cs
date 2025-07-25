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
using Mir2Assistant.Common.Functions;
using Mir2Assistant.Common.Utils;

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
        private BindingSource useItemSource = new BindingSource();
        private List<ItemModel> GetSelectedItems()
        {
            return useItemsListBox.SelectedItems.Cast<ItemModel>().ToList();
        }
        public CharacterStatusForm()
        {
            InitializeComponent();
            buttonTakeOff.Click += buttonTakeOff_Click;
            buttonButch.Click += buttonButch_Click;
            buttonHit.Click += buttonHit_Click;
        }

        private void CharacterStatusForm_Load(object sender, EventArgs e)
        {
            useItemSource.DataSource = items;
            useItemsListBox.DataSource = useItemSource;
            useItemsListBox.DisplayMember = "Display";
        }
        private HashSet<int> lastItemIds = new HashSet<int>();

        private void timer1_Tick(object sender, EventArgs e)
        {
            var info = GameInstance!.CharacterStatus;
            statusLabel.Text=$"{info!.Name} {info.MapName}[{info.MapId}] --> {info.X}, {info.Y} \n 血：{info.CurrentHP}/{info.MaxHP} 蓝：{info.CurrentMP}/{info.MaxMP} L {info.Level}";
            
            var currentItems = GameInstance!.CharacterStatus!.useItems.Where(o => o != null).ToList();
            var currentIds = new HashSet<int>(currentItems.Select(item => item.Id));

            if (!currentIds.SetEquals(lastItemIds))
            {
                var scrollOffset = useItemsListBox.TopIndex;
                var selectedIndices = useItemsListBox.SelectedIndices.Cast<int>().ToList();
                useItemsListBox.BeginUpdate();
                
                items.Clear();
                foreach (var item in currentItems)
                {
                    items.Add(item);
                }
                
                useItemSource.DataSource = items;
                useItemSource.ResetBindings(false);

                useItemsListBox.EndUpdate();
                useItemsListBox.TopIndex = scrollOffset;
                
                //foreach (var index in selectedIndices)
                //{
                //    if (index < useItemsListBox.Items.Count)
                //    {
                //        useItemsListBox.SetSelected(index, true);
                //    }
                //}

                lastItemIds = currentIds;
            }
        }

        private async void buttonTakeOff_Click(object sender, EventArgs e)
        {
            SendMirCall.Send(GameInstance!, 9010, new nint[] {  });
            await Task.Delay(500);
            var selectedItems = GetSelectedItems();
            foreach (var item in selectedItems)
            {
                var data = Common.Utils.StringUtils.GenerateMixedData(
                    item.Name,    
                    item.Index, 
                    item.Id   
                );
                SendMirCall.Send(GameInstance!, 3022, data);
                await Task.Delay(200);
                // var addr = GameInstance!.MemoryUtils!.GetMemoryAddress(item.addr);
                GameInstance!.MemoryUtils!.WriteByte(item.addr, 0);
                
                await Task.Delay(700);
            }

            await Task.Delay(500);
            SendMirCall.Send(GameInstance!, 9010, new nint[] {  });
        }
        private void buttonButch_Click(object sender, EventArgs e)
        {
            // 寻找旁边的isDead 并且没有被屠宰的怪物, 还需要距离<=2格
            var currentX = GameInstance.CharacterStatus.X;
            var currentY = GameInstance.CharacterStatus.Y;
            
            var nearbyMonsters = GameInstance.Monsters
                .Where(m => m.Value.isDead && !m.Value.isButched)
                .Where(m => Math.Abs((short)(m.Value.X - currentX)) <= 2 && Math.Abs((short)(m.Value.Y - currentY)) <= 2)
                .ToList();
            if (nearbyMonsters.Count > 0)
            {
                // 固定的dir 0 就可以
                var first = nearbyMonsters.First();
                SendMirCall.Send(GameInstance!, 3030, new nint[] { (nint)first.Value.X, (nint)first.Value.Y, 0, first.Value.Id });
            }
        }

        private void buttonHit_Click(object sender, EventArgs e)
        {
            // Empty implementation for the Hit button click event
        }
    }
}
