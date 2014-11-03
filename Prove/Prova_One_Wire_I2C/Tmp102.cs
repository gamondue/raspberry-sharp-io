using System;
using System.Diagnostics;
using System.Threading;

public class Tmp102
{
    private string i2cgetExe = "/usr/sbin/i2cget";
    private string i2cgetCmdArgs = "-y 1 0x48 0 w";
    private string hexString = "";
    private Process p;

    public Tmp102()
    {
        p = new Process();
    }

    public double tempC
    {
        get { return readRawTempData() * 0.0625; }
    }
    public double tempF
    {
        get { return this.tempC * 1.8 + 32; }
    }

    private int readRawTempData()
    {
        // Don't raise event when process exits
        p.EnableRaisingEvents = false;
        // We're using an executable not document, so UseShellExecute false
        p.StartInfo.UseShellExecute = false;
        // Redirect StandardError
        p.StartInfo.RedirectStandardError = true;
        // Redirect StandardOutput so we can capture it
        p.StartInfo.RedirectStandardOutput = true;
        // i2cgetExe has full path to executable
        // Need full path because UseShellExecute is false

        p.StartInfo.FileName = i2cgetExe;
        // Pass arguments as a single string
        p.StartInfo.Arguments = i2cgetCmdArgs;
        // Now run i2cget & wait for it to finish
        p.Start();
        p.WaitForExit();
        // Data returned in format 0xa017
        // Last 2 digits are actually most significant byte (MSB)
        // 2 digits right after 0x are really least significant byte (LSB)
        string data = p.StandardOutput.ReadToEnd();
        // Get LSB & parse as integer
        hexString = data.Substring(2, 2);
        int lsb = Int32.Parse(hexString,
            System.Globalization.NumberStyles.AllowHexSpecifier);
        // Get MSB & parse as integer
        hexString = data.Substring(4, 2);
        int msb = Int32.Parse(hexString,
            System.Globalization.NumberStyles.AllowHexSpecifier);
        // Shift bits as indicated in TMP102 docs & return
        return (((msb << 8) | lsb) >> 4);
    }

    public static void Main()
    {
        Tmp102 t = new Tmp102();
        while(true)
        {
            // Print temp in degrees C and F to console
            Console.WriteLine("{0}°C  {1}°F", t.tempC , t.tempF);
            Thread.Sleep(1000);
        }
    }
}