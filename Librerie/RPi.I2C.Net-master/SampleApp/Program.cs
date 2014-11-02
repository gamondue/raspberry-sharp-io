using System;
using System.Collections.Generic;
using System.Text;
using RPi.I2C.Net;

namespace SampleApp
{
	class Program
	{
		static void Main(string[] args)
		{
            byte REG_SECONDS = 0x02;
			using (var bus = RPi.I2C.Net.I2CBus.Open("/dev/i2c-1"))
			{
                while (true) 
                { 
                    //bus.WriteByte(42, 96);
                    bus.WriteByte(0x51, REG_SECONDS);
                    //byte[] res = bus.ReadBytes(42, 3);
                    byte[] res = bus.ReadBytes(0x51, 1);
                    Console.WriteLine(res[0]); 
                }
            }
		}
	}
}
