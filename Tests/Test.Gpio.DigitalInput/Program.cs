using Raspberry.IO.GeneralPurpose;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Test.Gpio.DigitalInput
{
    class Program
    {
        static void Main(string[] args)
        {
            ConnectorPin connPin = ConnectorPin.P1Pin40;
            ProcessorPin procPin = connPin.ToProcessor();
            var driver = GpioConnectionSettings.DefaultDriver;

            Console.WriteLine("Digital Input Sample: just a button");
            Console.WriteLine();

            Console.WriteLine("\tWatching Processor Pin: {0}, Connector pin: {1}", procPin, connPin);
            Console.WriteLine();

            Console.WriteLine("Press CTRL-C to stop");

            try
            {
                driver.Allocate(procPin, PinDirection.Input);

                var isHigh = driver.Read(procPin);

                while (true)
                {
                    var now = DateTime.Now;

                    Console.WriteLine(now + "." + now.Millisecond.ToString("000") + ": " + (isHigh ? "HIGH" : "LOW"));

                    driver.Wait(procPin, !isHigh, TimeSpan.FromDays(7)); //TODO: infinite
                    isHigh = !isHigh;
                }
            }
            finally
            {
                // Leaves the pin unreleased so that other processes can keep reading
                //driver.Release(procPin);
            }
        }
    }
}
