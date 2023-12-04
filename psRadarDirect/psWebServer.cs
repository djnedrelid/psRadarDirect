using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace psRadarDirect
{
	class psWebServer
	{
		public static object GrafMatrixBufferLock = new object();
		HttpListener Listener = new HttpListener();
		public bool IsRunning {get; private set;}
		private Form1 MainForm;
		private string PORT;
		string DbQuery = "";
		CultureInfo ci = new CultureInfo("en-US");

		public void Start(string PORT, Form1 MainForm)
		{
			if (PORT == "")
				return;

			// Supported on current OS?
			if (!HttpListener.IsSupported) {
				MessageBox.Show("This OS is too old.","Sorry",MessageBoxButtons.OK,MessageBoxIcon.Hand);
				Environment.Exit(1);
				return;
			}

			// Set things up for starting.
			try {
			this.MainForm = MainForm;
			this.PORT = PORT;

			// Start hovedmotoren (main loop).
			Thread T = new Thread(new ThreadStart(psWebMainLoop));
			T.IsBackground = true;
			T.Start();

			} catch (Exception e) {
				Form1.LogError("(psWebServer.cs) Start(): "+ e.Message);
			}
		}

		private void psWebMainLoop()
		{
			try {
			Listener.Prefixes.Add("http://+:"+ PORT +"/");
			Listener.Start();
			IsRunning = true;
			IAsyncResult AsyncResult;

			while (IsRunning) {

				try {
				
				// Try to keep requests under N.
				if (MainForm.RequestsRunning >= 50)
					continue;

				// Wait for requests, then kick them off in their own thread.
				AsyncResult = Listener.BeginGetContext(new AsyncCallback(psWebDoorman),Listener);
				AsyncResult.AsyncWaitHandle.WaitOne();
				MainForm.UpdateWebRequestsRunning(true);

				} catch (Exception e) { 
					Form1.LogError("(psWebServer.cs) psWebMainLoop(inner): "+ e.Message); 
					break;
				}
			}

			// Stop() called, it lets the current iteration complete, 
			// we then stop the listener here once loop is broken.
			Listener.Stop();
			Listener.Prefixes.Clear();
			
			} catch (Exception e) {
				Form1.LogError("(psWebServer.cs) psWebMainLoop(outer): "+ e.Message +e.ToString());
			}
		}
		
		private void psWebDoorman(IAsyncResult result)
		{
				try {
				// Receiving work as a callback function, reporting back.
				HttpListener DoormanListener = (HttpListener)result.AsyncState;
				HttpListenerContext context = DoormanListener.EndGetContext(result);

				// Thread started, Listener is ready for new requests.
				string ourHTML = "";
				byte[] SendBackBuffer;
				Stream SendStream;
				string PrivatePIN = "";
				string PublicPIN = "";
				bool PrivateSession = false;
				bool PublicSession = false;
				string FullFileUrl = "";
				int BytesRead = 0;
				string EnteredPIN = "";

				// Check for a valid querystring PIN.
				foreach (string _qsi in context.Request.QueryString.AllKeys) {

					// Sjekk ukryptert PIN.
					if (_qsi == "PIN" && context.Request.QueryString[_qsi].Length == 4) {
						if (context.Request.QueryString[_qsi] == MainForm.PublicToken) {
							PublicPIN = context.Request.QueryString[_qsi];
							PublicSession = true;
							EnteredPIN = PublicPIN;
							break;

						} else if (context.Request.QueryString[_qsi] == MainForm.AgentToken) {
							PrivatePIN = context.Request.QueryString[_qsi];
							PrivateSession = true;
							EnteredPIN = PrivatePIN;
							break;
						}
					}

					// Sjekk kryptert PIN (fra android appen).
					if (_qsi == "PIN" && context.Request.QueryString[_qsi].Contains("-")) {
						if (djDecryptString(context.Request.QueryString[_qsi]) == MainForm.PublicToken) {
							PublicPIN = context.Request.QueryString[_qsi];
							PublicSession = true;
							EnteredPIN = PublicPIN;
							break;

						} else if (djDecryptString(context.Request.QueryString[_qsi]) == MainForm.AgentToken) {
							PrivatePIN = context.Request.QueryString[_qsi];
							PrivateSession = true;
							EnteredPIN = PrivatePIN;
							break;
						}
					}
				}

				// We only accept private or public authenticated requests based on our pin codes.
				if (!PrivateSession && !PublicSession) {
					ourHTML = HtmlSkeleton(context.Request.UserAgent,"","",EnteredPIN) + "No valid PIN received.</body></html>";
					SendBackBuffer = Encoding.UTF8.GetBytes(ourHTML);
					context.Response.ContentLength64 = SendBackBuffer.Length;
					SendStream = context.Response.OutputStream;
					SendStream.Write(SendBackBuffer,0,SendBackBuffer.Length);
					SendStream.Close();
					MainForm.UpdateWebRequestsRunning(false);
					return;
				}

				// Are we handling a Sonarr thumb?
				try {
				foreach (string _qsi in context.Request.QueryString.AllKeys) {
					if (
						_qsi == "Thumb" && 
						(context.Request.QueryString[_qsi].Contains(".jpg") || 
						 context.Request.QueryString[_qsi].Contains(".png"))
					) {
						FullFileUrl = Form1.WebFolder + @"\Images\SonarrPosters\"+ context.Request.QueryString[_qsi];
						if (File.Exists(FullFileUrl)) {
							using (MemoryStream NewPoster = new MemoryStream()) {
								context.Response.ContentType = "image/jpeg";
								SonarrPosterThumbGen(FullFileUrl).Save(NewPoster,ImageFormat.Jpeg);
								NewPoster.Position = 0;
								context.Response.ContentLength64 = NewPoster.Length;

								SendStream = context.Response.OutputStream;
								SendBackBuffer = new byte[64 * 1024]; // Send in 64 KB chunks.
								while ((BytesRead = NewPoster.Read(SendBackBuffer, 0, SendBackBuffer.Length)) > 0) {
									SendStream.Write(SendBackBuffer, 0, BytesRead);
								}
								SendStream.Close();
							}
							MainForm.UpdateWebRequestsRunning(false);
							return;
						} else {
							context.Response.StatusCode = 404;
						}
					}
				}} catch (Exception e) {
					if (!e.ToString().Contains("System.Net.HttpResponseStream.Write"))
						Form1.LogError("(psWebServer.cs) SonarrPosterThumb handling: "+ e.Message +e.ToString());
				}

				// Is it a specific file that's requested?
				if (
					context.Request.RawUrl.Contains(".png") ||
					context.Request.RawUrl.Contains(".jpg") || 
					context.Request.RawUrl.Contains(".gif") || 
					context.Request.RawUrl.Contains(".ico") || 
					context.Request.RawUrl.Contains(".css") || 
					context.Request.RawUrl.Contains(".js")
				) {
					FullFileUrl = Form1.WebFolder + context.Request.RawUrl.Replace('/','\\');
					FullFileUrl = FullFileUrl.Split('?')[0];

					if (FullFileUrl.Contains(".png"))
						context.Response.ContentType = "image/png";
					else if (FullFileUrl.Contains(".jpg"))
						context.Response.ContentType = "image/jpeg";
					else if (FullFileUrl.Contains(".gif"))
						context.Response.ContentType = "image/gif";
					else if (FullFileUrl.Contains(".css"))
						context.Response.ContentType = "text/css";
					else if (FullFileUrl.Contains(".js"))
						context.Response.ContentType = "text/javascript";
					else if (FullFileUrl.Contains(".ico"))
						context.Response.ContentType = "image/x-icon";

					if (File.Exists(FullFileUrl)) {
						using (FileStream fs = File.OpenRead(FullFileUrl)) {
							context.Response.ContentLength64 = fs.Length;
							SendStream = context.Response.OutputStream;
							SendBackBuffer = new byte[64 * 1024]; // Send in 64 KB chunks.
							while ((BytesRead = fs.Read(SendBackBuffer,0,SendBackBuffer.Length)) > 0) {
								SendStream.Write(SendBackBuffer,0,BytesRead);
							}
							SendStream.Close();
						}
						MainForm.UpdateWebRequestsRunning(false);
						return;
					} else {
						context.Response.StatusCode = 404;
					}
				}

				// FrontUI
				if (context.Request.RawUrl.Contains("FrontUI")) {
					ourHTML = HtmlSkeleton(context.Request.UserAgent,"FrontUI.css","",EnteredPIN) + 
					FrontUI(PrivateSession,PublicSession,EnteredPIN);

					SendBackBuffer = Encoding.UTF8.GetBytes(ourHTML);
					context.Response.ContentLength64 = SendBackBuffer.Length;
					context.Response.ContentType = "text/html";
					SendStream = context.Response.OutputStream;
					SendStream.Write(SendBackBuffer,0,SendBackBuffer.Length);
					SendStream.Close();
					MainForm.UpdateWebRequestsRunning(false);
					return;
				}

				// PlexUI
				if (context.Request.RawUrl.Contains("PlexUI")) {
					ourHTML = HtmlSkeleton(context.Request.UserAgent,"PlexUI.css","PlexUI.js",EnteredPIN) + 
					PlexUI(PrivateSession,PublicSession,EnteredPIN);

					SendBackBuffer = Encoding.UTF8.GetBytes(ourHTML);
					context.Response.ContentLength64 = SendBackBuffer.Length;
					context.Response.ContentType = "text/html";
					SendStream = context.Response.OutputStream;
					SendStream.Write(SendBackBuffer,0,SendBackBuffer.Length);
					SendStream.Close();
					MainForm.UpdateWebRequestsRunning(false);
					return;
				}

				// SonarrUI
				if (context.Request.RawUrl.Contains("SonarrUI")) {
					ourHTML = HtmlSkeleton(context.Request.UserAgent,"SonarrUI.css","SonarrUI.js",EnteredPIN) + 
					SonarrUI(PrivateSession,PublicSession,EnteredPIN);

					SendBackBuffer = Encoding.UTF8.GetBytes(ourHTML);
					context.Response.ContentLength64 = SendBackBuffer.Length;
					context.Response.ContentType = "text/html";
					SendStream = context.Response.OutputStream;
					SendStream.Write(SendBackBuffer,0,SendBackBuffer.Length);
					SendStream.Close();
					MainForm.UpdateWebRequestsRunning(false);
					return;
				}

				// PlexMonitorAjax	(.php)
				if (context.Request.RawUrl.Contains("PlexMonitorAjax")) {
					ourHTML = PlexMonitorAjax();
					SendBackBuffer = Encoding.UTF8.GetBytes(ourHTML);
					context.Response.ContentLength64 = SendBackBuffer.Length;
					context.Response.ContentType = "text/plain";
					SendStream = context.Response.OutputStream;
					SendStream.Write(SendBackBuffer,0,SendBackBuffer.Length);
					SendStream.Close();
					MainForm.UpdateWebRequestsRunning(false);
					return;
				}

				// PlexMonitorTopListsAjax	(.php)
				if (context.Request.RawUrl.Contains("PlexMonitorTopListsAjax")) {
					ourHTML = PlexMonitorTopListsAjax();
					SendBackBuffer = Encoding.UTF8.GetBytes(ourHTML);
					context.Response.ContentLength64 = SendBackBuffer.Length;
					context.Response.ContentType = "text/plain";
					SendStream = context.Response.OutputStream;
					SendStream.Write(SendBackBuffer,0,SendBackBuffer.Length);
					SendStream.Close();
					MainForm.UpdateWebRequestsRunning(false);
					return;
				}

				// HWMonitorAjaxSonarr	(.php)
				if (context.Request.RawUrl.Contains("HWMonitorAjaxSonarr")) {
					ourHTML = HWMonitorAjaxSonarr();
					SendBackBuffer = Encoding.UTF8.GetBytes(ourHTML);
					context.Response.ContentLength64 = SendBackBuffer.Length;
					context.Response.ContentType = "text/plain";
					SendStream = context.Response.OutputStream;
					SendStream.Write(SendBackBuffer,0,SendBackBuffer.Length);
					SendStream.Close();
					MainForm.UpdateWebRequestsRunning(false);
					return;
				}

				// HWMonitorAjax	(.php)
				if (context.Request.RawUrl.Contains("HWMonitorAjax")) {
					ourHTML = HWMonitorAjax();
					SendBackBuffer = Encoding.UTF8.GetBytes(ourHTML);
					context.Response.ContentLength64 = SendBackBuffer.Length;
					context.Response.ContentType = "text/plain";
					SendStream = context.Response.OutputStream;
					SendStream.Write(SendBackBuffer,0,SendBackBuffer.Length);
					SendStream.Close();
					MainForm.UpdateWebRequestsRunning(false);
					return;
				}
					

				// Standard tilbakemelding hvis ingen andre aksjoner har blitt oppfanget.
				ourHTML = HtmlSkeleton(context.Request.UserAgent,"","",EnteredPIN) + 
				"Hi there! You have entered a valid "+ (PrivateSession?"private":"public") +
				" PIN, but without any meaning or purpose!<br><br>"+
				"<b>psWebServer Default Page.</b><br>"+
				"&copy; 2018 Dag J Nedrelid &lt;dj@thronic.com>"+
				"</body></html>";
				SendBackBuffer = Encoding.UTF8.GetBytes(ourHTML);
				context.Response.ContentLength64 = SendBackBuffer.Length;
				SendStream = context.Response.OutputStream;
				SendStream.Write(SendBackBuffer,0,SendBackBuffer.Length);
				SendStream.Close();
				MainForm.UpdateWebRequestsRunning(false);

				} catch (Exception e) {
					/*
					 * Only care about errors if we're not shutting down. 
					 * Certain exceptions will be thrown regardless because of web clients 
					 * having orphaned AJAX calls and/or connections being broken from server 
					 * (i.e. restarts). We filter out the ones we experience during testing.
					*/ 
										
					if (
						!e.ToString().Contains("System.Net.HttpResponseStream.Write") && 
						!e.ToString().Contains("System.NullReferenceException") && 
						!e.ToString().Contains("System.ObjectDisposedException")
					)
						Form1.LogError("(psWebServer.cs) psWebDoorman(): Message: "+ e.Message +", String: "+ e.ToString() +", IsRunning:"+ IsRunning.ToString());
						
					MainForm.UpdateWebRequestsRunning(false);
				}
		}

		private Image SonarrPosterThumbGen(string PosterFile)
		{
			double maxWidth = 100;
			double maxHeight = 100;
			float newHeight, newWidth;
			double cHeight, cWidth;

			Image OriginalImage = Image.FromFile(PosterFile);
			cHeight = OriginalImage.Height;
			cWidth = OriginalImage.Width;
			newHeight = (float)cHeight;
			newWidth = (float)cWidth;

			// Check heigh first, make sure it's within maxHeight.
			if (newHeight > maxHeight) {
				newHeight = (float)maxHeight;
				newWidth = (float)Math.Round(cWidth / (cHeight / newHeight));
			}

			// Check width after, make sure it's within maxWidth.
			if (newWidth > maxWidth) {
				newHeight = (float)Math.Round(newHeight / (newWidth / maxWidth));
				newWidth = (float)maxWidth;
			}

			// Create the resized image in high quality.
			Image newImage = new Bitmap((int)newWidth, (int)newHeight);
			using (Graphics graphicsHandle = Graphics.FromImage(newImage)) {
				graphicsHandle.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graphicsHandle.DrawImage(OriginalImage, 0, 0, newWidth, newHeight);
			}

			// Return the image.
			return newImage;
		}

		private string SonarrUI(bool PrivateSession, bool PublicSession, string EnteredPIN)
		{
			string ReturnString = "";

			if (!PrivateSession && !PublicSession)
				return "Need a valid PIN to access this UI. /UnicornPolice.</body></html>";

			ReturnString += ""+
			"<div id=\"Main\">"+
			"<!-- HEADER CONTAINER -->"+
			"<div id=\"Header\">"+
			(PrivateSession ? "<div id=\"SwapPlexCalContainer\"><a href=\"PlexUI?PIN="+ EnteredPIN +"\">Go To Plex</a></div>" : "" ) +
			"<img src=\"/Images/plexitude_logo_web.png?PIN="+ EnteredPIN +"\" class=\"HeaderImg1\" alt=\"HeaderLogo\"><br>"+
			"<div style=\"clear:both\"></div>"+
			"</div>";

			ReturnString += ""+
			"<!-- LEFT MENU CONTAINER -->"+
			"<div id=\"Content\">"+
			BuildSonarrUI(PrivateSession, PublicSession, EnteredPIN) +
			"</div>";

			ReturnString += ""+
			"<!-- FOOTER CONTAINER -->"+
			"<!--<div id=\"Footer\">"+
			"Copyright &copy; 2018 Dag J Nedrelid<br>"+
			"Visit https://thronic.com for more info."+
			"</div>-->"+
			"</div>"+
			"<script type=\"text/javascript\">location.href = '#today';</script>";
			
			return ReturnString +"</body></html>";
		}

		private string BuildSonarrUI(bool PrivateSession, bool PublicSession, string EnteredPIN)
		{
			string ReturnString = "";
			bool FirstRecord = true;
			string AnchorDate = "";
			bool AnchorDropped = false;
			bool AnchorBackupDateSet = false;
			string EpisodeDateHeader = "";
			string SeasonEpisode = "";
			DateTime EpDate;
			List<BuildSonarrEpisode> DBResult = new List<BuildSonarrEpisode>();

			DbQuery = "SELECT Id,Year,Month,Day,SeriesTitle,SeasonNumber,EpisodeNumber,FileExists,PosterURL,AirDate "+
			"FROM sonarrcalendar ORDER BY AirDate DESC";

			using (SqlCeDataReader r = djDBI.ExecuteQuery(DbQuery, new qParam[] {})) {
				while (r.Read()) {
					 DBResult.Add(new BuildSonarrEpisode() {
						Id = (long)r[0],
						Year = (string)r[1],
						Month = (string)r[2],
						Day = (string)r[3],
						SeriesTitle = (string)r[4],
						SeasonNumber = (int)r[5],
						EpisodeNumber = (int)r[6],
						FileExists = (byte)r[7],
						PosterURL = (string)r[8],
						AirDate = (DateTime)r[9]
					});
				} 
				
				if (DBResult.Count == 0)
					return "Calendar is currently empty.";
			}

			DBResult.Reverse();

			// Figure out HTML anchor date first.
			foreach (BuildSonarrEpisode Ep in DBResult) {
				EpDate = DateTime.ParseExact(Ep.Year +"-"+ Ep.Month +"-"+ Ep.Day,"yyyy-MM-dd",ci);
				if (EpDate.Date == DateTime.Now.Date) {
					AnchorDate = EpDate.ToString("yyyy-MM-dd",ci);
					break;
				}

				// Tag a previous date we can use as backup if no episode exists today.
				if (Math.Round((DateTime.Now - EpDate).TotalDays) < 2 && !AnchorBackupDateSet) {
					AnchorDate = EpDate.ToString("yyyy-MM-dd",ci);
					AnchorBackupDateSet = true;
				}	
			}

			// Build content.
			foreach (BuildSonarrEpisode Ep in DBResult) {
				EpDate = DateTime.ParseExact(Ep.Year +"-"+ Ep.Month +"-"+ Ep.Day,"yyyy-MM-dd",ci);
				SeasonEpisode = "Season "+ Ep.SeasonNumber +", Episode "+ Ep.EpisodeNumber;

				// Drop scroll anchor on reasonable timespan (exact date may not have episodes).
				if (EpDate.ToString("yyyy-MM-dd",ci) == AnchorDate && !AnchorDropped) {
					ReturnString += "<a id=\"today\"></a>";
					AnchorDropped = true;
				}

				// Make single date header for all episodes under the same.
				if (EpisodeDateHeader != EpDate.ToString("dddd, MMMM d",ci)) {
					EpisodeDateHeader = EpDate.ToString("dddd, MMMM d",ci);
					ReturnString += (!FirstRecord ? "<br><br>":"") +
					"<div class=\"SonarrCalendarContainer SonarrEpisodeDateHeader\">"+
					EpisodeDateHeader + "</div>";
					FirstRecord = false;
				}

				ReturnString += ""+
				"<div class=\"SonarrCalendarContainer"+ (Ep.FileExists == 0 ? " SonarrCalendarContainerNoFile":"") +"\">"+
				(Ep.PosterURL != "" ? "<img src=\"/Images/SonarrPosterThumb?Thumb="+ Ep.PosterURL +"&PIN="+ EnteredPIN +"\" class=\"SonarrPosterThumb\">" : "")+
				"<span class=\"ContentHeader1\">"+ Ep.SeriesTitle +"</span><br><span class=\"SeasonEpisodeHeader\">"+ SeasonEpisode +"</span><br>"+
				(Ep.FileExists == 0 ? "<span class=\"EpisodeUNAvailable\">Upcoming.</span>":"<span class=\"EpisodeAvailable\">Available Now.</span>") +
				"<div style=\"clear:both\"></div></div>";
			}

			return ReturnString +"</body></html>";
		}

		private string PlexUI(bool PrivateSession, bool PublicSession, string EnteredPIN)
		{
			string ReturnString = "";

			if (!PrivateSession)
				return "Need private PIN to access this UI. /UnicornPolice.</body></html>";
				
			ReturnString += "<div id=\"Main\">"+
			"<!-- HEADER CONTAINER -->"+
			"<div id=\"Header\">"+
			"<div id=\"SwapPlexCalContainer\"><a href=\"SonarrUI?PIN="+ EnteredPIN +"\">Go To Calendar</a></div>"+
			"<!--<img src=\"/Images/plexitude_logo_web.png?PIN="+ EnteredPIN +"\" class=\"HeaderImg1\" alt=\"HeaderLogo\"><br>-->"+
			"<div style=\"clear:both\"></div><br>"+
			"<div id=\"Grafs\">"+
			"<!-- CPU Graf -->"+
			"<canvas id=\"Graf0\" height=\"100px\"></canvas>"+
			"<canvas id=\"GrafTimeline0\" height=\"18px\"></canvas>"+
			"<div id=\"HWinfo\">"+
				"<canvas id=\"GageBox0\" height=\"35px\"></canvas><br>"+
				"<div id=\"HWinfoSubtext\">&nbsp;</div>"+
			"</div>"+
			"<script style=\"text/javascript\">psRadarGrafSetup();</script>"+
			"</div></div>";

			ReturnString += ""+
			"<!-- CONTENT CONTAINER -->"+
			"<div id=\"Content\">"+
			"<div id=\"PlexContainer\"><div style=\"text-align:center\"><img src=\"/Images/plexitude-ajax-loader1.gif?PIN="+ EnteredPIN +"\" alt=\"Loading...\"></div></div>"+
			"<div id=\"PlexContainerTopLists\">&nbsp;</div>"+
			"</div>";

			ReturnString += ""+
			"<!-- FOOTER CONTAINER -->"+
			"<!--<div id=\"Footer\">"+
			"Copyright &copy; 2018 Dag J Nedrelid<br>"+
			"Visit https://thronic.com for more info. "+
			"</div>-->"+
			"</div>"+
			"<script type=\"text/javascript\">"+
				"HWMonitor('"+ EnteredPIN +"');"+
				"PlexMonitor('"+ EnteredPIN +"');"+
				"PlexMonitorTopLists('"+ EnteredPIN +"');"+
			"</script>";

			return ReturnString +"</body></html>";
		}

		private string FrontUI(bool PrivateSession, bool PublicSession, string EnteredPIN)
		{
			string ReturnString = "<div id=\"Main\">";

			ReturnString += "<div id=\"Content\">"+
			"<img src=\"/Images/plexitude_logo_web.png?PIN="+ EnteredPIN +"\" class=\"HeaderImg1\" alt=\"HeaderLogo\"><br><br>";
			
			if (PrivateSession || PublicSession)
				ReturnString += "<a href=\"/SonarrUI?PIN="+ EnteredPIN +"\">Calendar</a>";
			if (PrivateSession)
				ReturnString += "<a href=\"/PlexUI?PIN="+ EnteredPIN +"\">Plex</a>";
			
			ReturnString += "</div>";
			
			ReturnString += "<div id=\"Footer\">"+
			"<div style=\"color:#ccc\">"+
			"<b>DISCLAIMER</b><br>"+
			"Plex and Sonarr calendar content is fully owned and controlled by token owner and psRadar account user. "+
			"psRadar does not control, censor or police its users and the systems they connect to and monitor in any way. "+
			"They take full responsibility for any content they own and/or share from their own systems. psRadarDirect "+
			"is only a monitoring service. Contact the person(s) you received the link or token from if you have any questions.</div></div>";

			ReturnString += "</div>";
			return ReturnString +"</body></html>";
		}

		private string PlexMonitorAjax()
		{
			string ReturnString = "";
			string ActivityList = "";
			int NowPlayingCount = 0;
			DateTime LastUpdated;
			bool NowPlayed = false;
			DateTime TimestampLast, Timestamp;
			string TranscodingReport = "";

			try {
			// Check last update from server.
			DbQuery = "SELECT LastUpdated FROM hwstats";
			using (SqlCeDataReader r = djDBI.ExecuteQuery(DbQuery, new qParam[] {})) {
				if (!r.Read()) {
					ReturnString += "<span class=\"MiniHeader1\">No hardware info yet.</span><br><br>";
				} else {
					LastUpdated = (DateTime)r[0];
					if ((LastUpdated.CompareTo(DateTime.Now.AddSeconds(-30))) < 0) 
						ReturnString += "<span class=\"MiniHeader1\">NOTE: "+
						(DateTime.Now - LastUpdated).Days +" Days,"+
						(DateTime.Now - LastUpdated).Hours +" Hours,"+
						(DateTime.Now - LastUpdated).Minutes +" Minutes and "+
						(DateTime.Now - LastUpdated).Seconds +" Seconds since last update.</span><br><br>";
				}
			}} catch (Exception e) {
				Form1.LogError("(psWebServer.cs) PlexMonitorAjax(1): "+ e.Message + e.ToString());
			}
			
			try {
			// Activity list.
			DbQuery = "SELECT TOP 20 "+
				"b.Username, c.Id, c.PLEXActivityUserID, c.Title, c.Timestamp, c.TimestampLast, c.CurrentlyPlaying, "+
				"c.SeasonEpisode, c.StreamID, c.UserIPAddress, c.PlayerTitle, c.TranscodeVideo, c.TranscodeAudio, "+
				"c.TranscodeProgress "+
				"FROM plexactivityusers b INNER JOIN plexstats c ON c.PLEXActivityUserID=b.Id "+
				"ORDER BY c.Id DESC";
			using (SqlCeDataReader r = djDBI.ExecuteQuery(DbQuery, new qParam[] {})) {
				while (r.Read()) {
					
					// Set currently played if tagged as being played.
					NowPlayed = false;
					if ((byte)r[6] == 1)
						NowPlayed = true;

					// Check if being played is outdated by 10 seconds.
					TimestampLast = DateTime.Now;
					if (r[5] != DBNull.Value) {
						TimestampLast = DateTime.ParseExact((string)r[5],"yyyyMMddHHmmss",ci);
						if ((TimestampLast.CompareTo(DateTime.Now.AddSeconds(-10))) < 0) {
							DbQuery = "UPDATE plexstats SET CurrentlyPlaying=0 WHERE Id="+ r[1];
							djDBI.ExecuteNonQuery(DbQuery, new qParam[] {});
							NowPlayed = false;
						} else {
							++NowPlayingCount;
						}
					}

					Timestamp = DateTime.ParseExact((string)r[4],"yyyyMMddHHmmss",ci);
					// 11=video, 12=audio, 13=progress
					if ((byte)r[11] == 1 && (byte)r[12] == 1)
						TranscodingReport = "Transcoding Video and Audio at "+ r[13] +"&#37;";
					else if ((byte)r[11] == 1 && (byte)r[12] == 0)
						TranscodingReport = "Transcoding only Video at "+ r[13] +"&#37;";
					else if ((byte)r[11] == 0 && (byte)r[12] == 1)
						TranscodingReport = "Transcoding only Audio at "+ r[13] +"&#37;";
					else if ((byte)r[11] == 2 && (byte)r[12] == 1)
						TranscodingReport = "Transcoding Video(HW) and Audio at "+ r[13] +"&#37;";
					else if ((byte)r[11] == 2 && (byte)r[12] == 0)
						TranscodingReport = "Transcoding only Video(HW) at "+ r[13] +"&#37;";
					else
						TranscodingReport = "Direct play/stream.";

					ActivityList += ""+
					"<div class=\"PlexActivityContainer"+ (NowPlayed ? " PlexActivityContainerNowPlayed":"") +"\">"+
					"<span class=\"ContentHeader1\">"+ r[0] + "</span> <span class=\"PlexUiPlayer\">("+ r[10] +")</span><br>"+
					"<span class=\"HeaderText1\">"+ (r[3].ToString().Length>25 ? r[3].ToString().Substring(0,25) +"..." : r[3].ToString()) +"</span>"+ 
					((string)r[7]=="-" ? "":" <span class=\"PlexUiSeasonEpisode\">("+ r[7] +")</span>") +"<br>"+
					r[9] +" "+ Timestamp.ToString("MMMM d, HH:mm",ci) + (r[5]!=DBNull.Value?" - "+ TimestampLast.ToString("HH:mm"):"") +
					(NowPlayed?"<br>"+ TranscodingReport:"") + 
					"</div><br>";
				}
			
			}} catch (Exception e) {
				Form1.LogError("(psWebServer.cs) PlexMonitorAjax(2): "+ e.Message + e.ToString());
			}

			if (ActivityList == "") {
				ReturnString = "No activity.";
				return ReturnString;
			}

			if (NowPlayingCount > 0)
				ReturnString += "<span class=\"now-playing-header\">"+ NowPlayingCount +" Users Active</span><br><br>"+
						"<div id=\"ActivityList\">"+ ActivityList +"<br></div>";
			else
				ReturnString += "<div id=\"ActivityList\">"+ ActivityList +"<br></div>";
			
			return ReturnString;
		}

		private string PlexMonitorTopListsAjax()
		{
			string ReturnString = "";
			DateTime TimestampLast;
			int PopularityNr = 0;
			int PopularityNr2 = 0;
					
			try {	
			// Most popular activity.
			// Wrap most popular stuff in a container that can be styled based on browser type (PC or mobile).
			ReturnString += "<div id=\"MostPopularContainerActivities\">"+
			"<span class=\"MiniHeader1\">Most popular activity:</span><br><br>";
			
			DbQuery = "SELECT TOP 50 COUNT(c.Id) as StatCount, c.Title FROM plexstats c "+
			"GROUP BY c.Title ORDER BY StatCount DESC, c.Title ASC";
			using (SqlCeDataReader r = djDBI.ExecuteQuery(DbQuery, new qParam[] {})) {
				while (r.Read()) {
					++PopularityNr;
					ReturnString += PopularityNr +". "+ 
					(r[1].ToString().Length > 20 ? r[1].ToString().Substring(0,20) +"..." : r[1].ToString()) +
					" ("+ r[0] +" views)<br>";
				}
			
			}} catch (Exception e) {
				Form1.LogError("(psWebServer.cs) PlexMonitorAjax(3): "+ e.Message + e.ToString());
			}
			ReturnString += "</div>";

			try {
			// Most active user.
			// Wrap most popular stuff in a container that can be styled based on browser type (PC or mobile).
			PopularityNr = 0;
			ReturnString += "<div id=\"MostPopularContainerUsers\">"+
			"<span class=\"MiniHeader1\"> Most active users:</span><br><br>";

			DbQuery = "SELECT TOP 10 COUNT(c.Id) as StatCount, b.Username, b.Id as PlexActivityUserID "+
			"FROM plexactivityusers b INNER JOIN plexstats c ON c.PLEXActivityUserID=b.Id "+
			"GROUP BY b.Id, b.Username ORDER BY StatCount DESC, b.Username ASC";
			using (SqlCeDataReader r = djDBI.ExecuteQuery(DbQuery, new qParam[] {})) {
				while (r.Read()) {
					++PopularityNr;
					ReturnString += PopularityNr +". <b>"+ r[1] +"</b> ("+ r[0] +"):<br>"+
					"<div id=\"PlexUserActivity-"+ r[2] +"\" class=\"PlexUserActivityLayer\">";
					
					PopularityNr2 = 0;
					DbQuery = "SELECT TOP 10 COUNT(c.Id) as StatCount, c.Title "+
					"FROM plexactivityusers b INNER JOIN plexstats c ON c.PLEXActivityUserID=b.Id "+
					"WHERE b.Id="+ r[2] +" GROUP BY c.Title ORDER BY StatCount DESC,c.Title ASC"; 
					using (SqlCeDataReader r2 = djDBI.ExecuteQuery(DbQuery, new qParam[] {})) {
						while (r2.Read()) {
							++PopularityNr2;
							DbQuery = "SELECT TOP 1 TimestampLast FROM plexstats "+
							"WHERE Title=@Title AND PLEXActivityUserID="+ r[2] +
							"ORDER BY TimestampLast DESC";
							using (SqlCeDataReader r3 = djDBI.ExecuteQuery(DbQuery, new qParam[] {
								new qParam("@Title", r2[1])
							})) {
								while (r3.Read() && r3[0] != DBNull.Value) {
									TimestampLast = DateTime.ParseExact((string)r3[0],"yyyyMMddHHmmss",ci);
									ReturnString += PopularityNr2 +". " +
										(r2[1].ToString().Length>20 ? r2[1].ToString().Substring(0,20) +"..." : r2[1].ToString()) +
										" ("+ r2[0] +") "+
										TimestampLast.ToString("d MMM") +" '"+ TimestampLast.ToString("yy") +"<br>";
								}
							}
						}
					}
					ReturnString += "</div>";
				}
			}
			
			} catch (Exception e) {
				Form1.LogError("(psWebServer.cs) PlexMonitorAjax(4): "+ e.Message + e.ToString());
			}
			ReturnString += "</div>";
			
			return ReturnString;
		}

		private string HWMonitorAjax()
		{
			string ReturnString = "";
			DateTime LastUpdated;
			
			try {
			DbQuery = "SELECT LastUpdated,CPU,GPU,RAM,UPTIME FROM hwstats";
			using (SqlCeDataReader r = djDBI.ExecuteQuery(DbQuery, new qParam[] {})) {
				if (!r.Read()) {
					ReturnString = "No hardware info yet.";
				} else {
					LastUpdated = (DateTime)r[0];
					if ((LastUpdated.CompareTo(DateTime.Now.AddSeconds(-30))) < 0)
						ReturnString = "N/A (30+ seconds since last update.";
					else
						ReturnString = r[4].ToString();
						//ReturnString = r[3].ToString() +"<br>"+ r[4].ToString();
						/*
						ReturnString = ""+
						r[1] +"<br>"+
						(r[2].ToString().Contains("N/A") ? "" : r[2] +"<br>") +
						r[3] +"<br>"+
						r[4];
						*/
				}
			
			}
			
			// Add GrafMatrix buffer in JSON.
			lock(GrafMatrixBufferLock) {
				ReturnString += "GRAFSPLIT"+ JsonConvert.SerializeObject(MainForm.GrafMatrix);
			}

			} catch (Exception e) {
				Form1.LogError("(psWebServer.cs) HWMonitorAjax(): "+ e.Message + e.ToString());
			}

			return ReturnString;
		}

		private string HWMonitorAjaxSonarr()
		{
			string ReturnString = "Service is alive and well.";
			DateTime LastUpdated;

			try {
			DbQuery = "SELECT LastUpdated FROM hwstats";
			using (SqlCeDataReader r = djDBI.ExecuteQuery(DbQuery, new qParam[] {})) {
				if (!r.Read()) {
					ReturnString = "No hardware info yet.";
				} else {
					LastUpdated = (DateTime)r[0];
					if ((LastUpdated.CompareTo(DateTime.Now.AddSeconds(-30))) < 0) 
						ReturnString = (DateTime.Now - LastUpdated).Days +" Days,"+
						(DateTime.Now - LastUpdated).Hours +" Hours,"+
						(DateTime.Now - LastUpdated).Minutes +" Minutes and "+
						(DateTime.Now - LastUpdated).Seconds +" Seconds since last update.";
				}
			
			}} catch (Exception e) {
				Form1.LogError("(psWebServer.cs) PlexMonitorAjaxSonarr(): "+ e.Message + e.ToString());
			}

			return ReturnString;
		}

		public bool IsListening()
		{
			return Listener.IsListening;
		}

		private string Missing(string EnteredPIN)
		{
			return "<!doctype html>"+
			"<html>"+
			"<head>"+
			"<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">"+
			"<link href=\"/CSS/missing.css?PIN="+ EnteredPIN +"\" rel=\"stylesheet\" type=\"text/css\">"+
			"<title>psRadar Direct | Oops</title>"+
			"</head>"+
			"<body>"+
			"<img src=\"/images/404.png?PIN="+ EnteredPIN +"\"><br>"+
			"<b>404</b> - This link does not exist.<br><br>"+
			"</body>"+
			"</html>";
		}

		private string HtmlSkeleton(string UserAgent, string cssCustomFile, string jsCustomFile, string EnteredPIN)
		{
			string DocType;
			string cssFileName = "main.css";
			string jsFileName = "main.js";

			if(UserAgent.Contains("Android"))
				DocType = "<!DOCTYPE html PUBLIC \"-//WAPFORUM//DTD XHTML Mobile 1.2//EN"+
					"http://www.openmobilealliance.org/tech/DTD/xhtml-mobile12.dtd\">";
			else
				DocType = "<!doctype html>";
			
			string HTML = ""+
			"<html>"+
			"<head>"+
			"<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">"+
			"<link rel=\"shortcut icon\" href=\"/Images/favicon.ico?PIN="+ EnteredPIN +"\" type=\"image/x-icon\">"+
			"<link href=\"/CSS/"+ (cssCustomFile!=""?cssCustomFile:cssFileName) +"?PIN="+ EnteredPIN +"\" rel=\"stylesheet\" type=\"text/css\">";
			
			// PlexUI needs extra juice.
			if (cssCustomFile == "PlexUI.css")
				HTML += ""+
				"<script src=\"/JS/Graf.js?PIN="+ EnteredPIN +"\" type=\"text/javascript\"></script>";

			HTML += ""+
			"<script src=\"/JS/"+ (jsCustomFile!=""?jsCustomFile:jsFileName) +"?PIN="+ EnteredPIN +"\" type=\"text/javascript\"></script>"+
			"<title>psRadar Direct</title>";

			if (cssCustomFile == "PlexUI.css" && !UserAgent.Contains("Android"))
				HTML += "<style type=\"text/css\">"+
				"#Content {"+
					"background-color: #fff !important;"+
				"}"+
				"#PlexContainer {"+
					"width: 40% !important;"+
					"min-width: 500px !important;"+
					"float: left;"+
				"}"+
				"#PlexContainerTopLists {"+
					"width: auto !important;"+
					"float: left;"+
				"}"+
				"#MostPopularContainerActivities, "+
				"#MostPopularContainerUsers {"+
					"float: left;"+
					"padding-left: 10px;"+
					"font-size: 14px;"+
					"width: auto;"+
				"}"+
				"</style>";

			HTML +=""+
			"</head>"+
			"<body>";

			return DocType + HTML;
		}

		public void Stop()
		{
			IsRunning = false;

			try {
			// Trigger a nonsense call to shut down any loop blocking listener.
			WebRequest webRequest = WebRequest.Create("http://127.0.0.1:" + PORT);
			WebResponse webResponse = webRequest.GetResponse();
			} catch (Exception) {}
		}

		private string djDecryptString(string s)
		{
			//
			// s is the output from djEncryptString().
			// key is the same key as used to encrypt with.
			//

			if (Regex.IsMatch(s,"[^0-9-]"))
				return "";

			char[] key_chars = Form1.EncKey.ToCharArray();
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
	}

	class BuildSonarrEpisode
	{
		public long Id;
		public string Year;
		public string Month;
		public string Day;
		public string SeriesTitle;
		public int SeasonNumber;
		public int EpisodeNumber;
		public byte FileExists;
		public string PosterURL;
		public DateTime AirDate;
	}
}
