// usb2lcd-util.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "usb2lcd-util.h"
#include "mpusbapi.h"

#define PIPE_NAME L"\\MCHP_EP1"

/////////////////////////////////////////////// Bootloader ////////////////////////////////////

#define SENDRCV_FAILED			0
#define SENDRCV_SUCCESS			1
#define SENDRCV_SUCCESS_SHORT	2 // success, but received data is short of expected

// from boot.h in the bootloader
#define READ_VERSION	0x00
#define WRITE_FLASH     0x02
//#define RESET			0xFF

static DWORD SendReceivePacket(HANDLE in, HANDLE out, BYTE *send, DWORD sendLen, BYTE *rcv, DWORD *rcvLen, UINT sendDelay, UINT rcvDelay) {
	DWORD sentLen = 0;
	DWORD expRcvLength = *rcvLen;
	if(MPUSBWrite(out, send, sendLen, &sentLen, sendDelay) && MPUSBRead(in, rcv, expRcvLength, rcvLen, rcvDelay)) {
		if(*rcvLen == expRcvLength) {
			return SENDRCV_SUCCESS;
		} else if(*rcvLen < expRcvLength) {
			return SENDRCV_SUCCESS_SHORT;
		}
	}
	return SENDRCV_FAILED;
}

static VersionInfo GetBLVersion(HANDLE in, HANDLE out) {
	BYTE rcv[6], send[2] = {READ_VERSION, 2}; //command, length
	DWORD len = 6;
	VersionInfo v;
	v._all = COMMUNICATION_FAILED;
	if (SendReceivePacket(in, out, send, 2, rcv, &len, 1000, 1000) == SENDRCV_SUCCESS && len == 6 && rcv[0] == READ_VERSION && rcv[1] == 0x02) {
		v.serialnumber = rcv[2] | rcv[3] << 8;
		v.module = rcv[4];
		v.version = rcv[5];
	}
	return v;
}

VersionInfo GetBootloaderVersion(int id) {
	HANDLE in = MPUSBOpen(id, USB_BL_ID, PIPE_NAME, MP_READ, 0);
	HANDLE out = MPUSBOpen(id, USB_BL_ID, PIPE_NAME, MP_WRITE, 0);
	VersionInfo v;
	v._all = COULD_NOT_OPEN_DEVICE;
	if (in != INVALID_HANDLE_VALUE && out != INVALID_HANDLE_VALUE) {
		v = GetBLVersion(in, out);
		MPUSBClose(in);
		MPUSBClose(out);
	}
	return v;
}

static int SendData(BYTE *data, DWORD len, DWORD addr, PROGRESS_CB progress, HANDLE in, HANDLE out) {
	DWORD i, rLen = 7, len_64 = len / 64 + ((len % 64) ? 1 : 0);
	int x = 0;
	BYTE rcv[7], send[64];

	if (addr % 32 != 0) { return ADDR_MUST_BE_MULT_32; }

	send[0] = WRITE_FLASH;
	send[1] = 7;
	send[2] = addr & 0xFF; /* low */
	send[3] = (addr >> 8) & 0xFF; /* high */
	send[4] = (addr >> 16) & 0x1F; /* upper */
	send[5] = len_64 & 0xFF;
	send[6] = (len_64 >> 8) & 0xFF;

	if (SendReceivePacket(in, out, send, 7, rcv, &rLen, 1000, 1000) == SENDRCV_FAILED) { return COMMUNICATION_FAILED; }

	for (i = 0, x = 0; i < len; i++, x++) {
		send[x] = data[i];
		if (x == 63) {
			x = -1;
			rLen = 7;
			if (SendReceivePacket(in, out, send, 64, rcv, &rLen, 1000, 1000) == SENDRCV_FAILED) { return COMMUNICATION_FAILED; }
			/*addr = (rcv[2] << 0) | (rcv[3] << 8) | (rcv[4] << 16);
			len_64 = (rcv[5] << 0) | (rcv[6] << 8);
			printf("a: %04x  c: %3d  ", addr, len_64);
			for (int j = 0; j < 64; j++) {
				printf("%02X", send[j]);
			}
			printf("\n");*/
			progress(i, len);
		}
	}
	if (x != 0) {
		memset(send+x, FILLER_BYTE, 64-x);
		rLen = 7;
		if (SendReceivePacket(in, out, send, 64, rcv, &rLen, 1000, 1000) == SENDRCV_FAILED) { return COMMUNICATION_FAILED; }
	}
	progress(len, len);
	return SUCCESS;
}

/*static int SendData(BYTE *data, DWORD len, DWORD addr, PROGRESS_CB progress, int id) {
	HANDLE in = MPUSBOpen(id, USB_BL_ID, PIPE_NAME, MP_READ, 0);
	HANDLE out = MPUSBOpen(id, USB_BL_ID, PIPE_NAME, MP_WRITE, 0);
	if (in != INVALID_HANDLE_VALUE && out != INVALID_HANDLE_VALUE) {
		int x = SendData(data, len, addr, progress, in, out);
		MPUSBClose(in);
		MPUSBClose(out);
		return x;
	}
	return COULD_NOT_OPEN_DEVICE;
}*/

/*bool Reset(HANDLE out) {
	BYTE send[2] = {RESET, 2};
	DWORD sentLen = 0;
	return MPUSBWrite(out, send, 2, &sentLen, 1000) ? true : false;
}

bool ResetBootloader(int id) {
	HANDLE out = MPUSBOpen(id, USB_BL_ID, PIPE_NAME, MP_WRITE, 0);
	if (out != INVALID_HANDLE_VALUE) {
		bool b = Reset(out);
		MPUSBClose(out);
		return b;
	}
	return false;
}*/

#define HEX_T_DATA	0x00
#define HEX_T_EOF	0x01
#define HEX_T_EXT	0x04

#define SumBYTE(x) (x & 0xFF)
#define SumWORD(x) ((x & 0xFF) + ((x >> 8) & 0xFF))

#define MAX_ADDR		0x7D00
#define MAX_SIZE		(MAX_ADDR-STARTING_POINT)

typedef unsigned int BYTE_;
typedef unsigned int WORD_;

/**
Reads a HEX file into buffer, marking the bytes written in mask.
len is the maximum length that can be written to buffer or buMask.
file is the filename to read.
Returns COULD_NOT_OPEN_FILE or CHECKSUM_FAILED in case of problems reading the file.
Any other negative value indicates the buffers were to small to fit the data and the magnitude of the value is how many bytes are required.
A positive value indicates how many bytes were written to the buffers.
The first byte written to the buffer is actually STARTING_POINT, not 0. All bytes in the HEX file before STARTING_POINT are ignored.
This will ignore all EEPROM bytes and CONFIG bits from the HEX file.
**/
static long ReadHEXfile(BYTE *buffer, BOOL *buMask, DWORD len, WCHAR *file) {
	FILE *f = _wfopen(file, L"r");
	
	unsigned int i;

	//:BBAAAATTHHHH...HHHHCC
	BYTE data[MAX_ADDR];
	BOOL mask[MAX_ADDR];
	BYTE_ b; // number of bytes in h row, max 0x10 (16)
	WORD_ a; // address
	BYTE_ t; // type of entry (HEX_T_XXX)
	BYTE_ h[16]; // data, 16 bytes max
	BYTE_ c; // checksum (two's complement of sum of all the other bytes)
	WORD_ max_a = STARTING_POINT;
	
	if (!f) { return COULD_NOT_OPEN_FILE; }
	memset(mask, FALSE, MAX_ADDR);

	while (!feof(f)) {
		BYTE c_ = 0;
		if (fwscanf(f, L":%2x%4x%2x", &b, &a, &t) != 3) {
			fclose(f);
			return NOT_HEX_FORMAT;
		}
		c_ = SumBYTE(b) + SumWORD(a) + SumBYTE(t);
		for (i = 0; i < b; i++) {
			if (fwscanf(f, L"%2x", h+i) != 1) {
				fclose(f);
				return NOT_HEX_FORMAT;
			}
			c_ += SumBYTE(h[i]);
		}
		if (fwscanf(f, L"%2x\n", &c) != 1) {
			fclose(f);
			return NOT_HEX_FORMAT;
		}
		if (c + c_ != 0x100 && !(c == 0 && c_ == 0)) {
			fclose(f);
			return CHECKSUM_FAILED;
		}
		if (t == HEX_T_EXT) {
			// skip
		} else if (t == HEX_T_EOF) {
			break; // done with file
		} else if (t == HEX_T_DATA) {
			// data byte, only care about > the starting point
			if (a >= STARTING_POINT) {
				if (a+b > max_a) { max_a = a+b; }
				for (i = 0; i < b; i++) {
					data[a+i] = (BYTE)h[i];
					mask[a+i] = TRUE;
				}
			}
		} // else unknown, do nothing
	}

	if (len < max_a - STARTING_POINT) {
		return STARTING_POINT - max_a;
	}

	for (i = STARTING_POINT; i < max_a; i++) {
		if (mask[i])
			buffer[i - STARTING_POINT] = data[i];
		buMask[i - STARTING_POINT] = mask[i];
	}

	fclose(f);
	return max_a - STARTING_POINT;
}

/*
Does the exact same thing as readHEXfile except that none of the data is saved.
It essentially checks that the file can be read, is in the HEX format, and all the checksums work.
If all checks pass, it returns the required size of the buffers in readHEXfile.
*/
long CheckHEXfile(WCHAR *file) {
	FILE *f = _wfopen(file, L"r");
	BYTE_ b, t, c, h[16];
	WORD_ a; // address
	WORD_ max_a = STARTING_POINT;
	if (!f) { return COULD_NOT_OPEN_FILE; }
	while (!feof(f)) {
		BYTE c_ = 0;
		BYTE_ i;
		if (fwscanf(f, L":%2x%4x%2x", &b, &a, &t) != 3) { fclose(f); return NOT_HEX_FORMAT; }
		c_ = SumBYTE(b) + SumWORD(a) + SumBYTE(t);
		for (i = 0; i < b; i++) {
			if (fwscanf(f, L"%2x", h+i) != 1) { fclose(f); return NOT_HEX_FORMAT; }
			c_ += SumBYTE(h[i]);
		}
		if (fwscanf(f, L"%2x\n", &c) != 1) { fclose(f); return NOT_HEX_FORMAT; }
		if (c + c_ != 0x100 && !(c == 0 && c_ == 0)) { fclose(f); return CHECKSUM_FAILED; }
		if (t == HEX_T_EOF) break;
		if (t == HEX_T_DATA && a+b > max_a) { max_a = a+b; }
	}
	fclose(f);
	return max_a - STARTING_POINT;
}

int UpdateFirmware(int id, WCHAR *file, PROGRESS_CB progress) {
	BYTE d[MAX_SIZE];
	BOOL m[MAX_SIZE];
	long retval;
	HANDLE in, out;
	memset(d, FILLER_BYTE, MAX_SIZE);
	memset(m, FALSE, MAX_SIZE);
	retval = ReadHEXfile(d, m, MAX_SIZE, file);
	if (retval < 0) {
		if (retval == COULD_NOT_OPEN_FILE || retval == CHECKSUM_FAILED)
			return retval;
		// we didn't give it enough room? what?
		return BUFFER_OVERFLOW;
	}
	in = MPUSBOpen(id, USB_BL_ID, PIPE_NAME, MP_READ, 0);
	out = MPUSBOpen(id, USB_BL_ID, PIPE_NAME, MP_WRITE, 0);
	if (in != INVALID_HANDLE_VALUE && out != INVALID_HANDLE_VALUE) {
		int x = SendData(d, retval, STARTING_POINT, progress, in, out);
		//BOOL b = (x == SUCCESS) ? reset(out) : FALSE;
		MPUSBClose(in);
		MPUSBClose(out);
		return x != SUCCESS ? x : SUCCESS; //b ? SUCCESS : RESET_FAILED;
	}
	return COULD_NOT_OPEN_DEVICE;
}

/**
Finds a USB2LCD that is in boot-loader mode.
Returns an ID upon finding it, otherwise NOT_FOUND.
**/
int* GetUSB2LCDBootloaders(int* _size) {
	DWORD max_count = MPUSBGetDeviceCount(USB_BL_ID), count = 0, i;
	VersionInfo v;
	int* ids = (int*)memset(malloc(16*sizeof(int)), 0, 16*sizeof(int));
	int size = 0, cap = 16;
	for (i = 0; i < MAX_NUM_MPUSB_DEV && count < max_count; i++) {
		v = GetBootloaderVersion(i);
		if (v._all != COULD_NOT_OPEN_DEVICE) {
			if (v.module == 0x5B) {
				if (size == cap) {
					ids = (int*)realloc(ids, cap*2*sizeof(int));
					memset(ids + cap, 0, cap*sizeof(int));
					cap *= 2;
				}
				ids[size++] = i;
			}
			count++;
		}
	}
	*_size = size;
	return ids;
}

void FreeUSB2LCDBootloaders(int *list) {
	free(list);
}


///////////////////// COM /////////////////////////////////////////

#define MAX_KEY_LEN	256
#define BUF_SIZE	1024

static wchar_t* MatchExists(HKEY keyCom, wchar_t *serv, wchar_t *port, DWORD* _err) {
	wchar_t name[BUF_SIZE], data[BUF_SIZE];
	DWORD name_len, data_len, type, j, err;
	for (j = 0; ; ++j) {
		name_len = ARRAYSIZE(name);
		data_len = sizeof(data);
		err = RegEnumValue(keyCom, j, name, &name_len, NULL, &type, (LPBYTE)data, &data_len);
		if (err == ERROR_NO_MORE_ITEMS) { break; } // not found
		if (err != ERROR_SUCCESS) { *_err = (DWORD)ERROR_REG_QUERY_VALUE; break; }
		if (_wcsicmp(data, port) == 0) { // found the port
			return _wcsnicmp(name, data, swprintf(data, BUF_SIZE, L"\\Device\\%s", serv)) == 0 ? _wcsdup(port) : NULL; // check the service
		}
	}
	return NULL;
}

wchar_t** GetUSB2LCDComs(int* _size) {
	DWORD err;
	HKEY keyUsb, keyCom, keyDev, keyParam;

	wchar_t key_name[BUF_SIZE];

	wchar_t id[MAX_KEY_LEN], serv[BUF_SIZE], port[BUF_SIZE];
	DWORD id_len, serv_len, port_len;

	DWORD type = 0, i;

	wchar_t** coms, *com;
	int size = 0, cap = 16;
	
	// Get the key containing all of the USB2LCD+ devices
	swprintf(key_name, BUF_SIZE, L"SYSTEM\\CurrentControlSet\\Enum\\USB\\%s", USB_COM_ID);
	if ((err = RegOpenKeyEx(HKEY_LOCAL_MACHINE, key_name, 0, KEY_ENUMERATE_SUB_KEYS, &keyUsb)) != ERROR_SUCCESS) {
		*_size = err == ERROR_FILE_NOT_FOUND ? ERROR_NO_DEVICES : ERROR_REG_OPEN_KEY; 
		return NULL;
	}

	// Get the listing of current serial devices
	if ((err = RegOpenKeyEx(HKEY_LOCAL_MACHINE, L"HARDWARE\\DEVICEMAP\\SERIALCOMM", 0, KEY_QUERY_VALUE, &keyCom)) != ERROR_SUCCESS) {
		RegCloseKey(keyUsb);
		*_size = err == ERROR_FILE_NOT_FOUND ? ERROR_NO_COM_PORTS : ERROR_REG_OPEN_KEY; 
		return NULL;
	}

	coms = (wchar_t**)memset(malloc(16*sizeof(wchar_t*)), 0, 16*sizeof(wchar_t*));

	// Go through each device in the list
	for (i = 0; ; ++i) {
		// Get the next device
		id_len = ARRAYSIZE(id);
		err = RegEnumKeyEx(keyUsb, i, id, &id_len, NULL, NULL, NULL, NULL);
		if (err == ERROR_NO_MORE_ITEMS) { break; }
		if (err) { /*error!*/ break; }

		// Get the service name
		serv_len = sizeof(serv);
		if ((err = RegOpenKeyEx(keyUsb, id, 0, KEY_QUERY_VALUE, &keyDev)) != ERROR_SUCCESS) { /*error!*/ continue; }
		if ((err = RegQueryValueEx(keyDev, L"Service", NULL, &type, (LPBYTE)serv, &serv_len)) != ERROR_SUCCESS || type != REG_SZ) { /*error!*/ RegCloseKey(keyDev); continue; }

		// Get the port name
		port_len = sizeof(port);
		if ((err = RegOpenKeyEx(keyDev, L"Device Parameters", 0, KEY_QUERY_VALUE, &keyParam)) != ERROR_SUCCESS) { /*error!*/ RegCloseKey(keyDev); continue; }
		err = RegQueryValueEx(keyParam, L"PortName", NULL, &type, (LPBYTE)port, &port_len);
		
		// Close keys opened so far
		RegCloseKey(keyParam);
		RegCloseKey(keyDev);

		if (err != ERROR_SUCCESS || type != REG_SZ) { /*error!*/ continue; }

		// See if that port is actually active and associated with the service
		if ((com = MatchExists(keyCom, serv, port, &err)) != NULL) {
			// Add it to the list
			if (size == cap) {
				coms = (wchar_t**)realloc(coms, cap*2*sizeof(wchar_t*));
				memset(coms + cap, 0, cap*sizeof(wchar_t*));
				cap *= 2;
			}
			coms[size++] = com;
		}
	}
	RegCloseKey(keyCom);
	RegCloseKey(keyUsb);
	*_size = size;
	return coms;
}

void FreeUSB2LCDComs(wchar_t** list) {
	int i;
	for (i = 0; list[i]; ++i)
		free(list[i]);
	free(list);
}
