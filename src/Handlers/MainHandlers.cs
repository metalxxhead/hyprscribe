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

        public static Widget CreateTabLabel(Notebook notebook, string title, MainWindow window)
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
                //int page = notebook.CurrentPage;
                //notebook.RemovePage(page);
                RemoveTab(notebook, title, window);
            };

            hbox.PackStart(label, true, true, 0);
            hbox.PackStart(closeButton, false, false, 0);

            hbox.ShowAll();
            return hbox;
        }


        internal static void RemoveTab(Notebook notebook, string title, MainWindow window)
        {
  			for (int i = 0; i < notebook.NPages; i++)
			{

				var tabLabelBox = notebook.GetTabLabel(notebook.GetNthPage(i));
				var tabTitleLabel = (Gtk.Label)((Container)tabLabelBox).Children[0];
				var labelText = tabTitleLabel.LabelProp;

				if (title == labelText)
				{
					notebook.RemovePage(i);
                    window.tabManager.RemoveTabFromList(labelText);
				}
			} 
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

            var tabLabel = CreateTabLabel(notebook, "Tab " + index, window);

            int page = notebook.AppendPage(scroller, tabLabel);
            notebook.SetTabReorderable(scroller, true);
            notebook.CurrentPage = page;

            string fileSavePath = Logic.CoreLogic.GenerateUniqueFileName();
            window.tabManager.AddTab("Tab " + index, page, fileSavePath);

            CoreLogic.CreateBlankFileIfNotExists(fileSavePath);

            window.tabManager.SaveTabsToDb();

             textView.KeyReleaseEvent += (sender, args) =>
            {
                File.WriteAllText(fileSavePath, textView.Buffer.Text);
            };

            

            //Logic.CoreLogic.writeTabInfoFile(window);

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

            //int index = GenerateNewIndex(notebook, window);

            var tabLabel = CreateTabLabel(notebook, tabData.TabLabel, window);

            int page = notebook.AppendPage(scroller, tabLabel);
            notebook.SetTabReorderable(scroller, true);
            notebook.CurrentPage = page;

            //string fileSavePath = Logic.CoreLogic.GenerateUniqueFileName();
            //window.tabManager.AddTab("Tab " + index, page, fileSavePath);

            //CoreLogic.CreateBlankFileIfNotExists(fileSavePath);

            //window.tabManager.SaveTabsToDb();

             textView.KeyReleaseEvent += (sender, args) =>
            {
                File.WriteAllText(tabData.FilePath, textView.Buffer.Text);
            };

            //Logic.CoreLogic.writeTabInfoFile(window);

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