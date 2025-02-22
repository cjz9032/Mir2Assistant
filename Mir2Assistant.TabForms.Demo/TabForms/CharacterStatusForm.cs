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

        public CharacterStatusForm()
        {
            InitializeComponent();
        }

        private void CharacterStatusForm_Load(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            var info = GameInstance!.CharacterStatus;
            label1.Text=$"{info!.Name}\n{info.MapName}：{info.X} {info.Y}\n血：{info.CurrentHP}/{info.MaxHP}\n蓝：{info.CurrentMP}/{info.MaxMP}\n转生等级：{info.GradeZS}";
        }
    }
}
