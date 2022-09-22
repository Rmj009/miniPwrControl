using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;

//namespace SimpleReceiveEventCS
//{
namespace MiniPwrSupply.DoWuzhiCmd
{
    internal class WuzhiCmd
    {
        private void TakeInitiatives() 
        {
        
        }


    }

	public partial class Form1 : Form
	{
		private SerialPort comport;
		private Int32 totalLength = 0;
		delegate void Display(Byte[] buffer);


		private void Form1_Load(object sender, EventArgs e)
		{
			comport = new SerialPort("COM5", 9600, Parity.None, 8, StopBits.One);
			comport.DataReceived += new SerialDataReceivedEventHandler(comport_DataReceived);
			if (!comport.IsOpen)
			{
				comport.Open();
			}
		}

		private void comport_DataReceived(Object sender, SerialDataReceivedEventArgs e)
		{
			Byte[] buffer = new Byte[1024];
			Int32 length = (sender as SerialPort).Read(buffer, 0, buffer.Length);
			Array.Resize(ref buffer, length);
			//Display d = new Display(DisplayText);
			//this.Invoke(d, new Object[] { buffer });
		}

	}
}