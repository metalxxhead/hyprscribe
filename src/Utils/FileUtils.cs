// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 github.com/metalxxhead
using System;
using System.IO;
using System.Reflection.Emit;
using Gtk;

namespace HyprScribe.Utils {
	public static class FileUtils {

		public static bool FileExists(string path) {
			return File.Exists(path);
		}

		public static string ReadFile(string path) {
			return File.ReadAllText(path);
		}

		public static void WriteFile(string path, string content) {
			File.WriteAllText(path, content);
		}

	}
}

