/*********************************************************************
 *
 *                  MPUSBAPI Library Version 1.00
 *
 *********************************************************************
 * FileName:        _mpusbapi.cpp
 * Dependencies:    See #include section below.
 * Compiler:        Borland C++ Builder 6
 * Company:         Copyright (C) 2004 by Microchip Technology, Inc.
 *
 * Software License Agreement
 *
 * The software supplied herewith by Microchip Technology Incorporated
 * (the “Company”) for its PICmicro® Microcontroller is intended and
 * supplied to you, the Company’s customer, for use solely and
 * exclusively on Microchip PICmicro Microcontroller products. The
 * software is owned by the Company and/or its supplier, and is
 * protected under applicable copyright laws. All rights are reserved.
 * Any use in violation of the foregoing restrictions may subject the
 * user to criminal sanctions under applicable laws, as well as to
 * civil liability for the breach of the terms and conditions of this
 * license.
 *
 * THIS SOFTWARE IS PROVIDED IN AN “AS IS” CONDITION. NO WARRANTIES,
 * WHETHER EXPRESS, IMPLIED OR STATUTORY, INCLUDING, BUT NOT LIMITED
 * TO, IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A
 * PARTICULAR PURPOSE APPLY TO THIS SOFTWARE. THE COMPANY SHALL NOT,
 * IN ANY CIRCUMSTANCES, BE LIABLE FOR SPECIAL, INCIDENTAL OR
 * CONSEQUENTIAL DAMAGES, FOR ANY REASON WHATSOEVER.
 *
 * Author               Date        Comment
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 * Ross Fosler          9/2/04      Implemented MPUSBGetDeviceLink()
 * Rawin Rojvanit       11/19/04    Original version 1.00 completed
 ********************************************************************/

#include <stdafx.h>

#include "ioctls.h"
#include "mpusbapi.h"

#define	MPUSB_DEV_NO_INFO           2
#define	MPUSB_DEV_INVALID_INST      3
#define	MPUSB_DEV_VIDPID_NOT_FOUND  4
#define MPUSB_DEV_NOT_ATTACHED      5

///////////////////////////////////////////////////////////////////////////////
//	MPUSBIsVidPidEqual : Compares the pVID_PID string against the DeviceInstance
//  string retrieved from the registry using the DevicePath subkey.
//  This function should be called only from MPUSBGetDevicePath().
//
//  Note:
//  All Windows version has the DeviceClasses information stored in:
//  HKEY_LOCAL_MACHINE\\System\\CurrentControlSet\\Control\\DeviceClasses\\
//  {GUID_DEVINTERFACE_MCHPUSB}\\<DevicePath>
//  Win98SE,ME have different DevicePath string format from 2K,XP.
//  It does not contain vid&pid information in the DevicePath.
//  Thus necessitating the needs to check the DeviceInstance string in the
//  registry.
//
//	Note that "input" and "output" refer to the parameter designations in calls
//	to this function, which are the opposite of common sense from the
//	perspective of an application making the calls.
//
DWORD MPUSBIsVidPidEqual(PWCHAR pDevicePath, PWCHAR pVID_PID)
{
	DWORD dwResult = MPUSB_FAIL;
	WCHAR lszValue[255];
	WCHAR lpSubKey[512];

	HKEY hKey;
	LONG returnStatus;
	DWORD dwType=REG_SZ;
	DWORD dwSize=255;
	GUID guid = GUID_DEVINTERFACE_MCHPUSB;

	/* Modify DevicePath to use registry format */
	pDevicePath[0] = L'#';
	pDevicePath[1] = L'#';
	pDevicePath[3] = L'#';

	/* Form SubKey */
	wsprintf(lpSubKey, L"SYSTEM\\CURRENTCONTROLSET\\CONTROL\\DEVICECLASSES\\{%4.2x-%2.2x-%2.2x-%.2x%.2x-%.2x%.2x%.2x%.2x%.2x%.2x}\\%s",
		guid.Data1,guid.Data2,guid.Data3,guid.Data4[0],guid.Data4[1],guid.Data4[2],
		guid.Data4[3],guid.Data4[4],guid.Data4[5],guid.Data4[6],guid.Data4[7],pDevicePath);

	/* Open Key */
	returnStatus = RegOpenKeyEx(HKEY_LOCAL_MACHINE,
								lpSubKey,
								0L,
								KEY_ALL_ACCESS,
								&hKey);
	if(returnStatus == ERROR_SUCCESS)
	{
		returnStatus = RegQueryValueEx(hKey,
									   L"DeviceInstance",
									   NULL,
									   &dwType,
									   (LPBYTE)&lszValue,
									   &dwSize);
		if(returnStatus == ERROR_SUCCESS)
		{
			/*
			 * The string info stored in 'DeviceInstance' is the same
			 * across all Windows platforms: 98SE, ME, 2K, and XP.
			 * Upper-case in 98SE,ME.
			 * Converts all to lower-case anyway.
			 */
			_wcslwr(lszValue);
			if(wcsstr(lszValue,pVID_PID) != NULL)
			{
				dwResult = MPUSB_SUCCESS;
			}
		}
	}
	RegCloseKey(hKey);

	/* Modify DevicePath to use the original format */
	pDevicePath[0] = L'\\';
	pDevicePath[1] = L'\\';
	pDevicePath[3] = L'\\';

	return dwResult;
}//end

///////////////////////////////////////////////////////////////////////////////
//	MPUSBGetDeviceLink : Returns the path to device hardware with a given
//  instance number.
//
//	Note that "input" and "output" refer to the parameter designations in calls
//	to this function, which are the opposite of common sense from the
//	perspective of an application making the calls.
//
DWORD MPUSBGetDeviceLink(DWORD instance,    // Input
						 PWCHAR pVID_PID,   // Input
						 PWCHAR pPath,      // Output
						 DWORD dwLen,       // Input
						 PDWORD pLength)    // Output
{
	HDEVINFO info;
	SP_DEVICE_INTERFACE_DATA intf_data;
	PSP_DEVICE_INTERFACE_DETAIL_DATA intf_detail;
	DWORD ReqLen;

	if(pLength != NULL)*pLength = 0;        // Initialization

	info = SetupDiGetClassDevs((LPGUID)&GUID_DEVINTERFACE_MCHPUSB,
										NULL,
										NULL,
										DIGCF_PRESENT|DIGCF_DEVICEINTERFACE);
	if(info==INVALID_HANDLE_VALUE)
	{
		SetupDiDestroyDeviceInfoList(info);
		return MPUSB_DEV_NO_INFO;
	}// end if

	// Get interface data for the requested instance
	intf_data;
	intf_data.cbSize = sizeof(SP_DEVICE_INTERFACE_DATA);

	if(!SetupDiEnumDeviceInterfaces(info,
									NULL,
									(LPGUID)&GUID_DEVINTERFACE_MCHPUSB,
									instance,
									&intf_data))
	{
		SetupDiDestroyDeviceInfoList(info);
		return MPUSB_DEV_INVALID_INST;
	}// end if

	// Get size of symbolic link
	SetupDiGetDeviceInterfaceDetail(info, &intf_data, NULL, 0, &ReqLen, NULL);
	intf_detail = (PSP_DEVICE_INTERFACE_DETAIL_DATA)malloc(ReqLen);

	if( intf_detail == NULL)
	{
		SetupDiDestroyDeviceInfoList(info);
		return MPUSB_DEV_NO_INFO;
	}// end if

	// Get symbolic link name
	intf_detail->cbSize = sizeof(SP_DEVICE_INTERFACE_DETAIL_DATA);
	// sizeof(SP_DEVICE_INTERFACE_DETAIL_DATA) should equals 5.
	// In C++ Builder, go to Project/Options/Advanced Compiler/Data Alignment
	// and select "byte" align.

	if(!SetupDiGetDeviceInterfaceDetail(info,
										&intf_data,
										intf_detail,
										ReqLen,
										NULL,
										NULL))
	{
		SetupDiDestroyDeviceInfoList(info);
		free(intf_detail);
		return MPUSB_DEV_NO_INFO;
	}// end if

	// Check for a valid VID&PID - if argument is not null)
	if(pVID_PID != NULL)
	{
		if(MPUSBIsVidPidEqual(intf_detail->DevicePath, pVID_PID) == 0)
		{
			SetupDiDestroyDeviceInfoList(info);
			free(intf_detail);
			return MPUSB_DEV_VIDPID_NOT_FOUND;
		}// end if
	}// end if

	// Set the length of the path string
	if(pLength != NULL)
		*pLength = (DWORD)wcslen(intf_detail->DevicePath);

	// Copy output string path to buffer pointed to by pPath
	if(pPath != NULL)
	{
		// Check that input buffer has enough room...
		// Use > not >= because strlen does not include null
		if(dwLen > wcslen(intf_detail->DevicePath))
			wcscpy(pPath, intf_detail->DevicePath);
		else
		{
			SetupDiDestroyDeviceInfoList(info);
			free(intf_detail);
			return MPUSB_FAIL;
		}// end if
	}// end if

	// Clean up
	SetupDiDestroyDeviceInfoList(info);
	free(intf_detail);
	return MPUSB_SUCCESS;

}// end MPUSBGetDeviceLink

///////////////////////////////////////////////////////////////////////////////
//	MPUSBGetDeviceCount : Returns the number of devices with matching VID & PID
//
//	Note that "input" and "output" refer to the parameter designations in calls
//	to this function, which are the opposite of common sense from the
//	perspective of an application making the calls.
//
DWORD MPUSBGetDeviceCount(PWCHAR pVID_PID)
{
	int i;
	DWORD count;        // Number of USB device with matching VID & PID
	count = 0;          // Initialization

	for(i = 0; i < MAX_NUM_MPUSB_DEV; i++)
	{
		if(MPUSBGetDeviceLink(i,pVID_PID,NULL,0,NULL) == MPUSB_SUCCESS)
			count++;
	}//end for
	return count;
}//end MPUSBGetDeviceCount

///////////////////////////////////////////////////////////////////////////////
//	MPUSBOpen : Returns the handle to the endpoint pipe with matching VID & PID
//
//  All pipes are opened with the FILE_FLAG_OVERLAPPED attribute.
//  This allows MPUSBRead,MPUSBWrite, and MPUSBReadInt to have a time-out value.
//
//  Note: Time-out value has no meaning for Isochronous pipes.
//
//  instance - An instance number of the device to open.
//             Typical usage is to call MPUSBGetDeviceCount first to find out
//             how many instances there are.
//             It is important to understand that the driver is shared among
//             different devices. The number of devices returned by
//             MPUSBGetDeviceCount could be equal to or less than the number
//             of all the devices that are currently connected & using the
//             generic driver.
//
//             Example:
//             if there are 3 device with the following PID&VID connected:
//             Device Instance 0, VID 0x04d8, PID 0x0001
//             Device Instance 1, VID 0x04d8, PID 0x0002
//             Device Instance 2, VID 0x04d8, PID 0x0001
//
//             If the device of interest has VID = 0x04d8 and PID = 0x0002
//             Then MPUSBGetDeviceCount will only return '1'.
//             The calling function should have a mechanism that attempts
//             to call MPUSBOpen up to the absolute maximum of MAX_NUM_MPUSB_DEV
//             (MAX_NUM_MPUSB_DEV is defined in _mpusbapi.h).
//             It should also keep track of the number of successful calls
//             to MPUSBOpen(). Once the number of successes equals the
//             number returned by MPUSBGetDeviceCount, the attempts should
//             be aborted because there will no more devices with
//             a matching vid&pid left.
//
//  pVID_PID - A string containing the PID&VID value of the target device.
//             The format is "vid_xxxx&pid_yyyy". Where xxxx is the VID value
//             in hex and yyyy is the PID value in hex.
//             Example: If a device has the VID value of 0x04d8 and PID value
//                      of 0x000b, then the input string should be:
//                      "vid_04d8&pid_000b"
//
//  pEP      - A string of the endpoint number on the target endpoint to open.
//             The format is "\\MCHP_EPz". Where z is the endpoint number in
//             decimal.
//             Example: "\\MCHP_EP1"
//
//             This arguement can be NULL. A NULL value should be used to
//             create a handles for non-specific endpoint functions.
//             MPUSBRead, MPUSBWrite, MPUSBReadInt are endpoint specific
//             functions.
//             All others are not.
//             Non-specific endpoint functions will become available in the
//             next release of the DLL.
//
//             Note: To use MPUSBReadInt(), the format of pEP has to be
//                   "\\MCHP_EPz_ASYNC". This option is only available for
//                   an IN interrupt endpoint. A data pipe opened with the
//                   "_ASYNC" keyword would buffer the data at the interval
//                   specified in the endpoint descriptor upto the maximum of
//                   100 data sets. Any data received after the driver buffer
//                   is full will be ignored.
//                   The user application should call MPUSBReadInt() often
//                   enough so that the maximum limit of 100 is never reached.
//
//  dwDir    - Specifies the direction of the endpoint.
//             Use MP_READ for MPUSBRead, MPSUBReadInt
//             Use MP_WRITE for MPUSBWrite
//
//  dwReserved Future Use
//
//  Summary of transfer type usage:
//  ============================================================================
//  Transfer Type       Functions                       Time-Out Applicable?
//  ============================================================================
//  Interrupt - IN      MPUSBRead, MPUSBReadInt         Yes
//  Interrupt - OUT     MPUSBWrite                      Yes
//  Bulk - IN           MPUSBRead                       Yes
//  Bulk - OUT          MPUSBWrite                      Yes
//  Isochronous - IN    MPUSBRead                       No
//  Isochronous - OUT   MPUSBWrite                      No
//  ============================================================================
//
//	Note that "input" and "output" refer to the parameter designations in calls
//	to this function, which are the opposite of common sense from the
//	perspective of an application making the calls.
//
HANDLE MPUSBOpen(DWORD instance,    // Input
				 PWCHAR pVID_PID,   // Input
				 PWCHAR pEP,        // Input
				 DWORD dwDir,       // Input
				 DWORD dwReserved)  // Input <Future Use>
{

	WCHAR path[MAX_PATH];
	DWORD dwReqLen;

	HANDLE handle = INVALID_HANDLE_VALUE;
	
	(dwReserved);

	// Check arguments first
	if((pVID_PID != NULL) && ((dwDir == MP_WRITE) || (dwDir == MP_READ)))
	{
		if(MPUSBGetDeviceLink(instance,pVID_PID,path,MAX_PATH,&dwReqLen)==\
			MPUSB_SUCCESS)
		{
			WCHAR path_io[MAX_PATH];
			wcscpy(path_io,path);
			if(pEP != NULL) wcscat(path_io,pEP);

			if(dwDir == MP_READ)
			{
				handle = CreateFile(path_io,
									GENERIC_READ,
									0,
									NULL,
									OPEN_EXISTING,
									FILE_ATTRIBUTE_NORMAL|FILE_FLAG_OVERLAPPED,
									NULL);
			}
			else
			{
				handle = CreateFile(path_io,
									GENERIC_WRITE,
									0,
									NULL,
									OPEN_EXISTING,
									FILE_ATTRIBUTE_NORMAL|FILE_FLAG_OVERLAPPED,
									NULL);
			}//end if
		}//end if
	}//end if
	return handle;
}//end MPUSBOpen(...)

///////////////////////////////////////////////////////////////////////////////
//	MPUSBGetDeviceDescriptor : Returns the Device Descriptor Data
//
//  **** INCOMPLETE ****
//
//	Note that "input" and "output" refer to the parameter designations in calls
//	to this function, which are the opposite of common sense from the
//	perspective of an application making the calls.
//
DWORD MPUSBGetDeviceDescriptor(HANDLE handle,       // Input
							   PVOID pDevDsc,       // Output
							   DWORD dwLen,         // Input
							   PDWORD pLength)      // Output
{
	GET_DESCRIPTOR_PARAMETER dscParam;

	if(pLength != NULL)*pLength = 0;
	//if(pDevDsc == NULL) return MPUSB_FAIL;

	dscParam.bType =  USB_DEVICE_DESCRIPTOR_TYPE;
	if(!DeviceIoControl(handle,
						IOCTL_MCHPUSB_GET_DESCRIPTOR,
						&dscParam,
						sizeof(GET_DESCRIPTOR_PARAMETER),
						pDevDsc,
						dwLen,
						pLength,
						NULL))
	{
		printf("Get dsc error: %d",GetLastError());
		return MPUSB_FAIL;
	}//end if

	return MPUSB_SUCCESS;

}// MPUSBGetDeviceDescriptor


//  **** INCOMPLETE ****
DWORD MPUSBGetConfigInfoSize(HANDLE handle)
{
	DWORD config_size;
	DWORD dwReqLen;
	DeviceIoControl(handle,
					IOCTL_MCHPUSB_GET_CONFIGURATION_INFO,
					NULL,
					0,
					&config_size,
					sizeof(DWORD),
					&dwReqLen,
					NULL);
	return config_size;
}//end MPUSBGetConfigInfoSize

//  **** INCOMPLETE ****
DWORD MPUSBGetConfigInfo(HANDLE handle,         // Input
						 PVOID pData,           // Output
						 DWORD dwLen)           // Input
{
	DWORD dwReqLen;
	if(!DeviceIoControl(handle,
						IOCTL_MCHPUSB_GET_CONFIGURATION_INFO,
						NULL,
						0,
						pData,
						dwLen,
						&dwReqLen,
						NULL))
	{
		printf("Get config error: %d",GetLastError());
		return MPUSB_FAIL;
	}//end if

	return MPUSB_SUCCESS;
}//end MPUSBGetConfigInfo

//  **** INCOMPLETE ****
void MPUSBSendControl(void)
{
}

//  **** INCOMPLETE ****
void MPUSBGetControl(void)
{
}

///////////////////////////////////////////////////////////////////////////////
//	MPUSBRead :
//
//  handle  - Identifies the endpoint pipe to be read. The pipe handle must
//            have been created with MP_READ access attribute.
//
//  pData   - Points to the buffer that receives the data read from the pipe.
//
//  dwLen   - Specifies the number of bytes to be read from the pipe.
//
//  pLength - Points to the number of bytes read. MPUSBRead sets this value to
//            zero before doing any work or error checking.
//
//  dwMilliseconds
//          - Specifies the time-out interval, in milliseconds. The function
//            returns if the interval elapses, even if the operation is
//            incomplete. If dwMilliseconds is zero, the function tests the
//            data pipe and returns immediately. If dwMilliseconds is INFINITE,
//            the function's time-out interval never elapses.
//
//	Note that "input" and "output" refer to the parameter designations in calls
//	to this function, which are the opposite of common sense from the
//	perspective of an application making the calls.
//
DWORD MPUSBRead(HANDLE handle,          // Input
				PVOID pData,            // Output
				DWORD dwLen,            // Input
				PDWORD pLength,         // Output
				DWORD dwMilliseconds)   // Input
{
	BOOL bResult;
	DWORD nBytesRead;
	OVERLAPPED gOverlapped;
	DWORD dwResult;

	dwResult = MPUSB_FAIL;

	// set up overlapped structure fields
	gOverlapped.Internal     = 0;
	gOverlapped.InternalHigh = 0;
	gOverlapped.Offset       = 0;
	gOverlapped.OffsetHigh   = 0;
	gOverlapped.hEvent       = CreateEvent(NULL, FALSE, FALSE, NULL);

	if(pLength != NULL)*pLength = 0;

	// attempt an asynchronous read operation
	bResult = ReadFile(handle,pData,dwLen,&nBytesRead,&gOverlapped);

	if(!bResult)
	{
		// deal with the error code
		switch (GetLastError())
		{
			case ERROR_HANDLE_EOF:
			{
				// we have reached the end of the file
				// during the call to ReadFile
				break;
			}
			case ERROR_IO_PENDING:
			{
				// asynchronous i/o is still in progress
				switch(WaitForSingleObject(gOverlapped.hEvent, dwMilliseconds))
				{
					case WAIT_OBJECT_0:
						// check on the results of the asynchronous read
						// and update the nBytesRead...
						bResult = GetOverlappedResult(handle, &gOverlapped,
													  &nBytesRead, FALSE);
						if(!bResult)
						{
							printf("Error: %d", GetLastError());
						}
						else
						{
							if(pLength != NULL)
								*pLength = nBytesRead;
							dwResult = MPUSB_SUCCESS;
						}//end if else
						break;
					case WAIT_TIMEOUT:
						CancelIo(handle);
						break;
					default:
						CancelIo(handle);
						break;
				}//end switch
			}//end case
			default:
				CancelIo(handle);
				break;
		}//end switch
	}
	else
	{
		if(pLength != NULL)
			*pLength = nBytesRead;
		dwResult = MPUSB_SUCCESS;
	}//end if else

	ResetEvent(gOverlapped.hEvent);
	CloseHandle(gOverlapped.hEvent);
	
	return dwResult;
}//end MPUSBRead

///////////////////////////////////////////////////////////////////////////////
//	MPUSBWrite :
//
//  handle  - Identifies the endpoint pipe to be written to. The pipe handle
//            must have been created with MP_WRITE access attribute.
//
//  pData   - Points to the buffer containing the data to be written to the pipe.
//
//  dwLen   - Specifies the number of bytes to write to the pipe.
//
//  pLength - Points to the number of bytes written by this function call.
//            MPUSBWrite sets this value to zero before doing any work or
//            error checking.
//
//  dwMilliseconds
//          - Specifies the time-out interval, in milliseconds. The function
//            returns if the interval elapses, even if the operation is
//            incomplete. If dwMilliseconds is zero, the function tests the
//            data pipe and returns immediately. If dwMilliseconds is INFINITE,
//            the function's time-out interval never elapses.
//
//	Note that "input" and "output" refer to the parameter designations in calls
//	to this function, which are the opposite of common sense from the
//	perspective of an application making the calls.
//
DWORD MPUSBWrite(HANDLE handle,         // Input
				 PVOID pData,           // Input
				 DWORD dwLen,           // Input
				 PDWORD pLength,        // Output
				 DWORD dwMilliseconds)  // Input
{
	BOOL bResult;
	DWORD nBytesWritten;
	OVERLAPPED gOverlapped;
	DWORD dwResult;

	dwResult = MPUSB_FAIL;

	// set up overlapped structure fields
	gOverlapped.Internal     = 0;
	gOverlapped.InternalHigh = 0;
	gOverlapped.Offset       = 0;
	gOverlapped.OffsetHigh   = 0;
	gOverlapped.hEvent       = CreateEvent(NULL, FALSE, FALSE, NULL);

	if(pLength != NULL)*pLength = 0;

	// attempt an asynchronous read operation
	bResult = WriteFile(handle,pData,dwLen,&nBytesWritten,&gOverlapped);

	if(!bResult)
	{
		// deal with the error code
		switch (GetLastError())
		{
			case ERROR_HANDLE_EOF:
			{
				// we have reached the end of the file
				// during the call to ReadFile
				break;
			}
			case ERROR_IO_PENDING:
			{
				// asynchronous i/o is still in progress
				switch(WaitForSingleObject(gOverlapped.hEvent, dwMilliseconds))
				{
					case WAIT_OBJECT_0:
						// check on the results of the asynchronous read
						// and update the nBytesWritten...
						bResult = GetOverlappedResult(handle, &gOverlapped,
													  &nBytesWritten, FALSE);
						if(!bResult)
						{
							printf("Error: %d", GetLastError());
						}
						else
						{
							if(pLength != NULL)
								*pLength = nBytesWritten;
							dwResult = MPUSB_SUCCESS;
						}//end if else
						break;
					case WAIT_TIMEOUT:
						CancelIo(handle);
						break;
					default:
						CancelIo(handle);
						break;
				}//end switch
			}//end case
			default:
				CancelIo(handle);
				break;
		}//end switch
	}
	else
	{
		if(pLength != NULL)
			*pLength = nBytesWritten;
		dwResult = MPUSB_SUCCESS;
	}//end if else

	ResetEvent(gOverlapped.hEvent);
	CloseHandle(gOverlapped.hEvent);

	return dwResult;

}//end MPUSBWrite

///////////////////////////////////////////////////////////////////////////////
//	MPUSBReadInt :
//
//  handle  - Identifies the endpoint pipe to be read. The pipe handle must
//            have been created with MP_READ access attribute.
//
//  pData   - Points to the buffer that receives the data read from the pipe.
//
//  dwLen   - Specifies the number of bytes to be read from the pipe.
//
//  pLength - Points to the number of bytes read. MPUSBRead sets this value to
//            zero before doing any work or error checking.
//
//  dwMilliseconds
//          - Specifies the time-out interval, in milliseconds. The function
//            returns if the interval elapses, even if the operation is
//            incomplete. If dwMilliseconds is zero, the function tests the
//            data pipe and returns immediately. If dwMilliseconds is INFINITE,
//            the function's time-out interval never elapses.
//
//	Note that "input" and "output" refer to the parameter designations in calls
//	to this function, which are the opposite of common sense from the
//	perspective of an application making the calls.
//
DWORD MPUSBReadInt(HANDLE handle,       // Input
				   PVOID pData,         // Output
				   DWORD dwLen,         // Input
				   PDWORD pLength,      // Output
				   DWORD dwMilliseconds)// Input
{
	BOOL bResult;
	DWORD nBytesRead;
	OVERLAPPED gOverlapped;
	DWORD dwResult;

	dwResult = MPUSB_FAIL;

	// set up overlapped structure fields
	gOverlapped.Internal     = 0;
	gOverlapped.InternalHigh = 0;
	gOverlapped.Offset       = 0;
	gOverlapped.OffsetHigh   = 0;
	gOverlapped.hEvent       = CreateEvent(NULL, FALSE, FALSE, NULL);

	if(pLength != NULL)*pLength = 0;
	if(pData == NULL)
	{
		SetLastError(ERROR_NOACCESS);
		return dwResult;
	}//end if

	// attempt an asynchronous read operation
	bResult = DeviceIoControl(handle,
							  IOCTL_MCHPUSB_WAIT_INTERRUPT,
							  NULL,
							  0,
							  pData,
							  dwLen,
							  &nBytesRead,
							  &gOverlapped);
	if(!bResult)
	{
		// deal with the error code
		switch (GetLastError())
		{
			case ERROR_HANDLE_EOF:
			{
				// we have reached the end of the file
				// during the call to ReadFile
				break;
			}
			case ERROR_IO_PENDING:
			{
				// asynchronous i/o is still in progress
				switch(WaitForSingleObject(gOverlapped.hEvent, dwMilliseconds))
				{
					case WAIT_OBJECT_0:
						// check on the results of the asynchronous read
						// and update the nBytesRead...
						bResult = GetOverlappedResult(handle, &gOverlapped,
													  &nBytesRead, FALSE);
						if(!bResult)
						{
							printf("Error: %d", GetLastError());
						}
						else
						{
							if(pLength != NULL)
								*pLength = nBytesRead;
							dwResult = MPUSB_SUCCESS;
						}//end if else
						break;
					case WAIT_TIMEOUT:
						CancelIo(handle);
						break;
					default:
						CancelIo(handle);
						break;
				}//end switch
			}//end case
			default:
				CancelIo(handle);
				break;
		}//end switch
	}
	else
	{
		if(pLength != NULL)
			*pLength = nBytesRead;
		dwResult = MPUSB_SUCCESS;
	}//end if else

	ResetEvent(gOverlapped.hEvent);
	CloseHandle(gOverlapped.hEvent);

	return dwResult;
}//end MPUSBReadInt


///////////////////////////////////////////////////////////////////////////////
//	MPUSBClose : closes a given handle.
//
//	Note that "input" and "output" refer to the parameter designations in calls
//	to this function, which are the opposite of common sense from the
//	perspective of an application making the calls.
//
BOOL MPUSBClose(HANDLE handle)
{
	return (handle != INVALID_HANDLE_VALUE) ? CloseHandle(handle) : TRUE;
}//end MPUSBClose

//---------------------------------------------------------------------------
