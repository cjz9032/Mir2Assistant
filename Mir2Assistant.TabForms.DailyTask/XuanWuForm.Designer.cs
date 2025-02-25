namespace Mir2Assistant.TabForms.DailyTask
{
    partial class XuanWuForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            button1 = new Button();
            button2 = new Button();
            checkBox1 = new CheckBox();
            listBox1 = new ListBox();
            button3 = new Button();
            button4 = new Button();
            button5 = new Button();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(12, 294);
            button1.Name = "button1";
            button1.Size = new Size(91, 25);
            button1.TabIndex = 0;
            button1.Text = "手动玄武任务";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.Location = new Point(119, 294);
            button2.Name = "button2";
            button2.Size = new Size(82, 25);
            button2.TabIndex = 1;
            button2.Text = "结束任务";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(229, 298);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(75, 21);
            checkBox1.TabIndex = 2;
            checkBox1.Text = "暂停任务";
            checkBox1.UseVisualStyleBackColor = true;
            checkBox1.CheckedChanged += checkBox1_CheckedChanged;
            // 
            // listBox1
            // 
            listBox1.FormattingEnabled = true;
            listBox1.ItemHeight = 17;
            listBox1.Items.AddRange(new object[] { "噬魂毒牙", "幽冥毒牙", "圣山竹鼠", "赤狐劫掠者", "赤狐弩手", "白狐刀客", "浪客帮拳师", "浪客帮香女", "浪客帮斧手", "黑狐滚刀手", "浪客帮舵主", "狱焰巨蛛", "野狐统领", "圣域猎人", "时红夜蛇", "虎头海雕" });
            listBox1.Location = new Point(12, 12);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(91, 276);
            listBox1.TabIndex = 3;
            // 
            // button3
            // 
            button3.Location = new Point(221, 227);
            button3.Name = "button3";
            button3.Size = new Size(83, 35);
            button3.TabIndex = 4;
            button3.Text = "飞苍月";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // button4
            // 
            button4.Location = new Point(119, 227);
            button4.Name = "button4";
            button4.Size = new Size(83, 35);
            button4.TabIndex = 4;
            button4.Text = "回安全区";
            button4.UseVisualStyleBackColor = true;
            button4.Click += button4_Click;
            // 
            // button5
            // 
            button5.Location = new Point(119, 100);
            button5.Name = "button5";
            button5.Size = new Size(185, 90);
            button5.TabIndex = 5;
            button5.Text = "全自动玄武任务";
            button5.UseVisualStyleBackColor = true;
            button5.Click += button5_Click;
            // 
            // XuanWuForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(459, 345);
            Controls.Add(button5);
            Controls.Add(button4);
            Controls.Add(button3);
            Controls.Add(listBox1);
            Controls.Add(checkBox1);
            Controls.Add(button2);
            Controls.Add(button1);
            Name = "XuanWuForm";
            Text = "XuanWuForm";
            Load += XuanWuForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private Button button2;
        private CheckBox checkBox1;
        private ListBox listBox1;
        private Button button3;
        private Button button4;
        private Button button5;
    }
}