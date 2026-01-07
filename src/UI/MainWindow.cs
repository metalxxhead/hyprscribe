using Gtk;
using System;
using System.Linq;
using System.Collections.Generic;
using HyprScribe.Models;
using System.Security.Cryptography.X509Certificates;
using HyprScribe.Logic;
using System.Security.Principal;

namespace HyprScribe.UI
{
    public class MainWindow : Window
    {
        public Notebook notebook;
        public TabManager tabManager = new TabManager();
        public Button plusButton; // headerbar button to add new notebook page


        public MainWindow(AppConfig cfg) : base(cfg.AppName)
        {
            SetDefaultSize(cfg.WindowWidth, cfg.WindowHeight);
            DeleteEvent += (o, args) => Application.Quit();

            var headerBar = new HeaderBar
            {
                Title = cfg.AppName,
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


    }

}