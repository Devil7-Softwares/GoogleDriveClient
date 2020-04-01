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
                Models.FileListItem newFolder = Utils.Drive.NewFolder(MyDrive.CurrentDirectory.Id, textField.Text.ToString()).Result;
                MyDrive.Files.Add(newFolder);

                MyDrive.UpdateDataSource();

                MyDrive.SelectItem(newFolder);

                MyDrive.Window.ChildNeedsDisplay();
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
    }
}
