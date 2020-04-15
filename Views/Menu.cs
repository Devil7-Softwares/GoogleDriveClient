using Terminal.Gui;

namespace Devil7.Utils.GoogleDriveClient.Views
{
    class Menu
    {
        public static void Create()
        {
            MenuBar menu = new MenuBar(new MenuBarItem[] {
                new MenuBarItem ("_File", new MenuItem [] {
                    new MenuItem("_New Folder", "", Dialogs.NewFolder),
                    new MenuItem("_Upload File", "", UploadFile.Start),
                    new MenuItem ("_Quit", "", Dialogs.Quit)
                }),
                new MenuBarItem ("_View", new MenuItem [] {
                   new MenuItem("Sort _By","", Dialogs.SortBy),
                   new MenuItem("Sort _Order", "", Dialogs.SortOrder)
                })
            });

            Application.Top.Add(menu);
        }
    }
}
