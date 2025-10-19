/*
FscryptDisk Proxy Services.

Copyright (C) 2005-2007 Olof Lagerkvist.

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

#ifndef _INC_FSCRYPTDPROXY_
#define _INC_FSCRYPTDPROXY_

#if !defined(_WIN32) && !defined(_NTDDK_)
typedef int32_t LONG;
typedef uint32_t ULONG;
typedef int64_t LONGLONG;
typedef uint64_t ULONGLONG;
typedef u_short WCHAR;
typedef u_char UCHAR;
#endif

#define FSCRYPTDPROXY_SVC L"FscryptDskSvc"
#define FSCRYPTDPROXY_SVC_PIPE_DOSDEV_NAME L"\\\\.\\PIPE\\" FSCRYPTDPROXY_SVC
#define FSCRYPTDPROXY_SVC_PIPE_NATIVE_NAME L"\\Device\\NamedPipe\\" FSCRYPTDPROXY_SVC

#define DEVIODRV_DEVICE_NAME L"DevIoDrv"
#define DEVIODRV_DEVICE_DOSDEV_NAME L"\\\\.\\" DEVIODRV_DEVICE_NAME
#define DEVIODRV_DEVICE_NATIVE_NAME L"\\Device\\" DEVIODRV_DEVICE_NAME
#define DEVIODRV_SYMLINK_NATIVE_NAME L"\\DosDevices\\" DEVIODRV_DEVICE_NAME

#define FSCRYPTDPROXY_FLAG_RO 0x01              // Read-only
#define FSCRYPTDPROXY_FLAG_SUPPORTS_UNMAP 0x02  // Unmap/TRIM ranges
#define FSCRYPTDPROXY_FLAG_SUPPORTS_ZERO 0x04   // Zero-fill ranges
#define FSCRYPTDPROXY_FLAG_SUPPORTS_SCSI 0x08   // SCSI SRB operations
#define FSCRYPTDPROXY_FLAG_SUPPORTS_SHARED 0x10 // Shared image access with reservations
#define FSCRYPTDPROXY_FLAG_KEEP_OPEN 0x20       // DevIoDrv mode with persistent virtual file

typedef enum _FSCRYPTDPROXY_REQ
{
    FSCRYPTDPROXY_REQ_NULL,
    FSCRYPTDPROXY_REQ_INFO,
    FSCRYPTDPROXY_REQ_READ,
    FSCRYPTDPROXY_REQ_WRITE,
    FSCRYPTDPROXY_REQ_CONNECT,
    FSCRYPTDPROXY_REQ_CLOSE,
    FSCRYPTDPROXY_REQ_UNMAP,
    FSCRYPTDPROXY_REQ_ZERO,
    FSCRYPTDPROXY_REQ_SCSI,
    FSCRYPTDPROXY_REQ_SHARED
} FSCRYPTDPROXY_REQ,
    *PFSCRYPTDPROXY_REQ;

typedef struct _FSCRYPTDPROXY_CLOSE_REQ
{
    ULONGLONG request_code;
} FSCRYPTDPROXY_CLOSE_REQ, *PFSCRYPTDPROXY_CLOSE_REQ;

typedef struct _FSCRYPTDPROXY_CONNECT_REQ
{
    ULONGLONG request_code;
    ULONGLONG flags;
    ULONGLONG length;
} FSCRYPTDPROXY_CONNECT_REQ, *PFSCRYPTDPROXY_CONNECT_REQ;

typedef struct _FSCRYPTDPROXY_CONNECT_RESP
{
    ULONGLONG error_code;
    ULONGLONG object_ptr;
} FSCRYPTDPROXY_CONNECT_RESP, *PFSCRYPTDPROXY_CONNECT_RESP;

typedef struct _FSCRYPTDPROXY_INFO_RESP
{
    ULONGLONG file_size;
    ULONGLONG req_alignment;
    ULONGLONG flags;
} FSCRYPTDPROXY_INFO_RESP, *PFSCRYPTDPROXY_INFO_RESP;

typedef struct _FSCRYPTDPROXY_READ_REQ
{
    ULONGLONG request_code;
    ULONGLONG offset;
    ULONGLONG length;
} FSCRYPTDPROXY_READ_REQ, *PFSCRYPTDPROXY_READ_REQ;

typedef struct _FSCRYPTDPROXY_READ_RESP
{
    ULONGLONG errorno;
    ULONGLONG length;
} FSCRYPTDPROXY_READ_RESP, *PFSCRYPTDPROXY_READ_RESP;

typedef struct _FSCRYPTDPROXY_WRITE_REQ
{
    ULONGLONG request_code;
    ULONGLONG offset;
    ULONGLONG length;
} FSCRYPTDPROXY_WRITE_REQ, *PFSCRYPTDPROXY_WRITE_REQ;

typedef struct _FSCRYPTDPROXY_WRITE_RESP
{
    ULONGLONG errorno;
    ULONGLONG length;
} FSCRYPTDPROXY_WRITE_RESP, *PFSCRYPTDPROXY_WRITE_RESP;

typedef struct _FSCRYPTDPROXY_UNMAP_REQ
{
    ULONGLONG request_code;
    ULONGLONG length;
} FSCRYPTDPROXY_UNMAP_REQ, *PFSCRYPTDPROXY_UNMAP_REQ;

typedef struct _FSCRYPTDPROXY_UNMAP_RESP
{
    ULONGLONG errorno;
} FSCRYPTDPROXY_UNMAP_RESP, *PFSCRYPTDPROXY_UNMAP_RESP;

typedef struct _FSCRYPTDPROXY_ZERO_REQ
{
    ULONGLONG request_code;
    ULONGLONG length;
} FSCRYPTDPROXY_ZERO_REQ, *PFSCRYPTDPROXY_ZERO_REQ;

typedef struct _FSCRYPTDPROXY_ZERO_RESP
{
    ULONGLONG errorno;
} FSCRYPTDPROXY_ZERO_RESP, *PFSCRYPTDPROXY_ZERO_RESP;

typedef struct _FSCRYPTDPROXY_SCSI_REQ
{
    ULONGLONG request_code;
    UCHAR cdb[16];
    ULONGLONG request_length;
    ULONGLONG max_response_length;
} FSCRYPTDPROXY_SCSI_REQ, *PFSCRYPTDPROXY_SCSI_REQ;

typedef struct _FSCRYPTDPROXY_SCSI_RESP
{
    ULONGLONG errorno;
    ULONGLONG length;
} FSCRYPTDPROXY_SCSI_RESP, *PFSCRYPTDPROXY_SCSI_RESP;

typedef struct _FSCRYPTDPROXY_SHARED_REQ
{
    ULONGLONG request_code;
    ULONGLONG operation_code;
    ULONGLONG reserve_scope;
    ULONGLONG reserve_type;
    ULONGLONG existing_reservation_key;
    ULONGLONG current_channel_key;
    ULONGLONG operation_channel_key;
} FSCRYPTDPROXY_SHARED_REQ, *PFSCRYPTDPROXY_SHARED_REQ;

typedef struct _FSCRYPTDPROXY_SHARED_RESP
{
    ULONGLONG errorno;
    UCHAR unique_id[16];
    ULONGLONG channel_key;
    ULONGLONG reservation_key;
    ULONGLONG reservation_scope;
    ULONGLONG reservation_type;
    ULONGLONG length;
} FSCRYPTDPROXY_SHARED_RESP, *PFSCRYPTDPROXY_SHARED_RESP;

#define FSCRYPTDPROXY_RESERVATION_KEY_ANY MAXULONGLONG

typedef enum _FSCRYPTDPROXY_SHARED_OP_CODE
{
    GetUniqueId,
    ReadKeys,
    Register,
    ClearKeys,
    Reserve,
    Release,
    Preempt
} FSCRYPTDPROXY_SHARED_OP_CODE,
    *PFSCRYPTDPROXY_SHARED_OP_CODE;

typedef enum _FSCRYPTDPROXY_SHARED_RESP_CODE
{
    NoError,
    ReservationCollision,
    InvalidParameter,
    IOError
} FSCRYPTDPROXY_SHARED_RESP_CODE,
    *PFSCRYPTDPROXY_SHARED_RESP_CODE;

// For shared memory proxy communication only. Offset to data area in
// shared memory.
#define FSCRYPTDPROXY_HEADER_SIZE 4096

// For use with deviodrv driver, where requests and responses are tagged
// with an id for asynchronous operations.
typedef struct _FSCRYPTDPROXY_DEVIODRV_BUFFER_HEADER
{
    ULONGLONG request_code; // Request code to forward to response header.
    ULONGLONG io_tag;       // Tag to forward to response header.
    ULONGLONG flags;        // Reserved. Currently not used.
} FSCRYPTDPROXY_DEVIODRV_BUFFER_HEADER, *PFSCRYPTDPROXY_DEVIODRV_BUFFER_HEADER;

#if defined(CTL_CODE) && !defined(IOCTL_DEVIODRV_EXCHANGE_IO)
#ifndef FILE_DEVICE_FSCRYPTDISK
#define FILE_DEVICE_FSCRYPTDISK 0x8372
#endif
#define IOCTL_DEVIODRV_EXCHANGE_IO ((ULONG)CTL_CODE(FILE_DEVICE_FSCRYPTDISK, 0x8D0, METHOD_OUT_DIRECT, FILE_READ_ACCESS | FILE_WRITE_ACCESS))
#define IOCTL_DEVIODRV_LOCK_MEMORY ((ULONG)CTL_CODE(FILE_DEVICE_FSCRYPTDISK, 0x8D1, METHOD_OUT_DIRECT, FILE_READ_ACCESS | FILE_WRITE_ACCESS))
#endif

#endif // _INC_FSCRYPTDPROXY_
