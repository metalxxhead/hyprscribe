using System;
using System.IO;
using Newtonsoft.Json;
using HyprScribe.Models;

namespace HyprScribe.Config {
	public static class JsonConfig {

		public static AppConfig Load(string path) {
			var text = File.ReadAllText(path);
			return JsonConvert.DeserializeObject<AppConfig>(text);
		}

		public static void Save(string path, AppConfig config) {
			var output = JsonConvert.SerializeObject(config, Formatting.Indented);
			File.WriteAllText(path, output);
		}
	}
}

