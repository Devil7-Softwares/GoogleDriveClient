using Google.Apis.Drive.v3.Data;
using System.Collections.Generic;
using Terminal.Gui;

namespace Devil7.Utils.GDriveCLI.Views
{
    class Dialogs
    {
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
            btnOk.Clicked = delegate ()
            {
                Utils.Drive.NewFolder(MyDrive.CurrentDirectory.Id, textField.Text.ToString()).ContinueWith((task) =>
                {
                    Application.MainLoop.Invoke(() =>
                    {
                        MyDrive.Files.Add(task.Result);

                        MyDrive.SortItems();

                        MyDrive.SelectItem(task.Result);

                        dialog.Running = false;
                    });
                });
            };
            Button btnCancel = new Button("Cancel");
            btnCancel.Clicked = delegate ()
            {
                dialog.Running = false;
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
                Application.Top.Running = false;
            }
            else
            {
                Application.Top.SetFocus(MyDrive.Window);
            }
        }

        public static void SortBy()
        {
            Dialog dialog = new Dialog("Sort By", 25, 10);
            RadioGroup radioGroup = new RadioGroup(5, 1, new string[] { "Name", "Date" }, (int)Utils.Settings.SortBy);
            dialog.Add(radioGroup);

            Button btnOk = new Button("OK", true);
            btnOk.Clicked = delegate ()
            {
                Utils.Settings.SortBy = (Utils.SortBy)radioGroup.Selected;
                Utils.Settings.Save();

                MyDrive.SortItems();

                dialog.Running = false;
            };
            Button btnCancel = new Button("Cancel");
            btnCancel.Clicked = delegate ()
            {
                dialog.Running = false;
            };
            dialog.AddButton(btnOk);
            dialog.AddButton(btnCancel);

            Application.Run(dialog);
            Application.Refresh();
        }

        public static void SortOrder()
        {
            Dialog dialog = new Dialog("Sort By", 25, 10);
            RadioGroup radioGroup = new RadioGroup(3, 1, new string[] { "Ascending", "Descending" }, (int)Utils.Settings.SortOrder);
            dialog.Add(radioGroup);

            Button btnOk = new Button("OK", true);
            btnOk.Clicked = delegate ()
            {
                Utils.Settings.SortOrder = (Utils.SortOrder)radioGroup.Selected;
                Utils.Settings.Save();

                MyDrive.SortItems();

                dialog.Running = false;
            };
            Button btnCancel = new Button("Cancel");
            btnCancel.Clicked = delegate ()
            {
                dialog.Running = false;
            };
            dialog.AddButton(btnOk);
            dialog.AddButton(btnCancel);

            Application.Run(dialog);
            Application.Refresh();
        }
    }
}
