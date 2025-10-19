﻿Imports System.ComponentModel
Imports System.IO
Imports System.Runtime.InteropServices
Imports Microsoft.Win32.SafeHandles

Namespace FscryptDisk

    ''' <summary>
    ''' Represents FscryptDisk Virtual Disk Driver disk device objects.
    ''' </summary>
    <Guid("e1edf6ff-7b7a-49d7-9943-c30812dcf9b1")>
    <ClassInterface(ClassInterfaceType.AutoDual)>
    Public Class FscryptDiskDevice
        Inherits FscryptDiskObject

        Private _RawDiskStream As FscryptDiskDeviceStream

        Private Shared Function OpenDeviceHandle(DeviceNumber As UInt32, AccessMode As FileAccess) As SafeFileHandle

            Dim NativeAccessMode As UInt32 = NativeFileIO.UnsafeNativeMethods.FILE_READ_ATTRIBUTES
            If (AccessMode And FileAccess.Read) = FileAccess.Read Then
                NativeAccessMode += NativeFileIO.UnsafeNativeMethods.GENERIC_READ
            End If
            If (AccessMode And FileAccess.Write) = FileAccess.Write Then
                NativeAccessMode += NativeFileIO.UnsafeNativeMethods.GENERIC_WRITE
            End If

            Dim Handle = UnsafeNativeMethods.FscryptDiskOpenDeviceByNumber(DeviceNumber, NativeAccessMode)
            If Handle.IsInvalid Then
                Throw New Win32Exception
            End If

            If Handle.IsInvalid Then
                Throw New Win32Exception
            End If

            NativeFileIO.UnsafeNativeMethods.DeviceIoControl(Handle, NativeFileIO.UnsafeNativeMethods.FSCTL_ALLOW_EXTENDED_DASD_IO, IntPtr.Zero, 0UI, IntPtr.Zero, 0UI, 0UI, IntPtr.Zero)
            Return Handle

        End Function

        Private Shared Function OpenDeviceHandle(MountPoint As String, AccessMode As FileAccess) As SafeFileHandle

            Dim NativeAccessMode As UInt32 = NativeFileIO.UnsafeNativeMethods.FILE_READ_ATTRIBUTES
            If (AccessMode And FileAccess.Read) = FileAccess.Read Then
                NativeAccessMode += NativeFileIO.UnsafeNativeMethods.GENERIC_READ
            End If
            If (AccessMode And FileAccess.Write) = FileAccess.Write Then
                NativeAccessMode += NativeFileIO.UnsafeNativeMethods.GENERIC_WRITE
            End If

            Dim Handle = UnsafeNativeMethods.FscryptDiskOpenDeviceByMountPoint(MountPoint, NativeAccessMode)
            If Handle.IsInvalid Then
                Throw New Win32Exception
            End If

            NativeFileIO.UnsafeNativeMethods.DeviceIoControl(Handle, NativeFileIO.UnsafeNativeMethods.FSCTL_ALLOW_EXTENDED_DASD_IO, IntPtr.Zero, 0UI, IntPtr.Zero, 0UI, 0UI, IntPtr.Zero)
            Return Handle

        End Function

        ''' <summary>
        ''' Creates a new instance and opens an existing FscryptDisk virtual disk device.
        ''' </summary>
        ''' <param name="DeviceNumber">Device number of FscryptDisk virtual disk to open.</param>
        ''' <param name="AccessMode">Access mode to request for accessing disk object.</param>
        Public Sub New(DeviceNumber As UInt32, AccessMode As FileAccess)
            MyBase.New(OpenDeviceHandle(DeviceNumber, AccessMode), AccessMode)

        End Sub

        ''' <summary>
        ''' Creates a new instance and opens an existing disk device.
        ''' </summary>
        ''' <param name="MountPoint">Mount point of disk device to open.</param>
        ''' <param name="AccessMode">Access mode to request for accessing disk object.</param>
        Public Sub New(MountPoint As String, AccessMode As FileAccess)
            MyBase.New(OpenDeviceHandle(MountPoint, AccessMode), AccessMode)

        End Sub

        ''' <summary>
        ''' Retrieves volume size of disk device.
        ''' </summary>
        Public ReadOnly Property DiskSize As Long
            Get
                Dim Size As Int64
                NativeFileIO.Win32Try(UnsafeNativeMethods.FscryptDiskGetVolumeSize(SafeFileHandle, Size))
                Return Size
            End Get
        End Property

        ''' <summary>
        ''' Locks and dismounts filesystem on a volume. Upon successful return, further access to the device
        ''' can only be done through this device object instance until it is either closed (disposed) or lock is
        ''' released on the underlying handle.
        ''' </summary>
        ''' <param name="Force">Indicates if True that volume should be immediately dismounted even if it
        ''' cannot be locked. This causes all open handles to files on the volume to become invalid. If False,
        ''' successful lock (no other open handles) is required before attempting to dismount filesystem.</param>
        Public Sub DismountVolumeFilesystem(Force As Boolean)

            NativeFileIO.Win32Try(NativeFileIO.DismountVolumeFilesystem(SafeFileHandle, Force))

        End Sub

        ''' <summary>
        ''' Opens a FscryptDiskDeviceStream object around this FscryptDisk device that can be used to directly access disk data.
        ''' </summary>
        Public Function GetRawDiskStream() As FscryptDiskDeviceStream
            If _RawDiskStream Is Nothing Then
                _RawDiskStream = New FscryptDiskDeviceStream(SafeFileHandle, AccessMode)
            End If
            Return _RawDiskStream
        End Function

        ''' <summary>
        ''' Saves contents of disk device to an image file.
        ''' </summary>
        ''' <param name="ImageFile">FileStream object opened for writing where disk contents will be written.</param>
        ''' <param name="BufferSize">Buffer size to use when transferring data from disk device to file.</param>
        <ComVisible(False)>
        Public Sub SaveImageFile(ImageFile As FileStream, BufferSize As UInt32)

            If ImageFile Is Nothing Then
                Throw New ArgumentNullException(NameOf(ImageFile))
            End If

            NativeFileIO.Win32Try(UnsafeNativeMethods.FscryptDiskSaveImageFile(SafeFileHandle,
                                                          ImageFile.SafeFileHandle,
                                                          BufferSize,
                                                          IntPtr.Zero))

        End Sub

        ''' <summary>
        ''' Saves contents of disk device to an image file while pumping window messages between reads and writes.
        ''' </summary>
        ''' <param name="ImageFile">FileStream object opened for writing where disk contents will be written.</param>
        ''' <param name="BufferSize">Buffer size to use when transferring data from disk device to file.</param>
        ''' <param name="CancelAction">A boolean flag that will be checked between buffer reads/writes. If flag is set to True
        ''' operation will be cancelled and an exception thrown.</param>
        <ComVisible(False)>
        Public Sub SaveImageFile(ImageFile As FileStream, BufferSize As UInt32, CancelAction As Action(Of Action(Of Boolean)))

            If ImageFile Is Nothing Then
                Throw New ArgumentNullException(NameOf(ImageFile))
            End If

            Dim CancelFlag As Integer
            Dim CancelFlagHandle = GCHandle.Alloc(CancelFlag, GCHandleType.Pinned)
            Try
                If CancelAction IsNot Nothing Then
                    CancelAction(Sub(flag) CancelFlag = If(flag, 1, 0))
                End If
                NativeFileIO.Win32Try(UnsafeNativeMethods.FscryptDiskSaveImageFile(SafeFileHandle,
                                                              ImageFile.SafeFileHandle,
                                                              BufferSize,
                                                              CancelFlagHandle.AddrOfPinnedObject()))
            Finally
                CancelFlagHandle.Free()

            End Try

        End Sub

        ''' <summary>
        ''' Saves contents of disk device to an image file.
        ''' </summary>
        ''' <param name="ImageFile">Native file handle opened for writing where disk contents will be written.</param>
        ''' <param name="BufferSize">Buffer size to use when transferring data from disk device to file.</param>
        <ComVisible(False)>
        Public Sub SaveImageFile(ImageFile As SafeFileHandle, BufferSize As UInt32)

            NativeFileIO.Win32Try(UnsafeNativeMethods.FscryptDiskSaveImageFile(SafeFileHandle,
                                                          ImageFile,
                                                          BufferSize,
                                                          IntPtr.Zero))

        End Sub

        ''' <summary>
        ''' Saves contents of disk device to an image file while pumping window messages between reads and writes.
        ''' </summary>
        ''' <param name="ImageFile">Native file handle opened for writing where disk contents will be written.</param>
        ''' <param name="BufferSize">Buffer size to use when transferring data from disk device to file.</param>
        ''' <param name="CancelAction">A boolean flag that will be checked between buffer reads/writes. If flag is set to True
        ''' operation will be cancelled and an exception thrown.</param>
        <ComVisible(False)>
        Public Sub SaveImageFile(ImageFile As SafeFileHandle, BufferSize As UInt32, CancelAction As Action(Of Action(Of Boolean)))

            If ImageFile Is Nothing Then
                Throw New ArgumentNullException(NameOf(ImageFile))
            End If

            Dim CancelFlag As Integer
            Dim CancelFlagHandle = GCHandle.Alloc(CancelFlag, GCHandleType.Pinned)

            Try
                If CancelAction IsNot Nothing Then
                    CancelAction(Sub(flag) CancelFlag = If(flag, 1, 0))
                End If

                NativeFileIO.Win32Try(UnsafeNativeMethods.FscryptDiskSaveImageFile(SafeFileHandle,
                                                              ImageFile,
                                                              BufferSize,
                                                              CancelFlagHandle.AddrOfPinnedObject()))

            Finally
                CancelFlagHandle.Free()

            End Try

        End Sub

        ''' <summary>
        ''' Saves contents of disk device to an image file.
        ''' </summary>
        ''' <param name="ImageFile">Name of file to which disk contents will be written.</param>
        ''' <param name="BufferSize">Buffer size to use when transferring data from disk device to file.</param>
        <ComVisible(False)>
        Public Sub SaveImageFile(ImageFile As String, BufferSize As UInt32)

            If ImageFile Is Nothing Then
                Throw New ArgumentNullException(NameOf(ImageFile))
            End If

            Using ImageFileHandle = NativeFileIO.OpenFileHandle(ImageFile, FileAccess.Write, FileShare.None, FileMode.Create, Overlapped:=False)

                NativeFileIO.Win32Try(UnsafeNativeMethods.FscryptDiskSaveImageFile(SafeFileHandle,
                                                              ImageFileHandle,
                                                              BufferSize,
                                                              IntPtr.Zero))

            End Using

        End Sub

        ''' <summary>
        ''' Saves contents of disk device to an image file while pumping window messages between reads and writes.
        ''' </summary>
        ''' <param name="ImageFile">Name of file to which disk contents will be written.</param>
        ''' <param name="BufferSize">Buffer size to use when transferring data from disk device to file.</param>
        ''' <param name="CancelAction">A boolean flag that will be checked between buffer reads/writes. If flag is set to True
        ''' operation will be cancelled and an exception thrown.</param>
        <ComVisible(False)>
        Public Sub SaveImageFile(ImageFile As String, BufferSize As UInt32, CancelAction As Action(Of Action(Of Boolean)))

            If ImageFile Is Nothing Then
                Throw New ArgumentNullException(NameOf(ImageFile))
            End If

            Using ImageFileHandle = NativeFileIO.OpenFileHandle(ImageFile, FileAccess.Write, FileShare.None, FileMode.Create, Overlapped:=False)

                Dim CancelFlag As Integer
                Dim CancelFlagHandle = GCHandle.Alloc(CancelFlag, GCHandleType.Pinned)
                Try
                    If CancelAction IsNot Nothing Then
                        CancelAction(Sub(flag) CancelFlag = If(flag, 1, 0))
                    End If
                    NativeFileIO.Win32Try(UnsafeNativeMethods.FscryptDiskSaveImageFile(SafeFileHandle,
                                                                  ImageFileHandle,
                                                                  BufferSize,
                                                                  CancelFlagHandle.AddrOfPinnedObject()))
                Finally
                    CancelFlagHandle.Free()

                End Try
            End Using

        End Sub

        ''' <summary>
        ''' Saves contents of disk device to an image file.
        ''' </summary>
        ''' <param name="ImageFile">Name of file to which disk contents will be written.</param>
        Public Sub SaveImageFile(ImageFile As String)

            If ImageFile Is Nothing Then
                Throw New ArgumentNullException(NameOf(ImageFile))
            End If

            Using ImageFileHandle = NativeFileIO.OpenFileHandle(ImageFile, FileAccess.Write, FileShare.None, FileMode.Create, Overlapped:=False)

                NativeFileIO.Win32Try(UnsafeNativeMethods.FscryptDiskSaveImageFile(SafeFileHandle,
                                                                ImageFileHandle,
                                                                0,
                                                                IntPtr.Zero))

            End Using

        End Sub

        ''' <summary>
        ''' This function saves the contents of a device to an image file. This is a
        ''' user-interactive function that displays dialog boxes where user can select
        ''' image file and other options.
        ''' </summary>
        ''' <param name="hWnd">Handle to existing window that will be parent to dialog
        ''' boxes etc.</param>
        ''' <param name="BufferSize">I/O buffer size to use when reading source disk. This
        ''' parameter is optional, if it is zero the buffer size to use
        ''' will automatically chosen.</param>
        ''' <param name="IsCdRomType">If this parameter is TRUE and the source device type cannot
        ''' be automatically determined this function will ask user for
        ''' a .iso suffixed image file name.</param>
        Public Sub SaveImageFileInteractive(hWnd As IntPtr, BufferSize As UInt32, IsCdRomType As Boolean)

            UnsafeNativeMethods.FscryptDiskSaveImageFileInteractive(SafeFileHandle, hWnd, BufferSize, IsCdRomType)

        End Sub

        ''' <summary>
        ''' This function saves the contents of a device to an image file. This is a
        ''' user-interactive function that displays dialog boxes where user can select
        ''' image file and other options.
        ''' </summary>
        ''' <param name="hWnd">Handle to existing window that will be parent to dialog
        ''' boxes etc.</param>
        ''' <param name="IsCdRomType">If this parameter is TRUE and the source device type cannot
        ''' be automatically determined this function will ask user for
        ''' a .iso suffixed image file name.</param>
        <ComVisible(False)>
        Public Sub SaveImageFileInteractive(hWnd As IntPtr, IsCdRomType As Boolean)

            UnsafeNativeMethods.FscryptDiskSaveImageFileInteractive(SafeFileHandle, hWnd, 0, IsCdRomType)

        End Sub

        ''' <summary>
        ''' This function saves the contents of a device to an image file. This is a
        ''' user-interactive function that displays dialog boxes where user can select
        ''' image file and other options.
        ''' </summary>
        ''' <param name="hWnd">Handle to existing window that will be parent to dialog
        ''' boxes etc.</param>
        ''' <param name="BufferSize">I/O buffer size to use when reading source disk. This
        ''' parameter is optional, if it is zero the buffer size to use
        ''' will automatically chosen.</param>
        <ComVisible(False)>
        Public Sub SaveImageFileInteractive(hWnd As IntPtr, BufferSize As UInt32)

            UnsafeNativeMethods.FscryptDiskSaveImageFileInteractive(SafeFileHandle, hWnd, BufferSize, False)

        End Sub

        ''' <summary>
        ''' This function saves the contents of a device to an image file. This is a
        ''' user-interactive function that displays dialog boxes where user can select
        ''' image file and other options.
        ''' </summary>
        ''' <param name="hWnd">Handle to existing window that will be parent to dialog
        ''' boxes etc.</param>
        <ComVisible(False)>
        Public Sub SaveImageFileInteractive(hWnd As IntPtr)

            UnsafeNativeMethods.FscryptDiskSaveImageFileInteractive(SafeFileHandle, hWnd, 0, False)

        End Sub

        ''' <summary>
        ''' This function saves the contents of a device to an image file. This is a
        ''' user-interactive function that displays dialog boxes where user can select
        ''' image file and other options.
        ''' </summary>
        ''' <param name="IsCdRomType">If this parameter is TRUE and the source device type cannot
        ''' be automatically determined this function will ask user for
        ''' a .iso suffixed image file name.</param>
        <ComVisible(False)>
        Public Sub SaveImageFileInteractive(IsCdRomType As Boolean)

            UnsafeNativeMethods.FscryptDiskSaveImageFileInteractive(SafeFileHandle, IntPtr.Zero, 0, IsCdRomType)

        End Sub

        ''' <summary>
        ''' This function saves the contents of a device to an image file. This is a
        ''' user-interactive function that displays dialog boxes where user can select
        ''' image file and other options.
        ''' </summary>
        <ComVisible(False)>
        Public Sub SaveImageFileInteractive()

            UnsafeNativeMethods.FscryptDiskSaveImageFileInteractive(SafeFileHandle, IntPtr.Zero, 0, False)

        End Sub

        ''' <summary>
        ''' Forcefully removes FscryptDisk virtual disk from system even if it is use by other applications.
        ''' </summary>
        Public Overloads Sub ForceRemoveDevice()

            NativeFileIO.Win32Try(UnsafeNativeMethods.FscryptDiskForceRemoveDevice(SafeFileHandle, 0))

        End Sub

        Protected Overrides Sub Dispose(disposing As Boolean)
            If _RawDiskStream IsNot Nothing Then
                _RawDiskStream.Dispose()
                _RawDiskStream = Nothing
            End If

            MyBase.Dispose(disposing)
        End Sub

    End Class

End Namespace
