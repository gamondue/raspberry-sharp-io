using System;
using System.Diagnostics;

namespace GOR.ITT.Cesena
{
    public class DS1822_Temp_Sensor //: ISensor
    {
        // programma ispirato a: 
        // http://bradsrpi.blogspot.it/2014/07/c-mono-code-to-read-tmp102-i2c.html

        private string program = "/bin/cat";
        private string programArguments = " /sys/bus/w1/devices/SENS_ID/w1_slave";
        //private string programArguments = "/home/pi/testSDL_PCF8563.py";

        private Process p;

        public DS1822_Temp_Sensor()
        {
            p = new Process();
            // Don't raise event when process exits
            p.EnableRaisingEvents = false;
            // We're using an executable not document, so UseShellExecute false
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            //p.StartInfo.WorkingDirectory 

            // Redirect 
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardError = true;

            p.StartInfo.FileName = program; // Need full path because UseShellExecute is false
            //p.StartInfo.Arguments = programArguments; DOPO! 

            //wr = p.StandardInput;
            //rr = p.StandardOutput;
        }

        public string Lettura(string idSensore)
        {
            string data = null;
            string argsConSensore = programArguments.Replace("SENS_ID", idSensore); 
            //Console.WriteLine(argsConSensore);
            try
            {
                // Pass arguments as a single string
                p.StartInfo.Arguments = argsConSensore; 

                // Now run cat & wait for it to finish
                p.Start();
                p.WaitForExit();
                data = p.StandardOutput.ReadToEnd().ToString();
                //Console.WriteLine(data + " | Lung." + data.Length);

                p.Close();
                p.Dispose(); 
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message); 
                data = ex.Message; 
            }

            return (data);
        }

        public int LetturaInt(string idSensore)
        {
            string argsConSensore = programArguments.Replace("SENS_ID", idSensore);
            int numero = int.MaxValue; 
            try
            {
                // Now run cat & wait for it to finish
                p.Start();
                p.WaitForExit();
                numero = int.Parse(p.StandardOutput.ReadToEnd().Substring(69));

                p.Close();
                p.Dispose();
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message); 
                numero = int.MaxValue; 
            }

            return (numero);
        }

        public double Misurazione(string idSensore)
        {
            double numero = double.MaxValue;

            try
            {
                numero = double.Parse(Lettura(idSensore).Substring(69)) / 1000.0;
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
                numero = double.NaN;
            }
            return numero;
        }

        /*public Measurement LastMeasurement
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool Simulation
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public double AlarmMax
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public double AlarmMin
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public double MaxValue
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public double MinValue
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int ReadingPhase()
        {
            throw new NotImplementedException();
        }

        public Measurement MeasurementPhase()
        {
            throw new NotImplementedException();
        }

        public Measurement MeasurementPhase(DateTime SimulationTime)
        {
            throw new NotImplementedException();
        }

        public Measurement MeasurementPhase(int nLecture, double standardDeviation)
        {
            throw new NotImplementedException();
        }

        public void Inizialization()
        {
            throw new NotImplementedException();
        }

        public double LoopPhase()
        {
            throw new NotImplementedException();
        }

        public void StartCalibration()
        {
            throw new NotImplementedException();
        }

        public void PointCalibration(double value)
        {
            throw new NotImplementedException();
        }

        public void EndCalibration()
        {
            throw new NotImplementedException();
        }*/
    }
}
