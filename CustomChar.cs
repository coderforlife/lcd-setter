using System;
using System.Drawing;
using System.Windows.Forms;

namespace LCD.Setter
{
    // The user control for a custom character editor.
    // Will use whatever foreground/background color you give it.
    // The default character is some zig-zag thing.
    class CustomChar : UserControl
    {
        private const int CC_WIDTH = 5;

        private readonly byte[] data = new byte[]{1, 2, 4, 8, 16, 8, 4, 2};
        private int active_c = -1, active_r = -1;

        public CustomChar()
        {
            this.SuspendLayout();
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.FixedWidth, true);
            this.SetStyle(ControlStyles.FixedHeight, true);
            this.SetStyle(ControlStyles.Selectable, true);
            this.AutoScaleMode = AutoScaleMode.None;
            this.BackColor = Color.White;
            this.Cursor = Cursors.Cross;
            this.ForeColor = Color.Black;
            this.MaximumSize = new Size(CC_WIDTH*10, 80);
            this.MinimumSize = new Size(CC_WIDTH*10, 80);
            this.Size        = new Size(CC_WIDTH*10, 80);
            this.Name = "CustomChar";
            this.ResumeLayout(false);
        }
        // TODO: make more C#-esque
        public byte[] GetDataDynamic()  { return data; } // changes in the returned array effect the data in the character
        public byte[] GetData()         { return (byte[])data.Clone(); } // returns a copy of the data array
        public byte GetRowData(int row) { return data[row]; } // gets one of the rows of data
        public bool GetPixelData(int row, int col) { return ((data[row] >> (CC_WIDTH-col-1)) & 1) == 1; } // gets the data for a single pixel
        public void SetData(byte[] rows) { Array.Copy(rows, data, 8); Invalidate(); } // sets the entire character
        public void SetRowData(int row, byte value) { data[row] = value; Invalidate(); } // sets a row of data
        public void SetPixelData(int row, int col, bool on) // sets a single pixel of data
        {
            col = CC_WIDTH-col-1;
            if (on)	data[row] |= (byte)(  1 << col );
            else	data[row] &= (byte)(~(1 << col));
            Invalidate();
        }
        public void InvertPixel(int row, int col) // switches a single pixel from on to off and off to on
        {
            bool x = GetPixelData(row, col);
            col = CC_WIDTH-col-1;
            if (!x)	data[row] |= (byte)(  1 << col );
            else	data[row] &= (byte)(~(1 << col));
            Invalidate();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            // TODO: make more C#-esque
            //UserControl.OnPaint(e);
            Graphics g = e.Graphics;
            Brush bg = new SolidBrush(this.Enabled ? BackColor : SystemColors.Control);
            Brush fg = new SolidBrush(this.Enabled ? ForeColor : SystemColors.GrayText);
            g.FillRectangle(bg, 0, 0, CC_WIDTH*10, 80);
            for (int y = 0; y < 8; y++)
                for (int x = 0; x < CC_WIDTH; x++) {
                    bool on = ((data[y] >> (CC_WIDTH-x-1)) & 1) == 1;
                    if (this.Enabled && y == active_r && x == active_c) // highlight pixel
                    {
                        if (on) g.FillRectangle(Brushes.Green, x*10, y*10, 10, 10); 
                        else    g.FillRectangle(Brushes.GreenYellow, x*10, y*10, 10, 10);
                    }
                    else if (on)
                    {
                        g.FillRectangle(fg, x*10, y*10, 10, 10);
                    }
                }
            if (this.Focused && this.Enabled) // draw focus rectangle
            {
                using (Pen p = new Pen(fg, 1))
                {
                    p.DashPattern = new float[]{1.0f, 1.0f};
                    g.DrawRectangle(p, 0, 0, CC_WIDTH*10-1, 79);
                }
            }
            bg.Dispose();
            fg.Dispose();
        }
        protected override void OnGotFocus(EventArgs e)       { Invalidate(); base.OnGotFocus(e);       } //needed to redraw border
        protected override void OnLostFocus(EventArgs e)      { Invalidate(); base.OnLostFocus(e);      }
        protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); } //needed to change colors to disabled
        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (this.Enabled)
            {
                if (e.Button == MouseButtons.Right)
                {
                    // show data
                    MessageBox.Show(this, data[0]+","+data[1]+","+data[2]+","+data[3]+","+data[4]+","+data[5]+","+data[6]+","+data[7], "Custom Character Data");
                }
                else
                {
                    // invert a pixel
                    int c = e.X / 10;
                    int r = e.Y / 10;
                    if (c < CC_WIDTH && r < 8 && c >= 0 && r >= 0)
                        InvertPixel(r, c);
                }
            }
            base.OnMouseClick(e);
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            // during of highlighting
            active_c = -1;
            active_r = -1;
            Invalidate();
            base.OnMouseLeave(e);
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            // highlight a pixel
            int c = e.X / 10;
            int r = e.Y / 10;
            if (c < CC_WIDTH && r < 8 && c >= 0 && r >= 0)
            {
                active_c = c;
                active_r = r;
            }
            else
            {
                active_c = -1;
                active_r = -1;
            }
            Invalidate();
            base.OnMouseMove(e);
        }
    }
}
