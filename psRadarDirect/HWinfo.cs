using System;
using System.Collections.Generic;
using OpenHardwareMonitor.Hardware;
using System.Diagnostics;
using System.Windows.Forms;

namespace psRadarDirect
{
	struct HWstats 
	{
		public string CPU;
		public string GPU;
		public string RAM;
		public string Uptime;
	}

	public struct GrafMatrixItem
	{
		public string LastX;
		public string LastY;
		public string NewX;
		public string NewY;
		public string GrafColor;
		public string Name;
		public string Time;
	}

	class HWinfo
	{
		private Computer HWi = new Computer();
		private Dictionary<string,string> SensorNames = new Dictionary<string, string>();
		
		public HWinfo(string Type)
		{
			try {
			switch (Type)
			{
				// Bruk OHM/LHM programmet for å lese av navn vi kan bruke på sensorer.
				// Ligger lenker og nåværende programmer under prosjektmappen ressurser.
				
				case "CPU":
					HWi.CPUEnabled = true;
					SensorNames.Add("TotalLoad", "CPU Total");
					SensorNames.Add("TotalTemp", "CPU Package");
					break;

				case "GPU":
					HWi.GPUEnabled = true;
					SensorNames.Add("TotalLoad", "GPU Core");
					SensorNames.Add("TotalTemp", "GPU Core");
					break;

				case "RAM":
					HWi.RAMEnabled = true;
					SensorNames.Add("TotalLoad", "Memory");
					SensorNames.Add("UsedRAM", "Used Memory");
					SensorNames.Add("FreeRAM", "Available Memory");
					break;
			}

			HWi.Open();
			} catch (Exception e) {
				MessageBox.Show("HWinfo.cs,HWinfo() problem: "+ e.Message);
			}
		}

		public string getUptime()
		{
			TimeSpan TS;
			try {
			
			using (var uptime = new PerformanceCounter("System", "System Up Time")) {
				uptime.NextValue(); //Call this an extra time before reading its value
				TS = TimeSpan.FromSeconds(uptime.NextValue());
			}

			return "Up "+ TS.Days.ToString() +" Days, "+ TS.Hours.ToString() +" Hours, "+ TS.Minutes.ToString() +" Min.";

			} catch(Exception e) {
				MessageBox.Show("HWinfo.cs,getUptime() problem: "+ e.Message);
				return "Uptime N/A";
			}
		}

		public string getAvailableRAM()
		{
			string AvailableRAM = "";
			decimal UsedRAM = 0;
			decimal FreeRAM = 0;
			int LoadRAM = 0;

			try {
			HWi.Hardware[0].Update();

			foreach (var s in HWi.Hardware[0].Sensors) {
				if (s.SensorType == SensorType.Data && s.Name == SensorNames["UsedRAM"])
					UsedRAM = Math.Round((decimal)s.Value,1);

				if (s.SensorType == SensorType.Data && s.Name == SensorNames["FreeRAM"])
					FreeRAM = Math.Round((decimal)s.Value,1);

				if (s.SensorType == SensorType.Load && s.Name == SensorNames["TotalLoad"])
					LoadRAM = (int)Math.Round((decimal)s.Value);
			}

			AvailableRAM = UsedRAM.ToString("0.0") +"/"+ (UsedRAM + FreeRAM) +" GB ("+ LoadRAM +" %)";
			return AvailableRAM;

			} catch (Exception e) {
				MessageBox.Show("HWinfo.cs,getAvailableRAM() problem: "+ e.Message);
				return "N/A";
			}
		}

		public string getLoad()
		{
			string Load = "N/A";
			
			try {
			HWi.Hardware[0].Update();

			foreach (var s in HWi.Hardware[0].Sensors) {
				if (s.SensorType == SensorType.Load && s.Name == SensorNames["TotalLoad"]) {

					// Avoid crazy values, as with Ryzen 2400G Vega 11 graphics.
					if (Math.Round((float)s.Value) >= 0 && Math.Round((float)s.Value) <= 100) {

						// Ta utgangspunkt i at GPU Core skal brukes (spill etc).
						Load = Math.Round((decimal)s.Value).ToString() + "%";
						
						// Sjekk om GPU Video Engine har mest aktivitet (transkoding etc).
						foreach (var s2 in HWi.Hardware[0].Sensors) {
							if (s2.SensorType == SensorType.Load && s2.Name == "GPU Video Engine") {
								if (s2.Value > s.Value)
									Load = Math.Round((decimal)s2.Value).ToString() + "%";
								break;
							}
						}
					}

					break;
				}
			}} catch (Exception) {
				Load = "N/A";
			}

			return Load;
		}

		public string getTemp()
		{
			string Temp = "0 °C";
			
			try {
			HWi.Hardware[0].Update();

			foreach (var s in HWi.Hardware[0].Sensors) {
				
				// Generally for Intel.
				if (s.SensorType == SensorType.Temperature && s.Name == SensorNames["TotalTemp"]) {
					Temp = Math.Round((decimal)s.Value).ToString() + "°C";
					break;
				
				// Maybe it's a Ryzen 2400G or other Tdie based CPU?
				} else if (s.SensorType == SensorType.Temperature && s.Name == "Core (Tdie)") {
					Temp = Math.Round((decimal)s.Value).ToString() + "°C";
					break;
				}
			
			}} catch (Exception) {
				Temp = "0 °C";
			}

			return Temp;
		}
	}
}
