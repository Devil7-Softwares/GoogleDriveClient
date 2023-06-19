using NStack;
using Terminal.Gui;

namespace Devil7.Utils.GoogleDriveClient.Views
{
    class Dialogs
    {
        public static void Delete()
        {
            int result = MessageBox.Query(50, 7, "Warning!", string.Format("Are you sure? Do you want to permanently delete selected {0}?", MyDrive.SelectedItem.IsDirectory ? "directory" : "file"), "Yes", "No");

            switch (result)
            {
                case 0:
                    Utils.Drive.Delete(MyDrive.SelectedItem.Id).ContinueWith((task) =>
                    {
                        if (task.Result)
                        {
                            Application.MainLoop.Invoke(() =>
                            {
                                var selectedItemIndex = MyDrive.Files.IndexOf(MyDrive.SelectedItem);

                                if (selectedItemIndex == (MyDrive.Files.Count - 1))
                                    MyDrive.SelectItem(MyDrive.Files[selectedItemIndex - 1]);

                                MyDrive.Files.Remove(MyDrive.SelectedItem);

                                MyDrive.Window.SetFocus();
                                Application.Refresh();
                            });
                        }
                    });
                    break;
                case 1:
                    MyDrive.Window.SetFocus();
                    break;
            }
        }

        public static void MoveToTrash()
        {
            int result = MessageBox.Query(50, 7, "Warning!", string.Format("Are you sure? Do you want to move selected {0} to Trash?", MyDrive.SelectedItem.IsDirectory ? "directory" : "file"), "Yes", "No");

            switch (result)
            {
                case 0:
                    Utils.Drive.MoveToTrash(MyDrive.SelectedItem.Id).ContinueWith((task) =>
                    {
                        if (task.Result)
                        {
                            Application.MainLoop.Invoke(() =>
                            {
                                var selectedItemIndex = MyDrive.Files.IndexOf(MyDrive.SelectedItem);

                                if (selectedItemIndex == (MyDrive.Files.Count - 1))
                                    MyDrive.SelectItem(MyDrive.Files[selectedItemIndex - 1]);

                                MyDrive.Files.Remove(MyDrive.SelectedItem);

                                MyDrive.Window.SetFocus();
                                Application.Refresh();
                            });
                        }
                    });
                    break;
                case 1:
                    MyDrive.Window.SetFocus();
                    break;
            }
        }

        public static void NewFolder()
        {
            string newFolderName = "New Folder";
            int newFolderCount = 0;

            while (MyDrive.Files != null && MyDrive.Files.Find((file) => file.Name.Equals(newFolderName)) != null)
            {
                newFolderName = string.Format("New Folder ({0})", ++newFolderCount);
            }

            Dialog dialog = new Dialog("Enter Folder Name", 50, 8);
            TextField textField = new TextField(newFolderName)
            {
                X = 1,
                Y = 1,
                Width = Dim.Fill(1)
            };
            dialog.Add(textField);

            Button btnOk = new Button("OK", true);
            btnOk.Clicked += delegate ()
            {
                Utils.Drive.NewFolder(MyDrive.CurrentDirectory.Id, textField.Text.ToString()).ContinueWith((task) =>
                {
                    Application.MainLoop.Invoke(() =>
                    {
                        MyDrive.Files.Add(task.Result);

                        MyDrive.SortItems();

                        MyDrive.SelectItem(task.Result);

                        dialog.Running = false;
                        MyDrive.Window.SetFocus();
                    });
                });
            };
            Button btnCancel = new Button("Cancel");
            btnCancel.Clicked += delegate ()
            {
                dialog.Running = false;
                MyDrive.Window.SetFocus();
            };
            dialog.AddButton(btnOk);
            dialog.AddButton(btnCancel);

            Application.Run(dialog);
            Application.Refresh();
        }

        public static void Quit()
        {
            int dialogResult = MessageBox.Query(50, 7, "Quit", "Are you sure you want to quit?", "Yes", "No");
            if (dialogResult == 0)
            {
                Application.RequestStop();
            }
            else
            {
                MyDrive.Window.SetFocus();
            }
        }

        public static void SortBy()
        {
            Dialog dialog = new Dialog("Sort By", 25, 10);
            RadioGroup radioGroup = new RadioGroup(5, 1, new ustring[] { "Name", "Date" }, (int)Utils.Settings.SortBy);
            dialog.Add(radioGroup);

            Button btnOk = new Button("OK", true);
            btnOk.Clicked += delegate ()
            {
                Utils.Settings.SortBy = (Utils.SortBy)radioGroup.SelectedItem;
                Utils.Settings.Save();

                MyDrive.SortItems();

                dialog.Running = false;
                MyDrive.Window.SetFocus();
            };
            Button btnCancel = new Button("Cancel");
            btnCancel.Clicked += delegate ()
            {
                dialog.Running = false;
                MyDrive.Window.SetFocus();
            };
            dialog.AddButton(btnOk);
            dialog.AddButton(btnCancel);

            Application.Run(dialog);
            Application.Refresh();
        }

        public static void SortOrder()
        {
            Dialog dialog = new Dialog("Sort By", 25, 10);
            RadioGroup radioGroup = new RadioGroup(3, 1, new ustring[] { "Ascending", "Descending" }, (int)Utils.Settings.SortOrder);
            dialog.Add(radioGroup);

            Button btnOk = new Button("OK", true);
            btnOk.Clicked += delegate ()
            {
                Utils.Settings.SortOrder = (Utils.SortOrder)radioGroup.SelectedItem;
                Utils.Settings.Save();

                MyDrive.SortItems();

                dialog.Running = false;
                MyDrive.Window.SetFocus();
            };
            Button btnCancel = new Button("Cancel");
            btnCancel.Clicked += delegate ()
            {
                dialog.Running = false;
                MyDrive.Window.SetFocus();
            };
            dialog.AddButton(btnOk);
            dialog.AddButton(btnCancel);

            Application.Run(dialog);
            Application.Refresh();
        }
    }
}
