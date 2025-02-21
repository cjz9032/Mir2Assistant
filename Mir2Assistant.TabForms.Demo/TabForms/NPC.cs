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
    /// NPC
    /// </summary>
    [Order(3)]
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

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItems.Count > 0)
            {
                var npc = listBox1.SelectedItems?[0] as MonsterModel;
                if (npc != null)
                {
                    NpcFunction.ClickNPC(GameInstance!, npc);
                }
            }
        }

        private void NPCForm_Load(object sender, EventArgs e)
        {
            bindingSource1.DataSource = npcs;
            bindingSource2.DataSource = skills;
            listBox1.DataSource = bindingSource1;
            listBox1.DisplayMember = "Display";
            listBox2.DataSource = bindingSource2;
            listBox2.DisplayMember = "Display";

        }

        private void button2_Click(object sender, EventArgs e)
        {
            var memoryUtils = GameInstance!.MemoryUtils!;
            if (listBox1.SelectedItems.Count > 0)
            {
                var monster = listBox1.SelectedItems?[0] as MonsterModel;
                if (monster != null)
                {
                    MonsterFunction.LockMonster(GameInstance!, monster.Addr);
                }
            }
            if (listBox2.SelectedItems.Count > 0)
            {
                var skill = listBox2.SelectedItems[0] as SkillModel;
                if (skill != null)
                {
                    SkillFunction.SkillCall(GameInstance, skill.Addr!.Value);

                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            var scrollOffset = listBox1.TopIndex;
            npcs.Clear();
            foreach (var item in GameInstance!.Monsters.Where(o => o.TypeStr == "NPC"))
            {
                npcs.Add(item);
            }
            bindingSource1.ResetBindings(false);
            listBox1.TopIndex = scrollOffset;


        }
    }
}
