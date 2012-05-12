using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace LCD.Setter
{
    // A user control to extend the abilities of the text box.
    // It maps all displayed characters to some other characters for display
    // But all characters can be mapped back
    // Uses the chars.txt file if available
    class LCDText : TextBox
    {
        public static string GetStringFromBytes(byte[] bs)
        {
            StringBuilder sb = new StringBuilder(bs.Length);
            foreach (byte b in bs)
            {
                sb.Append((char)b);
            }
            return sb.ToString();
        }

        // Gets the actually character to display as
        public static string GetDisplayChar(char c) { return char_map[c]; }

        // Converts a whole string to the actual display characters
        public static string GetDisplayText(string text)
        {
            StringBuilder sb = new StringBuilder(text.Length);
            foreach (char c in text)
            {
                System.Diagnostics.Debug.WriteLine(((int)c) + " => " + char_map[c]);
                sb.Append(char_map[c]);
                //sb.Append(c > 126 ? c : char_map[c]);
            }
            return sb.ToString();
        }

        // Converts a display string back to the real characters to write to the LCD
        public static string UnGetDisplayText(string text)
        {
            StringBuilder sb = new StringBuilder(text.Length);
            TextElementEnumerator c = StringInfo.GetTextElementEnumerator(text);
            while (c.MoveNext())
            {
                sb.Append(char_map_[c.GetTextElement()]);
                //sb.Append(c > 126 ? char_map_[c] : c);
            }
            return sb.ToString();
        }

        static LCDText()
        {
            // Build the character map
            if (File.Exists("chars.txt"))
            {
                StringBuilder text = new StringBuilder(512);
                string[] lines = File.ReadAllLines("chars.txt", Encoding.Unicode);
                for (int i = 0; i < lines.Length; ++i)
                {
                    int tab = lines[i].IndexOf('\t');
                    text.Append((tab >= 0) ? lines[i].Remove(tab) : lines[i]);
                }

                int index = 0;
                TextElementEnumerator c = StringInfo.GetTextElementEnumerator(text.ToString());
                while (c.MoveNext() && index < char_map.Length)
                {
                    char_map[index++] = c.GetTextElement();
                }
            }

            // Build the reverse character map
            char_map_ = new Dictionary<string, char>();
            for (char i = '\0'; i < char_map.Length; ++i)
                char_map_[char_map[i]] = i;
        }
        private static readonly Dictionary<string, char> char_map_ = null; // the reverse char map (display to real)
        private static readonly string[] char_map = // the mapping of real (0-255) to display (any valid UNICODE) characters
        {
            "\x2460", "\x2461", "\x2462", "\x2463", "\x2464", "\x2465", "\x2466", "\x2467", // custom characters 1-8 (circled numbers)
            "\x2776", "\x2777", "\x2778", "\x2779", "\x277A", "\x277B", "\x277C", "\x277D", // custom characters 1-8 repeat (dark circled numbers)
            "\x00B1", "\x2261", // +/-, identity (triple equal)
            "\x2196", "\x2199", // sigma parts
            "\x256D", "\x2570", // left parenthesis parts
            "\x256E", "\x256F", // right parenthesis parts
            "\x0283", "\x0285", // curly brace parts
            "\x2248", "\x222B", "\x2017", "~", // congruent (2 squiggles), integral, double low line, squiggle
            "\x00B2", "\x00B3", // squared, cubed
            " ", "!", "\"", "#", "$", "%", "&", "'", "(", ")", "*", "+", ",", "-", ".", "/", // space and symbols
            "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", // 0-9
            ":", ";", "<", "=", ">", "?", "@", // symbols
            "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", // A-Z
            "[", "\\", "]", "^", "_", "`", // symbols
            "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", // a-z
            "{", "|", "}", "\x02DC", // symbols
            "\x2206", // delta (increment)
            "\xC7", // C with cedilla
            "\xFC", // u with diaeresis
            "\xE9", // e with acute
            "\xE2", "\xE4", "\xE0", "\xE5", // a with circumflex, diaeresis, grave, ring
            "\xE7", // c with cedilla
            "\xEA", "\xEB", "\xE8", // e with circumflex, diaeresis, grave
            "\xEF", "\xEE", "\xEC", // i with diaeresis, circumflex, grave
            "\xC4", "\xC2", // A with diaeresis, circumflex
            "\xC9", // E with acute
            "\xE6", "\xC6", // ae, AE
            "\xF4", "\xF6", "\xF2", // o with circumflex, diaeresis, grave
            "\xFB", "\xF9", // u with circumflex, grave
            "\xFF", "\xD6", "\xDC", // y/O/U with diaeresis
            "\xF1", "\xD1", // n/N with tilde
            "a\x0332", "o\x0332", // a/o with underline
            "\xBF", // inverted ?
            "\xE1", "\xED", "\xF3", "\xFA", // a/i/o/u with accute
            "\xA2", "\xA3", "\xA5", "\x20A7", "\x192", // cents, pounds, yen, Pts (Spanish currency), cursive f (florin, currency of Netherlands / Aruba)
            "\xA1", // inverted !
            "\xC3", "\xE3", //A/a with tilde
            "\xD5", "\xF5", //O/o with tilde
            "\xD8", "\xF8", // O/o with stroke
            "\x2D9", "\xA8", "\x2DA", "\x2CB", "\x2CA", //dot, diaeresis, ring, grave, acute
            "\xBD", "\xBC", // 1/2 and 1/4
            "\xD7", "\xF7", "\x2264", "\x2265", //multiply, divide, less than equal, greater than equal
            "\xAB", "\xBB", // double angle brackets
            "\x2260", "\x221A", "\x203E", // not equal, square root, overline
            "\x2320", "\x2321", // integral half symbols
            "\x221E", // infinity
            "\x21D6", "\x21B5", // triangle in corner (glyph is a double arrow to NW), new line
            "\x2191", "\x2193", "\x2192", "\x2190", // arrows up, down, right, left
            "\x250C", "\x2510", "\x2514", "\x2518", // corners: tl, tr, bl, br
            "\x2022", "\xAE", "\xA9", "\x2122", // bullet, registered, copyright, trademark
            "\x2020", "\xA7", "\xB6", // dagger, section, paragraph (pilcrow)
            "\x393", "\x394", "\x398", "\x39B", "\x39E", "\x3A0", "\x3A3", "\x3D2", "\x3A6", "\x3A8", "\x3A9", //Gamma, Delta?, Theta, Lambda, Xi, Pi, Sigma, Upsilon, Phi, Psi, Omega
            "\x3B1", "\x3B2", "\x3B3", "\x3B4", "\x3B5", "\x3B6", "\x3B7", "\x3B8", "\x3B9", "\x3BA", "\x3BB", "\x3BC", "\x3BD", "\x3BE", "\x3C0", "\x3C1", "\x3C3", "\x3C4", "\x3C5", "\x3C7", "\x3C8", "\x3C9", //alpha-omega, skips omicron and phi
            "\x25BC", "\x25BA", "\x25C4", // black arrow down, right, left
            "\x1D5E5", "\x21A4", "\x1D5D9", "\x21E5", // bold R, left arrow from , bold F, right arrow to bar
            "\x25A1", "\x25AC", // white square, black rectangle (thick hyphen)
            "\x1D54A", "\x2117", // s in box (glyph is double-struck S), p in box (glyph is p in circle)
        };

        // Overridden to convert tabs to spaces
        public override string Text { set { base.Text = value.Replace('\t', ' '); } }

        // Gets the real (not display) text for this text box.
        public string RealText { get { return UnGetDisplayText(this.Text); } set { this.Text = GetDisplayText(value); } }
        
        public LCDText()
        {
            this.SuspendLayout();
            this.Font = new Font("Consolas", 12.0F, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.HideSelection = false;
            //this.MaxLength = 20;
            this.ResumeLayout(false);
        }

        //protected override void OnKeyDown(KeyEventArgs e)
        //{
        //    base.OnKeyDown(e);
        //}

        //protected override void OnKeyPress(KeyPressEventArgs e)
        //{
        //    base.OnKeyPress(e);
        //}

        // TODO: needs great improvements
        private string previousText = "";
        protected override void OnTextChanged(EventArgs e)
        {
            StringInfo si = new StringInfo(this.Text);
            if (si.LengthInTextElements >= 20)
            {
                this.Text = previousText;
            }
            this.previousText = this.Text;
            base.OnTextChanged(e);
        }
    }
}
