using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;

namespace psRadarDirect
{
	class PlexActivity
	{
		public string UserName;
		public string MovieOrSeriesTitle;
		public string SeasonEpisode;
		public int SessionKey;
		public string UserIPAddress;
		public string PlayerTitle;
		public int TranscodeVideo;
		public int TranscodeAudio;
		public int TranscodeProgress;	
	}

	class PlexMediaServer
	{
		public bool Connected {get; private set;}
		private string PlexURL;
		private string AuthToken;
		private XmlDocument PMS_XML;
		private HttpWebRequest webcall;
		public List<PlexActivity> pActivities = new List<PlexActivity>();

		public PlexMediaServer()
		{
			Connected = false;
		}

		public void Disconnect()
		{
			Connected = false;
		}

		public int Connect(string IP, string Port, string User, string Pass)
		{
			PlexURL = "http://"+ IP +":"+ Port;
			string LoginURL = "https://my.plexapp.com/users/sign_in.xml";
			string Credentials;
			string ResponseString;
			XmlNode AuthNode;

			// Initialize autentication details.
			Credentials = Convert.ToBase64String(
				Encoding.UTF8.GetBytes(User +":"+ Pass)
			);

			// Initialize web request.
			try {
				webcall = (HttpWebRequest)WebRequest.Create(LoginURL);
				webcall.Timeout = 5000;
				webcall.KeepAlive = false;
				webcall.Proxy = null;
				webcall.Method = "POST";
				webcall.ContentType = "text/xml";
				webcall.ContentLength = 0;
				webcall.Headers.Add("X-Plex-Client-Identifier", "PlexActivityLogger");
				webcall.Headers.Add("Authorization", "Basic "+ Credentials);

				// Send the web request.
				using (StreamReader SR = new StreamReader(webcall.GetResponse().GetResponseStream())) {
					ResponseString = SR.ReadToEnd();
					webcall.Abort();
				}
			
			} catch (Exception e) {
				Form1.LogError("(PlexMediaServer.cs) FAILED connecting to my.plexapp.com: "+ e.Message);
				return 2;
			}

			// Check if expected XML was received.
			PMS_XML = new XmlDocument();
			try { PMS_XML.LoadXml(ResponseString); } 
			catch (Exception) { 
				Form1.LogError("(PlexMediaServer.cs) FAIL could not parse XML from my.plexapp.com: "+ ResponseString);
				return 2; 
			}

			// Check if authentication-token was received.
			AuthNode = PMS_XML.SelectSingleNode("user/authentication-token");
			if (AuthNode == null) {
				return 2;
			} else {
				AuthToken = AuthNode.InnerText;
				Connected = true;
				return 1;
			}
		}

		public bool CheckActivity()
		{
			if (!Connected)
				return false;

			string ActivityURL;
			string ResponseString;
			string PlayerTitleNonEmpty;
			int TranscodeVideoIn = 0;
			int TranscodeAudioIn = 0;
			int TranscodeProgressIn = 0;
			XmlNode TranscodeSession;
			pActivities.Clear();

			// The session URL to retrieve activity XML data.
			ActivityURL = 
				PlexURL + 
				"/status/sessions/?X-Plex-Token=" +
				AuthToken;
			
			// Make the web call.
			try {
				webcall = (HttpWebRequest)WebRequest.Create(ActivityURL);
				webcall.Timeout = 5000;
				webcall.KeepAlive = false;
				webcall.Proxy = null;
				using (StreamReader SR = new StreamReader(webcall.GetResponse().GetResponseStream())) {
					ResponseString = SR.ReadToEnd();
					webcall.Abort();
				}
			
			} catch (Exception e) {
				Connected = false;
				Form1.LogError("(PlexMediaServer.cs) CheckActivity(): "+ e.Message);
				return false;
			}

			PMS_XML = new XmlDocument();
			try {
				PMS_XML.LoadXml(ResponseString);
			} catch (Exception e) {
				Connected = false;
				Form1.LogError("(PlexMediaServer.cs) FAILED trying to read XML from specified IP/Host: "+ e.Message);
				return false;
			}

			// Look for VIDEO activity.
			try {
			foreach (XmlNode Video in PMS_XML.SelectNodes("MediaContainer/Video")) {
				
				// Handle transcoding information.
				TranscodeSession = Video.SelectSingleNode("TranscodeSession");
				if (TranscodeSession == null) {
					// Do nothing.
				} else {

					if (TranscodeSession.Attributes["videoDecision"] != null)
						TranscodeVideoIn = (TranscodeSession.Attributes["videoDecision"].Value == "transcode" ? 1 : 0);
					
					if (TranscodeSession.Attributes["audioDecision"] != null)
						TranscodeAudioIn = (TranscodeSession.Attributes["audioDecision"].Value == "transcode" ? 1 : 0);
					
					if (TranscodeSession.Attributes["transcodeHwRequested"] != null) {
						if (TranscodeSession.Attributes["transcodeHwRequested"].Value == "1" && TranscodeVideoIn == 1) 
						{
							TranscodeVideoIn = 2;
						}
					}

					TranscodeProgressIn = Int32.Parse(TranscodeSession.Attributes["progress"].Value.Split('.')[0]);
				}
				
				// Handle player title.
				if (Video.SelectSingleNode("Player").Attributes["title"].Value != "")
					PlayerTitleNonEmpty = Video.SelectSingleNode("Player").Attributes["title"].Value;
				else if (Video.SelectSingleNode("Player").Attributes["product"].Value != "")
					PlayerTitleNonEmpty = Video.SelectSingleNode("Player").Attributes["product"].Value;
				else if (Video.SelectSingleNode("Player").Attributes["profile"].Value != "")
					PlayerTitleNonEmpty = Video.SelectSingleNode("Player").Attributes["profile"].Value;
				else
					PlayerTitleNonEmpty = "N/A";

				// Add activity to collection.
				pActivities.Add(new PlexActivity {
					UserName = Video.SelectSingleNode("User").Attributes["title"].Value,
					MovieOrSeriesTitle = (Video.Attributes["type"].Value == "episode" ? Video.Attributes["grandparentTitle"].Value : Video.Attributes["title"].Value),
					SeasonEpisode = (Video.Attributes["type"].Value == "episode" ? "S" + Video.Attributes["parentIndex"].Value +"/E"+ Video.Attributes["index"].Value : "-"),
					SessionKey = Int32.Parse(Video.Attributes["sessionKey"].Value),
					UserIPAddress = Video.SelectSingleNode("Player").Attributes["address"].Value,
					PlayerTitle = PlayerTitleNonEmpty,
					TranscodeAudio = TranscodeAudioIn,
					TranscodeVideo = TranscodeVideoIn,
					TranscodeProgress = TranscodeProgressIn
				});
			}
				return true;
			
			} catch (Exception e) {
				Connected = false;
				Form1.LogError("(PlexMediaServer.cs) FAILED trying to parse XML from specified IP/Host: "+ e.Message);
				return false;
			}
		}
	}
}
