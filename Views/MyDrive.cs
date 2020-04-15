using System;
using System.Collections.Generic;
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
        public static List<Models.FileListItem> Files { get => ((DataSources.FileListDataSource)ListView.Source).Items; }
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
                AllowsMarking = true
            };
            ListView.OnKeyPressed += ListView_OnKeyPressed;

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
