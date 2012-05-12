using System;
using System.IO;
using System.IO.Ports;
using System.Windows.Forms;

namespace LCD.Setter
{
    // A simple interface for a COM port.
    // This is really just a wrapper for System.IO.Ports.SerialPort
    // It handles sending commands and reading data.
    class COM
    {
        private readonly SerialPort Port;
        private readonly Stream Stream;
        
        public COM(string port, int baud)
        {
            this.Port = new SerialPort(port, baud, Parity.None, 8, StopBits.One);
            this.Port.ReadTimeout = 1000;
            this.Port.WriteTimeout = 1000;
            try
            {
                this.Port.Open();
            }
            catch (Exception ex)
            {
                throw new COMException("COM Error: "+port, "The COM port could not be opened:\n"+ex.Message);
            }
            if (!this.Port.IsOpen)
            {
                throw new COMException("COM Error: "+port, "The COM port could not be opened, but no reason was given.");
            }
            this.Port.DiscardInBuffer();
            this.Stream = this.Port.BaseStream;
        }
        public void Close() { if (this.Port != null && this.Port.IsOpen) { this.Port.Close(); } } // Closes the underlying serial port
        ~COM() { this.Close(); }    // Calls Close()
        
        public void SendCmd(USB2LCD.Command cmd)                { this.Stream.WriteByte(254); this.Stream.WriteByte((byte)cmd); this.Stream.Flush(); } // Send a command with no parameters
        public void SendCmd(USB2LCD.Command cmd, byte subCmd)   { this.Stream.WriteByte(254); this.Stream.WriteByte((byte)cmd); this.Stream.WriteByte(subCmd); this.Stream.Flush(); } // Send a command that takes a single parameter
        public void SendCmd(USB2LCD.Command cmd, ushort subCmd) { this.SendCmd(USB2LCD.Command.SetSerialNum, (byte)(subCmd & 0xFF), (byte)((subCmd >> 8) & 0xFF)); }
        public void SendCmd(USB2LCD.Command cmd, byte param1, byte param2) { this.Stream.WriteByte(254); this.Stream.WriteByte((byte)cmd); this.Stream.WriteByte(param1); this.Stream.WriteByte(param2); this.Stream.Flush(); } // Send a command that takes 2 params

        private static byte CheckByte(int n)
        {
            if (n < 0)
                throw new EndOfStreamException();
            return (byte)n;
        }
        public byte ReadByte(USB2LCD.Command cmd)              { this.Port.DiscardInBuffer(); this.SendCmd(cmd);         return CheckByte(this.Port.ReadByte()); } // Send a command and read the returned byte
        public byte ReadByte(USB2LCD.Command cmd, byte subCmd) { this.Port.DiscardInBuffer(); this.SendCmd(cmd, subCmd); return CheckByte(this.Port.ReadByte()); } // Send a command with a parameter and read the returned byte
        public ushort ReadShort(USB2LCD.Command cmd)           { this.Port.DiscardInBuffer(); this.SendCmd(cmd); return (ushort)(CheckByte(this.Port.ReadByte()) | CheckByte(this.Port.ReadByte()) << 8); } // Send a command and read the returned 2 bytes
        private byte[] ReadBytes(int n)
        {
            byte[] buffer = new byte[n];
            System.Threading.Thread.Sleep(100); // make sure the PIC has time to process this and start sending data
            int read = this.Port.Read(buffer, 0, n);
            while (read < n) { read += this.Port.Read(buffer, read, n-read); }
            return buffer;
        }
        public byte[] ReadBytes(USB2LCD.Command cmd, int n)              { this.Port.DiscardInBuffer(); this.SendCmd(cmd);         return ReadBytes(n); } // Send a command and read n returned bytes
        public byte[] ReadBytes(USB2LCD.Command cmd, byte subCmd, int n) { this.Port.DiscardInBuffer(); this.SendCmd(cmd, subCmd); return ReadBytes(n); } // Send a command with a parameter and read n returned bytes

        public void SendCharacter(USB2LCD.Command cmd, byte c, byte[] bytes) // Send an entire character using cmd (DefineCustom or RememberCustom) and a character #0-7
        {
            if ((cmd != USB2LCD.Command.DefineCustom && cmd != USB2LCD.Command.RememberCustom) || c >= 8 || bytes.Length != 8)
                throw new ArgumentException();
            this.SendCmd(cmd, c);
            this.Stream.Write(bytes, 0, 8);
            this.Stream.Flush();
        }
        public void SendLine(string line, bool cmdEsc) // Send a line of data, may need a Position or SaveStartup command before this, will force line to 20 characters with spaces
        {
            line = line.PadRight(20);
            // TODO: if !cmdEsc then padding will be different
            for (int i = 0; i < 20; i++)
            {
                if (line[i] == 0x100)              { this.Stream.WriteByte(0); } // due to not able to have 0 in a string...
                else if (line[i] > 0xFF)           { this.Stream.WriteByte(32); } // space
                else if (cmdEsc && line[i] == 254) { this.Stream.WriteByte(254); this.Stream.WriteByte(254); } // escape the 254 character with another 254
                else                               { this.Stream.WriteByte((byte)line[i]); }
            }
            this.Stream.Flush();
        }

        // Reads a GPO val
        public void ReadGPOVal(RadioButton on, RadioButton off, TrackBar bar, Label label, byte i, bool cur)
        {
            int gpo = this.ReadByte(cur ? USB2LCD.Command.ReadGPO : USB2LCD.Command.ReadSavedGPO, i);
            int pwm = this.ReadByte(cur ? USB2LCD.Command.ReadGPOpwm : USB2LCD.Command.ReadSavedGPOpwm, i);
            on.Checked = gpo==1;
            off.Checked = gpo==0;
            bar.Value = pwm;
            label.Text = pwm.ToString();
        }
    }

    // A special exception class thrown by the COM constructor that has a title and text ready for a dialog box
    class COMException : Exception
    {
        public readonly string Title;
        public readonly string Text;
        public COMException(string title, string text) : base(text) { this.Title = title; this.Text = text; }
        // Shows a dialog box with the title of this exception and the text of this exception
        public void Show(Form f)
        {
            //if (f.GetType() == lcdsetter::Setter::typeid) {
            //    ((lcdsetter::Setter)f).ShowErrorMessage(text, title);
            //} else {
                MessageBox.Show(f, this.Text, this.Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
        }
    }
}
