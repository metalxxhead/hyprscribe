using Gtk;
using System;
using System.Collections.Generic;
using HyprScribe.Models;

namespace HyprScribe.UI
{
    public class MainWindow : Window
    {
        public Notebook notebook;
        public int tabCounter = 1;
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

        }


    }

}