using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Terminal.Gui;

namespace Devil7.Utils.GDriveCLI.Views
{
    class MyDrive
    {
        #region Variables
        private static Stack<Models.FileListItem> Routes = new Stack<Models.FileListItem>();
        private static Models.FileListItem CurrentDirectory = null;
        #endregion

        #region Properties
        public static Window Window { get; private set; }
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
        #endregion

        #region Private Methods
        private static void CreateListView()
        {
            DataSources.FileListDataSource ds = new DataSources.FileListDataSource();
            Controls.CustomListView listView = new Controls.CustomListView(ds)
            {
                Width = Dim.Fill(1),
                Height = Dim.Fill(1),
                X = 0,
                Y = 0,
                AllowsMarking = true
            };
            listView.OnKeyPressed += ListView_OnKeyPressed;

            Window.Add(listView);

            ChangeDirectory(listView, new Models.FileListItem("root", "My Drive", "me", DateTime.Now, 0, Utils.Constants.MIME_GDRIVE_DIRECTORY), Utils.Direction.Forward);
        }

        private static void ChangeDirectory(ListView listView, Models.FileListItem currentItem, Utils.Direction direction)
        {
            Task.Run(delegate ()
            {
                List<Models.FileListItem> files = Utils.Drive.ListFiles(currentItem.Id).Result;

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

                    files.Insert(0, new Models.FileListItem("", "..", "", DateTime.Now, 0, Utils.Constants.MIME_GDRIVE_DIRECTORY));
                }

                DataSources.FileListDataSource ds = new DataSources.FileListDataSource(files);
                ds.DirectoriesFirst();

                listView.Source = ds;

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
                ListView listView = sender as ListView;
                if (listView != null && listView.SelectedItem >= 0)
                {
                    DataSources.FileListDataSource dataSource = listView.Source as DataSources.FileListDataSource;
                    if (dataSource != null)
                    {
                        Models.FileListItem selectedItem = dataSource.Items[listView.SelectedItem];
                        if (selectedItem.IsDirectory)
                        {
                            if (selectedItem.Name == "..")
                            {
                                ChangeDirectory(listView, Routes.Pop(), Utils.Direction.Backward);
                            }
                            else
                            {
                                ChangeDirectory(listView, selectedItem, Utils.Direction.Forward);
                            }
                        }
                    }
                }
            }
        }
        #endregion
    }
}
