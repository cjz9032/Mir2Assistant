namespace Mir2Assistant
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            dataGridViewAccounts = new DataGridView();
            colAccount = new DataGridViewTextBoxColumn();
            colPassword = new DataGridViewTextBoxColumn();
            colCharName = new DataGridViewTextBoxColumn();
            colIsMainControl = new DataGridViewCheckBoxColumn();
            colPid = new DataGridViewTextBoxColumn();
            colRestart = new DataGridViewButtonColumn();
            btnRestartAll = new Button();
            btnRestartTask = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridViewAccounts).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 15);
            label1.Name = "label1";
            label1.Size = new Size(99, 17);
            label1.TabIndex = 0;
            label1.Text = "游戏内Del键呼出";
            // 
            // dataGridViewAccounts
            // 
            dataGridViewAccounts.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewAccounts.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewAccounts.Columns.AddRange(new DataGridViewColumn[] { colAccount, colPassword, colCharName, colIsMainControl, colPid, colRestart });
            dataGridViewAccounts.Location = new Point(12, 45);
            dataGridViewAccounts.Name = "dataGridViewAccounts";
            dataGridViewAccounts.Size = new Size(771, 387);
            dataGridViewAccounts.TabIndex = 1;
            dataGridViewAccounts.CellContentClick += dataGridViewAccounts_CellContentClick;
            dataGridViewAccounts.AllowUserToAddRows = false; // 禁止新增行
            // 
            // colAccount
            // 
            colAccount.DataPropertyName = "Account";
            colAccount.HeaderText = "账号";
            colAccount.Name = "colAccount";
            // 
            // colPassword
            // 
            colPassword.DataPropertyName = "Password";
            colPassword.HeaderText = "密码";
            colPassword.Name = "colPassword";
            // 
            // colCharName
            // 
            colCharName.DataPropertyName = "CharacterName";
            colCharName.HeaderText = "角色名";
            colCharName.Name = "colCharName";
            // 
            // colIsMainControl
            // 
            colIsMainControl.DataPropertyName = "IsMainControl";
            colIsMainControl.HeaderText = "是否主控";
            colIsMainControl.Name = "colIsMainControl";
            // 
            // colPid
            // 
            colPid.DataPropertyName = "ProcessId";
            colPid.HeaderText = "PID";
            colPid.Name = "colPid";
            colPid.ReadOnly = true;
            // 
            // colRestart
            // 
            colRestart.HeaderText = "操作";
            colRestart.Name = "colRestart";
            colRestart.Text = "重启";
            colRestart.UseColumnTextForButtonValue = true;
            // 
            // btnRestartAll
            // 
            btnRestartAll.Location = new Point(683, 9);
            btnRestartAll.Name = "btnRestartAll";
            btnRestartAll.Size = new Size(100, 23);
            btnRestartAll.TabIndex = 2;
            btnRestartAll.Text = "重启全部";
            btnRestartAll.UseVisualStyleBackColor = true;
            btnRestartAll.Click += btnRestartAll_Click;
            // 
            // btnRestartTask
            // 
            btnRestartTask.Location = new Point(577, 9);
            btnRestartTask.Name = "btnRestartTask";
            btnRestartTask.Size = new Size(100, 23);
            btnRestartTask.TabIndex = 3;
            btnRestartTask.Text = "重启任务";
            btnRestartTask.UseVisualStyleBackColor = true;
            btnRestartTask.Click += btnRestartTask_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(795, 448);
            Controls.Add(btnRestartTask);
            Controls.Add(btnRestartAll);
            Controls.Add(dataGridViewAccounts);
            Controls.Add(label1);
            Name = "MainForm";
            Text = "主窗口";
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridViewAccounts).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private DataGridView dataGridViewAccounts;
        private DataGridViewTextBoxColumn colAccount;
        private DataGridViewTextBoxColumn colPassword;
        private DataGridViewTextBoxColumn colCharName;
        private DataGridViewCheckBoxColumn colIsMainControl;
        private DataGridViewTextBoxColumn colPid;
        private DataGridViewButtonColumn colRestart;
        private Button btnRestartAll;
        private Button btnRestartTask;
    }
}
