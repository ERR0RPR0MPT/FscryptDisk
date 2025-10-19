#Disable Warning CA1707 ' Identifiers should not contain underscores
#Disable Warning IDE1006 ' Naming Styles

Imports System.Runtime.InteropServices

Public Enum FSCRYPTDPROXY_REQ As ULong

    ''' <summary>
    ''' No operation.
    ''' </summary>
    FSCRYPTDPROXY_REQ_NULL

    ''' <summary>
    ''' Request information about I/O provider.
    ''' </summary>
    FSCRYPTDPROXY_REQ_INFO

    ''' <summary>
    ''' Request to read data.
    ''' </summary>
    FSCRYPTDPROXY_REQ_READ

    ''' <summary>
    ''' Request to write data.
    ''' </summary>
    FSCRYPTDPROXY_REQ_WRITE

    ''' <summary>
    ''' Request to connect to serial port, TCP/IP host etc. Only used internally between FscryptDisk driver and helper service.
    ''' </summary>
    FSCRYPTDPROXY_REQ_CONNECT

    ''' <summary>
    ''' Request to close proxy connection.
    ''' </summary>
    FSCRYPTDPROXY_REQ_CLOSE

    ''' <summary>
    ''' Request to unmap allocation range, that is mark as not in use. Sent to proxy services that indicate support for this
    ''' request by setting FSCRYPTDPROXY_FLAG_SUPPORTS_UNMAP flag in Flags response field. The request is sent in response to TRIM
    ''' requests sent by filesystem drivers.
    ''' </summary>
    FSCRYPTDPROXY_REQ_UNMAP

    ''' <summary>
    ''' Request to fill a range with zeros. Sent to proxy services that indicate support for this request by setting
    ''' FSCRYPTDPROXY_FLAG_SUPPORTS_ZERO flag in Flags response field. The request is sent to proxy when an all-zeros range is
    ''' written, or when FSCTL_SET_ZERO_DATA is received.
    ''' </summary>
    FSCRYPTDPROXY_REQ_ZERO
End Enum

<Flags>
Public Enum FSCRYPTDPROXY_FLAGS As ULong
    FSCRYPTDPROXY_FLAG_NONE = 0UL
    FSCRYPTDPROXY_FLAG_RO = 1UL
    FSCRYPTDPROXY_FLAG_SUPPORTS_UNMAP = 2UL
    FSCRYPTDPROXY_FLAG_SUPPORTS_ZERO = 4UL
End Enum

''' <summary>
''' Constants used in connection with FscryptDisk/Devio proxy communication.
''' </summary>
Public MustInherit Class FSCRYPTDPROXY_CONSTANTS
    Private Sub New()
    End Sub

    ''' <summary>
    ''' Header size when communicating using a shared memory object.
    ''' </summary>
    Public Const FSCRYPTDPROXY_HEADER_SIZE As Integer = 4096

    ''' <summary>
    ''' Default required alignment for I/O operations.
    ''' </summary>
    Public Const REQUIRED_ALIGNMENT As Integer = 1
End Class

<StructLayout(LayoutKind.Sequential)>
Public Structure FSCRYPTDPROXY_CONNECT_REQ
    Public Property request_code As FSCRYPTDPROXY_REQ
    Public Property flags As ULong
    Public Property length As ULong
End Structure

<StructLayout(LayoutKind.Sequential)>
Public Structure FSCRYPTDPROXY_CONNECT_RESP
    Public Property error_code As ULong
    Public Property object_ptr As ULong
End Structure

''' <summary>
''' Message sent by proxy service after connection has been established. This
''' message indicates what features this proxy service supports and total size
''' of virtual image and alignment requirement for I/O requests.
''' </summary>
<StructLayout(LayoutKind.Sequential)>
Public Structure FSCRYPTDPROXY_INFO_RESP

    ''' <summary>
    ''' Total size in bytes of virtual image
    ''' </summary>
    Public Property file_size As ULong

    ''' <summary>
    ''' Required alignment in bytes for I/O requests sent to this proxy service
    ''' </summary>
    Public Property req_alignment As ULong

    ''' <summary>
    ''' Flags from FSCRYPTDPROXY_FLAGS enumeration
    ''' </summary>
    Public Property flags As FSCRYPTDPROXY_FLAGS

End Structure

<StructLayout(LayoutKind.Sequential)>
Public Structure FSCRYPTDPROXY_READ_REQ
    Public Property request_code As FSCRYPTDPROXY_REQ
    Public Property offset As ULong
    Public Property length As ULong
End Structure

<StructLayout(LayoutKind.Sequential)>
Public Structure FSCRYPTDPROXY_READ_RESP
    Public Property errorno As ULong
    Public Property length As ULong
End Structure

<StructLayout(LayoutKind.Sequential)>
Public Structure FSCRYPTDPROXY_WRITE_REQ
    Public Property request_code As FSCRYPTDPROXY_REQ
    Public Property offset As ULong
    Public Property length As ULong
End Structure

<StructLayout(LayoutKind.Sequential)>
Public Structure FSCRYPTDPROXY_WRITE_RESP
    Public Property errorno As ULong
    Public Property length As ULong
End Structure
