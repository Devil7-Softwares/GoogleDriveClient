﻿using Terminal.Gui;

namespace Devil7.Utils.GDriveCLI.Views
{
    class Menu
    {
        public static void Create()
        {
            MenuBar menu = new MenuBar(new MenuBarItem[] {
                new MenuBarItem ("_File", new MenuItem [] {
                    new MenuItem("_New Folder", "", Dialogs.NewFolder),
                    new MenuItem ("_Quit", "", Dialogs.Quit)
                })
            });

            Application.Top.Add(menu);
        }
    }
}
