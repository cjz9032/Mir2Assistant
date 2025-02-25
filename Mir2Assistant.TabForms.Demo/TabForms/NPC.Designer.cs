namespace Mir2Assistant.TabForms.Demo.TabForms
{
    partial class NPCForm
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
            button1 = new Button();
            listBox1 = new ListBox();
            bindingSource1 = new BindingSource(components);
            listBox2 = new ListBox();
            bindingSource2 = new BindingSource(components);
            timer1 = new System.Windows.Forms.Timer(components);
            textBox1 = new TextBox();
            button3 = new Button();
            textBox2 = new TextBox();
            ((System.ComponentModel.ISupportInitialize)bindingSource1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)bindingSource2).BeginInit();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            button1.Location = new Point(63, 234);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 0;
            button1.Text = "点NPC";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // listBox1
            // 
            listBox1.FormattingEnabled = true;
            listBox1.ItemHeight = 17;
            listBox1.Location = new Point(1, 3);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(249, 140);
            listBox1.TabIndex = 1;
            // 
            // listBox2
            // 
            listBox2.FormattingEnabled = true;
            listBox2.ItemHeight = 17;
            listBox2.Location = new Point(256, 3);
            listBox2.Name = "listBox2";
            listBox2.Size = new Size(225, 140);
            listBox2.TabIndex = 4;
            // 
            // timer1
            // 
            timer1.Enabled = true;
            timer1.Interval = 1000;
            timer1.Tick += timer1_Tick;
            // 
            // textBox1
            // 
            textBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            textBox1.Location = new Point(1, 149);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.ScrollBars = ScrollBars.Vertical;
            textBox1.Size = new Size(480, 79);
            textBox1.TabIndex = 5;
            // 
            // button3
            // 
            button3.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            button3.Location = new Point(292, 234);
            button3.Name = "button3";
            button3.Size = new Size(75, 23);
            button3.TabIndex = 0;
            button3.Text = "二级对话";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // textBox2
            // 
            textBox2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            textBox2.Location = new Point(1, 263);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(480, 23);
            textBox2.TabIndex = 6;
            // 
            // NPCForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(482, 288);
            Controls.Add(textBox2);
            Controls.Add(textBox1);
            Controls.Add(listBox2);
            Controls.Add(listBox1);
            Controls.Add(button3);
            Controls.Add(button1);
            Name = "NPCForm";
            Text = "NPCForm";
            Load += NPCForm_Load;
            ((System.ComponentModel.ISupportInitialize)bindingSource1).EndInit();
            ((System.ComponentModel.ISupportInitialize)bindingSource2).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private ListBox listBox1;
        private BindingSource bindingSource1;
        private ListBox listBox2;
        private BindingSource bindingSource2;
        private System.Windows.Forms.Timer timer1;
        private TextBox textBox1;
        private Button button3;
        private TextBox textBox2;
    }
}