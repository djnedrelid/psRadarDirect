using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace psRadarDirect
{
	public partial class Form1 : Form
	{
		private static Version Version = Assembly.GetExecutingAssembly().GetName().Version;
		//private static string psAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\psRadarDirect\";

		// Statics and Constants.
		private static string XMLfile = Environment.GetEnvironmentVariable("temp") + @"\psRadarStartup.xml";
		public static string exepath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		public static string EncKey = "vEA3GUhgiPKWeMsk12mLYSjzQD8TJlnw5.o4bX09yN7FORaBdIqpfZ ctCuxV6rH";
		private const string THIS_HOST = "127.0.0.1";
		public static string psRadarDirectDB = exepath + @"\DB\psRadarDirectDB.sdf";
		public static string DBFileConStr = @"Data Source="+ psRadarDirectDB + "; Max Database Size=4091;";
		public static string SonarrPosterFolder = string.Format(@"{0}\{1}", exepath, @"DirectWebFiles\Images\SonarrPosters\");
		public static string LogFolder = string.Format(@"{0}\{1}", exepath, @"Logs\");
		private static string ErrorLogFile = LogFolder +"Errors.log";
		public static string WebFolder = string.Format(@"{0}\{1}", exepath, @"DirectWebFiles");

		// vars.
		private bool StartMinimized = false;
		private bool ReallyQuit = false;
		private bool PlexReAuthenticationNeeded = false;
		private bool SonarrReAuthenticationNeeded = true;
		private bool WebServerReAuthenticationNeeded = false;
		private MenuItem[] IcoMenuItems;
		private ContextMenu IcoContextMenu;
		private bool ShowWillContinueToRun = true;
		private string SonarrKey, SonarrPort;
		private string PlexUser, PlexPass, PlexPort, DirectPort;
		public string AgentToken {get; private set;}
		public string PublicToken {get; private set;}
		public bool SonarrRelaxRound = false;
		public int RequestsRunning = 0;
		private string GeneratedUrl = "";
		private psWebServer psWeb = new psWebServer();
		public Queue<GrafMatrixItem> GrafMatrix = new Queue<GrafMatrixItem>();

		// Delegates (thread safe communication).
		delegate void lblChangerDg(string _txt, string _lbl);
		delegate void UpdateWebRequestsRunningDg(bool w);

		public Form1(string[] args)
		{
			InitializeComponent();
			if (Process.GetProcessesByName("psRadarDirect").Length > 1) {
				MessageBox.Show("Already running.");
				notifyIcon1.Visible = false;
				Environment.Exit(0);
			}

			this.Text = "psRadar Direct v"+ Version.Major +"."+ Version.Minor +"."+ Version.Build;
			ServicePointManager.DefaultConnectionLimit = 10;
			ServicePointManager.CheckCertificateRevocationList = false;
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
			ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => true);
			txtAgentToken.SelectionStart = txtAgentToken.Text.Length;
			txtAgentToken.ScrollToCaret();
			checkFirstStart();
			chkStartup_IsActive();
			CreateTasktrayIcon();
			LoadUserConfiguration();
			djDBI.Open();

			if (args.Length > 0)
				if (args[0] == "-startmini")
					StartMinimized = true;
			
			// Show welcome tab as default.
			tabControl1.SelectTab(0);

			// Attempt to start our web service.
			if (DirectPort != "")
				psWeb.Start(DirectPort, this);

			// Start hovedmotoren (main loop).
			Thread T = new Thread(new ThreadStart(mainloop));
			T.IsBackground = true;
			T.Start();
		}

		private void mainloop()
		{
			HWinfo CPUi = new HWinfo("CPU");
			HWinfo GPUi = new HWinfo("GPU");
			HWinfo RAMi = new HWinfo("RAM");
			PlexMediaServer PMS = new PlexMediaServer();
			Sonarr SNR = new Sonarr(THIS_HOST, this);
			HWstats NewHWData = new HWstats();
			DataRegistryHandler DataRegistry = new DataRegistryHandler();
			Stopwatch SonarrUploadTimer = Stopwatch.StartNew();		

			while(true) {
				Thread.Sleep(2000);
				lblChanger("Monitoring active.", "Status");

				// Update HW stats.
				try {
				NewHWData.CPU = "CPU: "+ CPUi.getLoad() +", "+ CPUi.getTemp();
				NewHWData.GPU = "GPU: "+ GPUi.getLoad() +", "+ GPUi.getTemp();
				NewHWData.RAM = "RAM: "+ RAMi.getAvailableRAM();
				NewHWData.Uptime = CPUi.getUptime();

				if (NewHWData.GPU.Contains("N/A"))
					NewHWData.GPU = "GPU: N/A";

				lblChanger(NewHWData.CPU, "CPU");
				lblChanger(NewHWData.GPU, "GPU");
				lblChanger(NewHWData.RAM, "RAM");
				lblChanger(NewHWData.Uptime, "Uptime");
				} catch (Exception e) {
					LogError("Update HW stats: "+ e.Message);
				}

				// Check web service status.
				if (!psWeb.IsListening())
					lblWebServerStatus.Text = "Webserver is DOWN.";
				else
					lblWebServerStatus.Text = "Webserver is UP. Workers: "+RequestsRunning;

				if (WebServerReAuthenticationNeeded) {
					psWeb.Stop();
					WebServerReAuthenticationNeeded = false;

					if (DirectPort != "")
						psWeb.Start(DirectPort, this);
				}

				// Attempt connection to Plex Media Server / Plex.tv (for security access token).
				try {
				if (!PMS.Connected || PlexReAuthenticationNeeded) {
					if (PlexUser != "" && PlexPass != "" && PlexPort != "") {
						if(PMS.Connect(THIS_HOST, PlexPort, PlexUser, PlexPass) > 1)
							lblChanger("Bad Plex Connection.", "Status");
						else
							lblChanger("Plex.tv Connected OK.", "Status");

						PlexReAuthenticationNeeded = false;
					}
				}} catch (Exception e) {
					LogError("Attempt connection to Plex.tv for token: "+ e.Message);
				}
				
				// Check activity if plex got connected OK.
				try {
				if (PMS.Connected && PMS.CheckActivity()) {

					// Check if we're still connected, as we may loose connection if something fails in CheckActivity().
					if (PMS.Connected) {
						// Collection of new activity went well. Plex connection is still good.
						if (PMS.pActivities.Count > 0) {
							lblChanger("Plex Last Updated: "+ ShowProperLogTime(DateTime.Now.ToString("HHmm")), "Plex");
						}
					} else {
						lblChanger("Plex Error, Bad Port?", "Status");
					}

					// Check if we should disconnect if user has emptied and saved credentials on UI.
					if (PlexUser == "" || PlexPass == "" || PlexPort == "") {
						lblChanger("Disconnecting Plex.", "Status");
						PMS.Disconnect();
						lblChanger("Plex Activities N/A.", "Status");
					}
				}} catch (Exception e) {
					LogError("Check activity if plex connected OK: "+ e.Message);
				}

				// Sonarr.
				// Register data only every 5 second to keep it somewhat relaxed; it's a calendar.
				try {
				if (SonarrReAuthenticationNeeded) {
					SNR.SetPort(SonarrPort);
					SNR.SetAPIKey(SonarrKey);
					SonarrReAuthenticationNeeded = false;
				}
				if (SonarrUploadTimer.ElapsedMilliseconds >= 5000) {
					// Datacollection happens in Sonarr class object separate thread.
					if (SNR.SonarrData.Count > 0)
						lblChanger("Sonarr Last Updated: "+ ShowProperLogTime(DateTime.Now.ToString("HHmm")), "Sonarr");

					SonarrUploadTimer.Restart();
					SonarrRelaxRound = false;
				}

				/* JSON - Left for historic reference, we're not sending data off-site in psRadarDirect.
				FinalOutput = JsonConvert.SerializeObject(new List<FinalOutputPackage>() { new FinalOutputPackage {
					HW = NewHWData,
					PLEX = PMS.pActivities,
					Sonarr = SNR.SonarrData
				}});*/

				/* This is where Direct becomes radically different from Agent.
				 * Instead of sending data to a centralized server, we send it 
				 * to a local class instance instead to register it to a local 
				 * portable DB. For use with our own webservice later. */ 
				try {
					// Classes are always sent by ref, so we send reference 
					// to copies so PMS and SNR can continue to update.
					lock (Sonarr.SonarrDataLock) {
					DataRegistry.ReceiveNewData(new List<FinalOutputPackage>() {
						new FinalOutputPackage {
							HW = NewHWData, // Structs are sent by value.
							PLEX = new List<PlexActivity>(PMS.pActivities),
							Sonarr = new List<Episode>(SNR.SonarrData)
					}}, this);}

				} catch (Exception e) {
					MessageBox.Show("Critical error occurred, check logs.","psRadar Direct",MessageBoxButtons.OK,MessageBoxIcon.Error);
					LogError("ReceiveNewData(1): "+ e.Message + e.ToString());
					Environment.Exit(1);
				}

				} catch (Exception e) {
					LogError("Sonarr and Create JSON: "+ e.Message + e.ToString());
				} 
			}
		}

		public void UpdateWebRequestsRunning(bool w)
		{
			if (InvokeRequired) {
				UpdateWebRequestsRunningDg Dg = new UpdateWebRequestsRunningDg(UpdateWebRequestsRunning);
				this.Invoke(Dg, w);
				return;
			}

			if (w)
				++RequestsRunning;
			else
				--RequestsRunning;
		}

		public void lblChanger(string _txt, string _lbl)
		{
			if (InvokeRequired) {
				lblChangerDg Dg = new lblChangerDg(lblChanger);
				this.Invoke(Dg, new object[]{_txt, _lbl});
				return;
			}

			switch (_lbl) {
				case "CPU":
					lblCPU.Text = _txt;
					break;

				case "GPU":
					lblGPU.Text = _txt;
					break;

				case "RAM":
					lblRAM.Text = _txt;
					break;

				case "Plex":
					lblPlex.Text = _txt;
					break;

				case "Sonarr":
					lblSonarr.Text = _txt;
					break;

				case "Uptime":
					lblUptime.Text = _txt;
					break;

				case "Status":
					lblStatus.Text = _txt;
					break;

				case "DBSize":
					lblDbSize.Text = _txt;
					break;

				case "WebServerStatus":
					lblWebServerStatus.Text = _txt;
					break;		
			}
		}

		private void LoadUserConfiguration()
		{
			//
			// Triggers when a new version is installed which we make sure has NeedUpgrade = true as default.
			// This is needed for settings to pass between versions in AppData where it's stored.
			//
			try {
			if (Properties.Settings.Default.NeedUpgrade) {
				Properties.Settings.Default.Upgrade();
				Properties.Settings.Default.NeedUpgrade = false;
				Properties.Settings.Default.Save();
			}

			SonarrKey = (Properties.Settings.Default.SonarrAPIKey == "" ? "" : djDecryptString(Properties.Settings.Default.SonarrAPIKey));
			SonarrPort = (Properties.Settings.Default.SonarrPort == "" ? "" : djDecryptString(Properties.Settings.Default.SonarrPort));
			chkSonarrSSL.Checked = Properties.Settings.Default.SonarrUsesSSL;
			chkStartup.Checked = Properties.Settings.Default.AutoStartUp;
			PlexUser = (Properties.Settings.Default.PlexUsername == "" ? "" : djDecryptString(Properties.Settings.Default.PlexUsername));
			PlexPass = (Properties.Settings.Default.PlexPassword == "" ? "" : djDecryptString(Properties.Settings.Default.PlexPassword));
			PlexPort = (Properties.Settings.Default.PlexPort == "" ? "" : djDecryptString(Properties.Settings.Default.PlexPort));
			AgentToken = (Properties.Settings.Default.PrivateToken == "" ? "" : djDecryptString(Properties.Settings.Default.PrivateToken));
			PublicToken = (Properties.Settings.Default.PrivateToken == "" ? "" : djDecryptString(Properties.Settings.Default.PublicToken));
			DirectPort = (Properties.Settings.Default.PrivateToken == "" ? "" : djDecryptString(Properties.Settings.Default.DirectPort));

			txtSonarrKey.Text = SonarrKey;
			txtSonarrPort.Text = SonarrPort;
			txtPlexUser.Text = PlexUser;
			txtPlexPass.Text = PlexPass;
			txtPlexPort.Text = PlexPort;
			txtAgentToken.Text = AgentToken;
			txtPublicToken.Text = PublicToken;
			txtpsRadarPort.Text = DirectPort;

			} catch (Exception e) {
				LogError("LoadUserConfiguration(): "+ e.Message);
				MsgBox("LoadUserConfiguration() logged an error.");
			}
		}

		private void chkStartup_IsActive()
		{
			try {

			if (!Properties.Settings.Default.AutoStartUp)
				chkStartup.Checked = false;
			else
				chkStartup.Checked = true;

			} catch (Exception e) {
				LogError("chkStartup_IsActive(): "+ e.Message);
				MsgBox("chkStartup_IsActive() logged an error.");
			}
		}

		private void checkFirstStart()
		{
			try {
			string ExistKey = @"Thronic\psRadarDirect";
			using (RegistryKey RK = Registry.CurrentUser.OpenSubKey("SOFTWARE", true)) {
				if (RK.OpenSubKey(ExistKey, false) == null) { 
					RK.CreateSubKey(ExistKey);
					checkStartupState();
				}

				RK.Close();
			
			}} catch (Exception e) {
				LogError("checkFirstStart(): "+ e.Message);
				MsgBox("checkFirstStart() logged an error.");
			}
		}

		private void checkStartupState()
		{
			Process p;

			try {
			if (chkStartup.Checked) {
				// Update registry run key.
				/*string RunKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
				using(RegistryKey RK = Registry.CurrentUser.OpenSubKey(RunKey, true)) {
					RK.SetValue("psRadarDirect", "\""+ exepath + @"\psRadarDirect.exe"+"\" -startmini");
					RK.Close();
				}*/

				try {
					// Remove any startup task.
					p = new Process();
					p.StartInfo.UseShellExecute = false;
					p.StartInfo.FileName = "SCHTASKS";
					p.StartInfo.Arguments = "/CREATE /TN psRadarDirectStartup /F /XML "+ XMLfile;
					p.StartInfo.RedirectStandardOutput = false;
					p.StartInfo.RedirectStandardError = false;
					File.WriteAllText(XMLfile, CreateTaskXML(exepath + @"\psRadarDirect.exe"));
					p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
					p.Start();
					p.WaitForExit();

				} catch (Exception ee) {
					Console.WriteLine("Startup option error: "+ ee.Message);
				}

				Properties.Settings.Default.AutoStartUp = true;
				Properties.Settings.Default.Save();
				MsgBox("I will now start automatically when you boot and log in.");

			} else {
				// Update registry run key.
				/*
				string RunKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
				using(RegistryKey RK = Registry.CurrentUser.OpenSubKey(RunKey, true)) {
					RK.DeleteValue("psRadarDirect", false);
					RK.Close();
				}*/
				
				try {
					p = new Process();
					p.StartInfo.UseShellExecute = false;
					p.StartInfo.FileName = "schtasks";
					p.StartInfo.Arguments = "/delete /TN psRadarAgentStartup /F";
					p.StartInfo.RedirectStandardOutput = false;
					p.StartInfo.RedirectStandardError = false;
					p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
					p.Start();
					p.WaitForExit();
		
				} catch (Exception) {
					// Do nothing.
				}

				Properties.Settings.Default.AutoStartUp = false;
				Properties.Settings.Default.Save();
				MsgBox("I should no longer start automatically.");
			
			}} catch (Exception ee) {
				MsgBox("Startup option error: "+ ee.Message);
			}
		}

		private static string CreateTaskXML(string ProgramFilePath)
		{
			return ""+
			"<?xml version=\"1.0\" encoding=\"UTF-16\"?>"+
			"<Task version=\"1.2\" xmlns=\"http://schemas.microsoft.com/windows/2004/02/mit/task\">"+
			"  <Triggers>"+
			"    <LogonTrigger>"+
			"      <Enabled>true</Enabled>"+
			"    </LogonTrigger>"+
			"  </Triggers>"+
			"  <Principals>"+
			"    <Principal id=\"Author\">"+
			"      <LogonType>InteractiveToken</LogonType>"+
			"      <RunLevel>HighestAvailable</RunLevel>"+
			"    </Principal>"+
			"  </Principals>"+
			"  <Settings>"+
			"    <MultipleInstancesPolicy>IgnoreNew</MultipleInstancesPolicy>"+
			"    <DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries>"+
			"    <StopIfGoingOnBatteries>false</StopIfGoingOnBatteries>"+
			"    <AllowHardTerminate>false</AllowHardTerminate>"+
			"    <StartWhenAvailable>false</StartWhenAvailable>"+
			"    <RunOnlyIfNetworkAvailable>false</RunOnlyIfNetworkAvailable>"+
			"    <IdleSettings>"+
			"      <StopOnIdleEnd>true</StopOnIdleEnd>"+
			"      <RestartOnIdle>false</RestartOnIdle>"+
			"    </IdleSettings>"+
			"    <AllowStartOnDemand>true</AllowStartOnDemand>"+
			"    <Enabled>true</Enabled>"+
			"    <Hidden>false</Hidden>"+
			"    <RunOnlyIfIdle>false</RunOnlyIfIdle>"+
			"    <WakeToRun>false</WakeToRun>"+
			"    <ExecutionTimeLimit>PT0S</ExecutionTimeLimit>"+
			"    <Priority>7</Priority>"+
			"  </Settings>"+
			"  <Actions Context=\"Author\">"+
			"    <Exec>"+
			"      <Command>\""+ ProgramFilePath +"\"</Command>"+
			"      <Arguments>-startmini</Arguments>"+
			"    </Exec>"+
			"  </Actions>"+
			"</Task>";
		}

		public void MsgBox(string s)
		{
			MessageBox.Show(s, "psRadar Direct", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		private void CreateTasktrayIcon()
		{
			IcoContextMenu = new ContextMenu();
			IcoMenuItems = new MenuItem[2];

			// Initialize the menu item(s).
			IcoMenuItems[0] = new MenuItem();
			IcoMenuItems[0].Index = 0;
			IcoMenuItems[0].Text = "Open";
			IcoMenuItems[0].Click += new EventHandler(IcoMenuItem_DoubleClick);

			IcoMenuItems[1] = new MenuItem();
			IcoMenuItems[1].Index = 1;
			IcoMenuItems[1].Text = "Exit";
			IcoMenuItems[1].Click += new EventHandler(IcoMenuItem_ExitClick);

			// Add menu items to a context menu.
			IcoContextMenu.MenuItems.AddRange(IcoMenuItems);

			// Add the menu item(s) to the NotifyIcon object.
			notifyIcon1.ContextMenu = IcoContextMenu;

			// Add a double-click handler to maximize program.
			notifyIcon1.DoubleClick += new EventHandler(IcoMenuItem_DoubleClick);

			// Hide by default.
			//notifyIcon1.Visible = false;
		}

		private string djEncryptString(string s)
		{
			//
			// s is a random string to be encrypted.
			// key is a randomized string that must contain all
			// the characters found in s as an absolute minimum.
			//

			char[] key_chars = EncKey.ToCharArray();
			char[] string_chars = s.ToCharArray();
			string key_coords = "";
			string key_coords_lengths = "";

			foreach (char c in string_chars) {
				for (int n=0; n<key_chars.Length; n++) {
					if (c == key_chars[n]) {
						key_coords += n.ToString();
						key_coords_lengths += n.ToString().Length.ToString();
					}
				}
			}

			return key_coords +"-"+ key_coords_lengths;
		}

		private void notifyIcon1_BalloonTipClicked(object sender, EventArgs e)
		{
			if (this.WindowState == FormWindowState.Minimized) {
				this.Show();
				this.WindowState = FormWindowState.Normal;
				//notifyIcon1.Visible = false;
			}
		}

		private void IcoMenuItem_ExitClick(object Sender, EventArgs e)
		{
			// Close MainForm, which closes the application.
			ReallyQuit = true;
			this.Close();
		}

		private void IcoMenuItem_DoubleClick(object Sender, EventArgs e)
		{
			if (this.WindowState == FormWindowState.Minimized) {
				this.Show();
				this.WindowState = FormWindowState.Normal;
				//notifyIcon1.Visible = false;
			}
		}

		private void chkStartup_Click(object sender, EventArgs e)
		{
			checkStartupState();
		}

		private string djDecryptString(string s)
		{
			//
			// s is the output from djEncryptString().
			// key is the same key as used to encrypt with.
			//

			char[] key_chars = EncKey.ToCharArray();
			string key_coords = s.Split('-')[0];
			char[] key_coords_lengths = s.Split('-')[1].ToCharArray();
			string DecryptedString = "";

			foreach (char c in key_coords_lengths) {
				DecryptedString += key_chars[
					Int32.Parse(key_coords.Substring(0,(int)Char.GetNumericValue(c)))
				];
				key_coords = key_coords.Substring((int)Char.GetNumericValue(c));
			}
			
			return DecryptedString;
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (e.CloseReason == CloseReason.UserClosing && !ReallyQuit) {
				e.Cancel = true;
				this.WindowState = FormWindowState.Minimized;
				return;
			}

			// Close DB.
			djDBI.Close();

			// Remove icon from systray (usually lags behind otherwise).
			notifyIcon1.Visible = false;

			// Kill any child processes that may be lagging behind.
			Process[] children = Process.GetProcessesByName("psRadarDirect");
			foreach (Process child in children) {
				child.Kill();
			}
		}

		private void Form1_Shown(object sender, EventArgs e)
		{
			lblStatus.Focus();

			if (StartMinimized) {
				ShowWillContinueToRun = false;
				this.WindowState = FormWindowState.Minimized;
			}
		}

		private void lblShowHideToken_Click(object sender, EventArgs e)
		{
			if (txtAgentToken.PasswordChar == '*') {
				txtAgentToken.PasswordChar = '\0';
				txtPublicToken.PasswordChar = '\0';
			} else {
				txtAgentToken.PasswordChar = '*';
				txtPublicToken.PasswordChar = '*';
			}
		}

		private void Form1_Resize(object sender, EventArgs e)
		{
			if (this.WindowState == FormWindowState.Minimized) {
				this.Hide();
				//notifyIcon1.Visible = true;
				if (ShowWillContinueToRun) {
					notifyIcon1.BalloonTipText = "psRadarDirect will continue to run.";
					notifyIcon1.ShowBalloonTip(2000);
					ShowWillContinueToRun = false;
				}
			}
		}

		private void lnkGuideGenerated_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			try {
			Process.Start(GeneratedUrl);
			} catch (Exception) {
				MessageBox.Show("Could not open "+ GeneratedUrl);
			}
		}

		private void tabControl1_Selected(object sender, TabControlEventArgs e)
		{
			if (tabControl1.SelectedIndex == 4) {
				GeneratedUrl = "http://";
				GeneratedUrl += "127.0.0.1:"+ DirectPort +"/FrontUI?PIN="+ AgentToken;
				lnkGuideGenerated.Text = GeneratedUrl;
			}
		}

		private void btnTokenUpdate_Click(object sender, EventArgs e)
		{
			// Sanity check for webserver port.
			if (
				Regex.IsMatch(txtpsRadarPort.Text, "[^0-9]") || 
				(txtpsRadarPort.Text.Length < 4 && txtpsRadarPort.Text != "")
			) {
				txtpsRadarPort.Text = "";
				MessageBox.Show("Choose a more sensible port.","Please",MessageBoxButtons.OK,MessageBoxIcon.Hand);
				return;
			}

			// Sanity check for pin koder.
			if (
				Regex.IsMatch(txtPublicToken.Text, "[^a-zA-Z0-9]") || 
				(txtPublicToken.Text.Length < 4 && txtPublicToken.Text != "") 
				
			) {
				txtPublicToken.Text = "";
				MessageBox.Show("Public PIN must be a 4 character long alphanumeric value.","Please",MessageBoxButtons.OK,MessageBoxIcon.Hand);
				return;
			}
			if (
				Regex.IsMatch(txtAgentToken.Text, "[^a-zA-Z0-9]") || 
				(txtAgentToken.Text.Length < 4 && txtAgentToken.Text != "")
			) {
				txtAgentToken.Text = "";
				MessageBox.Show("Private PIN must be a 4 character long alphanumeric value.","Please",MessageBoxButtons.OK,MessageBoxIcon.Hand);
				return;
			}

			Properties.Settings.Default.SonarrAPIKey = djEncryptString(txtSonarrKey.Text);
			Properties.Settings.Default.SonarrPort = djEncryptString(txtSonarrPort.Text);
			Properties.Settings.Default.SonarrUsesSSL = chkSonarrSSL.Checked;
			Properties.Settings.Default.PlexUsername = djEncryptString(txtPlexUser.Text);
			Properties.Settings.Default.PlexPassword = djEncryptString(txtPlexPass.Text);
			Properties.Settings.Default.PlexPort = djEncryptString(txtPlexPort.Text);
			Properties.Settings.Default.PrivateToken = djEncryptString(txtAgentToken.Text);
			Properties.Settings.Default.PublicToken = djEncryptString(txtPublicToken.Text);
			Properties.Settings.Default.DirectPort = djEncryptString(txtpsRadarPort.Text);
			Properties.Settings.Default.Save();
			MsgBox("Save OK. Signaling re-authentication.");
			LoadUserConfiguration();
			PlexReAuthenticationNeeded = true;
			SonarrReAuthenticationNeeded = true;
			WebServerReAuthenticationNeeded = true;
		}

		private void chkSonarrSSL_CheckedChanged(object sender, EventArgs e)
		{

		}

		public static void LogError(string s)
		{
			// Logging.
			File.AppendAllText(ErrorLogFile, ShowProperLogDate(DateTime.Now.ToString("yyyyMMddHHmmss")) +"\t"+ s + Environment.NewLine);

			// Rotate logfile if it's getting big.
			double _fsz = new FileInfo(ErrorLogFile).Length;
			double _fszKiB = Math.Round(_fsz/1024,1);
			if (_fszKiB >= 100) { // 100 KB as margin.
				File.Copy(ErrorLogFile, ErrorLogFile +".old.log", true); 
				File.WriteAllText(ErrorLogFile,"");
			}
		}

		private static string ShowProperLogDate(string tStamp)
		{
			string Year, Month, Day, Hours, Minutes, Seconds;
			string FormattedLogtime;

			try {
			Year = tStamp.Substring(0,4);
			Month = tStamp.Substring(4,2);
			Day = tStamp.Substring(6,2);
			Hours = tStamp.Substring(8,2);
			Minutes = tStamp.Substring(10,2);
			Seconds = tStamp.Substring(12,2);

			FormattedLogtime = Year +"-"+ Month +"-"+ Day +" "+ Hours + ":" + Minutes + ":" + Seconds;
			return FormattedLogtime;
			
			} catch (Exception e) {
				LogError("ShowProperLogDate(string tStamp): "+ e.Message);
				throw new Exception("ShowProperLogDate Tryna Hardt: "+ e.Message);
			} 
		}

		public static string ShowProperLogTime(string tStamp)
		{
			string Hours, Minutes;
			string FormattedLogtime;

			try {
			Hours = tStamp.Substring(0,2);
			Minutes = tStamp.Substring(2,2);

			FormattedLogtime = Hours + ":" + Minutes;
			return FormattedLogtime;

			} catch (Exception e) {
				LogError("ShowProperLogTime(string tStamp): "+ e.Message);
				throw new Exception("ShowProperLogTime Tryna Hardt: "+ e.Message);
			} 
		}

		public bool SonarrUsesSSL()
		{
			return chkSonarrSSL.Checked;
		}
	}

	class FinalOutputPackage
	{
		public HWstats HW;
		public List<PlexActivity> PLEX;
		public List<Episode> Sonarr;
	}
}
