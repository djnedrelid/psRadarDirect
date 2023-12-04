using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using Newtonsoft.Json;

namespace psRadarDirect
{
	class Episode
	{
		// Calendar properties.
		public string airDateUtc;
		public bool hasFile;
		public int seriesId;
		public int episodeNumber;
		public int seasonNumber;
		public string SeriesTitle;
		
		// Infolayer properties.
		public string SeriesPosterURL;
		//public string FirstAired;
		//public string added;
		//public string genres;
		//public string SeriesOverview;
		//public string overview;

		/* NEEDED DATA.
		airDateUtc
		hasFile
		seriesId
		seasonNumber
		episodeNumber
		series > title

		// Infolayer
		series > images > [2] > url (coverType = poster)
		series > firstAired
		series > added
		series > genres
		series > overview
		overview
		*/
	}

	class Sonarr
	{
		public static object SonarrDataLock = new object();
		private HttpWebRequest webcall;
		private string API_URL = "http://";
		private string API_URL_SSL = "https://";
		private string API_KEY = "";
		private string Port = "";
		private string HostIP;
		private Form1 MainForm;
		public List<Episode> SonarrData = new List<Episode>();

		public Sonarr(string HostIP, Form1 MainForm)
		{
			this.HostIP = HostIP;
			this.MainForm = MainForm;

			// Start hovedmotoren (main loop).
			Thread T = new Thread(new ThreadStart(SonarrMainLoop));
			T.IsBackground = true;
			T.Start();
		}

		private void SonarrMainLoop()
		{
			while (true) {

				Thread.Sleep(3000);
				// Continuously search for Sonarr data. We need a separate thread for this 
				// since there's a lot more potential data which requires a longer timeout  
				// and it's OK that it takes longer than PLEX, but should not stand in the way.
				if (Port == "" || API_KEY == "") {
					SonarrData.Clear();
					continue;
				}

				if (!BuildRelevantJSONdata(HostIP))
					MainForm.lblChanger("Bad Sonarr Connection.", "Status");
			}
		}

		public bool BuildRelevantJSONdata(string ThisHost)
		{
			string UseURL = API_URL;

			if (MainForm.SonarrUsesSSL())
				UseURL = API_URL_SSL;
			
			string SonarrURL = UseURL + ThisHost +":"+ Port +"/api/calendar?apikey="+ API_KEY + GenerateStartStopSearchDates();
			string ResponseString = "";

			// Initialize web request.
			try {
				webcall = (HttpWebRequest)WebRequest.Create(SonarrURL);
				webcall.Timeout = 10000;
				webcall.ServicePoint.Expect100Continue = false;
				//webcall.ServicePoint.ConnectionLeaseTimeout = 5000;
				//webcall.ServicePoint.MaxIdleTime = 5000;
				webcall.KeepAlive = false;
				webcall.Proxy = null;
				webcall.Method = "GET";
				webcall.ContentType = "application/json";

				// Send the web request.
				using (StreamReader SR = new StreamReader(webcall.GetResponse().GetResponseStream())) {
					ResponseString = SR.ReadToEnd();
					webcall.Abort();
				}

				if (!ParseJSONResponse(ResponseString))
					return false;
			
			} catch (Exception e) {
				Form1.LogError("(Sonarr.cs) FAILED connecting to '"+ SonarrURL +"': "+ e.Message);
				return false;
			}

			return true;
		}

		private bool ParseJSONResponse(string s)
		{
			try {
			var _js = JsonConvert.DeserializeObject<dynamic>(s);
			// File.WriteAllText("DEBUG_JSON_DATA.txt", s);
			string PosterUrl = "";

			SonarrData.Clear();
			lock(SonarrDataLock) {
			foreach (var _jsi in _js) {
				// Handle the incredibly irregular poster image crap.
				try {
				if (_jsi["series"].images[0] != null && _jsi["series"].images[0].coverType == "poster")
					PosterUrl = _jsi["series"].images[0].url;
				else if (_jsi["series"].images[1] != null && _jsi["series"].images[1].coverType == "poster")
					PosterUrl = _jsi["series"].images[1].url;
				else if (_jsi["series"].images[2] != null && _jsi["series"].images[2].coverType == "poster")
					PosterUrl = _jsi["series"].images[2].url;
				
				} catch (Exception) {} // I guess no poster then.

				// Add episode data to export list.
				SonarrData.Add(new Episode {
					airDateUtc = _jsi.airDateUtc,
					hasFile = _jsi.hasFile,
					seriesId = _jsi.seriesId,
					episodeNumber = _jsi.episodeNumber,
					seasonNumber = _jsi.seasonNumber,
					SeriesTitle = _jsi["series"].title,
					SeriesPosterURL = PosterUrl
					//FirstAired = _jsi["series"].firstAired,
					//added = _jsi["series"].added,
					//genres = String.Join(", ", _jsi["series"].genres),
					//SeriesOverview = _jsi["series"].overview,
					//overview = _jsi.overview
				});

				// FOR DEBUGGING.
				/*File.AppendAllText(
					"DEBUG_JSON_LINES.txt", 
					(string) _jsi.airDateUtc +"\t"+
					(string) _jsi.hasFile +"\t"+
					(string) _jsi.seriesId +"\t"+
					(string) _jsi.episodeNumber +"\t"+
					(string) _jsi.seasonNumber +"\t"+
					(string) _jsi.title +"\t"+
					(string) _jsi.PosterUrl +"\t"+ Environment.NewLine
				);*/
			}} // Locking SonarrData in Form1.cs when it copies to DataRegistryHandler.cs

			return true;

			} catch (Exception e) {
				Form1.LogError("(Sonarr.cs) FAILED returning parsed JSON response: "+ e.Message +", "+ e.Data);
				return false;
			}
		}

		private string GenerateStartStopSearchDates()
		{
			//
			//	Set up start and end dates.
			//
			DateTime _today = DateTime.Today;
			DateTime FirstDayDate = _today.AddDays(-7);
			DateTime LastDayDate = _today.AddDays(14);
			DateTime NextMonthLastDayDate = new DateTime(LastDayDate.Year, LastDayDate.Month, LastDayDate.Day);

			string StartDate = FirstDayDate.ToString("yyyy-MM-dd") + "T00:00:00Z";
			string EndDate = NextMonthLastDayDate.ToString("yyyy-MM-dd") + "T23:59:59Z";

			return "&start="+ StartDate +"&end="+ EndDate;
		}

		public void SetPort(string s)
		{
			Port = s;
		}

		public void SetAPIKey(string s)
		{
			API_KEY = s;
		}
	}
}
