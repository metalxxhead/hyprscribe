using System;
using System.Collections.Generic;
using System.Security.Principal;
using Gtk;
using HyprScribe.Models;
using HyprScribe.UI;
using Internal;

namespace HyprScribe.Handlers 
{
    public static class MainHandlers 
    {

        public static int CountRealTabs(Notebook notebook)
        {
            return notebook.NPages;
        }


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


        public static void RemoveTab(Notebook notebook, string title, MainWindow window)
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




        public static void AddEditorTab(Notebook notebook, MainWindow window)
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

            window.tabManager.AddTab("Tab " + index, "");

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