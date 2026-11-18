namespace winclock
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private ClockTimeDisplay timeDisplay;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Timer labelCronTimer;
        private System.Windows.Forms.Timer mouseEnterTimer;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.timeDisplay = new ClockTimeDisplay();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.labelCronTimer = new System.Windows.Forms.Timer(this.components);
            this.mouseEnterTimer = new System.Windows.Forms.Timer(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();


            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 140F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.Controls.Add(this.timeDisplay, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";

            this.timeDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.timeDisplay.Margin = new System.Windows.Forms.Padding(0);
            this.timeDisplay.Name = "timeDisplay";

            this.label2.AutoSize = false;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Text = "2000年01月01日 星期一";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label2.BackColor = System.Drawing.SystemColors.Control;
            this.label2.Margin = new System.Windows.Forms.Padding(0);
            this.label2.Padding = new System.Windows.Forms.Padding(0);
            this.label2.Name = "label2";

            this.label3.AutoSize = false;
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Text = "农历";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label3.BackColor = System.Drawing.SystemColors.Control;
            this.label3.Margin = new System.Windows.Forms.Padding(0);
            this.label3.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.label3.Name = "label3";

            this.labelCronTimer.Enabled = true;
            this.labelCronTimer.Interval = 1000;
            this.labelCronTimer.Tick += new System.EventHandler(this.LabelCronTick);

            this.mouseEnterTimer.Interval = 500;
            this.mouseEnterTimer.Tick += new System.EventHandler(this.MouseEnterTick);

            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(574, 240);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.Opacity = 0.7D;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.TransparencyKey = System.Drawing.SystemColors.Control;
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion
    }
}