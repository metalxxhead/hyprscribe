using System;
using System.IO;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using System.Data;
using HyprScribe.Logic;

namespace HyprScribe.Models
{
	public class AppConfig 
	{
		public string AppName { get; set; } = "HyprScribe";
		public int WindowWidth { get; set; } = 600;
		public int WindowHeight { get; set; } = 400;

		public AppConfig() {}
	}


	public class TabManager
    {
        private List<TabInfo> _tabs;

        public TabManager()
        {
            _tabs = new List<TabInfo>();
        }

        // Method to add a new tab
        public void AddTab(string tabLabel, int tabIndex, string filePath)
        {
            var tabInfo = new TabInfo(tabLabel, tabIndex, filePath);
            _tabs.Add(tabInfo);

            Console.WriteLine("Added Tab Label: " + tabLabel + " Index: " + tabIndex + " Path: " + filePath);
        }


        public void RemoveTabFromList(string tabLabel)
        {
            for (int x = 0; x < _tabs.Count; x++)
            {
                if (_tabs[x].TabLabel == tabLabel)
                {
                    _tabs.RemoveAt(x);
                    break;
                }
            }
        }



        // Method to get all tabs
        public List<TabInfo> GetAllTabs()
        {
            return _tabs;
        }



        public void SaveTabsToDb()
        {
            string filePath = Path.Combine(CoreLogic.getUserDirectory(), "tabs.db");
            string connectionString = "URI=file:" + filePath;

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                // Create the tabs table if it doesn't exist
                string createTableQuery =
                    "CREATE TABLE IF NOT EXISTS tabs (" +
                    "id INTEGER PRIMARY KEY AUTOINCREMENT," +
                    "tab_label TEXT NOT NULL," +
                    "file_path TEXT NOT NULL UNIQUE," +
                    "tab_index INTEGER NOT NULL)";

                using (var createTableCommand = new SqliteCommand(createTableQuery, connection))
                {
                    createTableCommand.ExecuteNonQuery();
                }

                // Insert or update the tabs into the database
                foreach (var tab in _tabs)
                {
                    string insertQuery =
                        "REPLACE INTO tabs (tab_label, file_path, tab_index) " +
                        "VALUES (@tabLabel, @filePath, @tabIndex)";

                    using (var insertCommand = new SqliteCommand(insertQuery, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@tabLabel", tab.TabLabel);
                        insertCommand.Parameters.AddWithValue("@filePath", tab.FilePath);
                        insertCommand.Parameters.AddWithValue("@tabIndex", tab.TabIndex);
                        insertCommand.ExecuteNonQuery();
                    }
                }
            }
        }



        public void LoadTabsFromDb()
        {
            string filePath = Path.Combine(CoreLogic.getUserDirectory(), "tabs.db");
            string connectionString = "URI=file:" + filePath;

            if (!File.Exists(filePath))
            {
                Console.WriteLine("Database file not found: " + filePath);
                return;
            }

            try
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    // Clear the existing tabs list
                    _tabs.Clear();

                    // Retrieve the tabs from the database
                    string selectQuery = "SELECT * FROM tabs";
                    using (var selectCommand = new SqliteCommand(selectQuery, connection))
                    {
                        using (var reader = selectCommand.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                try
                                {
                                    string tabLabel = reader.GetString(1);
                                    string tabFilePath = reader.GetString(2);
                                    int tabIndex = reader.GetInt32(3);

                                    var tab = new TabInfo(tabLabel, tabIndex, tabFilePath);
                                    _tabs.Add(tab);
                                }
                                catch (Exception ex)
                                {
                                    // Handle any exceptions that occur while reading a row
                                    Console.WriteLine("Error reading a row from the database: " + ex.Message);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur while connecting to or interacting with the database
                Console.WriteLine("Error accessing the database: " + ex.Message);
            }
        }

    



        public void PurgeUnnecessaryEntries()
        {
            string filePath = Path.Combine(CoreLogic.getUserDirectory(), "tabs.db");
            string connectionString = "URI=file:" + filePath;

            if (!File.Exists(filePath))
            {
                Console.WriteLine("Database file not found: " + filePath);
                return;
            }

            try
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    // Retrieve the tabs from the database
                    string selectQuery = "SELECT * FROM tabs";
                    using (var selectCommand = new SqliteCommand(selectQuery, connection))
                    {
                        using (var reader = selectCommand.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                try
                                {
                                    string tabFilePath = reader.GetString(2);

                                    if (!File.Exists(tabFilePath))
                                    {
                                        // File path does not exist, so it's an unnecessary entry
                                        string deleteQuery = "DELETE FROM tabs WHERE file_path = @filePath";
                                        using (var deleteCommand = new SqliteCommand(deleteQuery, connection))
                                        {
                                            deleteCommand.Parameters.AddWithValue("@filePath", tabFilePath);
                                            deleteCommand.ExecuteNonQuery();
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // Handle any exceptions that occur while processing a row
                                    Console.WriteLine("Error processing a row: " + ex.Message);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur while connecting to or interacting with the database
                Console.WriteLine("Error accessing the database: " + ex.Message);
            }
        }
    }

	public class TabInfo
	{
		// Property to hold the tab label
		public string TabLabel { get; set; }

		// Property to hold the file path
		public string FilePath { get; set; }

        public int TabIndex { get; set; }

		// Constructor to initialize the TabInfo object
		public TabInfo(string tabLabel, int tabIndex, string filePath)
		{
			TabLabel = tabLabel;
            TabIndex = tabIndex;
			FilePath = filePath;
		}
	}
}

