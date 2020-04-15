﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Terminal.Gui;

namespace Devil7.Utils.GDriveCLI.Views
{
    class MyDrive
    {
        #region Variables
        private static readonly Stack<Models.FileListItem> Routes = new Stack<Models.FileListItem>();
        #endregion

        #region Properties
        public static Models.FileListItem CurrentDirectory { get; private set; }
        public static List<Models.FileListItem> Files { get; private set; }
        public static Window Window { get; private set; }

        private static Controls.CustomListView ListView { get; set; }
        #endregion

        #region Public Methods
        public static void Create()
        {
            Window = new Window("My Drive")
            {
                X = 0,
                Y = 1,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            Application.Top.Add(Window);

            CreateListView();
        }

        public static void UpdateDataSource()
        {
            DataSources.FileListDataSource ds = new DataSources.FileListDataSource(Files);
            ds.Sort();

            ListView.Source = ds;
            Files = ds.Items;
        }

        public static void SelectItem(Models.FileListItem item)
        {
            int index = Files.IndexOf(item);
            ListView.SelectedItem = index;
        }
        #endregion

        #region Private Methods
        private static void CreateListView()
        {
            DataSources.FileListDataSource ds = new DataSources.FileListDataSource();
            ListView = new Controls.CustomListView(ds)
            {
                Width = Dim.Fill(1),
                Height = Dim.Fill(1),
                X = 0,
                Y = 0,
                AllowsMarking = true
            };
            ListView.OnKeyPressed += ListView_OnKeyPressed;

            Window.Add(ListView);

            ChangeDirectory(new Models.FileListItem("root", "My Drive", "me", DateTime.Now, 0, Utils.Constants.MIME_GDRIVE_DIRECTORY), Utils.Direction.Forward);
        }

        private static void ChangeDirectory(Models.FileListItem currentItem, Utils.Direction direction)
        {
            Task.Run(delegate ()
            {
                Files = Utils.Drive.ListFiles(currentItem.Id).Result;

                if (currentItem.Id == "root")
                {
                    Routes.Clear();
                }
                else
                {
                    if (direction == Utils.Direction.Forward)
                    {
                        Routes.Push(CurrentDirectory);
                    }

                    Files.Insert(0, new Models.FileListItem("", "..", "", DateTime.Now, 0, Utils.Constants.MIME_GDRIVE_DIRECTORY));
                }

                UpdateDataSource();

                Window.Title = currentItem.Name;

                Application.Refresh();

                CurrentDirectory = currentItem;
            });
        }
        #endregion

        #region Event Handlers
        private static void ListView_OnKeyPressed(object sender, Controls.KeyPressedEventArgs args)
        {
            if (args.KeyEvent.Key == Key.Enter)
            {
                if (sender is ListView listView && listView.SelectedItem >= 0)
                {
                    if (listView.Source is DataSources.FileListDataSource dataSource)
                    {
                        Models.FileListItem selectedItem = dataSource.Items[listView.SelectedItem];
                        if (selectedItem.IsDirectory)
                        {
                            if (selectedItem.Name == "..")
                            {
                                ChangeDirectory(Routes.Pop(), Utils.Direction.Backward);
                            }
                            else
                            {
                                ChangeDirectory(selectedItem, Utils.Direction.Forward);
                            }
                        }
                        else
                        {
                            DownloadFile.Start(selectedItem);
                        }
                    }
                }
            }
        }
        #endregion
    }
}
