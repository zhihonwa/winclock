using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

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

        private int Y_Step = 0;

        private string FlowString = "";

        private int FlowIndex = 0;

        private string FlowTime = "";
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
        /// 阳历日期转阴历日期
        /// </summary>

        public string SolarToChineseLunisolarDate(DateTime solarDateTime)
        {
            int year = _calendar.GetYear(solarDateTime);
            int month = _calendar.GetMonth(solarDateTime);
            int day = _calendar.GetDayOfMonth(solarDateTime);
            int leapMonth = _calendar.GetLeapMonth(year);
            return string.Format("农历{0}{1}（{2}）{3}年{4}{5}月{6}{7}"
                                , "甲乙丙丁戊己庚辛壬癸"[(year - 4) % 10]
                                , "子丑寅卯辰巳午未申酉戌亥"[(year - 4) % 12]
                                , "鼠牛虎兔龙蛇马羊猴鸡狗猪"[(year - 4) % 12]
                                , year
                                , month == leapMonth ? "闰" : ""
                                , "无正二三四五六七八九十冬腊"[leapMonth > 0 && leapMonth <= month ? month - 1 : month]
                                , "初十廿三"[day / 10]
                                , "日一二三四五六七八九"[day % 10]
                                );
        }
        #endregion

        #region get/set:public string NoticeContentString
        /// <summary>
        /// get得到字符串，每500ms获取一次模拟流水牌效果
        /// set设置新的字符串
        /// </summary>
        public string NoticeContentString
        {
            get
            {
                if (FlowString == null || FlowString == "")
                    return "";
                int TMP = FlowString.Length;
                if (FlowIndex > TMP - 1)
                    FlowIndex = 0;
                else
                    FlowIndex++;
                if (TMP < 12)
                    return FlowString;
                else if (TMP - FlowIndex > 20)
                    return FlowString.Substring(FlowIndex, 20);
                else
                    return FlowString.Substring(FlowIndex, FlowString.Length - FlowIndex);
            }
            set
            {
                FlowString = value;
            }
        }
        #endregion

        #region get/set:public string NoticeTimeString
        /// <summary>
        /// 用于显示Schedule中的时间
        /// </summary>
        public string NoticeTimeString
        {
            get { return FlowTime; }
            set { FlowTime = value; }
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

        #region public Color[] GetInverseColor(int X, int Y)
        /// <summary>
        /// SetColor[0]  X,Y点颜色的反色
        /// SetColor[1]  X,Y点颜色的反色的相近色，作为背景剔除
        /// SetColor[2]  X,Y点颜色的反色偏红色
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        /// 
        public Color[] GetInverseColor(int X, int Y)
        {
            Color[] _OutColor = new Color[3];
            IntPtr hdc = GetDC(IntPtr.Zero);
            uint pixel = GetPixel(hdc, X, Y);
            ReleaseDC(IntPtr.Zero, hdc);
            int R = 255 - ((int)(pixel & 0x000000FF)),
                G = 255 - ((int)(pixel & 0x0000FF00) >> 8),
                B = 255 - ((int)(pixel & 0x00FF0000) >> 16);
            _OutColor[0] = Color.FromArgb(R, G, B);
            if (R < 5)
                R += 6;
            _OutColor[1] = Color.FromArgb(R - 5, G, B);
            _OutColor[2] = Color.FromArgb(255, G / 2, B / 2);
            return _OutColor;
        }
        #endregion

        #region public string[,] ReadFromSchedule()
        /// <summary>
        /// 根据本地记录Schedule.txt获取时间和日程
        /// 第一列数据为时间，第二列数据为日程，两列数据以_分割
        /// </summary>
        /// <returns></returns>
        public string[,] ReadFromSchedule()
        {
            if (File.Exists("Schedule.txt"))
            {
                try
                {
                    using (StreamReader SR = new StreamReader("Schedule.txt", Encoding.UTF8))
                    {
                        string TMP = "";
                        List<string> _Out_List = new List<string>();
                        while ((TMP = SR.ReadLine()) != null)
                            _Out_List.Add(TMP);
                        string[] _Out_Temp = _Out_List.ToArray();
                        int Total_Line = _Out_List.Count();
                        string[,] _Out = new string[Total_Line, 2];
                        for (int i = 0; i < Total_Line; i++)
                        {
                            string[] _Temp_For = _Out_Temp[i].Split('_');
                            try
                            {
                                _Out[i, 0] = _Temp_For[0];
                                _Out[i, 1] = _Temp_For[1];
                            }
                            catch (Exception Ex)
                            {
                                _Out[i, 0] = "25:00";
                                _Out[i, 1] = Ex.Message;
                            }
                        }
                        return _Out;
                    }
                }
                catch (Exception Ex)
                {
                    return new string[,] { { "25:00", Ex.Message } };
                }
            }
            else
            {
                using (FileStream fs = new FileStream("Schedule.txt", FileMode.Create))
                {
                    return new string[,] { { "24:00", "nothing" } };
                }
            }
        }
        #endregion

        #region public string[] ReadNextLine(DateTime Time)
        /// <summary>
        /// 读取Schedule.txt中与当前事件最近的提醒
        /// _Out[0]表示时间,格式00:00
        /// _Out[1]表示内容
        /// </summary>
        /// <param name="Time"></param>
        /// <returns></returns>
        public string[] ReadNextLine(DateTime Time)
        {
            string[] _Out = new string[2];
            string[,] _Temp_String = ReadFromSchedule();
            int Total_Line = _Temp_String.Length / 2;
            for (int i = 0; i < Total_Line; i++)
            {
                string[] Split_Time = _Temp_String[i, 0].Split(':');
                if (Split_Time[0] == "-")
                    continue;
                int Hour = Convert.ToInt16(Split_Time[0]);
                int Minute = Convert.ToInt16(Split_Time[1]);
                //Hour>=24表示无日程或发生异常
                if (Hour >= 24)
                {
                    _Out[0] = "";
                    if (_Temp_String[i, 1] == "nothing")
                    {
                        _Out[1] = "暂无提醒哦！";
                    }
                    else
                    {
                        _Out[1] = "您的日程里时间格式填写有误，请检查(" + _Temp_String[i, 1] + ")";
                    }
                    break;
                }
                if ((Hour > Time.Hour) || (Hour == Time.Hour && Minute >= Time.Minute))
                {
                    _Out[0] = Hour.ToString("00:") + Minute.ToString("00");
                    _Out[1] = _Temp_String[i, 1];
                    //仅获取最近的日程
                    break;
                }
            }
            return _Out;
        }
        #endregion
    }
}
