using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace GOR.ITT.Cesena
{
    class MainClass
    {
        // public static void Main(string[] args)
        public static void Main()
        {
            PCF8563_RTC rtc = new PCF8563_RTC();
            DS1822_Temp_Sensor temp = new DS1822_Temp_Sensor(); 

            while (true)
            {
                //Console.WriteLine(rtc.Lettura(2));
                //Console.WriteLine("{0} s; {1} mese", rtc.Seconds(), rtc.Month());
                
                //Console.WriteLine(temp.Lettura("28-0000062196f0")); 
                Console.WriteLine(temp.Misurazione("28-0000062196f0"));
                Console.WriteLine(temp.Misurazione("22-0000003c0ff9"));
                Console.WriteLine(""); 

                Thread.Sleep(1000);
            }
        }
    }
}
