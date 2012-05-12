using System;
using System.Windows.Forms;

namespace LCD.Setter
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // Enabling Windows XP visual effects before any controls are created
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Get the device to use
            DeviceChooser dc = new DeviceChooser();
            if (dc.ShowDialog() == DialogResult.OK)
            {
                ProgressDialog.CreateForm();
                // Create the main window and run it
                Device d = dc.device;
                if (d.type == DeviceType.Bootloader)
                {
                    Application.DoEvents();
                    if (MessageBox.Show("For bootloader devices you can only update the firmware. Do you wish to update the firmware of this device?", "Update Firmware", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        OpenFileDialog openFileDialog = new OpenFileDialog();
                        openFileDialog.Filter = "HEX File (*.hex)|*.hex";
                        openFileDialog.RestoreDirectory = true;
                        if (openFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            Application.DoEvents();
                            int retval = USB2LCD.CheckHEXfile(openFileDialog.FileName);
                            if (retval < 0)
                            {
                                MessageBox.Show("The selected file could not be read / parsed: "+retval, "File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            else
                            {
                                ProgressDialog.ShowNonModal("Firmware Update", "Updating firmware...");
                                ProgressDialog.SetProgressStyle(false);
                                Application.DoEvents();
                                retval = USB2LCD.UpdateFirmware(d.blID, openFileDialog.FileName, ProgressDialog.SetProgress);
                                ProgressDialog.CloseForm();
                                Application.DoEvents();
                                if (retval < 0)
                                {
                                    MessageBox.Show("The firmware failed to update (error: "+retval+").\nTry running LCD Setter again.","Firmware Update Problem");
                                }
                                else
                                {
                                    MessageBox.Show("The firmware was successfully updated.\n\nYou must unplug the device and plug it back in to complete the update.\n\nSome saved settings may have changed during this process.","Firmware Updated Successfully");
                                }
                            }
                        }
                    }
                }
                else
                {
                    Application.Run(new Setter(d.type == DeviceType.USB2LCD, d.id));
                }
            }
        }
    }
}
