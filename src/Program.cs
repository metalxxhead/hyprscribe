// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 github.com/metalxxhead

using System;
using System.IO;
using Gtk;
using Mono.Unix.Native;

namespace HyprScribe
{
    class Program
    {
        static int lockFd = -1;

        static void Main(string[] args)
        {
            string userDir = Logic.CoreLogic.getUserDirectory();
            string lockPath = Path.Combine(userDir, "hyprscribe.lock");

            try
            {
                if (!Directory.Exists(userDir))
                    Directory.CreateDirectory(userDir);
            }
            catch (Exception ex)
            {
                ShowFatalError(
                    "Unable to create user data directory:\n" + ex.Message
                );
                return;
            }

            lockFd = Syscall.open(
                lockPath,
                OpenFlags.O_CREAT | OpenFlags.O_RDWR,
                FilePermissions.S_IRUSR | FilePermissions.S_IWUSR
            );

            if (lockFd == -1)
            {
                ShowFatalError(
                    "Unable to open lock file.\n" +
                    "Check permissions for:\n" + userDir
                );
                return;
            }

            Flock fl = new Flock
            {
                l_type = LockType.F_WRLCK,
                l_whence = SeekFlags.SEEK_SET,
                l_start = 0,
                l_len = 0
            };

            if (Syscall.fcntl(lockFd, FcntlCommand.F_SETLK, ref fl) == -1)
            {
                ShowAlreadyRunningDialog();
                return;
            }

            // ---- normal startup ----
            Application.Init();
            var win = new UI.MainWindow();
            win.ShowAll();
            Application.Run();

            // ---- cleanup ----
            fl.l_type = LockType.F_UNLCK;
            Syscall.fcntl(lockFd, FcntlCommand.F_SETLK, ref fl);
            Syscall.close(lockFd);
        }

        static void ShowAlreadyRunningDialog()
        {
            Application.Init();

            var dialog = new MessageDialog(
                null,
                DialogFlags.Modal,
                MessageType.Warning,
                ButtonsType.Ok,
                "Failed to start instance.  Another instance of HyprScribe is already running."
            );

            dialog.Run();
            dialog.Destroy();
        }

        static void ShowFatalError(string message)
        {
            Application.Init();

            var dialog = new MessageDialog(
                null,
                DialogFlags.Modal,
                MessageType.Error,
                ButtonsType.Ok,
                message
            );

            dialog.Run();
            dialog.Destroy();
        }
    }
}
