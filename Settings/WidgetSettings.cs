using System;
using System.IO;
using System.Text.Json;

namespace NostromoGraphs.Settings
{
    public class WidgetSettings
    {
        // Phosphor & Styling (Default: Matrix / Nostromo Green, Vintage Monochrome)
        public string PhosphorColorHex { get; set; } = "#00FF66";
        public string BackgroundColorHex { get; set; } = "#08140B";
        public double BackgroundOpacity { get; set; } = 0.90;
        public double HudBrightness { get; set; } = 1.0;
        public bool IsMonochrome { get; set; } = true;


        // Display Switching & Timing (10 to 60 seconds)
        public double DisplayDurationSeconds { get; set; } = 20.0;
        public bool AutoCycleEnabled { get; set; } = true;
        public int LastSelectedDisplayIndex { get; set; } = 0;

        // Visual Effects Suite
        public bool BloomEnabled { get; set; } = true;
        public double BloomIntensity { get; set; } = 8.0;

        public bool CrtScanlinesEnabled { get; set; } = false;
        public double ScanlineThickness { get; set; } = 3.0;

        public bool CrtGlitchEnabled { get; set; } = true;
        public double GlitchIntensity { get; set; } = 15.0;

        public bool ScreenClearTransitionsEnabled { get; set; } = true;
        public double TransitionDurationSeconds { get; set; } = 0.85;

        public bool CrtSnowEnabled { get; set; } = false;
        public double SnowAmount { get; set; } = 25.0;

        // Window Frame & Layout
        public bool AlwaysOnTop { get; set; } = false;
        public bool WindowShadow { get; set; } = true;

        public double? WindowWidth { get; set; } = 860;
        public double? WindowHeight { get; set; } = 580;
        public double? WindowLeft { get; set; }
        public double? WindowTop { get; set; }

        private static readonly object FileLock = new object();

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        private static string GetSettingsFilePath()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string folder = Path.Combine(appData, "NostromoGraphs");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            return Path.Combine(folder, "settings.json");
        }

        public static WidgetSettings Load()
        {
            lock (FileLock)
            {
                try
                {
                    string path = GetSettingsFilePath();
                    if (File.Exists(path))
                    {
                        string json = File.ReadAllText(path);
                        if (!string.IsNullOrWhiteSpace(json))
                        {
                            var settings = JsonSerializer.Deserialize<WidgetSettings>(json, JsonOptions);
                            if (settings != null)
                            {
                                // Clamp DisplayDurationSeconds to [10, 60]
                                settings.DisplayDurationSeconds = Math.Clamp(settings.DisplayDurationSeconds, 10.0, 60.0);
                                return settings;
                            }
                        }
                    }
                }
                catch { }

                return new WidgetSettings();
            }
        }

        public void Save()
        {
            lock (FileLock)
            {
                try
                {
                    string path = GetSettingsFilePath();
                    string tempPath = path + ".tmp";
                    string json = JsonSerializer.Serialize(this, JsonOptions);
                    File.WriteAllText(tempPath, json);
                    File.Move(tempPath, path, overwrite: true);
                }
                catch { }
            }
        }
    }
}

