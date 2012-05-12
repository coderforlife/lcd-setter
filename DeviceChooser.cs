using System;
using System.IO.Ports;
using System.Windows.Forms;

namespace LCD.Setter
{
    // The device chooser screen (the first window that comes up)
    class DeviceChooser : Form
    {
        public Device device;
        public DeviceChooser()
        {
            this.InitializeComponent();
            try { this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location); }
            catch (Exception) { }
            this.RefreshList();
        }
        protected void RefreshList()
        {
            combo.Items.Clear();

            int[] bls;
            string[] coms;

            try
            {
                bls = USB2LCD.GetBootloaderIds();
            }
            catch (USB2LCD.Exception ex)
            {
                MessageBox.Show(this, "Could not get the number of devices in bootloader mode (Error: "+ex.error+")", "Device Search Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                bls = new int[0];
            }

            try
            {
                coms = USB2LCD.GetCOMPorts();
            }
            catch (USB2LCD.Exception ex)
            {
                MessageBox.Show(this, "Could not get the number of devices in COM mode (Error: "+ex.error+")", "Device Search Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                coms = new string[0];
            }

            foreach (int bl in bls)      combo.Items.Add(new Device(DeviceType.Bootloader, bl, USB2LCD.GetBootloaderVersion(bl)));
            foreach (string com in coms) combo.Items.Add(new Device(DeviceType.USB2LCD, com, GetCOMVersion(com)));

            foreach (string s in SerialPort.GetPortNames())
            {
                if (!Array.Exists(coms, delegate(string com){ return com == s; }))
                    combo.Items.Add(new Device(DeviceType.COM, s, new USB2LCD.VersionInfo()));
            }

            if (combo.Items.Count == 0)
            {
                combo.Items.Add("No devices found");
                buttonSelect.Enabled = false;
            }
            else
            {
                buttonSelect.Enabled = true;
            }

            combo.SelectedIndex = 0;
        }
        protected USB2LCD.VersionInfo GetCOMVersion(string port)
        {
            COM c = null;
            USB2LCD.VersionInfo v = new USB2LCD.VersionInfo();
            try
            {
                c = new COM(port, 9600);
                v.version = c.ReadByte(USB2LCD.Command.ReadVersion);
                v.module = c.ReadByte(USB2LCD.Command.ReadModuleType);
                v.serialnumber = c.ReadShort(USB2LCD.Command.ReadSerialNum);;
                c.Close();
            }
            catch (COMException ex)
            {
                ex.Show(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "The COM port caused an error:\n"+ex.Message, "COM Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (c != null) c.Close();
            }
            return v;
        }
        private ComboBox combo;
        private Button buttonSelect;
        private void InitializeComponent()
        {
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Button buttonRefresh;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DeviceChooser));
            this.combo = new System.Windows.Forms.ComboBox();
            this.buttonSelect = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            buttonRefresh = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(12, 9);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(116, 13);
            label1.TabIndex = 0;
            label1.Text = "Select a device to use:";
            // 
            // combo
            // 
            this.combo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.combo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.combo.FormattingEnabled = true;
            this.combo.Location = new System.Drawing.Point(15, 30);
            this.combo.Name = "combo";
            this.combo.Size = new System.Drawing.Size(250, 21);
            this.combo.TabIndex = 0;
            // 
            // buttonRefresh
            // 
            buttonRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            buttonRefresh.Location = new System.Drawing.Point(271, 29);
            buttonRefresh.Name = "buttonRefresh";
            buttonRefresh.Size = new System.Drawing.Size(75, 23);
            buttonRefresh.TabIndex = 1;
            buttonRefresh.Text = "Refresh";
            buttonRefresh.UseVisualStyleBackColor = true;
            buttonRefresh.Click += buttonRefresh_Click;
            // 
            // buttonSelect
            // 
            this.buttonSelect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSelect.Location = new System.Drawing.Point(352, 29);
            this.buttonSelect.Name = "buttonSelect";
            this.buttonSelect.Size = new System.Drawing.Size(75, 23);
            this.buttonSelect.TabIndex = 2;
            this.buttonSelect.Text = "Select";
            this.buttonSelect.UseVisualStyleBackColor = true;
            this.buttonSelect.Click += buttonSelect_Click;
            // 
            // DeviceChooser
            // 
            this.AcceptButton = this.buttonSelect;
            this.ClientSize = new System.Drawing.Size(439, 64);
            this.Controls.Add(label1);
            this.Controls.Add(this.buttonSelect);
            this.Controls.Add(buttonRefresh);
            this.Controls.Add(this.combo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "DeviceChooser";
            this.Text = "LCD Setter: Choose Device";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        private void buttonRefresh_Click(object sender, EventArgs e) { this.RefreshList(); }
        private void buttonSelect_Click(object sender, EventArgs e)
        {
            Device d = (Device)combo.SelectedItem;
            this.DialogResult = DialogResult.OK;
            this.device = d;
            this.Close();
        }
    }
}
