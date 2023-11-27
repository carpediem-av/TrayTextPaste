/**********************************************************************/
/* Copyright (c) 2023 Carpe Diem Software Developing by Alex Versetty */
/* http://carpediem.0fees.us                                          */
/**********************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CDSD.Data;

namespace TrayTextPaste
{
	public class Config
	{
		public string CopySwitchPasteHotkey { get; set; } = "Отключить";
		public string Strings { get; set; } = "";

        public Config clone()
		{
			return (Config) MemberwiseClone();
		}
	}

	class ConfigManager
	{
		static string configFilename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.xml");
		public static Config Current { get; set; }

		public static void load()
		{
			try {
				var xml = File.ReadAllText(configFilename);
				Current = Serialization.XmlDeserialize<Config>(xml);
			}
			catch (Exception ex) {
				Current = new Config();
                if (isConfigFileExists()) throw ex;
			}
		}

		public static void save()
		{
			if (Current == null) throw new InvalidOperationException("Load config first");
			var clone = Current.clone();

			try {
				var xml = Serialization.XmlSerialize(clone);
				File.WriteAllText(configFilename, xml);
			}
			catch (Exception ex) {
				throw ex;
			}
		}

		public static bool isConfigFileExists()
		{
			return File.Exists(configFilename);
		}
	}
}
