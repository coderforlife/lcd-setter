#pragma once

// This allows us to use this file for imports or exports
#ifdef USB2LCDUTIL_EXPORTS
#define USB2LCDUTIL_API(type) __declspec(dllexport) type __cdecl
#else
#define USB2LCDUTIL_API(type) __declspec(dllimport) type __cdecl
#endif

// VID and PID identifiers for COM and Bootloader modes
#define USB_COM_ID	L"VID_5BCD&PID_5BCD"
#define USB_BL_ID	L"vid_04d8&pid_000b"

// Error messages returned by these functions
#define ERROR_REG_OPEN_KEY			-101
#define ERROR_REG_QUERY_VALUE		-102
#define ERROR_REG_UNEXPECTED_TYPE	-103
#define ERROR_NO_DEVICES			-104
#define ERROR_NO_COM_PORTS			-105

typedef union _VersionInfo {
	long _all;
	struct {
		unsigned short serialnumber;
		unsigned char module;
		unsigned char version;
	};
} VersionInfo;

/////////////// Device Finding Functions //////////////////////////////////////
// These functions find the USB2LCD device in its different modes
///////////////////////////////////////////////////////////////////////////////

// Gets all USB2LCD devices in bootloader mode.
// If error, returns NULL and sets size to an error code
USB2LCDUTIL_API(int*) GetUSB2LCDBootloaders(int* size);

// Required to free the memory
USB2LCDUTIL_API(void) FreeUSB2LCDBootloaders(int* list);

// Gets all USB2LCD devices in COM mode.
// If error, returns NULL and sets size to an error code
USB2LCDUTIL_API(wchar_t**) GetUSB2LCDComs(int* size);

// Required to free the memory
USB2LCDUTIL_API(void) FreeUSB2LCDComs(wchar_t** list);


/////////////// COM Mode Functions ////////////////////////////////////////////
// There are no functions. Use any COM port implementation you wish.
// Provided here are all the commands supported in COM mode. Remember to
// always send all the required data and read all the data otherwise you may
// cause a device lock. Also remember that before doing a function you will
// read the result of get rid of all data currently in the stream (button
// presses are always being sent).
///////////////////////////////////////////////////////////////////////////////
#define Firmware			  1u // Causes device to reset into a firmware / bootloader mode
//#define ResetDevice			  2u
#define ReadDisplay			  3u // Returns 1 byte, with the 3 LSB being the current display, cursor and blink
#define ReadDisplayMin		  4u // Returns 1 byte, the current remaining time till display off, or 0
#define ReadContrast		  5u // Returns 1 byte, the current contrast level
#define ReadBacklight		  6u // Returns 1 byte, the current backlight level
#define ReadCustom			  7u // [char:0-7] Returns 8 bytes, the 8 bytes of the current char
#define ReadMessage			  8u // Returns 80 bytes, the current message
#define ReadGPO				  9u // [1-5] Returns 1 byte, the current state of the GPO
#define ReadGPOpwm			 10u // [1-5] Returns 1 byte, the current pwm of the GPO
#define ReadSavedDisplay	 13u // Returns 1 byte, with the 3 LSB being the saved display, cursor and blink
#define ReadSavedDisplayMin	 14u // Returns 1 byte, the saved time till display off, or 0
#define ReadSavedContrast	 15u // Returns 1 byte, the saved contrast level
#define ReadSavedBacklight	 16u // Returns 1 byte, the saved backlight level
#define ReadSavedCustom		 17u // [char:0-7] Returns 8 bytes, the 8 bytes of the saved char
#define ReadSavedMessage	 18u // Returns 160 bytes, the saved startup message
#define ReadSavedGPO		 19u // [1-5] Returns 1 byte, the current state of the GPO
#define ReadSavedGPOpwm		 20u // [1-5] Returns 1 byte, the current pwm of the GPO
#define SetLargeDisplay		 21u // [0-1] 1 for a display that is large (>80 characters); this setting is always remembered
#define IsLargeDisplay		 22u // Return 1 byte, 1 for large display, 0 otherwise
#define SetSerialNum		 52u // [2 bytes], can be called any number of times
#define ReadSerialNum		 53u // Returns 2 bytes
#define ReadVersion			 54u // Returns 1 byte, the version of the firmware (major version in high nibble, minor version in low nibble)
#define ReadModuleType		 55u // Returns 1 byte, exactly 0x5B to identify this software
#define SaveStartup			 64u // [160 chars] (spec says 40, but we want to be able to use 40x4)
#define DisplayOn			 66u // [mins:0-100]
#define DisplayOff			 70u
#define Position			 71u // [col][row]
#define Home				 72u
#define CursorOn			 74u
#define CursorOff			 75u
#define CursorLeft			 76u
#define CursorRight			 77u
#define DefineCustom		 78u // [char:0-7][8 bytes]
#define Contrast			 80u // [0-255]
#define BlinkOn				 83u
#define BlinkOff			 84u
#define GPOoff				 86u // [1-5]
#define GPOon				 87u // [1-5]
#define ClearDisplay		 88u
#define Backlight_			 89u // [0-255], duplicate of 152
#define GPOpwm				102u // [1-5][0-255]
#define SaveBacklight		145u // [0-255]
#define Remember			147u // [0-1]
#define Backlight			152u // [0-255]
#define GPOpwm_				192u // [1-5][0-255], duplicate of 102
#define ReadButton			193u // [1-5], returns one character A-E (this is originally for reading fan RPM) or X if not pressed
#define RememberCustom		194u // [char:0-7][8 bytes]
#define RememberGPOpwm		195u // [1-5][0-255]
#define RememberGPO			196u // [1-5][0-1]
#define Char254				254u


/////////////// Bootloader Mode Functions /////////////////////////////////////
// While in bootloader mode the device is considerably more difficult to work
// with. These functions act to simplify that work. However they are simply
// wrappers for the MPUSBAPI functions (which in turn are just wrappers for
// the Windows DeviceIoControl and other native calls).
///////////////////////////////////////////////////////////////////////////////
#define SUCCESS					0
#define BUFFER_OVERFLOW			-1
#define COULD_NOT_OPEN_FILE		-2
#define NOT_HEX_FORMAT			-3
#define CHECKSUM_FAILED			-4
#define COULD_NOT_OPEN_DEVICE	-5
#define ADDR_MUST_BE_MULT_32	-6
#define COMMUNICATION_FAILED	-7
//#define RESET_FAILED			-8

typedef void (__stdcall *PROGRESS_CB)(int step, int total);
#define FILLER_BYTE		0xFF // or 0x00?
#define STARTING_POINT	0x800

// The all-in-one function for updating the firmware.
// Calls ReadHEXfile and SendData.
// Returns SUCCESS upon successfully reading the HEX file, updating the device.
// Upon failure, returns any of the errors listed above.
USB2LCDUTIL_API(int) UpdateFirmware(int id, WCHAR *file, PROGRESS_CB progress);

// Reads a HEX file into buffer, marking the bytes written in mask.
// len is the maximum length that can be written to buffer or buMask.
// file is the filename to read.
// Returns COULD_NOT_OPEN_FILE, NOT_HEX_FORMAT, or CHECKSUM_FAILED in case of problems reading the file.
// Any other negative value indicates the buffers were to small to fit the data and the magnitude of the value is how many bytes are required.
// A positive value indicates how many bytes were written to the buffers.
// The first byte written to the buffer is actually STARTING_POINT, not 0. All bytes in the HEX file before STARTING_POINT are ignored.
// This will ignore all EEPROM bytes and CONFIG bits from the HEX file.
//long ReadHEXfile(BYTE *buffer, bool *buMask, DWORD len, WCHAR *file);

// Same as readHEXfile except does not save data, but does all the checks (file is readable, format is correct, checksums work).
// Returns the required size of the buffer to store the data or an error (COULD_NOT_OPEN_FILE, NOT_HEX_FORMAT, or CHECKSUM_FAILED).
USB2LCDUTIL_API(long) CheckHEXfile(WCHAR *file);

// Returns COMMUNICATION_FAILED on failure, >=0 is version returned by device (byte 2 is major, byte 1 is minor)
// Returns COULD_NOT_OPEN_DEVICE if it could not open device id, otherwise see other getVersion().
USB2LCDUTIL_API(VersionInfo) GetBootloaderVersion(int id);

// Returns SUCCESS on successfully sending the data.
// Failures: ADDR_MUST_BE_MULT_32 or COMMUNICATION_FAILED
// The progress callback may be NULL. The function is called periodically to update the caller the progress of updating the device.
// Returns COULD_NOT_OPEN_DEVICE if it could not open device id, otherwise see other sendData().
//int SendData(BYTE *data, DWORD len, DWORD addr, PROGRESS_CB, int id);

// Resets the device
// Returns true on success, false on failure
//USB2LCDUTIL_API(bool) ResetBootloader(int id);
