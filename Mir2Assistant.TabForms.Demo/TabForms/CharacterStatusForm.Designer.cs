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
            // 
            // CharacterStatusForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(413, 361);
            Controls.Add(buttonTakeOff);
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
    }
}