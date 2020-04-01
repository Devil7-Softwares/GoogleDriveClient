using Terminal.Gui;

namespace Devil7.Utils.GDriveCLI.Views
{
    class Dialogs
    {
        public static void Quit()
        {
            int dialogResult = MessageBox.Query(50, 7, "Quit", "Are you sure you want to quit?", "Yes", "No");
            if (dialogResult == 0)
            {
                Application.Top.Running = false;
            } else
            {
                Application.Top.SetFocus(MyDrive.Window);
            }
        }
    }
}
