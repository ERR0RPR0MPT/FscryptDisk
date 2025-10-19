﻿Imports System.Collections.ObjectModel
Imports System.IO
Imports LTR.IO.FscryptDisk.Devio.Server.Providers

Namespace Server.Services

    ''' <summary>
    ''' Class deriving from DevioServiceBase, but without providing a proxy service. Instead,
    ''' it just passes a disk image file name for direct mounting internally in FscryptDisk Virtual
    ''' Disk Driver.
    ''' </summary>
    Public Class DevioNoneService
        Inherits DevioServiceBase

        ''' <summary>
        ''' Name and path of image file mounted by FscryptDisk Virtual Disk Driver.
        ''' </summary>
        Public ReadOnly Property Imagefile As String

        Private ReadOnly _Access As FileAccess

        ''' <summary>
        ''' Creates a DevioServiceBase compatible object, but without providing a proxy service.
        ''' Instead, it just passes a disk image file name for direct mounting internally in FscryptDisk
        ''' Virtual Disk Driver.
        ''' </summary>
        ''' <param name="Imagefile">Name and path of image file mounted by FscryptDisk Virtual Disk Driver.</param>
        ''' <param name="Access">Specifies access to image file.</param>
        Public Sub New(Imagefile As String, Access As FileAccess)
            MyBase.New(New DevioProviderFromStream(New FileStream(Imagefile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite Or FileShare.Delete), ownsStream:=True), OwnsProvider:=True)

            _Access = Access
            Offset = FscryptDiskAPI.GetOffsetByFileExt(Imagefile)
            Me._Imagefile = Imagefile

        End Sub

        ''' <summary>
        ''' Reads partition table and parses partition entry values into a collection of PARTITION_INFORMATION
        ''' structure objects.
        ''' </summary>
        ''' <returns>Collection of PARTITION_INFORMATION structures objects.</returns>
        Public Overrides Function GetPartitionInformation() As ReadOnlyCollection(Of NativeFileIO.UnsafeNativeMethods.PARTITION_INFORMATION)
            Return FscryptDiskAPI.GetPartitionInformation(_Imagefile, SectorSize, Offset)
        End Function

        Protected Overrides ReadOnly Property FscryptDiskProxyObjectName As String
            Get
                Return _Imagefile
            End Get
        End Property

        Protected Overrides ReadOnly Property FscryptDiskProxyModeFlags As FscryptDiskFlags
            Get
                If (_Access And FileAccess.Write) = 0 Then
                    Return FscryptDiskFlags.TypeFile Or FscryptDiskFlags.ReadOnly
                Else
                    Return FscryptDiskFlags.TypeFile
                End If
            End Get
        End Property

        ''' <summary>
        ''' Dummy implementation that always returns True.
        ''' </summary>
        ''' <returns>Fixed value of True.</returns>
        Public Overrides Function StartServiceThread() As Boolean
            Return True
        End Function

        ''' <summary>
        ''' Dummy implementation that just raises ServiceReady event.
        ''' </summary>
        Public Overrides Sub RunService()
            OnServiceReady()
        End Sub

    End Class

End Namespace
