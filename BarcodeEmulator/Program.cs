using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;
using TextCopy;
using WindowsInput;
using WindowsInput.Native;

namespace BarcodeEmulator
{
    class Program
    {
        static bool _continue;
        static SerialPort _serialPort;
        static string[] _conf = new string[] { };
        static bool _keyboardOutput = true;
        static bool _clipboardOutput = false;
        static string _fileOutput;
        static bool _trimData = true;
        static string _prefix;
        static string _postfix;

        /*static void Main(string[] args)
        {
        }*/
        public static void Main()
        {
            string message = "";
            StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
            Thread readThread = new Thread(Read);

            // load config
            string lineData = "";
            string confPath = System.IO.Path.GetFullPath(@".\config.txt");
            if (!System.IO.File.Exists(confPath)) {
                Console.WriteLine("Cannot find config at " + confPath);
                Console.WriteLine("Set params manually:\r\n");
            } else {
                Console.WriteLine("Load config from " + confPath);
                foreach (string line in System.IO.File.ReadLines(@".\config.txt"))
                {
                    if (!line.Contains("#") & !String.IsNullOrEmpty(line.Trim()))
                    {
                        lineData = line.Trim();

                        string confParam, confValue;
                        confParam = lineData.Split('=')[0].Trim();
                        confValue = lineData.Split('=')[1].Trim();

                        Console.WriteLine("Conf:" + confParam + "=>" + confValue);

                        if (confParam.Contains("connection"))
                        {
                            _conf = confValue.Split(',');
                        }
                        else if (confParam.Contains("clipboard"))
                        {
                            _clipboardOutput = bool.Parse(confValue);
                        }
                        else if (confParam.Contains("keyboard"))
                        {
                            _keyboardOutput = bool.Parse(confValue);
                        }
                        else if (confParam.Contains("file"))
                        {
                            _fileOutput = confValue;
                        }
                        else if (confParam.Contains("trimData"))
                        {
                            _trimData = bool.Parse(confValue);
                        }
                        else if (confParam.Contains("prefix"))
                        {
                            _prefix = confValue;
                        }
                        else if (confParam.Contains("postfix"))
                        {
                            _postfix = confValue;
                        }

                    }
                }
            }

            // Create a new SerialPort object with default settings.
            _serialPort = new SerialPort();

            if (_conf.Length == 6)
            {
                //#port,baud rate,databits,parity,StopBits,Handshake
                _serialPort.PortName = _conf[0];
                _serialPort.BaudRate = int.Parse(_conf[1]);
                _serialPort.DataBits = int.Parse(_conf[2]);
                _serialPort.Parity = (Parity)Enum.Parse(typeof(Parity), _conf[3], true);
                _serialPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), _conf[4], true);
                _serialPort.Handshake = (Handshake)Enum.Parse(typeof(Handshake), _conf[5], true);
            }
            else
            {
                // Allow the user to set the appropriate properties. 
                _serialPort.PortName = SetPortName(_serialPort.PortName);
                _serialPort.BaudRate = SetPortBaudRate(_serialPort.BaudRate);
                _serialPort.DataBits = SetPortDataBits(_serialPort.DataBits);
                _serialPort.Parity = SetPortParity(_serialPort.Parity);
                _serialPort.StopBits = SetPortStopBits(_serialPort.StopBits);
                _serialPort.Handshake = SetPortHandshake(_serialPort.Handshake);
            }

            // Set the read/write timeouts
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;


            _serialPort.Open();
            _serialPort.DiscardInBuffer();

            _continue = true;
            readThread.Start();

            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Begin reading from " + _serialPort.PortName + "...");
            Console.WriteLine("Type QUIT or Ctrl+C to exit (or close this window)");

            Console.CancelKeyPress += delegate(object sender, ConsoleCancelEventArgs e) {
                e.Cancel = true;
                _continue = false;
                Console.WriteLine("Ctrl-C received. Exitting...");
            };

            
            while (_continue)
            {
                //message = Console.ReadLine();
                try
                {
                    message = Reader.ReadLine(500);
                } catch
                {

                }

                if (stringComparer.Equals("quit", message))
                {
                    _continue = false;
                }

            }

            readThread.Join();
            _serialPort.Close();
        }

        public static void Read()
        {
            var sim = new InputSimulator();

            while (_continue)
            {
                try
                {
                    string message = _serialPort.ReadLine();

                    if (_trimData)
                    {
                        message = message.Trim();
                    }

                    if (!String.IsNullOrEmpty(_prefix))
                    {
                        message = _prefix + message;
                    }

                    if (!String.IsNullOrEmpty(_postfix))
                    {
                        message = message + _postfix;
                    }

                    Console.WriteLine(message);

                    if (_clipboardOutput)
                    {
                        ClipboardService.SetText(message);
                    }

                    if (_keyboardOutput)
                    {
                        sim.Keyboard.TextEntry(message);
                    }
                    if (!String.IsNullOrEmpty(_fileOutput))
                    {
                        string fileRecord = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + ";\t" + message.Trim() + Environment.NewLine;
                        System.IO.File.AppendAllText(_fileOutput, fileRecord);
                    }

                }
                catch (TimeoutException) { }
            }
        }

        // Display Port values and prompt user to enter a port.
        public static string SetPortName(string defaultPortName)
        {
            string portName;

            Console.WriteLine("Available Ports:");
            foreach (string s in SerialPort.GetPortNames())
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Enter COM port value (Default: {0}): ", defaultPortName);
            portName = Console.ReadLine();

            if (portName == "" || !(portName.ToLower()).StartsWith("com"))
            {
                portName = defaultPortName;
            }
            return portName;
        }
        // Display BaudRate values and prompt user to enter a value.
        public static int SetPortBaudRate(int defaultPortBaudRate)
        {
            string baudRate;

            Console.Write("Baud Rate(default:{0}): ", defaultPortBaudRate);
            baudRate = Console.ReadLine();

            if (baudRate == "")
            {
                baudRate = defaultPortBaudRate.ToString();
            }

            return int.Parse(baudRate);
        }

        // Display PortParity values and prompt user to enter a value.
        public static Parity SetPortParity(Parity defaultPortParity)
        {
            string parity;

            Console.WriteLine("Available Parity options:");
            foreach (string s in Enum.GetNames(typeof(Parity)))
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Enter Parity value (Default: {0}):", defaultPortParity.ToString(), true);
            parity = Console.ReadLine();

            if (parity == "")
            {
                parity = defaultPortParity.ToString();
            }

            return (Parity)Enum.Parse(typeof(Parity), parity, true);
        }
        // Display DataBits values and prompt user to enter a value.
        public static int SetPortDataBits(int defaultPortDataBits)
        {
            string dataBits;

            Console.Write("Enter DataBits value (Default: {0}): ", defaultPortDataBits);
            dataBits = Console.ReadLine();

            if (dataBits == "")
            {
                dataBits = defaultPortDataBits.ToString();
            }

            return int.Parse(dataBits.ToUpperInvariant());
        }

        // Display StopBits values and prompt user to enter a value.
        public static StopBits SetPortStopBits(StopBits defaultPortStopBits)
        {
            string stopBits;

            Console.WriteLine("Available StopBits options:");
            foreach (string s in Enum.GetNames(typeof(StopBits)))
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Enter StopBits value (None is not supported and \n" +
             "raises an ArgumentOutOfRangeException. \n (Default: {0}):", defaultPortStopBits.ToString());
            stopBits = Console.ReadLine();

            if (stopBits == "")
            {
                stopBits = defaultPortStopBits.ToString();
            }

            return (StopBits)Enum.Parse(typeof(StopBits), stopBits, true);
        }
        public static Handshake SetPortHandshake(Handshake defaultPortHandshake)
        {
            string handshake;

            Console.WriteLine("Available Handshake options:");
            foreach (string s in Enum.GetNames(typeof(Handshake)))
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Enter Handshake value (Default: {0}):", defaultPortHandshake.ToString());
            handshake = Console.ReadLine();

            if (handshake == "")
            {
                handshake = defaultPortHandshake.ToString();
            }

            return (Handshake)Enum.Parse(typeof(Handshake), handshake, true);
        }
    }

    class Reader
    {
        private static Thread inputThread;
        private static AutoResetEvent getInput, gotInput;
        private static string input;

        static Reader()
        {
            getInput = new AutoResetEvent(false);
            gotInput = new AutoResetEvent(false);
            inputThread = new Thread(reader);
            inputThread.IsBackground = true;
            inputThread.Start();
        }

        private static void reader()
        {
            while (true)
            {
                getInput.WaitOne();
                input = Console.ReadLine();
                gotInput.Set();
            }
        }

        // omit the parameter to read a line without a timeout
        public static string ReadLine(int timeOutMillisecs = Timeout.Infinite)
        {
            getInput.Set();
            bool success = gotInput.WaitOne(timeOutMillisecs);
            if (success)
                return input;
            else
                throw new TimeoutException("User did not provide input within the timelimit.");
        }
    }
}
