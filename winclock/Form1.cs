using System;
using System.Drawing;
using System.Windows.Forms;

namespace winclock
{
    public partial class Form1 : Form
    {
        private const string AppTitle = "WinClock";
        private const int ScreenMargin = 8;

        private NotifyIcon notifyIcon;
        private ContextMenuStrip trayMenu;
        private ContextMenuStrip clockMenu;
        private MenuBinding trayMenuBinding;
        private MenuBinding clockMenuBinding;
        private readonly Util util = new Util();
        private AppSettings _settings;
        private Font _clockFont;
        private Font _dateFont;
        private Color _textColor = Color.White;
        private Color _lastBackColor = Color.Empty;
        private int num;
        private Point mouseOffset;
        private bool _positioned;

        public Form1()
        {
            InitializeComponent();
            Text = AppTitle;
            ShowInTaskbar = false;
            DoubleBuffered = true;
            try
            {
                Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch (Exception)
            {
            }
            _settings = SettingsStore.Load();
            ApplySettings(_settings);
            InitializeMenus();
            WireInteractions(this);
        }

        private void InitializeMenus()
        {
            trayMenuBinding = CreateMenu();
            trayMenu = trayMenuBinding.Menu;

            clockMenuBinding = CreateMenu();
            clockMenu = clockMenuBinding.Menu;

            UpdateMenuChecks();
            AssignContextMenu(this, clockMenu);

            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = Icon ?? SystemIcons.Application;
            notifyIcon.Text = "WinClock 桌面时钟";
            notifyIcon.Visible = true;
            notifyIcon.ContextMenuStrip = trayMenu;
            notifyIcon.DoubleClick += delegate { ShowWindow(); };
        }

        private MenuBinding CreateMenu()
        {
            MenuBinding binding = new MenuBinding();

            ContextMenuStrip menu = new ContextMenuStrip();
            menu.AutoClose = true;
            menu.ShowImageMargin = true;
            menu.BackColor = UiTheme.Surface;
            menu.ForeColor = UiTheme.Text;
            menu.Font = FontFactory.Create(AppSettings.DefaultClockFontFamily, 9.5f, FontStyle.Regular);
            menu.Renderer = new DarkMenuRenderer();
            menu.Opening += delegate { UpdateMenuChecks(); };

            binding.Menu = menu;
            binding.AllowMoveItem = CreateToggleItem("允许移动", delegate { ToggleAllowMove(); });
            binding.TopMostItem = CreateToggleItem("始终置顶", delegate { ToggleTopMost(); });
            binding.HideSecondsItem = CreateToggleItem("隐藏秒数", delegate { ToggleHideSeconds(); });

            menu.Items.Add(binding.AllowMoveItem);
            menu.Items.Add(binding.TopMostItem);
            menu.Items.Add(binding.HideSecondsItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("放大", null, delegate { ScaleFonts(1.25f); });
            menu.Items.Add("缩小", null, delegate { ScaleFonts(0.75f); });
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("设置", null, delegate { OpenSettings(); });
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("退出", null, delegate { ExitApplication(); });

            return binding;
        }

        private static ToolStripMenuItem CreateToggleItem(string text, EventHandler onClick)
        {
            ToolStripMenuItem item = new ToolStripMenuItem(text, null, onClick);
            item.CheckOnClick = false;
            return item;
        }

        private static void AssignContextMenu(Control control, ContextMenuStrip menu)
        {
            control.ContextMenuStrip = menu;
            foreach (Control child in control.Controls)
            {
                AssignContextMenu(child, menu);
            }
        }

        private void WireInteractions(Control control)
        {
            control.MouseDown += Form1MouseDown;
            control.MouseMove += Form1MouseMove;
            control.MouseEnter += Form1MouseEnter;
            foreach (Control child in control.Controls)
            {
                WireInteractions(child);
            }
        }

        private void OpenSettings()
        {
            if (trayMenu != null)
            {
                trayMenu.Close();
            }

            if (clockMenu != null)
            {
                clockMenu.Close();
            }

            using (SettingsForm form = new SettingsForm(_settings.Clone(), ApplySettingsFromDialog))
            {
                form.Icon = Icon;
                form.TopMost = TopMost;
                form.ShowDialog(this);
            }
        }

        private bool ApplySettingsFromDialog(AppSettings settings)
        {
            if (settings != null && _settings != null)
            {
                settings.AllowMove = _settings.AllowMove;
                settings.HideSeconds = _settings.HideSeconds;
            }

            ApplySettings(settings);
            CaptureWindowPosition(_settings);
            bool success = true;

            try
            {
                SettingsStore.Save(_settings);
            }
            catch (Exception)
            {
                MessageBox.Show(
                    this,
                    "设置文件保存失败，请检查当前用户配置目录的写入权限。",
                    "WinClock",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                success = false;
            }

            try
            {
                SettingsStore.ApplyAutoStart(_settings.AutoStart);
            }
            catch (Exception)
            {
                MessageBox.Show(
                    this,
                    "自动启动设置写入失败，请检查当前用户启动项权限。",
                    "WinClock",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                success = false;
            }

            return success;
        }

        private void ApplySettings(AppSettings settings)
        {
            if (settings == null)
            {
                settings = AppSettings.CreateDefault();
            }

            settings.Normalize();
            _settings = settings;
            _textColor = TextColorPalette.Parse(settings.ClockTextColor);
            _lastBackColor = Color.Empty;

            int previousRight = _positioned ? Right : 0;
            Font previousClockFont = _clockFont;
            Font previousDateFont = _dateFont;

            _clockFont = FontFactory.Create(settings.ClockFontFamily, settings.ClockFontSize, FontStyle.Bold);
            _dateFont = FontFactory.Create(settings.DateFontFamily, settings.DateFontSize, FontStyle.Regular);

            timeDisplay.ApplyFont(_clockFont);
            timeDisplay.SetShowSeconds(!settings.HideSeconds);
            timeDisplay.SetForeColor(_textColor);
            label2.Font = _dateFont;
            label3.Font = _dateFont;
            label2.ForeColor = _textColor;
            label3.ForeColor = _textColor;
            timeDisplay.Visible = true;
            label2.Visible = settings.ShowDate;
            label3.Visible = settings.ShowLunar;

            TopMost = settings.TopMost;

            if (settings.DisableMinimize && WindowState == FormWindowState.Minimized)
            {
                WindowState = FormWindowState.Normal;
            }

            UpdateClockLayout();

            if (_positioned)
            {
                Screen screen = Screen.FromControl(this);
                int rightEdge = Math.Min(previousRight, screen.WorkingArea.Right - ScreenMargin);
                Left = Math.Max(screen.WorkingArea.Left, rightEdge - Width);
                if (Top < screen.WorkingArea.Top)
                {
                    Top = screen.WorkingArea.Top;
                }
                else if (Top + Height > screen.WorkingArea.Bottom)
                {
                    Top = Math.Max(screen.WorkingArea.Top, screen.WorkingArea.Bottom - Height);
                }
            }

            ApplyOpacity();

            if (previousClockFont != null)
            {
                previousClockFont.Dispose();
            }

            if (previousDateFont != null)
            {
                previousDateFont.Dispose();
            }

            if (IsHandleCreated)
            {
                LabelCronTick(null, null);
            }

            UpdateMenuChecks();
        }

        private void UpdateMenuChecks()
        {
            ApplyMenuState(trayMenuBinding);
            ApplyMenuState(clockMenuBinding);
        }

        private void ApplyMenuState(MenuBinding binding)
        {
            if (binding == null)
            {
                return;
            }

            binding.AllowMoveItem.Checked = _settings != null && _settings.AllowMove;
            binding.TopMostItem.Checked = _settings != null && _settings.TopMost;
            binding.HideSecondsItem.Checked = _settings != null && _settings.HideSeconds;
        }

        private void ToggleAllowMove()
        {
            if (_settings == null)
            {
                return;
            }

            _settings.AllowMove = !_settings.AllowMove;
            mouseOffset = Point.Empty;
            UpdateMenuChecks();
            SaveMenuSettings();
        }

        private void ToggleTopMost()
        {
            if (_settings == null)
            {
                return;
            }

            _settings.TopMost = !_settings.TopMost;
            TopMost = _settings.TopMost;
            UpdateMenuChecks();
            SaveMenuSettings();
        }

        private void ToggleHideSeconds()
        {
            if (_settings == null)
            {
                return;
            }

            _settings.HideSeconds = !_settings.HideSeconds;
            ApplySettings(_settings);
            SaveMenuSettings();
        }

        private void ScaleFonts(float factor)
        {
            if (_settings == null)
            {
                return;
            }

            _settings.ClockFontSize = ScaleFontSize(_settings.ClockFontSize, factor);
            _settings.DateFontSize = ScaleFontSize(_settings.DateFontSize, factor);
            ApplySettings(_settings);
            SaveMenuSettings();
        }

        private static float ScaleFontSize(float size, float factor)
        {
            float scaled = (float)Math.Round(size * factor, 1, MidpointRounding.AwayFromZero);
            return Math.Max(6f, Math.Min(160f, scaled));
        }

        private void SaveMenuSettings()
        {
            if (_settings == null)
            {
                return;
            }

            CaptureWindowPosition(_settings);
            try
            {
                SettingsStore.Save(_settings);
            }
            catch (Exception)
            {
            }
        }

        private void UpdateClockLayout()
        {
            int timeHeight = Math.Max(timeDisplay.RequiredHeight, _clockFont.Height + 6);
            int width = timeDisplay.RequiredWidth;

            string dateSample = _settings.HideSeconds ? "00-00 星期天" : "0000年00月00日 星期天";
            string lunarSample = _settings.HideSeconds ? "闰正月初十" : "农历甲乙（鼠）0000年闰正月初十";

            int dateHeight = Math.Max(_dateFont.Height + 6, TextRenderer.MeasureText(dateSample, _dateFont).Height + 4);
            if (_settings.ShowDate)
            {
                width = Math.Max(width, TextRenderer.MeasureText(dateSample, _dateFont).Width);
            }

            if (_settings.ShowLunar)
            {
                width = Math.Max(width, TextRenderer.MeasureText(lunarSample, _dateFont).Width);
            }

            width = Math.Max(240, width + 24);
            Screen screen = Screen.FromControl(this);
            int maxWidth = Math.Max(240, screen.WorkingArea.Width - ScreenMargin * 2);
            if (width > maxWidth)
            {
                width = maxWidth;
            }

            int height = timeHeight;
            if (_settings.ShowDate)
            {
                height += dateHeight;
            }

            if (_settings.ShowLunar)
            {
                height += dateHeight + 4;
            }

            tableLayoutPanel1.RowStyles.Clear();
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, timeHeight));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, _settings.ShowDate ? dateHeight : 0));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, _settings.ShowLunar ? dateHeight + 4 : 0));

            ClientSize = new Size(width, height);
            tableLayoutPanel1.PerformLayout();
        }

        private void ApplyOpacity()
        {
            if (!_settings.EnableTransparency)
            {
                Opacity = 1d;
                return;
            }

            double value = Math.Max(0, Math.Min(100, _settings.OpacityPercent)) / 100d;
            Opacity = value;
        }

        private void ShowWindow()
        {
            ShowInTaskbar = true;
            WindowState = FormWindowState.Normal;
            notifyIcon.Visible = true;
            trayMenu.Close();
            Util.RefreshNotificationArea();
            Show();
        }

        private void ExitApplication()
        {
            trayMenu.Close();
            clockMenu.Close();
            notifyIcon.Visible = false;
            notifyIcon.Dispose();

            if (_clockFont != null)
            {
                _clockFont.Dispose();
            }

            if (_dateFont != null)
            {
                _dateFont.Dispose();
            }

            Application.Exit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                SettingsStore.ApplyAutoStart(_settings.AutoStart);
            }
            catch (Exception)
            {
            }

            RestoreWindowPosition();
            UpdateClockLayout();
            ClampWindowToWorkingArea();
            LabelCronTick(null, null);
        }

        private void RestoreWindowPosition()
        {
            if (_settings != null &&
                _settings.RememberLastPosition &&
                !string.IsNullOrEmpty(_settings.ScreenDeviceName))
            {
                Point saved = new Point(_settings.WindowX, _settings.WindowY);
                Screen screen = FindScreen(_settings.ScreenDeviceName, saved);
                if (screen != null)
                {
                    Location = ClampToWorkingArea(screen, saved, Size);
                    _positioned = true;
                    return;
                }
            }

            Screen primary = Screen.PrimaryScreen;
            Left = primary.WorkingArea.Right - Width - ScreenMargin;
            Top = primary.WorkingArea.Top + ScreenMargin;
            _positioned = true;
        }

        private static Screen FindScreen(string deviceName, Point location)
        {
            foreach (Screen screen in Screen.AllScreens)
            {
                if (string.Equals(screen.DeviceName, deviceName, StringComparison.OrdinalIgnoreCase))
                {
                    return screen;
                }
            }

            return Screen.FromPoint(location);
        }

        private void ClampWindowToWorkingArea()
        {
            Point location = ClampToWorkingArea(Screen.FromControl(this), Location, Size);
            if (location != Location)
            {
                Location = location;
            }
        }

        private static Point ClampToWorkingArea(Screen screen, Point location, Size size)
        {
            Rectangle area = screen.WorkingArea;
            int left = Math.Max(area.Left, Math.Min(location.X, area.Right - size.Width));
            int top = Math.Max(area.Top, Math.Min(location.Y, area.Bottom - size.Height));
            return new Point(left, top);
        }

        private void CaptureWindowPosition(AppSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            Rectangle bounds = WindowState == FormWindowState.Minimized ? RestoreBounds : Bounds;
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            settings.WindowX = bounds.X;
            settings.WindowY = bounds.Y;
            settings.ScreenDeviceName = Screen.FromRectangle(bounds).DeviceName;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_settings != null)
            {
                CaptureWindowPosition(_settings);
                try
                {
                    SettingsStore.Save(_settings);
                }
                catch (Exception)
                {
                }
            }

            base.OnFormClosing(e);
        }

        private static readonly string[] WeekDays =
        {
            "星期天", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六"
        };

        private void LabelCronTick(object sender, EventArgs e)
        {
            DateTime now = DateTime.UtcNow.AddHours(_settings.TimeZoneOffsetHours);

            timeDisplay.SetTime(now);

            if (_settings.ShowDate)
            {
                string dateText = FormatDateText(now);
                if (label2.Text != dateText)
                {
                    label2.Text = dateText;
                }
            }

            if (_settings.ShowLunar)
            {
                string lunarText = _settings.HideSeconds
                    ? util.SolarToChineseLunisolarShortDate(now)
                    : util.SolarToChineseLunisolarDate(now);
                if (label3.Text != lunarText)
                {
                    label3.Text = lunarText;
                }
            }

            RefreshTransparentBackColor();
        }

        private string FormatDateText(DateTime now)
        {
            string week = WeekDays[(int)now.DayOfWeek];
            if (_settings.HideSeconds)
            {
                return now.ToString("MM-dd") + " " + week;
            }

            return now.ToString("yyyy年MM月dd日") + " " + week;
        }

        private void RefreshTransparentBackColor()
        {
            if (WindowState != FormWindowState.Normal)
            {
                return;
            }

            Point sample = timeDisplay.GetColorSamplePoint();
            Color backColor = EnsureTransparentKey(util.GetScreenColor(sample.X, sample.Y), _textColor);
            if (backColor == _lastBackColor)
            {
                return;
            }

            _lastBackColor = backColor;

            timeDisplay.SetBackColor(backColor);
            label2.BackColor = backColor;
            label3.BackColor = backColor;
            BackColor = backColor;
            TransparencyKey = backColor;
        }

        private static Color EnsureTransparentKey(Color background, Color textColor)
        {
            if (background.ToArgb() != textColor.ToArgb())
            {
                return background;
            }

            int r = background.R;
            int g = background.G;
            int b = background.B;
            if (r < 255)
            {
                r++;
            }
            else if (g < 255)
            {
                g++;
            }
            else if (b < 255)
            {
                b++;
            }
            else
            {
                r = 254;
                g = 254;
                b = 254;
            }

            return Color.FromArgb(r, g, b);
        }

        private void Form1MouseEnter(object sender, EventArgs e)
        {
            if (!_settings.EnableTransparency)
            {
                Opacity = 1d;
                return;
            }

            Opacity = 1d;
            mouseEnterTimer.Enabled = true;
        }

        public void MouseEnterTick(object sender, EventArgs e)
        {
            if (num <= 2)
            {
                num++;
                return;
            }

            num = 0;
            mouseEnterTimer.Enabled = false;
            ApplyOpacity();
        }

        private void Form1MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && _settings != null && _settings.AllowMove)
            {
                mouseOffset = new Point(-e.X, -e.Y);
            }
        }

        private void Form1MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && _settings != null && _settings.AllowMove)
            {
                Point mousePos = Control.MousePosition;
                mousePos.Offset(mouseOffset.X, mouseOffset.Y);
                Location = mousePos;
            }
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_MINIMIZE = 0xF020;

            if (_settings != null &&
                _settings.DisableMinimize &&
                m.Msg == WM_SYSCOMMAND &&
                (m.WParam.ToInt64() & 0xFFF0) == SC_MINIMIZE)
            {
                return;
            }

            base.WndProc(ref m);
        }

        private sealed class MenuBinding
        {
            public ContextMenuStrip Menu { get; set; }

            public ToolStripMenuItem AllowMoveItem { get; set; }

            public ToolStripMenuItem TopMostItem { get; set; }

            public ToolStripMenuItem HideSecondsItem { get; set; }
        }
    }
}
