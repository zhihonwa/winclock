using System;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;

namespace winclock
{
    class Util
    {
        #region  using DLL
        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hwnd);
        [DllImport("user32.dll")]
        private static extern Int32 ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [DllImport("user32", EntryPoint = "SetWindowLong")]
        private static extern uint SetWindowLong(IntPtr hwnd, int nIndex, uint dwNewLong);

        [DllImport("user32", EntryPoint = "GetWindowLong")]
        private static extern uint GetWindowLong(IntPtr hwnd, int nIndex);

        [DllImport("gdi32.dll")]
        private static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern int ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("shell32.dll")]
        private static extern int SHAppBarMessage(int dwMessage, ref APPBARDATA pData);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        #endregion

        #region Statement
        private static readonly ChineseLunisolarCalendar _calendar = new ChineseLunisolarCalendar();
        private const uint WS_EX_LAYERED = 0x80000;

        private const int WS_EX_TRANSPARENT = 0x20;

private const int GWL_EXSTYLE = (-20);

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;
        private const int ABM_SETSTATE = 0x0000000A;
        private const int ABS_AUTOHIDE = 0x0000001;
        private const int TB_AUTOSIZE = 0x421; // WM_USER + 33
        private const int WM_SIZE = 0x0005;
        #endregion

        #region 任务栏隐藏显示逻辑
        [StructLayout(LayoutKind.Sequential)]
        private struct APPBARDATA
        {
            public int cbSize;
            public IntPtr hWnd;
            public int uCallbackMessage;
            public int uEdge;
            public RECT rc;
            public IntPtr lParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }
        public static void SetTaskbarAutoHide(bool autoHide)
        {
            APPBARDATA abd = new APPBARDATA();
            abd.cbSize = Marshal.SizeOf(abd);
            abd.hWnd = FindWindow("Shell_TrayWnd", null);
            abd.lParam = (IntPtr)(autoHide ? ABS_AUTOHIDE : 0);
            SHAppBarMessage(ABM_SETSTATE, ref abd);
        }

        public static void ToggleTaskbarVisibility(bool visible)
        {
            IntPtr hWnd = FindWindow("Shell_TrayWnd", null);
            ShowWindow(hWnd, visible ? SW_SHOW : SW_HIDE);
        }
        #endregion

        #region public static void RefreshNotificationArea()
        /// <summary>
        /// 刷新系统托盘区域，触发自动折叠
        /// </summary>
        public static void RefreshNotificationArea()
        {
            // 1. 查找任务栏的句柄（类名：Shell_TrayWnd）
            IntPtr hTaskbar = FindWindow("Shell_TrayWnd", null);
            if (hTaskbar == IntPtr.Zero) return;

            // 2. 查找通知区域的容器窗口（类名：TrayNotifyWnd）
            IntPtr hTrayNotify = FindWindowEx(hTaskbar, IntPtr.Zero, "TrayNotifyWnd", null);
            if (hTrayNotify == IntPtr.Zero) return;

            // 3. 查找托盘图标工具栏（类名：ToolbarWindow32）
            IntPtr hToolbar = FindWindowEx(hTrayNotify, IntPtr.Zero, "ToolbarWindow32", null);
            if (hToolbar == IntPtr.Zero) return;

            // 4. 发送消息强制刷新布局
            SendMessage(hToolbar, TB_AUTOSIZE, 0, 0); // 自动调整大小
            SendMessage(hToolbar, WM_SIZE, 0, 0);     // 触发尺寸变化事件
        }
        #endregion

        #region public string SolarToChineseLunisolarDate(DateTime solarDateTime)
        /// <summary>
        /// 阳历日期转阴历日期（长格式：农历乙巳（蛇）2025年十月十五）
        /// </summary>

        public string SolarToChineseLunisolarDate(DateTime solarDateTime)
        {
            int year;
            int month;
            int day;
            bool isLeapMonth;
            string monthName;
            GetLunisolarParts(solarDateTime, out year, out month, out day, out isLeapMonth, out monthName);
            return string.Format("农历{0}{1}（{2}）{3}年{4}{5}月{6}"
                                , "甲乙丙丁戊己庚辛壬癸"[(year - 4) % 10]
                                , "子丑寅卯辰巳午未申酉戌亥"[(year - 4) % 12]
                                , "鼠牛虎兔龙蛇马羊猴鸡狗猪"[(year - 4) % 12]
                                , year
                                , isLeapMonth ? "闰" : ""
                                , monthName
                                , GetLunarDayName(day)
                                );
        }

        /// <summary>
        /// 阳历日期转阴历日期（短格式：十月十五、闰六月初一）
        /// </summary>
        public string SolarToChineseLunisolarShortDate(DateTime solarDateTime)
        {
            int year;
            int month;
            int day;
            bool isLeapMonth;
            string monthName;
            GetLunisolarParts(solarDateTime, out year, out month, out day, out isLeapMonth, out monthName);
            return (isLeapMonth ? "闰" : "") + monthName + "月" + GetLunarDayName(day);
        }

        private static void GetLunisolarParts(
            DateTime solarDateTime,
            out int year,
            out int month,
            out int day,
            out bool isLeapMonth,
            out string monthName)
        {
            year = _calendar.GetYear(solarDateTime);
            month = _calendar.GetMonth(solarDateTime);
            day = _calendar.GetDayOfMonth(solarDateTime);
            int leapMonth = _calendar.GetLeapMonth(year);
            isLeapMonth = leapMonth > 0 && month == leapMonth;
            int nameIndex = leapMonth > 0 && leapMonth <= month ? month - 1 : month;
            monthName = "无正二三四五六七八九十冬腊"[nameIndex].ToString();
        }

        private static string GetLunarDayName(int day)
        {
            const string digits = "一二三四五六七八九十";
            if (day == 10)
            {
                return "初十";
            }

            if (day == 20)
            {
                return "二十";
            }

            if (day == 30)
            {
                return "三十";
            }

            if (day < 10)
            {
                return "初" + digits[day - 1];
            }

            if (day < 20)
            {
                return "十" + digits[day % 10 - 1];
            }

            return "廿" + digits[day % 10 - 1];
        }
        #endregion


        #region public void Penetrate(IntPtr Handle)
        /// <summary>
        /// 鼠标穿透与透明
        /// </summary>
        /// <param name="Handle"></param>

        public void Penetrate(IntPtr Handle)
        {
            uint intExTemp = GetWindowLong(Handle, GWL_EXSTYLE);
            SetWindowLong(Handle, GWL_EXSTYLE, intExTemp | WS_EX_TRANSPARENT | WS_EX_LAYERED);
            Console.WriteLine("设置鼠标穿透成功");
        }
        #endregion

        #region public Color GetScreenColor(int X, int Y)
        /// <summary>
        /// 获取屏幕指定像素的颜色，用于设置透明键，使时钟背景与桌面完全融合
        /// </summary>
        public Color GetScreenColor(int X, int Y)
        {
            IntPtr hdc = GetDC(IntPtr.Zero);
            uint pixel = GetPixel(hdc, X, Y);
            ReleaseDC(IntPtr.Zero, hdc);
            return Color.FromArgb(
                (int)(pixel & 0x000000FF),
                (int)((pixel & 0x0000FF00) >> 8),
                (int)((pixel & 0x00FF0000) >> 16));
        }
        #endregion

    }
}
