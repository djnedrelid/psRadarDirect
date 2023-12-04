namespace psRadarDirect
{
	partial class Form1
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
			if (disposing && (components != null)) {
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			this.lblStatus = new System.Windows.Forms.Label();
			this.lblCPU = new System.Windows.Forms.Label();
			this.lblGPU = new System.Windows.Forms.Label();
			this.lblRAM = new System.Windows.Forms.Label();
			this.lblSonarr = new System.Windows.Forms.Label();
			this.lblPlex = new System.Windows.Forms.Label();
			this.lblUptime = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.txtPlexUser = new System.Windows.Forms.TextBox();
			this.txtPlexPass = new System.Windows.Forms.TextBox();
			this.txtPlexPort = new System.Windows.Forms.TextBox();
			this.chkStartup = new System.Windows.Forms.CheckBox();
			this.label7 = new System.Windows.Forms.Label();
			this.txtAgentToken = new System.Windows.Forms.TextBox();
			this.btnTokenUpdate = new System.Windows.Forms.Button();
			this.lblShowHideToken = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.txtPublicToken = new System.Windows.Forms.TextBox();
			this.label9 = new System.Windows.Forms.Label();
			this.txtpsRadarPort = new System.Windows.Forms.TextBox();
			this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.txtSonarrKey = new System.Windows.Forms.TextBox();
			this.txtSonarrPort = new System.Windows.Forms.TextBox();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabWelcome = new System.Windows.Forms.TabPage();
			this.label11 = new System.Windows.Forms.Label();
			this.richTextBox1 = new System.Windows.Forms.RichTextBox();
			this.tabSonarr = new System.Windows.Forms.TabPage();
			this.chkSonarrSSL = new System.Windows.Forms.CheckBox();
			this.tabPlex = new System.Windows.Forms.TabPage();
			this.tabOptions = new System.Windows.Forms.TabPage();
			this.tabHelp = new System.Windows.Forms.TabPage();
			this.lnkGuideGenerated = new System.Windows.Forms.LinkLabel();
			this.richTextBox2 = new System.Windows.Forms.RichTextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.lblDbSize = new System.Windows.Forms.Label();
			this.lblWebServerStatus = new System.Windows.Forms.Label();
			this.tabControl1.SuspendLayout();
			this.tabWelcome.SuspendLayout();
			this.tabSonarr.SuspendLayout();
			this.tabPlex.SuspendLayout();
			this.tabOptions.SuspendLayout();
			this.tabHelp.SuspendLayout();
			this.SuspendLayout();
			// 
			// lblStatus
			// 
			this.lblStatus.AutoSize = true;
			this.lblStatus.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblStatus.Location = new System.Drawing.Point(12, 9);
			this.lblStatus.Name = "lblStatus";
			this.lblStatus.Size = new System.Drawing.Size(122, 16);
			this.lblStatus.TabIndex = 0;
			this.lblStatus.Text = "Monitoring active.";
			// 
			// lblCPU
			// 
			this.lblCPU.AutoSize = true;
			this.lblCPU.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblCPU.Location = new System.Drawing.Point(12, 54);
			this.lblCPU.Margin = new System.Windows.Forms.Padding(3);
			this.lblCPU.Name = "lblCPU";
			this.lblCPU.Size = new System.Drawing.Size(39, 16);
			this.lblCPU.TabIndex = 1;
			this.lblCPU.Text = "CPU:";
			// 
			// lblGPU
			// 
			this.lblGPU.AutoSize = true;
			this.lblGPU.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblGPU.Location = new System.Drawing.Point(12, 74);
			this.lblGPU.Margin = new System.Windows.Forms.Padding(3);
			this.lblGPU.Name = "lblGPU";
			this.lblGPU.Size = new System.Drawing.Size(40, 16);
			this.lblGPU.TabIndex = 2;
			this.lblGPU.Text = "GPU:";
			// 
			// lblRAM
			// 
			this.lblRAM.AutoSize = true;
			this.lblRAM.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblRAM.Location = new System.Drawing.Point(12, 94);
			this.lblRAM.Margin = new System.Windows.Forms.Padding(3);
			this.lblRAM.Name = "lblRAM";
			this.lblRAM.Size = new System.Drawing.Size(41, 16);
			this.lblRAM.TabIndex = 3;
			this.lblRAM.Text = "RAM:";
			// 
			// lblSonarr
			// 
			this.lblSonarr.AutoSize = true;
			this.lblSonarr.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblSonarr.Location = new System.Drawing.Point(12, 114);
			this.lblSonarr.Margin = new System.Windows.Forms.Padding(3);
			this.lblSonarr.Name = "lblSonarr";
			this.lblSonarr.Size = new System.Drawing.Size(151, 16);
			this.lblSonarr.TabIndex = 4;
			this.lblSonarr.Text = "Sonarr last updated: N/A";
			// 
			// lblPlex
			// 
			this.lblPlex.AutoSize = true;
			this.lblPlex.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblPlex.Location = new System.Drawing.Point(12, 134);
			this.lblPlex.Margin = new System.Windows.Forms.Padding(3);
			this.lblPlex.Name = "lblPlex";
			this.lblPlex.Size = new System.Drawing.Size(139, 16);
			this.lblPlex.TabIndex = 5;
			this.lblPlex.Text = "Plex last updated: N/A";
			// 
			// lblUptime
			// 
			this.lblUptime.AutoSize = true;
			this.lblUptime.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblUptime.Location = new System.Drawing.Point(12, 154);
			this.lblUptime.Margin = new System.Windows.Forms.Padding(3);
			this.lblUptime.Name = "lblUptime";
			this.lblUptime.Size = new System.Drawing.Size(71, 15);
			this.lblUptime.TabIndex = 6;
			this.lblUptime.Text = "Uptime: N/A";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(46)))), ((int)(((byte)(130)))));
			this.label4.Location = new System.Drawing.Point(15, 15);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(126, 13);
			this.label4.TabIndex = 10;
			this.label4.Text = "Plex.tv Username:";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(46)))), ((int)(((byte)(130)))));
			this.label5.Location = new System.Drawing.Point(15, 59);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(122, 13);
			this.label5.TabIndex = 11;
			this.label5.Text = "Plex.tv Password:";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(46)))), ((int)(((byte)(130)))));
			this.label6.Location = new System.Drawing.Point(15, 101);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(70, 13);
			this.label6.TabIndex = 12;
			this.label6.Text = "Plex Port:";
			// 
			// txtPlexUser
			// 
			this.txtPlexUser.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtPlexUser.Location = new System.Drawing.Point(18, 31);
			this.txtPlexUser.MaxLength = 50;
			this.txtPlexUser.Name = "txtPlexUser";
			this.txtPlexUser.Size = new System.Drawing.Size(176, 23);
			this.txtPlexUser.TabIndex = 6;
			// 
			// txtPlexPass
			// 
			this.txtPlexPass.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtPlexPass.Location = new System.Drawing.Point(18, 75);
			this.txtPlexPass.MaxLength = 50;
			this.txtPlexPass.Name = "txtPlexPass";
			this.txtPlexPass.PasswordChar = '*';
			this.txtPlexPass.Size = new System.Drawing.Size(176, 23);
			this.txtPlexPass.TabIndex = 7;
			// 
			// txtPlexPort
			// 
			this.txtPlexPort.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtPlexPort.Location = new System.Drawing.Point(18, 117);
			this.txtPlexPort.MaxLength = 5;
			this.txtPlexPort.Name = "txtPlexPort";
			this.txtPlexPort.Size = new System.Drawing.Size(80, 23);
			this.txtPlexPort.TabIndex = 8;
			// 
			// chkStartup
			// 
			this.chkStartup.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.chkStartup.AutoSize = true;
			this.chkStartup.Checked = true;
			this.chkStartup.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkStartup.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.chkStartup.Location = new System.Drawing.Point(36, 183);
			this.chkStartup.Name = "chkStartup";
			this.chkStartup.Size = new System.Drawing.Size(124, 17);
			this.chkStartup.TabIndex = 1;
			this.chkStartup.Text = "Start at system boot.";
			this.chkStartup.UseVisualStyleBackColor = true;
			this.chkStartup.Click += new System.EventHandler(this.chkStartup_Click);
			// 
			// label7
			// 
			this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.label7.AutoSize = true;
			this.label7.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(46)))), ((int)(((byte)(130)))));
			this.label7.Location = new System.Drawing.Point(16, 57);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(129, 13);
			this.label7.TabIndex = 14;
			this.label7.Text = "Private 4-digit pin:";
			// 
			// txtAgentToken
			// 
			this.txtAgentToken.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.txtAgentToken.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtAgentToken.Location = new System.Drawing.Point(19, 75);
			this.txtAgentToken.MaxLength = 4;
			this.txtAgentToken.Name = "txtAgentToken";
			this.txtAgentToken.PasswordChar = '*';
			this.txtAgentToken.Size = new System.Drawing.Size(80, 23);
			this.txtAgentToken.TabIndex = 11;
			// 
			// btnTokenUpdate
			// 
			this.btnTokenUpdate.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.btnTokenUpdate.BackColor = System.Drawing.Color.White;
			this.btnTokenUpdate.Cursor = System.Windows.Forms.Cursors.Hand;
			this.btnTokenUpdate.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.btnTokenUpdate.FlatAppearance.BorderSize = 0;
			this.btnTokenUpdate.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
			this.btnTokenUpdate.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
			this.btnTokenUpdate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnTokenUpdate.Font = new System.Drawing.Font("Calibri", 9.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnTokenUpdate.ForeColor = System.Drawing.Color.RoyalBlue;
			this.btnTokenUpdate.Location = new System.Drawing.Point(401, 214);
			this.btnTokenUpdate.Margin = new System.Windows.Forms.Padding(0);
			this.btnTokenUpdate.Name = "btnTokenUpdate";
			this.btnTokenUpdate.Size = new System.Drawing.Size(108, 25);
			this.btnTokenUpdate.TabIndex = 13;
			this.btnTokenUpdate.Text = "Save and update";
			this.btnTokenUpdate.UseVisualStyleBackColor = false;
			this.btnTokenUpdate.Click += new System.EventHandler(this.btnTokenUpdate_Click);
			// 
			// lblShowHideToken
			// 
			this.lblShowHideToken.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblShowHideToken.AutoSize = true;
			this.lblShowHideToken.Cursor = System.Windows.Forms.Cursors.Hand;
			this.lblShowHideToken.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblShowHideToken.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(46)))), ((int)(((byte)(130)))));
			this.lblShowHideToken.Location = new System.Drawing.Point(19, 145);
			this.lblShowHideToken.Name = "lblShowHideToken";
			this.lblShowHideToken.Size = new System.Drawing.Size(68, 13);
			this.lblShowHideToken.TabIndex = 15;
			this.lblShowHideToken.Text = "Show/Hide";
			this.lblShowHideToken.Click += new System.EventHandler(this.lblShowHideToken_Click);
			// 
			// label8
			// 
			this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.label8.AutoSize = true;
			this.label8.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(46)))), ((int)(((byte)(130)))));
			this.label8.Location = new System.Drawing.Point(16, 101);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(121, 13);
			this.label8.TabIndex = 16;
			this.label8.Text = "Public 4-digit pin:";
			// 
			// txtPublicToken
			// 
			this.txtPublicToken.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.txtPublicToken.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtPublicToken.Location = new System.Drawing.Point(19, 119);
			this.txtPublicToken.MaxLength = 4;
			this.txtPublicToken.Name = "txtPublicToken";
			this.txtPublicToken.PasswordChar = '*';
			this.txtPublicToken.Size = new System.Drawing.Size(80, 23);
			this.txtPublicToken.TabIndex = 12;
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label9.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(46)))), ((int)(((byte)(130)))));
			this.label9.Location = new System.Drawing.Point(16, 15);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(138, 13);
			this.label9.TabIndex = 101;
			this.label9.Text = "psRadar Direct Port:";
			// 
			// txtpsRadarPort
			// 
			this.txtpsRadarPort.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtpsRadarPort.Location = new System.Drawing.Point(19, 31);
			this.txtpsRadarPort.MaxLength = 5;
			this.txtpsRadarPort.Name = "txtpsRadarPort";
			this.txtpsRadarPort.Size = new System.Drawing.Size(80, 23);
			this.txtpsRadarPort.TabIndex = 10;
			// 
			// notifyIcon1
			// 
			this.notifyIcon1.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
			this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
			this.notifyIcon1.Text = "psRadar Direct";
			this.notifyIcon1.Visible = true;
			this.notifyIcon1.BalloonTipClicked += new System.EventHandler(this.notifyIcon1_BalloonTipClicked);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(46)))), ((int)(((byte)(130)))));
			this.label2.Location = new System.Drawing.Point(18, 15);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(110, 13);
			this.label2.TabIndex = 8;
			this.label2.Text = "Sonarr API Key:";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(46)))), ((int)(((byte)(130)))));
			this.label3.Location = new System.Drawing.Point(18, 57);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(86, 13);
			this.label3.TabIndex = 9;
			this.label3.Text = "Sonarr Port:";
			// 
			// txtSonarrKey
			// 
			this.txtSonarrKey.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtSonarrKey.Location = new System.Drawing.Point(21, 31);
			this.txtSonarrKey.MaxLength = 100;
			this.txtSonarrKey.Name = "txtSonarrKey";
			this.txtSonarrKey.Size = new System.Drawing.Size(176, 23);
			this.txtSonarrKey.TabIndex = 3;
			// 
			// txtSonarrPort
			// 
			this.txtSonarrPort.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtSonarrPort.Location = new System.Drawing.Point(21, 73);
			this.txtSonarrPort.MaxLength = 5;
			this.txtSonarrPort.Name = "txtSonarrPort";
			this.txtSonarrPort.Size = new System.Drawing.Size(80, 23);
			this.txtSonarrPort.TabIndex = 4;
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabWelcome);
			this.tabControl1.Controls.Add(this.tabSonarr);
			this.tabControl1.Controls.Add(this.tabPlex);
			this.tabControl1.Controls.Add(this.tabOptions);
			this.tabControl1.Controls.Add(this.tabHelp);
			this.tabControl1.Location = new System.Drawing.Point(194, 8);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(312, 208);
			this.tabControl1.TabIndex = 9;
			this.tabControl1.TabStop = false;
			this.tabControl1.Selected += new System.Windows.Forms.TabControlEventHandler(this.tabControl1_Selected);
			// 
			// tabWelcome
			// 
			this.tabWelcome.Controls.Add(this.label11);
			this.tabWelcome.Controls.Add(this.richTextBox1);
			this.tabWelcome.Location = new System.Drawing.Point(4, 22);
			this.tabWelcome.Name = "tabWelcome";
			this.tabWelcome.Size = new System.Drawing.Size(304, 182);
			this.tabWelcome.TabIndex = 3;
			this.tabWelcome.Text = "Welcome";
			this.tabWelcome.UseVisualStyleBackColor = true;
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label11.Location = new System.Drawing.Point(8, 14);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(195, 19);
			this.label11.TabIndex = 1;
			this.label11.Text = "Welcome to psRadar Direct";
			// 
			// richTextBox1
			// 
			this.richTextBox1.BackColor = System.Drawing.Color.White;
			this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.richTextBox1.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.richTextBox1.Location = new System.Drawing.Point(12, 42);
			this.richTextBox1.Name = "richTextBox1";
			this.richTextBox1.ReadOnly = true;
			this.richTextBox1.Size = new System.Drawing.Size(282, 119);
			this.richTextBox1.TabIndex = 0;
			this.richTextBox1.Text = resources.GetString("richTextBox1.Text");
			// 
			// tabSonarr
			// 
			this.tabSonarr.Controls.Add(this.chkSonarrSSL);
			this.tabSonarr.Controls.Add(this.label2);
			this.tabSonarr.Controls.Add(this.label3);
			this.tabSonarr.Controls.Add(this.txtSonarrKey);
			this.tabSonarr.Controls.Add(this.txtSonarrPort);
			this.tabSonarr.Location = new System.Drawing.Point(4, 22);
			this.tabSonarr.Name = "tabSonarr";
			this.tabSonarr.Size = new System.Drawing.Size(304, 182);
			this.tabSonarr.TabIndex = 0;
			this.tabSonarr.Text = "Sonarr";
			this.tabSonarr.UseVisualStyleBackColor = true;
			// 
			// chkSonarrSSL
			// 
			this.chkSonarrSSL.AutoSize = true;
			this.chkSonarrSSL.Location = new System.Drawing.Point(21, 105);
			this.chkSonarrSSL.Name = "chkSonarrSSL";
			this.chkSonarrSSL.Size = new System.Drawing.Size(157, 17);
			this.chkSonarrSSL.TabIndex = 10;
			this.chkSonarrSSL.Text = "My server uses SSL / https.";
			this.chkSonarrSSL.UseVisualStyleBackColor = true;
			this.chkSonarrSSL.CheckedChanged += new System.EventHandler(this.chkSonarrSSL_CheckedChanged);
			// 
			// tabPlex
			// 
			this.tabPlex.Controls.Add(this.label4);
			this.tabPlex.Controls.Add(this.label5);
			this.tabPlex.Controls.Add(this.label6);
			this.tabPlex.Controls.Add(this.txtPlexUser);
			this.tabPlex.Controls.Add(this.txtPlexPass);
			this.tabPlex.Controls.Add(this.txtPlexPort);
			this.tabPlex.Location = new System.Drawing.Point(4, 22);
			this.tabPlex.Name = "tabPlex";
			this.tabPlex.Size = new System.Drawing.Size(304, 182);
			this.tabPlex.TabIndex = 1;
			this.tabPlex.Text = "Plex";
			this.tabPlex.UseVisualStyleBackColor = true;
			// 
			// tabOptions
			// 
			this.tabOptions.Controls.Add(this.label9);
			this.tabOptions.Controls.Add(this.txtpsRadarPort);
			this.tabOptions.Controls.Add(this.label7);
			this.tabOptions.Controls.Add(this.txtAgentToken);
			this.tabOptions.Controls.Add(this.txtPublicToken);
			this.tabOptions.Controls.Add(this.lblShowHideToken);
			this.tabOptions.Controls.Add(this.label8);
			this.tabOptions.Location = new System.Drawing.Point(4, 22);
			this.tabOptions.Name = "tabOptions";
			this.tabOptions.Size = new System.Drawing.Size(304, 182);
			this.tabOptions.TabIndex = 2;
			this.tabOptions.Text = "Options";
			this.tabOptions.UseVisualStyleBackColor = true;
			// 
			// tabHelp
			// 
			this.tabHelp.Controls.Add(this.lnkGuideGenerated);
			this.tabHelp.Controls.Add(this.richTextBox2);
			this.tabHelp.Location = new System.Drawing.Point(4, 22);
			this.tabHelp.Name = "tabHelp";
			this.tabHelp.Size = new System.Drawing.Size(304, 182);
			this.tabHelp.TabIndex = 4;
			this.tabHelp.Text = "Guide";
			this.tabHelp.UseVisualStyleBackColor = true;
			// 
			// lnkGuideGenerated
			// 
			this.lnkGuideGenerated.AutoSize = true;
			this.lnkGuideGenerated.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lnkGuideGenerated.Location = new System.Drawing.Point(8, 140);
			this.lnkGuideGenerated.Name = "lnkGuideGenerated";
			this.lnkGuideGenerated.Size = new System.Drawing.Size(32, 16);
			this.lnkGuideGenerated.TabIndex = 2;
			this.lnkGuideGenerated.TabStop = true;
			this.lnkGuideGenerated.Text = "N/A";
			this.lnkGuideGenerated.VisitedLinkColor = System.Drawing.Color.Blue;
			this.lnkGuideGenerated.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkGuideGenerated_LinkClicked);
			// 
			// richTextBox2
			// 
			this.richTextBox2.BackColor = System.Drawing.Color.White;
			this.richTextBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.richTextBox2.DetectUrls = false;
			this.richTextBox2.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.richTextBox2.Location = new System.Drawing.Point(11, 10);
			this.richTextBox2.Name = "richTextBox2";
			this.richTextBox2.ReadOnly = true;
			this.richTextBox2.Size = new System.Drawing.Size(282, 118);
			this.richTextBox2.TabIndex = 1;
			this.richTextBox2.TabStop = false;
			this.richTextBox2.Text = resources.GetString("richTextBox2.Text");
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.ForeColor = System.Drawing.Color.Gray;
			this.label1.Location = new System.Drawing.Point(12, 203);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(170, 13);
			this.label1.TabIndex = 103;
			this.label1.Text = "© 2018-2019 Dag J Nedrelid";
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label10.ForeColor = System.Drawing.Color.RoyalBlue;
			this.label10.Location = new System.Drawing.Point(1, 8);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(15, 18);
			this.label10.TabIndex = 104;
			this.label10.Text = ">";
			// 
			// lblDbSize
			// 
			this.lblDbSize.AutoSize = true;
			this.lblDbSize.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblDbSize.ForeColor = System.Drawing.Color.DimGray;
			this.lblDbSize.Location = new System.Drawing.Point(195, 221);
			this.lblDbSize.Name = "lblDbSize";
			this.lblDbSize.Size = new System.Drawing.Size(43, 13);
			this.lblDbSize.TabIndex = 105;
			this.lblDbSize.Text = "DB Size:";
			// 
			// lblWebServerStatus
			// 
			this.lblWebServerStatus.AutoSize = true;
			this.lblWebServerStatus.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblWebServerStatus.ForeColor = System.Drawing.Color.Black;
			this.lblWebServerStatus.Location = new System.Drawing.Point(12, 30);
			this.lblWebServerStatus.Name = "lblWebServerStatus";
			this.lblWebServerStatus.Size = new System.Drawing.Size(105, 13);
			this.lblWebServerStatus.TabIndex = 106;
			this.lblWebServerStatus.Text = "Webserver is DOWN.";
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(518, 243);
			this.Controls.Add(this.lblWebServerStatus);
			this.Controls.Add(this.lblDbSize);
			this.Controls.Add(this.label10);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.tabControl1);
			this.Controls.Add(this.btnTokenUpdate);
			this.Controls.Add(this.chkStartup);
			this.Controls.Add(this.lblUptime);
			this.Controls.Add(this.lblPlex);
			this.Controls.Add(this.lblSonarr);
			this.Controls.Add(this.lblRAM);
			this.Controls.Add(this.lblGPU);
			this.Controls.Add(this.lblCPU);
			this.Controls.Add(this.lblStatus);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "Form1";
			this.Text = "psRadar Direct";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
			this.Shown += new System.EventHandler(this.Form1_Shown);
			this.Resize += new System.EventHandler(this.Form1_Resize);
			this.tabControl1.ResumeLayout(false);
			this.tabWelcome.ResumeLayout(false);
			this.tabWelcome.PerformLayout();
			this.tabSonarr.ResumeLayout(false);
			this.tabSonarr.PerformLayout();
			this.tabPlex.ResumeLayout(false);
			this.tabPlex.PerformLayout();
			this.tabOptions.ResumeLayout(false);
			this.tabOptions.PerformLayout();
			this.tabHelp.ResumeLayout(false);
			this.tabHelp.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label lblStatus;
		private System.Windows.Forms.Label lblCPU;
		private System.Windows.Forms.Label lblGPU;
		private System.Windows.Forms.Label lblRAM;
		private System.Windows.Forms.Label lblSonarr;
		private System.Windows.Forms.Label lblPlex;
		private System.Windows.Forms.Label lblUptime;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TextBox txtPlexUser;
		private System.Windows.Forms.TextBox txtPlexPass;
		private System.Windows.Forms.TextBox txtPlexPort;
		private System.Windows.Forms.CheckBox chkStartup;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.TextBox txtAgentToken;
		private System.Windows.Forms.Button btnTokenUpdate;
		private System.Windows.Forms.Label lblShowHideToken;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.TextBox txtPublicToken;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.TextBox txtpsRadarPort;
		private System.Windows.Forms.NotifyIcon notifyIcon1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox txtSonarrKey;
		private System.Windows.Forms.TextBox txtSonarrPort;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabWelcome;
		private System.Windows.Forms.RichTextBox richTextBox1;
		private System.Windows.Forms.TabPage tabSonarr;
		private System.Windows.Forms.TabPage tabPlex;
		private System.Windows.Forms.TabPage tabOptions;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Label lblDbSize;
		private System.Windows.Forms.TabPage tabHelp;
		private System.Windows.Forms.RichTextBox richTextBox2;
		private System.Windows.Forms.Label lblWebServerStatus;
		private System.Windows.Forms.LinkLabel lnkGuideGenerated;
		private System.Windows.Forms.CheckBox chkSonarrSSL;
	}
}

