using NickvisionCavalier.GNOME.Views;
using NickvisionCavalier.Shared.Controllers;
using NickvisionCavalier.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using static NickvisionCavalier.Shared.Helpers.Gettext;

namespace NickvisionCavalier.GNOME;

/// <summary>
/// The Program 
/// </summary>
public partial class Program
{
    private readonly Adw.Application _application;
    private MainWindow? _mainWindow;
    private MainWindowController _mainWindowController;

    /// <summary>
    /// Main method
    /// </summary>
    /// <param name="args">string[]</param>
    /// <returns>Return code from Adw.Application.Run()</returns>
    public static int Main(string[] args) => new Program(args).Run();

    /// <summary>
    /// Constructs a Program
    /// </summary>
    public Program(string[] args)
    {
        _application = Adw.Application.New("org.nickvision.cavalier", Gio.ApplicationFlags.NonUnique);
        _mainWindow = null;
        _mainWindowController = new MainWindowController(args);
        _mainWindowController.AppInfo.Changelog =
            @"* Cavalier can now be controlled from command line. Run the app with --help option to see full list of available options.
              * Reverse mirror option is now available with full mirror.
              * It's now possible to set frames per second to 144.
              * Added anti-aliasing, so rounded items now look less pixelated.
              * Added ability to set background image.";
        _application.OnActivate += OnActivate;
        if (File.Exists(Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)) + "/org.nickvision.cavalier.gresource"))
        {
            //Load file from program directory, required for `dotnet run`
            Gio.Functions.ResourcesRegister(Gio.Functions.ResourceLoad(Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)) + "/org.nickvision.cavalier.gresource"));
        }
        else
        {
            var prefixes = new List<string> {
               Directory.GetParent(Directory.GetParent(Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))).FullName).FullName,
               Directory.GetParent(Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))).FullName,
               "/usr"
            };
            foreach (var prefix in prefixes)
            {
                if (File.Exists(prefix + "/share/org.nickvision.cavalier/org.nickvision.cavalier.gresource"))
                {
                    Gio.Functions.ResourcesRegister(Gio.Functions.ResourceLoad(Path.GetFullPath(prefix + "/share/org.nickvision.cavalier/org.nickvision.cavalier.gresource")));
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Runs the program
    /// </summary>
    /// <returns>Return code from Adw.Application.Run()</returns>
    public int Run()
    {
        try
        {
            return _application.RunWithSynchronizationContext();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine($"\n\n{ex.StackTrace}");
            return -1;
        }
    }

    /// <summary>
    /// Occurs when the application is activated
    /// </summary>
    /// <param name="sedner">Gio.Application</param>
    /// <param name="e">EventArgs</param>
    private void OnActivate(Gio.Application sedner, EventArgs e)
    {
        //Set Adw Theme
        _application.StyleManager!.ColorScheme = _mainWindowController.Theme switch
        {
            Theme.Light => Adw.ColorScheme.ForceLight,
            _ => Adw.ColorScheme.ForceDark
        };
        //Main Window
        if (_mainWindow != null)
        {
            _mainWindow!.SetVisible(true);
            _mainWindow.Present();
        }
        else
        {
            _mainWindow = new MainWindow(_mainWindowController, _application);
            _mainWindow.Start();
        }
    }
}
