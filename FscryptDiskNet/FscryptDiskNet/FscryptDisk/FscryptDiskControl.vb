Imports System.Runtime.InteropServices

Namespace FscryptDisk

    ''' <summary>
    ''' Represents FscryptDisk Virtual Disk Driver control device object.
    ''' </summary>
    <ComVisible(False)>
    Public Class FscryptDiskControl
        Inherits FscryptDiskObject

        ''' <summary>
        ''' Creates a new instance and opens FscryptDisk Virtual Disk Driver control device object.
        ''' </summary>
        Public Sub New()
            MyBase.New("\\?\FscryptDisk_APP_Ctl", AccessMode:=0)

        End Sub

    End Class

End Namespace