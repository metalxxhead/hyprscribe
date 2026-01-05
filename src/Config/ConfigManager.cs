using System;
using System.IO;
using HyprScribe.Models;

namespace HyprScribe.Config {
	public static class ConfigManager {

		private static readonly string ConfigPathJson = "config/config.json";

		public static AppConfig LoadConfig() {

			if (File.Exists(ConfigPathJson)) {
				try {
					return JsonConfig.Load(ConfigPathJson);
				} catch (Exception ex) {
					Console.WriteLine("JSON config load failed: " + ex.Message);
				}
			}

			// Fallback to defaults
			Console.WriteLine("No config found.");
			return new AppConfig();
		}

		public static void SaveConfig(AppConfig config) {
				JsonConfig.Save(ConfigPathJson, config);
		}
	}
}

