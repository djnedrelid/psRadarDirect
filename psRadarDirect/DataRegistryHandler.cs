using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace psRadarDirect
{
	class DataRegistryHandler
	{
		string DbQuery = "";
		private Form1 MainForm;
		CultureInfo ci = new CultureInfo("en-US");
		private string LastYCPUp = "0";
		private string LastYCPUc = "0";
		private string LastYGPUp = "0";
		private string LastYGPUc = "0";
		private string LastYRAM = "0";
		private Stopwatch TimeLineTimer = Stopwatch.StartNew();
		private string TLTime = "";

		public void ReceiveNewData(List<FinalOutputPackage> NewData, Form1 MainForm)
		{
			string _debugPoster = "";
			try {
				string PosterFileName = "";
				this.MainForm = MainForm;

				// Check DB size.
				double _fsz = new FileInfo(Form1.psRadarDirectDB).Length;
				double _fszKiB = Math.Round(_fsz/1024,1);
				if (_fszKiB < 1024)
					MainForm.lblChanger("DB Size: "+ _fszKiB.ToString() +" KB","DBSize");
				else if (_fszKiB > 1024)
					MainForm.lblChanger("DB Size: "+ (_fszKiB/1024).ToString() +" MB","DBSize");
					
				if (_fszKiB >= 2097152) // Rotate DB at 2 GB.
					CleanupDB();

				// HW stats.	
				RegisterHWStats(
					(string)NewData[0].HW.CPU,
					(string)NewData[0].HW.GPU,
					(string)NewData[0].HW.RAM,
					(string)NewData[0].HW.Uptime
				);

				// Plex.
				foreach (var _jsi in NewData[0].PLEX) {
					RegisterPlexActivity(
						_jsi.UserName,
						_jsi.MovieOrSeriesTitle,
						_jsi.SeasonEpisode,
						_jsi.SessionKey,
						_jsi.UserIPAddress,
						_jsi.PlayerTitle,
						_jsi.TranscodeVideo,
						_jsi.TranscodeAudio,
						_jsi.TranscodeProgress
					);
				}

				// Sonarr.				
				if (!MainForm.SonarrRelaxRound) {
				djDBI.PrepTransaction();

				foreach (var _jsi in NewData[0].Sonarr) {

					// Skip registration if SNR object is in a relaxed round (every 5 sec).
					if (MainForm.SonarrRelaxRound == true)
						break;

					// Fix a local copy of the poster image.
					if (_jsi.SeriesPosterURL != "") {
						
						PosterFileName = Regex.Replace(_jsi.SeriesTitle,@"[^a-zA-Z0-9_.]","") +".jpg";
						if (!File.Exists(Form1.SonarrPosterFolder + PosterFileName)) {
							using (WebClient wc = new WebClient()) {
								// Debug.
								//_debugPoster = "; "+ _jsi.SeriesPosterURL +">"+ Form1.SonarrPosterFolder + PosterFileName +" ";
								try {
									wc.DownloadFile(
										_jsi.SeriesPosterURL,
										Form1.SonarrPosterFolder + PosterFileName
									);
								} catch (Exception) {
									PosterFileName = "No_Picture_Available.png";
								}
							}
						}	

					} else {
						PosterFileName = "No_Picture_Available.png";
					}
					
					RegisterSonarrActivity(
						(string)_jsi.airDateUtc,
						(bool)_jsi.hasFile,
						(int)_jsi.seriesId,
						(int)_jsi.episodeNumber,
						(int)_jsi.seasonNumber,
						(string)_jsi.SeriesTitle,
						PosterFileName
					);

				} 
				
				// Commit transaction.
				djDBI.CommitTransaction();
				CleanSonarrDB(NewData[0].Sonarr);
				MainForm.SonarrRelaxRound = true; 
				}	
				
			} catch (Exception e) {
				Form1.LogError(
					"(DataRegistryHandler.cs) ReceiveNewData(): "+ e.Message + e.ToString() 
					+ _debugPoster
				);
			}
		}

		private void CleanupDB()
		{
			try {
			// This remains a guesstimate until I know how many records it's wise to eat away to rotate a 2GB SDF. 
			DbQuery = "DELETE FROM plexstats WHERE Id IN (SELECT TOP (25000) Id FROM plexstats ORDER BY Id ASC)";			
			djDBI.ExecuteNonQuery(DbQuery, new qParam[] {});
			
			// Remove users that are no longer with us because of inactivity.
			DbQuery = "DELETE FROM plexactivityusers WHERE Id NOT IN (SELECT PLEXActivityUserID FROM plexstats)";
			djDBI.ExecuteNonQuery(DbQuery, new qParam[] {});
				
			Form1.LogError("(DataRegistryHandler.cs) Cleaned up old activity records and inactive users.\n");
			
			} catch (Exception e) { 
				Form1.LogError("(DataRegistryHandler.cs) CleanupDB(): "+ e.Message); 
			}
		}

		private void RegisterHWStats(
			string _CPU, 
			string _GPU, 
			string _RAM, 
			string _Uptime)
		{
			MatchCollection CpuValues, GpuValues, RamValue;
			Regex CpuRegex, GpuRegex, RamRegex;
		
			try {
			
			DbQuery = "SELECT id FROM hwstats";
			using (SqlCeDataReader r = djDBI.ExecuteQuery(DbQuery, new qParam[] {})) {
				if (r.Read())
					DbQuery = "UPDATE hwstats SET "+
					"CPU='"+ _CPU +"',"+
					"GPU='"+ _GPU +"',"+
					"RAM='"+ _RAM +"',"+
					"UPTIME='"+ _Uptime +"',"+
					"LastUpdated='"+ DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss",ci) +"'";
				else
					DbQuery = "INSERT INTO hwstats "+
					"(CPU,GPU,RAM,UPTIME) VALUES "+
					"('"+ _CPU +"','"+ _GPU +"','"+ _RAM +"','"+ _Uptime +"')";
			}

			// Execute update/insert.
			djDBI.ExecuteNonQuery(DbQuery, new qParam[] {});

			//
			// Save in buffer for web interface:
			//

			if (TimeLineTimer.ElapsedMilliseconds >= 120000) {
				TLTime = Form1.ShowProperLogTime(DateTime.Now.ToString("HHmm"));
				TimeLineTimer.Restart();
			}

			lock(psWebServer.GrafMatrixBufferLock) {

			// CPU %
			CpuRegex = new Regex(@"([0-9]{1,3},*[0-9]{0,2})");
			CpuValues = CpuRegex.Matches(_CPU);
			MainForm.GrafMatrix.Enqueue(new GrafMatrixItem {
				LastX = "0", 
				LastY = LastYCPUp,
				NewX = "0", 
				NewY = CpuValues[0].Groups[1].Value,
				GrafColor = "#0066ff",
				Name = "CPU %",
				Time = TLTime
			});
			LastYCPUp = CpuValues[0].Groups[1].Value;

			// CPU C
			MainForm.GrafMatrix.Enqueue(new GrafMatrixItem {
				LastX = "0",
				LastY = LastYCPUc,
				NewX = "0",
				NewY = CpuValues[1].Groups[1].Value,
				GrafColor = "#ff0000",
				Name = "CPU C",
				Time = ""
			});
			LastYCPUc = CpuValues[1].Groups[1].Value;

			// GPU %
			GpuRegex = new Regex(@"([0-9]{1,3},*[0-9]{0,2})");
			GpuValues = CpuRegex.Matches(_GPU);
			if (GpuValues.Count > 0) {
				MainForm.GrafMatrix.Enqueue(new GrafMatrixItem {
					LastX = "0",
					LastY = LastYGPUp,
					NewX = "0",
					NewY = (GpuValues[0].Value.Contains("N/A") ? "" : GpuValues[0].Groups[1].Value),
					GrafColor = "#66cc00",
					Name = "GPU %",
					Time = ""
				});
				LastYGPUp = (GpuValues[0].Value.Contains("N/A") ? "" : GpuValues[0].Groups[1].Value);

				// GPU C
				MainForm.GrafMatrix.Enqueue(new GrafMatrixItem {
					LastX = "0",
					LastY = LastYGPUc,
					NewX = "0",
					NewY = (GpuValues[0].Value.Contains("N/A") ? "" : GpuValues[1].Groups[1].Value),
					GrafColor = "#ffcc00",
					Name = "GPU C",
					Time = ""
				});
				LastYGPUc = (GpuValues[0].Value.Contains("N/A") ? "" : GpuValues[1].Groups[1].Value);
			} else {
				MainForm.GrafMatrix.Enqueue(new GrafMatrixItem {
					LastX = "0",
					LastY = LastYGPUp,
					NewX = "0",
					NewY = "0",
					GrafColor = "#66cc00",
					Name = "GPU %",
					Time = ""
				});
				LastYGPUp = "0";

				// GPU C
				MainForm.GrafMatrix.Enqueue(new GrafMatrixItem {
					LastX = "0",
					LastY = LastYGPUc,
					NewX = "0",
					NewY = "0",
					GrafColor = "#ffcc00",
					Name = "GPU C",
					Time = ""
				});
				LastYGPUc = "0";
			}

			// RAM %
			RamRegex = new Regex(@"([0-9]{1,3},*[0-9]{0,2})");
			RamValue = RamRegex.Matches(_RAM);
			MainForm.GrafMatrix.Enqueue(new GrafMatrixItem {
				LastX = "0",
				LastY = LastYRAM,
				NewX = "0",
				NewY = RamValue[2].Groups[1].Value,
				GrafColor = "#cccccc",
				Name = "RAM %",
				Time = ""
			});
			LastYRAM = RamValue[2].Groups[1].Value;

			TLTime = "";

			// Keep the matrix width content at 4K support.
			while (MainForm.GrafMatrix.Count > (5*3840)) {
				MainForm.GrafMatrix.Dequeue();
			}}

			} catch (Exception e) {
				Form1.LogError("(DataRegistryHandler.cs) RegisterHWStats(): "+ e.Message);
			}
		}

		private void RegisterPlexActivity (
			string Username,
			string MovieOrSeriesTitle,
			string SeasonEpisode,
			int SessionKey,
			string UserIPAddress,
			string PlayerTitle,
			int TranscodeVideo,
			int TranscodeAudio,
			int TranscodeProgress) 
		{
			try {
			long PlexUserID = 0;
			bool DoLog = true;
			DateTime CurPlexActivityDateTime;
			DateTime LogPlexActivityDateTime;

			// Get the username id.
			DbQuery = "SELECT id FROM plexactivityusers WHERE Username='"+ Username +"'";
			using (SqlCeDataReader r = djDBI.ExecuteQuery(DbQuery, new qParam[] {})) {
				if (r.Read())
					PlexUserID = (long)r[0];
			}

			// Create a new user if one didn't already exist.
			if (PlexUserID == 0) {
				DbQuery = "INSERT INTO plexactivityusers (Username) VALUES ('"+ Username +"')";
				djDBI.ExecuteNonQuery(DbQuery, new qParam[] {});

				DbQuery = "SELECT id FROM plexactivityusers WHERE Username = '"+ Username +"'";
				using (SqlCeDataReader r = djDBI.ExecuteQuery(DbQuery, new qParam[] {})) {
					if (r.Read())
						PlexUserID = (long)r[0];
				}
			}

			// Search for existing activity.
			DbQuery = "SELECT Id,Timestamp FROM plexstats "+
				"WHERE PLEXActivityUserID = "+ PlexUserID +" AND "+
				"Title = @MovieOrSeriesTitle AND "+
				"SeasonEpisode = '"+ SeasonEpisode + "' AND "+
				"StreamID = "+ SessionKey +" "+
				"ORDER BY id DESC";
				
			using (SqlCeDataReader r = djDBI.ExecuteQuery(DbQuery, new qParam[] {
				new qParam("@MovieOrSeriesTitle", MovieOrSeriesTitle)
			})) {
				if (r.Read()) {
					// Check for freak accident that an old record may still have same stream ID.
					// and may wrongfully update that record instead which won't show in recent history UX.
					LogPlexActivityDateTime = DateTime.ParseExact((string)r[1], "yyyyMMddHHmmss", ci);
					CurPlexActivityDateTime = DateTime.Now;

					if ((CurPlexActivityDateTime-LogPlexActivityDateTime).TotalDays <= 1) {

						DoLog = false;
						DbQuery = "UPDATE plexstats SET " +
						"TimestampLast = '" + DateTime.Now.ToString("yyyyMMddHHmmss",ci) + "'," +
						"CurrentlyPlaying=1," +
						"TranscodeVideo=" + TranscodeVideo.ToString() + "," +
						"TranscodeAudio=" + TranscodeAudio.ToString() + "," +
						"TranscodeProgress=" + TranscodeProgress.ToString() + " " +
						"WHERE id=" + r[0].ToString();
						djDBI.ExecuteNonQuery(DbQuery, new qParam[]{});

					} else {
						DbQuery = "UPDATE plexstats SET StreamID=0 WHERE Id="+ r[0];
						djDBI.ExecuteNonQuery(DbQuery, new qParam[]{});
					}
				}
			}

			if (DoLog) {
				// Log new activity.
				DbQuery = "INSERT INTO plexstats "+
					"(PLEXActivityUserID,Title,Timestamp,SeasonEpisode,StreamID,UserIPAddress,"+
					"PlayerTitle,TranscodeVideo,TranscodeAudio,TranscodeProgress) VALUES "+
					"("+
						PlexUserID +","+
						"@MovieOrSeriesTitle,"+
						"@CurrentTime,"+
						"@SeasonEpisode,"+
						SessionKey +","+
						"@UserIPAddress,"+
						"@PlayerTitle,"+
						(byte)TranscodeVideo +","+
						(byte)TranscodeAudio +","+
						TranscodeProgress +
					")";

				djDBI.ExecuteNonQuery(DbQuery, new qParam[] {
					new qParam("@MovieOrSeriesTitle", MovieOrSeriesTitle),
					new qParam("@CurrentTime", DateTime.Now.ToString("yyyyMMddHHmmss",ci)),
					new qParam("@SeasonEpisode", SeasonEpisode),
					new qParam("@UserIPAddress", UserIPAddress),
					new qParam("@PlayerTitle", PlayerTitle)
				});

			}} catch (Exception e) {
				Form1.LogError("(DataRegistryHandler.cs) RegisterPlexActivity(): "+ e.Message + e.ToString() +" "+ 
				MovieOrSeriesTitle +", "+ DateTime.Now.ToString("yyyyMMddHHmmss",ci) +", "+
				SeasonEpisode +", "+ SessionKey +", "+ UserIPAddress +", "+ PlayerTitle +
				", "+ TranscodeVideo +", "+ TranscodeAudio +", "+ TranscodeProgress);
				// Supernatural, 20180307230109, S13 - E14, 139, 37.200.28.30, XboxOne, 0, 0, 0
			}
		}

		private void RegisterSonarrActivity (
			string airDateUtc,
			bool hasFile,
			int seriesId,
			int episodeNumber,
			int seasonNumber,
			string SeriesTitle,
			string SeriesPosterURL)
		{
			try {
			DateTime _airDateUtc = DateTime.ParseExact(airDateUtc,"MM/dd/yyyy HH:mm:ss",ci);

			// Register new or update existing.
			DbQuery = "SELECT id,FileExists FROM sonarrcalendar WHERE "+
				"SeriesTitle = @SeriesTitle AND "+
				"SeasonNumber = "+ seasonNumber +" AND "+
				"EpisodeNumber = "+ episodeNumber;
			
			using (SqlCeDataReader r = djDBI.ExecuteQuery(DbQuery, new qParam[]{
				new qParam("@SeriesTitle", SeriesTitle)
			})) {
				if (r.Read()) {
					DbQuery = "UPDATE sonarrcalendar SET FileExists="+ (byte)(hasFile?1:0) +" WHERE id="+ r[0];
					djDBI.ExecuteNonQuery(DbQuery, new qParam[]{});
				} else {
					DbQuery = "INSERT INTO sonarrcalendar "+
					"(Year,Month,Day,SeriesTitle,SeasonNumber,EpisodeNumber,FileExists,PosterURL,AirDate) VALUES "+
					"("+
						"'"+ _airDateUtc.ToString("yyyy") +"',"+
						"'"+ _airDateUtc.ToString("MM") +"',"+
						"'"+ _airDateUtc.ToString("dd") +"',"+
						"@SeriesTitle,"+
						seasonNumber +","+
						episodeNumber +","+
						(byte)(hasFile?1:0) +","+
						"@SeriesPosterURL,"+
						"@airDateUtc"+
					")";
					djDBI.ExecuteNonQuery(DbQuery, new qParam[]{
						new qParam("@SeriesTitle",SeriesTitle),
						new qParam("@SeriesPosterURL",SeriesPosterURL),
						new qParam("@airDateUtc",_airDateUtc),
					});
				}

			}} catch (Exception e) {
				Form1.LogError("(DataRegistryHandler.cs) RegisterSonarrActivity(): "+ e.Message + e.ToString());
			}
		}

		private void CleanSonarrDB(List<Episode> NewSonarrData)
		{
			try {
			bool DeleteRecord = false;
			DateTime AirDate;
			
			// Return if there is no data.
			DbQuery = "SELECT id,Month,Day,SeriesTitle,SeasonNumber,EpisodeNumber,PosterURL FROM sonarrcalendar";
			using (SqlCeDataReader r = djDBI.ExecuteQuery(DbQuery, new qParam[] {})) {
				while (r.Read()) {
					DeleteRecord = true;
					foreach (var new_r in NewSonarrData) {
						AirDate = DateTime.ParseExact(new_r.airDateUtc, "MM/dd/yyyy HH:mm:ss", ci);
						if (
							new_r.SeriesTitle == (string)r[3] &&
							new_r.seasonNumber == (int)r[4] &&
							new_r.episodeNumber == (int)r[5] &&
							AirDate.ToString("MM") == (string)r[1] &&
							AirDate.ToString("dd") == (string)r[2]
						) {
							DeleteRecord = false;
						}

						if (!DeleteRecord)
							break;
					}

					// Did not find the existing record in the new data collection. Delete it, it's old.
					if (DeleteRecord) {
						DbQuery = "DELETE FROM sonarrcalendar WHERE id=" + r[0];
						djDBI.ExecuteNonQuery(DbQuery, new qParam[] {});
					}
				}

			}} catch (Exception e) {
				Form1.LogError("(DataRegistryHandler.cs) CleanSonarrDB(): "+ e.Message);
			}
		}
	}
}
