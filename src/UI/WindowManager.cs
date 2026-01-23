// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 github.com/metalxxhead

using System;
using System.Collections.Generic;
using System.Linq;
using Gtk;
using HyprScribe.Models;

namespace HyprScribe.UI
{
    public static class WindowManager
    {
        private static List<MainWindow> _windows = new List<MainWindow>();
        private static TabManager _sharedTabManager = new TabManager();

        public static TabManager SharedTabManager => _sharedTabManager;

        public static void RegisterWindow(MainWindow window)
        {
            if (!_windows.Contains(window))
            {
                _windows.Add(window);
                Console.WriteLine($"Window registered. Total windows: {_windows.Count}");
            }
        }

        public static void UnregisterWindow(MainWindow window)
        {
            _windows.Remove(window);
            Console.WriteLine($"Window unregistered. Total windows: {_windows.Count}");
        }

        public static MainWindow GetAnyOtherWindow(MainWindow excludeWindow)
        {
            return _windows.FirstOrDefault(w => w != excludeWindow);
        }

        public static int GetWindowCount()
        {
            return _windows.Count;
        }

        public static MainWindow CreateNewWindow()
        {
            var newWindow = new MainWindow();
            newWindow.ShowAll();
            return newWindow;
        }

        public static MainWindow GetWindowForNotebook(Notebook notebook)
        {
            return _windows.FirstOrDefault(w => w.notebook == notebook);
        }

    }
}