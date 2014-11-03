using System;
using System.Diagnostics;
using System.Threading;

public class RTC_PCF8563
{
    private string i2cgetExe = "/usr/sbin/i2cget";
    // -y 1 = canale I2C n. 1, 0x51 = indirizzo del device, 2 = n.ro registro (secondi), b = letto a Byte
    private string i2cgetCmdArgs = "-y 1 0x51 2 b";
    private string hexString = "";
    private Process p;

    public RTC_PCF8563()
    {
        p = new Process();
    }

    public int Seconds()
    {
        string d = readRawDateData();
        //Console.WriteLine("{0}  {1}", d.Substring(2, 1), d.Substring(3, 1));
        
        // da BCD a decimale
        // nibble più significativo
        int s = Int32.Parse(d.Substring(2, 1), 
            System.Globalization.NumberStyles.AllowHexSpecifier);
        // mette a zero il bit più significativo di questo nibble 
        s &= 0x7;
        s *= 10; 
        // nibble meno significativo
        s += Int32.Parse(d.Substring(3, 1),
            System.Globalization.NumberStyles.AllowHexSpecifier);
        return s;
    }

    private string readRawDateData()
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
        return (data);
    }

    public static void Main()
    {
        RTC_PCF8563 t = new RTC_PCF8563();
        while (true)
        {
            //Console.WriteLine("{0}  {1}", t.readRawDateData(), t.Seconds());
            Console.WriteLine("{0}", t.Seconds());
            Thread.Sleep(500);
        }
    }
}