using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace PlayHologramFiles
{
    class Program
    {
        static Mutex mutex = new Mutex(true, "{97D4D7C6-047D-4D0E-86C0-066B2D475DDA}");

        public static ConfigSettings configSettings;

        static void Main(string[] args)
        {

            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                //Already running, just gtfo
                Console.WriteLine(DateTime.Now + " Already running. Exit");
                Environment.Exit(0);
            }


            string machineName = "";
            string manufacturer = "";
            string emulator = "";
            string custom2 = "";
            string defaultFile = "";
            bool setToLoop = false;
            bool setToPlayAll = false;
            bool testNetwork = false;

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i].ToLower();
                if (arg == "-loop" || arg == "/loop")
                {
                    setToLoop = true;
                }
                if (arg == "-playall" || arg == "/playall")
                {
                    setToPlayAll = true;
                }
                if (arg == "-testnetwork" || arg == "/testnetwork")
                {
                    testNetwork = true;
                }
                if (arg == "-machinename" || arg == "/machinename")
                {
                    i++;
                    machineName = args[i];
                }
                if (arg == "-manufacturer" || arg == "/manufacturer")
                {
                    i++;
                    manufacturer = args[i];
                }
                if (arg == "-emulator" || arg == "/emulator")
                {
                    i++;
                    emulator = args[i];
                }
                if (arg == "-custom2" || arg == "/custom2")
                {
                    i++;
                    custom2 = args[i];
                }
                if (arg == "-default" || arg == "/default")
                {
                    i++;
                    defaultFile = args[i];
                }

            }

            Initialize();
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "log.txt");

            if(configSettings.WriteLog)
            {
                FileStream filestream = new FileStream(path, FileMode.Append);
                var streamwriter = new StreamWriter(filestream);
                streamwriter.AutoFlush = true;
                Console.SetOut(streamwriter);
                Console.SetError(streamwriter);
            }

            Console.WriteLine(DateTime.Now + " " + "Called with parameters set to: machineName: " + machineName + " manufacturer: " + manufacturer + " emulator: " + emulator + " custom2: " + custom2 + " defaultFile: " + defaultFile + " setToLoop: " + setToLoop + " setToPlayAll: " + setToPlayAll + " testNetwork: " + testNetwork);

            try
            {
                RunCommands(machineName, manufacturer, emulator, custom2, defaultFile, setToLoop, setToPlayAll, testNetwork);
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now + " " + "Error occurred in Main calling RunCommands: " 
                    + " machineName: " + machineName 
                    + " manufacturer: " + manufacturer
                    + " emulator: " + emulator
                    + " custom2: " + custom2
                    + " defaultFile: " + defaultFile 
                    + " setToLoop: " + setToLoop 
                    + " setToPlayAll: " + setToPlayAll 
                    + " testNetwork: " + testNetwork 
                    + " " + ex.Message);
            }
            //Console.WriteLine(DateTime.Now + " About to exit");
            Environment.Exit(0);
        }

        private static void RunCommands(string machineName, string manufacturer, string emulator, string custom2, string defaultFile, bool setToLoop, bool setToPlayAll, bool testNetwork)
        {
            //Connect to device
            ConnectionHandler connectionHandler = new ConnectionHandler();
            if (testNetwork)
            {
                connectionHandler.TestNetwork();
            }

            if(!connectionHandler.GetList())
            {
                //If we can't get the list, we are done so just exit.
                return;
            }

            List<HologramListItem> hologramFileNames = ParseList(connectionHandler.LastResponse);

            if (setToLoop)
            {
                connectionHandler.LoopFile();
            }
            if (setToPlayAll)
            {
                connectionHandler.PlayAll();
            }

            //Find the file to play
            //  check machineName, then manufacturer, then play default
            bool found = false;
            if (!string.IsNullOrEmpty(machineName))
            {
                HologramListItem item = hologramFileNames.FirstOrDefault(r => string.Equals(r.name, machineName + ".bin", StringComparison.OrdinalIgnoreCase));
                found = PlayFile(connectionHandler, item);
            }
            if (!found)
            {
                if (!string.IsNullOrEmpty(custom2))
                {
                    HologramListItem item = hologramFileNames.FirstOrDefault(r => string.Equals(r.name, custom2 + ".bin", StringComparison.OrdinalIgnoreCase));
                    found = PlayFile(connectionHandler, item);
                }
            }
            if (!found)
            {
                if (!string.IsNullOrEmpty(manufacturer))
                {
                    HologramListItem item = hologramFileNames.FirstOrDefault(r => string.Equals(r.name, manufacturer + ".bin", StringComparison.OrdinalIgnoreCase));
                    found = PlayFile(connectionHandler, item);
                }
            }
            if (!found)
            {
                if (!string.IsNullOrEmpty(emulator))
                {
                    HologramListItem item = hologramFileNames.FirstOrDefault(r => string.Equals(r.name, emulator + ".bin", StringComparison.OrdinalIgnoreCase));
                    found = PlayFile(connectionHandler, item);
                }
            }
            if (!found)
            {
                if (!string.IsNullOrEmpty(defaultFile))
                {
                    HologramListItem item = hologramFileNames.FirstOrDefault(r => string.Equals(r.name, defaultFile + ".bin", StringComparison.OrdinalIgnoreCase));
                    found = PlayFile(connectionHandler, item);
                }
            }
        }

        private static bool PlayFile(ConnectionHandler connectionHandler, HologramListItem item)
        {
            bool rtnVal = false;

            if (item != null)
            {
                rtnVal = true;
                PlayTheFile(connectionHandler, item, 0);
            }

            return rtnVal;
        }

        private static void PlayTheFile(ConnectionHandler connectionHandler, HologramListItem item, int retryAttempt)
        {
            if (connectionHandler.PlayFile(item.id))
            {
                if (retryAttempt < 3)
                {
                    //Check to see if it actually took. The stupid hologram often doesn't do anything.
                    if (connectionHandler.GetList())
                    {
                        List<HologramListItem> hologramFileNames = ParseList(connectionHandler.LastResponse);
                        //In theory loop until it takes (or 3 tries)
                        HologramListItem myItem = hologramFileNames.Find(hfn => hfn.runningItem == true);
                        if (myItem.id != item.id)
                        {
                            retryAttempt++;
                            Console.WriteLine(DateTime.Now + " Retry attempt for " + item.name + " attempt number " + retryAttempt);
                            PlayTheFile(connectionHandler, item, retryAttempt);
                        }
                    }
                }
            }
        }

        private static void Initialize()
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var builder = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", true, true)
                .AddJsonFile($"appsettings.{env}.json", true, true)
                .AddEnvironmentVariables();

            var config = builder.Build();

            configSettings = config.Get<ConfigSettings>();
        }

        private static List<HologramListItem> ParseList(string lastResponse)
        {
            //Console.WriteLine(DateTime.Now + " response:::::" + lastResponse + ":::::");
            List<HologramListItem> rtnVal = new List<HologramListItem>();

            int idx = 0;
            int fileNameLength = 0;
            string nextExecutingStatement = "";
            try
            {
                string beginningChars = "c30c38";
                string endingChars = "a4a8c2e3";
                idx = lastResponse.IndexOf(beginningChars);
                if (idx < 0)
                    return rtnVal;
                int endLength = lastResponse.IndexOf(endingChars) - 4;  //Remove the HE10 or HE11 that says if the list is looping or not
                if (endLength <= 0)
                    return rtnVal;

                idx += beginningChars.Length + 3; //There are 3 more chars after that I don't know what they represent
                while (idx < (endLength - 2))
                {
                    nextExecutingStatement = "GetNumberFromString";
                    fileNameLength = GetNumberFromString(lastResponse.Substring(idx, 2));   //Two digit file number implies a limit of 99 files are allowed on the device.
                    idx = idx + 2;
                    nextExecutingStatement = "lastResponse.Substring(idx, fileNameLength)";
                    string item = lastResponse.Substring(idx, fileNameLength);
                    HologramListItem listItem = new HologramListItem()
                    {
                        id = item.Substring(0, 2),
                        name = item.Substring(2),
                        runningItem = false
                    };
                    //Console.WriteLine(DateTime.Now + " " + listItem.id + " " + listItem.name);
                    rtnVal.Add(listItem);
                    idx += fileNameLength;
                }
                SetRunningItem(lastResponse, idx, rtnVal);
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now + " " + "Error occurred in Main calling ParseList: "
                    + " idx: " + idx
                    + " lastResponse:" + lastResponse
                    + " fileNameLength: " + fileNameLength
                    + " nextExecutingStatement: " + nextExecutingStatement
                    + " " + ex.Message + ex.StackTrace); ;
                throw;
            }

            return rtnVal;
        }

        private static void SetRunningItem(string lastResponse, int idx, List<HologramListItem> rtnVal)
        {
            try
            {
                if (idx + 2 <= lastResponse.Length)
                {
                    int itemNumber = GetNumberFromString(lastResponse.Substring(idx, 2));
                    HologramListItem item = rtnVal[itemNumber - 1];
                    item.runningItem = true;
                    //Console.WriteLine(DateTime.Now + " Running item is: " + item.id + " " + item.name);
                }
            }
            catch { }
        }

        private static int GetNumberFromString(string value)
        {
            int rtnVal = 0;

            int.TryParse(value, out rtnVal);

            return rtnVal;
        }


    }

}
