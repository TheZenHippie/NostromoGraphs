using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using NostromoGraphs.Common;

namespace NostromoGraphs
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            EnsureStandardMenuDropAlignment();
            AutoUpdateIcon();
            base.OnStartup(e);

            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                LogException(args.ExceptionObject as Exception, "AppDomain.UnhandledException");
            };

            DispatcherUnhandledException += (s, args) =>
            {
                LogException(args.Exception, "DispatcherUnhandledException");
                MessageBox.Show($"MUTHUR-6000 Telemetry Exception:\n\n{args.Exception.Message}\n\nCheck error log in %APPDATA%\\NostromoGraphs\\crash.log", "NostromoGraphs Error", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true;
            };
        }

        private static void AutoUpdateIcon()
        {
            try
            {
                if (File.Exists("icon.png"))
                {
                    var pngTime = File.GetLastWriteTimeUtc("icon.png");
                    var icoTime = File.Exists("icon.ico") ? File.GetLastWriteTimeUtc("icon.ico") : DateTime.MinValue;
                    if (pngTime > icoTime)
                    {
                        IcoBuilder.GenerateIcon("icon.png", "icon.ico");
                    }
                }
            }
            catch { }
        }

        public static void EnsureStandardMenuDropAlignment()
        {
            try
            {
                var menuDropAlignmentField = typeof(SystemParameters).GetField("_menuDropAlignment", BindingFlags.NonPublic | BindingFlags.Static);
                if (menuDropAlignmentField != null && (bool)menuDropAlignmentField.GetValue(null)!)
                {
                    menuDropAlignmentField.SetValue(null, false);
                }
            }
            catch { }
        }

        private static void LogException(Exception? ex, string source)
        {
            if (ex == null) return;
            try
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string folder = Path.Combine(appData, "NostromoGraphs");
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
                string logPath = Path.Combine(folder, "crash.log");
                File.AppendAllText(logPath, $"[{DateTime.UtcNow:O}] [{source}] {ex}\n\n");
            }
            catch { }
        }
    }
}

