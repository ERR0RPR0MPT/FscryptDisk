#Disable Warning CA1401 ' P/Invokes should not be visible
#Disable Warning CA1711

Imports System.Runtime.InteropServices
Imports Microsoft.Win32.SafeHandles

Namespace FscryptDisk

    <ComVisible(False)>
    Public NotInheritable Class UnsafeNativeMethods

        Private Sub New()
        End Sub

        ''' <summary>
        ''' FscryptDisk API behaviour flags.
        ''' </summary>
        <Flags>
        Public Enum FscryptDiskAPIFlags As UInt64

            ''' <summary>
            ''' If set, no broadcast window messages are sent on creation and removal of drive letters.
            ''' </summary>
            NoBroadcastNotify = &H1

            ''' <summary>
            ''' If set, RemoveDevice() will automatically force a dismount of filesystem invalidating
            ''' any open handles.
            ''' </summary>
            ForceDismount = &H2

        End Enum

        Public Declare Unicode Function FscryptDiskGetAPIFlags _
          Lib "fscryptdisk.cpl" (
          ) As FscryptDiskAPIFlags

        Public Declare Unicode Function FscryptDiskSetAPIFlags _
          Lib "fscryptdisk.cpl" (
            Flags As FscryptDiskAPIFlags
          ) As FscryptDiskAPIFlags

        Public Declare Unicode Function FscryptDiskCheckDriverVersion _
          Lib "fscryptdisk.cpl" (
            Handle As SafeFileHandle
          ) As Boolean

        Public Declare Unicode Function FscryptDiskStartService _
          Lib "fscryptdisk.cpl" (
            <MarshalAs(UnmanagedType.LPWStr), [In]> ServiceName As String
          ) As Boolean

        Public Declare Unicode Function FscryptDiskGetOffsetByFileExt _
          Lib "fscryptdisk.cpl" (
            <MarshalAs(UnmanagedType.LPWStr), [In]> ImageFileName As String,
            ByRef Offset As Int64
          ) As Boolean

        Public Declare Unicode Function FscryptDiskGetPartitionInformation _
          Lib "fscryptdisk.cpl" (
            <MarshalAs(UnmanagedType.LPWStr), [In]> ImageFileName As String,
            SectorSize As UInt32,
            <[In]> ByRef Offset As Int64,
            PartitionInformation As IntPtr
          ) As Boolean

        Public Declare Unicode Function FscryptDiskGetPartitionInformation _
          Lib "fscryptdisk.cpl" (
            <MarshalAs(UnmanagedType.LPWStr), [In]> ImageFileName As String,
            SectorSize As UInt32,
            <[In]> ByRef Offset As Int64,
            <MarshalAs(UnmanagedType.LPArray), Out> PartitionInformation As NativeFileIO.UnsafeNativeMethods.PARTITION_INFORMATION()
          ) As Boolean

        Public Declare Unicode Function FscryptDiskGetPartitionInformationEx _
          Lib "fscryptdisk.cpl" (
            <MarshalAs(UnmanagedType.LPWStr), [In]> ImageFileName As String,
            SectorSize As UInt32,
            <[In]> ByRef Offset As Int64,
            <MarshalAs(UnmanagedType.FunctionPtr)> PartitionInformationProc As FscryptDiskGetPartitionInfoProc,
            UserData As IntPtr
          ) As Boolean

        Public Declare Unicode Function FscryptDiskGetSinglePartitionInformation _
          Lib "fscryptdisk.cpl" (
            <MarshalAs(UnmanagedType.LPWStr), [In]> ImageFileName As String,
            SectorSize As UInt32,
            <[In]> ByRef Offset As Int64,
            <Out> ByRef PartitionInformation As NativeFileIO.UnsafeNativeMethods.PARTITION_INFORMATION
          ) As Boolean

        Public Delegate Function FscryptDiskReadFileManagedProc _
          (
            Handle As IntPtr,
            <MarshalAs(UnmanagedType.LPArray, SizeParamIndex:=3), Out> Buffer As Byte(),
            Offset As Int64,
            NumberOfBytes As UInt32,
            <Out> ByRef NumberOfBytesRead As UInt32
          ) As Boolean

        Public Delegate Function FscryptDiskReadFileUnmanagedProc _
          (
            Handle As IntPtr,
            Buffer As IntPtr,
            Offset As Int64,
            NumberOfBytes As UInt32,
            <Out> ByRef NumberOfBytesRead As UInt32
          ) As Boolean

        Public Delegate Function FscryptDiskGetPartitionInfoProc _
          (
            UserData As IntPtr,
            <[In]> ByRef PartitionInformation As NativeFileIO.UnsafeNativeMethods.PARTITION_INFORMATION
          ) As Boolean

        Public Declare Unicode Function FscryptDiskReadFileHandle _
          Lib "fscryptdisk.cpl" (
            Handle As IntPtr,
            <MarshalAs(UnmanagedType.LPArray, SizeParamIndex:=3), Out> Buffer As Byte(),
            Offset As Int64,
            NumberOfBytes As UInt32,
            <Out> ByRef NumberOfBytesRead As UInt32
          ) As Boolean

        Public Declare Unicode Function FscryptDiskGetPartitionInfoIndirect _
          Lib "fscryptdisk.cpl" (
            Handle As IntPtr,
            <MarshalAs(UnmanagedType.FunctionPtr)> ReadFileProc As FscryptDiskReadFileManagedProc,
            SectorSize As UInt32,
            <[In]> ByRef Offset As Int64,
            PartitionInformation As IntPtr
          ) As Boolean

        Public Declare Unicode Function FscryptDiskGetPartitionInfoIndirect _
          Lib "fscryptdisk.cpl" (
            Handle As IntPtr,
            <MarshalAs(UnmanagedType.FunctionPtr)> ReadFileProc As FscryptDiskReadFileManagedProc,
            SectorSize As UInt32,
            <[In]> ByRef Offset As Int64,
            <MarshalAs(UnmanagedType.LPArray), Out> PartitionInformation As NativeFileIO.UnsafeNativeMethods.PARTITION_INFORMATION()
          ) As Boolean

        Public Declare Unicode Function FscryptDiskGetPartitionInfoIndirect _
          Lib "fscryptdisk.cpl" (
            Handle As IntPtr,
            <MarshalAs(UnmanagedType.FunctionPtr)> ReadFileProc As FscryptDiskReadFileUnmanagedProc,
            SectorSize As UInt32,
            <[In]> ByRef Offset As Int64,
            PartitionInformation As IntPtr
          ) As Boolean

        Public Declare Unicode Function FscryptDiskGetPartitionInfoIndirect _
          Lib "fscryptdisk.cpl" (
            Handle As IntPtr,
            <MarshalAs(UnmanagedType.FunctionPtr)> ReadFileProc As FscryptDiskReadFileUnmanagedProc,
            SectorSize As UInt32,
            <[In]> ByRef Offset As Int64,
            <MarshalAs(UnmanagedType.LPArray), Out> PartitionInformation As NativeFileIO.UnsafeNativeMethods.PARTITION_INFORMATION()
          ) As Boolean

        Public Declare Unicode Function FscryptDiskGetPartitionInfoIndirectEx _
          Lib "fscryptdisk.cpl" (
            Handle As IntPtr,
            <MarshalAs(UnmanagedType.FunctionPtr)> ReadFileProc As FscryptDiskReadFileManagedProc,
            SectorSize As UInt32,
            <[In]> ByRef Offset As Int64,
            <MarshalAs(UnmanagedType.FunctionPtr)> PartitionInformationProc As FscryptDiskGetPartitionInfoProc,
            UserData As IntPtr
          ) As Boolean

        Public Declare Unicode Function FscryptDiskGetPartitionInfoIndirectEx _
          Lib "fscryptdisk.cpl" (
            Handle As IntPtr,
            <MarshalAs(UnmanagedType.FunctionPtr)> ReadFileProc As FscryptDiskReadFileUnmanagedProc,
            SectorSize As UInt32,
            <[In]> ByRef Offset As Int64,
            <MarshalAs(UnmanagedType.FunctionPtr)> PartitionInformationProc As FscryptDiskGetPartitionInfoProc,
            UserData As IntPtr
          ) As Boolean

        Public Declare Unicode Function FscryptDiskGetSinglePartitionInfoIndirect _
          Lib "fscryptdisk.cpl" (
            Handle As IntPtr,
            <MarshalAs(UnmanagedType.FunctionPtr)> ReadFileProc As FscryptDiskReadFileManagedProc,
            SectorSize As UInt32,
            <[In]> ByRef Offset As Int64,
            <Out> ByRef PartitionInformation As NativeFileIO.UnsafeNativeMethods.PARTITION_INFORMATION
          ) As Boolean

        Public Declare Unicode Function FscryptDiskImageContainsISOFS _
          Lib "fscryptdisk.cpl" (
            <MarshalAs(UnmanagedType.LPWStr), [In]> ImageFileName As String,
            <[In]> ByRef Offset As Int64
          ) As Boolean

        Public Declare Unicode Function FscryptDiskImageContainsISOFSIndirect _
          Lib "fscryptdisk.cpl" (
            Handle As IntPtr,
            <MarshalAs(UnmanagedType.FunctionPtr)> ReadFileProc As FscryptDiskReadFileManagedProc,
            <[In]> ByRef Offset As Int64
          ) As Boolean

        Public Declare Unicode Function FscryptDiskImageContainsISOFSIndirect _
          Lib "fscryptdisk.cpl" (
            Handle As IntPtr,
            <MarshalAs(UnmanagedType.FunctionPtr)> ReadFileProc As FscryptDiskReadFileUnmanagedProc,
            <[In]> ByRef Offset As Int64
          ) As Boolean

        Public Declare Unicode Function FscryptDiskGetFormattedGeometry _
          Lib "fscryptdisk.cpl" (
            <MarshalAs(UnmanagedType.LPWStr), [In]> ImageFileName As String,
            <[In]> ByRef Offset As Int64,
            <Out> ByRef DiskGeometry As NativeFileIO.UnsafeNativeMethods.DISK_GEOMETRY
          ) As Boolean

        Public Declare Unicode Function FscryptDiskGetFormattedGeometryIndirect _
          Lib "fscryptdisk.cpl" (
            Handle As IntPtr,
            <MarshalAs(UnmanagedType.FunctionPtr)> ReadFileProc As FscryptDiskReadFileManagedProc,
            <[In]> ByRef Offset As Int64,
            <Out> ByRef DiskGeometry As NativeFileIO.UnsafeNativeMethods.DISK_GEOMETRY
          ) As Boolean

        Public Declare Unicode Function FscryptDiskGetFormattedGeometryIndirect _
          Lib "fscryptdisk.cpl" (
            Handle As IntPtr,
            <MarshalAs(UnmanagedType.FunctionPtr)> ReadFileProc As FscryptDiskReadFileUnmanagedProc,
            <[In]> ByRef Offset As Int64,
            <Out> ByRef DiskGeometry As NativeFileIO.UnsafeNativeMethods.DISK_GEOMETRY
          ) As Boolean

        Public Declare Unicode Function FscryptDiskCreateMountPoint _
          Lib "fscryptdisk.cpl" (
            <MarshalAs(UnmanagedType.LPWStr), [In]> Directory As String,
            <MarshalAs(UnmanagedType.LPWStr), [In]> Target As String
          ) As Boolean

        Public Declare Unicode Function FscryptDiskRemoveMountPoint _
          Lib "fscryptdisk.cpl" (
            <MarshalAs(UnmanagedType.LPWStr), [In]> MountPoint As String
          ) As Boolean

        Public Declare Unicode Function FscryptDiskOpenDeviceByNumber _
          Lib "fscryptdisk.cpl" (
            DeviceNumber As UInt32,
            AccessMode As UInt32
          ) As SafeFileHandle

        Public Declare Unicode Function FscryptDiskOpenDeviceByMountPoint _
          Lib "fscryptdisk.cpl" (
            <MarshalAs(UnmanagedType.LPWStr), [In]> MountPoint As String,
            AccessMode As UInt32
          ) As SafeFileHandle

        Public Declare Unicode Function FscryptDiskGetVolumeSize _
          Lib "fscryptdisk.cpl" (
            Handle As SafeFileHandle,
            ByRef Size As Int64
          ) As Boolean

        Public Declare Unicode Function FscryptDiskSaveImageFile _
          Lib "fscryptdisk.cpl" (
            DeviceHandle As SafeFileHandle,
            FileHandle As SafeFileHandle,
            BufferSize As UInt32,
            CancelFlagPtr As IntPtr
          ) As Boolean

        Public Declare Unicode Function FscryptDiskExtendDevice _
          Lib "fscryptdisk.cpl" (
            hWndStatusText As IntPtr,
            DeviceNumber As UInt32,
            ByRef ExtendSize As Int64
          ) As Boolean

        Public Declare Unicode Function FscryptDiskCreateDevice _
          Lib "fscryptdisk.cpl" (
            hWndStatusText As IntPtr,
            ByRef DiskGeometry As NativeFileIO.UnsafeNativeMethods.DISK_GEOMETRY,
            ByRef ImageOffset As Int64,
            Flags As UInt32,
            <MarshalAs(UnmanagedType.LPWStr), [In]> Filename As String,
            <MarshalAs(UnmanagedType.Bool)> NativePath As Boolean,
            <MarshalAs(UnmanagedType.LPWStr), [In]> MountPoint As String
          ) As Boolean

        Public Declare Unicode Function FscryptDiskCreateDeviceEx _
          Lib "fscryptdisk.cpl" (
            hWndStatusText As IntPtr,
            ByRef DeviceNumber As UInt32,
            ByRef DiskGeometry As NativeFileIO.UnsafeNativeMethods.DISK_GEOMETRY,
            ByRef ImageOffset As Int64,
            Flags As UInt32,
            <MarshalAs(UnmanagedType.LPWStr), [In]> Filename As String,
            <MarshalAs(UnmanagedType.Bool)> NativePath As Boolean,
            <MarshalAs(UnmanagedType.LPWStr), [In]> MountPoint As String
          ) As Boolean

        Public Declare Unicode Function FscryptDiskRemoveDevice _
          Lib "fscryptdisk.cpl" (
            hWndStatusText As IntPtr,
            DeviceNumber As UInt32,
            <MarshalAs(UnmanagedType.LPWStr), [In]> MountPoint As String
          ) As Boolean

        Public Declare Unicode Function FscryptDiskForceRemoveDevice _
          Lib "fscryptdisk.cpl" (
            DeviceHandle As IntPtr,
            DeviceNumber As UInt32
          ) As Boolean

        Public Declare Unicode Function FscryptDiskForceRemoveDevice _
          Lib "fscryptdisk.cpl" (
            DeviceHandle As SafeFileHandle,
            DeviceNumber As UInt32
          ) As Boolean

        Public Declare Unicode Function FscryptDiskChangeFlags _
          Lib "fscryptdisk.cpl" (
            hWndStatusText As IntPtr,
            DeviceNumber As UInt32,
            <MarshalAs(UnmanagedType.LPWStr), [In]> MountPoint As String,
            FlagsToChange As UInt32,
            Flags As UInt32
          ) As Boolean

        Public Declare Unicode Function FscryptDiskQueryDevice _
          Lib "fscryptdisk.cpl" (
            DeviceNumber As UInt32,
            <MarshalAs(UnmanagedType.LPArray, SizeParamIndex:=2), Out> CreateData As Byte(),
            CreateDataSize As Int32
          ) As Boolean

        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
        <ComVisible(False)>
        Public Structure FscryptDiskCreateData
            Public Property DeviceNumber As Int32
            Private ReadOnly _Dummy As Int32
            Public Property DiskSize As Int64
            Public Property MediaType As Int32
            Public Property TracksPerCylinder As UInt32
            Public Property SectorsPerTrack As UInt32
            Public Property BytesPerSector As UInt32
            Public Property ImageOffset As Int64
            Public Property Flags As FscryptDiskFlags
            Public Property DriveLetter As Char
            Private _FilenameLength As UInt16

            <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=16384)>
            Private _Filename As String

            Public Property Filename As String
                Get
                    If _Filename IsNot Nothing AndAlso _Filename.Length > _FilenameLength \ 2 Then
                        _Filename = _Filename.Remove(_FilenameLength \ 2)
                    End If
                    Return _Filename
                End Get
                Set
                    If Value Is Nothing Then
                        _Filename = Nothing
                        _FilenameLength = 0
                        Return
                    End If
                    _Filename = Value
                    _FilenameLength = CUShort(_Filename.Length * 2)
                End Set
            End Property
        End Structure

        Public Declare Unicode Function FscryptDiskQueryDevice _
          Lib "fscryptdisk.cpl" (
            DeviceNumber As UInt32,
            <Out> ByRef CreateData As FscryptDiskCreateData,
            CreateDataSize As Int32
          ) As Boolean

        Public Declare Unicode Function FscryptDiskFindFreeDriveLetter _
          Lib "fscryptdisk.cpl" (
          ) As Char

        <Obsolete("This method only supports a maximum of 64 simultaneously mounted devices. Use FscryptDiskGetDeviceListEx instead.")>
        Public Declare Unicode Function FscryptDiskGetDeviceList _
          Lib "fscryptdisk.cpl" (
          ) As UInt64

        Public Declare Unicode Function FscryptDiskGetDeviceListEx _
          Lib "fscryptdisk.cpl" (
            ListLength As Int32,
            <MarshalAs(UnmanagedType.LPArray)> DeviceList As Int32()
          ) As Boolean

        Public Declare Unicode Function FscryptDiskBuildMBR _
          Lib "fscryptdisk.cpl" (
            <[In]> ByRef DiskGeometry As NativeFileIO.UnsafeNativeMethods.DISK_GEOMETRY,
            <MarshalAs(UnmanagedType.LPArray), [In]> PartitionInfo As NativeFileIO.UnsafeNativeMethods.PARTITION_INFORMATION(),
            NumberOfParts As Byte,
            <MarshalAs(UnmanagedType.LPArray)> MBR As Byte(),
            MBRSize As IntPtr
          ) As Boolean

        Public Declare Unicode Function FscryptDiskConvertCHSToLBA _
          Lib "fscryptdisk.cpl" (
            <[In]> ByRef DiskGeometry As NativeFileIO.UnsafeNativeMethods.DISK_GEOMETRY,
            <MarshalAs(UnmanagedType.LPArray)> CHS As Byte()
          ) As UInt32

        Public Declare Unicode Function FscryptDiskConvertLBAToCHS _
          Lib "fscryptdisk.cpl" (
            <[In]> ByRef DiskGeometry As NativeFileIO.UnsafeNativeMethods.DISK_GEOMETRY,
            LBA As UInt32
          ) As UInt32

        Public Declare Unicode Sub FscryptDiskSaveImageFileInteractive _
          Lib "fscryptdisk.cpl" (
            DeviceHandle As SafeFileHandle,
            WindowHandle As IntPtr,
            BufferSize As UInt32,
            <MarshalAs(UnmanagedType.Bool)> IsCdRomType As Boolean
          )

        Public Declare Unicode Function FscryptDiskOpenRefreshEvent _
          Lib "fscryptdisk.cpl" (
            <MarshalAs(UnmanagedType.Bool)> InheritHandle As Boolean
          ) As SafeWaitHandle

        Public Declare Unicode Function FscryptDiskSaveRegistrySettings _
            Lib "fscryptdisk.cpl" _
            (<[In]> ByRef CreateData As FscryptDiskCreateData
             ) As Boolean

        Public Declare Unicode Function FscryptDiskRemoveRegistrySettings _
            Lib "fscryptdisk.cpl" _
            (DeviceNumber As UInt32
             ) As Boolean

        Public Declare Unicode Function FscryptDiskGetRegistryAutoLoadDevices _
            Lib "fscryptdisk.cpl" _
            (<Out> ByRef LoadDevicesValue As UInt32
             ) As Boolean

        Public Declare Unicode Function FscryptDiskNotifyShellDriveLetter _
            Lib "fscryptdisk.cpl" _
            (WindowHandle As IntPtr,
             <MarshalAs(UnmanagedType.LPWStr), [In]> DriveLetterPath As String
             ) As Boolean

        Public Declare Unicode Function FscryptDiskNotifyRemovePending _
            Lib "fscryptdisk.cpl" _
            (WindowHandle As IntPtr,
             DriveLetter As Char
             ) As Boolean

    End Class

End Namespace
