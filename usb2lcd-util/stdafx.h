// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently, but
// are changed infrequently
//

#pragma once

#define WINVER 0x0501           // Specifies that the minimum required platform is Windows XP
#define _WIN32_WINNT 0x0501

#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
#define _CRT_SECURE_NO_WARNINGS

// Windows Header Files:
#include <windows.h>

//IOCTL Headers
#include <setupapi.h>
#pragma comment(lib, "setupapi.lib")
#include <initguid.h>
#include <winioctl.h>

// Standard C Headers
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <malloc.h>
