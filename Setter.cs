using System;
using System.Threading;
using System.Windows.Forms;

namespace LCD.Setter
{
    class Setter : Form
    {
        private const int Baud = 9600;

        private LCDText lastLine;

        //private delegate void ShowMessage$(string, string, MessageBoxButtons, MessageBoxIcon);
        //private delegate void EnableForm$(bool);
        //private delegate DialogResult ShowDialog$(IWin32Window);
        //private delegate DialogResult DialogShowModal$(IWin32Window, string, string);

        private bool isU2L, isEmu;
        private string port;

        public static readonly byte[] one   = {4,  12,  4,  4,  4,  4, 14, 0};
        public static readonly byte[] two   = {14, 17,  1,  2,  4,  8, 31, 0};
        public static readonly byte[] three = {31,  2,  4,  2,  1, 17, 14, 0};
        public static readonly byte[] four  = { 2,  6, 10, 18, 31,  2,  2, 0};
        public static readonly byte[] five  = {31, 16, 30,  1,  1, 17, 14, 0};
        public static readonly byte[] six   = { 6,  8, 16, 30, 17, 17, 14, 0};
        public static readonly byte[] seven = {31,  1,  2,  2,  4,  4,  4, 0};
        public static readonly byte[] eight = {14, 17, 17, 14, 17, 17, 14, 0};

        public Setter(bool isU2L, string port)
        {
            this.port = port;
            this.isU2L = isU2L;
            this.isEmu = this.GetVersion().module == 0xEE;

            InitializeComponent();
            try { this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location); }
            catch (Exception) { }
            lastLine = textLine1;
            customChar1.SetData(one);
            customChar2.SetData(two);
            customChar3.SetData(three);
            customChar4.SetData(four);
            customChar5.SetData(five);
            customChar6.SetData(six);
            customChar7.SetData(seven);
            customChar8.SetData(eight);

            UpdateVersionInfo();

            butReadMem.Enabled = isU2L || isEmu;
            butReadCur.Enabled = isU2L || isEmu;
            butReadButtons.Enabled = isU2L || isEmu;
            butFirmware.Enabled = isU2L;
        }
        public void ShowMessage(string msg, string title, MessageBoxButtons but, MessageBoxIcon icon)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<string, string, MessageBoxButtons, MessageBoxIcon>(ShowMessage), msg, title, but, icon);
            else
                MessageBox.Show(this, msg, title, but, icon);
        }
        public void ShowMessage(string msg, string title, MessageBoxIcon icon) { ShowMessage(msg, title, MessageBoxButtons.OK, icon); }
        public void ShowErrorMessage(string msg, string title) { ShowMessage(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Error); }

        protected void EnableForm(bool e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<bool>(EnableForm), e);
            }
            else
            {
                this.Cursor = e ? Cursors.Default : Cursors.WaitCursor;
                butReadMem.Enabled = e && (isU2L || isEmu);
                butReadCur.Enabled = e && (isU2L || isEmu);
                butReadButtons.Enabled = e && (isU2L || isEmu);
                butFirmware.Enabled = e && isU2L;
                butSetCur.Enabled = e;
                butClear.Enabled = e;
                butSetMem.Enabled = e;
                butGPO.Enabled = e;
                groupDisplay.Enabled = e;
                groupScreen.Enabled = e;
                groupCustom.Enabled = e;
                groupMessage.Enabled = e;
            }
        }
        protected void UpdateVersionInfo()
        {
            USB2LCD.VersionInfo v = GetVersion();
            this.labelPort.Text =
                isU2L ? ("USB2LCD+ Device: " + port + ", v" + (((v.version >> 4) & 0xF) + "." + (v.version & 0xF)) + ", ID: " + v.serialnumber.ToString("X4")) :
                isEmu ? ("Emulated Device: " + port + ", v" + (((v.version >> 4) & 0xF) + "." + (v.version & 0xF)) + ", ID: " + v.serialnumber.ToString("X4")) :
                        ("Other Device: "+port+", module type: "+v.module.ToString("X2")+", v"+v.version.ToString("X2")+", ID: "+v.serialnumber.ToString("X4"));
        }
        protected USB2LCD.VersionInfo GetVersion()
        {
            COM c = null;
            USB2LCD.VersionInfo v = new USB2LCD.VersionInfo();
            try
            {
                c = new COM(port, Baud);
                v.version      = c.ReadByte(USB2LCD.Command.ReadVersion);
                v.module       = c.ReadByte(USB2LCD.Command.ReadModuleType);
                v.serialnumber = c.ReadShort(USB2LCD.Command.ReadSerialNum);
                c.Close();
            }
            catch (COMException ex)
            {
                ex.Show(this);
            }
            catch (Exception ex)
            {
                ShowErrorMessage("The COM port caused an error:\n"+ex.Message, "COM Error");
            }
            finally
            {
                if (c != null) { c.Close(); }
            }
            return v;
        }
        protected DialogResult ShowFileDialog()
        {
            if (this.InvokeRequired)
                return (DialogResult)this.Invoke(new Func<DialogResult>(ShowFileDialog), this);
            else
                return openFileDialog.ShowDialog(this);
        }

        private ToolTip toolTip;
        private OpenFileDialog openFileDialog;
        private Label labelPort;

        private Button butSetSerial;
        private Button butReadMem;
        private Button butReadCur;
        private Button butSetCur;
        private Button butReadButtons;
        private Button butClear;
        private Button butSetMem;
        private Button butFirmware;
        private Button butGPO;

        private GroupBox groupDisplay;
        private CheckBox checkDisplay;
        private CheckBox checkCursor;
        private CheckBox checkBlink;
        private RadioButton radioOnAlways;
        private NumericUpDown numericOnFor;
        private RadioButton radioOnFor;

        private GroupBox groupScreen;
        private TrackBar trackBarContrast;
        private TrackBar trackBarBacklight;
        private Label labelBacklight;
        private Label labelContrast;

        private GroupBox groupCustom;
        private CustomChar customChar1;
        private CustomChar customChar8;
        private CustomChar customChar7;
        private CustomChar customChar6;
        private CustomChar customChar5;
        private CustomChar customChar4;
        private CustomChar customChar3;
        private CustomChar customChar2;

        private GroupBox groupMessage;
        private LCDText textLine1;
        private LCDText textLine2;
        private LCDText textLine3;
        private LCDText textLine4;
        private GroupBox groupSpecial;
        private NumericUpDown numericChar;
        private TextBox label;
        private Button buttonInsert;

        private void InitializeComponent()
        {
            System.Windows.Forms.Label  label2;
            System.Windows.Forms.Label  label3;
            System.Windows.Forms.Label  label4;
            System.Windows.Forms.Label  label5;
            System.Windows.Forms.Label  label6;
            System.Windows.Forms.Label  label7;
            System.Windows.Forms.Label  label8;
            System.Windows.Forms.Label  label9;
            System.Windows.Forms.Label  label10;
            System.Windows.Forms.Label  label11;
            System.ComponentModel.ComponentResourceManager  resources = (new System.ComponentModel.ComponentResourceManager(typeof(Setter)));
            this.labelPort = (new System.Windows.Forms.Label());
            this.groupDisplay = (new System.Windows.Forms.GroupBox());
            this.radioOnFor = (new System.Windows.Forms.RadioButton());
            this.radioOnAlways = (new System.Windows.Forms.RadioButton());
            this.numericOnFor = (new System.Windows.Forms.NumericUpDown());
            this.checkDisplay = (new System.Windows.Forms.CheckBox());
            this.checkCursor = (new System.Windows.Forms.CheckBox());
            this.checkBlink = (new System.Windows.Forms.CheckBox());
            this.groupScreen = (new System.Windows.Forms.GroupBox());
            this.labelBacklight = (new System.Windows.Forms.Label());
            this.labelContrast = (new System.Windows.Forms.Label());
            this.trackBarBacklight = (new System.Windows.Forms.TrackBar());
            this.trackBarContrast = (new System.Windows.Forms.TrackBar());
            this.groupCustom = (new System.Windows.Forms.GroupBox());
            this.customChar8 = (new CustomChar());
            this.customChar7 = (new CustomChar());
            this.customChar6 = (new CustomChar());
            this.customChar5 = (new CustomChar());
            this.customChar4 = (new CustomChar());
            this.customChar3 = (new CustomChar());
            this.customChar2 = (new CustomChar());
            this.customChar1 = (new CustomChar());
            this.groupMessage = (new System.Windows.Forms.GroupBox());
            this.label = (new System.Windows.Forms.TextBox());
            this.groupSpecial = (new System.Windows.Forms.GroupBox());
            this.numericChar = (new System.Windows.Forms.NumericUpDown());
            this.buttonInsert = (new System.Windows.Forms.Button());
            this.textLine4 = (new LCDText());
            this.textLine3 = (new LCDText());
            this.textLine2 = (new LCDText());
            this.textLine1 = (new LCDText());
            this.butSetMem = (new System.Windows.Forms.Button());
            this.butFirmware = (new System.Windows.Forms.Button());
            this.toolTip = (new System.Windows.Forms.ToolTip());
            this.butSetSerial = (new System.Windows.Forms.Button());
            this.butReadMem = (new System.Windows.Forms.Button());
            this.butReadCur = (new System.Windows.Forms.Button());
            this.butSetCur = (new System.Windows.Forms.Button());
            this.butReadButtons = (new System.Windows.Forms.Button());
            this.butClear = (new System.Windows.Forms.Button());
            this.butGPO = (new System.Windows.Forms.Button());
            this.openFileDialog = (new System.Windows.Forms.OpenFileDialog());
            label2 = (new System.Windows.Forms.Label());
            label3 = (new System.Windows.Forms.Label());
            label4 = (new System.Windows.Forms.Label());
            label5 = (new System.Windows.Forms.Label());
            label6 = (new System.Windows.Forms.Label());
            label7 = (new System.Windows.Forms.Label());
            label8 = (new System.Windows.Forms.Label());
            label9 = (new System.Windows.Forms.Label());
            label10 = (new System.Windows.Forms.Label());
            label11 = (new System.Windows.Forms.Label());
            ((System.ComponentModel.ISupportInitialize)this.numericOnFor).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.trackBarBacklight).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.trackBarContrast).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.numericChar).BeginInit();
            this.groupDisplay.SuspendLayout();
            this.groupScreen.SuspendLayout();
            this.groupCustom.SuspendLayout();
            this.groupMessage.SuspendLayout();
            this.groupSpecial.SuspendLayout();
            this.SuspendLayout();
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(6, 16);
            label2.Text = "These properties cannot be changed directly in LCD Smartie and may interfere with its display.";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(266, 33);
            label3.Text = "minutes";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(6, 19);
            label4.Text = "Contrast: ";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(8, 64);
            label5.Text = "Backlight:";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(6, 22);
            label6.Text = "Line 1:";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(6, 43);
            label7.Text = "Line 2:";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new System.Drawing.Point(6, 64);
            label8.Text = "Line 3:";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new System.Drawing.Point(6, 85);
            label9.Text = "Line 4:";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new System.Drawing.Point(8, 21);
            label10.Text = "Code:";
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new System.Drawing.Point(6, 16);
            label11.Text = "Click in the characters to change pixels. Right click to see the integer data.";
            // 
            // labelPort
            // 
            this.labelPort.AutoSize = true;
            this.labelPort.Location = new System.Drawing.Point(12, 9);
            this.labelPort.Text = "XXXXXXXXXXXXXXXXXXX";
            // 
            // groupDisplay
            // 
            this.groupDisplay.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.groupDisplay.Controls.Add(label3);
            this.groupDisplay.Controls.Add(this.radioOnFor);
            this.groupDisplay.Controls.Add(this.radioOnAlways);
            this.groupDisplay.Controls.Add(this.numericOnFor);
            this.groupDisplay.Controls.Add(label2);
            this.groupDisplay.Controls.Add(this.checkDisplay);
            this.groupDisplay.Controls.Add(this.checkCursor);
            this.groupDisplay.Controls.Add(this.checkBlink);
            this.groupDisplay.Location = new System.Drawing.Point(12, 83);
            this.groupDisplay.Size = new System.Drawing.Size(495, 79);
            this.groupDisplay.TabIndex = 9;
            this.groupDisplay.TabStop = false;
            this.groupDisplay.Text = "Display";
            // 
            // radioOnFor
            // 
            this.radioOnFor.AutoSize = true;
            this.radioOnFor.Location = new System.Drawing.Point(157, 31);
            this.radioOnFor.TabIndex = 2;
            this.radioOnFor.Text = "On for";
            this.radioOnFor.UseVisualStyleBackColor = true;
            this.radioOnFor.CheckedChanged += this.radioOnFor_CheckedChanged;
            // 
            // radioOnAlways
            // 
            this.radioOnAlways.AutoSize = true;
            this.radioOnAlways.Checked = true;
            this.radioOnAlways.Location = new System.Drawing.Point(72, 31);
            this.radioOnAlways.TabIndex = 1;
            this.radioOnAlways.TabStop = true;
            this.radioOnAlways.Text = "Continuously";
            this.radioOnAlways.UseVisualStyleBackColor = true;
            // 
            // numericOnFor
            // 
            this.numericOnFor.Enabled = false;
            this.numericOnFor.Location = new System.Drawing.Point(217, 31);
            this.numericOnFor.Maximum = new System.Decimal(new int[4]{255, 0, 0, 0});
            this.numericOnFor.Minimum = new System.Decimal(new int[4]{1, 0, 0, 0});
            this.numericOnFor.Size = new System.Drawing.Size(43, 20);
            this.numericOnFor.TabIndex = 3;
            this.numericOnFor.Value = new System.Decimal(new int[4]{1, 0, 0, 0});
            // 
            // checkDisplay
            // 
            this.checkDisplay.AutoSize = true;
            this.checkDisplay.Checked = true;
            this.checkDisplay.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkDisplay.Location = new System.Drawing.Point(6, 32);
            this.checkDisplay.TabIndex = 0;
            this.checkDisplay.Text = "Display";
            this.checkDisplay.UseVisualStyleBackColor = true;
            this.checkDisplay.CheckedChanged += this.checkDisplay_CheckedChanged;
            // 
            // checkCursor
            // 
            this.checkCursor.AutoSize = true;
            this.checkCursor.Location = new System.Drawing.Point(6, 55);
            this.checkCursor.TabIndex = 4;
            this.checkCursor.Text = "Cursor";
            this.checkCursor.UseVisualStyleBackColor = true;
            // 
            // checkBlink
            // 
            this.checkBlink.AutoSize = true;
            this.checkBlink.Location = new System.Drawing.Point(68, 55);
            this.checkBlink.TabIndex = 5;
            this.checkBlink.Text = "Blink";
            this.checkBlink.UseVisualStyleBackColor = true;
            // 
            // groupScreen
            // 
            this.groupScreen.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.groupScreen.Controls.Add(this.labelBacklight);
            this.groupScreen.Controls.Add(this.labelContrast);
            this.groupScreen.Controls.Add(this.trackBarBacklight);
            this.groupScreen.Controls.Add(label5);
            this.groupScreen.Controls.Add(label4);
            this.groupScreen.Controls.Add(this.trackBarContrast);
            this.groupScreen.Location = new System.Drawing.Point(12, 168);
            this.groupScreen.Size = new System.Drawing.Size(495, 111);
            this.groupScreen.TabIndex = 10;
            this.groupScreen.TabStop = false;
            this.groupScreen.Text = "Screen";
            // 
            // labelBacklight
            // 
            this.labelBacklight.AutoSize = true;
            this.labelBacklight.Location = new System.Drawing.Point(35, 86);
            this.labelBacklight.Text = "255";
            // 
            // labelContrast
            // 
            this.labelContrast.AutoSize = true;
            this.labelContrast.Location = new System.Drawing.Point(36, 41);
            this.labelContrast.Text = "127";
            // 
            // trackBarBacklight
            // 
            this.trackBarBacklight.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.trackBarBacklight.LargeChange = 32;
            this.trackBarBacklight.Location = new System.Drawing.Point(64, 64);
            this.trackBarBacklight.Margin = new System.Windows.Forms.Padding(0);
            this.trackBarBacklight.Maximum = 255;
            this.trackBarBacklight.Size = new System.Drawing.Size(422, 45);
            this.trackBarBacklight.TabIndex = 1;
            this.trackBarBacklight.TickFrequency = 4;
            this.trackBarBacklight.Value = 255;
            this.trackBarBacklight.ValueChanged += this.trackBarBacklight_Scroll;
            this.trackBarBacklight.Scroll += this.trackBarBacklight_Scroll;
            // 
            // trackBarContrast
            // 
            this.trackBarContrast.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.trackBarContrast.LargeChange = 32;
            this.trackBarContrast.Location = new System.Drawing.Point(64, 19);
            this.trackBarContrast.Margin = new System.Windows.Forms.Padding(0);
            this.trackBarContrast.Maximum = 255;
            this.trackBarContrast.Size = new System.Drawing.Size(422, 45);
            this.trackBarContrast.TabIndex = 0;
            this.trackBarContrast.TickFrequency = 4;
            this.trackBarContrast.Value = 127;
            this.trackBarContrast.ValueChanged += this.trackBarContrast_Scroll;
            this.trackBarContrast.Scroll += this.trackBarContrast_Scroll;
            // 
            // groupCustom
            // 
            this.groupCustom.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.groupCustom.Controls.Add(label11);
            this.groupCustom.Controls.Add(this.customChar8);
            this.groupCustom.Controls.Add(this.customChar7);
            this.groupCustom.Controls.Add(this.customChar6);
            this.groupCustom.Controls.Add(this.customChar5);
            this.groupCustom.Controls.Add(this.customChar4);
            this.groupCustom.Controls.Add(this.customChar3);
            this.groupCustom.Controls.Add(this.customChar2);
            this.groupCustom.Controls.Add(this.customChar1);
            this.groupCustom.Location = new System.Drawing.Point(13, 285);
            this.groupCustom.Size = new System.Drawing.Size(495, 120);
            this.groupCustom.TabIndex = 11;
            this.groupCustom.TabStop = false;
            this.groupCustom.Text = "Custom Characters";
            // 
            // customChar8
            // 
            this.customChar8.BackColor = System.Drawing.Color.White;
            this.customChar8.Cursor = System.Windows.Forms.Cursors.Cross;
            this.customChar8.ForeColor = System.Drawing.Color.Black;
            this.customChar8.Location = new System.Drawing.Point(404, 33);
            this.customChar8.MaximumSize = new System.Drawing.Size(50, 80);
            this.customChar8.MinimumSize = new System.Drawing.Size(50, 80);
            this.customChar8.Size = new System.Drawing.Size(50, 80);
            this.customChar8.TabIndex = 7;
            // 
            // customChar7
            // 
            this.customChar7.BackColor = System.Drawing.Color.White;
            this.customChar7.Cursor = System.Windows.Forms.Cursors.Cross;
            this.customChar7.ForeColor = System.Drawing.Color.Black;
            this.customChar7.Location = new System.Drawing.Point(347, 33);
            this.customChar7.MaximumSize = new System.Drawing.Size(50, 80);
            this.customChar7.MinimumSize = new System.Drawing.Size(50, 80);
            this.customChar7.Size = new System.Drawing.Size(50, 80);
            this.customChar7.TabIndex = 6;
            // 
            // customChar6
            // 
            this.customChar6.BackColor = System.Drawing.Color.White;
            this.customChar6.Cursor = System.Windows.Forms.Cursors.Cross;
            this.customChar6.ForeColor = System.Drawing.Color.Black;
            this.customChar6.Location = new System.Drawing.Point(290, 33);
            this.customChar6.MaximumSize = new System.Drawing.Size(50, 80);
            this.customChar6.MinimumSize = new System.Drawing.Size(50, 80);
            this.customChar6.Size = new System.Drawing.Size(50, 80);
            this.customChar6.TabIndex = 5;
            // 
            // customChar5
            // 
            this.customChar5.BackColor = System.Drawing.Color.White;
            this.customChar5.Cursor = System.Windows.Forms.Cursors.Cross;
            this.customChar5.ForeColor = System.Drawing.Color.Black;
            this.customChar5.Location = new System.Drawing.Point(233, 32);
            this.customChar5.MaximumSize = new System.Drawing.Size(50, 80);
            this.customChar5.MinimumSize = new System.Drawing.Size(50, 80);
            this.customChar5.Size = new System.Drawing.Size(50, 80);
            this.customChar5.TabIndex = 4;
            // 
            // customChar4
            // 
            this.customChar4.BackColor = System.Drawing.Color.White;
            this.customChar4.Cursor = System.Windows.Forms.Cursors.Cross;
            this.customChar4.ForeColor = System.Drawing.Color.Black;
            this.customChar4.Location = new System.Drawing.Point(176, 33);
            this.customChar4.MaximumSize = new System.Drawing.Size(50, 80);
            this.customChar4.MinimumSize = new System.Drawing.Size(50, 80);
            this.customChar4.Size = new System.Drawing.Size(50, 80);
            this.customChar4.TabIndex = 3;
            // 
            // customChar3
            // 
            this.customChar3.BackColor = System.Drawing.Color.White;
            this.customChar3.Cursor = System.Windows.Forms.Cursors.Cross;
            this.customChar3.ForeColor = System.Drawing.Color.Black;
            this.customChar3.Location = new System.Drawing.Point(119, 33);
            this.customChar3.MaximumSize = new System.Drawing.Size(50, 80);
            this.customChar3.MinimumSize = new System.Drawing.Size(50, 80);
            this.customChar3.Size = new System.Drawing.Size(50, 80);
            this.customChar3.TabIndex = 2;
            // 
            // customChar2
            // 
            this.customChar2.BackColor = System.Drawing.Color.White;
            this.customChar2.Cursor = System.Windows.Forms.Cursors.Cross;
            this.customChar2.ForeColor = System.Drawing.Color.Black;
            this.customChar2.Location = new System.Drawing.Point(62, 32);
            this.customChar2.MaximumSize = new System.Drawing.Size(50, 80);
            this.customChar2.MinimumSize = new System.Drawing.Size(50, 80);
            this.customChar2.Size = new System.Drawing.Size(50, 80);
            this.customChar2.TabIndex = 1;
            // 
            // customChar1
            // 
            this.customChar1.BackColor = System.Drawing.Color.White;
            this.customChar1.Cursor = System.Windows.Forms.Cursors.Cross;
            this.customChar1.ForeColor = System.Drawing.Color.Black;
            this.customChar1.Location = new System.Drawing.Point(6, 32);
            this.customChar1.MaximumSize = new System.Drawing.Size(50, 80);
            this.customChar1.MinimumSize = new System.Drawing.Size(50, 80);
            this.customChar1.Size = new System.Drawing.Size(50, 80);
            this.customChar1.TabIndex = 0;
            // 
            // groupMessage
            // 
            this.groupMessage.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.groupMessage.Controls.Add(this.label);
            this.groupMessage.Controls.Add(this.groupSpecial);
            this.groupMessage.Controls.Add(label9);
            this.groupMessage.Controls.Add(this.textLine4);
            this.groupMessage.Controls.Add(label8);
            this.groupMessage.Controls.Add(this.textLine3);
            this.groupMessage.Controls.Add(label7);
            this.groupMessage.Controls.Add(this.textLine2);
            this.groupMessage.Controls.Add(label6);
            this.groupMessage.Controls.Add(this.textLine1);
            this.groupMessage.Location = new System.Drawing.Point(13, 411);
            this.groupMessage.Size = new System.Drawing.Size(495, 117);
            this.groupMessage.TabIndex = 12;
            this.groupMessage.TabStop = false;
            this.groupMessage.Text = "Message";
            // 
            // label
            // 
            this.label.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.label.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.label.Location = new System.Drawing.Point(383, 22);
            this.label.Multiline = true;
            this.label.ReadOnly = true;
            this.label.Size = new System.Drawing.Size(105, 80);
            this.label.TabStop = false;
            this.label.Text = "All special characters show up per the 'chars.txt' file. Every LCD has different special characters, check your manual.";
            // 
            // groupSpecial
            // 
            this.groupSpecial.Controls.Add(label10);
            this.groupSpecial.Controls.Add(this.numericChar);
            this.groupSpecial.Controls.Add(this.buttonInsert);
            this.groupSpecial.Location = new System.Drawing.Point(260, 19);
            this.groupSpecial.Size = new System.Drawing.Size(118, 76);
            this.groupSpecial.TabIndex = 9;
            this.groupSpecial.TabStop = false;
            this.groupSpecial.Text = "Special Character";
            // 
            // numericChar
            // 
            this.numericChar.Location = new System.Drawing.Point(60, 19);
            this.numericChar.Maximum = new System.Decimal(new int[4]{255, 0, 0, 0});
            this.numericChar.Size = new System.Drawing.Size(45, 20);
            this.numericChar.TabIndex = 0;
            this.numericChar.KeyPress += this.numericChar_KeyPress;
            // 
            // buttonInsert
            // 
            this.buttonInsert.Location = new System.Drawing.Point(10, 45);
            this.buttonInsert.Size = new System.Drawing.Size(95, 22);
            this.buttonInsert.TabIndex = 1;
            this.buttonInsert.Text = "Type Character";
            this.buttonInsert.UseVisualStyleBackColor = true;
            this.buttonInsert.Click += this.buttonInsert_Click;
            // 
            // textLine4
            // 
            this.textLine4.Location = new System.Drawing.Point(50, 82);
            this.textLine4.Size = new System.Drawing.Size(206, 20);
            this.textLine4.TabIndex = 3;
            this.textLine4.Text = "`~!@#$&{}\\|,.<>:;?\"'";
            this.textLine4.Enter += this.textLine4_Enter;
            // 
            // textLine3
            // 
            this.textLine3.Location = new System.Drawing.Point(50, 61);
            this.textLine3.Size = new System.Drawing.Size(206, 20);
            this.textLine3.TabIndex = 2;
            this.textLine3.Text = "0123456789-+=/*%()[]";
            this.textLine3.Enter += this.textLine3_Enter;
            // 
            // textLine2
            // 
            this.textLine2.Location = new System.Drawing.Point(50, 40);
            this.textLine2.Size = new System.Drawing.Size(206, 20);
            this.textLine2.TabIndex = 1;
            this.textLine2.Text = "abcdefghijklmnopqrst";
            this.textLine2.Enter += this.textLine2_Enter;
            // 
            // textLine1
            // 
            this.textLine1.Location = new System.Drawing.Point(50, 19);
            this.textLine1.Size = new System.Drawing.Size(206, 20);
            this.textLine1.TabIndex = 0;
            this.textLine1.Text = "ABCDEFGHIJKLMNOPQRST";
            this.textLine1.Enter += this.textLine1_Enter;
            // 
            // butSetMem
            // 
            this.butSetMem.Location = new System.Drawing.Point(131, 54);
            this.butSetMem.Size = new System.Drawing.Size(112, 23);
            this.butSetMem.TabIndex = 6;
            this.butSetMem.Text = "Set LCD Memory";
            this.toolTip.SetToolTip(this.butSetMem, "Set the saved LCD values from this form.\r\nThis will be used at bootup before anything has a chance to communicate with the device.\r\nNote: This inadvertently sets many current values.");
            this.butSetMem.UseVisualStyleBackColor = true;
            this.butSetMem.Click += this.butSetMem_Click;
            // 
            // butFirmware
            // 
            this.butFirmware.Location = new System.Drawing.Point(367, 26);
            this.butFirmware.Size = new System.Drawing.Size(112, 23);
            this.butFirmware.TabIndex = 4;
            this.butFirmware.Text = "Update Firmware";
            this.toolTip.SetToolTip(this.butFirmware, "Update the firmware of the USB2LCD+");
            this.butFirmware.UseVisualStyleBackColor = true;
            this.butFirmware.Click += this.butFirmware_Click;
            // 
            // toolTip
            // 
            this.toolTip.AutomaticDelay = 100;
            this.toolTip.AutoPopDelay = 2000;
            this.toolTip.InitialDelay = 100;
            this.toolTip.ReshowDelay = 20;
            // 
            // butSetSerial
            // 
            this.butSetSerial.Location = new System.Drawing.Point(367, 4);
            this.butSetSerial.Size = new System.Drawing.Size(112, 20);
            this.butSetSerial.TabIndex = 13;
            this.butSetSerial.Text = "Set ID";
            this.toolTip.SetToolTip(this.butSetSerial, "Sets the ID of the device");
            this.butSetSerial.UseVisualStyleBackColor = true;
            this.butSetSerial.Click += this.butSetSerial_Click;
            // 
            // butReadMem
            // 
            this.butReadMem.Location = new System.Drawing.Point(13, 54);
            this.butReadMem.Size = new System.Drawing.Size(112, 23);
            this.butReadMem.TabIndex = 5;
            this.butReadMem.Text = "Read LCD Memory";
            this.toolTip.SetToolTip(this.butReadMem, "Read the saved LCD values into the form");
            this.butReadMem.UseVisualStyleBackColor = true;
            this.butReadMem.Click += this.butReadMem_Click;
            // 
            // butReadCur
            // 
            this.butReadCur.Location = new System.Drawing.Point(13, 26);
            this.butReadCur.Size = new System.Drawing.Size(112, 23);
            this.butReadCur.TabIndex = 1;
            this.butReadCur.Text = "Read LCD Current";
            this.toolTip.SetToolTip(this.butReadCur, "Reads the current LCD values into the form");
            this.butReadCur.UseVisualStyleBackColor = true;
            this.butReadCur.Click += this.butReadCur_Click;
            // 
            // butSetCur
            // 
            this.butSetCur.Location = new System.Drawing.Point(131, 26);
            this.butSetCur.Size = new System.Drawing.Size(112, 23);
            this.butSetCur.TabIndex = 2;
            this.butSetCur.Text = "Set LCD Current";
            this.toolTip.SetToolTip(this.butSetCur, "Set the current LCD values from this form.\r\nLike using any other program (such as LCD Smartie)");
            this.butSetCur.UseVisualStyleBackColor = true;
            this.butSetCur.Click += this.butSetCur_Click;
            // 
            // butReadButtons
            // 
            this.butReadButtons.Location = new System.Drawing.Point(249, 54);
            this.butReadButtons.Size = new System.Drawing.Size(112, 23);
            this.butReadButtons.TabIndex = 7;
            this.butReadButtons.Text = "Read Buttons";
            this.toolTip.SetToolTip(this.butReadButtons, "Reads the button states");
            this.butReadButtons.UseVisualStyleBackColor = true;
            this.butReadButtons.Click += this.butReadButtons_Click;
            // 
            // butClear
            // 
            this.butClear.Location = new System.Drawing.Point(249, 26);
            this.butClear.Size = new System.Drawing.Size(112, 23);
            this.butClear.TabIndex = 3;
            this.butClear.Text = "Clear Display";
            this.toolTip.SetToolTip(this.butClear, "Clears the LCD display");
            this.butClear.UseVisualStyleBackColor = true;
            this.butClear.Click += this.butClear_Click;
            // 
            // butGPO
            // 
            this.butGPO.Location = new System.Drawing.Point(367, 54);
            this.butGPO.Size = new System.Drawing.Size(112, 23);
            this.butGPO.TabIndex = 8;
            this.butGPO.Text = "Edit GPOs";
            this.butGPO.UseVisualStyleBackColor = true;
            this.butGPO.Click += this.butGPO_Click;
            // 
            // openFileDialog
            // 
            this.openFileDialog.Filter = "HEX File (*.hex)|*.hex";
            this.openFileDialog.RestoreDirectory = true;
            // 
            // Setter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6, 13);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(519, 540);
            this.Controls.Add(this.butSetSerial);
            this.Controls.Add(this.butFirmware);
            this.Controls.Add(this.butGPO);
            this.Controls.Add(this.butClear);
            this.Controls.Add(this.butReadButtons);
            this.Controls.Add(this.butSetCur);
            this.Controls.Add(this.butReadCur);
            this.Controls.Add(this.butReadMem);
            this.Controls.Add(this.groupMessage);
            this.Controls.Add(this.groupCustom);
            this.Controls.Add(this.groupScreen);
            this.Controls.Add(this.groupDisplay);
            this.Controls.Add(this.labelPort);
            this.Controls.Add(this.butSetMem);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Setter";
            this.Text = "LCD Setter";
            this.FormClosing += this.Setter_FormClosing;
            this.groupDisplay.ResumeLayout();
            ((System.ComponentModel.ISupportInitialize)this.numericOnFor).EndInit();
            this.groupScreen.ResumeLayout();
            ((System.ComponentModel.ISupportInitialize)this.trackBarBacklight).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.trackBarContrast).EndInit();
            this.groupCustom.ResumeLayout();
            this.groupMessage.ResumeLayout();
            this.groupSpecial.ResumeLayout();
            ((System.ComponentModel.ISupportInitialize)this.numericChar).EndInit();
            this.ResumeLayout();
        }

        private void radioOnFor_CheckedChanged(object sender, EventArgs e) { numericOnFor.Enabled = radioOnFor.Checked; }
        private void checkDisplay_CheckedChanged(object sender, EventArgs e)
        {
            radioOnAlways.Enabled = checkDisplay.Checked;
            radioOnFor.Enabled = checkDisplay.Checked;
            numericOnFor.Enabled = checkDisplay.Checked && radioOnFor.Checked;
        }
        private void trackBarContrast_Scroll(object sender, EventArgs e) { labelContrast.Text = trackBarContrast.Value.ToString(); }
        private void trackBarBacklight_Scroll(object sender, EventArgs e) { labelBacklight.Text = trackBarBacklight.Value.ToString(); }
        private void textLine1_Enter(object sender, EventArgs e) { lastLine = textLine1; }
        private void textLine2_Enter(object sender, EventArgs e) { lastLine = textLine2; }
        private void textLine3_Enter(object sender, EventArgs e) { lastLine = textLine3; }
        private void textLine4_Enter(object sender, EventArgs e) { lastLine = textLine4; }
        private void buttonInsert_Click(object sender, EventArgs e)
        {
            int start = lastLine.SelectionStart;
            int len = lastLine.SelectionLength;
            if (lastLine.RealText.Length >= 20 && len < 1) return;

            char value = (char)numericChar.Value;
            string real = LCDText.GetDisplayChar(value);
            lastLine.Text = lastLine.Text.Substring(0, start) + real + lastLine.Text.Substring(start + len);

            lastLine.Focus();
            lastLine.SelectionStart = start + real.Length;
            lastLine.SelectionLength = 0;
        }
        private void numericChar_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                buttonInsert_Click(sender, e);
                numericChar.Focus();
            }
        }
        private void butSetSerial_Click(object sender, EventArgs e)
        {
            EnableForm(false);
            new Thread(SetSerial).Start();
        }
        private void SetSerial()
        {
            COM c = null;
            try
            {
                c = new COM(port, Baud);
                ushort cur = c.ReadShort(USB2LCD.Command.ReadSerialNum);
                //string str = Microsoft.VisualBasic.Interaction.InputBox("Enter 4 hex digits for the ID:", "Set ID", cur.ToString("X4"), -1, -1).Trim(); // TODO: remove Microsoft-specific usage
                //if (str.Length == 0)
                //    return;
                //ushort serial = Convert.ToUInt16(str, 16);
                //c.SendCmd(USB2LCD.Command.SetSerialNum, serial);
            }
            catch (COMException ex)
            {
                ex.Show(this);
            }
            catch (Exception ex)
            {
                ShowErrorMessage("There was an error:\n"+ex.Message, "Error");
            }
            finally
            {
                if (c != null) { c.Close(); }
                EnableForm(true);
            }
            UpdateVersionInfo();
        }
        private void butReadCur_Click(object sender, EventArgs e)
        {
            EnableForm(false);
            new Thread(ReadCur).Start();
        }
        private void ReadCur()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            COM c = null;
            try
            {
                c = new COM(port, Baud);
                int display = c.ReadByte(USB2LCD.Command.ReadDisplay);
                checkDisplay.Checked = ((display >> 2) & 1) == 1;
                checkBlink.Checked   = ((display >> 1) & 1) == 1;
                checkCursor.Checked  = ((display >> 0) & 1) == 1;
                int mins = c.ReadByte(USB2LCD.Command.ReadDisplayMin);
                radioOnAlways.Checked = mins == 0;
                radioOnFor.Checked = mins > 0;
                if (mins > 0)
                    numericOnFor.Value = mins;
                trackBarContrast.Value	= c.ReadByte(USB2LCD.Command.ReadContrast);
                trackBarBacklight.Value= c.ReadByte(USB2LCD.Command.ReadBacklight);

                customChar1.SetData(c.ReadBytes(USB2LCD.Command.ReadCustom, 0, 8));
                customChar2.SetData(c.ReadBytes(USB2LCD.Command.ReadCustom, 1, 8));
                customChar3.SetData(c.ReadBytes(USB2LCD.Command.ReadCustom, 2, 8));
                customChar4.SetData(c.ReadBytes(USB2LCD.Command.ReadCustom, 3, 8));
                customChar5.SetData(c.ReadBytes(USB2LCD.Command.ReadCustom, 4, 8));
                customChar6.SetData(c.ReadBytes(USB2LCD.Command.ReadCustom, 5, 8));
                customChar7.SetData(c.ReadBytes(USB2LCD.Command.ReadCustom, 6, 8));
                customChar8.SetData(c.ReadBytes(USB2LCD.Command.ReadCustom, 7, 8));

                string s = LCDText.GetStringFromBytes(c.ReadBytes(USB2LCD.Command.ReadMessage, 80));
                textLine1.RealText = s.Substring( 0, 20);
                textLine2.RealText = s.Substring(20, 20);
                textLine3.RealText = s.Substring(40, 20);
                textLine4.RealText = s.Substring(60, 20);
            }
            catch (COMException ex)
            {
                ex.Show(this);
            }
            catch (Exception ex)
            {
                ShowErrorMessage("There was an error:\n"+ex.Message, "Error");
            }
            finally
            {
                if (c != null) { c.Close(); }
                EnableForm(true);
            }
        }
        private void butReadMem_Click(object sender, EventArgs e)
        {
            EnableForm(false);
            new Thread(ReadMem).Start();
        }
        private void ReadMem()
        {
            COM c = null;
            try
            {
                c = new COM(port, Baud);

                int display = c.ReadByte(USB2LCD.Command.ReadSavedDisplay);
                checkDisplay.Checked = ((display >> 2) & 1) == 1;
                checkBlink.Checked   = ((display >> 1) & 1) == 1;
                checkCursor.Checked  = ((display >> 0) & 1) == 1;
                int mins = c.ReadByte(USB2LCD.Command.ReadSavedDisplayMin);
                radioOnAlways.Checked = mins == 0;
                radioOnFor.Checked = mins > 0;
                if (mins > 0)
                    numericOnFor.Value = mins;
                trackBarContrast.Value	= c.ReadByte(USB2LCD.Command.ReadSavedContrast);
                trackBarBacklight.Value= c.ReadByte(USB2LCD.Command.ReadSavedBacklight);

                customChar1.SetData(c.ReadBytes(USB2LCD.Command.ReadSavedCustom, 0, 8));
                customChar2.SetData(c.ReadBytes(USB2LCD.Command.ReadSavedCustom, 1, 8));
                customChar3.SetData(c.ReadBytes(USB2LCD.Command.ReadSavedCustom, 2, 8));
                customChar4.SetData(c.ReadBytes(USB2LCD.Command.ReadSavedCustom, 3, 8));
                customChar5.SetData(c.ReadBytes(USB2LCD.Command.ReadSavedCustom, 4, 8));
                customChar6.SetData(c.ReadBytes(USB2LCD.Command.ReadSavedCustom, 5, 8));
                customChar7.SetData(c.ReadBytes(USB2LCD.Command.ReadSavedCustom, 6, 8));
                customChar8.SetData(c.ReadBytes(USB2LCD.Command.ReadSavedCustom, 7, 8));

                string s = LCDText.GetStringFromBytes(c.ReadBytes(USB2LCD.Command.ReadSavedMessage, 80));
                textLine1.RealText = s.Substring( 0, 20);
                textLine2.RealText = s.Substring(20, 20);
                textLine3.RealText = s.Substring(40, 20);
                textLine4.RealText = s.Substring(60, 20);
            }
            catch (COMException ex)
            {
                ex.Show(this);
            }
            catch (Exception ex)
            {
                ShowErrorMessage("There was an error:\n"+ex.Message, "Error");
            }
            finally
            {
                if (c != null) { c.Close(); }
                EnableForm(true);
            }
        }
        private void butSetCur_Click(object sender, EventArgs e)
        {
            EnableForm(false);
            new Thread(SetCur).Start();
        }
        private void SetCur()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            COM c = null;
            try
            {
                c = new COM(port, Baud);
                if (checkDisplay.Checked)
                    c.SendCmd(USB2LCD.Command.DisplayOn, radioOnAlways.Checked ? (byte)0 : (byte)numericOnFor.Value);
                else
                    c.SendCmd(USB2LCD.Command.DisplayOff);
                c.SendCmd(checkBlink.Checked	? USB2LCD.Command.BlinkOn  : USB2LCD.Command.BlinkOff);
                c.SendCmd(checkCursor.Checked	? USB2LCD.Command.CursorOn : USB2LCD.Command.CursorOff);
                c.SendCmd(USB2LCD.Command.Contrast,  (byte)trackBarContrast.Value );
                c.SendCmd(USB2LCD.Command.Backlight, (byte)trackBarBacklight.Value);
                c.SendCharacter(USB2LCD.Command.DefineCustom, 0, customChar1.GetData());
                c.SendCharacter(USB2LCD.Command.DefineCustom, 1, customChar2.GetData());
                c.SendCharacter(USB2LCD.Command.DefineCustom, 2, customChar3.GetData());
                c.SendCharacter(USB2LCD.Command.DefineCustom, 3, customChar4.GetData());
                c.SendCharacter(USB2LCD.Command.DefineCustom, 4, customChar5.GetData());
                c.SendCharacter(USB2LCD.Command.DefineCustom, 5, customChar6.GetData());
                c.SendCharacter(USB2LCD.Command.DefineCustom, 6, customChar7.GetData());
                c.SendCharacter(USB2LCD.Command.DefineCustom, 7, customChar8.GetData());
                c.SendCmd(USB2LCD.Command.Position, 1, 1);
                c.SendLine(textLine1.RealText, true);
                c.SendCmd(USB2LCD.Command.Position, 1, 2);
                c.SendLine(textLine2.RealText, true);
                c.SendCmd(USB2LCD.Command.Position, 1, 3);
                c.SendLine(textLine3.RealText, true);
                c.SendCmd(USB2LCD.Command.Position, 1, 4);
                c.SendLine(textLine4.RealText, true);
            }
            catch (COMException ex)
            {
                ex.Show(this);
            }
            catch (Exception ex)
            {
                ShowErrorMessage("There was an error:\n"+ex.Message, "Error");
            }
            finally
            {
                if (c != null) { c.Close(); }
                EnableForm(true);
            }
        }
        private void butSetMem_Click(object sender, EventArgs e)
        {
            if (!isU2L && !isEmu)
                ShowMessage("The module type is not a USB2LCD+ and may not support setting the memory, but we will try anyways.", "Module Warning", MessageBoxIcon.Information);
            EnableForm(false);
            new Thread(SetMem).Start();
        }
        private void SetMem()
        {
            COM c = null;
            try
            {
                c = new COM(port, Baud);
                c.SendCmd(USB2LCD.Command.Remember, 1); //activate remembering
                if (checkDisplay.Checked)
                    c.SendCmd(USB2LCD.Command.DisplayOn, radioOnAlways.Checked ? (byte)0 : (byte)numericOnFor.Value);
                else
                    c.SendCmd(USB2LCD.Command.DisplayOff);
                c.SendCmd(checkBlink.Checked	? USB2LCD.Command.BlinkOn  : USB2LCD.Command.BlinkOff);
                c.SendCmd(checkCursor.Checked	? USB2LCD.Command.CursorOn : USB2LCD.Command.CursorOff);
                c.SendCmd(USB2LCD.Command.Contrast,  (byte)trackBarContrast.Value );
                c.SendCmd(USB2LCD.Command.Backlight, (byte)trackBarBacklight.Value);
                c.SendCmd(USB2LCD.Command.Remember, 0); //deactivate remembering
                c.SendCharacter(USB2LCD.Command.RememberCustom, 0, customChar1.GetData());
                c.SendCharacter(USB2LCD.Command.RememberCustom, 1, customChar2.GetData());
                c.SendCharacter(USB2LCD.Command.RememberCustom, 2, customChar3.GetData());
                c.SendCharacter(USB2LCD.Command.RememberCustom, 3, customChar4.GetData());
                c.SendCharacter(USB2LCD.Command.RememberCustom, 4, customChar5.GetData());
                c.SendCharacter(USB2LCD.Command.RememberCustom, 5, customChar6.GetData());
                c.SendCharacter(USB2LCD.Command.RememberCustom, 6, customChar7.GetData());
                c.SendCharacter(USB2LCD.Command.RememberCustom, 7, customChar8.GetData());
                c.SendCmd(USB2LCD.Command.SaveStartup);
                c.SendLine(textLine1.RealText, false);
                c.SendLine(textLine2.RealText, false);
                c.SendLine(textLine3.RealText, false);
                c.SendLine(textLine4.RealText, false);
            }
            catch (COMException ex)
            {
                ex.Show(this);
            }
            catch (Exception ex)
            {
                ShowErrorMessage("There was an error:\n"+ex.Message, "Error");
            }
            finally
            {
                if (c != null) { c.Close(); }
                EnableForm(true);
            }
        }
        private void butFirmware_Click(object sender, EventArgs e)
        {
            if (GetVersion().version < 0x11)
            {
                ShowMessage("The USB2LCD+ module is too old to update the firmware. The firmware will need to be updated manually.", "Module Error", MessageBoxIcon.Warning);
            }
            else
            {
                EnableForm(false);
                new Thread(UpdateFirmware).Start();
            }
        }
        private int findDifference(bool unique, int serial, int[] a, int[] b)
        {
            if (unique)
            {
                for (int i = 0; i < b.Length; i++)
                    if (USB2LCD.GetBootloaderVersion(b[i]).serialnumber == serial)
                        return b[i];
            }
            else
            {
                int len = Math.Min(a.Length, b.Length);
                for (int i = 0; i < len; i++)
                    if (a[i] != b[i]) { return b[i]; }
                if (b.Length > a.Length) { return b[len]; }
            }
            return -1;
        }
        private void UpdateFirmware()
        {
            int retval;

            // get the HEX file to use
            if (ShowFileDialog() == DialogResult.OK)
            {
                retval = USB2LCD.CheckHEXfile(openFileDialog.FileName);
                if (retval < 0)
                {
                    ShowErrorMessage("The selected file could not be read / parsed: "+retval, "File Error");
                    EnableForm(true);
                    return;
                }
            }
            else
            {
                EnableForm(true);
                return;
            }

            this.BeginInvoke(new Func<DialogResult, IWin32Window, string, string>(ProgressDialog.ShowModal), this, "Firmware Update", "Please wait while the device resets...");

            int[] ids = USB2LCD.GetBootloaderIds();
            ushort serial;
            bool unique = true;

            COM c = null;
            try
            {
                c = new COM(port, Baud);
                serial = c.ReadShort(USB2LCD.Command.ReadSerialNum);
                for (int i = 0; i < ids.Length && unique; i++)
                    unique = USB2LCD.GetBootloaderVersion(ids[i]).serialnumber != serial;
                c.SendCmd(USB2LCD.Command.Firmware);
            }
            catch (COMException ex)
            {
                ProgressDialog.CloseForm();
                ex.Show(this);
                EnableForm(true);
                return;
            }
            catch (Exception ex)
            {
                ProgressDialog.CloseForm();
                ShowErrorMessage("There was an error:\n"+ex.Message, "Error");
                EnableForm(true);
                return;
            }
            finally
            {
                if (c != null) { c.Close(); }
            }

            Thread.Sleep(2000); // takes about 3 seconds, so we might as well just not even check for a little bit

            // We keep checking the attached devices until a bootloader device shows up that wasn't there before
            int[] new_ids;
            int id = -1;
            int tries_left = 114; // about 30 seconds max of waiting
            do
            {
                Thread.Sleep(250);
                new_ids = USB2LCD.GetBootloaderIds();
            } while (((id = findDifference(unique, serial, ids, new_ids)) == -1) && (--tries_left > 0));

            if (id == -1)
            {
                ProgressDialog.CloseForm();
                ShowErrorMessage("The device never showed up after resetting. Try to unplug it, close LCD Setter, plug it back in again, and start LCD Setter again.","Firmware Update Problem");
                EnableForm(true);
                return;
            }

            ProgressDialog.SetText("Updating firmware...");
            ProgressDialog.SetProgressStyle(false);
            retval = USB2LCD.UpdateFirmware(id, openFileDialog.FileName, ProgressDialog.SetProgress);
            ProgressDialog.CloseForm();
            if (retval != USB2LCD.SUCCESS)
                ShowErrorMessage("The firmware failed to update (error: "+retval+").\nTry to unplug the device, close LCD Setter, plug the device back in again, and start LCD Setter again.","Firmware Update Problem");
            else
                ShowMessage("The firmware was successfully updated.\n\nYou must unplug the device and plug it back in to complete the update.\n\nSome saved settings may have changed during this process.","Firmware Updated Successfully",MessageBoxIcon.None);
            EnableForm(true);
        }
        private void butReadButtons_Click(object sender, EventArgs e)
        {
            EnableForm(false);
            new Thread(ReadButtons).Start();
        }
        private void ReadButtons()
        {
            COM c = null;
            try
            {
                c = new COM(port, Baud);
                string bs = "";
                for (byte i = 1; i <= 5; i++)
                {
                    char ch = (char)c.ReadByte(USB2LCD.Command.ReadButton, i);
                    if (Char.ToLower(ch) != ch)
                        bs += i + ", ";
                }
                bs = bs.Trim().Trim(',');
                ShowMessage((bs == "") ? "No buttons are pressed" : "Buttons pressed: "+bs, "Buttons", MessageBoxIcon.None);
            }
            catch (COMException ex)
            {
                ex.Show(this);
            }
            catch (Exception ex)
            {
                ShowErrorMessage("There was an error:\n"+ex.Message, "Error");
            }
            finally
            {
                if (c != null) { c.Close(); }
                EnableForm(true);
            }
        }
        private void butClear_Click(object sender, EventArgs e)
        {
            EnableForm(false);
            new Thread(Clear).Start();
        }
        private void Clear()
        {
            COM c = null;
            try
            {
                c = new COM(port, Baud);
                c.SendCmd(USB2LCD.Command.ClearDisplay);
            }
            catch (COMException ex)
            {
                ex.Show(this);
            }
            catch (Exception ex)
            {
                ShowErrorMessage("There was an error:\n"+ex.Message, "Error");
            }
            finally
            {
                if (c != null) { c.Close(); } 
                EnableForm(true);
            }
        }
        private void Setter_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.Cursor == Cursors.WaitCursor)
                e.Cancel = true;
            EditGPOs.CloseForm();
        }
        private void butGPO_Click(object sender, EventArgs e)
        {
            EditGPOs.OpenForm(isU2L || isEmu, port, Baud);
        }
    }
}
