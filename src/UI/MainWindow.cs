// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 github.com/metalxxhead

using Gtk;
using System;
using System.IO;
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


        // Status bar related
        public Statusbar statusBar;
        public uint statusContext;
        public uint? statusTimeoutId;

        public Gtk.Label emptyLabel = new Label
        { 
            LabelProp = "No tabs open\nClick + to create one",
            Justify = Justification.Center,
            Xalign = 0.5f,
            Yalign = 0.5f
        };


        

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


            notebook.PageRemoved += (s, e) => UpdateEmptyState();
            notebook.PageAdded += (s, e) => UpdateEmptyState();

            var root_vbox = new Box(Orientation.Vertical, 0);
            Add(root_vbox);


            emptyLabel.Opacity = 0.6;

            var overlay = new Overlay();
            overlay.Add(notebook);
            overlay.AddOverlay(emptyLabel);

            //overlay.SetOverlayPassThrough(emptyLabel, true);

            emptyLabel.NoShowAll = true;
            emptyLabel.Hide();


            //Add(notebook);
            //root_vbox.PackStart(notebook, true, true, 0);
            root_vbox.PackStart(overlay, true, true, 0);




            statusBar = new Statusbar();

            // Create a context ID (used for updates)
            statusContext = statusBar.GetContextId("main");

            root_vbox.PackEnd(statusBar, false, false, 0);

            // Initial message
            statusBar.Push(statusContext, "Ready");
            //Handlers.MainHandlers.SetTimedStatus(statusContext, "Test Message!", this, 5000);






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

            var openDirItem = new MenuItem("Open User Directory");
            openDirItem.Activated += (s, e) => OpenUserDirectory();
            menu.Append(openDirItem);


            var newTabItem = new MenuItem("Write Window Size to Config");
            newTabItem.Activated += (s, e) => SaveWindowSize(this);
            menu.Append(newTabItem);

            menu.Append(new SeparatorMenuItem());
            
            var saveCurrentTabItem = new MenuItem("Export Current Tab...");
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
            tabManager.SyncTabsFromDirectory(Logic.CoreLogic.getCurrentTabsDirectory());
            tabManager.LoadTabsFromDb();


            List<TabInfo> knownTabs = tabManager.GetAllTabs();

            var orderedTabs = knownTabs.OrderBy(t => t.TabIndex).ToList();

            foreach (var tab in orderedTabs)
            {
                if (!File.Exists(tab.FilePath))
                continue;

                Handlers.MainHandlers.AddKnownTabFromDB(notebook, this, tab);
            }

            if (orderedTabs.Count < 1)
            {
                Handlers.MainHandlers.AddEditorTab(notebook, this);
            }

            //UpdateEmptyState();
            emptyLabel.Visible = false;

        }





        public static void OpenUserDirectory()
        {
            string path = Logic.CoreLogic.getUserDirectory();

            if (!Directory.Exists(path))
                return;

            Process.Start(new ProcessStartInfo
            {
                FileName = "xdg-open",
                Arguments = $"\"{path}\"",
                UseShellExecute = false
            });
        }



        void UpdateEmptyState()
        {
            emptyLabel.Visible = notebook.NPages == 0;
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
