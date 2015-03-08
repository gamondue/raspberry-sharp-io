#region References

using System;
using Raspberry.IO.GeneralPurpose;
using Raspberry.Timers;

#endregion

namespace Raspberry.IO.Components.Sensors.Temperature.Dht
{
    /// <summary>
    /// Represents a connection to DHT-22 humidity / temperature sensor (also known as Am2302).
    /// </summary>
    /// <remarks>
    /// Requires a fast IO connection (such as <see cref="MemoryGpioConnectionDriver"/>).
    /// Based on <see href="https://www.virtuabotix.com/virtuabotix-dht22-pinout-coding-guide/"/>.
    /// </remarks>
    public class DhtXxConnection : IDisposable
    {
        #region References

        private readonly IInputOutputBinaryPin pin;
        private decimal startLowTime = 18m;     // [ms] 
        private long middleTimeZeroOne = 500;   // [hundred ns] (ticks)
        private long errTime = 2000;            // [hundred ns] (ticks)
        private decimal timeOutDecimal = 100m;  // [ms]
        private long timeOutTicks;              // [hundred ns] (ticks)

        private long twoSeconds = 20000000;     // [hundred ns] (ticks)

        private long lastSampleTicks;           // ticks at last sample

        const int maxRetries = 10; 

        #endregion

        #region Instance Management

        /// <summary>
        /// Initializes a new instance of the <see cref="DhtXxConnection"/> class.
        /// </summary>
        /// <param name="pin">The pin.</param>
        public DhtXxConnection(IInputOutputBinaryPin pin)
        {
            this.pin = pin;
            pin.AsOutput();
            timeOutTicks = (long)timeOutDecimal * 100;
            lastSampleTicks = DateTime.UtcNow.Ticks + twoSeconds;
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
        /// <returns>The Dht data. Null if error</returns>
        public DhtXxData GetData(ref int retries)
        {
            DhtXxData data = null;
            var retryCount = maxRetries;

            long ticksFromLastSample = DateTime.UtcNow.Ticks - lastSampleTicks;
            //Console.Write(ticksFromLastSample.ToString() + " ");

            // DHT22: wait until 2 s from last sample (requirement from productor's data sheet)
            HighResolutionTimer.Sleep((decimal)((twoSeconds - ticksFromLastSample) / 10000)); 

            while (data == null && retryCount-- > 0)
            {
                try
                {
                    data = TryGetData();
                }
                catch
                {
                    retries = maxRetries - retryCount;
                    Console.Write("Retry: " + retries.ToString() + " "); 
                    data = null;
                }
            }
            lastSampleTicks = DateTime.UtcNow.Ticks;
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

        private DhtXxData TryGetData()
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

            // Prepare for reading
            pin.AsInput();

            bool err = false; 
            try
            {
                // Read acknowledgement from Dht
                if (SuccessfulWaitForLevel(true, "Ack.true TOut "))
                {
                    if (SuccessfulWaitForLevel(false, "Ack.false TOut "))
                    {
                        // Read 40 bits output, or time-out
                        var cnt = 7;
                        var idx = 0;
                        for (var i = 0; i < 40; i++)
                        {
                            if (SuccessfulWaitForLevel(true, "Bit high receiving TOut "))
                            { 
                                var start = DateTime.UtcNow.Ticks;
                                pin.Wait(false, timeOutDecimal);
                                var ticksLevelOn = (DateTime.UtcNow.Ticks - start);
                                if (ticksLevelOn > middleTimeZeroOne)
                                    if (ticksLevelOn < errTime)
                                        // add one at right position 
                                        // (zero is already there!)
                                        data[idx] |= (byte)(1 << cnt); 
                                    else
                                    {
                                        err = true;
                                        throw new TimeoutException("Byte " + idx + " bit " + i + " Timeout"); 
                                    }
                                if (cnt == 0)
                                {
                                    idx++;   // next byte
                                    cnt = 7; // restart at MSB
                                }
                                else
                                    cnt--;
                            }
                            else
                                err = true;
                        } // for
                    }
                    else 
                        err = true; 
                }
                else 
                    err = true; 
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
                    throw new Exception ("DHTXX Cheksum error");
                    return null;
                }

                decimal humidity = ((data[0] << 8) + data[1]) * 0.1m;

                var sign = 1;
                if ((data[2] & 0x80) != 0) // negative temperature
                {
                    data[2] = (byte)(data[2] & 0x7F);
                    sign = -1;
                }
                decimal temperature = sign * ((data[2] << 8) + data[3]) * 0.1m;

                return new DhtXxData
                {
                    Humidity = humidity,
                    Temperature = temperature
                };
            }
            else
                return null; 
        }
        /// <summary>
        /// Wait until parameter passed Digital level is reached in chip's data pin, or timeout
        /// If timeout, notify error on console
        /// </summary>
        /// <param name="LevelValue">Digital level we have to wait for</param>
        /// <param name="ErrorString"></param>
        /// <returns></returns>
        private bool SuccessfulWaitForLevel(bool LevelValue, string ErrorString)
        {
            long ticksTOut = DateTime.UtcNow.Ticks + timeOutTicks;
            pin.Wait(LevelValue, timeOutDecimal);
            if (DateTime.UtcNow.Ticks > ticksTOut)
            {
                throw new TimeoutException(ErrorString + " Timeout while waiting for pin status to be " + LevelValue); 
                return false;
            }
            else
                return true; 
        }
        #endregion
    }
}