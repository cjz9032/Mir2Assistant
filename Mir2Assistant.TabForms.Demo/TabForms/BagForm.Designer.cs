
namespace Mir2Assistant.TabForms.Demo.TabForms
{
    partial class BagForm
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
            listBox1 = new ListBox();
            bindingSource1 = new BindingSource(components);
            timer1 = new System.Windows.Forms.Timer(components);
            btnSave = new Button();
            btnTake = new Button();
            btnSell = new Button();
            btnRepair = new Button();
            btnTakeOn = new Button();
            ((System.ComponentModel.ISupportInitialize)bindingSource1).BeginInit();
            SuspendLayout();
            // 
            // listBox1
            // 
            listBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listBox1.FormattingEnabled = true;
            listBox1.ItemHeight = 17;
            listBox1.Location = new Point(1, 30);
            listBox1.Name = "listBox1";
            listBox1.SelectionMode = SelectionMode.MultiExtended;
            listBox1.Size = new Size(480, 412);
            listBox1.TabIndex = 0;
            // 
            // timer1
            // 
            timer1.Enabled = true;
            timer1.Interval = 1000;
            timer1.Tick += timer1_Tick;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(1, 3);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(75, 23);
            btnSave.TabIndex = 1;
            btnSave.Text = "存";
            btnSave.UseVisualStyleBackColor = true;
            // 
            // btnTake
            // 
            btnTake.Location = new Point(356, 3);
            btnTake.Name = "btnTake";
            btnTake.Size = new Size(75, 23);
            btnTake.TabIndex = 1;
            btnTake.Text = "取(test)";
            btnTake.UseVisualStyleBackColor = true;
            // 
            // btnSell
            // 
            btnSell.Location = new Point(82, 3);
            btnSell.Name = "btnSell";
            btnSell.Size = new Size(75, 23);
            btnSell.TabIndex = 2;
            btnSell.Text = "卖";
            btnSell.UseVisualStyleBackColor = true;
            // 
            // btnRepair
            // 
            btnRepair.Location = new Point(163, 3);
            btnRepair.Name = "btnRepair";
            btnRepair.Size = new Size(75, 23);
            btnRepair.TabIndex = 3;
            btnRepair.Text = "修";
            btnRepair.UseVisualStyleBackColor = true;
            // 
            // btnTakeOn
            // 
            btnTakeOn.Location = new Point(244, 3);
            btnTakeOn.Name = "btnTakeOn";
            btnTakeOn.Size = new Size(75, 23);
            btnTakeOn.TabIndex = 4;
            btnTakeOn.Text = "穿";
            btnTakeOn.UseVisualStyleBackColor = true;
            // 
            // BagForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(482, 450);
            Controls.Add(btnSell);
            Controls.Add(btnRepair);
            Controls.Add(btnTakeOn);
            Controls.Add(btnSave);
            Controls.Add(btnTake);
            Controls.Add(listBox1);
            Name = "BagForm";
            Text = "BagForm";
            Load += BagForm_Load;
            ((System.ComponentModel.ISupportInitialize)bindingSource1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private ListBox listBox1;
        private BindingSource bindingSource1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnTake;
        private System.Windows.Forms.Button btnSell;
        private System.Windows.Forms.Button btnRepair;
        private System.Windows.Forms.Button btnTakeOn;
    }
}