using System;
using System.Drawing;
using System.Windows.Forms;

namespace winclock
{
    internal sealed class ClockTimeDisplay : Panel
    {
        private readonly ClockDigitLabel[] _labels = new ClockDigitLabel[8];
        private readonly char[] _shown = new char[8];
        private Font _font;
        private int _digitWidth;
        private int _colonWidth;
        private int _cellHeight;
        private Color _foreColor = Color.Empty;
        private Color _backColor = Color.Empty;
        private bool _showSeconds = true;

        public ClockTimeDisplay()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor,
                true);
            BackColor = SystemColors.Control;

            for (int i = 0; i < _labels.Length; i++)
            {
                ClockDigitLabel label = new ClockDigitLabel();
                label.Text = IsColon(i) ? ":" : "0";
                label.BackColor = SystemColors.Control;
                label.ForeColor = SystemColors.ControlText;
                label.Size = new Size(1, 1);
                label.Margin = new Padding(0);
                _labels[i] = label;
                _shown[i] = IsColon(i) ? ':' : '\0';
                Controls.Add(label);
            }
        }

        public int RequiredWidth
        {
            get
            {
                int digitCount = _showSeconds ? 6 : 4;
                int colonCount = _showSeconds ? 2 : 1;
                return _digitWidth * digitCount + _colonWidth * colonCount;
            }
        }

        public int RequiredHeight
        {
            get { return _cellHeight; }
        }

        public void ApplyFont(Font font)
        {
            if (font == null)
            {
                return;
            }

            _font = font;
            int maxDigitWidth = 0;
            int maxDigitHeight = 0;
            for (int digit = 0; digit <= 9; digit++)
            {
                string text = digit.ToString();
                Size size = Measure(text, font);
                maxDigitWidth = Math.Max(maxDigitWidth, MeasureAdvance(text, font));
                maxDigitHeight = Math.Max(maxDigitHeight, size.Height);
            }

            int colonWidth = MeasureAdvance(":", font);
            _digitWidth = Math.Max(1, maxDigitWidth + 4);
            _colonWidth = Math.Max(1, colonWidth + 2);
            _cellHeight = Math.Max(font.Height + 4, maxDigitHeight + 4);

            for (int i = 0; i < _labels.Length; i++)
            {
                _labels[i].Font = font;
                _labels[i].Size = new Size(IsColon(i) ? _colonWidth : _digitWidth, _cellHeight);
            }

            RepositionLabels();
            Invalidate();
        }

        public void SetTime(DateTime time)
        {
            string text = _showSeconds ? time.ToString("HH:mm:ss") : time.ToString("HH:mm");
            int count = _showSeconds ? _labels.Length : 5;
            for (int i = 0; i < count; i++)
            {
                char value = text[i];
                if (_shown[i] == value)
                {
                    continue;
                }

                _shown[i] = value;
                _labels[i].Text = value.ToString();
            }
        }

        public void SetShowSeconds(bool showSeconds)
        {
            if (_showSeconds == showSeconds)
            {
                return;
            }

            _showSeconds = showSeconds;
            for (int i = 5; i < _labels.Length; i++)
            {
                _labels[i].Visible = showSeconds;
                _shown[i] = '\0';
            }

            RepositionLabels();
            Invalidate();
        }

        public void SetForeColor(Color foreColor)
        {
            if (_foreColor == foreColor)
            {
                return;
            }

            _foreColor = foreColor;
            for (int i = 0; i < _labels.Length; i++)
            {
                if (_labels[i].ForeColor != foreColor)
                {
                    _labels[i].ForeColor = foreColor;
                }
            }
        }

        public void SetBackColor(Color backColor)
        {
            if (_backColor == backColor)
            {
                return;
            }

            _backColor = backColor;
            BackColor = backColor;
            for (int i = 0; i < _labels.Length; i++)
            {
                if (_labels[i].BackColor != backColor)
                {
                    _labels[i].BackColor = backColor;
                }
            }
        }

        public Point GetColorSamplePoint()
        {
            Point local = new Point(2, Math.Max(1, Height / 2));
            if (IsHandleCreated)
            {
                return PointToScreen(local);
            }

            return local;
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            RepositionLabels();
        }

        private void RepositionLabels()
        {
            if (_font == null)
            {
                return;
            }

            int totalWidth = RequiredWidth;
            int left = Width >= totalWidth ? (Width - totalWidth) / 2 : 0;
            int top = Height >= RequiredHeight ? (Height - RequiredHeight) / 2 : 0;

            for (int i = 0; i < _labels.Length; i++)
            {
                if (!_labels[i].Visible)
                {
                    continue;
                }

                _labels[i].Location = new Point(left, top);
                left += _labels[i].Width;
            }
        }

        private static bool IsColon(int index)
        {
            return index == 2 || index == 5;
        }

        private static Size Measure(string text, Font font)
        {
            return TextRenderer.MeasureText(
                text,
                font,
                new Size(1000, 1000),
                TextFormatFlags.NoPadding |
                TextFormatFlags.SingleLine |
                TextFormatFlags.NoPrefix);
        }

        private static int MeasureAdvance(string text, Font font)
        {
            int singleWidth = Measure(text, font).Width;
            int doubleWidth = Measure(text + text, font).Width;
            return Math.Max(1, doubleWidth - singleWidth);
        }
    }

    internal sealed class ClockDigitLabel : Label
    {
        public ClockDigitLabel()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor,
                true);
            AutoSize = false;
            Margin = new Padding(0);
            Padding = new Padding(0);
            TextAlign = ContentAlignment.MiddleCenter;
            BackColor = SystemColors.Control;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            using (SolidBrush background = new SolidBrush(BackColor))
            {
                e.Graphics.FillRectangle(background, ClientRectangle);
            }

            TextRenderer.DrawText(
                e.Graphics,
                Text,
                Font,
                ClientRectangle,
                ForeColor,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPadding |
                TextFormatFlags.SingleLine |
                TextFormatFlags.NoPrefix);
        }
    }
}