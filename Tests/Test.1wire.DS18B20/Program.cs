using Raspberry.IO.Components.Sensors.Temperature.Ds18b20;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test._1wire.DS18B20
{
    class Program
    {
        // sensor's Ids
        //private static string thermometerId = "28-000006707ae6";
        //private static string thermometerId = "22-0000003c0ff9";
        private static string thermometerId = "28-0000062196f0";
        //private static string thermometerId = "28-0000062196f0";
        
        static void Main(string[] args)
        {
            Ds18b20Connection Tconnection = new Ds18b20Connection(thermometerId);
            
            Console.WriteLine("Ds18b20 Sample: 1wire digital temperature sensor ");
            Console.WriteLine();
            while (!Console.KeyAvailable)
            {
                Console.WriteLine(Tconnection.GetTemperatureCelsius());
                Console.WriteLine();
            }
        }
    }
}
