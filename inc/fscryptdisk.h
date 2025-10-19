/*
FscryptDisk Virtual Disk Driver for Windows NT/2000/XP.

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

#ifndef _INC_FSCRYPTDISK_
#define _INC_FSCRYPTDISK_

#ifndef __T
#if defined(_NTDDK_) || defined(UNICODE) || defined(_UNICODE)
#define __T(x) L##x
#else
#define __T(x) x
#endif
#endif

#ifndef _T
#define _T(x) __T(x)
#endif

#include "fscryptdiskver.h"
#define FSCRYPTDISK_VERSION ((FSCRYPTDISK_MAJOR_VERSION << 8) | (FSCRYPTDISK_MINOR_VERSION << 4) | (FSCRYPTDISK_MINOR_LOW_VERSION))
#define FSCRYPTDISK_DRIVER_VERSION 0x0103

#ifndef ZERO_STRUCT
#define ZERO_STRUCT {0}
#endif

///
/// Base names for device objects created in \Device
///
#define FSCRYPTDISK_DEVICE_DIR_NAME _T("\\Device")
#define FSCRYPTDISK_DEVICE_BASE_NAME FSCRYPTDISK_DEVICE_DIR_NAME _T("\\FscryptDisk_APP_")
#define FSCRYPTDISK_CTL_DEVICE_NAME FSCRYPTDISK_DEVICE_BASE_NAME _T("Ctl")

///
/// Symlinks created in \DosDevices to device objects
///
#define FSCRYPTDISK_SYMLNK_NATIVE_DIR_NAME _T("\\DosDevices")
#define FSCRYPTDISK_SYMLNK_WIN32_DIR_NAME _T("\\\\?")
#define FSCRYPTDISK_SYMLNK_NATIVE_BASE_NAME FSCRYPTDISK_SYMLNK_NATIVE_DIR_NAME _T("\\FscryptDisk_APP_")
#define FSCRYPTDISK_SYMLNK_WIN32_BASE_NAME FSCRYPTDISK_SYMLNK_WIN32_DIR_NAME _T("\\FscryptDisk_APP_")
#define FSCRYPTDISK_CTL_SYMLINK_NAME FSCRYPTDISK_SYMLNK_NATIVE_BASE_NAME _T("Ctl")
#define FSCRYPTDISK_CTL_DOSDEV_NAME FSCRYPTDISK_SYMLNK_WIN32_BASE_NAME _T("Ctl")

///
/// The driver name and image path
///
#define FSCRYPTDISK_DRIVER_NAME _T("FscryptDisk")
#define FSCRYPTDISK_DRIVER_PATH _T("system32\\drivers\\fscryptdisk.sys")

#ifndef AWEFSALLOC_DRIVER_NAME
#define AWEFSALLOC_DRIVER_NAME _T("AWE_FS_Alloc")
#endif
#ifndef AWEFSALLOC_DEVICE_NAME
#define AWEFSALLOC_DEVICE_NAME _T("\\Device\\") AWEFSALLOC_DRIVER_NAME
#endif

///
/// Global refresh event name
///
#define FSCRYPTDISK_REFRESH_EVENT_NAME _T("FscryptDiskRefresh")

///
/// Registry settings. It is possible to specify devices to be mounted
/// automatically when the driver loads.
///
#define FSCRYPTDISK_CFG_PARAMETER_KEY _T("\\Parameters")
#define FSCRYPTDISK_CFG_MAX_DEVICES_VALUE _T("MaxDevices")
#define FSCRYPTDISK_CFG_LOAD_DEVICES_VALUE _T("LoadDevices")
#define FSCRYPTDISK_CFG_DISALLOWED_DRIVE_LETTERS_VALUE _T("DisallowedDriveLetters")
#define FSCRYPTDISK_CFG_IMAGE_FILE_PREFIX _T("FileName")
#define FSCRYPTDISK_CFG_SIZE_PREFIX _T("Size")
#define FSCRYPTDISK_CFG_FLAGS_PREFIX _T("Flags")
#define FSCRYPTDISK_CFG_DRIVE_LETTER_PREFIX _T("DriveLetter")
#define FSCRYPTDISK_CFG_OFFSET_PREFIX _T("ImageOffset")

#define KEY_NAME_HKEY_MOUNTPOINTS \
    _T("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\MountPoints")
#define KEY_NAME_HKEY_MOUNTPOINTS2 \
    _T("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\MountPoints2")

#define FSCRYPTDISK_WINVER_MAJOR() (GetVersion() & 0xFF)
#define FSCRYPTDISK_WINVER_MINOR() ((GetVersion() & 0xFF00) >> 8)

#define FSCRYPTDISK_WINVER() ((FSCRYPTDISK_WINVER_MAJOR() << 8) | \
                              FSCRYPTDISK_WINVER_MINOR())

#if defined(NT4_COMPATIBLE) && defined(_M_IX86)
#define FSCRYPTDISK_GTE_WIN2K() (FSCRYPTDISK_WINVER_MAJOR() >= 0x05)
#else
#define FSCRYPTDISK_GTE_WIN2K() TRUE
#endif

#ifdef _M_IX86
#define FSCRYPTDISK_GTE_WINXP() (FSCRYPTDISK_WINVER() >= 0x0501)
#else
#define FSCRYPTDISK_GTE_WINXP() TRUE
#endif

#define FSCRYPTDISK_GTE_SRV2003() (FSCRYPTDISK_WINVER() >= 0x0502)

#define FSCRYPTDISK_GTE_VISTA() (FSCRYPTDISK_WINVER_MAJOR() >= 0x06)

#ifndef FSCRYPTDISK_API
#ifdef FSCRYPTDISK_CPL_EXPORTS
#define FSCRYPTDISK_API __declspec(dllexport)
#else
#define FSCRYPTDISK_API __declspec(dllimport)
#endif
#endif

///
/// Base value for the IOCTL's.
///
#define FILE_DEVICE_FSCRYPTDISK 0x8372

#define IOCTL_FSCRYPTDISK_QUERY_VERSION ((ULONG)CTL_CODE(FILE_DEVICE_FSCRYPTDISK, 0x800, METHOD_BUFFERED, 0))
#define IOCTL_FSCRYPTDISK_CREATE_DEVICE ((ULONG)CTL_CODE(FILE_DEVICE_FSCRYPTDISK, 0x801, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS))
#define IOCTL_FSCRYPTDISK_QUERY_DEVICE ((ULONG)CTL_CODE(FILE_DEVICE_FSCRYPTDISK, 0x802, METHOD_BUFFERED, 0))
#define IOCTL_FSCRYPTDISK_QUERY_DRIVER ((ULONG)CTL_CODE(FILE_DEVICE_FSCRYPTDISK, 0x803, METHOD_BUFFERED, 0))
#define IOCTL_FSCRYPTDISK_REFERENCE_HANDLE ((ULONG)CTL_CODE(FILE_DEVICE_FSCRYPTDISK, 0x804, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS))
#define IOCTL_FSCRYPTDISK_SET_DEVICE_FLAGS ((ULONG)CTL_CODE(FILE_DEVICE_FSCRYPTDISK, 0x805, METHOD_BUFFERED, 0))
#define IOCTL_FSCRYPTDISK_REMOVE_DEVICE ((ULONG)CTL_CODE(FILE_DEVICE_FSCRYPTDISK, 0x806, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS))
#define IOCTL_FSCRYPTDISK_IOCTL_PASS_THROUGH ((ULONG)CTL_CODE(FILE_DEVICE_FSCRYPTDISK, 0x807, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS))
#define IOCTL_FSCRYPTDISK_FSCTL_PASS_THROUGH ((ULONG)CTL_CODE(FILE_DEVICE_FSCRYPTDISK, 0x808, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS))
#define IOCTL_FSCRYPTDISK_GET_REFERENCED_HANDLE ((ULONG)CTL_CODE(FILE_DEVICE_FSCRYPTDISK, 0x809, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS))

///
/// Bit constants for the Flags field in FSCRYPTDISK_CREATE_DATA
///

/// Read-only device
#define FSCRYPTDISK_OPTION_RO 0x00000001

/// Check if flags specifies read-only
#define FSCRYPTDISK_READONLY(x) ((ULONG)(x) & 0x00000001)

/// Removable, hot-plug, device
#define FSCRYPTDISK_OPTION_REMOVABLE 0x00000002

/// Check if flags specifies removable
#define FSCRYPTDISK_REMOVABLE(x) ((ULONG)(x) & 0x00000002)

/// Specifies that image file is created with sparse attribute.
#define FSCRYPTDISK_OPTION_SPARSE_FILE 0x00000004

/// Check if flags specifies sparse
#define FSCRYPTDISK_SPARSE_FILE(x) ((ULONG)(x) & 0x00000004)

/// Swaps each byte pair in image file.
#define FSCRYPTDISK_OPTION_BYTE_SWAP 0x00000008

/// Check if flags specifies byte swapping
#define FSCRYPTDISK_BYTE_SWAP(x) ((ULONG)(x) & 0x00000008)

/// Device type is virtual harddisk partition
#define FSCRYPTDISK_DEVICE_TYPE_HD 0x00000010
/// Device type is virtual floppy drive
#define FSCRYPTDISK_DEVICE_TYPE_FD 0x00000020
/// Device type is virtual CD/DVD-ROM drive
#define FSCRYPTDISK_DEVICE_TYPE_CD 0x00000030
/// Device type is unknown "raw" (for use with third-party client drivers)
#define FSCRYPTDISK_DEVICE_TYPE_RAW 0x00000040

/// Extracts the FSCRYPTDISK_DEVICE_TYPE_xxx from flags
#define FSCRYPTDISK_DEVICE_TYPE(x) ((ULONG)(x) & 0x000000F0)

/// Virtual disk is backed by image file
#define FSCRYPTDISK_TYPE_FILE 0x00000100
/// Virtual disk is backed by virtual memory
#define FSCRYPTDISK_TYPE_VM 0x00000200
/// Virtual disk is backed by proxy connection
#define FSCRYPTDISK_TYPE_PROXY 0x00000300

/// Extracts the FSCRYPTDISK_TYPE_xxx from flags
#define FSCRYPTDISK_TYPE(x) ((ULONG)(x) & 0x00000F00)

// Types with proxy mode

/// Proxy connection is direct-type
#define FSCRYPTDISK_PROXY_TYPE_DIRECT 0x00000000
/// Proxy connection is over serial line
#define FSCRYPTDISK_PROXY_TYPE_COMM 0x00001000
/// Proxy connection is over TCP/IP
#define FSCRYPTDISK_PROXY_TYPE_TCP 0x00002000
/// Proxy connection uses shared memory
#define FSCRYPTDISK_PROXY_TYPE_SHM 0x00003000

/// Extracts the FSCRYPTDISK_PROXY_TYPE_xxx from flags
#define FSCRYPTDISK_PROXY_TYPE(x) ((ULONG)(x) & 0x0000F000)

// Types with file mode

/// Serialized I/O to an image file, done in a worker thread
#define FSCRYPTDISK_FILE_TYPE_QUEUED_IO 0x00000000
/// Direct parallel I/O to AWE_FS_Alloc driver (physical RAM), done in request
/// thread
#define FSCRYPTDISK_FILE_TYPE_AWEFSALLOC 0x00001000
/// Direct parallel I/O to an image file, done in request thread
#define FSCRYPTDISK_FILE_TYPE_PARALLEL_IO 0x00002000
/// Buffered I/O to an image file. Disables FILE_NO_INTERMEDIATE_BUFFERING when
/// opening image file.
#define FSCRYPTDISK_FILE_TYPE_BUFFERED_IO 0x00003000

/// Extracts the FSCRYPTDISK_FILE_TYPE_xxx from flags
#define FSCRYPTDISK_FILE_TYPE(x) ((ULONG)(x) & 0x0000F000)

/// Flag set by write request dispatchers to indicated that virtual disk has
/// been since mounted
#define FSCRYPTDISK_IMAGE_MODIFIED 0x00010000

// 0x00020000 is reserved. Corresponds to IMSCSI_FAKE_DISK_SIG in
// Arsenal Image Mounter.

/// This flag causes the driver to open image files in shared write mode even
/// if the image is opened for writing. This could be useful in some cases,
/// but could easily corrupt filesystems on image files if used incorrectly.
#define FSCRYPTDISK_OPTION_SHARED_IMAGE 0x00040000
/// Check if flags indicate shared write mode
#define FSCRYPTDISK_SHARED_IMAGE(x) ((ULONG)(x) & 0x00040000)

/// Macro to determine if flags specify either virtual memory (type vm) or
/// physical memory (type file with awefsalloc) virtual disk drive
#define FSCRYPTDISK_IS_MEMORY_DRIVE(x)                  \
    ((FSCRYPTDISK_TYPE(x) == FSCRYPTDISK_TYPE_VM) ||    \
     ((FSCRYPTDISK_TYPE(x) == FSCRYPTDISK_TYPE_FILE) && \
      (FSCRYPTDISK_FILE_TYPE(x) == FSCRYPTDISK_FILE_TYPE_AWEFSALLOC)))

/// Specify as device number to automatically select first free.
#define FSCRYPTDISK_AUTO_DEVICE_NUMBER ((ULONG) - 1)

/**
Structure used by the IOCTL_FSCRYPTDISK_CREATE_DEVICE and
IOCTL_FSCRYPTDISK_QUERY_DEVICE calls and by the FscryptDiskQueryDevice function.
*/
typedef struct _FSCRYPTDISK_CREATE_DATA
{
    /// On create this can be set to FSCRYPTDISK_AUTO_DEVICE_NUMBER
    ULONG DeviceNumber;
    /// Total size in bytes (in the Cylinders field) and virtual geometry.
    DISK_GEOMETRY DiskGeometry;
    /// The byte offset in the image file where the virtual disk begins.
    LARGE_INTEGER ImageOffset;
    /// Creation flags. Type of device and type of connection.
    ULONG Flags;
    /// Drive letter (if used, otherwise zero).
    WCHAR DriveLetter;
    /// Length in bytes of the FileName member.
    USHORT FileNameLength;
    /// Dynamically-sized member that specifies the image file name.
    WCHAR FileName[1];
} FSCRYPTDISK_CREATE_DATA, *PFSCRYPTDISK_CREATE_DATA;

typedef struct _FSCRYPTDISK_SET_DEVICE_FLAGS
{
    ULONG FlagsToChange;
    ULONG FlagValues;
} FSCRYPTDISK_SET_DEVICE_FLAGS, *PFSCRYPTDISK_SET_DEVICE_FLAGS;

#define FSCRYPTDISK_API_NO_BROADCAST_NOTIFY 0x00000001
#define FSCRYPTDISK_API_FORCE_DISMOUNT 0x00000002

#pragma pack(push)
#pragma pack(1)
typedef struct _FAT_BPB
{
    USHORT BytesPerSector;
    UCHAR SectorsPerCluster;
    USHORT ReservedSectors;
    UCHAR NumberOfFileAllocationTables;
    USHORT NumberOfRootEntries;
    USHORT NumberOfSectors;
    UCHAR MediaDescriptor;
    USHORT SectorsPerFileAllocationTable;
    USHORT SectorsPerTrack;
    USHORT NumberOfHeads;
    union
    {
        struct
        {
            USHORT NumberOfHiddenSectors;
            USHORT TotalNumberOfSectors;
        } DOS_320;
        struct
        {
            ULONG NumberOfHiddenSectors;
            ULONG TotalNumberOfSectors;
        } DOS_331;
    };
} FAT_BPB, *PFAT_BPB;

typedef struct _FAT_VBR
{
    UCHAR JumpInstruction[3];
    CHAR OEMName[8];
    FAT_BPB BPB;
    UCHAR FillData[512 - 3 - 8 - sizeof(FAT_BPB) - 1 - 2];
    UCHAR PhysicalDriveNumber;
    UCHAR Signature[2];
} FAT_VBR, *PFAT_VBR;

#pragma warning(push)
#pragma warning(disable : 4214)
typedef struct _MBR_PARTITION_TABLE_ENTRY
{
    UCHAR Zero : 7;
    UCHAR BootIndicator : 1;
    UCHAR StartCHS[3];
    UCHAR PartitionType;
    UCHAR EndCHS[3];
    ULONG StartingSector;
    ULONG NumberOfSectors;
} MBR_PARTITION_TABLE_ENTRY, *PMBR_PARTITION_TABLE_ENTRY;
#pragma warning(pop)

typedef struct _MBR_PARTITION_TABLE
{
    MBR_PARTITION_TABLE_ENTRY PartitionEntry[4];
    USHORT Signature; // == 0xAA55
} MBR_PARTITION_TABLE, *PMBR_PARTITION_TABLE;

#pragma pack(pop)

#ifdef WINAPI

#ifdef __cplusplus
extern "C"
{
#endif

    /**
    Get behaviour flags for API.
    */
    FSCRYPTDISK_API ULONGLONG
        WINAPI
        FscryptDiskGetAPIFlags();

    /**
    Set behaviour flags for API. Returns previously defined flag field.

    Flags        New flags value to set.
    */
    FSCRYPTDISK_API ULONGLONG
        WINAPI
        FscryptDiskSetAPIFlags(ULONGLONG Flags);

    /**
    An interactive rundll32.exe-compatible function to show the Add New Virtual
    Disk dialog box with a file name already filled in. It is used by the
    Windows Explorer context menus.

    hWnd         Specifies a window that will be the owner window of any
    MessageBox:es or similar.

    hInst        Ignored.

    lpszCmdLine  An ANSI string specifying the image file to mount.

    nCmdShow     Ignored.
    */
    FSCRYPTDISK_API void
        WINAPI
        RunDLL_MountFile(HWND hWnd,
                         HINSTANCE hInst,
                         LPSTR lpszCmdLine,
                         int nCmdShow);

    /**
    An interactive rundll32.exe-compatible function to remove an existing FscryptDisk
    virtual disk. If the filesystem on the device cannot be locked and
    dismounted a MessageBox is displayed that asks the user if dismount should
    be forced.

    hWnd         Specifies a window that will be the owner window of any
    MessageBox:es or similar.

    hInst        Ignored.

    lpszCmdLine  An ANSI string specifying the virtual disk to remove. This
    can be on the form "F:" or "F:\" (without the quotes).

    nCmdShow     Ignored.
    */
    FSCRYPTDISK_API void
        WINAPI
        RunDLL_RemoveDevice(HWND hWnd,
                            HINSTANCE hInst,
                            LPSTR lpszCmdLine,
                            int nCmdShow);

    /**
    An interactive rundll32.exe-compatible function to save a virtual or
    physical drive as an image file. If the filesystem on the device cannot be
    locked and dismounted a MessageBox is displayed that asks the user if the
    image saving should continue anyway.

    hWnd         Specifies a window that will be the owner window of any
    MessageBox:es or similar.

    hInst        Ignored.

    lpszCmdLine  An ANSI string specifying the disk to save. This can be on
    the form "F:" or "F:\" (without the quotes).

    nCmdShow     Ignored.
    */
    FSCRYPTDISK_API void
        WINAPI
        RunDLL_SaveImageFile(HWND hWnd,
                             HINSTANCE hInst,
                             LPSTR lpszCmdLine,
                             int nCmdShow);

    /**
    This function displays a MessageBox dialog with a
    FormatMessage-formatted message.

    hWndParent   Parent window for the MessageBox call.

    uStyle       Style for the MessageBox call.

    lpTitle      Window title for the MessageBox call.

    lpMessage    Format string to be used in call to FormatMessage followed
    by field parameters.
    */
    FSCRYPTDISK_API BOOL
        CDECL
        FscryptDiskMsgBoxPrintF(IN HWND hWndParent OPTIONAL,
                                IN UINT uStyle,
                                IN LPCWSTR lpTitle,
                                IN LPCWSTR lpMessage, ...);

    /**
    Synchronously flush Windows message queue to make GUI components responsive.
    */
    FSCRYPTDISK_API VOID
        WINAPI
        FscryptDiskFlushWindowMessages(HWND hWnd);

    /**
    Used to get a string describing a partition type.

    PartitionType  Partition type from partition table.

    Name           Pointer to memory that receives a string describing the
    partition type.

    NameSize       Size of memory area pointed to by the Name parameter.
    */
    FSCRYPTDISK_API VOID
        WINAPI
        FscryptDiskGetPartitionTypeName(IN BYTE PartitionType,
                                        IN OUT LPWSTR Name,
                                        IN DWORD NameSize);

    /**
    Returns the offset in bytes to actual disk image data for some known
    "non-raw" image file formats with headers. Returns TRUE if file extension
    is recognized and the known offset has been stored in the variable pointed
    to by the Offset parameter. Otherwise the function returns FALSE and the
    value pointed to by the Offset parameter is not changed.

    ImageFile    Name of raw disk image file. This does not need to be a valid
    path or filename, just the extension is used by this function.

    Offset       Returned offset in bytes if function returns TRUE.
    */
    FSCRYPTDISK_API BOOL
        WINAPI
        FscryptDiskGetOffsetByFileExt(IN LPCWSTR ImageFile,
                                      IN OUT PLARGE_INTEGER Offset);

    /**
    Prototype for raw disk reader function used with FscryptDisk***Indirect
    functions.

    Handle                Value that was passed as first parameter to
    FscryptDiskGetPartitionInfoIndirect.

    Buffer                Buffer where read data is to be stored.

    Offset                Disk offset where read operation starts.

    NumberOfBytesToRead   Number of bytes to read from disk.

    NumberOfBytesRead     Pointer to DWORD size variable where function stores
    number of bytes actually read into Buffer. This value
    can be equal to or less than NumberOfBytesToRead
    parameter.
    */
    typedef BOOL(WINAPI *FscryptDiskReadFileProc)(IN HANDLE Handle,
                                                  IN OUT LPVOID Buffer,
                                                  IN LARGE_INTEGER Offset,
                                                  IN DWORD NumberOfBytesToRead,
                                                  IN OUT LPDWORD NumberOfBytesRead);

    /**
    Prototype for caller supplied function that receives a
    PARTITION_INFORMATION structure called one time for each partition
    that FscryptDiskGetPartitionInfoIndirectEx finds. The function should return
    TRUE to indicate that search for further partition entries should be
    done, or FALSE to stop the search. The FscryptDisk***Get*** function will then
    return FALSE to the caller with GetLastError value set to ERROR_CANCELLED.

    UserData              Value that was passed as last parameter to
    FscryptDiskGetPartitionInfoIndirectEx.

    PartitionInformation  Pointer to PARTITION_INFORMATION structure.
    */
    typedef BOOL(WINAPI *FscryptDiskGetPartitionInfoProc)(LPVOID UserData,
                                                          PPARTITION_INFORMATION PartitionInformation);

    /**
    Attempts to find partition information from a partition table for a raw
    disk image file. If no master boot record is found this function returns
    FALSE. Returns TRUE if a master boot record with a partition table is found
    and values stored in the structures pointed to by the PartitionInformation
    parameter. Otherwise the function returns FALSE.

    ImageFile    Name of raw disk image file to examine.

    SectorSize   Optional sector size used on disk if different from default
    512 bytes.

    Offset       Optional offset in bytes to master boot record within file for
    use with "non-raw" image files with headers before the actual
    disk image data.

    PartitionInformation
    Pointer to an array of eight PARTITION_INFORMATION structures
    which will receive information from four recognized primary
    partition entries followed by four recognized extended entries.
    */
    FSCRYPTDISK_API BOOL
        WINAPI
        FscryptDiskGetPartitionInformation(IN LPCWSTR ImageFile,
                                           IN DWORD SectorSize OPTIONAL,
                                           IN PLARGE_INTEGER Offset OPTIONAL,
                                           IN OUT PPARTITION_INFORMATION PartitionInformation);

    /**
    Attempts to find partition information from a partition table for a raw
    disk image file. If no master boot record is found this function returns
    FALSE. Returns TRUE if a master boot record with a partition table is found
    and a value stored in the structure pointed to by the PartitionInformation
    parameter. Otherwise the function returns FALSE.

    ImageFile    Name of raw disk image file to examine.

    SectorSize   Optional sector size used on disk if different from default
    512 bytes.

    Offset       Optional offset in bytes to master boot record within file for
    use with "non-raw" image files with headers before the actual
    disk image data.

    PartitionInformation
    Pointer to a PARTITION_INFORMATION structure that will receive information
    from a recognized partition entry.

    PartitionNumber
    Number of partition to receive information for. If there was no partition
    found with this number, the function returns FALSE and GetLastError will
    return ERROR_NOT_FOUND.
    */
    FSCRYPTDISK_API BOOL
        WINAPI
        FscryptDiskGetSinglePartitionInformation(IN LPCWSTR ImageFile,
                                                 IN DWORD SectorSize OPTIONAL,
                                                 IN PLARGE_INTEGER Offset OPTIONAL,
                                                 IN OUT PPARTITION_INFORMATION PartitionInformation,
                                                 IN INT PartitionNumber);

    /**
    Attempts to find partition information from a partition table for a raw
    disk image file.

    If no master boot record is found this function returns FALSE. Returns TRUE
    if a master boot record with a partition table is found and values where
    returned to callback function. If the callback function cancels search by
    returning FALSE, this function returns FALSE with GetLastError value set
    to ERROR_CANCELLED.

    ImageFile    Name of raw disk image file to examine.

    SectorSize   Optional sector size used on disk if different from default
    512 bytes.

    Offset       Optional offset in bytes to master boot record within file for
    use with "non-raw" image files with headers before the actual
    disk image data.

    CallBack
    Caller supplied function that is called with a PARTITION_INFORMATION
    structure for each recognized partition entry.

    UserData     Optional data to send as first parameter to callback
    function.
    */
    FSCRYPTDISK_API BOOL
        WINAPI
        FscryptDiskGetPartitionInformationEx(IN LPCWSTR ImageFile,
                                             IN DWORD SectorSize OPTIONAL,
                                             IN PLARGE_INTEGER Offset OPTIONAL,
                                             IN FscryptDiskGetPartitionInfoProc GetPartitionInfoProc,
                                             IN LPVOID UserData OPTIONAL);

    /**
    A device read function with FscryptDiskReadFileProc, which means that it can be
    used when calling FscryptDiskGetPartitionInfoIndirect function.

    Handle       Operating system file handle representing a file or device
    opened for reading.

    Buffer       Buffer where read data is to be stored.

    Offset       Disk offset where read operation starts.

    NumberOfBytesToRead
    Number of bytes to read from disk.

    NumberOfBytesRead
    Pointer to DWORD size variable where function stores number of
    bytes actually read into Buffer. This value can be equal to or
    less than NumberOfBytesToRead parameter.
    */
    FSCRYPTDISK_API BOOL
        WINAPI
        FscryptDiskReadFileHandle(IN HANDLE Handle,
                                  IN OUT LPVOID Buffer,
                                  IN LARGE_INTEGER Offset,
                                  IN DWORD NumberOfBytesToRead,
                                  IN OUT LPDWORD NumberOfBytesRead);

    /**
    Attempts to find partition information from a partition table for a disk
    image through a supplied device reader function.

    If no master boot record is found this function returns FALSE. Returns TRUE
    if a master boot record with a partition table is found and values where
    returned to callback function. If the callback function cancels search by
    returning FALSE, this function returns FALSE with GetLastError value set
    to ERROR_CANCELLED.

    Handle       Value that is passed as first parameter to ReadFileProc.

    ReadFileProc Procedure of type FscryptDiskReadFileProc that is called to read raw
    disk image.

    SectorSize   Optional sector size used on disk if different from default
    512 bytes.

    Offset       Optional offset in bytes to master boot record within file for
    use with "non-raw" image files with headers before the actual
    disk image data.

    CallBack
    Caller supplied function that is called with a PARTITION_INFORMATION
    structure for each recognized partition entry.

    UserData     Optional data to send as first parameter to callback
    function.
    */
    FSCRYPTDISK_API BOOL
        WINAPI
        FscryptDiskGetPartitionInfoIndirectEx(IN HANDLE Handle,
                                              IN FscryptDiskReadFileProc ReadFileProc,
                                              IN DWORD SectorSize OPTIONAL,
                                              IN PLARGE_INTEGER Offset OPTIONAL,
                                              IN FscryptDiskGetPartitionInfoProc GetPartitionInfoProc,
                                              IN LPVOID UserData OPTIONAL);

    /**
    Attempts to find partition information from a partition table for a disk
    image through a supplied device reader function.

    If no master boot record is found this function returns FALSE. Returns TRUE
    if a master boot record with a partition table is found and values stored in
    the structures pointed to by the PartitionInformation parameter. Otherwise
    the function returns FALSE.

    Handle       Value that is passed as first parameter to ReadFileProc.

    ReadFileProc Procedure of type FscryptDiskReadFileProc that is called to read raw
    disk image.

    SectorSize   Optional sector size used on disk if different from default
    512 bytes.

    Offset       Optional offset in bytes to master boot record within file for
    use with "non-raw" image files with headers before the actual
    disk image data.

    PartitionInformation
    Pointer to an array of eight PARTITION_INFORMATION structures
    which will receive information from four recognized primary
    partition entries followed by four recognized extended entries.
    */
    FSCRYPTDISK_API BOOL
        WINAPI
        FscryptDiskGetPartitionInfoIndirect(IN HANDLE Handle,
                                            IN FscryptDiskReadFileProc ReadFileProc,
                                            IN DWORD SectorSize OPTIONAL,
                                            IN PLARGE_INTEGER Offset OPTIONAL,
                                            IN OUT PPARTITION_INFORMATION PartitionInfo);

    /**
    Attempts to find partition information from a partition table for a disk
    image through a supplied device reader function.

    If no master boot record is found this function returns FALSE. Returns
    TRUE if a master boot record with a partition table is found and a value
    stored in the structure pointed to by the PartitionInformation parameter.
    Otherwise the function returns FALSE.

    ImageFile    Name of raw disk image file to examine.

    SectorSize   Optional sector size used on disk if different from default
    512 bytes.

    Offset       Optional offset in bytes to master boot record within file for
    use with "non-raw" image files with headers before the actual
    disk image data.

    PartitionInformation
    Pointer to a PARTITION_INFORMATION structure that will receive information
    from a recognized partition entry.

    PartitionNumber
    Number of partition to receive information for. If there was no partition
    found with this number, the function returns FALSE and GetLastError will
    return ERROR_NOT_FOUND.
    */
    FSCRYPTDISK_API BOOL
        WINAPI
        FscryptDiskGetSinglePartitionInfoIndirect(IN HANDLE Handle,
                                                  IN FscryptDiskReadFileProc ReadFileProc,
                                                  IN DWORD SectorSize OPTIONAL,
                                                  IN PLARGE_INTEGER Offset OPTIONAL,
                                                  IN OUT PPARTITION_INFORMATION PartitionInformation,
                                                  IN INT PartitionNumber);

    /**
    Finds out if image file contains an ISO9660 filesystem.

    ImageFile    Name of disk image file to examine.

    Offset       Optional offset in bytes to where raw disk data begins, for use
    with "non-raw" image files with headers before the actual disk
    image data.
    */
    FSCRYPTDISK_API BOOL
        WINAPI
        FscryptDiskImageContainsISOFS(IN LPCWSTR ImageFile,
                                      IN PLARGE_INTEGER Offset OPTIONAL);

    /**
    Finds out if image file contains an ISO9660 filesystem, through a supplied
    device reader function.

    Handle       Value that is passed as first parameter to ReadFileProc.

    ReadFileProc Procedure of type FscryptDiskReadFileProc that is called to read raw
    disk image.

    Offset       Optional offset in bytes to where raw disk data begins, for use
    with "non-raw" image files with headers before the actual disk
    image data.
    */
    FSCRYPTDISK_API BOOL
        WINAPI
        FscryptDiskImageContainsISOFSIndirect(IN HANDLE Handle,
                                              IN FscryptDiskReadFileProc ReadFileProc,
                                              IN PLARGE_INTEGER Offset OPTIONAL);

    /**
    Starts a Win32 service or loads a kernel module or driver.

    ServiceName  Key name of the service or driver.
    */
    FSCRYPTDISK_API BOOL
        WINAPI
        FscryptDiskStartService(IN LPWSTR ServiceName);

    /**
    An easy way to turn an empty NTFS directory to a reparse point that redirects
    requests to a mounted device. Acts quite like mount points or symbolic links
    in *nix. If MountPoint specifies a character followed by a colon, a drive
    letter is instead created to point to Target.

    MountPoint   Path to empty directory on an NTFS volume, or a drive letter
    followed by a colon.

    Target       Target device path on kernel object namespace form, e.g.
    \Device\FscryptDisk_APP_2 or similar.
    */
    FSCRYPTDISK_API BOOL
        WINAPI
        FscryptDiskCreateMountPoint(IN LPCWSTR MountPoint,
                                    IN LPCWSTR Target);

    /**
    Restores a reparse point to be an ordinary empty directory, or removes a
    drive letter mount point. When removing a drive letter mount point, this
    function notifies shell components that drive letter is gone unless API
    flags are set to turn off shell notifications.

    MountPoint   Path to a reparse point on an NTFS volume, or a drive letter
    followed by a colon to remove a drive letter mount point.
    */
    FSCRYPTDISK_API BOOL
        WINAPI
        FscryptDiskRemoveMountPoint(IN LPCWSTR MountPoint);

    /**
    Opens a device object in the kernel object namespace.

    FileName     Full kernel object namespace path to the object to open, e.g.
    \Device\FscryptDisk_APP_2 or similar.

    AccessMode   Access mode to request.
    */
    FSCRYPTDISK_API HANDLE
        WINAPI
        FscryptDiskOpenDeviceByName(IN PUNICODE_STRING FileName,
                                    IN DWORD AccessMode);

    /**
    Opens an FscryptDisk device by the device number.

    DeviceNumber Number of the FscryptDisk device to open.

    AccessMode   Access mode to request.
    */
    FSCRYPTDISK_API HANDLE
        WINAPI
        FscryptDiskOpenDeviceByNumber(IN DWORD DeviceNumber,
                                      IN DWORD AccessMode);

    /**
    Opens the device a junction/mount-point type reparse point is pointing to.

    MountPoint   Path to the reparse point on an NTFS volume.

    AccessMode   Access mode to request to the target device.
    */
    FSCRYPTDISK_API HANDLE
        WINAPI
        FscryptDiskOpenDeviceByMountPoint(IN LPCWSTR MountPoint,
                                          IN DWORD AccessMode);

    /**
    Check that the user-mode library and kernel-mode driver version matches for
    an open FscryptDisk created device object.

    DeviceHandle Handle to an open FscryptDisk virtual disk or control device.
    */
    FSCRYPTDISK_API BOOL
        WINAPI
        FscryptDiskCheckDriverVersion(IN HANDLE DeviceHandle);

    /**
    Retrieves the version numbers of the user-mode API library and the kernel-
    mode driver.
    */
    FSCRYPTDISK_API BOOL
        WINAPI
        FscryptDiskGetVersion(IN OUT PULONG LibraryVersion OPTIONAL,
                              IN OUT PULONG DriverVersion OPTIONAL);

    /**
    Returns the first free drive letter in the range D-Z.
    */
    FSCRYPTDISK_API WCHAR
        WINAPI
        FscryptDiskFindFreeDriveLetter();

    /**
    Returns a bit-field representing FscryptDisk devices. Bit 0 represents device 0,
    bit 1 represents device 1 and so on. A bit is 1 if the device exists or 0 if
    the device number is free.

    Compatibility notice:
    This function is exported for compatibility with FscryptDisk versions before
    1.7.0. Since that version, drives can have device numbers above 63. This
    function cannot return such device numbers, so in case any drive with device
    number above 63 exist when this function is called, it returns a value
    filled with all ones ((ULONGLONG)-1).

    Use FscryptDiskGetDeviceListEx function with newer versions of FscryptDisk.
    */
    FSCRYPTDISK_API ULONGLONG
        WINAPI
        FscryptDiskGetDeviceList();

    /**
    Builds a list of currently existing FscryptDisk virtual disks.

    ListLength      Set this parameter to number of ULONG element that can be
    store at the location pointed to by DeviceList parameter.
    This parameter must be at least 3 for this function to work
    correctly.

    DeviceList      Pointer to memory location where one ULONG, containing a
    device number, will be stored for each currently existing
    FscryptDisk device. First element in list is used to store number
    of devices.

    Upon return, first element in DeviceList will contain number of currently
    existing FscryptDisk virtual disks. If DeviceList is too small to contain all
    items as indicated by ListLength parameter, number of existing devices will
    be stored at DeviceList location, but no further items will be stored.

    If an error occurs, this function returns FALSE and GetLastError
    will return an error code. If successful, the function returns TRUE and
    first element at location pointed to by DeviceList will contain number of
    devices currently on the system, i.e. number of elements following the first
    one in DeviceList.

    If DeviceList buffer is too small, the function returns FALSE and
    GetLastError returns ERROR_MORE_DATA. In that case, only number of
    existing devices will be stored at location pointed to by DeviceList
    parameter. That value, plus one for the first length element, indicates how
    large the buffer needs to be to successfully store all items.
    */
    FSCRYPTDISK_API BOOL
        WINAPI
        FscryptDiskGetDeviceListEx(IN ULONG ListLength,
                                   OUT PULONG DeviceList);

    /**
    This function sends an IOCTL_FSCRYPTDISK_QUERY_DEVICE control code to an existing
    device and returns information about the device in an FSCRYPTDISK_CREATE_DATA
    structure.

    DeviceNumber    Number of the FscryptDisk device to query.

    CreateData      Pointer to a sufficiently large FSCRYPTDISK_CREATE_DATA
    structure to receive all data including the image file name
    where applicable.

    CreateDataSize  The size in bytes of the memory the CreateData parameter
    points to. The function call will fail if the memory is not
    large enough to hold the entire FSCRYPTDISK_CREATE_DATA
    structure.
    */
    FSCRYPTDISK_API BOOL
        WINAPI
        FscryptDiskQueryDevice(IN DWORD DeviceNumber,
                               IN OUT PFSCRYPTDISK_CREATE_DATA CreateData,
                               IN ULONG CreateDataSize);

    /**
    This function creates a new FscryptDisk virtual disk device.

    hWndStatusText  A handle to a window that can display status message text.
    The function will send WM_SETTEXT messages to this window.
    If this parameter is NULL no WM_SETTEXT messages are sent
    and the function acts non-interactive.

    DiskGeometry    The virtual geometry of the new virtual disk. Note that the
    Cylinders member does not specify the number of Cylinders
    but the total size in bytes of the new virtual disk. The
    actual number of cylinders are then automatically
    calculated and rounded down if necessary.

    The Cylinders member can be zero if the device is backed by
    an image file or a proxy device, but not if it is virtual
    memory only device.

    All or some of the other members of this structure can be
    zero in which case they are automatically filled in with
    most reasonable values by the driver.

    Flags           Bitwise or-ed combination of one of the FSCRYPTDISK_TYPE_xxx
    flags, one of the FSCRYPTDISK_DEVICE_TYPE_xxx flags and any
    number of FSCRYPTDISK_OPTION_xxx flags. The flags can often be
    left zero and left to the driver to automatically select.
    For example, if a virtual disk size is specified to 1440 KB
    and an image file name is not specified, the driver
    automatically selects FSCRYPTDISK_TYPE_VM|FSCRYPTDISK_DEVICE_TYPE_FD
    for this parameter.

    FileName        Name of disk image file. In case FSCRYPTDISK_TYPE_VM is
    specified in the Flags parameter, this file will be loaded
    into the virtual memory-backed disk when created.

    NativePath      Set to TRUE if the FileName parameter specifies an NT
    native path, such as \??\C:\imagefile.img or FALSE if it
    specifies a Win32/DOS-style path such as C:\imagefile.img.

    MountPoint      Drive letter to assign to the new virtual device. It can be
    specified on the form F: or F:\. It can also specify an empty directory
    on another NTFS volume.
    */
    FSCRYPTDISK_API BOOL
        WINAPI
        FscryptDiskCreateDevice(IN HWND hWndStatusText OPTIONAL,
                                IN OUT PDISK_GEOMETRY DiskGeometry OPTIONAL,
                                IN PLARGE_INTEGER ImageOffset OPTIONAL,
                                IN DWORD Flags OPTIONAL,
                                IN LPCWSTR FileName OPTIONAL,
                                IN BOOL NativePath,
                                IN LPWSTR MountPoint OPTIONAL);

    /**
    This function creates a new FscryptDisk virtual disk device.

    hWndStatusText  A handle to a window that can display status message text.
    The function will send WM_SETTEXT messages to this window.
    If this parameter is NULL no WM_SETTEXT messages are sent
    and the function acts non-interactive.

    DeviceNumber    In: Device number for device to create. Device number must
    not be in use by an existing virtual disk. For automatic
    allocation of device number, use FSCRYPTDISK_AUTO_DEVICE_NUMBER
    constant or specify a NULL pointer.

    Out: If DeviceNumber parameter is not NULL, device number
    for created device is returned in DWORD variable pointed to.

    DiskGeometry    The virtual geometry of the new virtual disk. Note that the
    Cylinders member does not specify the number of Cylinders
    but the total size in bytes of the new virtual disk. The
    actual number of cylinders are then automatically
    calculated and rounded down if necessary.

    The Cylinders member can be zero if the device is backed by
    an image file or a proxy device, but not if it is virtual
    memory only device.

    All or some of the other members of this structure can be
    zero in which case they are automatically filled in with
    most reasonable values by the driver.

    Flags           Bitwise or-ed combination of one of the FSCRYPTDISK_TYPE_xxx
    flags, one of the FSCRYPTDISK_DEVICE_TYPE_xxx flags and any
    number of FSCRYPTDISK_OPTION_xxx flags. The flags can often be
    left zero and left to the driver to automatically select.
    For example, if a virtual disk size is specified to 1440 KB
    and an image file name is not specified, the driver
    automatically selects FSCRYPTDISK_TYPE_VM|FSCRYPTDISK_DEVICE_TYPE_FD
    for this parameter.

    FileName        Name of disk image file. In case FSCRYPTDISK_TYPE_VM is
    specified in the Flags parameter, this file will be loaded
    into the virtual memory-backed disk when created.

    NativePath      Set to TRUE if the FileName parameter specifies an NT
    native path, such as \??\C:\imagefile.img or FALSE if it
    specifies a Win32/DOS-style path such as C:\imagefile.img.

    MountPoint      Drive letter to assign to the new virtual device. It can
    be specified on the form F: or F:\. It can also specify an empty directory
    on another NTFS volume.
    */
    FSCRYPTDISK_API BOOL
        WINAPI
        FscryptDiskCreateDeviceEx(IN HWND hWndStatusText OPTIONAL,
                                  IN OUT LPDWORD DeviceNumber OPTIONAL,
                                  IN OUT PDISK_GEOMETRY DiskGeometry OPTIONAL,
                                  IN PLARGE_INTEGER ImageOffset OPTIONAL,
                                  IN DWORD Flags OPTIONAL,
                                  IN LPCWSTR FileName OPTIONAL,
                                  IN BOOL NativePath,
                                  IN LPWSTR MountPoint OPTIONAL);

    /**
    This function removes (unmounts) an existing FscryptDisk virtual disk device.

    hWndStatusText  A handle to a window that can display status message text.
    The function will send WM_SETTEXT messages to this window.
    If this parameter is NULL no WM_SETTEXT messages are sent
    and the function acts non-interactive.

    DeviceNumber    Number of the FscryptDisk device to remove. This parameter is
    only used if MountPoint parameter is null.

    MountPoint      Drive letter of the device to remove. It can be specified
    on the form F: or F:\.
    */
    FSCRYPTDISK_API BOOL
        WINAPI
        FscryptDiskRemoveDevice(IN HWND hWndStatusText OPTIONAL,
                                IN DWORD DeviceNumber OPTIONAL,
                                IN LPCWSTR MountPoint OPTIONAL);

    /**
    This function forcefully removes (unmounts) an existing FscryptDisk virtual disk
    device. Any unsaved data will be lost.

    Device          Handle to open device. If not NULL, it is used to query
    device number to find out which device to remove. If this
    parameter is NULL the DeviceNumber parameter is used
    instead.

    DeviceNumber    Number of the FscryptDisk device to remove. This parameter is
    only used if Device parameter is NULL.
    */
    FSCRYPTDISK_API BOOL
        WINAPI
        FscryptDiskForceRemoveDevice(IN HANDLE Device OPTIONAL,
                                     IN DWORD DeviceNumber OPTIONAL);

    /**
    This function changes the device characteristics of an existing FscryptDisk
    virtual disk device.

    hWndStatusText  A handle to a window that can display status message text.
    The function will send WM_SETTEXT messages to this window.
    If this parameter is NULL no WM_SETTEXT messages are sent
    and the function acts non-interactive.

    DeviceNumber    Number of the FscryptDisk device to change. This parameter is
    only used if MountPoint parameter is null.

    MountPoint      Drive letter of the device to change. It can be specified
    on the form F: or F:\.

    FlagsToChange   A bit-field specifying which flags to edit. The flags are
    the same as the option flags in the Flags parameter used
    when a new virtual disk is created. Only flags set in this
    parameter are changed to the corresponding flag value in the
    Flags parameter.

    Flags           New values for the flags specified by the FlagsToChange
    parameter.
    */
    FSCRYPTDISK_API BOOL
        WINAPI
        FscryptDiskChangeFlags(HWND hWndStatusText OPTIONAL,
                               DWORD DeviceNumber OPTIONAL,
                               LPCWSTR MountPoint OPTIONAL,
                               DWORD FlagsToChange,
                               DWORD Flags);

    /**
    This function extends the size of an existing FscryptDisk virtual disk device.

    hWndStatusText  A handle to a window that can display status message text.
    The function will send WM_SETTEXT messages to this window.
    If this parameter is NULL no WM_SETTEXT messages are sent
    and the function acts non-interactive.

    DeviceNumber    Number of the FscryptDisk device to extend.

    ExtendSize      A pointer to a LARGE_INTEGER structure that specifies the
    number of bytes to extend the device.
    */
    FSCRYPTDISK_API BOOL
        WINAPI
        FscryptDiskExtendDevice(IN HWND hWndStatusText OPTIONAL,
                                IN DWORD DeviceNumber,
                                IN CONST PLARGE_INTEGER ExtendSize);

    /**
    This function saves the contents of a device to an image file.

    DeviceHandle    Handle to a device for which the contents are to be saved to
    an image file.

    The handle must be opened for reading, may be
    opened for sequential scan and/or without intermediate
    buffering but cannot be opened for overlapped operation.
    Please note that a call to this function will turn on
    FSCTL_ALLOW_EXTENDED_DASD_IO on for this handle.

    FileHandle      Handle to an image file opened for writing. The handle
    can be opened for operation without intermediate buffering
    but performance is usually better if the handle is opened
    with intermediate buffering. The handle cannot be opened for
    overlapped operation.

    BufferSize      I/O buffer size to use when reading source disk. This
    parameter is optional, if it is zero the buffer size to use
    will automatically chosen.

    CancelFlag      Optional pointer to a BOOL value. If this BOOL value is set
    to TRUE during the function call the operation is cancelled,
    the function returns FALSE and GetLastError will return
    ERROR_CANCELLED. If this parameter is non-null the function
    will also dispatch window messages for the current thread
    between each I/O operation.
    */
    FSCRYPTDISK_API BOOL
        WINAPI
        FscryptDiskSaveImageFile(IN HANDLE DeviceHandle,
                                 IN HANDLE FileHandle,
                                 IN DWORD BufferSize OPTIONAL,
                                 IN LPBOOL CancelFlag OPTIONAL);

    /**
    This function gets the size of a disk volume.

    Handle          Handle to a disk volume device.

    Size            Pointer to a 64 bit variable that upon successful completion
    receives disk volume size as a signed integer.
    */
    FSCRYPTDISK_API BOOL
        WINAPI
        FscryptDiskGetVolumeSize(IN HANDLE Handle,
                                 IN OUT PLONGLONG Size);

    /**
    Reads formatted geometry for a volume by parsing BPB, BIOS Parameter Block,
    from volume boot record into a DISK_GEOMETRY structure.

    If no boot record signature is found, this function returns FALSE.

    ImageFile    Path to a volume image file or a device path to a disk volume,
    such as \\.\A: or \\.\C:.

    Offset       Optional offset in bytes to volume boot record within file for
    use with "non-raw" volume image files. This parameter can be
    used to for example skip over headers for specific disk image
    formats, or to skip over master boot record in a disk image
    file that contains a complete raw disk image and not only a
    single volume.

    DiskGeometry Pointer to DISK_GEOMETRY structure that receives information
    about formatted geometry. This function zeros the Cylinders
    member.
    */
    FSCRYPTDISK_API BOOL
        WINAPI
        FscryptDiskGetFormattedGeometry(IN LPCWSTR ImageFile,
                                        IN PLARGE_INTEGER Offset OPTIONAL,
                                        IN OUT PDISK_GEOMETRY DiskGeometry);

    /**
    Reads formatted geometry for a volume by parsing BPB, BIOS Parameter Block,
    from volume boot record into a DISK_GEOMETRY structure.

    If no boot record signature is found, this function returns FALSE.

    Handle       Value that is passed as first parameter to ReadFileProc.

    ReadFileProc Procedure of type FscryptDiskReadFileProc that is called to read
    disk volume.

    Offset       Optional offset in bytes to volume boot record within file for
    use with "non-raw" volume image files. This parameter can be
    used to for example skip over headers for specific disk image
    formats, or to skip over master boot record in a disk image
    file that contains a complete raw disk image and not only a
    single volume.

    DiskGeometry Pointer to DISK_GEOMETRY structure that receives information
    about formatted geometry. This function zeros the Cylinders
    member.
    */
    FSCRYPTDISK_API BOOL
        WINAPI
        FscryptDiskGetFormattedGeometryIndirect(IN HANDLE Handle,
                                                IN FscryptDiskReadFileProc ReadFileProc,
                                                IN PLARGE_INTEGER Offset OPTIONAL,
                                                IN OUT PDISK_GEOMETRY DiskGeometry);

    /**
    This function builds a Master Boot Record, MBR, in memory. The MBR will
    contain a default Initial Program Loader, IPL, which could be used to boot
    an operating system partition when the MBR is written to a disk.

    DiskGeometry    Pointer to a DISK_GEOMETRY or DISK_GEOMETRY_EX structure
    that contains information about logical geometry of the
    disk.

    This function only uses the BytesPerSector, SectorsPerTrack
    and TracksPerCylinder members.

    This parameter can be NULL if NumberOfParts parameter is
    zero.

    PartitionInfo   Pointer to an array of up to four PARTITION_INFORMATION
    structures containing information about partitions to store
    in MBR partition table.

    This function only uses the StartingOffset, PartitionLength,
    BootIndicator and PartitionType members.

    This parameter can be NULL if NumberOfParts parameter is
    zero.

    NumberOfParts   Number of PARTITION_INFORMATION structures in array that
    PartitionInfo parameter points to.

    If this parameter is zero, DiskGeometry and PartitionInfo
    parameters are ignored and can be NULL. In that case MBR
    will contain an empty partition table when this function
    returns.

    MBR             Pointer to memory buffer of at least 512 bytes where MBR
    will be built.

    MBRSize         Size of buffer pointed to by MBR parameter. This parameter
    must be at least 512.
    */
    FSCRYPTDISK_API BOOL
        WINAPI
        FscryptDiskBuildMBR(IN PDISK_GEOMETRY DiskGeometry OPTIONAL,
                            IN PPARTITION_INFORMATION PartitionInfo OPTIONAL,
                            IN BYTE NumberOfParts OPTIONAL,
                            IN OUT LPBYTE MBR,
                            IN DWORD_PTR MBRSize);

    /**
    This function converts a CHS disk address to LBA format.

    DiskGeometry    Pointer to a DISK_GEOMETRY or DISK_GEOMETRY_EX structure
    that contains information about logical geometry of the
    disk. This function only uses the SectorsPerTrack and
    TracksPerCylinder members.

    CHS             Pointer to CHS disk address in three-byte partition table
    style format.
    */
    FSCRYPTDISK_API DWORD
        WINAPI
        FscryptDiskConvertCHSToLBA(IN PDISK_GEOMETRY DiskGeometry,
                                   IN LPBYTE CHS);

    /**
    This function converts an LBA disk address to three-byte partition style CHS
    format. The three bytes are returned in the three lower bytes of a DWORD.

    DiskGeometry    Pointer to a DISK_GEOMETRY or DISK_GEOMETRY_EX structure
    that contains information about logical geometry of the
    disk. This function only uses the SectorsPerTrack and
    TracksPerCylinder members.

    LBA             LBA disk address.
    */
    FSCRYPTDISK_API DWORD
        WINAPI
        FscryptDiskConvertLBAToCHS(IN PDISK_GEOMETRY DiskGeometry,
                                   IN DWORD LBA);

    /**
    This function adjusts size of a saved image file. If file size is less than
    requested disk size, the size will be left unchanged with return value FALSE
    and GetLastError will return ERROR_DISK_OPERATION_FAILED.

    FileHandle      Handle to file where disk image has been saved.

    FileSize        Size of original disk which image file should be adjusted
    to.
    */
    FSCRYPTDISK_API BOOL
        WINAPI
        FscryptDiskAdjustImageFileSize(IN HANDLE FileHandle,
                                       IN PLARGE_INTEGER FileSize);

    /**
    This function converts a native NT-style path to a Win32 DOS-style path. The
    path string is converted in-place and the start address is adjusted to skip
    over native directories such as \??\. Because of this, the Path parameter is
    a pointer to a pointer to a string so that the pointer can be adjusted to
    the new start address.

    Path            Pointer to pointer to Path string in native NT-style format.
    Upon return the pointed address will contain the start
    address of the Win32 DOS-style path within the original
    buffer.
    */
    FSCRYPTDISK_API VOID
        WINAPI
        FscryptDiskNativePathToWin32(IN OUT LPWSTR *Path);

    /**
    This function saves the contents of a device to an image file. This is a
    user-interactive function that displays dialog boxes where user can select
    image file and other options.

    DeviceHandle    Handle to a device for which the contents are to be saved to
    an image file.

    The handle must be opened for reading, may be
    opened for sequential scan and/or without intermediate
    buffering but cannot be opened for overlapped operation.
    Please note that a call to this function will turn on
    FSCTL_ALLOW_EXTENDED_DASD_IO on for this handle.

    WindowHandle    Handle to existing window that will be parent to dialog
    boxes etc.

    BufferSize      I/O buffer size to use when reading source disk. This
    parameter is optional, if it is zero the buffer size to use
    will automatically chosen.

    IsCdRomType     If this parameter is TRUE and the source device type cannot
    be automatically determined this function will ask user for
    a .iso suffixed image file name.
    */
    FSCRYPTDISK_API VOID
        WINAPI
        FscryptDiskSaveImageFileInteractive(IN HANDLE DeviceHandle,
                                            IN HWND WindowHandle OPTIONAL,
                                            IN DWORD BufferSize OPTIONAL,
                                            IN BOOL IsCdRomType OPTIONAL);

    /*
    Opens or creates a global synchronization event. This event is shared with
    FscryptDisk driver and will be pulsed when an FscryptDisk device is created, removed
    or have settings changed in some other way.

    This is particularly useful for user interface components that need to be
    notified when device lists and similar need to be updated.

    If successful, this function returns a handle to an event that can be used
    in call to system wait functions, such as WaitForSingleObject. When the
    handle is not needed, it must be closed by calling CloseHandle.

    If the function fails, it returns NULL and GetLastError will return a
    system error code that further explains the error.

    InheritHandle   Specifies whether or not the returned handle will be
    inherited by child processes.

    */
    FSCRYPTDISK_API HANDLE
        WINAPI
        FscryptDiskOpenRefreshEvent(BOOL InheritHandle);

    /*
    Adds registry settings for creating a virtual disk at system startup (or
    when driver is loaded).

    This function returns TRUE if successful, FALSE otherwise. If FALSE is
    returned, GetLastError could be used to get actual error code.

    CreateData      Pointer to FSCRYPTDISK_CREATE_DATA structure that contains
    device creation settings to save.

    */
    FSCRYPTDISK_API BOOL
        WINAPI
        FscryptDiskSaveRegistrySettings(PFSCRYPTDISK_CREATE_DATA CreateData);

    /*
    Remove registry settings for creating a virtual disk at system startup (or
    when driver is loaded).

    This function returns TRUE if successful, FALSE otherwise. If FALSE is
    returned, GetLastError could be used to get actual error code.

    DeviceNumber    Device number specified in registry settings.
    */
    FSCRYPTDISK_API BOOL
        WINAPI
        FscryptDiskRemoveRegistrySettings(DWORD DeviceNumber);

    /*
    Retrieves number of auto-loading devices at system startup, or when driver
    is loaded. This is the value of the LoadDevices registry value for
    fscryptdisk.sys driver.

    This function returns TRUE if successful, FALSE otherwise. If FALSE is
    returned, GetLastError could be used to get actual error code.

    LoadDevicesValue
    Pointer to variable that receives the value.
    */
    FSCRYPTDISK_API BOOL
        WINAPI
        FscryptDiskGetRegistryAutoLoadDevices(LPDWORD LoadDevicesValue);

    /*
    Notify Explorer and other shell components that a new drive letter has
    been created. Called automatically by device creation after creating a
    drive letter. If no drive letter was created by a device creation routine
    or if API flags was set to turn off shell notification during device
    creation, this function can be called manually later.

    Note that calling this function has no effect if API flags are set to
    turn off shell notifications, or if supplied drive letter path does not
    specify an A-Z drive letter.

    This function returns TRUE if successful, FALSE otherwise. If FALSE is
    returned, GetLastError could be used to get actual error code.

    hWnd
    Window handle to use as parent handle for any message boxes. If this
    parameter is NULL, no message boxes are displayed.

    DriveLetterPath
    Drive letter path in one of formats A:\ or A:.
    */
    FSCRYPTDISK_API BOOL
        WINAPI
        FscryptDiskNotifyShellDriveLetter(HWND hWnd,
                                          LPWSTR DriveLetterPath);

    /*
    Notify Explorer and other shell components that a drive is about to be
    removed.

    hWnd
    Window handle to use as parent handle for any message boxes. If this
    parameter is NULL, no message boxes are displayed.

    DriveLetter
    Drive letter.
    */
    FSCRYPTDISK_API BOOL
        WINAPI
        FscryptDiskNotifyRemovePending(HWND hWnd,
                                       WCHAR DriveLetter);

    FSCRYPTDISK_API LPWSTR
        CDECL
        FscryptDiskAllocPrintF(LPCWSTR lpMessage, ...);

    FSCRYPTDISK_API LPSTR
        CDECL
        FscryptDiskAllocPrintFA(LPCSTR lpMessage, ...);

    FSCRYPTDISK_API int
        WINAPI
        FscryptDiskConsoleMessageA(
            HWND hWnd,
            LPCSTR lpText,
            LPCSTR lpCaption,
            UINT uType);

    FSCRYPTDISK_API int
        WINAPI
        FscryptDiskConsoleMessageW(
            HWND hWnd,
            LPCWSTR lpText,
            LPCWSTR lpCaption,
            UINT uType);

    FSCRYPTDISK_API BOOL
        WINAPI
        FscryptDiskIsProcessElevated();

#ifdef CORE_BUILD

#ifdef CharToOemA
#undef CharToOemA
#endif

#define CharToOemA(s, t)

#ifdef MessageBoxA
#undef MessageBoxA
#endif
#ifdef MessageBoxW
#undef MessageBoxW
#endif
#ifdef SetWindowTextA
#undef SetWindowTextA
#endif
#ifdef SetWindowTextW
#undef SetWindowTextW
#endif

#define MessageBoxA FscryptDiskConsoleMessageA
#define MessageBoxW FscryptDiskConsoleMessageW

#define SetWindowTextA(h, t) FscryptDiskConsoleMessageA(h, t, "", 0)
#define SetWindowTextW(h, t) FscryptDiskConsoleMessageW(h, t, L"", 0)

#endif

#ifdef __cplusplus
}
#endif

#endif

#endif // _INC_FSCRYPTDISK_
