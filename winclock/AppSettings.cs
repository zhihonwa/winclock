using System;
using System.Drawing;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Win32;
using System.Windows.Forms;

namespace winclock
{
    [XmlRoot("WinClockSettings")]
    public class AppSettings
    {
        public const string DefaultClockFontFamily = "Microsoft YaHei UI";
        public const string DefaultDateFontFamily = "Microsoft YaHei UI";

        public int TimeZoneOffsetHours { get; set; } = 8;
        public string ClockFontFamily { get; set; } = DefaultClockFontFamily;
        public float ClockFontSize { get; set; } = 35f;
        public bool ShowDate { get; set; } = true;
        public string DateFontFamily { get; set; } = DefaultDateFontFamily;
        public float DateFontSize { get; set; } = 12f;
        public bool ShowLunar { get; set; } = false;
        public bool TopMost { get; set; } = true;
        public bool EnableTransparency { get; set; } = true;
        public int OpacityPercent { get; set; } = 70;
        public bool DisableMinimize { get; set; } = false;
        public bool AutoStart { get; set; } = true;
        public bool RememberLastPosition { get; set; } = true;
        public int WindowX { get; set; }
        public int WindowY { get; set; }
        public string ScreenDeviceName { get; set; }

        public static AppSettings CreateDefault()
        {
            return new AppSettings();
        }

        public AppSettings Clone()
        {
            return (AppSettings)MemberwiseClone();
        }

        public void Normalize()
        {
            if (TimeZoneOffsetHours < -12 || TimeZoneOffsetHours > 14)
            {
                TimeZoneOffsetHours = 8;
            }

            ClockFontFamily = NormalizeFont(ClockFontFamily, DefaultClockFontFamily);
            DateFontFamily = NormalizeFont(DateFontFamily, DefaultDateFontFamily);
            ClockFontSize = NormalizeSize(ClockFontSize, 35f);
            DateFontSize = NormalizeSize(DateFontSize, 12f);

            if (OpacityPercent < 0)
            {
                OpacityPercent = 0;
            }
            else if (OpacityPercent > 100)
            {
                OpacityPercent = 100;
            }
        }

        private static string NormalizeFont(string family, string fallback)
        {
            return string.IsNullOrWhiteSpace(family) ? fallback : family.Trim();
        }

        private static float NormalizeSize(float size, float fallback)
        {
            if (float.IsNaN(size) || float.IsInfinity(size) || size < 6f)
            {
                return fallback;
            }

            return size > 160f ? 160f : size;
        }
    }

    public static class SettingsStore
    {
        private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string RunValueName = "WinClock";

        public static string SettingsDirectory
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "WinClock");
            }
        }

        public static string SettingsPath
        {
            get { return Path.Combine(SettingsDirectory, "settings.xml"); }
        }

        public static AppSettings Load()
        {
            try
            {
                if (!File.Exists(SettingsPath))
                {
                    return AppSettings.CreateDefault();
                }

                using (FileStream stream = File.OpenRead(SettingsPath))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(AppSettings));
                    AppSettings settings = serializer.Deserialize(stream) as AppSettings;
                    if (settings == null)
                    {
                        return AppSettings.CreateDefault();
                    }

                    settings.Normalize();
                    return settings;
                }
            }
            catch (Exception)
            {
                return AppSettings.CreateDefault();
            }
        }

        public static void Save(AppSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            settings.Normalize();
            Directory.CreateDirectory(SettingsDirectory);

            string tempPath = SettingsPath + ".tmp";
            XmlWriterSettings writerSettings = new XmlWriterSettings();
            writerSettings.Indent = true;
            writerSettings.Encoding = new System.Text.UTF8Encoding(false);

            XmlSerializer serializer = new XmlSerializer(typeof(AppSettings));
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            using (XmlWriter writer = XmlWriter.Create(tempPath, writerSettings))
            {
                serializer.Serialize(writer, settings, namespaces);
            }

            if (File.Exists(SettingsPath))
            {
                File.Replace(tempPath, SettingsPath, null);
            }
            else
            {
                File.Move(tempPath, SettingsPath);
            }
        }

        public static void ApplyAutoStart(bool enabled)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RunKeyPath))
            {
                if (key == null)
                {
                    return;
                }

                if (enabled)
                {
                    string executable = Application.ExecutablePath;
                    key.SetValue(RunValueName, "\"" + executable + "\"", RegistryValueKind.String);
                }
                else
                {
                    key.DeleteValue(RunValueName, false);
                }
            }
        }
    }

    public static class FontFactory
    {
        private static readonly string[] FallbackFamilies =
        {
            "Microsoft YaHei UI",
            "Microsoft YaHei",
            "Segoe UI",
            "Arial"
        };

        public static Font Create(string family, float size, FontStyle style)
        {
            Font font = TryCreate(family, size, style);
            if (font != null)
            {
                return font;
            }

            foreach (string fallback in FallbackFamilies)
            {
                font = TryCreate(fallback, size, style);
                if (font != null)
                {
                    return font;
                }
            }

            return new Font(SystemFonts.MessageBoxFont.FontFamily, size, style, GraphicsUnit.Point);
        }

        private static Font TryCreate(string family, float size, FontStyle style)
        {
            if (string.IsNullOrWhiteSpace(family))
            {
                return null;
            }

            try
            {
                Font font = new Font(family, size, style, GraphicsUnit.Point);
                bool matches = string.Equals(font.FontFamily.Name, family, StringComparison.OrdinalIgnoreCase);
                if (matches)
                {
                    return font;
                }

                font.Dispose();
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}