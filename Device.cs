using System;

namespace LCD.Setter
{
    enum DeviceType
    {
        Bootloader = 1,
        USB2LCD = 2,
        COM = 3,
    }

    //Simple container for information about a device.
    struct Device
    {
        public readonly DeviceType type;
        public readonly string id;
        public readonly ushort serialnum;
        public readonly byte module;
        public readonly byte version;
        public Device(DeviceType type, int id, USB2LCD.VersionInfo version)
        {
            this.type = type;
            this.id = id.ToString();
            this.serialnum = version.serialnumber;
            this.module = version.module;
            this.version = version.version;
        }
        public Device(DeviceType type, string id, USB2LCD.VersionInfo version)
        {
            this.type = type;
            this.id = id;
            this.serialnum = version.serialnumber;
            this.module = version.module;
            this.version = version.version;
        }
        public int blID { get { return Convert.ToInt32(this.id); } }
        public override string ToString()
        {
            switch (type)
            {
                case DeviceType.Bootloader: return "USB2LCD+ in Bootloader #" + id + ", v" + (((version >> 4) & 0xF) + "." + (version & 0xF)) + ", ID: " + serialnum.ToString("X4") + ")";
                case DeviceType.USB2LCD:    return "USB2LCD+ (" + id + ", v" + (((version >> 4) & 0xF) + "." + (version & 0xF)) + ", ID: " + serialnum.ToString("X4") + ")";
                case DeviceType.COM:        return "Other (" + id + ")";
                default:		            return "Unknown ("+id+")";
            }
        }
    }
}
