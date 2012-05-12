using System;
using System.Windows.Forms;

namespace LCD.Setter
{
    class EditGPOs : Form
    {
        private static EditGPOs form = null;
        private static bool isU2L;
        private static string port;
        private static int baud;

        public static void OpenForm(bool isU2L, string port, int baud)
        {
    
            if (form == null)
                form = new EditGPOs();
            EditGPOs.isU2L = isU2L;
            EditGPOs.port = port;
            EditGPOs.baud = baud;
            form.butReadCur.Enabled = isU2L;
            form.butReadMem.Enabled = isU2L;
            form.Show();
            form.Focus();
        }
        public static void CloseForm() { if (form != null) { form.Close(); } }
        protected EditGPOs()
        {
            this.InitializeComponent();
            try { this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location); }
            catch (Exception) { }
        }
        private void InitializeComponent()
        {
            this.butReadCur = (new System.Windows.Forms.Button());
            this.butReadMem = (new System.Windows.Forms.Button());
            this.butSetMem = (new System.Windows.Forms.Button());
            this.butSetCur = (new System.Windows.Forms.Button());
            this.radioOn1 = (new System.Windows.Forms.RadioButton());
            this.groupGPO1 = (new System.Windows.Forms.GroupBox());
            this.label1 = (new System.Windows.Forms.Label());
            this.trackBar1 = (new System.Windows.Forms.TrackBar());
            this.radioOff1 = (new System.Windows.Forms.RadioButton());
            this.groupGPO2 = (new System.Windows.Forms.GroupBox());
            this.label2 = (new System.Windows.Forms.Label());
            this.trackBar2 = (new System.Windows.Forms.TrackBar());
            this.radioOff2 = (new System.Windows.Forms.RadioButton());
            this.radioOn2 = (new System.Windows.Forms.RadioButton());
            this.groupGPO3 = (new System.Windows.Forms.GroupBox());
            this.label3 = (new System.Windows.Forms.Label());
            this.trackBar3 = (new System.Windows.Forms.TrackBar());
            this.radioOff3 = (new System.Windows.Forms.RadioButton());
            this.radioOn3 = (new System.Windows.Forms.RadioButton());
            this.groupGPO4 = (new System.Windows.Forms.GroupBox());
            this.label4 = (new System.Windows.Forms.Label());
            this.trackBar4 = (new System.Windows.Forms.TrackBar());
            this.radioOff4 = (new System.Windows.Forms.RadioButton());
            this.radioOn4 = (new System.Windows.Forms.RadioButton());
            this.groupGPO5 = (new System.Windows.Forms.GroupBox());
            this.label5 = (new System.Windows.Forms.Label());
            this.trackBar5 = (new System.Windows.Forms.TrackBar());
            this.radioOff5 = (new System.Windows.Forms.RadioButton());
            this.radioOn5 = (new System.Windows.Forms.RadioButton());
            this.groupGPO1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this.trackBar1).BeginInit();
            this.groupGPO2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this.trackBar2).BeginInit();
            this.groupGPO3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this.trackBar3).BeginInit();
            this.groupGPO4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this.trackBar4).BeginInit();
            this.groupGPO5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this.trackBar5).BeginInit();
            this.SuspendLayout();
            // 
            // butReadCur
            // 
            this.butReadCur.Location = new System.Drawing.Point(12, 12);
            this.butReadCur.Size = new System.Drawing.Size(112, 23);
            this.butReadCur.TabIndex = 0;
            this.butReadCur.Text = "Read LCD Current";
            this.butReadCur.UseVisualStyleBackColor = true;
            this.butReadCur.Click += this.butReadCur_Click;
            // 
            // butReadMem
            // 
            this.butReadMem.Location = new System.Drawing.Point(130, 12);
            this.butReadMem.Size = new System.Drawing.Size(112, 23);
            this.butReadMem.TabIndex = 1;
            this.butReadMem.Text = "Read LCD Memory";
            this.butReadMem.UseVisualStyleBackColor = true;
            this.butReadMem.Click += this.butReadMem_Click;
            // 
            // butSetMem
            // 
            this.butSetMem.Location = new System.Drawing.Point(366, 12);
            this.butSetMem.Size = new System.Drawing.Size(112, 23);
            this.butSetMem.TabIndex = 3;
            this.butSetMem.Text = "Set LCD Memory";
            this.butSetMem.UseVisualStyleBackColor = true;
            this.butSetMem.Click += this.butSetMem_Click;
            // 
            // butSetCur
            // 
            this.butSetCur.Location = new System.Drawing.Point(248, 12);
            this.butSetCur.Size = new System.Drawing.Size(112, 23);
            this.butSetCur.TabIndex = 2;
            this.butSetCur.Text = "Set LCD Current";
            this.butSetCur.UseVisualStyleBackColor = true;
            this.butSetCur.Click += this.butSetCur_Click;
            // 
            // radioOn1
            // 
            this.radioOn1.AutoSize = true;
            this.radioOn1.Location = new System.Drawing.Point(6, 19);
            this.radioOn1.Size = new System.Drawing.Size(39, 17);
            this.radioOn1.TabIndex = 0;
            this.radioOn1.Text = "On";
            this.radioOn1.UseVisualStyleBackColor = true;
            // 
            // groupGPO1
            // 
            this.groupGPO1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.groupGPO1.Controls.Add(this.label1);
            this.groupGPO1.Controls.Add(this.trackBar1);
            this.groupGPO1.Controls.Add(this.radioOff1);
            this.groupGPO1.Controls.Add(this.radioOn1);
            this.groupGPO1.Location = new System.Drawing.Point(12, 41);
            this.groupGPO1.Size = new System.Drawing.Size(465, 69);
            this.groupGPO1.TabIndex = 5;
            this.groupGPO1.TabStop = false;
            this.groupGPO1.Text = "GPO 1";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(57, 39);
            this.label1.Size = new System.Drawing.Size(0, 13);
            // 
            // trackBar1
            // 
            this.trackBar1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.trackBar1.LargeChange = 32;
            this.trackBar1.Location = new System.Drawing.Point(96, 19);
            this.trackBar1.Maximum = 255;
            this.trackBar1.Size = new System.Drawing.Size(363, 45);
            this.trackBar1.TabIndex = 2;
            this.trackBar1.TickFrequency = 4;
            this.trackBar1.Value = 255;
            this.trackBar1.Scroll += this.trackBar1_Scroll;
            // 
            // radioOff1
            // 
            this.radioOff1.AutoSize = true;
            this.radioOff1.Checked = true;
            this.radioOff1.Location = new System.Drawing.Point(51, 19);
            this.radioOff1.Size = new System.Drawing.Size(39, 17);
            this.radioOff1.TabIndex = 1;
            this.radioOff1.TabStop = true;
            this.radioOff1.Text = "Off";
            this.radioOff1.UseVisualStyleBackColor = true;
            // 
            // groupGPO2
            // 
            this.groupGPO2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.groupGPO2.Controls.Add(this.label2);
            this.groupGPO2.Controls.Add(this.trackBar2);
            this.groupGPO2.Controls.Add(this.radioOff2);
            this.groupGPO2.Controls.Add(this.radioOn2);
            this.groupGPO2.Location = new System.Drawing.Point(12, 118);
            this.groupGPO2.Size = new System.Drawing.Size(465, 69);
            this.groupGPO2.TabIndex = 6;
            this.groupGPO2.TabStop = false;
            this.groupGPO2.Text = "GPO 2";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(57, 39);
            this.label2.Size = new System.Drawing.Size(0, 13);
            // 
            // trackBar2
            // 
            this.trackBar2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.trackBar2.LargeChange = 32;
            this.trackBar2.Location = new System.Drawing.Point(96, 19);
            this.trackBar2.Maximum = 255;
            this.trackBar2.Size = new System.Drawing.Size(363, 45);
            this.trackBar2.TabIndex = 2;
            this.trackBar2.TickFrequency = 4;
            this.trackBar2.Value = 255;
            this.trackBar2.Scroll += this.trackBar2_Scroll;
            // 
            // radioOff2
            // 
            this.radioOff2.AutoSize = true;
            this.radioOff2.Checked = true;
            this.radioOff2.Location = new System.Drawing.Point(51, 19);
            this.radioOff2.Size = new System.Drawing.Size(39, 17);
            this.radioOff2.TabIndex = 1;
            this.radioOff2.TabStop = true;
            this.radioOff2.Text = "Off";
            this.radioOff2.UseVisualStyleBackColor = true;
            // 
            // radioOn2
            // 
            this.radioOn2.AutoSize = true;
            this.radioOn2.Location = new System.Drawing.Point(6, 19);
            this.radioOn2.Size = new System.Drawing.Size(39, 17);
            this.radioOn2.TabIndex = 0;
            this.radioOn2.Text = "On";
            this.radioOn2.UseVisualStyleBackColor = true;
            // 
            // groupGPO3
            // 
            this.groupGPO3.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.groupGPO3.Controls.Add(this.label3);
            this.groupGPO3.Controls.Add(this.trackBar3);
            this.groupGPO3.Controls.Add(this.radioOff3);
            this.groupGPO3.Controls.Add(this.radioOn3);
            this.groupGPO3.Location = new System.Drawing.Point(12, 193);
            this.groupGPO3.Size = new System.Drawing.Size(465, 69);
            this.groupGPO3.TabIndex = 7;
            this.groupGPO3.TabStop = false;
            this.groupGPO3.Text = "GPO 3";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(57, 39);
            this.label3.Size = new System.Drawing.Size(0, 13);
            // 
            // trackBar3
            // 
            this.trackBar3.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.trackBar3.LargeChange = 32;
            this.trackBar3.Location = new System.Drawing.Point(96, 19);
            this.trackBar3.Maximum = 255;
            this.trackBar3.Size = new System.Drawing.Size(363, 45);
            this.trackBar3.TabIndex = 2;
            this.trackBar3.TickFrequency = 4;
            this.trackBar3.Value = 255;
            this.trackBar3.Scroll += this.trackBar3_Scroll;
            // 
            // radioOff3
            // 
            this.radioOff3.AutoSize = true;
            this.radioOff3.Checked = true;
            this.radioOff3.Location = new System.Drawing.Point(51, 19);
            this.radioOff3.Size = new System.Drawing.Size(39, 17);
            this.radioOff3.TabIndex = 1;
            this.radioOff3.TabStop = true;
            this.radioOff3.Text = "Off";
            this.radioOff3.UseVisualStyleBackColor = true;
            // 
            // radioOn3
            // 
            this.radioOn3.AutoSize = true;
            this.radioOn3.Location = new System.Drawing.Point(6, 19);
            this.radioOn3.Size = new System.Drawing.Size(39, 17);
            this.radioOn3.TabIndex = 0;
            this.radioOn3.Text = "On";
            this.radioOn3.UseVisualStyleBackColor = true;
            // 
            // groupGPO4
            // 
            this.groupGPO4.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.groupGPO4.Controls.Add(this.label4);
            this.groupGPO4.Controls.Add(this.trackBar4);
            this.groupGPO4.Controls.Add(this.radioOff4);
            this.groupGPO4.Controls.Add(this.radioOn4);
            this.groupGPO4.Location = new System.Drawing.Point(12, 268);
            this.groupGPO4.Size = new System.Drawing.Size(465, 69);
            this.groupGPO4.TabIndex = 8;
            this.groupGPO4.TabStop = false;
            this.groupGPO4.Text = "GPO 4";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(57, 39);
            this.label4.Size = new System.Drawing.Size(0, 13);
            // 
            // trackBar4
            // 
            this.trackBar4.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.trackBar4.LargeChange = 32;
            this.trackBar4.Location = new System.Drawing.Point(96, 19);
            this.trackBar4.Maximum = 255;
            this.trackBar4.Size = new System.Drawing.Size(363, 45);
            this.trackBar4.TabIndex = 2;
            this.trackBar4.TickFrequency = 4;
            this.trackBar4.Value = 255;
            this.trackBar4.Scroll += this.trackBar4_Scroll;
            // 
            // radioOff4
            // 
            this.radioOff4.AutoSize = true;
            this.radioOff4.Checked = true;
            this.radioOff4.Location = new System.Drawing.Point(51, 19);
            this.radioOff4.Size = new System.Drawing.Size(39, 17);
            this.radioOff4.TabIndex = 1;
            this.radioOff4.TabStop = true;
            this.radioOff4.Text = "Off";
            this.radioOff4.UseVisualStyleBackColor = true;
            // 
            // radioOn4
            // 
            this.radioOn4.AutoSize = true;
            this.radioOn4.Location = new System.Drawing.Point(6, 19);
            this.radioOn4.Size = new System.Drawing.Size(39, 17);
            this.radioOn4.TabIndex = 0;
            this.radioOn4.Text = "On";
            this.radioOn4.UseVisualStyleBackColor = true;
            // 
            // groupGPO5
            // 
            this.groupGPO5.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.groupGPO5.Controls.Add(this.label5);
            this.groupGPO5.Controls.Add(this.trackBar5);
            this.groupGPO5.Controls.Add(this.radioOff5);
            this.groupGPO5.Controls.Add(this.radioOn5);
            this.groupGPO5.Location = new System.Drawing.Point(12, 343);
            this.groupGPO5.Size = new System.Drawing.Size(465, 69);
            this.groupGPO5.TabIndex = 9;
            this.groupGPO5.TabStop = false;
            this.groupGPO5.Text = "GPO 5";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(57, 39);
            this.label5.Size = new System.Drawing.Size(0, 13);
            // 
            // trackBar5
            // 
            this.trackBar5.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.trackBar5.LargeChange = 32;
            this.trackBar5.Location = new System.Drawing.Point(96, 19);
            this.trackBar5.Maximum = 255;
            this.trackBar5.Size = new System.Drawing.Size(363, 45);
            this.trackBar5.TabIndex = 2;
            this.trackBar5.TickFrequency = 4;
            this.trackBar5.Value = 255;
            this.trackBar5.Scroll += this.trackBar5_Scroll;
            // 
            // radioOff5
            // 
            this.radioOff5.AutoSize = true;
            this.radioOff5.Checked = true;
            this.radioOff5.Location = new System.Drawing.Point(51, 19);
            this.radioOff5.Size = new System.Drawing.Size(39, 17);
            this.radioOff5.TabIndex = 1;
            this.radioOff5.TabStop = true;
            this.radioOff5.Text = "Off";
            this.radioOff5.UseVisualStyleBackColor = true;
            // 
            // radioOn5
            // 
            this.radioOn5.AutoSize = true;
            this.radioOn5.Location = new System.Drawing.Point(6, 19);
            this.radioOn5.Size = new System.Drawing.Size(39, 17);
            this.radioOn5.TabIndex = 0;
            this.radioOn5.Text = "On";
            this.radioOn5.UseVisualStyleBackColor = true;
            // 
            // EditGPOs
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6, 13);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(489, 424);
            this.Controls.Add(this.groupGPO5);
            this.Controls.Add(this.groupGPO4);
            this.Controls.Add(this.groupGPO3);
            this.Controls.Add(this.groupGPO2);
            this.Controls.Add(this.groupGPO1);
            this.Controls.Add(this.butSetMem);
            this.Controls.Add(this.butSetCur);
            this.Controls.Add(this.butReadMem);
            this.Controls.Add(this.butReadCur);
            this.MinimumSize = new System.Drawing.Size(505, 460);
            this.Text = "Edit GPOs";
            this.groupGPO1.ResumeLayout();
            ((System.ComponentModel.ISupportInitialize)this.trackBar1).EndInit();
            this.groupGPO2.ResumeLayout();
            ((System.ComponentModel.ISupportInitialize)this.trackBar2).EndInit();
            this.groupGPO3.ResumeLayout();
            ((System.ComponentModel.ISupportInitialize)this.trackBar3).EndInit();
            this.groupGPO4.ResumeLayout();
            ((System.ComponentModel.ISupportInitialize)this.trackBar4).EndInit();
            this.groupGPO5.ResumeLayout();
            ((System.ComponentModel.ISupportInitialize)this.trackBar5).EndInit();
            this.ResumeLayout(false);
        }
        private Button butReadCur, butReadMem, butSetMem, butSetCur;
        private GroupBox groupGPO1, groupGPO2, groupGPO3, groupGPO4, groupGPO5;
        private RadioButton radioOn1, radioOn2, radioOn3, radioOn4, radioOn5;
        private RadioButton radioOff1, radioOff2, radioOff3, radioOff4, radioOff5;
        private Label label1, label2, label3, label4, label5;
        private TrackBar trackBar1, trackBar2, trackBar3, trackBar4, trackBar5;

        private void trackBar1_Scroll(object sender, EventArgs e) { label1.Text = trackBar1.Value.ToString(); }
        private void trackBar2_Scroll(object sender, EventArgs e) { label2.Text = trackBar2.Value.ToString(); }
        private void trackBar3_Scroll(object sender, EventArgs e) { label3.Text = trackBar3.Value.ToString(); }
        private void trackBar4_Scroll(object sender, EventArgs e) { label4.Text = trackBar4.Value.ToString(); }
        private void trackBar5_Scroll(object sender, EventArgs e) { label5.Text = trackBar5.Value.ToString(); }
        private void butReadCur_Click(object sender, EventArgs e)
        {
            COM c = null;
            try
            {
                c = new COM(port, baud);
                c.ReadGPOVal(radioOn1, radioOff1, trackBar1, label1, 1, true);
                c.ReadGPOVal(radioOn2, radioOff2, trackBar2, label2, 2, true);
                c.ReadGPOVal(radioOn3, radioOff3, trackBar3, label3, 3, true);
                c.ReadGPOVal(radioOn4, radioOff4, trackBar4, label4, 4, true);
                c.ReadGPOVal(radioOn5, radioOff5, trackBar5, label5, 5, true);
            }
            catch (COMException ex)
            {
                ex.Show(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "The COM port caused an error:\n" + ex.Message, "COM Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { if (c != null) c.Close(); }
        }
        private void butReadMem_Click(object sender, EventArgs e)
        {
            COM c = null;
            try
            {
                c = new COM(port, baud);
                c.ReadGPOVal(radioOn1, radioOff1, trackBar1, label1, 1, false);
                c.ReadGPOVal(radioOn2, radioOff2, trackBar2, label2, 2, false);
                c.ReadGPOVal(radioOn3, radioOff3, trackBar3, label3, 3, false);
                c.ReadGPOVal(radioOn4, radioOff4, trackBar4, label4, 4, false);
                c.ReadGPOVal(radioOn5, radioOff5, trackBar5, label5, 5, false);
            }
            catch (COMException ex)
            {
                ex.Show(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "The COM port caused an error:\n" + ex.Message, "COM Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { if (c != null) c.Close(); }
        }
        private void butSetCur_Click(object sender, EventArgs e)
        {
            COM c = null;
            try
            {
                c = new COM(port, baud);
                c.SendCmd(USB2LCD.Command.GPOpwm, 1, (byte)trackBar1.Value);
                c.SendCmd(radioOn1.Checked ? USB2LCD.Command.GPOon : USB2LCD.Command.GPOoff, 1);
                c.SendCmd(USB2LCD.Command.GPOpwm, 2, (byte)trackBar2.Value);
                c.SendCmd(radioOn2.Checked ? USB2LCD.Command.GPOon : USB2LCD.Command.GPOoff, 2);
                c.SendCmd(USB2LCD.Command.GPOpwm, 3, (byte)trackBar3.Value);
                c.SendCmd(radioOn3.Checked ? USB2LCD.Command.GPOon : USB2LCD.Command.GPOoff, 3);
                c.SendCmd(USB2LCD.Command.GPOpwm, 4, (byte)trackBar4.Value);
                c.SendCmd(radioOn4.Checked ? USB2LCD.Command.GPOon : USB2LCD.Command.GPOoff, 4);
                c.SendCmd(USB2LCD.Command.GPOpwm, 5, (byte)trackBar5.Value);
                c.SendCmd(radioOn5.Checked ? USB2LCD.Command.GPOon : USB2LCD.Command.GPOoff, 5);
            }
            catch (COMException ex)
            {
                ex.Show(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "The COM port caused an error:\n" + ex.Message, "COM Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { if (c != null) c.Close(); }
        }
        private void butSetMem_Click(object sender, EventArgs e)
        {
            COM c = null;
            try
            {
                c = new COM(port, baud);
                c.SendCmd(USB2LCD.Command.RememberGPOpwm, 1, (byte)trackBar1.Value);
                c.SendCmd(USB2LCD.Command.RememberGPO, 1, (byte)(radioOn1.Checked ? 1 : 0));
                c.SendCmd(USB2LCD.Command.RememberGPOpwm, 2, (byte)trackBar2.Value);
                c.SendCmd(USB2LCD.Command.RememberGPO, 2, (byte)(radioOn2.Checked ? 1 : 0));
                c.SendCmd(USB2LCD.Command.RememberGPOpwm, 3, (byte)trackBar3.Value);
                c.SendCmd(USB2LCD.Command.RememberGPO, 3, (byte)(radioOn3.Checked ? 1 : 0));
                c.SendCmd(USB2LCD.Command.RememberGPOpwm, 4, (byte)trackBar4.Value);
                c.SendCmd(USB2LCD.Command.RememberGPO, 4, (byte)(radioOn4.Checked ? 1 : 0));
                c.SendCmd(USB2LCD.Command.RememberGPOpwm, 5, (byte)trackBar5.Value);
                c.SendCmd(USB2LCD.Command.RememberGPO, 5, (byte)(radioOn5.Checked ? 1 : 0));
            }
            catch (COMException ex)
            {
                ex.Show(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "The COM port caused an error:\n" + ex.Message, "COM Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { if (c != null) c.Close(); }
        }
    }
}
