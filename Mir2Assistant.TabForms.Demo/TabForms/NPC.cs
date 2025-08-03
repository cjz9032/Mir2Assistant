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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mir2Assistant.TabForms.Demo.TabForms
{
    /// <summary>
    /// NPC
    /// </summary>
    [Order(4)]
    public partial class NPCForm : Form, ITabForm
    {
        public NPCForm()
        {
            InitializeComponent();
        }

        public string Title => "NPC";

        public MirGameInstanceModel? GameInstance { get; set; }

        private ObservableCollection<MonsterModel> npcs = new ObservableCollection<MonsterModel>();
        private ObservableCollection<SkillModel> skills = new ObservableCollection<SkillModel>();

        private async void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItems.Count > 0)
            {
                var npc = listBox1.SelectedItems?[0] as MonsterModel;
                if (npc != null)
                {
                    listBox2.Items.Clear();
                    textBox1.Text = await NpcFunction.ClickNPC(GameInstance!, npc);
                    NpcFunction.GetTalkCmds(textBox1.Text).ForEach(cmd =>
                    {
                        listBox2.Items.Add(cmd);
                    });

                }
            }
        }

        private void NPCForm_Load(object sender, EventArgs e)
        {
            bindingSource1.DataSource = npcs;
            bindingSource2.DataSource = skills;
            listBox1.DataSource = bindingSource1;
            listBox1.DisplayMember = "Display";
            
            // 添加双击事件处理复制功能
            listBox1.DoubleClick += (s, args) => {
                if (listBox1.SelectedItem != null)
                {
                    var npc = listBox1.SelectedItem as MonsterModel;
                    if (npc != null)
                    {
                        Clipboard.SetText(npc.Display);
                    }
                }
            };
            
            GameInstance!.NewSysMsg += (flag, str) => this.Invoke(() => textBox2.Text = str);
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            var memoryUtils = GameInstance!.memoryUtils!;


            var scrollOffset = listBox1.TopIndex;
            npcs.Clear();
            foreach (var item in GameInstance!.Monsters.Values.Where(o => o.TypeStr == "NPC"))
            {
                npcs.Add(item);
            }
            bindingSource1.ResetBindings(false);
            listBox1.TopIndex = scrollOffset;


        }

        private async void button3_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(listBox2.SelectedItem?.ToString()))
            {
                textBox1.Text = await NpcFunction.Talk2(GameInstance!, listBox2.SelectedItem.ToString()!.Split('/')[1]);
                listBox2.Items.Clear();
                NpcFunction.GetTalkCmds(textBox1.Text).ForEach(cmd =>
                {
                    listBox2.Items.Add(cmd);
                });
            }

        }

        private async void button2_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox3.Text))
            {
                NpcFunction.BuyImmediate(GameInstance!, textBox3.Text);
            }
        }
    }
}
