Imports System.Runtime.InteropServices
Imports System.Threading.Thread

Public Class cLCD

#Region "### LIB DECLARATION ###"
    Private BusHandle As Integer = 0

    <DllImport("libnativei2c.so", EntryPoint:="openBus", SetLastError:=True)>
    Private Shared Function OpenBus(busFileName As String) As Integer
    End Function

    <DllImport("libnativei2c.so", EntryPoint:="closeBus", SetLastError:=True)>
    Private Shared Function CloseBus(busHandle As Integer) As Integer
    End Function

    <DllImport("libnativei2c.so", EntryPoint:="readBytes", SetLastError:=True)>
    Private Shared Function ReadBytes(busHandle As Integer, addr As Integer, buf As Byte(), len As Integer) As Integer
    End Function

    <DllImport("libnativei2c.so", EntryPoint:="writeBytes", SetLastError:=True)>
    Private Shared Function WriteBytes(busHandle As Integer, addr As Integer, buf As Byte(), len As Integer) As Integer
    End Function
#End Region

    Private Addr As Integer

    Private BT As New BitArray({CByte(0)})

    Public Property RS = 0
    Public Property RW = 1
    Public Property ES = 2
    Public Property Lite = 3

#Region "### RAWRITE ###"

    Private Sub WriteByte(X As Byte, Optional Half As Boolean = False)
        Dim WByte As New BitArray({X})
        If Half = False Then HalfWrite({WByte(4), WByte(5), WByte(6), WByte(7)})
        HalfWrite({WByte(0), WByte(1), WByte(2), WByte(3)})
    End Sub

    Private Sub HalfWrite(T As Boolean())
        BT(4) = T(0)
        BT(5) = T(1)
        BT(6) = T(2)
        BT(7) = T(3)
        Pulse()
    End Sub

    Private Function ToByte(X As BitArray) As Byte
        Dim W(0) As Byte
        X.CopyTo(W, 0)
        Return W(0)
    End Function


    Private Sub WR()
        Dim W(0) As Byte
        BT.CopyTo(W, 0)
        WriteBytes(BusHandle, Addr, W, 1)
    End Sub

    Private Sub Pulse()
        BT(ES) = 1
        WR()
        Sleep(1)
        BT(ES) = 0
        WR()
    End Sub
#End Region


    Public Function Init(Addr As Byte, Optional ShowCursor As Boolean = False) As Boolean
        Me.Addr = Addr

        BusHandle = OpenBus("/dev/i2c-1")
        If BusHandle < 0 Then Return False


        BT(Lite) = 1
        BT(RW) = 0
        BT(ES) = 0
        BT(RS) = 0
        WR()


        'INIT SEQUENCE
        WriteByte(2)
        Sleep(10)
        WriteByte(40)

        Sleep(10)

        Dim CMD As New BitArray({CByte(15)})
        CMD(0) = ShowCursor
        CMD(1) = ShowCursor
        WriteByte(ToByte(CMD))

        Sleep(10)

        WriteByte(1)

        Sleep(10)

        WriteByte(3)

        Return True
    End Function

    Public Sub Close()
        If BusHandle > 0 Then CloseBus(BusHandle)
    End Sub

    Public Sub Print(S As String)
        BT(RS) = 1
        For Each B In System.Text.Encoding.ASCII.GetBytes(S)
            WriteByte(B)
        Next
    End Sub

    Public Sub SetLight(LightState As Boolean)
        BT(Lite) = LightState
        WR()
    End Sub


    Public Sub SetCursor(x As Integer, y As Integer)
        BT(RS) = 0

        Dim Pos As Integer = x + (y * 64)
        Dim CMD As New BitArray({CByte(Pos)})
        CMD(7) = 1
        WriteByte(ToByte(CMD))
    End Sub

    Public Sub CursorLeft()
        BT(RS) = 0
        WriteByte(16)
    End Sub

    Public Sub CursorRight()
        BT(RS) = 0
        WriteByte(20)
    End Sub

    Public Sub CursorHome()
        BT(RS) = 0
        WriteByte(2)
    End Sub

    Public Sub Clean()
        BT(RS) = 0
        WriteByte(1)
    End Sub



End Class
