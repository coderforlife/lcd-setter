using System;
using System.Drawing;
using System.Windows.Forms;

namespace LCD.Setter
{
    // The progress bar dialog used during firmware updates
    class ProgressDialog : Form
    {
        private static ProgressDialog d = null;
        private ProgressDialog(string title, string message)
        {
            InitializeComponent();
            this.Text = title;
            label.Text = message;
        }
        private DialogResult ShowDialog_(IWin32Window w)
        {
            if (this.InvokeRequired)
                return (DialogResult)this.Invoke(new Func<DialogResult, IWin32Window>(d.ShowDialog), w);
            else
                return this.ShowDialog(w);
        }

        public static void CreateForm()
        {
            if (d == null)
                d = new ProgressDialog("", "");
        }
        public static DialogResult ShowModal(IWin32Window w, string title, string message)
        {
            if (d != null)
            {
                SetTitle(title);
                SetText(message);
            }
            else
            {
                d = new ProgressDialog(title, message);
            }
            SetProgress(0, 100);
            SetProgressStyle(true);
            return d.ShowDialog_(w);
        }
        public static void ShowNonModal(string title, string message)
        {
            if (d != null)
            {
                SetTitle(title);
                SetText(message);
            }
            else
            {
                d = new ProgressDialog(title, message);
            }
            SetProgress(0, 100);
            SetProgressStyle(true);
            d.ShowInTaskbar = true;
            d.Show();
        }
        public static void CloseForm()
        {
            if (d == null) return;
            if (d.InvokeRequired)
                d.Invoke(new Action(d.Close));
            else
                d.Close();
        }

        public static void SetTitle(string title)
        {
            if (d == null) return;
            if (d.InvokeRequired)
                d.Invoke(new Action<string>(SetTitle), title);
            else
                d.Text = title;
        }
        public static void SetText(string text)
        {
            if (d == null) return;
            if (d.InvokeRequired)
                d.Invoke(new Action<string>(SetText), text);
            else
                d.label.Text = text;
        }
        public static void SetProgress(int progress)
        {
            if (d == null) return;
            if (d.InvokeRequired)
                d.Invoke(new Action<int>(SetProgress), progress);
            else
                d.progressBar.Value = progress;
        }
        public static void SetProgressMax(int max)
        {
            if (d == null) return;
            if (d.InvokeRequired)
                d.Invoke(new Action<int>(SetProgressMax), max);
            else
                d.progressBar.Maximum = max;
        }
        public static void SetProgressStyle(bool marquee)
        {
            if (d == null) return;
            if (d.InvokeRequired)
                d.Invoke(new Action<bool>(SetProgressStyle), marquee);
            else
                d.progressBar.Style = marquee ? ProgressBarStyle.Marquee : ProgressBarStyle.Continuous;
        }

        private ProgressBar progressBar;
        private Label label;
        private void InitializeComponent()
        {
            this.progressBar = (new System.Windows.Forms.ProgressBar());
            this.label = (new System.Windows.Forms.Label());
            this.SuspendLayout();
            // 
            // progressBar
            // 
            this.progressBar.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.progressBar.Location = new System.Drawing.Point(12, 29);
            this.progressBar.Size = new System.Drawing.Size(288, 25);
            this.progressBar.Minimum = 0;
            // 
            // label
            // 
            this.label.AutoSize = true;
            this.label.Location = new System.Drawing.Point(12, 9);
            // 
            // Dialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6, 13);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new Size(312, 66);
            this.ControlBox = false;
            this.Controls.Add(this.label);
            this.Controls.Add(this.progressBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.ResumeLayout();
        }

        public static void SetProgress(int cur, int total)
        {
            if (d == null) return;
            if (d.InvokeRequired)
            {
                d.Invoke(new USB2LCD.ProgressCallback(SetProgress), cur, total);
            }
            else
            {
                d.progressBar.Value = cur;
                d.progressBar.Maximum = total;
            }
        }
    }
}
