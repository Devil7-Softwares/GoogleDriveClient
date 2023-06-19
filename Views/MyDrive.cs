using System;
using System.Collections.Generic;
using Terminal.Gui;

namespace Devil7.Utils.GoogleDriveClient.Views
{
    class MyDrive
    {
        #region Variables
        private static readonly Stack<Models.FileListItem> Routes = new Stack<Models.FileListItem>();
        #endregion

        #region Properties
        public static Models.FileListItem CurrentDirectory { get; private set; }
        public static Window Window { get; private set; }

        public static List<Models.FileListItem> Files { get => ListView.Source is DataSources.FileListDataSource ? ((DataSources.FileListDataSource)ListView.Source).Items : null; }
        public static Models.FileListItem SelectedItem { get => Files == null || ListView.SelectedItem < 0 || ListView.SelectedItem >= Files.Count ? null : Files[ListView.SelectedItem]; }

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

        public static void SortItems()
        {
            DataSources.FileListDataSource ds = (DataSources.FileListDataSource)ListView.Source;
            ds.Sort();
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
                AllowsMarking = true,
            };
            ListView.OnKeyPressed += ListView_OnKeyPressed;
            ListView.OpenSelectedItem += ListView_OpenSelectedItem;

            Window.Add(ListView);

            ChangeDirectory(new Models.FileListItem("root", "My Drive", "me", DateTime.Now, 0, Utils.Constants.MIME_GDRIVE_DIRECTORY), Utils.Direction.Forward);
        }

        private static void ChangeDirectory(Models.FileListItem currentItem, Utils.Direction direction)
        {

            Utils.Drive.ListFiles(currentItem.Id).ContinueWith((task) =>
            {
                Application.MainLoop.Invoke(() =>
                {
                    DataSources.FileListDataSource ds = new DataSources.FileListDataSource(task.Result);
                    ds.Sort();

                    ListView.Source = ds;

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

                    Window.Title = currentItem.Name;

                    CurrentDirectory = currentItem;
                });
            });
        }

        private static void OpenItem()
        {
            if (SelectedItem.IsDirectory)
            {
                if (SelectedItem.Name == "..")
                {
                    ChangeDirectory(Routes.Pop(), Utils.Direction.Backward);
                }
                else
                {
                    ChangeDirectory(SelectedItem, Utils.Direction.Forward);
                }
            }
            else
            {
                DownloadFile.Start(SelectedItem);
            }
        }
        #endregion

        #region Event Handlers
        private static void ListView_OnKeyPressed(object sender, Controls.KeyPressedEventArgs args)
        {
            if (SelectedItem == null)
                return;

            if (args.KeyEvent.Key == Key.DeleteChar)
            {
                /// TODO: Implement Permenent Delete on Shift+Delete
                Dialogs.MoveToTrash();
            }
        }

        private static void ListView_OpenSelectedItem(ListViewItemEventArgs args)
        {
            if (SelectedItem == null)
                return;

            OpenItem();
        }
        #endregion
    }
}
