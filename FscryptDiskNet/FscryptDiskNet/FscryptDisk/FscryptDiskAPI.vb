Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Threading

Namespace FscryptDisk

    ''' <summary>
    ''' FscryptDisk API for sending commands to FscryptDisk Virtual Disk Driver from .NET applications.
    ''' </summary>
    <ComVisible(False)>
    Public Class FscryptDiskAPI

        Private Sub New()

        End Sub

        Private Shared ReadOnly _RefreshListeners As New List(Of EventHandler)

        Private Shared ReadOnly _EventListenerThread As New Thread(AddressOf RefreshEventThread)

        Private Shared ReadOnly _ThreadStopEvent As New EventWaitHandle(initialState:=False, mode:=EventResetMode.ManualReset)

        Private Shared Sub RefreshEventThread()
            Using RefreshEvent = OpenRefreshEvent()
                Do
                    Dim wait_handles As WaitHandle() = {_ThreadStopEvent, RefreshEvent}
                    Dim wait_result = WaitHandle.WaitAny(wait_handles)
                    If wait_handles(wait_result) Is _ThreadStopEvent Then
                        Exit Do
                    End If
                    RaiseEvent DriveListChanged(Nothing, EventArgs.Empty)
                Loop
            End Using
        End Sub

        ''' <summary>
        ''' This event is fired when drives are added or removed, or when parameters and options
        ''' are changed for a drive.
        ''' </summary>
        Public Shared Custom Event DriveListChanged As EventHandler
            AddHandler(value As EventHandler)
                SyncLock _RefreshListeners
                    If Not _EventListenerThread.IsAlive Then
                        _ThreadStopEvent.Reset()
                        _EventListenerThread.Start()
                    End If
                    _RefreshListeners.Add(value)
                End SyncLock
            End AddHandler

            RemoveHandler(value As EventHandler)
                SyncLock _RefreshListeners
                    _RefreshListeners.Remove(value)

                    If _RefreshListeners.Count = 0 AndAlso
                        _EventListenerThread.IsAlive Then

                        _ThreadStopEvent.Set()
                        _EventListenerThread.Join()
                    End If
                End SyncLock
            End RemoveHandler

            RaiseEvent(sender As Object, e As EventArgs)
                SyncLock _RefreshListeners
                    _RefreshListeners.ForEach(Sub(eh) eh(sender, e))
                End SyncLock
            End RaiseEvent
        End Event

        ''' <summary>
        ''' FscryptDisk API behaviour flags.
        ''' </summary>
        Public Shared Property APIFlags As UnsafeNativeMethods.FscryptDiskAPIFlags
            Get
                Return UnsafeNativeMethods.FscryptDiskGetAPIFlags()
            End Get
            Set
                UnsafeNativeMethods.FscryptDiskSetAPIFlags(Value)
            End Set
        End Property

        ''' <summary>
        ''' Opens a synchronization event that can be used with wait functions to
        ''' synchronize with change events in FscryptDisk driver. Event is fired when
        ''' for example a drive is added or removed, or when some options or
        ''' settings are changed for an existing drive.
        ''' </summary>
        Public Shared Function OpenRefreshEvent() As FscryptDiskRefreshEvent

            Return New FscryptDiskRefreshEvent(InheritHandle:=False)

        End Function

        ''' <summary>
        ''' Opens a synchronization event that can be used with wait functions to
        ''' synchronize with change events in FscryptDisk driver. Event is fired when
        ''' for example a drive is added or removed, or when some options or
        ''' settings are changed for an existing drive.
        ''' </summary>
        Public Shared Function OpenRefreshEvent(InheritHandle As Boolean) As FscryptDiskRefreshEvent

            Return New FscryptDiskRefreshEvent(InheritHandle)

        End Function

        ''' <summary>
        ''' Checks if filename contains a known extension for which FscryptDisk knows of a constant offset value. That value can be
        ''' later passed as Offset parameter to CreateDevice method.
        ''' </summary>
        ''' <param name="ImageFile">Name of disk image file.</param>
        Public Shared Function GetOffsetByFileExt(ImageFile As String) As Long

            Dim Offset As Long
            If UnsafeNativeMethods.FscryptDiskGetOffsetByFileExt(ImageFile, Offset) Then
                Return Offset
            Else
                Return 0
            End If

        End Function

        Private Shared Function GetStreamReaderFunction(stream As Stream) As UnsafeNativeMethods.FscryptDiskReadFileManagedProc

            Return _
                Function(_Handle As IntPtr,
                         _Buffer As Byte(),
                         _Offset As Int64,
                         _NumberOfBytesToRead As UInt32,
                         ByRef _NumberOfBytesRead As UInt32) As Boolean

                    Try
                        stream.Position = _Offset
                        _NumberOfBytesRead = CUInt(stream.Read(_Buffer, 0, CInt(_NumberOfBytesToRead)))
                        Return True

                    Catch
                        Return False

                    End Try

                End Function

        End Function

        ''' <summary>
        ''' Parses partition table entries from a master boot record and extended partition table record, if any.
        ''' </summary>
        ''' <param name="ImageFile">Name of image file to examine.</param>
        ''' <param name="SectorSize">Sector size for translating sector values to absolute byte positions. This
        ''' parameter is in most cases 512.</param>
        ''' <param name="Offset">Offset in image file where master boot record is located.</param>
        ''' <returns>An array of PARTITION_INFORMATION structures</returns>
        Public Shared Function GetPartitionInformation(ImageFile As String, SectorSize As UInt32, Offset As Long) As ReadOnlyCollection(Of NativeFileIO.UnsafeNativeMethods.PARTITION_INFORMATION)

            Dim PartitionInformation As New List(Of NativeFileIO.UnsafeNativeMethods.PARTITION_INFORMATION)

            NativeFileIO.Win32Try(UnsafeNativeMethods.FscryptDiskGetPartitionInformationEx(ImageFile, SectorSize, Offset,
                                                                      Function(data, ByRef info)
                                                                          PartitionInformation.Add(info)
                                                                          Return True
                                                                      End Function,
                                                                      Nothing))

            Return PartitionInformation.AsReadOnly()

        End Function

        ''' <summary>
        ''' Parses partition table entries from a master boot record and extended partition table record, if any.
        ''' </summary>
        ''' <param name="ImageFile">Disk image to examine.</param>
        ''' <param name="SectorSize">Sector size for translating sector values to absolute byte positions. This
        ''' parameter is in most cases 512.</param>
        ''' <param name="Offset">Offset in image file where master boot record is located.</param>
        ''' <returns>An array of PARTITION_INFORMATION structures</returns>
        Public Shared Function GetPartitionInformation(ImageFile As Stream, SectorSize As UInt32, Offset As Long) As ReadOnlyCollection(Of NativeFileIO.UnsafeNativeMethods.PARTITION_INFORMATION)

            Dim StreamReader = GetStreamReaderFunction(ImageFile)

            Dim PartitionInformation As New List(Of NativeFileIO.UnsafeNativeMethods.PARTITION_INFORMATION)

            NativeFileIO.Win32Try(UnsafeNativeMethods.FscryptDiskGetPartitionInfoIndirectEx(Nothing, StreamReader, SectorSize, Offset,
                Function(data, ByRef info)
                    PartitionInformation.Add(info)
                    Return True
                End Function,
                Nothing))

            Return PartitionInformation.AsReadOnly()

        End Function

        ''' <summary>
        ''' Parses partition table entries from a master boot record and extended partition table record, if any.
        ''' </summary>
        ''' <param name="Handle">Value to pass as first parameter to ReadFileProc.</param>
        ''' <param name="ReadFileProc">Reference to method that reads disk image.</param>
        ''' <param name="SectorSize">Sector size for translating sector values to absolute byte positions. This
        ''' parameter is in most cases 512.</param>
        ''' <param name="Offset">Offset in image file where master boot record is located.</param>
        ''' <returns>An array of PARTITION_INFORMATION structures</returns>
        Public Shared Function GetPartitionInformation(Handle As IntPtr, ReadFileProc As UnsafeNativeMethods.FscryptDiskReadFileManagedProc, SectorSize As UInt32, Offset As Long) As ReadOnlyCollection(Of NativeFileIO.UnsafeNativeMethods.PARTITION_INFORMATION)

            Dim PartitionInformation As New List(Of NativeFileIO.UnsafeNativeMethods.PARTITION_INFORMATION)

            NativeFileIO.Win32Try(UnsafeNativeMethods.FscryptDiskGetPartitionInfoIndirectEx(Handle, ReadFileProc, SectorSize, Offset,
                                                                       Function(data, ByRef info)
                                                                           PartitionInformation.Add(info)
                                                                           Return True
                                                                       End Function,
                                                                       Nothing))

            Return PartitionInformation.AsReadOnly()

        End Function

        ''' <summary>
        ''' Parses partition table entries from a master boot record and extended partition table record, if any.
        ''' </summary>
        ''' <param name="Handle">Value to pass as first parameter to ReadFileProc.</param>
        ''' <param name="ReadFileProc">Reference to method that reads disk image.</param>
        ''' <param name="SectorSize">Sector size for translating sector values to absolute byte positions. This
        ''' parameter is in most cases 512.</param>
        ''' <returns>An array of PARTITION_INFORMATION structures</returns>
        Public Shared Function GetPartitionInformation(Handle As IntPtr, ReadFileProc As UnsafeNativeMethods.FscryptDiskReadFileManagedProc, SectorSize As UInt32) As ReadOnlyCollection(Of NativeFileIO.UnsafeNativeMethods.PARTITION_INFORMATION)

            Return GetPartitionInformation(Handle, ReadFileProc, SectorSize, 0)

        End Function

        ''' <summary>
        ''' Parses partition table entries from a master boot record and extended partition table record, if any.
        ''' </summary>
        ''' <param name="Handle">Value to pass as first parameter to ReadFileProc.</param>
        ''' <param name="ReadFileProc">Reference to method that reads disk image.</param>
        ''' <param name="SectorSize">Sector size for translating sector values to absolute byte positions. This
        ''' parameter is in most cases 512.</param>
        ''' <param name="Offset">Offset in image file where master boot record is located.</param>
        ''' <returns>An array of PARTITION_INFORMATION structures</returns>
        Public Shared Function GetPartitionInformation(Handle As IntPtr, ReadFileProc As UnsafeNativeMethods.FscryptDiskReadFileUnmanagedProc, SectorSize As UInt32, Offset As Long) As ReadOnlyCollection(Of NativeFileIO.UnsafeNativeMethods.PARTITION_INFORMATION)

            Dim PartitionInformation As New List(Of NativeFileIO.UnsafeNativeMethods.PARTITION_INFORMATION)

            NativeFileIO.Win32Try(UnsafeNativeMethods.FscryptDiskGetPartitionInfoIndirectEx(Handle, ReadFileProc, SectorSize, Offset,
                                                                       Function(data, ByRef info)
                                                                           PartitionInformation.Add(info)
                                                                           Return True
                                                                       End Function,
                                                                       Nothing))

            Return PartitionInformation.AsReadOnly()

        End Function

        ''' <summary>
        ''' Parses partition table entries from a master boot record and extended partition table record, if any.
        ''' </summary>
        ''' <param name="Handle">Value to pass as first parameter to ReadFileProc.</param>
        ''' <param name="ReadFileProc">Reference to method that reads disk image.</param>
        ''' <param name="SectorSize">Sector size for translating sector values to absolute byte positions. This
        ''' parameter is in most cases 512.</param>
        ''' <returns>An array of PARTITION_INFORMATION structures</returns>
        Public Shared Function GetPartitionInformation(Handle As IntPtr, ReadFileProc As UnsafeNativeMethods.FscryptDiskReadFileUnmanagedProc, SectorSize As UInt32) As ReadOnlyCollection(Of NativeFileIO.UnsafeNativeMethods.PARTITION_INFORMATION)

            Return GetPartitionInformation(Handle, ReadFileProc, SectorSize, 0)

        End Function

        ''' <summary>
        ''' Parses partition table entries from a master boot record and extended partition table record, if any.
        ''' </summary>
        ''' <param name="ImageFile">Name of image file to examine</param>
        ''' <param name="SectorSize">Sector size for translating sector values to absolute byte positions. This
        ''' parameter is in most cases 512.</param>
        ''' <returns>An array of PARTITION_INFORMATION structures</returns>
        Public Shared Function GetPartitionInformation(ImageFile As String, SectorSize As UInt32) As ReadOnlyCollection(Of NativeFileIO.UnsafeNativeMethods.PARTITION_INFORMATION)

            Return GetPartitionInformation(ImageFile, SectorSize, 0)

        End Function

        ''' <summary>
        ''' Creates a new collection of partition table entries that only contains those entries
        ''' from source sequence with valid partition definitions.
        ''' </summary>
        ''' <param name="PartitionList">Sequence of partition table entries</param>
        Public Shared Function FilterDefinedPartitions(PartitionList As IEnumerable(Of NativeFileIO.UnsafeNativeMethods.PARTITION_INFORMATION)) As ReadOnlyCollection(Of NativeFileIO.UnsafeNativeMethods.PARTITION_INFORMATION)

            If PartitionList Is Nothing Then
                Throw New ArgumentNullException(NameOf(PartitionList))
            End If

            Dim DefinedPartitions As New List(Of NativeFileIO.UnsafeNativeMethods.PARTITION_INFORMATION)(7)
            For Each PartitionInfo In PartitionList
                If PartitionInfo.PartitionLength <> 0 AndAlso
                  Not PartitionInfo.IsContainerPartition Then

                    DefinedPartitions.Add(PartitionInfo)
                End If
            Next
            Return DefinedPartitions.AsReadOnly()
        End Function

        ''' <summary>
        ''' Checks whether an image file contains an ISO9660 filesystem.
        ''' </summary>
        ''' <param name="Imagefile">Path to a volume image file or a device path to a disk volume,
        ''' such as \\.\A: or \\.\C:.</param>
        ''' <param name="Offset">Optional offset in bytes to where raw disk data begins, for use
        ''' with "non-raw" image files with headers before the actual disk image data.</param>
        Public Shared Function ImageContainsISOFS(Imagefile As String, Offset As Int64) As Boolean
            Dim rc = UnsafeNativeMethods.FscryptDiskImageContainsISOFS(Imagefile, Offset)
            If rc Then
                Return True
            ElseIf Marshal.GetLastWin32Error() = 0 Then
                Return False
            Else
                Throw New Win32Exception
            End If
        End Function

        ''' <summary>
        ''' Checks whether an image file contains an ISO9660 filesystem.
        ''' </summary>
        ''' <param name="Imagefile">Open stream that can be used to read the image file.</param>
        ''' <param name="Offset">Optional offset in bytes to where raw disk data begins, for use
        ''' with "non-raw" image files with headers before the actual disk image data.</param>
        Public Shared Function ImageContainsISOFS(Imagefile As Stream, Offset As Int64) As Boolean
            Dim rc = UnsafeNativeMethods.FscryptDiskImageContainsISOFSIndirect(Nothing, GetStreamReaderFunction(Imagefile), Offset)
            If rc Then
                Return True
            ElseIf Marshal.GetLastWin32Error() = 0 Then
                Return False
            Else
                Throw New Win32Exception
            End If
        End Function

        ''' <summary>
        '''    Reads formatted geometry for a volume by parsing BPB, BIOS Parameter Block,
        '''    from volume boot record into a DISK_GEOMETRY structure.
        '''
        '''    If no boot record signature is found, an exception is thrown.
        ''' </summary>
        ''' <param name="Imagefile">Path to a volume image file or a device path to a disk volume,
        ''' such as \\.\A: or \\.\C:.</param>
        ''' <param name="Offset">Optional offset in bytes to volume boot record within file for
        ''' use with "non-raw" volume image files. This parameter can be used to for example
        ''' skip over headers for specific disk image formats, or to skip over master boot
        ''' record in a disk image file that contains a complete raw disk image and not only a
        ''' single volume.</param>
        ''' <returns>A DISK_GEOMETRY structure that receives information about formatted geometry.
        ''' This function zeroes the Cylinders member.</returns>
        Public Shared Function GetFormattedGeometry(Imagefile As String, Offset As Int64) As NativeFileIO.UnsafeNativeMethods.DISK_GEOMETRY
            Dim DiskGeometry As NativeFileIO.UnsafeNativeMethods.DISK_GEOMETRY
            NativeFileIO.Win32Try(UnsafeNativeMethods.FscryptDiskGetFormattedGeometry(Imagefile, Offset, DiskGeometry))
            Return DiskGeometry
        End Function

        ''' <summary>
        '''    Reads formatted geometry for a volume by parsing BPB, BIOS Parameter Block,
        '''    from volume boot record into a DISK_GEOMETRY structure.
        '''
        '''    If no boot record signature is found, an exception is thrown.
        ''' </summary>
        ''' <param name="Imagefile">Open stream that can be used to read from volume image.</param>
        ''' <param name="Offset">Optional offset in bytes to volume boot record within file for
        ''' use with "non-raw" volume image files. This parameter can be used to for example
        ''' skip over headers for specific disk image formats, or to skip over master boot
        ''' record in a disk image file that contains a complete raw disk image and not only a
        ''' single volume.</param>
        ''' <returns>A DISK_GEOMETRY structure that receives information about formatted geometry.
        ''' This function zeroes the Cylinders member.</returns>
        Public Shared Function GetFormattedGeometry(Imagefile As Stream, Offset As Int64) As NativeFileIO.UnsafeNativeMethods.DISK_GEOMETRY
            Dim DiskGeometry As NativeFileIO.UnsafeNativeMethods.DISK_GEOMETRY
            NativeFileIO.Win32Try(UnsafeNativeMethods.FscryptDiskGetFormattedGeometryIndirect(Nothing, GetStreamReaderFunction(Imagefile), Offset, DiskGeometry))
            Return DiskGeometry
        End Function

        ''' <summary>
        ''' Combines GetOffsetByFileExt() and GetPartitionInformation() so that both format-specific offset and 
        ''' offset to first partition is combined into resulting Offset. If a partition was found, size of it is
        ''' also returned in the Size parameter.
        ''' </summary>
        ''' <param name="Imagefile">Name of image file to examine</param>
        ''' <param name="SectorSize">Sector size for translating sector values to absolute byte positions. This
        ''' parameter is in most cases 512.</param>
        ''' <param name="Offset">Absolute offset in image file where volume data begins</param>
        ''' <param name="Size">Size of partition if a partition table was found, otherwise zero</param>
        ''' <remarks></remarks>
        Public Shared Sub AutoFindOffsetAndSize(Imagefile As String,
                                                SectorSize As UInt32,
                                                <Out> ByRef Offset As Long,
                                                <Out> ByRef Size As Long)

            Offset = 0
            Size = 0

            Try
                Offset = FscryptDiskAPI.GetOffsetByFileExt(Imagefile)

                Dim PartitionList = FscryptDiskAPI.FilterDefinedPartitions(FscryptDiskAPI.GetPartitionInformation(Imagefile, SectorSize, Offset))
                If PartitionList Is Nothing OrElse PartitionList.Count = 0 Then
                    Exit Try
                End If
                If PartitionList(0).StartingOffset > 0 AndAlso
                      PartitionList(0).PartitionLength > 0 AndAlso
                      Not PartitionList(0).IsContainerPartition Then

                    Offset += PartitionList(0).StartingOffset
                    Size = PartitionList(0).PartitionLength
                End If

            Catch

            End Try

        End Sub

        ''' <summary>
        ''' Loads FscryptDisk Virtual Disk Driver into Windows kernel. This driver is needed to create FscryptDisk virtual disks. For
        ''' this method to be called successfully, driver needs to be installed and caller needs permission to load kernel mode
        ''' drivers.
        ''' </summary>
        Public Shared Sub LoadDriver()

            NativeFileIO.Win32Try(UnsafeNativeMethods.FscryptDiskStartService("FscryptDisk"))

        End Sub

        ''' <summary>
        ''' Starts FscryptDisk Virtual Disk Driver Helper Service. This service is needed to create proxy type FscryptDisk virtual disks
        ''' where the I/O proxy application is called through TCP/IP or a serial communications port. For
        ''' this method to be called successfully, service needs to be installed and caller needs permission to start services.
        ''' </summary>
        ''' <remarks></remarks>
        Public Shared Sub LoadHelperService()

            NativeFileIO.Win32Try(UnsafeNativeMethods.FscryptDiskStartService("FscryptDskSvc"))

        End Sub

        ''' <summary>
        ''' An easy way to turn an empty NTFS directory to a reparsepoint that redirects
        ''' requests to a mounted device. Acts quite like mount points or symbolic links
        ''' in *nix. If MountPoint specifies a character followed by a colon, a drive
        ''' letter is instead created to point to Target.
        ''' </summary>
        ''' <param name="Directory">Path to empty directory on an NTFS volume, or a drive letter
        ''' followed by a colon.</param>
        ''' <param name="Target">Target path in native format, for example \Device\FscryptDisk_APP_0</param>
        Public Shared Sub CreateMountPoint(Directory As String, Target As String)

            NativeFileIO.Win32Try(UnsafeNativeMethods.FscryptDiskCreateMountPoint(Directory, Target))

        End Sub

        ''' <summary>
        ''' An easy way to turn an empty NTFS directory to a reparsepoint that redirects
        ''' requests to an FscryptDisk device. Acts quite like mount points or symbolic links
        ''' in *nix. If MountPoint specifies a character followed by a colon, a drive
        ''' letter is instead created to point to Target.
        ''' </summary>
        ''' <param name="Directory">Path to empty directory on an NTFS volume, or a drive letter
        ''' followed by a colon.</param>
        ''' <param name="DeviceNumber">Device number of an existing FscryptDisk virtual disk</param>
        Public Shared Sub CreateMountPoint(Directory As String, DeviceNumber As UInt32)

            NativeFileIO.Win32Try(UnsafeNativeMethods.FscryptDiskCreateMountPoint(Directory, "\Device\FscryptDisk_APP_" & DeviceNumber))

        End Sub

        ''' <summary>
        ''' Restores a reparsepoint to be an ordinary empty directory, or removes a drive
        ''' letter mount point.
        ''' </summary>
        ''' <param name="MountPoint">Path to a reparse point on an NTFS volume, or a drive
        ''' letter followed by a colon to remove a drive letter mount point.</param>
        Public Shared Sub RemoveMountPoint(MountPoint As String)

            NativeFileIO.Win32Try(UnsafeNativeMethods.FscryptDiskRemoveMountPoint(MountPoint))

        End Sub

        ''' <summary>
        ''' Returns first free drive letter available.
        ''' </summary>
        Public Shared Function FindFreeDriveLetter() As Char

            Return UnsafeNativeMethods.FscryptDiskFindFreeDriveLetter()

        End Function

        ''' <summary>
        ''' Retrieves a list of virtual disks on this system. Each element in returned list holds a device number of a loaded
        ''' FscryptDisk virtual disk.
        ''' </summary>
        Public Shared Function GetDeviceList() As List(Of Integer)

            Dim NativeList(0 To 2) As Integer

            For i = 0 To 1

                If UnsafeNativeMethods.FscryptDiskGetDeviceListEx(NativeList.Length, NativeList) Then
                    Exit For
                End If

                Dim errorcode = Marshal.GetLastWin32Error()

                Select Case errorcode

                    Case NativeFileIO.UnsafeNativeMethods.ERROR_MORE_DATA
                        Array.Resize(NativeList, NativeList(0) + 1)
                        Continue For

                    Case Else
                        Throw New Win32Exception(errorcode)

                End Select

            Next

            Array.Resize(NativeList, NativeList(0) + 1)

            Dim List As New List(Of Integer)(NativeList)

            List.RemoveAt(0)

            Return List

        End Function

        ''' <summary>
        ''' Extends size of an existing FscryptDisk virtual disk.
        ''' </summary>
        ''' <param name="DeviceNumber">Device number of FscryptDisk virtual disk to extend.</param>
        ''' <param name="ExtendSize">Size to add.</param>
        ''' <param name="StatusControl">Optional handle to control that can display status messages during operation.</param>
        Public Shared Sub ExtendDevice(DeviceNumber As UInt32, ExtendSize As Int64, StatusControl As IntPtr)

            NativeFileIO.Win32Try(UnsafeNativeMethods.FscryptDiskExtendDevice(StatusControl, DeviceNumber, ExtendSize))

        End Sub

        ''' <summary>
        ''' Extends size of an existing FscryptDisk virtual disk.
        ''' </summary>
        ''' <param name="DeviceNumber">Device number of FscryptDisk virtual disk to extend.</param>
        ''' <param name="ExtendSize">Size to add.</param>
        Public Shared Sub ExtendDevice(DeviceNumber As UInt32, ExtendSize As Int64)

            ExtendDevice(DeviceNumber, ExtendSize, Nothing)

        End Sub

        ''' <summary>
        ''' Creates a new FscryptDisk virtual disk.
        ''' </summary>
        ''' <param name="DiskSize">Size of virtual disk. If this parameter is zero, current size of disk image file will
        ''' automatically be used as virtual disk size.</param>
        ''' <param name="TracksPerCylinder">Number of tracks per cylinder for virtual disk geometry. This parameter can be zero
        '''  in which case most reasonable value will be automatically used by the driver.</param>
        ''' <param name="SectorsPerTrack">Number of sectors per track for virtual disk geometry. This parameter can be zero
        '''  in which case most reasonable value will be automatically used by the driver.</param>
        ''' <param name="BytesPerSector">Number of bytes per sector for virtual disk geometry. This parameter can be zero
        '''  in which case most reasonable value will be automatically used by the driver.</param>
        ''' <param name="ImageOffset">A skip offset if virtual disk data does not begin immediately at start of disk image file.
        ''' Frequently used with image formats like Nero NRG which start with a file header not used by FscryptDisk or Windows
        ''' filesystem drivers.</param>
        ''' <param name="Flags">Flags specifying properties for virtual disk. See comments for each flag value.</param>
        ''' <param name="Filename">Name of disk image file to use or create. If disk image file already exists the DiskSize
        ''' parameter can be zero in which case current disk image file size will be used as virtual disk size. If Filename
        ''' paramter is Nothing/null disk will be created in virtual memory and not backed by a physical disk image file.</param>
        ''' <param name="NativePath">Specifies whether Filename parameter specifies a path in Windows native path format, the
        ''' path format used by drivers in Windows NT kernels, for example \Device\Harddisk0\Partition1\imagefile.img. If this
        ''' parameter is False path in FIlename parameter will be interpreted as an ordinary user application path.</param>
        ''' <param name="MountPoint">Mount point in the form of a drive letter and colon to create for newly created virtual
        ''' disk. If this parameter is Nothing/null the virtual disk will be created without a drive letter.</param>
        ''' <param name="StatusControl">Optional handle to control that can display status messages during operation.</param>
        Public Shared Sub CreateDevice(DiskSize As Int64,
                                       TracksPerCylinder As UInt32,
                                       SectorsPerTrack As UInt32,
                                       BytesPerSector As UInt32,
                                       ImageOffset As Int64,
                                       Flags As FscryptDiskFlags,
                                       Filename As String,
                                       NativePath As Boolean,
                                       MountPoint As String,
                                       StatusControl As IntPtr)

            Dim DiskGeometry As New NativeFileIO.UnsafeNativeMethods.DISK_GEOMETRY With {
              .Cylinders = DiskSize,
              .TracksPerCylinder = TracksPerCylinder,
              .SectorsPerTrack = SectorsPerTrack,
              .BytesPerSector = BytesPerSector
            }

            NativeFileIO.Win32Try(UnsafeNativeMethods.FscryptDiskCreateDevice(StatusControl,
                                                         DiskGeometry,
                                                         ImageOffset,
                                                         Flags,
                                                         Filename,
                                                         NativePath,
                                                         MountPoint))

        End Sub

        ''' <summary>
        ''' Creates a new FscryptDisk virtual disk.
        ''' </summary>
        ''' <param name="ImageOffset">A skip offset if virtual disk data does not begin immediately at start of disk image file.
        ''' Frequently used with image formats like Nero NRG which start with a file header not used by FscryptDisk or Windows
        ''' filesystem drivers.</param>
        ''' <param name="Flags">Flags specifying properties for virtual disk. See comments for each flag value.</param>
        ''' <param name="Filename">Name of disk image file to use or create. If disk image file already exists the DiskSize
        ''' parameter can be zero in which case current disk image file size will be used as virtual disk size. If Filename
        ''' paramter is Nothing/null disk will be created in virtual memory and not backed by a physical disk image file.</param>
        ''' <param name="NativePath">Specifies whether Filename parameter specifies a path in Windows native path format, the
        ''' path format used by drivers in Windows NT kernels, for example \Device\Harddisk0\Partition1\imagefile.img. If this
        ''' parameter is False path in FIlename parameter will be interpreted as an ordinary user application path.</param>
        ''' <param name="MountPoint">Mount point in the form of a drive letter and colon to create for newly created virtual
        ''' disk. If this parameter is Nothing/null the virtual disk will be created without a drive letter.</param>
        ''' <param name="DeviceNumber">In: Device number for device to create. Device number must not be in use by an existing
        ''' virtual disk. For automatic allocation of device number, pass UInt32.MaxValue.
        '''
        ''' Out: Device number for created device.</param>
        Public Shared Sub CreateDevice(ImageOffset As Int64,
                                       Flags As FscryptDiskFlags,
                                       Filename As String,
                                       NativePath As Boolean,
                                       MountPoint As String,
                                       ByRef DeviceNumber As UInt32)

            Dim DiskGeometry As New NativeFileIO.UnsafeNativeMethods.DISK_GEOMETRY

            NativeFileIO.Win32Try(UnsafeNativeMethods.FscryptDiskCreateDeviceEx(IntPtr.Zero,
                                                           DeviceNumber,
                                                           DiskGeometry,
                                                           ImageOffset,
                                                           Flags,
                                                           Filename,
                                                           NativePath,
                                                           MountPoint))

        End Sub

        ''' <summary>
        ''' Creates a new memory backed FscryptDisk virtual disk with the specified size in bytes.
        ''' </summary>
        ''' <param name="DiskSize">Size of virtual disk.</param>
        ''' <param name="MountPoint">Mount point in the form of a drive letter and colon to create for newly created virtual
        ''' disk. If this parameter is Nothing/null the virtual disk will be created without a drive letter.</param>
        ''' <param name="DeviceNumber">In: Device number for device to create. Device number must not be in use by an existing
        ''' virtual disk. For automatic allocation of device number, pass UInt32.MaxValue.
        '''
        ''' Out: Device number for created device.</param>
        Public Shared Sub CreateDevice(DiskSize As Int64,
                                       MountPoint As String,
                                       ByRef DeviceNumber As UInt32)

            Dim DiskGeometry As New NativeFileIO.UnsafeNativeMethods.DISK_GEOMETRY With {
              .Cylinders = DiskSize
            }

            NativeFileIO.Win32Try(UnsafeNativeMethods.FscryptDiskCreateDeviceEx(IntPtr.Zero,
                                                           DeviceNumber,
                                                           DiskGeometry,
                                                           0,
                                                           0,
                                                           Nothing,
                                                           Nothing,
                                                           MountPoint))

        End Sub

        Public Enum MemoryType

            ''' <summary>
            ''' Virtual memory, allocated directly by FscryptDisk driver.
            ''' </summary>
            VirtualMemory

            ''' <summary>
            ''' Physical memory, allocated through AWE_FS_Alloc driver.
            ''' </summary>
            PhysicalMemory

        End Enum

        ''' <summary>
        ''' Creates a new memory backed FscryptDisk virtual disk with the specified size in bytes, or with disk volume data from an
        ''' image file. Memory could be either virtual memory allocated directly by FscryptDisk driver, or physical memory allocated
        ''' by AWE_FS_Alloc driver.
        ''' </summary>
        ''' <param name="DiskSize">Size of virtual disk. This parameter can be zero if ImageFile parameter specifies an image
        ''' file, in which case the size of the existing image file will be used as size of the newly created virtual disk
        ''' volume.</param>
        ''' <param name="MountPoint">Mount point in the form of a drive letter and colon to create for newly created virtual
        ''' disk. If this parameter is Nothing/null the virtual disk will be created without a drive letter.</param>
        ''' <param name="DeviceNumber">In: Device number for device to create. Device number must not be in use by an existing
        ''' virtual disk. For automatic allocation of device number, pass UInt32.MaxValue.
        '''
        ''' Out: Device number for created device.</param>
        ''' <param name="ImageFile">Optional name of image file that will be loaded onto the newly created memory disk.</param>
        ''' <param name="MemoryType">Specifies whether to use virtual or physical memory for the virtual disk.</param>
        Public Shared Sub CreateDevice(DiskSize As Int64,
                                       ImageFile As String,
                                       MemoryType As MemoryType,
                                       MountPoint As String,
                                       ByRef DeviceNumber As UInt32)

            Dim DiskGeometry As New NativeFileIO.UnsafeNativeMethods.DISK_GEOMETRY With {
              .Cylinders = DiskSize
            }

            NativeFileIO.Win32Try(UnsafeNativeMethods.FscryptDiskCreateDeviceEx(IntPtr.Zero,
                                                           DeviceNumber,
                                                           DiskGeometry,
                                                           0,
                                                           If(MemoryType = FscryptDiskAPI.MemoryType.PhysicalMemory,
                                                              FscryptDiskFlags.TypeFile Or FscryptDiskFlags.FileTypeAwe,
                                                              FscryptDiskFlags.TypeVM),
                                                           ImageFile,
                                                           Nothing,
                                                           MountPoint))

        End Sub

        ''' <summary>
        ''' Creates a new FscryptDisk virtual disk.
        ''' </summary>
        ''' <param name="DiskSize">Size of virtual disk. If this parameter is zero, current size of disk image file will
        ''' automatically be used as virtual disk size.</param>
        ''' <param name="TracksPerCylinder">Number of tracks per cylinder for virtual disk geometry. This parameter can be zero
        '''  in which case most reasonable value will be automatically used by the driver.</param>
        ''' <param name="SectorsPerTrack">Number of sectors per track for virtual disk geometry. This parameter can be zero
        '''  in which case most reasonable value will be automatically used by the driver.</param>
        ''' <param name="BytesPerSector">Number of bytes per sector for virtual disk geometry. This parameter can be zero
        '''  in which case most reasonable value will be automatically used by the driver.</param>
        ''' <param name="ImageOffset">A skip offset if virtual disk data does not begin immediately at start of disk image file.
        ''' Frequently used with image formats like Nero NRG which start with a file header not used by FscryptDisk or Windows
        ''' filesystem drivers.</param>
        ''' <param name="Flags">Flags specifying properties for virtual disk. See comments for each flag value.</param>
        ''' <param name="Filename">Name of disk image file to use or create. If disk image file already exists the DiskSize
        ''' parameter can be zero in which case current disk image file size will be used as virtual disk size. If Filename
        ''' paramter is Nothing/null disk will be created in virtual memory and not backed by a physical disk image file.</param>
        ''' <param name="NativePath">Specifies whether Filename parameter specifies a path in Windows native path format, the
        ''' path format used by drivers in Windows NT kernels, for example \Device\Harddisk0\Partition1\imagefile.img. If this
        ''' parameter is False path in FIlename parameter will be interpreted as an ordinary user application path.</param>
        ''' <param name="MountPoint">Mount point in the form of a drive letter and colon to create for newly created virtual
        ''' disk. If this parameter is Nothing/null the virtual disk will be created without a drive letter.</param>
        ''' <param name="DeviceNumber">In: Device number for device to create. Device number must not be in use by an existing
        ''' virtual disk. For automatic allocation of device number, pass UInt32.MaxValue.
        '''
        ''' Out: Device number for created device.</param>
        ''' <param name="StatusControl">Optional handle to control that can display status messages during operation.</param>
        Public Shared Sub CreateDevice(DiskSize As Int64,
                                       TracksPerCylinder As UInt32,
                                       SectorsPerTrack As UInt32,
                                       BytesPerSector As UInt32,
                                       ImageOffset As Int64,
                                       Flags As FscryptDiskFlags,
                                       Filename As String,
                                       NativePath As Boolean,
                                       MountPoint As String,
                                       ByRef DeviceNumber As UInt32,
                                       StatusControl As IntPtr)

            Dim DiskGeometry As New NativeFileIO.UnsafeNativeMethods.DISK_GEOMETRY With {
              .Cylinders = DiskSize,
              .TracksPerCylinder = TracksPerCylinder,
              .SectorsPerTrack = SectorsPerTrack,
              .BytesPerSector = BytesPerSector
            }

            NativeFileIO.Win32Try(UnsafeNativeMethods.FscryptDiskCreateDeviceEx(StatusControl,
                                                           DeviceNumber,
                                                           DiskGeometry,
                                                           ImageOffset,
                                                           Flags,
                                                           Filename,
                                                           NativePath,
                                                           MountPoint))

        End Sub

        ''' <summary>
        ''' Removes an existing FscryptDisk virtual disk from system.
        ''' </summary>
        ''' <param name="DeviceNumber">Device number to remove.</param>
        Public Shared Sub RemoveDevice(DeviceNumber As UInt32)

            RemoveDevice(DeviceNumber, Nothing)

        End Sub

        ''' <summary>
        ''' Removes an existing FscryptDisk virtual disk from system.
        ''' </summary>
        ''' <param name="DeviceNumber">Device number to remove.</param>
        ''' <param name="StatusControl">Optional handle to control that can display status messages during operation.</param>
        Public Shared Sub RemoveDevice(DeviceNumber As UInt32, StatusControl As IntPtr)

            NativeFileIO.Win32Try(UnsafeNativeMethods.FscryptDiskRemoveDevice(StatusControl, DeviceNumber, Nothing))

        End Sub

        ''' <summary>
        ''' Removes an existing FscryptDisk virtual disk from system.
        ''' </summary>
        ''' <param name="MountPoint">Mount point of virtual disk to remove.</param>
        Public Shared Sub RemoveDevice(MountPoint As String)

            RemoveDevice(MountPoint, Nothing)

        End Sub

        ''' <summary>
        ''' Removes an existing FscryptDisk virtual disk from system.
        ''' </summary>
        ''' <param name="MountPoint">Mount point of virtual disk to remove.</param>
        ''' <param name="StatusControl">Optional handle to control that can display status messages during operation.</param>
        Public Shared Sub RemoveDevice(MountPoint As String, StatusControl As IntPtr)

            If String.IsNullOrEmpty(MountPoint) Then
                Throw New ArgumentNullException(NameOf(MountPoint))
            End If

            NativeFileIO.Win32Try(UnsafeNativeMethods.FscryptDiskRemoveDevice(StatusControl, 0, MountPoint))

        End Sub

        ''' <summary>
        ''' Forcefully removes an existing FscryptDisk virtual disk from system even if it is use by other applications.
        ''' </summary>
        ''' <param name="DeviceNumber">Device number to remove.</param>
        Public Shared Sub ForceRemoveDevice(DeviceNumber As UInt32)

            NativeFileIO.Win32Try(UnsafeNativeMethods.FscryptDiskForceRemoveDevice(IntPtr.Zero, DeviceNumber))

        End Sub

        ''' <summary>
        ''' Retrieves properties for an existing FscryptDisk virtual disk.
        ''' </summary>
        ''' <param name="DeviceNumber">Device number of FscryptDisk virtual disk to retrieve properties for.</param>
        ''' <param name="DiskSize">Size of virtual disk.</param>
        ''' <param name="TracksPerCylinder">Number of tracks per cylinder for virtual disk geometry.</param>
        ''' <param name="SectorsPerTrack">Number of sectors per track for virtual disk geometry.</param>
        ''' <param name="BytesPerSector">Number of bytes per sector for virtual disk geometry.</param>
        ''' <param name="ImageOffset">A skip offset if virtual disk data does not begin immediately at start of disk image file.
        ''' Frequently used with image formats like Nero NRG which start with a file header not used by FscryptDisk or Windows
        ''' filesystem drivers.</param>
        ''' <param name="Flags">Flags specifying properties for virtual disk. See comments for each flag value.</param>
        ''' <param name="DriveLetter">Drive letter if specified when virtual disk was created. If virtual disk was created
        ''' without a drive letter this parameter will be set to an empty Char value.</param>
        ''' <param name="Filename">Name of disk image file holding storage for file type virtual disk or used to create a
        ''' virtual memory type virtual disk.</param>
        Public Shared Sub QueryDevice(DeviceNumber As UInt32,
                                      ByRef DiskSize As Int64,
                                      ByRef TracksPerCylinder As UInt32,
                                      ByRef SectorsPerTrack As UInt32,
                                      ByRef BytesPerSector As UInt32,
                                      ByRef ImageOffset As Int64,
                                      ByRef Flags As FscryptDiskFlags,
                                      ByRef DriveLetter As Char,
                                      ByRef Filename As String)

            Dim CreateDataBuffer As Byte() = Nothing
            Array.Resize(CreateDataBuffer, 1096)

            NativeFileIO.Win32Try(UnsafeNativeMethods.FscryptDiskQueryDevice(DeviceNumber, CreateDataBuffer, CreateDataBuffer.Length))

            Using CreateDataReader As New BinaryReader(New MemoryStream(CreateDataBuffer), Encoding.Unicode)
                CreateDataReader.ReadUInt32()
                Dim Dummy = CreateDataReader.ReadUInt32()
                DiskSize = CreateDataReader.ReadInt64()
                Dim MediaType = CreateDataReader.ReadInt32()
                TracksPerCylinder = CreateDataReader.ReadUInt32()
                SectorsPerTrack = CreateDataReader.ReadUInt32()
                BytesPerSector = CreateDataReader.ReadUInt32()
                ImageOffset = CreateDataReader.ReadInt64()
                Flags = CType(CreateDataReader.ReadUInt32(), FscryptDiskFlags)
                DriveLetter = CreateDataReader.ReadChar()
                Dim FilenameLength = CreateDataReader.ReadUInt16()
                If FilenameLength = 0 Then
                    Filename = Nothing
                Else
                    Filename = Encoding.Unicode.GetString(CreateDataReader.ReadBytes(FilenameLength))
                End If
            End Using

        End Sub

        ''' <summary>
        ''' Retrieves properties for an existing FscryptDisk virtual disk.
        ''' </summary>
        ''' <param name="DeviceNumber">Device number of FscryptDisk virtual disk to retrieve properties for.</param>
        Public Shared Function QueryDevice(DeviceNumber As UInt32) As UnsafeNativeMethods.FscryptDiskCreateData

            Dim CreateDataBuffer As New UnsafeNativeMethods.FscryptDiskCreateData
            NativeFileIO.Win32Try(UnsafeNativeMethods.FscryptDiskQueryDevice(DeviceNumber, CreateDataBuffer, Marshal.SizeOf(CreateDataBuffer.GetType())))
            Return CreateDataBuffer

        End Function

        ''' <summary>
        ''' Modifies properties for an existing FscryptDisk virtual disk.
        ''' </summary>
        ''' <param name="DeviceNumber">Device number of FscryptDisk virtual disk to modify properties for.</param>
        ''' <param name="FlagsToChange">Flags for which to change values for.</param>
        ''' <param name="Flags">New flag values.</param>
        Public Shared Sub ChangeFlags(DeviceNumber As UInt32,
                                      FlagsToChange As FscryptDiskFlags,
                                      Flags As FscryptDiskFlags)

            ChangeFlags(DeviceNumber, FlagsToChange, Flags, Nothing)

        End Sub

        ''' <summary>
        ''' Modifies properties for an existing FscryptDisk virtual disk.
        ''' </summary>
        ''' <param name="DeviceNumber">Device number of FscryptDisk virtual disk to modify properties for.</param>
        ''' <param name="FlagsToChange">Flags for which to change values for.</param>
        ''' <param name="Flags">New flag values.</param>
        ''' <param name="StatusControl">Optional handle to control that can display status messages during operation.</param>
        Public Shared Sub ChangeFlags(DeviceNumber As UInt32,
                                      FlagsToChange As FscryptDiskFlags,
                                      Flags As FscryptDiskFlags,
                                      StatusControl As IntPtr)

            NativeFileIO.Win32Try(UnsafeNativeMethods.FscryptDiskChangeFlags(StatusControl,
                                                        DeviceNumber,
                                                        Nothing,
                                                        FlagsToChange,
                                                        Flags))

        End Sub

        ''' <summary>
        ''' Modifies properties for an existing FscryptDisk virtual disk.
        ''' </summary>
        ''' <param name="MountPoint">Mount point of FscryptDisk virtual disk to modify properties for.</param>
        ''' <param name="FlagsToChange">Flags for which to change values for.</param>
        ''' <param name="Flags">New flag values.</param>
        Public Shared Sub ChangeFlags(MountPoint As String,
                                      FlagsToChange As FscryptDiskFlags,
                                      Flags As FscryptDiskFlags)

            ChangeFlags(MountPoint, FlagsToChange, Flags, Nothing)

        End Sub

        ''' <summary>
        ''' Modifies properties for an existing FscryptDisk virtual disk.
        ''' </summary>
        ''' <param name="MountPoint">Mount point of FscryptDisk virtual disk to modify properties for.</param>
        ''' <param name="FlagsToChange">Flags for which to change values for.</param>
        ''' <param name="Flags">New flag values.</param>
        ''' <param name="StatusControl">Optional handle to control that can display status messages during operation.</param>
        Public Shared Sub ChangeFlags(MountPoint As String,
                                      FlagsToChange As FscryptDiskFlags,
                                      Flags As FscryptDiskFlags,
                                      StatusControl As IntPtr)

            NativeFileIO.Win32Try(UnsafeNativeMethods.FscryptDiskChangeFlags(StatusControl,
                                                        0,
                                                        MountPoint,
                                                        FlagsToChange,
                                                        Flags))

        End Sub

        ''' <summary>
        ''' Checks if Flags specifies a read only virtual disk.
        ''' </summary>
        ''' <param name="Flags">Flag field to check.</param>
        Public Shared Function IsReadOnly(Flags As FscryptDiskFlags) As Boolean

            Return (Flags And FscryptDiskFlags.ReadOnly) = FscryptDiskFlags.ReadOnly

        End Function

        ''' <summary>
        ''' Checks if Flags specifies a removable virtual disk.
        ''' </summary>
        ''' <param name="Flags">Flag field to check.</param>
        Public Shared Function IsRemovable(Flags As FscryptDiskFlags) As Boolean

            Return (Flags And FscryptDiskFlags.Removable) = FscryptDiskFlags.Removable

        End Function

        ''' <summary>
        ''' Checks if Flags specifies a modified virtual disk.
        ''' </summary>
        ''' <param name="Flags">Flag field to check.</param>
        Public Shared Function IsModified(Flags As FscryptDiskFlags) As Boolean

            Return (Flags And FscryptDiskFlags.Modified) = FscryptDiskFlags.Modified

        End Function

        ''' <summary>
        ''' Check if flags indicate shared write mode.
        ''' </summary>
        ''' <param name="Flags">Flag field to check.</param>
        Public Function IsSharedImage(Flags As FscryptDiskFlags) As Boolean

            Return (Flags And FscryptDiskFlags.SharedImage) = FscryptDiskFlags.SharedImage

        End Function

        ''' <summary>
        ''' Gets device type bits from a Flag field.
        ''' </summary>
        ''' <param name="Flags">Flag field to check.</param>
        Public Shared Function GetDeviceType(Flags As FscryptDiskFlags) As FscryptDiskFlags

            Return CType(Flags And &HF0UI, FscryptDiskFlags)

        End Function

        ''' <summary>
        ''' Gets disk type bits from a Flag field.
        ''' </summary>
        ''' <param name="Flags">Flag field to check.</param>
        Public Shared Function GetDiskType(Flags As FscryptDiskFlags) As FscryptDiskFlags

            Return CType(Flags And &HF00UI, FscryptDiskFlags)

        End Function

        ''' <summary>
        ''' Gets proxy type bits from a Flag field.
        ''' </summary>
        ''' <param name="Flags">Flag field to check.</param>
        Public Shared Function GetProxyType(Flags As FscryptDiskFlags) As FscryptDiskFlags

            Return CType(Flags And &HF000UI, FscryptDiskFlags)

        End Function

        ''' <summary>
        ''' Gets file type bits from a Flag field.
        ''' </summary>
        ''' <param name="Flags">Flag field to check.</param>
        Public Shared Function GetFileType(Flags As FscryptDiskFlags) As FscryptDiskFlags

            Return CType(Flags And &HF000UI, FscryptDiskFlags)

        End Function

        ''' <summary>
        ''' Determines whether flags specify either a virtual memory drive, or an
        ''' AWE_FS_Alloc (physical memory) drive.
        ''' </summary>
        ''' <param name="Flags">Flag field to check</param>
        Public Shared Function IsMemoryDrive(Flags As FscryptDiskFlags) As Boolean

            Return _
                    GetDiskType(Flags) = FscryptDiskFlags.TypeVM OrElse
                    (GetDiskType(Flags) = FscryptDiskFlags.TypeFile AndAlso
                     GetFileType(Flags) = FscryptDiskFlags.FileTypeAwe)

        End Function

        ''' <summary>
        '''    This function builds a Master Boot Record, MBR, in memory. The MBR will
        '''    contain a default Initial Program Loader, IPL, which could be used to boot
        '''    an operating system partition when the MBR is written to a disk.
        ''' </summary>
        ''' <param name="DiskGeometry">Pointer to a DISK_GEOMETRY structure that contains
        ''' information about logical geometry of the disk.
        ''' 
        ''' This function only uses the BytesPerSector, SectorsPerTrack and
        ''' TracksPerCylinder members.
        ''' 
        ''' This parameter can be Nothing/null if PartitionInfo parameter is Nothing/null
        ''' or references an empty array.</param>
        ''' <param name="PartitionInfo">Array of up to four PARTITION_INFORMATION structures
        ''' containing information about partitions to store in MBR partition table.
        ''' 
        ''' This function only uses the StartingOffset, PartitionLength, BootIndicator and
        ''' PartitionType members.
        ''' 
        ''' This parameter can be Nothing/null to create an empty MBR with just boot code
        ''' without any partition definitions.</param>
        ''' <param name="MBR">Pointer to memory buffer of at least 512 bytes where MBR will
        ''' be built.</param>
        Public Shared Sub BuildInMemoryMBR(DiskGeometry As NativeFileIO.UnsafeNativeMethods.DISK_GEOMETRY,
                                           PartitionInfo As NativeFileIO.UnsafeNativeMethods.PARTITION_INFORMATION(),
                                           MBR As Byte())

            If MBR Is Nothing Then
                Throw New ArgumentNullException(NameOf(MBR))
            End If

            NativeFileIO.Win32Try(UnsafeNativeMethods.FscryptDiskBuildMBR(DiskGeometry,
                                                     PartitionInfo,
                                                     CByte(If(PartitionInfo Is Nothing, 0, PartitionInfo.Length)),
                                                     MBR,
                                                     New IntPtr(MBR.Length)))

        End Sub

        ''' <summary>
        '''    This function builds a Master Boot Record, MBR, in memory. The MBR will
        '''    contain a default Initial Program Loader, IPL, which could be used to boot
        '''    an operating system partition when the MBR is written to a disk.
        ''' </summary>
        ''' <param name="DiskGeometry">Pointer to a DISK_GEOMETRY structure that contains
        ''' information about logical geometry of the disk.
        ''' 
        ''' This function only uses the BytesPerSector, SectorsPerTrack and
        ''' TracksPerCylinder members.
        ''' 
        ''' This parameter can be Nothing/null if PartitionInfo parameter is Nothing/null
        ''' or references an empty array.</param>
        ''' <param name="PartitionInfo">Array of up to four PARTITION_INFORMATION structures
        ''' containing information about partitions to store in MBR partition table.
        ''' 
        ''' This function only uses the StartingOffset, PartitionLength, BootIndicator and
        ''' PartitionType members.
        ''' 
        ''' This parameter can be Nothing/null to create an empty MBR with just boot code
        ''' without any partition definitions.</param>
        ''' <returns>Memory buffer containing built MBR.</returns>
        Public Shared Function BuildInMemoryMBR(DiskGeometry As NativeFileIO.UnsafeNativeMethods.DISK_GEOMETRY,
                                           PartitionInfo As NativeFileIO.UnsafeNativeMethods.PARTITION_INFORMATION()) As Byte()

            Dim MBR(0 To 511) As Byte

            NativeFileIO.Win32Try(UnsafeNativeMethods.FscryptDiskBuildMBR(DiskGeometry,
                                                     PartitionInfo,
                                                     CByte(If(PartitionInfo Is Nothing, 0, PartitionInfo.Length)),
                                                     MBR,
                                                     New IntPtr(MBR.Length)))

            Return MBR

        End Function

        ''' <summary>
        ''' This function converts a CHS disk address to LBA format.
        ''' </summary>
        ''' <param name="DiskGeometry">Pointer to a DISK_GEOMETRY structure that contains
        ''' information about logical geometry of the disk. This function only uses the
        ''' SectorsPerTrack and TracksPerCylinder members.</param>
        ''' <param name="CHS">Pointer to CHS disk address in three-byte partition table
        ''' style format.</param>
        ''' <returns>Calculated LBA disk address.</returns>
        Public Shared Function ConvertCHSToLBA(DiskGeometry As NativeFileIO.UnsafeNativeMethods.DISK_GEOMETRY,
                                               CHS As Byte()) As UInteger

            Return UnsafeNativeMethods.FscryptDiskConvertCHSToLBA(DiskGeometry, CHS)

        End Function

        ''' <summary>
        ''' This function converts an LBA disk address to three-byte partition style CHS
        ''' format.
        ''' </summary>
        ''' <param name="DiskGeometry">Pointer to a DISK_GEOMETRY structure that contains
        ''' information about logical geometry of the disk. This function only uses the
        ''' SectorsPerTrack and TracksPerCylinder members.</param>
        ''' <param name="LBA">LBA disk address.</param>
        ''' <returns>Calculated CHS values expressed in an array of three bytes.</returns>
        Public Shared Function ConvertCHSToLBA(DiskGeometry As NativeFileIO.UnsafeNativeMethods.DISK_GEOMETRY,
                                               LBA As UInteger) As Byte()

            Dim bytes = BitConverter.GetBytes(UnsafeNativeMethods.FscryptDiskConvertLBAToCHS(DiskGeometry, LBA))
            Array.Resize(bytes, 3)
            Return bytes

        End Function

        ''' <summary>
        ''' Adds registry settings for creating a virtual disk at system startup (or
        ''' when driver is loaded).</summary>
        ''' <param name="CreateData">FscryptDiskCreateData that contains device creation
        ''' settings to save. This structure is for example returned by QueryDevice.</param>
        Public Shared Sub SaveRegistrySettings(CreateData As UnsafeNativeMethods.FscryptDiskCreateData)

            NativeFileIO.Win32Try(UnsafeNativeMethods.FscryptDiskSaveRegistrySettings(CreateData))

        End Sub

        ''' <summary>
        '''   Remove registry settings for creating a virtual disk at system startup (or
        '''   when driver is loaded).
        '''</summary>
        ''' <param name="DeviceNumber">Device number specified in registry settings.</param>
        Public Shared Sub RemoveRegistrySettings(DeviceNumber As UInt32)

            NativeFileIO.Win32Try(UnsafeNativeMethods.FscryptDiskRemoveRegistrySettings(DeviceNumber))

        End Sub

        ''' <summary>
        '''   Retrieves number of auto-loading devices at system startup, or when driver
        '''   is loaded. This is the value of the LoadDevices registry value for
        '''   fscryptdisk.sys driver.
        ''' </summary>
        Public Shared Function GetRegistryAutoLoadDevices() As UInt32

            Dim LoadDevicesValue As UInt32
            NativeFileIO.Win32Try(UnsafeNativeMethods.FscryptDiskGetRegistryAutoLoadDevices(LoadDevicesValue))
            Return LoadDevicesValue

        End Function

        ''' <summary>
        '''   	Notify Explorer and other shell components that a new drive letter has
        ''' 	been created. Called automatically by device creation after creating a
        ''' 	drive letter. If no drive letter was created by a device creation routine
        ''' 	or if API flags was set to turn off shell notification during device
        ''' 	creation, this function can be called manually later.
        ''' 
        '''	    Note that calling this function has no effect if API flags are set to
        ''' 	turn off shell notifications, or if supplied drive letter path does not
        ''' 	specify an A-Z drive letter.
        ''' 
        ''' 	This function returns TRUE if successful, FALSE otherwise. If FALSE is
        ''' 	returned, GetLastError could be used to get actual error code.
        ''' </summary>
        ''' <param name="WindowHandle">
        ''' 	Window handle to use as parent handle for any message boxes. If this
        ''' 	parameter is NULL, no message boxes are displayed.
        ''' </param>
        ''' <param name="DriveLetterPath">
        ''' 	Drive letter path in one of formats A:\ or A:.
        ''' </param>
        Public Shared Sub NotifyShellDriveLetter(WindowHandle As IntPtr, DriveLetterPath As String)

            NativeFileIO.Win32Try(UnsafeNativeMethods.FscryptDiskNotifyShellDriveLetter(WindowHandle, DriveLetterPath))

        End Sub

        ''' <summary>
        '''   	Notify Explorer and other shell components that a drive is about to be
        ''' 	removed.
        ''' </summary>
        ''' <param name="WindowHandle">
        ''' 	Window handle to use as parent handle for any message boxes. If this
        ''' 	parameter is NULL, no message boxes are displayed.
        ''' </param>
        ''' <param name="DriveLetter">
        ''' 	Drive letter.
        ''' </param>
        Public Shared Sub NotifyRemovePending(WindowHandle As IntPtr, DriveLetter As Char)

            NativeFileIO.Win32Try(UnsafeNativeMethods.FscryptDiskNotifyRemovePending(WindowHandle, DriveLetter))

        End Sub

    End Class

End Namespace
