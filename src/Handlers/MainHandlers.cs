using System;
using Gtk;
using HyprScribe.UI;

namespace HyprScribe.Handlers 
{
    public static class MainHandlers 
    {

        public static int CountRealTabs(Notebook notebook)
        {
            return notebook.NPages;
        }


        public static Widget CreateTabLabel(Notebook notebook, string title)
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
                int page = notebook.CurrentPage;
                notebook.RemovePage(page);
            };

            hbox.PackStart(label, true, true, 0);
            hbox.PackStart(closeButton, false, false, 0);

            hbox.ShowAll();
            return hbox;
        }


        public static void AddEditorTab(Notebook notebook, MainWindow window)
        {
            var textView = new TextView
            {
                WrapMode = WrapMode.WordChar
            };

            var scroller = new ScrolledWindow();
            scroller.Add(textView);

            var tabLabel = Handlers.MainHandlers.CreateTabLabel(notebook, $"Tab {window.tabCounter++}");

            int page = notebook.AppendPage(scroller, tabLabel);
            notebook.SetTabReorderable(scroller, true);
            notebook.CurrentPage = page;

            window.ShowAll();

        }

    }
}