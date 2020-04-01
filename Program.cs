using System;
using Terminal.Gui;

namespace Devil7.Utils.GDriveCLI
{
    class Program
    {
        static void Main()
        {
            if (Utils.Drive.IsAuthorized())
            {
                Console.WriteLine("Application already authorized!");
            }
            else
            {
                if (Utils.Drive.Authorize())
                {
                    Console.WriteLine("Authorization successful!");
                }
                else
                {
                    return;
                }
            }

            if (Utils.Drive.Init())
            {
                Console.WriteLine("API initialization successful! Loading UI...");
            }
            else
            {
                Console.WriteLine("API initialization failed!");
                return;
            }

            Application.Init();

            Views.MyDrive.Create();
            Views.Menu.Create();

            Application.Run();
        }
    }
}
