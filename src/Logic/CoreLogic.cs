using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Numerics;
using System;
using System.Collections.Generic;
using System.IO;
using Internal;
using System.Reflection;
using HyprScribe.UI;
using HyprScribe.Models;

namespace HyprScribe.Logic 
{
	public class CoreLogic 
	{

		public static string GenerateRandomString()
		{
			// Define the character pool
			string characterPool = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

			// Create a random number generator
			Random random = new Random();

   			// Use LINQ to select 5 random characters from the pool
    		string randomString = new string(characterPool.Select(c => characterPool[random.Next(characterPool.Length)]).Take(5).ToArray());


			return randomString;
		}


		public static string GenerateUniqueRandomString(List<string> existingStrings)
		{
			while (true)
			{
				// Generate a random string
				string randomString = GenerateRandomString();

				// Check if the generated string is unique
				if (!existingStrings.Contains(randomString))
				{
					// If unique, return the string
					return randomString;
				}
			}
		}


		public static List<string> GetFilePaths(string directoryPath)
		{
			List<string> filePaths = new List<string>(); 

			if (Directory.Exists(directoryPath))  
			{  
				string[] files = Directory.GetFiles(directoryPath);

				foreach (string file in files)  
				{  
					filePaths.Add(file);  
				}  
			}  
			else  
			{  
				Console.WriteLine("Directory does not exist.");  
			}

			return filePaths;  
		}


		public static List<string> GetFilenamesFromDirectories(string[] directories)
		{
			List<string> filenames = new List<string>();

			foreach (var directoryPath in directories)
			{
				if (Directory.Exists(directoryPath))
				{
					foreach (var fileName in Directory.GetFiles(directoryPath))
					{
						filenames.Add(fileName);
					}
				}
			}

			return filenames;
		}


		public static string GetConfigDirectory()
		{
			string exePath = Assembly.GetEntryAssembly().Location;
			string buildDir = Path.GetDirectoryName(exePath);
			string parentDir = Directory.GetParent(buildDir).FullName;
			string userDir = Path.Combine(parentDir, "user_data");
			string configDir = Path.Combine(parentDir, "config");

			Directory.CreateDirectory(userDir);

			string current_tabs_path = Path.Combine(userDir, "current_tabs");

			Directory.CreateDirectory(current_tabs_path);

			string archived_tabs_path = Path.Combine(userDir, "archived_tabs");

			Directory.CreateDirectory(archived_tabs_path);

			return configDir;
		}



		public static string getUserDirectory()
		{
			string exePath = Assembly.GetEntryAssembly().Location;
			string buildDir = Path.GetDirectoryName(exePath);
			string parentDir = Directory.GetParent(buildDir).FullName;
			string userDir = Path.Combine(parentDir, "user_data");

			Directory.CreateDirectory(userDir);

			string current_tabs_path = Path.Combine(userDir, "current_tabs");

			Directory.CreateDirectory(current_tabs_path);

			string archived_tabs_path = Path.Combine(userDir, "archived_tabs");

			Directory.CreateDirectory(archived_tabs_path);

			return userDir;
		}


		public static string getCurrentTabsDirectory()
		{
			string exePath = Assembly.GetEntryAssembly().Location;
			string buildDir = Path.GetDirectoryName(exePath);
			string parentDir = Directory.GetParent(buildDir).FullName;
			string userDir = Path.Combine(parentDir, "user_data");

			Directory.CreateDirectory(userDir);

			string current_tabs_path = Path.Combine(userDir, "current_tabs");

			Directory.CreateDirectory(current_tabs_path);

			string archived_tabs_path = Path.Combine(userDir, "archived_tabs");

			Directory.CreateDirectory(archived_tabs_path);

			return current_tabs_path;
		}


		public static string GenerateUniqueFileName()
		{
			string exePath = Assembly.GetEntryAssembly().Location;
			string buildDir = Path.GetDirectoryName(exePath);
			string parentDir = Directory.GetParent(buildDir).FullName;
			string userDir = Path.Combine(parentDir, "user_data");

			Directory.CreateDirectory(userDir);

			string current_tabs_path = Path.Combine(userDir, "current_tabs");

			Directory.CreateDirectory(current_tabs_path);

			string archived_tabs_path = Path.Combine(userDir, "archived_tabs");

			Directory.CreateDirectory(archived_tabs_path);

           
			string[] directories = {
                current_tabs_path,
                archived_tabs_path
            };

			return Path.Combine(current_tabs_path, GenerateUniqueRandomString(GetFilenamesFromDirectories(directories))) + ".txt";
		}


		internal static void writeTabInfoFile(MainWindow window)
		{
			string filePath = Path.Combine(getUserDirectory(), "tabinfo.txt");

			List<TabInfo> tabFilePairs = window.tabManager.GetAllTabs();

			using (StreamWriter writer = new StreamWriter(filePath))
			{
				foreach (var item in tabFilePairs)
				{
					writer.WriteLine($"{item.TabLabel},{item.FilePath}");
				}
			}
		}


		public static void CreateBlankFileIfNotExists(string filePath)
		{
			if (!File.Exists(filePath))
			{
				File.Create(filePath).Close();
			}
		}



		public static void ArchiveTab(string targetTabText, string filePath, MainWindow window)
		{
			string userDirectory = getUserDirectory();
			string currentTabsPath = Path.Combine(userDirectory, "current_tabs");
			string archivedTabsPath = Path.Combine(userDirectory, "archived_tabs");

			// Check if the file exists and is located in the current_tabs folder
			if (File.Exists(filePath) && Path.GetDirectoryName(filePath).Equals(currentTabsPath))
			{
				string relativePath = Path.GetRelativePath(currentTabsPath, filePath);
				string destFilePath = Path.Combine(archivedTabsPath, relativePath);

				// Create the destination directory if it doesn't exist
				string destDir = Path.GetDirectoryName(destFilePath);
				if (!Directory.Exists(destDir))
				{
					Directory.CreateDirectory(destDir);
				}

				// Move the file to the archived_tabs folder
				File.Move(filePath, destFilePath);

				Console.WriteLine("File moved from current_tabs to archived_tabs: " + filePath);

				Handlers.MainHandlers.SetTimedStatus(window.statusContext, "Archived " + targetTabText + " to " + filePath, window);
			}
			else
			{
				Console.WriteLine("File not found or not in the current_tabs folder: " + filePath);
			}
		}



	}
}




