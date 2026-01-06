using System.Collections.Generic;

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
        public void AddTab(string tabLabel, string filePath)
        {
            var tabInfo = new TabInfo(tabLabel, filePath);
            _tabs.Add(tabInfo);
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
    }


	public class TabInfo
	{
		// Property to hold the tab label
		public string TabLabel { get; set; }

		// Property to hold the file path
		public string FilePath { get; set; }

		// Constructor to initialize the TabInfo object
		public TabInfo(string tabLabel, string filePath)
		{
			TabLabel = tabLabel;
			FilePath = filePath;
		}
	}
}

