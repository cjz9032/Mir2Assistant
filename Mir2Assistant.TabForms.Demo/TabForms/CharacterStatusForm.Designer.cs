namespace Mir2Assistant.TabForms.Demo
{
    partial class CharacterStatusForm
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
            statusLabel = new Label();
            timer1 = new System.Windows.Forms.Timer(components);
            useItemsListBox = new ListBox();
            buttonTakeOff = new Button();
            SuspendLayout();
            // 
            // statusLabel
            // 
            statusLabel.AutoSize = true;
            statusLabel.Location = new Point(8, 8);
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(43, 17);
            statusLabel.TabIndex = 0;
            statusLabel.Text = "statusLabel";
            // 
            // timer1
            // 
            timer1.Enabled = true;
            timer1.Tick += timer1_Tick;
            // 
            // useItemsListBox
            // 
            useItemsListBox.FormattingEnabled = true;
            useItemsListBox.HorizontalScrollbar = true;
            useItemsListBox.ItemHeight = 17;
            useItemsListBox.Location = new Point(8, 68);
            useItemsListBox.Name = "useItemsListBox";
            useItemsListBox.SelectionMode = SelectionMode.MultiExtended;
            useItemsListBox.Size = new Size(319, 276);
            useItemsListBox.TabIndex = 1;
            // 
            // buttonTakeOff
            // 
            buttonTakeOff.Location = new Point(333, 68);
            buttonTakeOff.Name = "buttonTakeOff";
            buttonTakeOff.Size = new Size(75, 23);
            buttonTakeOff.TabIndex = 2;
            buttonTakeOff.Text = "脱";
            buttonTakeOff.UseVisualStyleBackColor = true;

            buttonButch = new System.Windows.Forms.Button();
            // 
            // buttonButch
            // 
            buttonButch.Location = new Point(333, 97);
            buttonButch.Name = "buttonButch";
            buttonButch.Size = new Size(75, 23);
            buttonButch.TabIndex = 3;
            buttonButch.Text = "屠";
            buttonButch.UseVisualStyleBackColor = true;

            buttonPickUp = new System.Windows.Forms.Button();
            // 
            // buttonPickUp
            // 
            buttonPickUp.Location = new Point(333, 126);
            buttonPickUp.Name = "buttonPickUp";
            buttonPickUp.Size = new Size(75, 23);
            buttonPickUp.TabIndex = 4;
            buttonPickUp.Text = "捡取";
            buttonPickUp.UseVisualStyleBackColor = true;

            buttonHit = new System.Windows.Forms.Button();
            // 
            // buttonHit
            // 
            buttonHit.Location = new Point(333, 155);
            buttonHit.Name = "buttonHit";
            buttonHit.Size = new Size(75, 23);
            buttonHit.TabIndex = 5;
            buttonHit.Text = "打";
            buttonHit.UseVisualStyleBackColor = true;
            buttonHit.Click += buttonHit_Click;
            // 
            // CharacterStatusForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(413, 361);
            Controls.Add(buttonTakeOff);
            Controls.Add(buttonButch);
            Controls.Add(buttonPickUp);
            Controls.Add(buttonHit);
            Controls.Add(useItemsListBox);
            Controls.Add(statusLabel);
            Name = "CharacterStatusForm";
            Text = "CharacterStatusForm";
            Load += CharacterStatusForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label statusLabel;
        private System.Windows.Forms.Timer timer1;
        private ListBox useItemsListBox;
        private Button buttonTakeOff;
        private Button buttonButch;
        private Button buttonPickUp;
        private Button buttonHit;
    }
}