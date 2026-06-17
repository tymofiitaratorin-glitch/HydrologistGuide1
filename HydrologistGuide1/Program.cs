using HydrologistGuide1.Forms;
using HydrologistGuide1;
using HydrologistGuide1.Forms;
using System;
using System.Windows.Forms;

namespace HydrologistGuide;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}