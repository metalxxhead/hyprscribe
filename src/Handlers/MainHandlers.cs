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
using Microsoft.Win32.SafeHandles;

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
            CoreLogic.ArchiveTab(targetTab);
        }



        internal static void AddEditorTab(Notebook notebook, MainWindow window)
        {
            var textView = new TextView
            {
                WrapMode = WrapMode.WordChar
            };

            var scroller = new ScrolledWindow();
            scroller.Add(textView);

            int index = GenerateNewIndex(notebook, window);

            string fileSavePath = Logic.CoreLogic.GenerateUniqueFileName();

            var tabLabel = CreateTabLabel(notebook, "Tab " + index, window, fileSavePath);

            int page = notebook.AppendPage(scroller, tabLabel);
            notebook.SetTabReorderable(scroller, false);
            notebook.CurrentPage = page;


            window.tabManager.AddTab("Tab " + index, page, fileSavePath);


            CoreLogic.CreateBlankFileIfNotExists(fileSavePath);

            window.tabManager.SaveTabsToDb();

             textView.KeyReleaseEvent += (sender, args) =>
            {
                File.WriteAllText(fileSavePath, textView.Buffer.Text);
            };

            window.ShowAll();

            notebook.CurrentPage = page;
        }



        internal static void AddKnownTabFromDB(Notebook notebook, MainWindow window, TabInfo tabData)
        {
            var textView = new TextView
            {
                WrapMode = WrapMode.WordChar
            };

            var buffer = textView.Buffer;
            buffer.Text = FileUtils.ReadFile(tabData.FilePath);

            var scroller = new ScrolledWindow();
            scroller.Add(textView);

            var tabLabel = CreateTabLabel(notebook, tabData.TabLabel, window, tabData.FilePath);

            int page = notebook.AppendPage(scroller, tabLabel);
            notebook.SetTabReorderable(scroller, false);
            notebook.CurrentPage = page;


             textView.KeyReleaseEvent += (sender, args) =>
            {
                File.WriteAllText(tabData.FilePath, textView.Buffer.Text);
            };



            window.ShowAll();
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



        





    }



}