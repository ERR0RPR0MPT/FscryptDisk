/*
I/O Packet Forwarder Service for the FscryptDisk Virtual Disk Driver for
Windows NT/2000/XP.
This service redirects I/O requests sent to the FscryptDisk Virtual Disk Driver
to another computer through a serial communication interface or by opening
a TCP/IP connection.

Copyright (C) 2005-2023 Olof Lagerkvist.

Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or
sell copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
*/

#include <windows.h>
#include <winioctl.h>
#include <ntsecapi.h>
#include <winsock.h>

#include <stdlib.h>
#include <malloc.h>
#include <process.h>

#include "..\inc\fscryptdisk.h"
#include "..\inc\fscryptdproxy.h"
#include "..\inc\ntumapi.h"
#include "..\inc\wio.hpp"
#include "..\inc\wmem.hpp"

#pragma comment(lib, "ntdll.lib")
#pragma comment(lib, "advapi32.lib")
#pragma comment(lib, "ws2_32.lib")

// #pragma comment(linker, "/subsystem:Console /entry:wWinMainCRTStartup")

SERVICE_STATUS FscryptDiskSvcStatus;
SERVICE_STATUS_HANDLE FscryptDiskSvcStatusHandle;
HANDLE FscryptDiskSvcStopEvent = NULL;

// Define DEBUG if you want debug output.
// #define DEBUG

#if defined(DEBUG) || defined(_DEBUG)

#define KdPrint(x) DbgPrint x
#define KdPrintLastError(x) DbgPrintLastError x

void DbgPrintLastError(LPCSTR Prefix)
{
    LPWSTR MsgBuf;

    FormatMessageA(FORMAT_MESSAGE_MAX_WIDTH_MASK |
                       FORMAT_MESSAGE_ALLOCATE_BUFFER |
                       FORMAT_MESSAGE_FROM_SYSTEM |
                       FORMAT_MESSAGE_IGNORE_INSERTS,
                   NULL, GetLastError(), 0, (LPSTR)&MsgBuf, 0, NULL);

    DbgPrint("FscryptDskSvc: %s: %s\n", Prefix, MsgBuf);

    LocalFree(MsgBuf);
}

#else

#define KdPrint(x)
#define KdPrintLastError(x)

#endif

class FscryptDiskSvcServerSession
{
    HANDLE hPipe;
    WOverlapped Overlapped;

    DWORD CALLBACK Thread()
    {
        FSCRYPTDPROXY_CONNECT_REQ ConnectReq;

        KdPrint(("FscryptDskSvc: Thread created.\n"));

        if (Overlapped.BufRecv(hPipe, &ConnectReq.request_code,
                               sizeof ConnectReq.request_code) !=
            sizeof ConnectReq.request_code)
        {
            KdPrintLastError(("Overlapped.BufRecv() failed"));

            delete this;
            return 0;
        }

        if (ConnectReq.request_code != FSCRYPTDPROXY_REQ_CONNECT)
        {
            delete this;
            return 0;
        }

        if (Overlapped.BufRecv(hPipe,
                               ((PUCHAR)&ConnectReq) +
                                   sizeof(ConnectReq.request_code),
                               sizeof(ConnectReq) -
                                   sizeof(ConnectReq.request_code)) !=
            sizeof(ConnectReq) -
                sizeof(ConnectReq.request_code))
        {
            KdPrintLastError(("Overlapped.BufRecv() failed"));

            delete this;
            return 0;
        }

        if ((ConnectReq.length == 0) || (ConnectReq.length > 520))
        {
            KdPrint(("FscryptDskSvc: Bad connection string length received (%u).\n",
                     ConnectReq.length));

            delete this;
            return 0;
        }

        WCRTMem<WCHAR> ConnectionString((size_t)ConnectReq.length + 2);
        if (!ConnectionString)
        {
            KdPrintLastError(("malloc() failed"));

            delete this;
            return 0;
        }

        if (Overlapped.BufRecv(hPipe, ConnectionString,
                               (DWORD)ConnectReq.length) !=
            ConnectReq.length)
        {
            KdPrintLastError(("Overlapped.BufRecv() failed"));

            delete this;
            return 0;
        }

        FSCRYPTDPROXY_CONNECT_RESP connect_resp = {0};

        ConnectionString[ConnectReq.length / sizeof *ConnectionString] = 0;

        // Split server connection string and string that should be sent to server
        // for server side connection to specific image file.

        LPWSTR path_part = wcsstr(ConnectionString, L"://");

        if (path_part != NULL)
        {
            path_part[0] = 0;
            path_part++;
        }

        HANDLE hTarget;
        switch (FSCRYPTDISK_PROXY_TYPE(ConnectReq.flags))
        {
#ifndef _M_ARM
        case FSCRYPTDISK_PROXY_TYPE_COMM:
        {
            LPWSTR FileName = wcstok(ConnectionString, L": ");

            KdPrint(("FscryptDskSvc: Connecting to '%ws'.\n", FileName));

            hTarget = CreateFile(FileName,
                                 GENERIC_READ | GENERIC_WRITE,
                                 0,
                                 NULL,
                                 OPEN_EXISTING,
                                 0,
                                 NULL);

            if (hTarget == INVALID_HANDLE_VALUE)
            {
                connect_resp.error_code = GetLastError();

                KdPrintLastError(("CreateFile() failed"));

                Overlapped.BufSend(hPipe, &connect_resp, sizeof connect_resp);

                delete this;
                return 0;
            }

            LPWSTR DCBAndTimeouts = wcstok(NULL, L"");
            if (DCBAndTimeouts != NULL)
            {
                DCB dcb = {0};
                COMMTIMEOUTS timeouts = {0};

                if (DCBAndTimeouts[0] == L' ')
                    ++DCBAndTimeouts;

                KdPrint(("FscryptDskSvc: Configuring '%ws'.\n", DCBAndTimeouts));

                GetCommState(hTarget, &dcb);
                GetCommTimeouts(hTarget, &timeouts);
                BuildCommDCBAndTimeouts(DCBAndTimeouts, &dcb, &timeouts);
                SetCommState(hTarget, &dcb);
                SetCommTimeouts(hTarget, &timeouts);
            }

            KdPrint(("FscryptDskSvc: Connected to '%ws' and configured.\n",
                     FileName));

            break;
        }
#endif

        case FSCRYPTDISK_PROXY_TYPE_TCP:
        {
            LPWSTR ServerName = wcstok(ConnectionString, L":");
            LPWSTR PortName = wcstok(NULL, L"");

            if (PortName == NULL)
                PortName = L"9000";

            KdPrint(("FscryptDskSvc: Connecting to '%ws:%ws'.\n",
                     ServerName, PortName));

            hTarget = (HANDLE)ConnectTCP(ServerName, PortName);
            if (hTarget == INVALID_HANDLE_VALUE)
            {
                connect_resp.error_code = GetLastError();

                KdPrintLastError(("ConnectTCP() failed"));

                Overlapped.BufSend(hPipe, &connect_resp, sizeof connect_resp);

                delete this;
                return 0;
            }

            bool b = true;
            setsockopt((SOCKET)hTarget, IPPROTO_TCP, TCP_NODELAY, (LPCSTR)&b,
                       sizeof b);

            KdPrint(("FscryptDskSvc: Connected to '%ws:%ws' and configured.\n",
                     ServerName, PortName));

            break;
        }

        default:
            KdPrint(("FscryptDskSvc: Unsupported connection type (%#x).\n",
                     FSCRYPTDISK_PROXY_TYPE(ConnectReq.flags)));

            connect_resp.error_code = (ULONGLONG)-1;
            Overlapped.BufSend(hPipe, &connect_resp, sizeof connect_resp);

            delete this;
            return 0;
        }

        // Connect to requested server side image path

        if (path_part != NULL)
        {
            size_t path_size = wcslen(path_part) << 1;

            size_t req_size = sizeof(FSCRYPTDPROXY_CONNECT_REQ) +
                              path_size;

            WCRTMem<FSCRYPTDPROXY_CONNECT_REQ> open_request(req_size);

            if (!open_request)
            {
                KdPrintLastError(("malloc() failed"));

                connect_resp.error_code = (ULONGLONG)-1;
                Overlapped.BufSend(hPipe, &connect_resp, sizeof connect_resp);

                delete this;
                return 0;
            }

            ZeroMemory(open_request, req_size);

            open_request->request_code = FSCRYPTDPROXY_REQ_CONNECT;
            open_request->length = path_size;

            memcpy(open_request + 1, path_part, path_size);

            if (!Overlapped.BufSend(hTarget, open_request, (DWORD)req_size))
            {
                KdPrintLastError(("Failed to send connect request to server"));

                connect_resp.error_code = (ULONGLONG)-1;
                Overlapped.BufSend(hPipe, &connect_resp, sizeof connect_resp);

                delete this;
                return 0;
            }

            open_request.Free();

            if (Overlapped.BufRecv(hTarget, &connect_resp, sizeof connect_resp) !=
                sizeof connect_resp)
            {
                connect_resp.object_ptr = NULL;
                if (connect_resp.error_code == 0)
                {
                    connect_resp.error_code = (ULONGLONG)-1;
                }

                Overlapped.BufSend(hPipe, &connect_resp, sizeof connect_resp);

                delete this;
                return 0;
            }
        }

        ConnectionString.Free();

        HANDLE hDriver = CreateFile(FSCRYPTDISK_CTL_DOSDEV_NAME,
                                    GENERIC_READ | GENERIC_WRITE,
                                    FILE_SHARE_READ | FILE_SHARE_WRITE,
                                    NULL,
                                    OPEN_EXISTING,
                                    0,
                                    NULL);

        if (hDriver == INVALID_HANDLE_VALUE)
        {
            connect_resp.error_code = GetLastError();

            KdPrintLastError(("Opening FscryptDiskCtl device failed"));
            CloseHandle(hTarget);

            Overlapped.BufSend(hPipe, &connect_resp, sizeof connect_resp);

            delete this;
            return 0;
        }

        DWORD dw;
        if (!DeviceIoControl(hDriver,
                             IOCTL_FSCRYPTDISK_REFERENCE_HANDLE,
                             &hTarget,
#pragma warning(suppress : 28132)
                             sizeof hTarget,
                             &connect_resp.object_ptr,
                             sizeof connect_resp.object_ptr,
                             &dw,
                             NULL))
        {
            connect_resp.error_code = GetLastError();

            KdPrintLastError(("IOCTL_FSCRYPTDISK_REFERENCE_HANDLE failed"));
            CloseHandle(hDriver);
            CloseHandle(hTarget);

            Overlapped.BufSend(hPipe, &connect_resp, sizeof connect_resp);

            delete this;
            return 0;
        }

        CloseHandle(hDriver);

        Overlapped.BufSend(hPipe, &connect_resp, sizeof connect_resp);

        // This will fail when driver closes the pipe, but that just indicates that
        // we should shut down this connection.
        Overlapped.BufRecv(hPipe, &dw, sizeof dw);

        KdPrint(("FscryptDskSvc: Cleaning up.\n"));

        CloseHandle(hTarget);

        delete this;
        return 1;
    }

public:
    ~FscryptDiskSvcServerSession()
    {
        if (hPipe != INVALID_HANDLE_VALUE)
            CloseHandle(hPipe);
    }

    bool Connect()
    {
        if (!ConnectNamedPipe(hPipe, &Overlapped))
            if (GetLastError() != ERROR_IO_PENDING)
            {
                FscryptDiskSvcStatus.dwWin32ExitCode = GetLastError();
                delete this;
                return false;
            }

        HANDLE hWaitObjects[] =
            {
                Overlapped.hEvent,
                FscryptDiskSvcStopEvent};

        switch (WaitForMultipleObjects(sizeof(hWaitObjects) /
                                           sizeof(*hWaitObjects),
                                       hWaitObjects,
                                       FALSE,
                                       INFINITE))
        {
        case WAIT_OBJECT_0:
        {
            union
            {
                DWORD (CALLBACK FscryptDiskSvcServerSession::*Member)();
                UINT(CALLBACK *Static)(LPVOID);
            } ThreadFunction;
            ThreadFunction.Member = &FscryptDiskSvcServerSession::Thread;

            KdPrint(("FscryptDskSvc: Creating thread.\n"));

            UINT id = 0;
            HANDLE hThread = (HANDLE)
                _beginthreadex(NULL, 0, ThreadFunction.Static, this, 0, &id);
            if (hThread == NULL)
            {
                FscryptDiskSvcStatus.dwWin32ExitCode = GetLastError();
                delete this;
                return false;
            }

            CloseHandle(hThread);
            FscryptDiskSvcStatus.dwWin32ExitCode = NO_ERROR;
            return true;
        }

        case WAIT_OBJECT_0 + 1:
            FscryptDiskSvcStatus.dwWin32ExitCode = NO_ERROR;
            delete this;
            return false;

        default:
            FscryptDiskSvcStatus.dwWin32ExitCode = GetLastError();
            delete this;
            return false;
        }
    }

    FscryptDiskSvcServerSession()
    {
        hPipe = CreateNamedPipe(FSCRYPTDPROXY_SVC_PIPE_DOSDEV_NAME,
                                PIPE_ACCESS_DUPLEX |
                                    FILE_FLAG_WRITE_THROUGH |
                                    FILE_FLAG_OVERLAPPED,
                                0,
                                PIPE_UNLIMITED_INSTANCES,
                                0,
                                0,
                                0,
                                NULL);
    }

    bool IsOk()
    {
        if (this == NULL)
            return false;

        return (hPipe != NULL) &&
               (hPipe != INVALID_HANDLE_VALUE) &&
               (Overlapped.hEvent != NULL);
    }
};

VOID
    CALLBACK
    FscryptDiskSvcCtrlHandler(DWORD Opcode)
{
    if (Opcode == SERVICE_CONTROL_STOP)
    {
        SetEvent(FscryptDiskSvcStopEvent);

        FscryptDiskSvcStatus.dwWin32ExitCode = NO_ERROR;
        FscryptDiskSvcStatus.dwCurrentState = SERVICE_STOP_PENDING;
        FscryptDiskSvcStatus.dwCheckPoint = 0;
        FscryptDiskSvcStatus.dwWaitHint = 0;

        if (!SetServiceStatus(FscryptDiskSvcStatusHandle, &FscryptDiskSvcStatus))
        {
            KdPrintLastError(("SetServiceStatus() failed"));
        }

        return;
    }

    SetServiceStatus(FscryptDiskSvcStatusHandle, &FscryptDiskSvcStatus);
}

VOID
    CALLBACK
    FscryptDiskSvcStart(DWORD, LPWSTR *)
{
    FscryptDiskSvcStatus.dwServiceType = SERVICE_WIN32;
    FscryptDiskSvcStatus.dwCurrentState = SERVICE_START_PENDING;
    FscryptDiskSvcStatus.dwControlsAccepted = SERVICE_ACCEPT_STOP;
    FscryptDiskSvcStatus.dwWin32ExitCode = NO_ERROR;
    FscryptDiskSvcStatus.dwServiceSpecificExitCode = 0;
    FscryptDiskSvcStatus.dwCheckPoint = 0;
    FscryptDiskSvcStatus.dwWaitHint = 0;

    FscryptDiskSvcStatusHandle = RegisterServiceCtrlHandler(FSCRYPTDPROXY_SVC,
                                                            FscryptDiskSvcCtrlHandler);

    if (FscryptDiskSvcStatusHandle == (SERVICE_STATUS_HANDLE)0)
    {
        KdPrintLastError(("RegisterServiceCtrlHandler() failed"));
        return;
    }

    FscryptDiskSvcStatus.dwCurrentState = SERVICE_RUNNING;
    SetServiceStatus(FscryptDiskSvcStatusHandle, &FscryptDiskSvcStatus);

    for (;;)
    {
        if (WaitForSingleObject(FscryptDiskSvcStopEvent, 0) != WAIT_TIMEOUT)
        {
            FscryptDiskSvcStatus.dwWin32ExitCode = NO_ERROR;
            break;
        }

#pragma warning(suppress : 28197)
        FscryptDiskSvcServerSession *ServerSession = new FscryptDiskSvcServerSession;
        if (!ServerSession->IsOk())
        {
            delete ServerSession;
            KdPrintLastError(("Pipe initialization failed"));
            break;
        }

        if (!ServerSession->Connect())
        {
            if (FscryptDiskSvcStatus.dwWin32ExitCode != NO_ERROR)
            {
                KdPrintLastError(("Pipe connect failed"));
            }
            break;
        }
    }

    FscryptDiskSvcStatus.dwCurrentState = SERVICE_STOPPED;
    FscryptDiskSvcStatus.dwControlsAccepted = 0;
    SetServiceStatus(FscryptDiskSvcStatusHandle, &FscryptDiskSvcStatus);
}

#ifdef _DEBUG

extern "C" int
    CALLBACK
#pragma warning(suppress : 28251)
    wWinMain(HINSTANCE,
             HINSTANCE,
             LPWSTR,
             int)
{
    KdPrint(("FscryptDskSvc: Starting up process.\n"));

    WSADATA wsadata;
    WSAStartup(0x0101, &wsadata);

    FscryptDiskSvcStopEvent = CreateEvent(NULL, TRUE, FALSE, NULL);
    if (FscryptDiskSvcStopEvent == NULL)
    {
        KdPrintLastError(("CreateEvent() failed"));
        return 0;
    }

    for (;;)
    {
        if (WaitForSingleObject(FscryptDiskSvcStopEvent, 0) != WAIT_TIMEOUT)
        {
            FscryptDiskSvcStatus.dwWin32ExitCode = NO_ERROR;
            break;
        }

#pragma warning(suppress : 28197)
        FscryptDiskSvcServerSession *ServerSession = new FscryptDiskSvcServerSession;
        if (!ServerSession->IsOk())
        {
            delete ServerSession;
            KdPrintLastError(("Pipe initialization failed"));
            break;
        }

        if (!ServerSession->Connect())
        {
            if (FscryptDiskSvcStatus.dwWin32ExitCode != NO_ERROR)
            {
                KdPrintLastError(("Pipe connect failed"));
            }
            break;
        }
    }

    SetEvent(FscryptDiskSvcStopEvent);

    return 1;
}

#else

extern "C" int
    CALLBACK
#pragma warning(suppress : 28251)
    wWinMain(HINSTANCE,
             HINSTANCE,
             LPWSTR,
             int)
{
    KdPrint(("FscryptDskSvc: Starting up process.\n"));

    WSADATA wsadata;
    WSAStartup(0x0101, &wsadata);

    SERVICE_TABLE_ENTRY ServiceTable[] =
        {
            {FSCRYPTDPROXY_SVC, FscryptDiskSvcStart},
            {NULL, NULL}};

    FscryptDiskSvcStopEvent = CreateEvent(NULL, TRUE, FALSE, NULL);
    if (FscryptDiskSvcStopEvent == NULL)
    {
        KdPrintLastError(("CreateEvent() failed"));
        return 0;
    }

    if (!StartServiceCtrlDispatcher(ServiceTable))
    {
        KdPrintLastError(("StartServiceCtrlDispatcher() failed"));

#ifndef _M_ARM
        MessageBoxA(NULL, "This program can only run as a Windows NT service.",
                    "FscryptDisk Service",
                    MB_ICONSTOP | MB_TASKMODAL);
#endif

        return 0;
    }

    SetEvent(FscryptDiskSvcStopEvent);

    return 1;
}

#endif

#if !defined(_DEBUG) && !defined(DEBUG) && _MSC_PLATFORM_TOOLSET < 140

// We have our own EXE entry to be less dependent on
// specific MSVCRT code that may not be available in older Windows versions.
// It also saves some EXE file size.
__declspec(noreturn) extern "C" void __cdecl wWinMainCRTStartup()
{
    ExitProcess(wWinMain(NULL, NULL, NULL, 0));
}

#endif
