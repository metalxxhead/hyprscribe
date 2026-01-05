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
        public void AddTab(int tabIndex, string tabLabel, string filePath)
        {
            var tabInfo = new TabInfo(tabIndex, tabLabel, filePath);
            _tabs.Add(tabInfo);
        }

        // Method to remove a tab by its index
        public void RemoveTab(int tabIndex)
        {
            var tabToRemove = _tabs.Find(t => t.TabIndex == tabIndex);
            if (tabToRemove != null)
            {
                _tabs.Remove(tabToRemove);
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
		// Property to hold the tab index
		public int TabIndex { get; set; }

		// Property to hold the tab label
		public string TabLabel { get; set; }

		// Property to hold the file path
		public string FilePath { get; set; }

		// Constructor to initialize the TabInfo object
		public TabInfo(int tabIndex, string tabLabel, string filePath)
		{
			TabIndex = tabIndex;
			TabLabel = tabLabel;
			FilePath = filePath;
		}
	}
}

