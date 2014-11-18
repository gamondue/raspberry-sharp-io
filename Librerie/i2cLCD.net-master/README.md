i2cLCD.net
==========

This library allow you to use LCD 1602 with i2c expansion module on raspberry pi with .net mono runtime.

INSTALL:

1) compile libNativeI2C by "make" command (on raspberry pi, linux).
2) compile your project with cLCD.vb (Visual Studio .NET)
3) put the libnativei2c.so in the same dir as executable. (in destination linux folder)

Functions:

1) Init(Addr as byte, Optional ShowCursor As Boolean = False) As Boolean
initialize display, clean all text, set cursor to 0 position.
Addr = i2c address, ShowCursor - show blinking cursor on display (useful for debuging)

2) Print(S As String) 
Prints ASCII line to LCD, from cusrsor position

3) SetCursor(x As Integer, y As Integer)
Sets the cursor to position y=0 or 1, x=0..16

4) CursorHome()
Sets cursor to zero position (beginning)

5) CursorLeft() CursorRight() , no comments ;)

6) SetLight(LightState As Boolean)
Set display light on/off

