using NickvisionCavalier.GNOME.Views;
using NickvisionCavalier.Shared.Controllers;
using NickvisionCavalier.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using static NickvisionCavalier.Shared.Helpers.Gettext;

namespace NickvisionCavalier.GNOME;

/// <summary>
/// The Program 
/// </summary>
public partial class Program
{
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nuint gtk_file_chooser_cell_get_type();

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint g_resource_load(string path);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void g_resources_register(nint file);

    private readonly Adw.Application _application;
    private MainWindow? _mainWindow;
    private MainWindowController _mainWindowController;

    /// <summary>
    /// Main method
    /// </summary>
    /// <param name="args">string[]</param>
    /// <returns>Return code from Adw.Application.Run()</returns>
    public static int Main(string[] args) => new Program().Run(args);

    /// <summary>
    /// Constructs a Program
    /// </summary>
    public Program()
    {
        gtk_file_chooser_cell_get_type();
        _application = Adw.Application.New("org.nickvision.cavalier", Gio.ApplicationFlags.FlagsNone);
        _mainWindow = null;
        _mainWindowController = new MainWindowController();
        _mainWindowController.AppInfo.ID = "org.nickvision.cavalier";
        _mainWindowController.AppInfo.Name = "Nickvision Cavalier";
        _mainWindowController.AppInfo.ShortName = _("Cavalier");
        _mainWindowController.AppInfo.Description = $"{_("Visualize audio with CAVA")}.";
        _mainWindowController.AppInfo.Version = "2023.8.0-next";
        _mainWindowController.AppInfo.Changelog = "";
        _mainWindowController.AppInfo.GitHubRepo = new Uri("https://github.com/NickvisionApps/Cavalier");
        _mainWindowController.AppInfo.IssueTracker = new Uri("https://github.com/NickvisionApps/Cavalier/issues/new");
        _mainWindowController.AppInfo.SupportUrl = new Uri("https://github.com/NickvisionApps/Cavalier/discussions");
        _application.OnActivate += OnActivate;
        if (File.Exists(Path.GetFullPath(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)) + "/org.nickvision.cavalier.gresource"))
        {
            //Load file from program directory, required for `dotnet run`
            g_resources_register(g_resource_load(Path.GetFullPath(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)) + "/org.nickvision.cavalier.gresource"));
        }
        else
        {
            var prefixes = new List<string> {
               Directory.GetParent(Directory.GetParent(Path.GetFullPath(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))).FullName).FullName,
               Directory.GetParent(Path.GetFullPath(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))).FullName,
               "/usr"
            };
            foreach (var prefix in prefixes)
            {
                if (File.Exists(prefix + "/share/org.nickvision.cavalier/org.nickvision.cavalier.gresource"))
                {
                    g_resources_register(g_resource_load(Path.GetFullPath(prefix + "/share/org.nickvision.cavalier/org.nickvision.cavalier.gresource")));
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Runs the program
    /// </summary>
    /// <returns>Return code from Adw.Application.Run()</returns>
    public int Run(string[] args)
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
