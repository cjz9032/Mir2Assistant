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
    /// 读怪，打怪
    /// </summary>
    [Order(3)]
    public partial class MonsterForm : Form, ITabForm
    {
        public MonsterForm()
        {
            InitializeComponent();
        }

        public string Title => "读怪、打怪";

        public MirGameInstanceModel? GameInstance { get; set; }

        private ObservableCollection<MonsterModel> monsters = new ObservableCollection<MonsterModel>();
        private ObservableCollection<SkillModel> skills = new ObservableCollection<SkillModel>();

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItems.Count > 0)
            {
                var monster = listBox1.SelectedItems?[0] as MonsterModel;
                if (monster != null)
                {
                    MonsterFunction.SlayingMonster(GameInstance!, monster.Addr);
                }
            }
        }

        private void MonsterForm_Load(object sender, EventArgs e)
        {
            //bindingSource1.DataSource = monsters;
           
            bindingSource2.DataSource = skills;
            listBox1.DataSource = bindingSource1;
            listBox1.DisplayMember = "Display";
            listBox2.DataSource = bindingSource2;
            listBox2.DisplayMember = "Display";

        }

        private void button2_Click(object sender, EventArgs e)
        {
            var memoryUtils = GameInstance!.memoryUtils!;
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
            listBox1.BeginUpdate();
            bindingSource1.DataSource = GameInstance!.Monsters.Values.Where(o => o.TypeStr != "NPC").OrderBy(o => o.X).ThenBy(o => o.Y);
            listBox1.EndUpdate();
            //monsters.Clear();
            //foreach (var item in GameInstance!.Monsters.Values.Where(o => o.TypeStr != "NPC").OrderBy(o => o.X).ThenBy(o => o.Y))
            //{
            //    monsters.Add(item);
            //}


            //bindingSource1.ResetBindings(false);

            listBox1.TopIndex = scrollOffset;
            if (skills.Count != GameInstance.Skills.Count)
            {
                skills.Clear();
                foreach (var item in GameInstance!.Skills)
                {
                    skills.Add(item);
                }
                bindingSource2.ResetBindings(false);
            }

        }

        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            var name = (listBox1.Items[e.Index] as MonsterModel)?.Name;
            var text = (listBox1.Items[e.Index] as MonsterModel)?.Display;
            if (name == "沙漠霸主")
            {
                e.Graphics.DrawString(text, e.Font, Brushes.Red, e.Bounds);
            }
            else
            {
                e.Graphics.DrawString(text, e.Font, Brushes.Black, e.Bounds);
            }
        }
    }
}
