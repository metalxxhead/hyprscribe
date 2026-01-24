// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 github.com/metalxxhead

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Principal;
using Gtk;
using Gdk;
using Pango;
using HyprScribe.Logic;
using HyprScribe.Models;
using HyprScribe.UI;
using HyprScribe.Utils;
using Internal;


namespace HyprScribe.Handlers 
{
    public static class MainHandlers 
    {



	internal static Widget CreateTabLabel(
		Notebook notebook,
		Widget pageWidget,
		string initialText,
		MainWindow window,
		string filePath
	)
	{
		// Enforce invariants immediately
		pageWidget.Data["filePath"] = filePath;
		pageWidget.Data["tabLabel"] = initialText;

		var box = new Box(Orientation.Horizontal, 6);

		var label = new Label(initialText)
		{
			Xalign = 0,
			Ellipsize = Pango.EllipsizeMode.End,
			WidthChars = 5,
			MaxWidthChars = 20
		};

		pageWidget.Data["tabLabelWidget"] = label;

		var closeBtn = new Button("×")
		{
			Relief = ReliefStyle.None,
			FocusOnClick = false
		};

		// closeBtn.Clicked += (s, e) =>
		// {
		// 	// ---- Confirmation dialog ----
		// 	var dialog = new MessageDialog(
		// 		window,
		// 		DialogFlags.Modal,
		// 		MessageType.Question,
		// 		ButtonsType.None,
		// 		"Archive this tab?"
		// 	);

		// 	dialog.SecondaryText =
		// 		"The tab will be moved to archived_tabs and require manual retrieval.\n\n" +
		// 		"Do you want to archive it now?";

		// 	dialog.AddButton("_Cancel", ResponseType.Cancel);
		// 	dialog.AddButton("_Archive Tab", ResponseType.Accept);
		// 	dialog.DefaultResponse = ResponseType.Cancel;

		// 	var response = (ResponseType)dialog.Run();
		// 	dialog.Destroy();

		// 	if (response != ResponseType.Accept)
		// 		return;

		// 	// ---- Hard invariants ----
		// 	if (!(pageWidget.Data["filePath"] is string path))
		// 		throw new InvalidOperationException("Close tab failed: missing filePath metadata.");

		// 	string tabLabelText =
		// 		(pageWidget.Data["tabLabel"] as string) ??
		// 		initialText ??
		// 		"Untitled";

		// 	// ---- Archive FIRST (so path still exists) ----
		// 	// Uses your existing routine from the older RemoveTab() flow.
		// 	Logic.CoreLogic.ArchiveTab(tabLabelText, path, window);

		// 	// ---- Remove from notebook ----
		// 	int idx = notebook.PageNum(pageWidget);
		// 	if (idx >= 0)
		// 		notebook.RemovePage(idx);

		// 	// ---- Update model + DB ----
		// 	window.tabManager.RemoveTabFromList(path);
		// 	window.tabManager.RemoveTabFromDb(path);

		// 	// Important: SaveTabsToDb() does not delete stale rows by itself,
		// 	// but calling it keeps remaining indexes/rows consistent.
		// 	window.tabManager.SaveTabsToDb();

		// 	// Optional: status
		// 	SetTimedStatus(window.statusContext, $"Archived {tabLabelText}", window, 1200);

		// };

		closeBtn.Clicked += (s, e) =>
		{
			// Resolve CURRENT window dynamically
			var currentNotebook = pageWidget.Parent as Notebook;
			if (currentNotebook == null)
				return;

			var window2 = WindowManager.GetWindowForNotebook(currentNotebook);
			if (window2 == null)
				return;

			// ---- Confirmation dialog ----
			var dialog = new MessageDialog(
				window2,
				DialogFlags.Modal,
				MessageType.Question,
				ButtonsType.None,
				"Archive this tab?"
			);

			dialog.SecondaryText =
				"The tab will be moved to archived_tabs and require manual retrieval.\n\n" +
				"Do you want to archive it now?";

			dialog.AddButton("_Cancel", ResponseType.Cancel);
			dialog.AddButton("_Archive Tab", ResponseType.Accept);
			dialog.DefaultResponse = ResponseType.Cancel;

			var response = (ResponseType)dialog.Run();
			dialog.Destroy();

			if (response != ResponseType.Accept)
				return;

			// ---- Metadata (authoritative) ----
			string path = pageWidget.Data["filePath"] as string;
			string tabLabelText =
				pageWidget.Data["tabLabel"] as string ?? initialText ?? "Untitled";

			if (path == null)
				throw new InvalidOperationException("Missing filePath on tab widget.");

			// ---- Archive FIRST ----
			Logic.CoreLogic.ArchiveTab(tabLabelText, path, window2);

			// ---- Remove page from the CURRENT notebook ----
			int idx = currentNotebook.PageNum(pageWidget);
			if (idx >= 0)
				currentNotebook.RemovePage(idx);

			// ---- Update model + DB ----
			window2.tabManager.RemoveTabFromList(path);
			window2.tabManager.RemoveTabFromDb(path);
			window2.tabManager.SaveTabsToDb();

			SetTimedStatus(
				window2.statusContext,
				$"Archived {tabLabelText}",
				window2,
				1200
			);
		};



		box.PackStart(label, true, true, 0);
		box.PackStart(closeBtn, false, false, 0);

		box.ShowAll();
		return box;
	}


		internal static void SetTabLabel(Widget pageWidget, string newText)
		{
			pageWidget.Data["tabLabel"] = newText;

			var lbl = pageWidget.Data["tabLabelWidget"] as Label;
			if (lbl != null)
				lbl.Text = newText;
		}



        // public static Widget CreateTabLabel(Notebook notebook, string title, MainWindow window, string fileSavePath)
        // {
        //     var hbox = new Box(Orientation.Horizontal, 5);

        //     var label = new Label(title);

        //     var closeButton = new Button("×")
        //     {
        //         Relief = ReliefStyle.None,
        //         FocusOnClick = false
        //     };

		// 	closeButton.Clicked += (sender, e) =>
		// 	{
		// 		var dialog = new MessageDialog(
		// 			window,
		// 			DialogFlags.Modal,
		// 			MessageType.Question,
		// 			ButtonsType.None,
		// 			"Close this tab?"
		// 		);

		// 		dialog.SecondaryText =
		// 			"The tab will be moved to archived_tabs and require manual retrieval.  Is that what you want?";

		// 		dialog.AddButton("_Cancel", ResponseType.Cancel);
		// 		dialog.AddButton("_Close Tab", ResponseType.Accept);

		// 		dialog.DefaultResponse = ResponseType.Cancel;

		// 		var response = (ResponseType)dialog.Run();
		// 		dialog.Destroy();

		// 		if (response == ResponseType.Accept)
		// 		{
		// 			RemoveTab(notebook, title, window, fileSavePath);
		// 		}
		// 	};


        //     hbox.PackStart(label, true, true, 0);
        //     hbox.PackStart(closeButton, false, false, 0);

        //     hbox.ShowAll();
        //     return hbox;
        // }

		// internal static void SaveNow(string statusMsg, uint durationMs = 500)
		// {
		// 	File.WriteAllText(filePath, textView.Buffer.Text ?? "");
		// 	SetTimedStatus(window.statusContext, statusMsg, window, durationMs);
		// }



		internal static void SaveAndStatusFromTextView(TextView textView)
		{
			// Walk up the widget tree
			var scroller = textView.Parent as ScrolledWindow;
			if (scroller == null)
				return;

			var notebook = scroller.Parent as Notebook;
			if (notebook == null)
				return;

			var window = WindowManager.GetWindowForNotebook(notebook);
			if (window == null)
				return;

			string tabLabel =
				scroller.Data["tabLabel"] as string ?? "Unknown Tab";

			string filePath =
				scroller.Data["filePath"] as string ?? "Unknown Path";

			// Always save
			File.WriteAllText(filePath, textView.Buffer.Text);

			SetTimedStatus(
				window.statusContext,
				$"Saved {tabLabel} to {filePath}",
				window,
				800
			);
		}





		internal static void WireEditorAutosaveUndoRedo(
			TextView textView,
			Notebook notebook,
			MainWindow window,
			string filePath,
			Func<string> getTabLabelForStatus // so we can say "Saved Tab X"
		)
		{
			var undoStack = new Stack<string>();
			var redoStack = new Stack<string>();
		    bool isUndoing = false;

			// Track previous text so undo actually reverts to the prior state.
			string lastText = textView.Buffer.Text ?? "";


			textView.Buffer.Changed += (s, e) =>
			{
				string current = textView.Buffer.Text ?? "";

				if (!isUndoing)
				{
					undoStack.Push(lastText);
					redoStack.Clear();
				}

				lastText = current;

				// save + status logic
				SaveAndStatusFromTextView(textView);
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

					redoStack.Push(textView.Buffer.Text ?? "");
					textView.Buffer.Text = undoStack.Pop();

					lastText = textView.Buffer.Text ?? "";
					isUndoing = false;

					// Changed handler already saved, but giving a clearer status feels nice:
					//SetTimedStatus(window.statusContext, $"Undo: {getTabLabelForStatus()}", window, 700);

					args.RetVal = true;
					return;
				}

				// REDO (Ctrl+Shift+Z)
				if (ctrl && shift &&
					(args.Event.Key == Gdk.Key.z || args.Event.Key == Gdk.Key.Z) &&
					redoStack.Count > 0)
				{
					isUndoing = true;

					undoStack.Push(textView.Buffer.Text ?? "");
					textView.Buffer.Text = redoStack.Pop();

					lastText = textView.Buffer.Text ?? "";
					isUndoing = false;

					//SetTimedStatus(window.statusContext, $"Redo: {getTabLabelForStatus()}", window, 700);

					args.RetVal = true;
					return;
				}
			};
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

		//Console.WriteLine($"AddEditorTab called - Current stack trace:");
    	//Console.WriteLine(Environment.StackTrace);

	    //var undoStack = new Stack<string>();
	    //var redoStack = new Stack<string>();
	    //bool isUndoing = false;

	    string fileSavePath = Logic.CoreLogic.GenerateUniqueFileName();

		var scroller = new ScrolledWindow();
	    
	    int index = GenerateNewIndex(notebook, window);
	    
		var tabLabel = CreateTabLabel(
		notebook,
		scroller,
		"Tab " + index,
		window,
		fileSavePath
	    );

	    var textView = new TextView
	    {
			WrapMode = Gtk.WrapMode.WordChar,
			LeftMargin = 40, 
			RightMargin = 40,
			TopMargin = 40, 
			BottomMargin = 30
	    };

		var fontDesc = FontDescription.FromString("Cantarell 14");
		textView.ModifyFont(fontDesc);

		WireEditorAutosaveUndoRedo(
			textView,
			notebook,
			window,
			fileSavePath,
			() => (string)(scroller.Data["tabLabel"] ?? $"Tab {index}")
		);

	    
	    scroller.Add(textView);

	    int page = notebook.AppendPage(scroller, tabLabel);
		scroller.Data["tabLabel"] = $"Tab {index}";
		scroller.Data["filePath"] = fileSavePath;

	    notebook.SetTabReorderable(scroller, false);
		//notebook.SetTabReorderable(scroller, true);  // Enable drag and drop
		notebook.SetTabDetachable(scroller, true);

	    CoreLogic.CreateBlankFileIfNotExists(fileSavePath);
	    window.tabManager.AddTab("Tab " + index, page, fileSavePath);
	    window.tabManager.SaveTabsToDb();

		


	    window.ShowAll();

		notebook.CurrentPage = page;
		textView.GrabFocus();
	}




	internal static void AddKnownTabFromDB(Notebook notebook, MainWindow window, TabInfo tabData)
	{
	    //var undoStack = new Stack<string>();
	    //var redoStack = new Stack<string>();
	    //bool isUndoing = false;

	    var textView = new TextView
	    {
			WrapMode = Gtk.WrapMode.WordChar,
			LeftMargin = 40, 
			RightMargin = 40,
			TopMargin = 40, 
			BottomMargin = 30
	    };

		var fontDesc = FontDescription.FromString("Cantarell 14");
		textView.ModifyFont(fontDesc);

	    // Load file content
	    textView.Buffer.Text = FileUtils.ReadFile(tabData.FilePath);

		WireEditorAutosaveUndoRedo(
			textView,
			notebook,
			window,
			tabData.FilePath,
			() => tabData.TabLabel
		);


	    var scroller = new ScrolledWindow();
	    scroller.Add(textView);

		scroller.Data["tabLabel"] = tabData.TabLabel;
		scroller.Data["filePath"] = tabData.FilePath;


	    var tabLabel = CreateTabLabel(
		notebook,
		scroller,
		tabData.TabLabel,
		window,
		tabData.FilePath
	    );

	    int page = notebook.AppendPage(scroller, tabLabel);
	    notebook.SetTabReorderable(scroller, false);
		//notebook.SetTabReorderable(scroller, true);  // Enable drag and drop
		notebook.SetTabDetachable(scroller, true);


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
						// get tab's label
						var tabLabelBox = notebook.GetTabLabel(notebook.GetNthPage(activeIndex));
						var tabTitleLabel = (Gtk.Label)((Container)tabLabelBox).Children[0];
						var labelText = tabTitleLabel.LabelProp;

                        string filePath = dialog.Filename;
                        File.WriteAllText(filePath, textContent);
                        //SetTimedStatus(window.statusContext, "Exported file to " + filePath, window);
						//SetTimedStatus(window.statusContext, "Exported " + labelText + " to " + filePath, window, 1500);


							var currentWindow = WindowManager.GetWindowForNotebook(
								(Notebook)textView.Parent.Parent   // TextView → ScrolledWindow → Notebook
							);

							if (currentWindow != null)
							{
								SetTimedStatus(
									currentWindow.statusContext,
									"Exported " + labelText + " to " + filePath,
									currentWindow,
									1500
								);
							}



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
