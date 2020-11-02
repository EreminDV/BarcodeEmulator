# Problem 
I cannot switch Barcode scanner to keyboard wedge mode (this model not support it).
Barcode scanner registerd in system as device connected to COM3 port.
Scanned data can be received as text line son COM3 port.

My friend wants to use scanner on her website admin panel. 
When he pressed button on scanner he wanst to get data pasted to input filed.


# BarcodeEmulator
Keyboard Wedge Emulation for Barcode scanner with serial (COM) virtual port 

# Purpose
Simple bridge: read line from COM-port, write to clipboard (or simulate keyboard input).

# Config
all in .\config.txt
* you must set COM port connection params: name, baud rate and etc.
* you can specify output: clipboard or keyboars or both
* by default, log will be written to .\barcode.scan.log
* you can enable data tirmming (remove whitespaces)
* you can add prefix or suffix for readed data

# Requirments
.net framework 4.7.2
but you can easy (Ctrl-C + Ctrl-V) port it to .net Core 2 or .net Core 3 or .Net FW 4.8


This is a 10-minute cooked gift for my friend.


# Known bugs
* Scanner not send <cr><lf> ("\r\n") at the end of data.
  we use scanner configuration software to configure suffix (#0D#0A) for scanned data.
  Although, you can add some code in Read function (at Program.cs) near  
  _serialPort.ReadLine()
  to set read timeout 
  or use _serialPort.Read(buffer,0, 13); // for EAN13
