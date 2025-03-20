using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace winclock
{
    public partial class Form1: Form
    {

        private NotifyIcon notifyIcon;
        private ContextMenuStrip trayMenu;        
        private readonly StringBuilder _sb = new StringBuilder(32);
        private string[] _reusableArray = new string[2];
        private readonly StringBuilder _sb1 = new StringBuilder(64);  // 预分配足够容量  
        private string _cachedFlowText = "";
        int num = 0;

        public Form1()
        {
            InitializeComponent();
            this.Text = "我的时钟";
            this.ShowInTaskbar = false;
            InitializeTrayIcon();
        }
        private void InitializeTrayIcon()
        {
            // 创建托盘图标
            notifyIcon = new NotifyIcon
            {
                Icon = this.Icon, // 托盘专用图标（同桌面图标）
                Text = "双击此处恢复窗口",
                Visible = true // 默认显示
            };

            // 创建右键菜单
            trayMenu = new ContextMenuStrip
            {
                AutoClose = false
            };
            trayMenu.Items.Add("显示窗口", null, (s, e) => ShowWindow());
            trayMenu.Items.Add("退出", null, (s, e) => ExitApplication());

            notifyIcon.ContextMenuStrip = trayMenu;

            // 双击托盘图标恢复窗口
            notifyIcon.DoubleClick += (s, e) => ShowWindow();
        }

        #region private void ShowWindow()
        /// <summary>
        /// 系统托盘应用图标中右键菜单‘显示窗口’按钮鼠标单击事件
        /// </summary>
        
        private void ShowWindow()
        {
            this.ShowInTaskbar = true;
            this.WindowState = FormWindowState.Normal;
            notifyIcon.Visible = true;
            trayMenu.Close();
            //实现任务栏折叠
            //Util.ToggleTaskbarVisibility(false); 
            //实现系统托盘自动隐藏效果
            Util.RefreshNotificationArea();
            this.Show();
        }
        #endregion

        #region private void ExitApplication()
        /// <summary>
        /// 系统托盘应用图标中右键菜单‘退出’按钮鼠标单击事件
        /// </summary>
        
        private void ExitApplication()
        {
            trayMenu.Close();
            notifyIcon.Dispose();
            Util.RefreshNotificationArea(); // 强制刷新托盘区域
            Application.Exit();
        }
        #endregion

         #region Util util = new Util();
        /// <summary>
        /// 引用Util类，可调用类内函数
        /// </summary>
       
        readonly Util util = new Util();
        #endregion

        #region private void Form1_Load(object sender, EventArgs e)
        /// <summary>
        /// 窗口初始化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            //鼠标穿透功能与鼠标移入自动隐藏功能互斥，若使用可穿透功能取消下行的注释即可。
            //MC.Penetrate(this.Handle);

            //位置初始化
            this.Left = Screen.PrimaryScreen.WorkingArea.Width - this.Width;
            this.Top = 0;
            //系统初始化
            LabelCronTick(null, null);
            FlowBrandTick(null, null);
            Form1MouseEnter(null, null);
            MouseEnterTick(null, null);
        }
        #endregion

        #region private void labelCronTick(object sender, EventArgs e)
        /// <summary>
        /// 每秒钟执行一次，用于更新所显示的时间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LabelCronTick(object sender, EventArgs e)
        {
            this.TopMost = true;
            string[] WeekDays = { "星期天", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六" };
            DateTime now = DateTime.Now;
            // 更新label1  时间
            _sb.Clear();
            _sb.Append(now.Hour.ToString("00"))
               .Append(':')
               .Append(now.Minute.ToString("00"))
               .Append(':')
               .Append(now.Second.ToString("00"));
            label1.Text = _sb.ToString();

            // 更新label2  阳历日期
            _sb.Clear();
            _sb.Append(now.ToString("yyyy年MM月dd日"))
               .Append(' ')
               .Append(WeekDays[(int)now.DayOfWeek]);
            label2.Text = _sb.ToString();
            // 更新label3 阴历日期
            _sb.Clear();
            _sb.Append(util.SolarToChineseLunisolarDate(now));
            label3.Text = _sb.ToString();
            Color[] SC = util.GetInverseColor(this.Location.X + this.Width / 2, this.Location.Y + 38);
            label1.ForeColor = SC[0];
            label2.ForeColor = SC[0];
            label3.ForeColor = SC[0];
            label4.ForeColor = SC[0];
            BackColor = SC[1];
            TransparencyKey = SC[1];
        }
        #endregion

        #region private void FlowBrandTick(object sender, EventArgs e)
        /// <summary>
        /// 每0.5秒更新一次，提示牌流水效果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FlowBrandTick(object sender, EventArgs e)
        {
            //label4.Text = MC.Flow_Time + " " + MC.Flow_String;
            Array.Copy(util.ReadNextLine(DateTime.Now), _reusableArray, 2);
            util.NoticeTimeString = _reusableArray[0];
            util.NoticeContentString = _reusableArray[1];
            _sb1.Clear();
            _sb1.Append(util.NoticeTimeString)
               .Append(' ')
               .Append(util.NoticeContentString);
            if (_cachedFlowText != _sb1.ToString())
            {
                label4.Text = _sb1.ToString();
                _cachedFlowText = _sb1.ToString();
            }
            if (flowBrandTimer.Interval == 500)
            {
                if (util.NoticeContentString == "")
                {
                    flowBrandTimer.Interval = 2000;
                }
            }
            else
            {
                flowBrandTimer.Interval = 500;
            }
        }
        #endregion

        #region private void Form1MouseEnter(object sender, EventArgs e)
        /// <summary>
        /// 鼠标移入Form事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1MouseEnter(object sender, EventArgs e)
        {
            this.Opacity = 0;
            this.mouseEnterTimer.Enabled = true;
        }
        #endregion

        #region public void MouseEnterTick(object sender, EventArgs e)
        /// <summary>
        /// 当变量num大于2时恢复应用窗口显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MouseEnterTick(object sender, EventArgs e)
        {
            if (num <= 2)
                num++;
            else
            {
                num = 0;
                this.mouseEnterTimer.Enabled = false;
                this.Opacity = 1;
            }
        }
        #endregion

    }
}
