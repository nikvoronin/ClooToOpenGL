﻿using System;

namespace ClooToOpenTK
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            using (MainWindow mainWindow = new MainWindow())
            {
                mainWindow.Run();
            }
        }
    }
}
