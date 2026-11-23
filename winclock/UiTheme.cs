using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace winclock
{
    internal static class UiTheme
    {
        public static bool HighContrast
        {
            get { return SystemInformation.HighContrast; }
        }

        public static Color Background
        {
            get { return HighContrast ? SystemColors.Window : Color.FromArgb(15, 23, 42); }
        }

        public static Color HeaderStart
        {
            get { return HighContrast ? SystemColors.Window : Color.FromArgb(8, 17, 32); }
        }

        public static Color HeaderEnd
        {
            get { return HighContrast ? SystemColors.Window : Color.FromArgb(23, 35, 56); }
        }

        public static Color Surface
        {
            get { return HighContrast ? SystemColors.Control : Color.FromArgb(25, 33, 52); }
        }

        public static Color SurfaceAlt
        {
            get { return HighContrast ? SystemColors.Window : Color.FromArgb(11, 18, 32); }
        }

        public static Color Border
        {
            get { return HighContrast ? SystemColors.ControlDark : Color.FromArgb(38, 52, 73); }
        }

        public static Color Text
        {
            get { return HighContrast ? SystemColors.WindowText : Color.FromArgb(226, 232, 240); }
        }

        public static Color Muted
        {
            get { return HighContrast ? SystemColors.GrayText : Color.FromArgb(148, 163, 184); }
        }

        public static Color MutedStrong
        {
            get { return HighContrast ? SystemColors.GrayText : Color.FromArgb(176, 190, 208); }
        }

        public static Color Accent
        {
            get { return HighContrast ? SystemColors.Highlight : Color.FromArgb(56, 189, 248); }
        }

        public static Color AccentHover
        {
            get { return HighContrast ? SystemColors.Highlight : Color.FromArgb(125, 211, 252); }
        }

        public static Color AccentText
        {
            get { return HighContrast ? SystemColors.HighlightText : Color.FromArgb(8, 17, 32); }
        }

        public static Color Track
        {
            get { return HighContrast ? SystemColors.ControlDark : Color.FromArgb(51, 65, 85); }
        }

        public static Color RowHover
        {
            get { return HighContrast ? SystemColors.Control : Color.FromArgb(30, 41, 59); }
        }

        public static Color Danger
        {
            get { return HighContrast ? SystemColors.WindowText : Color.FromArgb(248, 113, 113); }
        }

        public static Font CreateFont(float size, FontStyle style)
        {
            return FontFactory.Create(AppSettings.DefaultClockFontFamily, size, style);
        }
    }

    internal static class UiDraw
    {
        public static GraphicsPath RoundRect(RectangleF rect, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            float diameter = Math.Max(1f, radius * 2f);
            diameter = Math.Min(diameter, Math.Min(rect.Width, rect.Height));

            if (diameter <= 1f)
            {
                path.AddRectangle(rect);
                return path;
            }

            float x = rect.X;
            float y = rect.Y;
            float w = rect.Width;
            float h = rect.Height;

            path.AddArc(x, y, diameter, diameter, 180f, 90f);
            path.AddArc(x + w - diameter, y, diameter, diameter, 270f, 90f);
            path.AddArc(x + w - diameter, y + h - diameter, diameter, diameter, 0f, 90f);
            path.AddArc(x, y + h - diameter, diameter, diameter, 90f, 90f);
            path.CloseFigure();
            return path;
        }

        public static void FillRounded(Graphics graphics, RectangleF rect, float radius, Color color)
        {
            using (GraphicsPath path = RoundRect(rect, radius))
            using (SolidBrush brush = new SolidBrush(color))
            {
                graphics.FillPath(brush, path);
            }
        }

        public static void DrawRoundedBorder(Graphics graphics, RectangleF rect, float radius, Color color)
        {
            using (GraphicsPath path = RoundRect(rect, radius))
            using (Pen pen = new Pen(color, 1f))
            {
                graphics.DrawPath(pen, path);
            }
        }
    }

    internal sealed class DarkMenuRenderer : ToolStripProfessionalRenderer
    {
        private static readonly Font CheckGlyphFont = new Font("Segoe UI Symbol", 9f, FontStyle.Bold);

        public DarkMenuRenderer()
            : base(new DarkColorTable())
        {
            RoundedEdges = false;
        }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            using (SolidBrush brush = new SolidBrush(UiTheme.Surface))
            {
                e.Graphics.FillRectangle(brush, e.AffectedBounds);
            }
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            using (Pen pen = new Pen(UiTheme.Border))
            {
                Rectangle rect = new Rectangle(0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);
                e.Graphics.DrawRectangle(pen, rect);
            }
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = e.Item.Enabled ? UiTheme.Text : UiTheme.Muted;
            base.OnRenderItemText(e);
        }

        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
        {
            Rectangle bounds = e.ImageRectangle;
            if (bounds.Height <= 0)
            {
                bounds = new Rectangle(bounds.X, 0, bounds.Width, e.Item.Height);
            }

            if (bounds.Width <= 0)
            {
                bounds = new Rectangle(0, 0, 22, e.Item.Height);
            }

            TextRenderer.DrawText(
                e.Graphics,
                "✔",
                CheckGlyphFont,
                bounds,
                UiTheme.Accent,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPadding |
                TextFormatFlags.NoPrefix);
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            using (Pen pen = new Pen(UiTheme.Border))
            {
                int y = e.Item.Height / 2;
                e.Graphics.DrawLine(pen, 8, y, e.Item.Width - 8, y);
            }
        }

        private sealed class DarkColorTable : ProfessionalColorTable
        {
            public override Color ToolStripDropDownBackground
            {
                get { return UiTheme.Surface; }
            }

            public override Color MenuItemSelected
            {
                get { return UiTheme.RowHover; }
            }

            public override Color MenuItemSelectedGradientBegin
            {
                get { return UiTheme.RowHover; }
            }

            public override Color MenuItemSelectedGradientEnd
            {
                get { return UiTheme.RowHover; }
            }

            public override Color MenuItemBorder
            {
                get { return UiTheme.Accent; }
            }

            public override Color MenuBorder
            {
                get { return UiTheme.Border; }
            }

            public override Color ImageMarginGradientBegin
            {
                get { return UiTheme.Surface; }
            }

            public override Color ImageMarginGradientMiddle
            {
                get { return UiTheme.Surface; }
            }

            public override Color ImageMarginGradientEnd
            {
                get { return UiTheme.Surface; }
            }

            public override Color SeparatorDark
            {
                get { return UiTheme.Border; }
            }

            public override Color SeparatorLight
            {
                get { return UiTheme.Border; }
            }
        }
    }

    internal sealed class ToggleSwitch : CheckBox
    {
        public ToggleSwitch()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor,
                true);
            Size = new Size(46, 26);
            Text = string.Empty;
            Cursor = Cursors.Hand;
            TabStop = true;
            BackColor = Color.Transparent;
            AccessibleRole = AccessibleRole.CheckButton;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            int trackHeight = Math.Min(26, Math.Max(18, Height - 2));
            Rectangle track = new Rectangle(1, (Height - trackHeight) / 2, Width - 2, trackHeight);
            Color trackColor = Checked ? UiTheme.Accent : UiTheme.Track;
            if (!Enabled)
            {
                trackColor = UiTheme.Border;
            }

            UiDraw.FillRounded(graphics, track, trackHeight / 2f, trackColor);

            int knobSize = trackHeight - 6;
            int knobX = Checked ? track.Right - knobSize - 3 : track.Left + 3;
            Rectangle knob = new Rectangle(knobX, track.Top + 3, knobSize, knobSize);
            using (SolidBrush knobBrush = new SolidBrush(Enabled ? Color.White : SystemColors.Control))
            {
                graphics.FillEllipse(knobBrush, knob);
            }

            if (Focused && ShowFocusCues)
            {
                ControlPaint.DrawFocusRectangle(graphics, Rectangle.Inflate(track, 2, 2), UiTheme.Text, Color.Transparent);
            }
        }
    }

    internal sealed class ModernSlider : Control
    {
        private int _minimum;
        private int _maximum = 100;
        private int _value = 70;
        private bool _dragging;

        public event EventHandler ValueChanged;

        public ModernSlider()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor,
                true);
            Height = 30;
            TabStop = true;
            Cursor = Cursors.Hand;
            BackColor = Color.Transparent;
            AccessibleRole = AccessibleRole.Slider;
        }

        public int Minimum
        {
            get { return _minimum; }
            set
            {
                _minimum = value;
                if (_maximum <= _minimum)
                {
                    _maximum = _minimum + 1;
                }

                Value = _value;
            }
        }

        public int Maximum
        {
            get { return _maximum; }
            set
            {
                _maximum = value;
                if (_maximum <= _minimum)
                {
                    _maximum = _minimum + 1;
                }

                Value = _value;
            }
        }

        public int Value
        {
            get { return _value; }
            set
            {
                int clamped = Math.Max(_minimum, Math.Min(_maximum, value));
                if (clamped == _value)
                {
                    return;
                }

                _value = clamped;
                Invalidate();
                EventHandler handler = ValueChanged;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            int padding = 9;
            int trackWidth = Math.Max(10, Width - padding * 2);
            int trackHeight = 6;
            int trackTop = (Height - trackHeight) / 2;
            Rectangle track = new Rectangle(padding, trackTop, trackWidth, trackHeight);
            UiDraw.FillRounded(graphics, track, trackHeight / 2f, UiTheme.Track);

            float ratio = (float)(_value - _minimum) / (_maximum - _minimum);
            int filledWidth = (int)Math.Round(trackWidth * ratio);
            if (filledWidth > 0)
            {
                Rectangle filled = new Rectangle(track.X, track.Y, filledWidth, track.Height);
                UiDraw.FillRounded(graphics, filled, trackHeight / 2f, Enabled ? UiTheme.Accent : UiTheme.Muted);
            }

            int knobSize = 14;
            int knobX = track.X + filledWidth - knobSize / 2;
            knobX = Math.Max(track.X - knobSize / 2, Math.Min(track.Right - knobSize / 2, knobX));
            Rectangle knob = new Rectangle(knobX, (Height - knobSize) / 2, knobSize, knobSize);
            using (SolidBrush knobBrush = new SolidBrush(Enabled ? Color.White : SystemColors.Control))
            using (Pen knobPen = new Pen(Enabled ? UiTheme.Accent : UiTheme.Border, 2f))
            {
                graphics.FillEllipse(knobBrush, knob);
                graphics.DrawEllipse(knobPen, knob);
            }

            if (Focused && ShowFocusCues)
            {
                ControlPaint.DrawFocusRectangle(graphics, new Rectangle(0, 0, Width - 1, Height - 1), UiTheme.Text, Color.Transparent);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                Focus();
                _dragging = true;
                SetValueFromX(e.X);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_dragging)
            {
                SetValueFromX(e.X);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _dragging = false;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            switch (e.KeyCode)
            {
                case Keys.Right:
                case Keys.Up:
                    Value += 1;
                    e.Handled = true;
                    break;
                case Keys.Left:
                case Keys.Down:
                    Value -= 1;
                    e.Handled = true;
                    break;
                case Keys.PageUp:
                    Value += 5;
                    e.Handled = true;
                    break;
                case Keys.PageDown:
                    Value -= 5;
                    e.Handled = true;
                    break;
                case Keys.Home:
                    Value = _minimum;
                    e.Handled = true;
                    break;
                case Keys.End:
                    Value = _maximum;
                    e.Handled = true;
                    break;
            }
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Invalidate();
        }

        private void SetValueFromX(int x)
        {
            int padding = 9;
            int trackWidth = Math.Max(1, Width - padding * 2);
            float ratio = (float)(x - padding) / trackWidth;
            ratio = Math.Max(0f, Math.Min(1f, ratio));
            Value = _minimum + (int)Math.Round(ratio * (_maximum - _minimum));
        }
    }

    internal sealed class ModernButton : Button
    {
        private bool _hover;
        private bool _pressed;

        public bool Primary { get; set; }

        public ModernButton()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            UseVisualStyleBackColor = false;
            Height = 40;
            Font = UiTheme.CreateFont(9.5f, FontStyle.Bold);
            Cursor = Cursors.Hand;
            TabStop = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Color background;
            Color foreground;
            Color border;

            if (Primary)
            {
                background = _pressed ? Color.FromArgb(2, 132, 199) : (_hover ? UiTheme.AccentHover : UiTheme.Accent);
                foreground = UiTheme.AccentText;
                border = background;
            }
            else
            {
                background = _pressed ? UiTheme.Track : (_hover ? UiTheme.RowHover : UiTheme.SurfaceAlt);
                foreground = UiTheme.Text;
                border = UiTheme.Border;
            }

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            UiDraw.FillRounded(graphics, rect, 8f, background);
            UiDraw.DrawRoundedBorder(graphics, rect, 8f, border);

            TextRenderer.DrawText(
                graphics,
                Text,
                Font,
                rect,
                foreground,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis |
                TextFormatFlags.NoPrefix);

            if (Focused && ShowFocusCues)
            {
                Rectangle focus = new Rectangle(4, 4, Math.Max(1, Width - 9), Math.Max(1, Height - 9));
                ControlPaint.DrawFocusRectangle(graphics, focus, foreground, background);
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _hover = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hover = false;
            _pressed = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                _pressed = true;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _pressed = false;
            Invalidate();
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Invalidate();
        }
    }

    internal sealed class CardPanel : Panel
    {
        public CardPanel()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor,
                true);
            BackColor = UiTheme.Background;
            Margin = new Padding(8);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            UiDraw.FillRounded(e.Graphics, rect, 14f, UiTheme.Surface);
            UiDraw.DrawRoundedBorder(e.Graphics, rect, 14f, UiTheme.Border);
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            int width = proposedSize.Width > 0 ? proposedSize.Width : Math.Max(1, Width);
            if (Controls.Count > 0)
            {
                Control child = Controls[0];
                child.Width = Math.Max(1, width - Padding.Horizontal);
                Size childSize = child.GetPreferredSize(new Size(child.Width, 0));
                int height = childSize.Height + Padding.Vertical;
                return new Size(width, Math.Max(40, height));
            }

            return base.GetPreferredSize(proposedSize);
        }
    }

    internal sealed class ColorSwatch : Control
    {
        private bool _selected;
        private bool _hover;

        public Color Value { get; set; }

        public bool Selected
        {
            get { return _selected; }
            set
            {
                if (_selected == value)
                {
                    return;
                }

                _selected = value;
                Invalidate();
            }
        }

        public ColorSwatch()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor,
                true);
            Size = new Size(26, 26);
            Cursor = Cursors.Hand;
            BackColor = Color.Transparent;
            TabStop = false;
            AccessibleRole = AccessibleRole.PushButton;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            int size = Math.Min(Width, Height);
            Rectangle bounds = new Rectangle((Width - size) / 2, (Height - size) / 2, size - 1, size - 1);
            if (_selected)
            {
                using (Pen ring = new Pen(UiTheme.Accent, 2f))
                {
                    graphics.DrawEllipse(ring, bounds);
                }
            }
            else if (_hover)
            {
                using (Pen ring = new Pen(UiTheme.Muted, 1f))
                {
                    graphics.DrawEllipse(ring, bounds);
                }
            }

            Rectangle circle = Rectangle.Inflate(bounds, -4, -4);
            using (SolidBrush brush = new SolidBrush(Value))
            {
                graphics.FillEllipse(brush, circle);
            }

            using (Pen outline = new Pen(Color.FromArgb(110, 255, 255, 255)))
            {
                graphics.DrawEllipse(outline, circle);
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _hover = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hover = false;
            Invalidate();
        }
    }

    internal sealed class DarkTextBox : TextBox
    {
        public DarkTextBox()
        {
            BorderStyle = BorderStyle.FixedSingle;
            BackColor = UiTheme.SurfaceAlt;
            ForeColor = UiTheme.Text;
            Font = UiTheme.CreateFont(9.5f, FontStyle.Regular);
            MaxLength = 7;
        }
    }

    internal sealed class HeaderPanel : Panel
    {
        public HeaderPanel()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);
            BackColor = UiTheme.HeaderStart;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = ClientRectangle;
            using (LinearGradientBrush brush = new LinearGradientBrush(
                rect,
                UiTheme.HeaderStart,
                UiTheme.HeaderEnd,
                LinearGradientMode.Horizontal))
            {
                e.Graphics.FillRectangle(brush, rect);
            }

            using (SolidBrush accentBrush = new SolidBrush(UiTheme.Accent))
            {
                e.Graphics.FillRectangle(accentBrush, 24, 18, 4, Height - 36);
            }

            int ringSize = 160;
            using (Pen ringPen = new Pen(Color.FromArgb(60, UiTheme.Accent), 1.5f))
            {
                e.Graphics.DrawEllipse(ringPen, Width - ringSize / 2 - 30, -ringSize / 2 + 8, ringSize, ringSize);
                e.Graphics.DrawEllipse(ringPen, Width - ringSize / 2 + 30, -ringSize / 2 + 66, ringSize, ringSize);
            }
        }
    }
}