CMD

aa 01 22 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 ce    power on

AA 01 2C 14 50 14 50 00 8A 07 EB 00 00 00 42 00 00 00 00 5D

AA 01 2C 14 50 14 50 00 8A 03 E8 00 00 00 42 00 00 00 00 56    1.38 1A

aa 01 2c 14 50 14 50 00 c8 0c 80 00 00 00 42 00 00 00 00 35

都是RESPONSE

receivedata --->AA-01-12-80-00-00-00-00-00-00-00-00-00-00 
 receivedata --->00-00-00-00-00-3D


[tutorial_1](https://stackoverflow.com/questions/34690108/c-sharp-scanning-com-ports-for-specific-input)


1.Scan COM Ports
2.Receive inputs from the devices
3.When an input has a specific phrase such as "connectAlready",
4.Close all ports and create a new one on the port that received the phrase.
5.Now that the program knows what COM port the Arduino is on, it can carry on its tasks and send it commands through SerialPorts.

-------------
Declare new serial port object for each serial port which you create:

At least four things to get and to handle your data:
1.Event handler for DataReceived for your serial port
2.Get the data from the underlying stream of the port.
3.Encoding.UTF8.GetString to convert the input data from byte[] to ASCII characters (Edit: consider of changing/skip this step and step 2 if the data received is not byte[] but ASCII
4.Check if the data string contains the data that you want
5.And as a last note, beware that the data from your serial port doesn't come together (like "conne" and then "ctAlready"). In such case, you need to copy your received data in a global buffer first then have additional checking (use Buffer.BlockCopy)
