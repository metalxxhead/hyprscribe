// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 github.com/metalxxhead

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
