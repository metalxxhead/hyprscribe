using Gtk;
using System;
using System.Linq;
using System.Collections.Generic;
using HyprScribe.Models;
using System.Security.Cryptography.X509Certificates;
using HyprScribe.Logic;
using System.Security.Principal;
using LightweightJson;
using HyprScribe.Utils;
using System.ComponentModel.Design;
using System.Diagnostics;

namespace HyprScribe.UI
{
    public class MainWindow : Window
    {
        public Notebook notebook;
        public TabManager tabManager = new TabManager();
        public Button plusButton; 
        

        public MainWindow() : base("HyprScribe") 
        {

            string foo = Logic.CoreLogic.GetConfigDirectory();
            string configFileName = "/config.json";
            string configPath = foo + configFileName;

            Console.WriteLine(configPath);

            //string configPath = Path.Combine(foo, "config.json");
            string configData = FileUtils.ReadFile(configPath);

            JsonValue root = Json.Parse(configData);

            var obj = root.AsObject();

            int width = (int)obj.GetDouble("WindowWidth");
            int height = (int)obj.GetDouble("WindowHeight");

            if (width < 100 || height < 100)
            {
                SetDefaultSize(640, 480);
            }
            else
            {
                SetDefaultSize(width, height);
            }
          
            DeleteEvent += (o, args) => Application.Quit();

            var headerBar = new HeaderBar
            {
                Title = "HyprScribe",
                ShowCloseButton = true
            };

            Titlebar = headerBar;

            
            notebook = new Notebook
            {
                Scrollable = true,
                ShowBorder = false
            };


            Add(notebook);

            plusButton = new Button("+")
            {
                Relief = ReliefStyle.None,
                FocusOnClick = false
            };

            plusButton.Clicked += (sender, e) =>
            {
                Handlers.MainHandlers.AddEditorTab(notebook, this);
            };

            headerBar.PackEnd(plusButton);





            // --- Menu ---
            var menu = new Menu();

            var newTabItem = new MenuItem("Write Window Size to Config");
            newTabItem.Activated += (s, e) => SaveWindowSize(this);
            menu.Append(newTabItem);

            menu.Append(new SeparatorMenuItem());

            var saveCurrentTabItem = new MenuItem("Save As (Current Tab Only)...");
            saveCurrentTabItem.Activated += (s, e) => Handlers.MainHandlers.saveTabBufferToFile(notebook, this);
            menu.Append(saveCurrentTabItem);


            var quitItem = new MenuItem("Quit");
            quitItem.Activated += (s, e) => Application.Quit();
            menu.Append(quitItem);

            menu.ShowAll();

            // --- Menu Button ---
            var menuButton = new MenuButton
            {
                Popup = menu
            };

            // Icon (hamburger)
            var image = new Image(Stock.Preferences, IconSize.Button);
            menuButton.Add(image);

            headerBar.PackStart(menuButton);




            ShowAll();

            tabManager.PurgeUnnecessaryEntries();

            tabManager.LoadTabsFromDb();


            List<TabInfo> knownTabs = tabManager.GetAllTabs();

            var orderedTabs = knownTabs.OrderBy(t => t.TabIndex).ToList();

            foreach (var tab in orderedTabs)
            {
                Handlers.MainHandlers.AddKnownTabFromDB(notebook, this, tab);
            }

        }


        private void SaveWindowSize(MainWindow window)
        {
            string foo = Logic.CoreLogic.GetConfigDirectory();
            string configFileName = "/config.json";
            string configPath = foo + configFileName;

            string configData = FileUtils.ReadFile(configPath);

            JsonValue root = Json.Parse(configData);

            var obj = root.AsObject();

            int width = -1;
            int height = -1;

            GetSize(out width, out height);


            obj["WindowWidth"]  = new JsonNumber(width, width.ToString());
            obj["WindowHeight"] = new JsonNumber(height, height.ToString());

            string updatedJson = Json.Stringify(root, pretty: true);

            FileUtils.WriteFile(configPath, updatedJson);
            Console.WriteLine("Wrote Window Size to Config");
        }

    }

}
