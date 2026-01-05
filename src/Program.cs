   using System;
   using Gtk;
   using HyprScribe.Config;
   using HyprScribe.Models;

   namespace HyprScribe
   {

	class Program
	{
	    static void Main(string[] args)
	    {
		var cfg = ConfigManager.LoadConfig();

		Application.Init();

		var win = new UI.MainWindow(cfg);
		win.ShowAll();

		Application.Run();
	    }
	}
   }
