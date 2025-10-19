﻿Imports System.IO
Imports System.IO.MemoryMappedFiles
Imports System.Runtime.InteropServices
Imports System.Threading
Imports LTR.IO.FscryptDisk.Devio.FSCRYPTDPROXY_CONSTANTS
Imports LTR.IO.FscryptDisk.Devio.Server.Providers
Imports Microsoft.Win32.SafeHandles

Namespace Server.Services

    ''' <summary>
    ''' Class that implements server end of FscryptDisk/Devio shared memory based communication
    ''' protocol. It uses an object implementing <see>IDevioProvider</see> interface as
    ''' storage backend for I/O requests received from client.
    ''' </summary>
    Public Class DevioShmService
        Inherits DevioServiceBase

        ''' <summary>
        ''' Object name of shared memory file mapping object created by this instance.
        ''' </summary>
        Public ReadOnly Property ObjectName As String

        ''' <summary>
        ''' Buffer size used by this instance.
        ''' </summary>
        Public ReadOnly Property BufferSize As Long

        ''' <summary>
        ''' Buffer size that will be automatically selected on this platform when
        ''' an instance is created by a constructor without a BufferSize argument.
        ''' </summary>
        Public Shared ReadOnly Property DefaultBufferSize As Long
            Get
                If Environment.OSVersion.Version.Major > 5 Then
                    Return 2097152 + FSCRYPTDPROXY_HEADER_SIZE
                Else
                    Return 1048576 + FSCRYPTDPROXY_HEADER_SIZE
                End If
            End Get
        End Property

        Private Shared Function GetNextRandomValue() As Integer
            Dim value As Integer
            NativeFileIO.UnsafeNativeMethods.RtlGenRandom(value, 4)
            Return value
        End Function

        ''' <summary>
        ''' Creates a new service instance with enough data to later run a service that acts as server end in FscryptDisk/Devio
        ''' shared memory based communication.
        ''' </summary>
        ''' <param name="ObjectName">Object name of shared memory file mapping object created by this instance.</param>
        ''' <param name="DevioProvider">IDevioProvider object to that serves as storage backend for this service.</param>
        ''' <param name="OwnsProvider">Indicates whether DevioProvider object will be automatically closed when this
        ''' instance is disposed.</param>
        ''' <param name="BufferSize">Buffer size to use for shared memory I/O communication.</param>
        Public Sub New(ObjectName As String, DevioProvider As IDevioProvider, OwnsProvider As Boolean, BufferSize As Long)
            MyBase.New(DevioProvider, OwnsProvider)

            _ObjectName = ObjectName
            _BufferSize = BufferSize

        End Sub

        ''' <summary>
        ''' Creates a new service instance with enough data to later run a service that acts as server end in FscryptDisk/Devio
        ''' shared memory based communication. A default buffer size will be used.
        ''' </summary>
        ''' <param name="ObjectName">Object name of shared memory file mapping object created by this instance.</param>
        ''' <param name="DevioProvider">IDevioProvider object to that serves as storage backend for this service.</param>
        ''' <param name="OwnsProvider">Indicates whether DevioProvider object will be automatically closed when this
        ''' instance is disposed.</param>
        Public Sub New(ObjectName As String, DevioProvider As IDevioProvider, OwnsProvider As Boolean)
            MyClass.New(ObjectName, DevioProvider, OwnsProvider, DefaultBufferSize)
        End Sub

        ''' <summary>
        ''' Creates a new service instance with enough data to later run a service that acts as server end in FscryptDisk/Devio
        ''' shared memory based communication. A default buffer size and a random object name will be used.
        ''' </summary>
        ''' <param name="DevioProvider">IDevioProvider object to that serves as storage backend for this service.</param>
        ''' <param name="OwnsProvider">Indicates whether DevioProvider object will be automatically closed when this
        ''' instance is disposed.</param>
        Public Sub New(DevioProvider As IDevioProvider, OwnsProvider As Boolean)
            MyClass.New(DevioProvider, OwnsProvider, DefaultBufferSize)
        End Sub

        ''' <summary>
        ''' Creates a new service instance with enough data to later run a service that acts as server end in FscryptDisk/Devio
        ''' shared memory based communication. A random object name will be used.
        ''' </summary>
        ''' <param name="DevioProvider">IDevioProvider object to that serves as storage backend for this service.</param>
        ''' <param name="OwnsProvider">Indicates whether DevioProvider object will be automatically closed when this
        ''' instance is disposed.</param>
        ''' <param name="BufferSize">Buffer size to use for shared memory I/O communication.</param>
        Public Sub New(DevioProvider As IDevioProvider, OwnsProvider As Boolean, BufferSize As Long)
            MyClass.New($"devio-{GetNextRandomValue()}", DevioProvider, OwnsProvider, BufferSize)
        End Sub

        ''' <summary>
        ''' Runs service that acts as server end in FscryptDisk/Devio shared memory based communication. It will first wait for
        ''' a client to connect, then serve client I/O requests and when client finally requests service to terminate, this
        ''' method returns to caller. To run service in a worker thread that automatically disposes this object after client
        ''' disconnection, call StartServiceThread() instead.
        ''' </summary>
        Public Overrides Sub RunService()

            Using DisposableObjects As New DisposableList(Of IDisposable)

                Dim RequestEvent As WaitHandle

                Dim ResponseEvent As WaitHandle

                Dim Mapping As MemoryMappedFile

                Dim MapView As SafeMemoryMappedViewHandle

                Dim ServerMutex As Mutex

                Try
                    Trace.WriteLine($"Creating objects for shared memory communication '{_ObjectName}'.")

                    RequestEvent = New EventWaitHandle(initialState:=False, mode:=EventResetMode.AutoReset, name:=$"Global\{_ObjectName}_Request")
                    DisposableObjects.Add(RequestEvent)

                    ResponseEvent = New EventWaitHandle(initialState:=False, mode:=EventResetMode.AutoReset, name:=$"Global\{_ObjectName}_Response")
                    DisposableObjects.Add(ResponseEvent)

                    ServerMutex = New Mutex(initiallyOwned:=False, name:=$"Global\{_ObjectName}_Server")
                    DisposableObjects.Add(ServerMutex)

                    If ServerMutex.WaitOne(0) = False Then
                        Trace.WriteLine("Service busy.")
                        OnServiceInitFailed()
                        Return
                    End If

#If NETFRAMEWORK AndAlso Not NET46_OR_GREATER Then
                    Mapping = MemoryMappedFile.CreateNew($"Global\{_ObjectName}",
                                                         _BufferSize,
                                                         MemoryMappedFileAccess.ReadWrite,
                                                         MemoryMappedFileOptions.None,
                                                         Nothing,
                                                         HandleInheritability.None)
#Else
                    Mapping = MemoryMappedFile.CreateNew($"Global\{_ObjectName}",
                                                         _BufferSize,
                                                         MemoryMappedFileAccess.ReadWrite,
                                                         MemoryMappedFileOptions.None,
                                                         HandleInheritability.None)
#End If

                    DisposableObjects.Add(Mapping)

                    Dim MapAccessor = Mapping.CreateViewAccessor()
                    DisposableObjects.Add(MapAccessor)

                    MapView = MapAccessor.SafeMemoryMappedViewHandle
                    DisposableObjects.Add(MapView)

                    Trace.WriteLine($"Created shared memory object, {MapView.ByteLength} bytes.")

                    Trace.WriteLine("Raising service ready event.")
                    OnServiceReady()

                Catch ex As Exception
                    Trace.WriteLine("Service thread initialization exception: " & ex.ToString())
                    OnServiceInitFailed()
                    Return

                End Try

                Try
                    Trace.WriteLine("Waiting for client to connect.")

                    Using StopServiceThreadEvent As New EventWaitHandle(initialState:=False, mode:=EventResetMode.ManualReset)
                        Dim StopServiceThreadHandler As New Action(AddressOf StopServiceThreadEvent.Set)
                        AddHandler StopServiceThread, StopServiceThreadHandler
                        Dim WaitEvents = {RequestEvent, StopServiceThreadEvent}
                        Dim EventIndex = WaitHandle.WaitAny(WaitEvents)
                        RemoveHandler StopServiceThread, StopServiceThreadHandler

                        Trace.WriteLine("Wait finished. Disposing file mapping object.")

                        Mapping.Dispose()
                        Mapping = Nothing

                        If WaitEvents(EventIndex) Is StopServiceThreadEvent Then
                            Trace.WriteLine("Service thread exit request.")
                            Return
                        End If
                    End Using

                    Trace.WriteLine("Client connected, waiting for request.")

                    Using MapView

                        Do
                            Dim RequestCode = MapView.Read(Of FSCRYPTDPROXY_REQ)(&H0)

                            'Trace.WriteLine("Got client request: " & RequestCode.ToString())

                            Select Case RequestCode

                                Case FSCRYPTDPROXY_REQ.FSCRYPTDPROXY_REQ_INFO
                                    SendInfo(MapView)

                                Case FSCRYPTDPROXY_REQ.FSCRYPTDPROXY_REQ_READ
                                    ReadData(MapView)

                                Case FSCRYPTDPROXY_REQ.FSCRYPTDPROXY_REQ_WRITE
                                    WriteData(MapView)

                                Case FSCRYPTDPROXY_REQ.FSCRYPTDPROXY_REQ_CLOSE
                                    Trace.WriteLine("Closing connection.")
                                    Return

                                Case Else
                                    Trace.WriteLine($"Unsupported request code: {RequestCode}")
                                    Return

                            End Select

                            'Trace.WriteLine("Sending response and waiting for next request.")

                            If WaitHandle.SignalAndWait(ResponseEvent, RequestEvent) = False Then
                                Trace.WriteLine("Synchronization failed.")
                            End If

                        Loop

                    End Using

                    Trace.WriteLine("Client disconnected.")

                Catch ex As Exception
                    Trace.WriteLine($"Unhandled exception in service thread: {ex}")
                    OnServiceUnhandledException(New UnhandledExceptionEventArgs(ex, True))

                Finally
                    OnServiceShutdown()

                End Try

            End Using

        End Sub

        Private Sub SendInfo(MapView As SafeBuffer)

            Dim Info As FSCRYPTDPROXY_INFO_RESP
            Info.file_size = CULng(DevioProvider.Length)
            Info.req_alignment = CULng(REQUIRED_ALIGNMENT)
            Info.flags = If(DevioProvider.CanWrite, FSCRYPTDPROXY_FLAGS.FSCRYPTDPROXY_FLAG_NONE, FSCRYPTDPROXY_FLAGS.FSCRYPTDPROXY_FLAG_RO)

            MapView.Write(&H0, Info)

        End Sub

        Private Sub ReadData(MapView As SafeBuffer)

            Dim Request = MapView.Read(Of FSCRYPTDPROXY_READ_REQ)(&H0)

            Dim Offset = CLng(Request.offset)
            Dim ReadLength = CInt(Request.length)

            Static largest_request As Integer
            If ReadLength > largest_request Then
                largest_request = ReadLength
                'Trace.WriteLine("Largest requested read size is now: " & largest_request & " bytes")
            End If

            Dim Response As FSCRYPTDPROXY_READ_RESP

            Try
                If ReadLength > MapView.ByteLength - FSCRYPTDPROXY_HEADER_SIZE Then
                    Trace.WriteLine($"Requested read length {ReadLength}, lowered to {MapView.ByteLength - FSCRYPTDPROXY_HEADER_SIZE} bytes.")
                    ReadLength = CInt(MapView.ByteLength - FSCRYPTDPROXY_HEADER_SIZE)
                End If
                Response.length = CULng(DevioProvider.Read(MapView.DangerousGetHandle(), FSCRYPTDPROXY_HEADER_SIZE, ReadLength, Offset))
                Response.errorno = 0

            Catch ex As Exception
                Trace.WriteLine(ex.ToString())
                Trace.WriteLine($"Read request at 0x{Offset:X8} for {ReadLength} bytes.")
                Response.errorno = 1
                Response.length = 0

            End Try

            MapView.Write(&H0, Response)

        End Sub

        Private Sub WriteData(MapView As SafeBuffer)

            Dim Request = MapView.Read(Of FSCRYPTDPROXY_WRITE_REQ)(&H0)

            Dim Offset = CLng(Request.offset)
            Dim Length = CInt(Request.length)

            Static largest_request As Integer
            If Length > largest_request Then
                largest_request = Length
                'Trace.WriteLine("Largest requested write size is now: " & largest_request & " bytes")
            End If

            Dim Response As FSCRYPTDPROXY_WRITE_RESP

            Try
                If Length > MapView.ByteLength - FSCRYPTDPROXY_HEADER_SIZE Then
                    Throw New Exception($"Requested write length {Length}. Buffer size is {CInt(MapView.ByteLength - FSCRYPTDPROXY_HEADER_SIZE)} bytes.")
                End If
                Length = DevioProvider.Write(MapView.DangerousGetHandle(), FSCRYPTDPROXY_HEADER_SIZE, Length, Offset)
                Response.errorno = 0
                Response.length = CULng(Length)

            Catch ex As Exception
                Trace.WriteLine(ex.ToString())
                Trace.WriteLine($"Write request at 0x{Offset:X8} for {Length} bytes.")
                Response.errorno = 1
                Response.length = 0

            End Try

            MapView.Write(&H0, Response)

        End Sub

        Protected Overrides ReadOnly Property FscryptDiskProxyObjectName As String
            Get
                Return _ObjectName
            End Get
        End Property

        Protected Overrides ReadOnly Property FscryptDiskProxyModeFlags As FscryptDiskFlags
            Get
                Return FscryptDiskFlags.TypeProxy Or FscryptDiskFlags.ProxyTypeSharedMemory
            End Get
        End Property

    End Class

End Namespace
