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



	}
}




