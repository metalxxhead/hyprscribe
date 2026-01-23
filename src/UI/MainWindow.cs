// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 github.com/metalxxhead

using Gtk;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using HyprScribe.Models;
using HyprScribe.Logic;
using HyprScribe.Utils;
using LightweightJson;
using System.Diagnostics;
//using System.ComponentModel.DataAnnotations.Schema;

namespace HyprScribe.UI
{
    public class MainWindow : Window
    {
        public Notebook notebook;
        public TabManager tabManager => WindowManager.SharedTabManager;
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

        private bool _isFirstWindow = false;

        public MainWindow() : base("HyprScribe") 
        {
            // Register this window
            WindowManager.RegisterWindow(this);
            _isFirstWindow = (WindowManager.GetWindowCount() == 1);

            string foo = Logic.CoreLogic.GetConfigDirectory();
            string configFileName = "/config.json";
            string configPath = foo + configFileName;

            Console.WriteLine(configPath);

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
          
            DeleteEvent += OnWindowDelete;

            var headerBar = new HeaderBar
            {
                Title = "HyprScribe",
                ShowCloseButton = true
            };

            Titlebar = headerBar;

            notebook = new Notebook
            {
                Scrollable = true,
                ShowBorder = false,
                GroupName = "hyprscribe-tabs"  // Enable DND between notebooks
            };

            notebook.EnablePopup = true;

            // Enable tab drag and drop
            notebook.PageRemoved += OnPageRemoved;
            notebook.PageAdded += OnPageAdded;

            var root_vbox = new Box(Orientation.Vertical, 0);
            Add(root_vbox);

            emptyLabel.Opacity = 0.6;

            var overlay = new Overlay();
            overlay.Add(notebook);
            overlay.AddOverlay(emptyLabel);

            emptyLabel.NoShowAll = true;
            emptyLabel.Hide();

            root_vbox.PackStart(overlay, true, true, 0);

            statusBar = new Statusbar();
            statusContext = statusBar.GetContextId("main");
            root_vbox.PackEnd(statusBar, false, false, 0);
            statusBar.Push(statusContext, "Ready");

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
            
            menu.Append(new SeparatorMenuItem());
            
            var aboutItem = new MenuItem("About");
		aboutItem.Activated += (s, e) => ShowAboutDialog(this);
		menu.Append(aboutItem);


            menu.ShowAll();

            var menuButton = new MenuButton
            {
                Popup = menu
            };

            var image = new Image(Stock.Preferences, IconSize.Button);
            menuButton.Add(image);

            headerBar.PackStart(menuButton);

            ShowAll();

            // Only load tabs on the first window
            if (_isFirstWindow)
            {
                LoadInitialTabs();
            }

            UpdateEmptyState();

            // Handle create-window signal for drag-out
            notebook.CreateWindow += OnCreateWindow;
        }
        
        
        
        private static void ShowAboutDialog(Window parent)
	{
	    var about = new AboutDialog
	    {
		ProgramName = AppInfo.Name,
		Version = AppInfo.Version,
		Comments = "A multi-tabbed auto-saving writing tool.\n\n" +
			   "This is a development build and may be unstable.",
		Website = "https://internalstaticvoid.dev/projects/software/hyprscribe/",
		Copyright = AppInfo.Copyright,
		TransientFor = parent,
		Modal = true
	    };

	    about.Run();
	    about.Destroy();
	}


        // private void LoadInitialTabs()
        // {
        //     tabManager.PurgeUnnecessaryEntries();
        //     tabManager.LoadTabsFromDb();
        //     tabManager.SyncTabsFromDirectory(Logic.CoreLogic.getCurrentTabsDirectory());
        //     tabManager.LoadTabsFromDb();

        //     List<TabInfo> knownTabs = tabManager.GetAllTabs();
        //     var orderedTabs = knownTabs.OrderBy(t => t.TabIndex).ToList();

        //     foreach (var tab in orderedTabs)
        //     {
        //         if (!File.Exists(tab.FilePath))
        //             continue;

        //         Handlers.MainHandlers.AddKnownTabFromDB(notebook, this, tab);
        //     }

        //     if (orderedTabs.Count < 1)
        //     {
        //         Handlers.MainHandlers.AddEditorTab(notebook, this);
        //     }
        // }


        private void LoadInitialTabs()
        {
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

            // Only create a default tab if NO tabs were loaded
            if (notebook.NPages == 0)
            {
                Console.WriteLine("No tabs loaded from DB - creating default tab");
                Handlers.MainHandlers.AddEditorTab(notebook, this);
            }
            else
            {
                Console.WriteLine($"Loaded {notebook.NPages} tabs from database");
            }
        }

        private void OnCreateWindow(object sender, CreateWindowArgs args)
        {
            Console.WriteLine("CreateWindow signal fired - creating new window for detached tab");
            
            // Create a new window
            var newWindow = WindowManager.CreateNewWindow();
            
            // Return the new window's notebook so GTK can move the tab there
            args.RetVal = newWindow.notebook;
        }

        private void OnPageAdded(object sender, PageAddedArgs args)
        {
            UpdateEmptyState();
            
            // Update title to show tab count
            var headerBar = (HeaderBar)Titlebar;
            int tabCount = notebook.NPages;
            //headerBar.Title = tabCount > 0 ? $"HyprScribe ({tabCount})" : "HyprScribe";
            //headerBar.Title = "HyprScribe";
            headerBar.Title = $"{AppInfo.Name} ({AppInfo.Version})";

        }

        private void OnPageRemoved(object sender, PageRemovedArgs args)
        {
            UpdateEmptyState();
            
            // Update title
            var headerBar = (HeaderBar)Titlebar;
            int tabCount = notebook.NPages;
            //headerBar.Title = tabCount > 0 ? $"HyprScribe ({tabCount})" : "HyprScribe";
            //headerBar.Title = "HyprScribe";
            headerBar.Title = $"{AppInfo.Name} ({AppInfo.Version})";
            
            // If this window has no tabs and it's not the only window, close it
            if (tabCount == 0 && WindowManager.GetWindowCount() > 1)
            {
                Console.WriteLine("Window has no tabs and other windows exist - closing");
                this.Destroy();
            }
        }

/*         private void OnWindowDelete(object o, DeleteEventArgs args)
        {
            Console.WriteLine($"Window closing. Current tab count: {notebook.NPages}");
            
            // If there are tabs in this window and other windows exist, transfer them
            if (notebook.NPages > 0 && WindowManager.GetWindowCount() > 1)
            {
                var targetWindow = WindowManager.GetAnyOtherWindow(this);
                if (targetWindow != null)
                {
                    TransferAllTabsToWindow(targetWindow);
                }
            }
            
            WindowManager.UnregisterWindow(this);
            
            // If this is the last window, quit the application
            if (WindowManager.GetWindowCount() == 0)
            {
                Application.Quit();
            }
        } */


        private void OnWindowDelete(object o, DeleteEventArgs args)
        {
            Console.WriteLine($"Window closing. Current tab count: {notebook.NPages}");
            
            // If there are tabs in this window and other windows exist, transfer them
            if (notebook.NPages > 0 && WindowManager.GetWindowCount() > 1)
            {
                var targetWindow = WindowManager.GetAnyOtherWindow(this);
                if (targetWindow != null)
                {
                    TransferAllTabsToWindow(targetWindow);
                }
            }
            
            // Unregister BEFORE checking window count
            WindowManager.UnregisterWindow(this);
            
            // If this is the last window, quit the application
            if (WindowManager.GetWindowCount() == 0)
            {
                Application.Quit();
            }
            
            // Don't let the event propagate further - we're handling cleanup
            args.RetVal = false;
        }



        private void TransferAllTabsToWindow(MainWindow targetWindow)
        {
            Console.WriteLine($"Transferring {notebook.NPages} tabs to another window");

            while (notebook.NPages > 0)
            {
                var page = notebook.GetNthPage(0); // this is your scroller
                if (page == null) break;

                //var tabLabelText = page.Data["tabLabel"] as string ?? "Tab";
                //var filePath     = page.Data["filePath"] as string ?? "";
                //var tabLabelText = page.Data["tabLabel"] as string;
                //var filePath     = page.Data["filePath"] as string;

                if (!page.Data.ContainsKey("tabLabel") || !(page.Data["tabLabel"] is string tabLabelText))
                {
                    throw new InvalidOperationException(
                        "Tab transfer failed: page is missing required 'tabLabel' metadata."
                    );
                }

                if (!page.Data.ContainsKey("filePath") || !(page.Data["filePath"] is string filePath))
                {
                    throw new InvalidOperationException(
                        "Tab transfer failed: page is missing required 'filePath' metadata."
                    );
                }


                // Remove from this notebook FIRST
                notebook.RemovePage(0);

                // Build a NEW tab label that captures the TARGET notebook/window
                var newTabLabel = Handlers.MainHandlers.CreateTabLabel(
                    targetWindow.notebook,
                    page,
                    tabLabelText,
                    targetWindow,
                    filePath
                );

                // Append the SAME page widget to target notebook
                int newIndex = targetWindow.notebook.AppendPage(page, newTabLabel);

                targetWindow.notebook.SetTabDetachable(page, true);
                targetWindow.notebook.ShowAll();
            }

            targetWindow.Present();
        }



        // last used but buggy
        // private void TransferAllTabsToWindow(MainWindow targetWindow)
        // {
        //     Console.WriteLine($"Transferring {notebook.NPages} tabs to another window");
            
        //     // We need to transfer in reverse order to maintain indices
        //     while (notebook.NPages > 0)
        //     {
        //         var page = notebook.GetNthPage(0);
        //         var tabLabel = notebook.GetTabLabel(page);
                
        //         // Reparent the page to the target notebook
        //         page.Reparent(targetWindow.notebook);
        //         notebook.Remove(page);
        //         targetWindow.notebook.AppendPage(page, tabLabel);
        //         Console.WriteLine("Page: " + page + " Tab Label: " + tabLabel);
        //         targetWindow.notebook.SetTabReorderable(page, true);
        //     }
            
        //     targetWindow.ShowAll();
        // }


        // private void TransferAllTabsToWindow(MainWindow targetWindow)
        // {
        //     Console.WriteLine($"Transferring {notebook.NPages} tabs to another window");
            
        //     // Create a list of pages to transfer (to avoid modification during iteration)
        //     var pagesToTransfer = new List<(Widget page, Widget label)>();
            
        //     for (int i = 0; i < notebook.NPages; i++)
        //     {
        //         var page = notebook.GetNthPage(i);
        //         var label = notebook.GetTabLabel(page);
        //         pagesToTransfer.Add((page, label));
        //     }
            
        //     // Now transfer them
        //     foreach (var (page, label) in pagesToTransfer)
        //     {
        //         // Remove from this notebook first
        //         notebook.Remove(page);
                
        //         // Add to target notebook
        //         int newPage = targetWindow.notebook.AppendPage(page, label);
        //         targetWindow.notebook.SetTabReorderable(page, true);
                
        //         // Re-enable detachable for the transferred tab
        //         GtkNative.gtk_notebook_set_tab_detachable(
        //             targetWindow.notebook.Handle,
        //             page.Handle,
        //             true
        //         );
        //     }
            
        //     targetWindow.ShowAll();
        //     targetWindow.Present(); // Bring the target window to front
        // }
        // src/UI/MainWindow.cs(317,13): error CS0230: Type and identifier are both required in a foreach statement
        // src/UI/MainWindow.cs(317,39): error CS1525: Unexpected symbol `in'
        // src/UI/MainWindow.cs(317,57): error CS1525: Unexpected symbol `)'



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
