   using System;
   using Gtk;
   using HyprScribe.Models;

   namespace HyprScribe
   {

	class Program
	{
	    static void Main(string[] args)
	    {

			Application.Init();

			var win = new UI.MainWindow();
			win.ShowAll();

			Application.Run();
	    }
	}
   }
