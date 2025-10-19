Imports System.Threading
Imports System.Security.AccessControl

Namespace FscryptDisk

    Public Class FscryptDiskRefreshEvent
        Inherits WaitHandle

        Public Sub New(InheritHandle As Boolean)
            SafeWaitHandle = UnsafeNativeMethods.FscryptDiskOpenRefreshEvent(InheritHandle)
        End Sub

        ''' <summary>
        ''' Notifies other applications that FscryptDisk drive list has changed. This
        ''' simulates the same action done by the driver after such changes.
        ''' </summary>
        Public Sub Notify()
            NativeFileIO.Win32Try(NativeFileIO.UnsafeNativeMethods.PulseEvent(SafeWaitHandle))
        End Sub

    End Class

End Namespace
