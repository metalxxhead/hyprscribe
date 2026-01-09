// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 github.com/metalxxhead
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Principal;
using Gtk;
using HyprScribe.Logic;
using HyprScribe.Models;
using HyprScribe.UI;
using HyprScribe.Utils;
using Internal;


namespace HyprScribe.Handlers 
{
    public static class MainHandlers 
    {

        public static Widget CreateTabLabel(Notebook notebook, string title, MainWindow window, string fileSavePath)
        {
            var hbox = new Box(Orientation.Horizontal, 5);

            var label = new Label(title);

            var closeButton = new Button("×")
            {
                Relief = ReliefStyle.None,
                FocusOnClick = false
            };

            closeButton.Clicked += (sender, e) =>
            {
                RemoveTab(notebook, title, window, fileSavePath);
            };

            hbox.PackStart(label, true, true, 0);
            hbox.PackStart(closeButton, false, false, 0);

            hbox.ShowAll();
            return hbox;
        }


        internal static void RemoveTab(Notebook notebook, string title, MainWindow window, string fileSavePath)
        {
            Console.WriteLine("Removing tab with file path: " + fileSavePath);

            List<TabInfo> allTabs = window.tabManager.GetAllTabs();
            string targetTab = "";
            int targetIndex = -1;
            string targetTabText = "";

            foreach (TabInfo ti in allTabs)
            {
                if (ti.FilePath == fileSavePath)
                {
                    targetTab = ti.FilePath;
                    targetIndex = ti.TabIndex;
                    targetTabText = ti.TabLabel;
                    Console.WriteLine("Target tab found with file path: " + targetTab);
                    break;
                }
            }

 			for (int i = 0; i < notebook.NPages; i++)
			{

				var tabLabelBox = notebook.GetTabLabel(notebook.GetNthPage(i));
				var tabTitleLabel = (Gtk.Label)((Container)tabLabelBox).Children[0];
				var labelText = tabTitleLabel.LabelProp;

				if (labelText == targetTabText)
				{
					notebook.RemovePage(i);
                    break;
				}
			}

            window.tabManager.RemoveTabFromList(targetTab);
            window.tabManager.RemoveTabFromDb(targetTab);
            CoreLogic.ArchiveTab(targetTabText, targetTab, window);
        }


	internal static void AddEditorTab(Notebook notebook, MainWindow window)
	{
	    var undoStack = new Stack<string>();
	    var redoStack = new Stack<string>();
	    bool isUndoing = false;

	    string fileSavePath = Logic.CoreLogic.GenerateUniqueFileName();
	    
	    int index = GenerateNewIndex(notebook, window);
	    var tabLabel = CreateTabLabel(
		notebook,
		"Tab " + index,
		window,
		fileSavePath
	    );

	    var textView = new TextView
	    {
		WrapMode = WrapMode.WordChar
	    };

	    // --- BUFFER CHANGED ---
	    textView.Buffer.Changed += (s, e) =>
	    {
		if (isUndoing) return;

		undoStack.Push(textView.Buffer.Text);
		redoStack.Clear();

		File.WriteAllText(fileSavePath, textView.Buffer.Text);
		SetTimedStatus(window.statusContext, "Saved Tab " + index + " to " + fileSavePath, window);
	    };

	    // --- UNDO / REDO ---
	    textView.KeyPressEvent += (sender, args) =>
	    {
		bool ctrl  = (args.Event.State & Gdk.ModifierType.ControlMask) != 0;
		bool shift = (args.Event.State & Gdk.ModifierType.ShiftMask) != 0;

		// UNDO (Ctrl+Z)
		if (ctrl && !shift && args.Event.Key == Gdk.Key.z && undoStack.Count > 0)
		{
		    isUndoing = true;

		    redoStack.Push(textView.Buffer.Text);
		    textView.Buffer.Text = undoStack.Pop();

		    isUndoing = false;
		    args.RetVal = true;
		}
		// REDO (Ctrl+Shift+Z)
		else if (ctrl && shift &&
			(args.Event.Key == Gdk.Key.z || args.Event.Key == Gdk.Key.Z) &&
			redoStack.Count > 0)
		{
		    isUndoing = true;

		    undoStack.Push(textView.Buffer.Text);
		    textView.Buffer.Text = redoStack.Pop();

		    isUndoing = false;
		    args.RetVal = true;
		}
	    };

	    var scroller = new ScrolledWindow();
	    scroller.Add(textView);

	    int page = notebook.AppendPage(scroller, tabLabel);
	    notebook.SetTabReorderable(scroller, false);


	    CoreLogic.CreateBlankFileIfNotExists(fileSavePath);
	    window.tabManager.AddTab("Tab " + index, page, fileSavePath);
	    window.tabManager.SaveTabsToDb();

		


	    window.ShowAll();

		notebook.CurrentPage = page;
		textView.GrabFocus();
	}




	internal static void AddKnownTabFromDB(Notebook notebook, MainWindow window, TabInfo tabData)
	{
	    var undoStack = new Stack<string>();
	    var redoStack = new Stack<string>();
	    bool isUndoing = false;

	    var textView = new TextView
	    {
		WrapMode = WrapMode.WordChar
	    };

	    // Load file content
	    textView.Buffer.Text = FileUtils.ReadFile(tabData.FilePath);

	    // --- BUFFER CHANGED ---
	    textView.Buffer.Changed += (s, e) =>
	    {
		if (isUndoing) return;

		undoStack.Push(textView.Buffer.Text);
		redoStack.Clear();

		File.WriteAllText(tabData.FilePath, textView.Buffer.Text);	
		SetTimedStatus(window.statusContext, "Saved " + tabData.TabLabel + " to " + tabData.FilePath, window);
	    };

	    // --- UNDO / REDO ---
	    textView.KeyPressEvent += (sender, args) =>
	    {
		bool ctrl  = (args.Event.State & Gdk.ModifierType.ControlMask) != 0;
		bool shift = (args.Event.State & Gdk.ModifierType.ShiftMask) != 0;

		// UNDO (Ctrl+Z)
		if (ctrl && !shift && args.Event.Key == Gdk.Key.z && undoStack.Count > 0)
		{
		    isUndoing = true;

		    redoStack.Push(textView.Buffer.Text);
		    textView.Buffer.Text = undoStack.Pop();

		    isUndoing = false;
		    args.RetVal = true;
		}
		// REDO (Ctrl+Shift+Z)
		else if (ctrl && shift &&
		        (args.Event.Key == Gdk.Key.z || args.Event.Key == Gdk.Key.Z) &&
		        redoStack.Count > 0)
		{
		    isUndoing = true;

		    undoStack.Push(textView.Buffer.Text);
		    textView.Buffer.Text = redoStack.Pop();

		    isUndoing = false;
		    args.RetVal = true;
		}
	    };

	    var scroller = new ScrolledWindow();
	    scroller.Add(textView);

	    var tabLabel = CreateTabLabel(
		notebook,
		tabData.TabLabel,
		window,
		tabData.FilePath
	    );

	    int page = notebook.AppendPage(scroller, tabLabel);
	    notebook.SetTabReorderable(scroller, false);



		textView.GrabFocus();


	    window.ShowAll();

		notebook.CurrentPage = page;
		textView.GrabFocus();
	}




        public static int GenerateNewIndex(Notebook notebook, MainWindow window)
		{
			int index = 0;

            List<string> tabNames = new List<string>();

            foreach (var tab in window.tabManager.GetAllTabs())
            {
                tabNames.Add(tab.TabLabel);
            }

            while(tabNames.Contains("Tab " + index))
            {
                index++;
            }

			return index;
			
		}



        internal static void saveTabBufferToFile(Notebook notebook, MainWindow window)
        {

            int activeIndex = notebook.CurrentPage;
            var scrolledWinTemp = notebook.GetNthPage(activeIndex);

            ScrolledWindow scrolledWindow = scrolledWinTemp as ScrolledWindow;

            if (scrolledWindow != null)
            {

                TextView textView = scrolledWindow.Child as TextView;
                
                if (textView != null)
                {
                    string textContent = textView.Buffer.Text;

                    var dialog = new Gtk.FileChooserDialog(
                        "Save File",
                        window,
                        Gtk.FileChooserAction.Save,
                        "_Cancel", Gtk.ResponseType.Cancel,
                        "_Save", Gtk.ResponseType.Accept
                    );

                    // Optional defaults
                    dialog.DoOverwriteConfirmation = true;
                    dialog.CurrentName = "";

                    if (dialog.Run() == (int)Gtk.ResponseType.Accept)
                    {
                        string filePath = dialog.Filename;
                        File.WriteAllText(filePath, textContent);
                        SetTimedStatus(window.statusContext, "Exported file to " + filePath, window);
                    }
                    else
                    {
                        Console.WriteLine("File save operation cancelled.");
                    }

                    // Destroy the dialog
                    dialog.Destroy();

                }
                else
                {
                    Console.WriteLine("TextView not found inside the ScrolledWindow.");
                }

            }
            
            else
            {
                Console.WriteLine("ScrolledWindow not found on the current page.");
            }

        }


        internal static void SetStatus(uint statusContext, string message, MainWindow window)
        {
            window.statusBar.Pop(window.statusContext);
            window.statusBar.Push(window.statusContext, message);
        }



        internal static void SetTimedStatus(uint statusContext, string message, MainWindow window, uint durationMs = 500)
        {
            // Cancel any existing timeout
            if (window.statusTimeoutId.HasValue)
            {
                GLib.Source.Remove(window.statusTimeoutId.Value);
                window.statusTimeoutId = null;
            }

            // Show the message
            window.statusBar.Pop(window.statusContext);
            window.statusBar.Push(window.statusContext, message);

            // Schedule revert to "Ready"
            window.statusTimeoutId = GLib.Timeout.Add(durationMs, () =>
            {
                window.statusBar.Pop(window.statusContext);
                window.statusBar.Push(window.statusContext, "Ready");
                window.statusTimeoutId = null;
                return false; // run once
            });
        }



    }

}
