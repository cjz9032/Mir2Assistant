namespace Mir2Assistant.TabForms.Demo
{
    partial class GoRunForm
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
            components = new System.ComponentModel.Container();
            groupBox1 = new GroupBox();
            radioButton3 = new RadioButton();
            radioButton2 = new RadioButton();
            radioButton1 = new RadioButton();
            button1 = new Button();
            button2 = new Button();
            button3 = new Button();
            button4 = new Button();
            button5 = new Button();
            button6 = new Button();
            button7 = new Button();
            button8 = new Button();
            button9 = new Button();
            timer1 = new System.Windows.Forms.Timer(components);
            label1 = new Label();
            label3 = new Label();
            textBox2 = new TextBox();
            label2 = new Label();
            buttonSeek = new Button();
            textBox1 = new TextBox();
            buttonTeleport = new Button();
            textBoxMapId = new TextBox();
            groupBox2 = new GroupBox();
            button11 = new Button();
            buttonGroup = new Button();
            textBoxMember = new TextBox();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(radioButton3);
            groupBox1.Controls.Add(radioButton2);
            groupBox1.Controls.Add(radioButton1);
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(200, 58);
            groupBox1.TabIndex = 2;
            groupBox1.TabStop = false;
            groupBox1.Text = "方式";
            // 
            // radioButton3
            // 
            radioButton3.AutoSize = true;
            radioButton3.Checked = true;
            radioButton3.Location = new Point(118, 22);
            radioButton3.Name = "radioButton3";
            radioButton3.Size = new Size(74, 21);
            radioButton3.TabIndex = 3;
            radioButton3.TabStop = true;
            radioButton3.Tag = "3";
            radioButton3.Text = "骑黑马跑";
            radioButton3.UseVisualStyleBackColor = true;
            // 
            // radioButton2
            // 
            radioButton2.AutoSize = true;
            radioButton2.Checked = true;
            radioButton2.Location = new Point(62, 22);
            radioButton2.Name = "radioButton2";
            radioButton2.Size = new Size(50, 21);
            radioButton2.TabIndex = 3;
            radioButton2.TabStop = true;
            radioButton2.Tag = "2";
            radioButton2.Text = "跑路";
            radioButton2.UseVisualStyleBackColor = true;
            // 
            // radioButton1
            // 
            radioButton1.AutoSize = true;
            radioButton1.Checked = true;
            radioButton1.Location = new Point(6, 22);
            radioButton1.Name = "radioButton1";
            radioButton1.Size = new Size(50, 21);
            radioButton1.TabIndex = 2;
            radioButton1.TabStop = true;
            radioButton1.Tag = "1";
            radioButton1.Text = "走路";
            radioButton1.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            button1.Location = new Point(12, 102);
            button1.Name = "button1";
            button1.Size = new Size(56, 49);
            button1.TabIndex = 3;
            button1.Text = "7";
            button1.UseVisualStyleBackColor = true;
            button1.Click += goRun;
            // 
            // button2
            // 
            button2.Location = new Point(87, 102);
            button2.Name = "button2";
            button2.Size = new Size(56, 49);
            button2.TabIndex = 3;
            button2.Text = "8";
            button2.UseVisualStyleBackColor = true;
            button2.Click += goRun;
            // 
            // button3
            // 
            button3.Location = new Point(156, 102);
            button3.Name = "button3";
            button3.Size = new Size(56, 49);
            button3.TabIndex = 3;
            button3.Text = "9";
            button3.UseVisualStyleBackColor = true;
            button3.Click += goRun;
            // 
            // button4
            // 
            button4.Location = new Point(12, 168);
            button4.Name = "button4";
            button4.Size = new Size(56, 49);
            button4.TabIndex = 3;
            button4.Text = "4";
            button4.UseVisualStyleBackColor = true;
            button4.Click += goRun;
            // 
            // button5
            // 
            button5.Location = new Point(87, 168);
            button5.Name = "button5";
            button5.Size = new Size(56, 49);
            button5.TabIndex = 3;
            button5.Text = "5";
            button5.UseVisualStyleBackColor = true;
            button5.Click += goRun;
            // 
            // button6
            // 
            button6.Location = new Point(156, 168);
            button6.Name = "button6";
            button6.Size = new Size(56, 49);
            button6.TabIndex = 3;
            button6.Text = "6";
            button6.UseVisualStyleBackColor = true;
            button6.Click += goRun;
            // 
            // button7
            // 
            button7.Location = new Point(12, 233);
            button7.Name = "button7";
            button7.Size = new Size(56, 49);
            button7.TabIndex = 3;
            button7.Text = "1";
            button7.UseVisualStyleBackColor = true;
            button7.Click += goRun;
            // 
            // button8
            // 
            button8.Location = new Point(87, 233);
            button8.Name = "button8";
            button8.Size = new Size(56, 49);
            button8.TabIndex = 3;
            button8.Text = "2";
            button8.UseVisualStyleBackColor = true;
            button8.Click += goRun;
            // 
            // button9
            // 
            button9.Location = new Point(156, 233);
            button9.Name = "button9";
            button9.Size = new Size(56, 49);
            button9.TabIndex = 3;
            button9.Text = "3";
            button9.UseVisualStyleBackColor = true;
            button9.Click += goRun;
            // 
            // timer1
            // 
            timer1.Interval = 150;
            timer1.Tick += timer1_Tick;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(16, 78);
            label1.Name = "label1";
            label1.Size = new Size(135, 17);
            label1.TabIndex = 4;
            label1.Text = "小键盘数字方向，5停止";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(90, 24);
            label3.Name = "label3";
            label3.Size = new Size(14, 17);
            label3.TabIndex = 5;
            label3.Text = "y";
            // 
            // textBox2
            // 
            textBox2.Location = new Point(106, 22);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(49, 23);
            textBox2.TabIndex = 6;
            textBox2.Text = "326";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(13, 24);
            label2.Name = "label2";
            label2.Size = new Size(14, 17);
            label2.TabIndex = 5;
            label2.Text = "x";
            // 
            // buttonSeek
            // 
            buttonSeek.Location = new Point(165, 21);
            buttonSeek.Name = "buttonSeek";
            buttonSeek.Size = new Size(61, 23);
            buttonSeek.TabIndex = 7;
            buttonSeek.Text = "寻路";
            buttonSeek.UseVisualStyleBackColor = true;
            buttonSeek.Click += buttonSeek_Click;
            // 
            // buttonTeleport
            // 
            buttonTeleport.Location = new Point(165, 51);
            buttonTeleport.Name = "buttonTeleport";
            buttonTeleport.Size = new Size(61, 23);
            buttonTeleport.TabIndex = 8;
            buttonTeleport.Text = "转图";
            buttonTeleport.UseVisualStyleBackColor = true;
            buttonTeleport.Click += buttonTeleport_Click;
            // 
            // textBoxMapId
            // 
            textBoxMapId.Location = new Point(29, 52);
            textBoxMapId.Name = "textBoxMapId";
            textBoxMapId.Size = new Size(49, 23);
            textBoxMapId.TabIndex = 9;
            textBoxMapId.Text = "0";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(29, 22);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(49, 23);
            textBox1.TabIndex = 6;
            textBox1.Text = "330";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(textBoxMapId);
            groupBox2.Controls.Add(buttonTeleport);
            groupBox2.Controls.Add(textBox1);
            groupBox2.Controls.Add(buttonSeek);
            groupBox2.Controls.Add(label2);
            groupBox2.Controls.Add(textBox2);
            groupBox2.Controls.Add(label3);
            groupBox2.Location = new Point(12, 284);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(242, 97);
            groupBox2.TabIndex = 8;
            groupBox2.TabStop = false;
            // 
            // button11
            // 
            button11.Location = new Point(241, 239);
            button11.Name = "button11";
            button11.Size = new Size(75, 36);
            button11.TabIndex = 9;
            button11.Text = "飞苍月";
            button11.UseVisualStyleBackColor = true;
            button11.Click += button11_Click;
            // 
            // buttonGroup
            // 
            buttonGroup.Location = new Point(348, 33);
            buttonGroup.Name = "buttonGroup";
            buttonGroup.Size = new Size(60, 23);
            buttonGroup.TabIndex = 13;
            buttonGroup.Text = "组队";
            buttonGroup.UseVisualStyleBackColor = true;
            buttonGroup.Click += buttonGroup_Click;
            // 
            // textBoxMember
            // 
            textBoxMember.Location = new Point(262, 34);
            textBoxMember.Name = "textBoxMember";
            textBoxMember.Size = new Size(80, 23);
            textBoxMember.TabIndex = 12;
            textBoxMember.Text = "sad13";
            // 
            // GoRunForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(458, 398);
            Controls.Add(button11);
            Controls.Add(groupBox2);
            Controls.Add(label1);
            Controls.Add(button9);
            Controls.Add(button6);
            Controls.Add(button3);
            Controls.Add(button8);
            Controls.Add(button5);
            Controls.Add(button2);
            Controls.Add(button7);
            Controls.Add(button4);
            Controls.Add(button1);
            Controls.Add(groupBox1);
            Controls.Add(buttonGroup);
            Controls.Add(textBoxMember);
            Name = "GoRunForm";
            Text = "GoRun";
            Load += GoRun_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private GroupBox groupBox1;
        private RadioButton radioButton3;
        private RadioButton radioButton2;
        private RadioButton radioButton1;
        private Button button1;
        private Button button2;
        private Button button3;
        private Button button4;
        private Button button5;
        private Button button6;
        private Button button7;
        private Button button8;
        private Button button9;
        private System.Windows.Forms.Timer timer1;
        private Label label1;
        private Label label3;
        private TextBox textBox2;
        private Label label2;
        private Button buttonSeek;
        private TextBox textBox1;
        private Button buttonTeleport;
        private TextBox textBoxMapId;
        private GroupBox groupBox2;
        private Button button11;
        private Button buttonGroup;   // 新增
        private TextBox textBoxMember; // 新增
    }
}