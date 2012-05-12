using System;
using System.Runtime.InteropServices;

namespace LCD.Setter
{
    static class USB2LCD
    {
        // Error messages returned by these functions
        public const int ERROR_REG_OPEN_KEY        = -101;
        public const int ERROR_REG_QUERY_VALUE     = -102;
        public const int ERROR_REG_UNEXPECTED_TYPE = -103;
        public const int ERROR_NO_DEVICES          = -104;
        public const int ERROR_NO_COM_PORTS        = -105;

        [StructLayout(LayoutKind.Sequential)]
        public struct VersionInfo
        {
            public ushort serialnumber;
            public byte module;
            public byte version;
        };

        public class Exception : System.Exception
        {
            public int error;
            public Exception(int error) { this.error = error; }
        }

        /////////////// Device Finding Functions //////////////////////////////////////
        // These functions find the USB2LCD device in its different modes
        ///////////////////////////////////////////////////////////////////////////////

        // Gets all USB2LCD devices in bootloader mode.
        // If error, returns NULL and sets size to an error code
        [DllImport("usb2lcd-util")] private static extern IntPtr GetUSB2LCDBootloaders(ref int count);
        [DllImport("usb2lcd-util")] private static extern void FreeUSB2LCDBootloaders(IntPtr list);
        public static int[] GetBootloaderIds()
        {
            int count = 0;
            IntPtr _ids = GetUSB2LCDBootloaders(ref count);
            if (_ids == IntPtr.Zero) { throw new USB2LCD.Exception(count); }
            int[] ids = new int[count];
            Marshal.Copy(_ids, ids, 0, count);
            FreeUSB2LCDBootloaders(_ids);
            return ids;
        }

        [DllImport("usb2lcd-util")] private static extern IntPtr GetUSB2LCDComs(ref int count);
        [DllImport("usb2lcd-util")] private static extern void FreeUSB2LCDComs(IntPtr list);
        public static string[] GetCOMPorts()
        {
            int count = 0;
            IntPtr _coms = GetUSB2LCDComs(ref count);
            if (_coms == IntPtr.Zero)
            {
                if (count == ERROR_NO_DEVICES || count == ERROR_NO_COM_PORTS) { return new string[0]; }
                throw new USB2LCD.Exception(count);
            }
            string[] coms = new string[count];
            for (int i = 0; i < count; ++i)
                coms[i] = Marshal.PtrToStringUni(Marshal.ReadIntPtr(_coms, i * IntPtr.Size));
            FreeUSB2LCDComs(_coms);
            return coms;
        }

        /////////////// COM Mode Functions ////////////////////////////////////////////
        // There are no functions. Use any COM port implementation you wish.
        // Provided here are all the commands supported in COM mode. Remember to
        // always send all the required data and read all the data otherwise you may
        // cause a device lock. Also remember that before doing a function you will
        // read the result of get rid of all data currently in the stream (button
        // presses are always being sent).
        ///////////////////////////////////////////////////////////////////////////////
        public enum Command : byte
        {
            Firmware            =   1, // Causes device to reset into a firmware / bootloader mode
            //ResetDevice         =   2,
            ReadDisplay         =   3, // Returns 1 byte, with the 3 LSB being the current display, cursor and blink
            ReadDisplayMin      =   4, // Returns 1 byte, the current remaining time till display off, or 0
            ReadContrast        =   5, // Returns 1 byte, the current contrast level
            ReadBacklight       =   6, // Returns 1 byte, the current backlight level
            ReadCustom          =   7, // [char:0-7] Returns 8 bytes, the 8 bytes of the current char
            ReadMessage         =   8, // Returns 80 bytes, the current message
            ReadGPO             =   9, // [1-5] Returns 1 byte, the current state of the GPO
            ReadGPOpwm          =  10, // [1-5] Returns 1 byte, the current pwm of the GPO
            ReadSavedDisplay    =  13, // Returns 1 byte, with the 3 LSB being the saved display, cursor and blink
            ReadSavedDisplayMin =  14, // Returns 1 byte, the saved time till display off, or 0
            ReadSavedContrast   =  15, // Returns 1 byte, the saved contrast level
            ReadSavedBacklight  =  16, // Returns 1 byte, the saved backlight level
            ReadSavedCustom     =  17, // [char:0-7] Returns 8 bytes, the 8 bytes of the saved char
            ReadSavedMessage    =  18, // Returns 160 bytes, the saved startup message
            ReadSavedGPO        =  19, // [1-5] Returns 1 byte, the current state of the GPO
            ReadSavedGPOpwm     =  20, // [1-5] Returns 1 byte, the current pwm of the GPO
            SetLargeDisplay     =  21, // [0-1] 1 for a display that is large (>80 characters); this setting is always remembered
            IsLargeDisplay      =  22, // Return 1 byte, 1 for large display, 0 otherwise
            SetSerialNum        =  52, // [2 bytes], can be called any number of times
            ReadSerialNum       =  53, // Returns 2 bytes
            ReadVersion         =  54, // Returns 1 byte, the version of the firmware (major version in high nibble, minor version in low nibble)
            ReadModuleType      =  55, // Returns 1 byte, exactly 0x5B to identify this software
            SaveStartup         =  64, // [160 chars] (spec says 40, but we want to be able to use 40x4)
            DisplayOn           =  66, // [mins:0-100]
            DisplayOff          =  70,
            Position            =  71, // [col][row]
            Home                =  72,
            CursorOn            =  74,
            CursorOff           =  75,
            CursorLeft          =  76,
            CursorRight         =  77,
            DefineCustom        =  78, // [char:0-7][8 bytes]
            Contrast            =  80, // [0-255]
            BlinkOn             =  83,
            BlinkOff            =  84,
            GPOoff              =  86, // [1-5]
            GPOon               =  87, // [1-5]
            ClearDisplay        =  88,
            Backlight_          =  89, // [0-255], duplicate of 152
            GPOpwm              = 102, // [1-5][0-255]
            SaveBacklight       = 145, // [0-255]
            Remember            = 147, // [0-1]
            Backlight           = 152, // [0-255]
            GPOpwm_             = 192, // [1-5][0-255], duplicate of 102
            ReadButton          = 193, // [1-5], returns one character A-E (this is originally for reading fan RPM) or X if not pressed
            RememberCustom      = 194, // [char:0-7][8 bytes]
            RememberGPOpwm      = 195, // [1-5][0-255]
            RememberGPO         = 196, // [1-5][0-1]
            Char254             = 254,
        };

        /////////////// Bootloader Mode Functions /////////////////////////////////////
        // While in bootloader mode the device is considerably more difficult to work
        // with. These functions act to simplify that work. However they are simply
        // wrappers for the MPUSBAPI functions (which in turn are just wrappers for
        // the Windows DeviceIoControl and other native calls).
        ///////////////////////////////////////////////////////////////////////////////
        public const int SUCCESS               = 0;
        public const int BUFFER_OVERFLOW       = -1;
        public const int COULD_NOT_OPEN_FILE   = -2;
        public const int NOT_HEX_FORMAT        = -3;
        public const int CHECKSUM_FAILED       = -4;
        public const int COULD_NOT_OPEN_DEVICE = -5;
        public const int ADDR_MUST_BE_MULT_32  = -6;
        public const int COMMUNICATION_FAILED  = -7;
        //public const int RESET_FAILED          = -8;

        public delegate void ProgressCallback(int step, int total);
        public const int FILLER_BYTE    = 0xFF; // or 0x00?
        public const int STARTING_POINT = 0x800;

        // The all-in-one function for updating the firmware.
        // Calls ReadHEXfile and SendData.
        // Returns SUCCESS upon successfully reading the HEX file, updating the device.
        // Upon failure, returns any of the errors listed above.
        [DllImport("usb2lcd-util", CharSet = CharSet.Unicode)]
        public static extern int UpdateFirmware(int id, string file, ProgressCallback progress);

        // Same as readHEXfile except does not save data, but does all the checks (file is readable, format is correct, checksums work).
        // Returns the required size of the buffer to store the data or an error (COULD_NOT_OPEN_FILE, NOT_HEX_FORMAT, or CHECKSUM_FAILED).
        [DllImport("usb2lcd-util", CharSet = CharSet.Unicode)]
        public static extern int CheckHEXfile(string file);

        // Returns COMMUNICATION_FAILED on failure, >=0 is version returned by device (byte 2 is major, byte 1 is minor)
        // Returns COULD_NOT_OPEN_DEVICE if it could not open device id, otherwise see other getVersion().
        [DllImport("usb2lcd-util")]
        public static extern VersionInfo GetBootloaderVersion(int id);
    }
}
