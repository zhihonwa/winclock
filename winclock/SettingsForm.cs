using System;
using System.Collections.Generic;
using System.Drawing;

using System.Drawing.Text;
using System.Windows.Forms;

namespace winclock
{
    public sealed class SettingsForm : Form
    {
        private const int HeaderHeight = 50;
        private const int FooterHeight = 70;

        private readonly Func<AppSettings, bool> _apply;
        private readonly List<string> _fontNames = new List<string>();
        private readonly ToolTip _toolTip = new ToolTip();

        private ComboBox _timeZoneBox;
        private DarkComboBox _clockFontBox;
        private DarkComboBox _dateFontBox;
        private NumericUpDown _clockSize;
        private NumericUpDown _dateSize;
        private DarkTextBox _textColorInput;
        private readonly List<ColorSwatch> _colorSwatches = new List<ColorSwatch>();
        private ToggleSwitch _showDateToggle;
        private ToggleSwitch _showLunarToggle;
        private ToggleSwitch _topMostToggle;
        private ToggleSwitch _transparencyToggle;
        private ToggleSwitch _minimizeToggle;
        private ToggleSwitch _autoStartToggle;
        private ToggleSwitch _rememberPositionToggle;
        private ModernSlider _opacitySlider;
        private Label _opacityValue;
        private Label _statusLabel;
        private Panel _contentHost;
        private CardPanel _timeCard;
        private CardPanel _dateCard;
        private CardPanel _appearanceCard;
        private CardPanel _behaviorCard;
        private bool _contentReady;

        public SettingsForm(AppSettings settings, Func<AppSettings, bool> apply)
        {
            if (settings == null)
            {
                settings = AppSettings.CreateDefault();
            }

            _apply = apply;
            BuildUi();
            PopulateFonts();
            PopulateTimeZones();
            LoadValues(settings);

        }

        private void BuildUi()
        {
            Text = "WinClock 设置";
            Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
            int width = Math.Min(760, Math.Max(680, workingArea.Width - 48));
            int height = Math.Min(620, Math.Max(600, workingArea.Height - 56));
            ClientSize = new Size(width, height);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ShowInTaskbar = true;
            BackColor = UiTheme.Background;
            ForeColor = UiTheme.Text;
            Font = UiTheme.CreateFont(9.5f, FontStyle.Regular);
            AutoScaleMode = AutoScaleMode.Font;
            DoubleBuffered = true;

            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.ColumnCount = 1;
            root.RowCount = 3;
            root.BackColor = UiTheme.Background;
            root.Margin = new Padding(0);
            root.Padding = new Padding(0);
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, HeaderHeight));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, FooterHeight));

            HeaderPanel header = new HeaderPanel();
            header.Dock = DockStyle.Fill;
            header.Margin = new Padding(0);
            BuildHeader(header);
            root.Controls.Add(header, 0, 0);

            _contentHost = new Panel();
            _contentHost.Dock = DockStyle.Fill;
            _contentHost.BackColor = UiTheme.Background;
            _contentHost.AutoScroll = true;
            _contentHost.Padding = new Padding(0);
            _contentHost.Margin = new Padding(0);
            _contentHost.Resize += delegate { LayoutCards(); };
            root.Controls.Add(_contentHost, 0, 1);

            Panel footer = BuildFooter();
            root.Controls.Add(footer, 0, 2);

            Controls.Add(root);

            BuildCards();
            _contentReady = true;
            LayoutCards();
        }

        private void BuildHeader(HeaderPanel header)
        {
            Label title = new Label();
            title.Text = "WinClock 设置";
            title.Font = UiTheme.CreateFont(16f, FontStyle.Bold);
            title.ForeColor = UiTheme.Text;
            title.BackColor = Color.Transparent;
            title.AutoSize = true;
            title.Location = new Point(42, 11);
            header.Controls.Add(title);

            Panel versionHost = new Panel();
            versionHost.Dock = DockStyle.Right;
            versionHost.Width = 116;
            versionHost.BackColor = Color.Transparent;
            header.Controls.Add(versionHost);

            Label version = new Label();
            version.Text = "v" + typeof(SettingsForm).Assembly.GetName().Version.ToString(3);
            version.Dock = DockStyle.Fill;
            version.TextAlign = ContentAlignment.MiddleRight;
            version.Padding = new Padding(0, 0, 24, 0);
            version.Font = UiTheme.CreateFont(9f, FontStyle.Regular);
            version.ForeColor = UiTheme.Muted;
            version.BackColor = Color.Transparent;
            versionHost.Controls.Add(version);
        }

        private Panel BuildFooter()
        {
            Panel footer = new Panel();
            footer.Dock = DockStyle.Fill;
            footer.BackColor = UiTheme.Background;
            footer.Margin = new Padding(0);
            footer.Paint += delegate(object sender, PaintEventArgs e)
            {
                using (Pen pen = new Pen(UiTheme.Border))
                {
                    e.Graphics.DrawLine(pen, 24, 0, footer.Width - 24, 0);
                }
            };

            _statusLabel = new Label();
            _statusLabel.AutoSize = false;
            _statusLabel.Location = new Point(24, 24);
            _statusLabel.Size = new Size(392, 22);
            _statusLabel.AutoEllipsis = true;
            _statusLabel.Text = "配置文件：" + SettingsStore.SettingsPath;
            _statusLabel.Font = UiTheme.CreateFont(8.5f, FontStyle.Regular);
            _statusLabel.ForeColor = UiTheme.MutedStrong;
            _statusLabel.BackColor = Color.Transparent;
            _statusLabel.AccessibleRole = AccessibleRole.StatusBar;
            footer.Controls.Add(_statusLabel);

            FlowLayoutPanel actions = new FlowLayoutPanel();
            actions.Dock = DockStyle.Right;
            actions.Width = 330;
            actions.FlowDirection = FlowDirection.RightToLeft;
            actions.WrapContents = false;
            actions.BackColor = Color.Transparent;
            actions.Padding = new Padding(0, 14, 24, 0);
            footer.Controls.Add(actions);

            ModernButton ok = new ModernButton();
            ok.Text = "确定";
            ok.Primary = true;
            ok.Width = 96;
            ok.Margin = new Padding(8, 0, 0, 0);
            ok.AccessibleName = "保存设置并关闭";
            ok.Click += delegate { Commit(true); };
            actions.Controls.Add(ok);
            AcceptButton = ok;

            ModernButton apply = new ModernButton();
            apply.Text = "应用";
            apply.Width = 96;
            apply.Margin = new Padding(8, 0, 0, 0);
            apply.AccessibleName = "保存并应用设置";
            apply.Click += delegate { Commit(false); };
            actions.Controls.Add(apply);

            ModernButton cancel = new ModernButton();
            cancel.Text = "取消";
            cancel.Width = 96;
            cancel.Margin = new Padding(8, 0, 0, 0);
            cancel.AccessibleName = "取消并关闭";
            cancel.DialogResult = DialogResult.Cancel;
            actions.Controls.Add(cancel);
            CancelButton = cancel;

            return footer;
        }

        private void BuildCards()
        {
            TableLayoutPanel body;

            _timeCard = CreateCard("时间与字体", out body);
            AddToBody(body, CreateField("时区", BuildTimeZoneBox()), 8);
            AddToBody(body, CreateField("时间字体", BuildFontBox(true)), 8);
            AddToBody(body, CreateField("时间字号（磅）", BuildSizeBox(true)), 0);

            _appearanceCard = CreateCard("外观", out body);
            AddToBody(body, CreateColorField(), 8);
            _topMostToggle = BuildToggle("始终置顶");
            _topMostToggle.AccessibleName = "始终置顶";
            AddToBody(body, CreateToggleRow("始终置顶", _topMostToggle), 8);

            _transparencyToggle = BuildToggle("透明效果");
            _transparencyToggle.AccessibleName = "启用透明效果";
            AddToBody(body, CreateToggleRow("透明效果", _transparencyToggle), 8);
            AddToBody(body, CreateOpacityField(), 6);

            Label opacityHint = BuildHint("0% 时窗口完全透明，可从系统托盘的“设置”恢复。", 300);
            AddToBody(body, opacityHint, 0);

            _dateCard = CreateCard("日期与农历", out body);
            _showDateToggle = BuildToggle("显示日期与星期");
            _showDateToggle.AccessibleName = "显示日期与星期";
            AddToBody(body, CreateToggleRow("显示日期与星期", _showDateToggle), 8);
            AddToBody(body, CreateField("日期/星期字体", BuildFontBox(false)), 8);
            AddToBody(body, CreateField("日期/星期字号（磅）", BuildSizeBox(false)), 8);
            _showLunarToggle = BuildToggle("显示农历日期");
            _showLunarToggle.AccessibleName = "显示农历日期";
            AddToBody(body, CreateToggleRow("显示农历日期", _showLunarToggle), 0);

            _behaviorCard = CreateCard("行为", out body);
            _minimizeToggle = BuildToggle("禁止窗口最小化");
            _minimizeToggle.AccessibleName = "禁止窗口最小化";
            AddToBody(body, CreateToggleRow("禁止窗口最小化", _minimizeToggle), 8);
            _autoStartToggle = BuildToggle("随操作系统自动启动");
            _autoStartToggle.AccessibleName = "随操作系统自动启动";
            AddToBody(body, CreateToggleRow("随操作系统自动启动", _autoStartToggle), 8);
            _rememberPositionToggle = BuildToggle("记住最后位置");
            _rememberPositionToggle.AccessibleName = "记住最后位置";
            AddToBody(body, CreateToggleRow("记住最后位置", _rememberPositionToggle), 0);

            _contentHost.Controls.Add(_timeCard);
            _contentHost.Controls.Add(_dateCard);
            _contentHost.Controls.Add(_appearanceCard);
            _contentHost.Controls.Add(_behaviorCard);
        }

        private CardPanel CreateCard(string title, out TableLayoutPanel body)
        {
            CardPanel card = new CardPanel();
            card.Dock = DockStyle.None;
            card.AutoSize = false;

            body = new TableLayoutPanel();
            body.Dock = DockStyle.Top;
            body.AutoSize = true;
            body.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            body.ColumnCount = 1;
            body.BackColor = Color.Transparent;
            body.Padding = new Padding(16, 8, 16, 10);
            body.Margin = new Padding(0);
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            card.Controls.Add(body);

            Label heading = new Label();
            heading.Text = title;
            heading.Font = UiTheme.CreateFont(12f, FontStyle.Bold);
            heading.ForeColor = UiTheme.Text;
            heading.BackColor = Color.Transparent;
            heading.AutoSize = true;
            AddToBody(body, heading, 8);

            return card;
        }

        private ComboBox BuildTimeZoneBox()
        {
            _timeZoneBox = new DarkComboBox();
            _timeZoneBox.AccessibleName = "时钟显示时区";
            _timeZoneBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _toolTip.SetToolTip(_timeZoneBox, "选择时钟显示所使用的时间偏移，默认 UTC+8。");
            return _timeZoneBox;
        }

        private Control BuildFontBox(bool clockFont)
        {
            DarkComboBox box = new DarkComboBox();
            box.DropDownStyle = ComboBoxStyle.DropDownList;
            if (clockFont)
            {
                _clockFontBox = box;
                box.AccessibleName = "时间字体";
                _toolTip.SetToolTip(box, "选择时钟时间使用的字体。");
            }
            else
            {
                _dateFontBox = box;
                box.AccessibleName = "日期与星期字体";
                _toolTip.SetToolTip(box, "选择日期、星期与农历使用的字体。");
            }


            return box;
        }

        private Control BuildSizeBox(bool clockSize)
        {
            NumericUpDown input = new NumericUpDown();
            input.Minimum = 6m;
            input.Maximum = 160m;
            input.DecimalPlaces = 1;
            input.Increment = 1m;
            input.TextAlign = HorizontalAlignment.Right;
            input.BorderStyle = BorderStyle.FixedSingle;
            input.BackColor = UiTheme.SurfaceAlt;
            input.ForeColor = UiTheme.Text;
            input.Font = UiTheme.CreateFont(9f, FontStyle.Regular);
            input.Height = 30;

            if (clockSize)
            {
                _clockSize = input;
                input.AccessibleName = "时间字号";
                _toolTip.SetToolTip(input, "时间字体大小，单位为磅。");
            }
            else
            {
                _dateSize = input;
                input.AccessibleName = "日期与星期字号";
                _toolTip.SetToolTip(input, "日期与星期字体大小，单位为磅。农历跟随此字号。");
            }


            return input;
        }

        private Control CreateColorField()
        {
            TableLayoutPanel row = new TableLayoutPanel();
            row.Dock = DockStyle.Top;
            row.AutoSize = true;
            row.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            row.ColumnCount = 2;
            row.RowCount = 1;
            row.BackColor = Color.Transparent;
            row.Margin = new Padding(0);
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 104f));
            row.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            FlowLayoutPanel swatches = new FlowLayoutPanel();
            swatches.AutoSize = true;
            swatches.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            swatches.WrapContents = false;
            swatches.BackColor = Color.Transparent;
            swatches.Margin = new Padding(0);
            row.Controls.Add(swatches, 0, 0);

            foreach (TextColorPreset preset in TextColorPalette.Presets)
            {
                ColorSwatch swatch = new ColorSwatch();
                swatch.Value = TextColorPalette.Parse(preset.Hex);
                swatch.Size = new Size(26, 26);
                swatch.Margin = new Padding(0, 1, 6, 1);
                swatch.Tag = preset;
                swatch.AccessibleName = preset.Name + " " + preset.Hex;
                _toolTip.SetToolTip(swatch, preset.Name + " " + preset.Hex);
                swatch.Click += delegate(object sender, EventArgs e)
                {
                    TextColorPreset selected = (TextColorPreset)((ColorSwatch)sender).Tag;
                    _textColorInput.Text = selected.Hex;
                    UpdateColorSelection(selected.Hex);
                };
                _colorSwatches.Add(swatch);
                swatches.Controls.Add(swatch);
            }

            _textColorInput = new DarkTextBox();
            _textColorInput.Width = 100;
            _textColorInput.Anchor = AnchorStyles.Left;
            _textColorInput.Margin = new Padding(4, 3, 0, 0);
            _textColorInput.AccessibleName = "时钟文字颜色十六进制值";
            _toolTip.SetToolTip(_textColorInput, "可输入 #RRGGBB 形式的十六进制颜色，例如 #FFFFFF。");
            _textColorInput.TextChanged += delegate { UpdateColorSelection(_textColorInput.Text); };
            row.Controls.Add(_textColorInput, 1, 0);

            return CreateField("时钟文字颜色", row);
        }

        private void UpdateColorSelection(string text)
        {
            Color color;
            bool valid = TextColorPalette.TryParse(text, out color);
            foreach (ColorSwatch swatch in _colorSwatches)
            {
                swatch.Selected = valid && swatch.Value.ToArgb() == color.ToArgb();
            }
        }

        private Control CreateOpacityField()
        {
            TableLayoutPanel row = new TableLayoutPanel();
            row.Dock = DockStyle.Top;
            row.AutoSize = true;
            row.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            row.ColumnCount = 3;
            row.RowCount = 2;
            row.BackColor = Color.Transparent;
            row.Margin = new Padding(0);
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 56f));
            row.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            row.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            Label caption = new Label();
            caption.Text = "透明度";
            caption.Font = UiTheme.CreateFont(8.5f, FontStyle.Regular);
            caption.ForeColor = UiTheme.Muted;
            caption.BackColor = Color.Transparent;
            caption.AutoSize = true;
            caption.Margin = new Padding(0, 0, 0, 2);
            row.Controls.Add(caption, 0, 0);
            row.SetColumnSpan(caption, 2);

            _opacitySlider = new ModernSlider();
            _opacitySlider.Minimum = 0;
            _opacitySlider.Maximum = 100;
            _opacitySlider.Dock = DockStyle.Fill;
            _opacitySlider.Margin = new Padding(0, 0, 10, 0);
            _opacitySlider.AccessibleName = "窗口透明度百分比";
            row.Controls.Add(_opacitySlider, 0, 1);

            _opacityValue = new Label();
            _opacityValue.Text = "70%";
            _opacityValue.Font = UiTheme.CreateFont(9f, FontStyle.Bold);
            _opacityValue.ForeColor = UiTheme.Text;
            _opacityValue.BackColor = Color.Transparent;
            _opacityValue.TextAlign = ContentAlignment.MiddleRight;
            _opacityValue.Anchor = AnchorStyles.Right | AnchorStyles.Left;
            _opacityValue.AutoSize = true;
            row.Controls.Add(_opacityValue, 1, 1);

            _opacitySlider.ValueChanged += delegate
            {
                _opacityValue.Text = _opacitySlider.Value + "%";

            };

            return row;
        }

        private static ToggleSwitch BuildToggle(string accessibleName)
        {
            ToggleSwitch toggle = new ToggleSwitch();
            toggle.AccessibleName = accessibleName;
            return toggle;
        }

        private Control CreateToggleRow(string text, ToggleSwitch toggle)
        {
            TableLayoutPanel row = new TableLayoutPanel();
            row.Dock = DockStyle.Top;
            row.AutoSize = true;
            row.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            row.ColumnCount = 2;
            row.RowCount = 1;
            row.BackColor = Color.Transparent;
            row.Margin = new Padding(0);
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 51f));
            row.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            Label caption = new Label();
            caption.Text = text;
            caption.Font = UiTheme.CreateFont(9.5f, FontStyle.Regular);
            caption.ForeColor = UiTheme.Text;
            caption.BackColor = Color.Transparent;
            caption.AutoSize = true;
            caption.Anchor = AnchorStyles.Left;
            caption.Margin = new Padding(0, 3, 8, 0);
            caption.Cursor = Cursors.Hand;
            caption.Click += delegate { toggle.Checked = !toggle.Checked; };
            row.Controls.Add(caption, 0, 0);

            toggle.Anchor = AnchorStyles.Right;
            toggle.Margin = new Padding(0, 0, 0, 0);
            row.Controls.Add(toggle, 1, 0);

            return row;
        }

        private static Label BuildHint(string text, int width)
        {
            Label hint = new Label();
            hint.Text = text;
            hint.Font = UiTheme.CreateFont(8.5f, FontStyle.Regular);
            hint.ForeColor = UiTheme.MutedStrong;
            hint.BackColor = Color.Transparent;
            hint.AutoSize = true;
            hint.MaximumSize = new Size(width, 0);
            hint.Margin = new Padding(0, 2, 0, 0);
            return hint;
        }

        private Control CreateField(string labelText, Control input)
        {
            TableLayoutPanel field = new TableLayoutPanel();
            field.Dock = DockStyle.Top;
            field.AutoSize = true;
            field.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            field.ColumnCount = 1;
            field.BackColor = Color.Transparent;
            field.Margin = new Padding(0);
            field.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            Label caption = new Label();
            caption.Text = labelText;
            caption.Font = UiTheme.CreateFont(8.5f, FontStyle.Regular);
            caption.ForeColor = UiTheme.MutedStrong;
            caption.BackColor = Color.Transparent;
            caption.AutoSize = true;
            AddToBody(field, caption, 4);

            input.Dock = DockStyle.Top;
            input.Margin = new Padding(0);
            AddToBody(field, input, 0);

            return field;
        }

        private static void AddToBody(TableLayoutPanel body, Control control, int bottomMargin)
        {
            int row = body.RowStyles.Count;
            body.RowCount = row + 1;
            body.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            control.Dock = DockStyle.Top;
            control.Margin = new Padding(0, 0, 0, bottomMargin);
            body.Controls.Add(control, 0, row);
        }

        private void LayoutCards()
        {
            if (!_contentReady || _contentHost == null)
            {
                return;
            }

            int available = _contentHost.ClientSize.Width - 48;
            if (available < 420)
            {
                available = 420;
            }

            int gap = 16;
            int cardWidth = (available - gap) / 2;
            int left = 12;
            int top = 2;

            int timeHeight = LayoutCard(_timeCard, left, top, cardWidth);
            int appearanceHeight = LayoutCard(_appearanceCard, left + cardWidth + gap, top, cardWidth);
            int firstRowHeight = Math.Max(timeHeight, appearanceHeight);
            SetCardHeight(_timeCard, firstRowHeight);
            SetCardHeight(_appearanceCard, firstRowHeight);

            int secondTop = top + firstRowHeight + 12;
            int dateHeight = LayoutCard(_dateCard, left, secondTop, cardWidth);
            int behaviorHeight = LayoutCard(_behaviorCard, left + cardWidth + gap, secondTop, cardWidth);
            int secondRowHeight = Math.Max(dateHeight, behaviorHeight);
            SetCardHeight(_dateCard, secondRowHeight);
            SetCardHeight(_behaviorCard, secondRowHeight);

            int totalHeight = secondTop + secondRowHeight + 12;
            _contentHost.AutoScrollMinSize = new Size(0, totalHeight);
            FitWindowHeight(totalHeight);
        }

        private void FitWindowHeight(int contentHeight)
        {
            int desired = HeaderHeight + contentHeight + FooterHeight + 2;
            Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
            int chrome = Height - ClientSize.Height;
            int maximum = Math.Max(HeaderHeight + FooterHeight + 120, workingArea.Height - chrome - 16);
            desired = Math.Min(desired, maximum);
            if (desired != ClientSize.Height)
            {
                ClientSize = new Size(ClientSize.Width, desired);
            }
        }

        private int LayoutCard(CardPanel card, int x, int y, int width)
        {
            if (card == null)
            {
                return 0;
            }

            card.SetBounds(x, y, width, 10);
            card.PerformLayout();
            int height = card.GetPreferredSize(new Size(width, 0)).Height;
            card.Height = Math.Max(40, height);
            card.PerformLayout();
            return card.Height;
        }

        private static void SetCardHeight(CardPanel card, int height)
        {
            if (card != null && card.Height != height)
            {
                card.Height = height;
            }
        }


        private void PopulateFonts()
        {
            _fontNames.Clear();
            try
            {
                using (InstalledFontCollection collection = new InstalledFontCollection())
                {
                    foreach (FontFamily family in collection.Families)
                    {
                        _fontNames.Add(family.Name);
                    }
                }
            }
            catch (Exception)
            {
            }

            if (_fontNames.Count == 0)
            {
                _fontNames.Add(AppSettings.DefaultClockFontFamily);
                _fontNames.Add("Segoe UI");
                _fontNames.Add("Arial");
            }

            _fontNames.Sort(StringComparer.CurrentCultureIgnoreCase);
        }

        private void PopulateTimeZones()
        {
            for (int offset = -12; offset <= 14; offset++)
            {
                _timeZoneBox.Items.Add(new TimeZoneOption(offset));
            }
        }

        private void LoadValues(AppSettings settings)
        {

            SelectTimeZone(settings.TimeZoneOffsetHours);
            FillFontBox(_clockFontBox, settings.ClockFontFamily);
            FillFontBox(_dateFontBox, settings.DateFontFamily);
            _clockSize.Value = (decimal)settings.ClockFontSize;
            _dateSize.Value = (decimal)settings.DateFontSize;
            _textColorInput.Text = settings.ClockTextColor;
            UpdateColorSelection(_textColorInput.Text);
            _showDateToggle.Checked = settings.ShowDate;
            _showLunarToggle.Checked = settings.ShowLunar;
            _topMostToggle.Checked = settings.TopMost;
            _transparencyToggle.Checked = settings.EnableTransparency;
            _opacitySlider.Value = settings.OpacityPercent;
            _opacityValue.Text = settings.OpacityPercent + "%";
            _minimizeToggle.Checked = settings.DisableMinimize;
            _autoStartToggle.Checked = settings.AutoStart;
            _rememberPositionToggle.Checked = settings.RememberLastPosition;

            _opacitySlider.Enabled = settings.EnableTransparency;
            _opacityValue.ForeColor = settings.EnableTransparency ? UiTheme.Text : UiTheme.Muted;

            _transparencyToggle.CheckedChanged += delegate
            {
                bool enabled = _transparencyToggle.Checked;
                _opacitySlider.Enabled = enabled;
                _opacityValue.ForeColor = enabled ? UiTheme.Text : UiTheme.Muted;
            };
        }

        private void SelectTimeZone(int offset)
        {
            for (int i = 0; i < _timeZoneBox.Items.Count; i++)
            {
                TimeZoneOption option = _timeZoneBox.Items[i] as TimeZoneOption;
                if (option != null && option.Offset == offset)
                {
                    _timeZoneBox.SelectedIndex = i;
                    return;
                }
            }

            _timeZoneBox.SelectedIndex = 20;
        }

        private void FillFontBox(ComboBox box, string selected)
        {
            box.Items.Clear();
            foreach (string name in _fontNames)
            {
                box.Items.Add(name);
            }

            int index = -1;
            for (int i = 0; i < box.Items.Count; i++)
            {
                string name = box.Items[i] as string;
                if (string.Equals(name, selected, StringComparison.OrdinalIgnoreCase))
                {
                    index = i;
                    break;
                }
            }

            if (index < 0)
            {
                box.Items.Add(selected);
                index = box.Items.Count - 1;
            }

            box.SelectedIndex = index;
        }

        private AppSettings ReadSettings()
        {
            AppSettings settings = new AppSettings();
            TimeZoneOption zone = _timeZoneBox.SelectedItem as TimeZoneOption;
            settings.TimeZoneOffsetHours = zone != null ? zone.Offset : 8;
            settings.ClockFontFamily = _clockFontBox.SelectedItem as string;
            settings.ClockFontSize = (float)_clockSize.Value;
            settings.ClockTextColor = TextColorPalette.Normalize(_textColorInput.Text);
            settings.ShowDate = _showDateToggle.Checked;
            settings.DateFontFamily = _dateFontBox.SelectedItem as string;
            settings.DateFontSize = (float)_dateSize.Value;
            settings.ShowLunar = _showLunarToggle.Checked;
            settings.TopMost = _topMostToggle.Checked;
            settings.EnableTransparency = _transparencyToggle.Checked;
            settings.OpacityPercent = _opacitySlider.Value;
            settings.DisableMinimize = _minimizeToggle.Checked;
            settings.AutoStart = _autoStartToggle.Checked;
            settings.RememberLastPosition = _rememberPositionToggle.Checked;
            settings.Normalize();
            return settings;
        }


        private void Commit(bool closeAfterApply)
        {
            Color textColor;
            if (!TextColorPalette.TryParse(_textColorInput.Text, out textColor))
            {
                _statusLabel.ForeColor = UiTheme.Danger;
                _statusLabel.Text = "文字颜色格式无效，请输入 #RRGGBB 形式的十六进制颜色。";
                _textColorInput.Focus();
                _textColorInput.SelectAll();
                return;
            }

            AppSettings settings = ReadSettings();
            bool applied = true;
            if (_apply != null)
            {
                try
                {
                    applied = _apply(settings);
                }
                catch (Exception ex)
                {
                    _statusLabel.ForeColor = UiTheme.Danger;
                    _statusLabel.Text = "应用失败：" + ex.Message;
                    return;
                }
            }

            if (!applied)
            {
                _statusLabel.ForeColor = UiTheme.Danger;
                _statusLabel.Text = "保存失败，请检查配置目录的写入权限。";
                return;
            }

            _statusLabel.ForeColor = UiTheme.Muted;
            _statusLabel.Text = "已保存并应用 · " + DateTime.Now.ToString("HH:mm:ss");

            if (closeAfterApply)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _toolTip.Dispose();
            }

            base.Dispose(disposing);
        }

        private sealed class TimeZoneOption
        {
            public int Offset { get; private set; }

            public TimeZoneOption(int offset)
            {
                Offset = offset;
            }

            public override string ToString()
            {
                string sign = Offset < 0 ? "-" : "+";
                int absolute = Math.Abs(Offset);
                string name = TimeZoneName(Offset);
                string result = "UTC" + sign + absolute.ToString("00") + ":00";
                return string.IsNullOrEmpty(name) ? result : result + "（" + name + "）";
            }

            private static string TimeZoneName(int offset)
            {
                switch (offset)
                {
                    case 0:
                        return "伦敦";
                    case 1:
                        return "中欧";
                    case 3:
                        return "莫斯科";
                    case 8:
                        return "中国";
                    case 9:
                        return "日本/韩国";
                    case 10:
                        return "悉尼";
                    case 12:
                        return "新西兰";
                    case -5:
                        return "美国东部";
                    case -8:
                        return "美国太平洋";
                    default:
                        return string.Empty;
                }
            }
        }
    }

    internal sealed class DarkComboBox : ComboBox
    {
        public DarkComboBox()
        {
            DrawMode = DrawMode.OwnerDrawFixed;
            DropDownStyle = ComboBoxStyle.DropDownList;
            FlatStyle = FlatStyle.Flat;
            BackColor = UiTheme.SurfaceAlt;
            ForeColor = UiTheme.Text;
            Font = UiTheme.CreateFont(9f, FontStyle.Regular);
            ItemHeight = 24;
            IntegralHeight = false;
            DropDownHeight = 240;
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color background = selected ? UiTheme.RowHover : UiTheme.SurfaceAlt;
            using (SolidBrush brush = new SolidBrush(background))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            if (e.Index >= 0 && e.Index < Items.Count)
            {
                string text = GetItemText(Items[e.Index]);
                Rectangle rect = new Rectangle(e.Bounds.X + 6, e.Bounds.Y, Math.Max(1, e.Bounds.Width - 12), e.Bounds.Height);
                TextRenderer.DrawText(
                    e.Graphics,
                    text,
                    Font,
                    rect,
                    Enabled ? UiTheme.Text : UiTheme.Muted,
                    TextFormatFlags.Left |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.EndEllipsis |
                    TextFormatFlags.NoPrefix);
            }

            if ((e.State & DrawItemState.Focus) == DrawItemState.Focus)
            {
                using (Pen pen = new Pen(UiTheme.Accent))
                {
                    Rectangle focus = e.Bounds;
                    focus.Width -= 1;
                    focus.Height -= 1;
                    e.Graphics.DrawRectangle(pen, focus);
                }
            }
        }
    }

}