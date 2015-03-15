﻿#region References

using System;
using Raspberry.IO.GeneralPurpose;
using Raspberry.Timers;
using UnitsNet;

#endregion

namespace Raspberry.IO.Components.Sensors.Temperature.Dht
{
    /// <summary>
    /// Represents a connection to DHT-22 humidity / temperature sensor (also known as AM2302).
    /// </summary>
    /// <remarks>
    /// Requires a fast IO connection (such as <see cref="MemoryGpioConnectionDriver"/>).
    /// Based on <see href="https://www.virtuabotix.com/virtuabotix-dht22-pinout-coding-guide/"/>. 
    /// 
    /// </remarks>
    public class Dht22Connection : IDisposable
    {
        #region fields
        private decimal startLowTime = 18m;     // [ms] 
        private long thresholdTimeZeroOne = 610;   // [hundred ns] (ticks) (26+70)/2 = 61 micro s
        private long errTime = 2000;            // [hundred ns] (ticks)
        private decimal timeOutDecimal = 100m;  // [ms]
        private long timeOutTicks;              // [hundred ns] (ticks)

        private long twoSeconds = 20000000;     // [hundred ns] (ticks)

        private long ticksAtLastSample;         // memorized to wait at least 2 s

        const int maxRetries = 10;
        #endregion

        #region References

        private readonly IInputOutputBinaryPin pin;

        #endregion

        #region Instance Management

        /// <summary>
        /// Initializes a new instance of the <see cref="DhtXxConnection"/> class.
        /// </summary>
        /// <param name="pin">The pin.</param>
        public Dht22Connection(IInputOutputBinaryPin pin)
        {
            this.pin = pin;
            pin.AsOutput();
            timeOutTicks = (long)timeOutDecimal * 100;
            ticksAtLastSample = DateTime.UtcNow.Ticks + twoSeconds;
        } 

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Close();
        }

        #endregion

        #region Method

        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <param name="retries">no. of repetitions in communication, for debugging</param>
        /// <returns>The DHT data. Null if error</returns>
        public DhtData GetData(ref int retries) 
        {
            DhtData data = null;
            var retryCount = maxRetries;

            retries = 0;
            while (data == null && retryCount-- > 0)
            {
                long ticksFromLastSample = DateTime.UtcNow.Ticks - ticksAtLastSample;
                //Console.Write(ticksFromLastSample.ToString() + " ");

                // DHT22: wait until 2 s from last sample (requirement from producer's data sheet) 
                ////HighResolutionTimer.Sleep((decimal)((twoSeconds - ticksFromLastSample) / 10000));

                try
                {
                    data = TryGetData();
                }
                catch (Exception ex)
                {
                    retries = maxRetries - retryCount - 1;
                    // next instruction is just for test
                    Console.WriteLine ("Tentative no. " + retries.ToString() + ". " + ex.Message); 
                    data = null;
                }
                ticksAtLastSample = DateTime.UtcNow.Ticks;
            }
            return data;
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public void Close()
        {
            pin.Dispose();
        }

        #endregion

        #region Private Helpers

        private DhtData TryGetData()
        {
            // Prepare bugger
            byte[] data = new byte[5];
            for (var i = 0; i < 5; i++)
                data[i] = 0;

            pin.Write(true);
            HighResolutionTimer.Sleep(100m);

            // Measure required by host : startLowTime ms down
            pin.Write(false);
            HighResolutionTimer.Sleep(startLowTime);
            pin.Write(true);
            HighResolutionTimer.Sleep(0.100m); // Tgo in datasheet

            // Prepare for reading
            pin.AsInput();

            int cnt = 7;
            int idx = 0;
            var bit = -1;
            int errCode = -1; 
            string errString = ""; 
            bool err = false;
            try
            {
                // Read acknowledgement from Dht
                errCode = 10;
                pin.Wait(true, timeOutDecimal);

                errCode = 20;
                pin.Wait(false, timeOutDecimal);

                // Read 40 bits input, or time-out
                cnt = 7;
                idx = 0;
                for (bit = 0; bit < 40; bit++)
                {
                    errCode = 30;
                    pin.Wait(true, timeOutDecimal); 
                        
                    errCode = 40;
                    var start = DateTime.UtcNow.Ticks;
                    pin.Wait(false, timeOutDecimal);

                    var ticksLevelOn = (DateTime.UtcNow.Ticks - start);
                    if (ticksLevelOn > thresholdTimeZeroOne)
                            data[idx] |= (byte)(1 << cnt); 
                    if (cnt == 0)
                    {
                        idx++;   // next byte
                        cnt = 7; // restart at MSB
                    }
                    else
                        cnt--;
                } // for
            }
            catch (Exception ex) 
            {
                // error decode based on last errCode value
                err = true;
                switch (errCode)
                {
                    case 10:
                        {
                            errString = "Ack.HIGH level timeout";
                            break;
                        }
                    case 20:
                        {
                            errString = "Ack.LOW level timeout";
                            break;
                        }
                    case 30:
                        {
                            errString = "Byte " + idx + " bit " + bit + " Timeout receiving HIGH level";
                            break;
                        }
                    case 40:
                        {
                            errString = "Byte " + idx + " bit " + bit + " Timeout receiving LOW level";
                            break;
                        }
                }
                //throw new Exception(errString + "\n" + ex.Message);
                throw new Exception(errString);
            }
            finally
            {
                // Prepare for next reading
                pin.Write(true);
            }
            if (!err)
            {
                var checkSum = data[0] + data[1] + data[2] + data[3];
                if ((checkSum & 0xff) != data[4])
                {
                    throw new Exception ("DHT22 Checksum error");
                    return null;
                }

                var humidity = ((data[0] << 8) + data[1]) * 0.1m;    // here DHT22 is different from DHT11

                var sign = 1;
                if ((data[2] & 0x80) != 0) // negative temperature
                {
                    data[2] = (byte)(data[2] & 0x7F);
                    sign = -1;
                }
                var temperature = sign * ((data[2] << 8) + data[3]) * 0.1m; // here DHT22 is different from DHT11

                return new DhtData
                {
                    RelativeHumidity = Ratio.FromPercent((double)humidity),
                    Temperature = UnitsNet.Temperature.FromDegreesCelsius((double)temperature)
                };
            }
            else
                return null; 
        }
        #endregion
    }
}