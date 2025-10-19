﻿Imports System.IO
Imports System.Runtime.InteropServices
Imports Microsoft.Win32.SafeHandles

Namespace FscryptDisk

    ''' <summary>
    ''' Base class that represents FscryptDisk Virtual Disk Driver created device objects.
    ''' </summary>
    <Guid("797bbe31-0d5a-4225-8139-385539c5ce7e")>
    <ClassInterface(ClassInterfaceType.AutoDual)>
    Public Class FscryptDiskObject
        Implements IDisposable

        Public ReadOnly Property SafeFileHandle As SafeFileHandle
        Public ReadOnly Property AccessMode As FileAccess

        ''' <summary>
        ''' Opens specified Path with CreateFile Win32 API and encapsulates the returned handle
        ''' in a new FscryptDiskObject.
        ''' </summary>
        ''' <param name="Path">Path to pass to CreateFile API</param>
        ''' <param name="AccessMode">Access mode for opening and for underlying FileStream</param>
        Protected Sub New(Path As String, AccessMode As FileAccess)
            Me.New(NativeFileIO.OpenFileHandle(Path, AccessMode, FileShare.Read, FileMode.Open, Overlapped:=False), AccessMode)
        End Sub

        ''' <summary>
        ''' Encapsulates a handle in a new FscryptDiskObject.
        ''' </summary>
        ''' <param name="Handle">Existing handle to use</param>
        ''' <param name="Access">Access mode for underlying FileStream</param>
        Protected Sub New(Handle As SafeFileHandle, Access As FileAccess)
            _SafeFileHandle = Handle
            _AccessMode = Access
        End Sub

        ''' <summary>
        ''' Checks if version of running FscryptDisk Virtual Disk Driver servicing this device object is compatible with this API
        ''' library. If this device object is not created by FscryptDisk Virtual Disk Driver this method returns False.
        ''' </summary>
        Public Function CheckDriverVersion() As Boolean

            Return UnsafeNativeMethods.FscryptDiskCheckDriverVersion(_SafeFileHandle)

        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    _SafeFileHandle?.Dispose()
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.

                ' TODO: set large fields to null.
                _SafeFileHandle = Nothing
            End If
            Me.disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
        Protected Overrides Sub Finalize()
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(False)
            MyBase.Finalize()
        End Sub

        ''' <summary>
        ''' Close device object.
        ''' </summary>
        Public Sub Close() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

    End Class

End Namespace