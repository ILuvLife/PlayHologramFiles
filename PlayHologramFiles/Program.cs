using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PlayHologramFiles
{
    class Program
    {
        public static ConfigSettings configSettings;

        static void Main(string[] args)
        {
            string machineName = "";
            string manufacturer = "";
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

            Console.WriteLine(DateTime.Now + " " + "Called with parameters set to: machineName: " + machineName + " manufacturer: " + manufacturer + " defaultFile: " + defaultFile + " setToLoop: " + setToLoop + " setToPlayAll: " + setToPlayAll + " testNetwork: " + testNetwork);

            try
            {
                RunCommands(machineName, manufacturer, defaultFile, setToLoop, setToPlayAll, testNetwork);
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now + " " + "Error occurred in Main calling RunCommands: " 
                    + " machineName: " + machineName 
                    + " manufacturer: " + manufacturer 
                    + " defaultFile: " + defaultFile 
                    + " setToLoop: " + setToLoop 
                    + " setToPlayAll: " + setToPlayAll 
                    + " testNetwork: " + testNetwork 
                    + " " + ex.Message);
            }

        }

        private static void RunCommands(string machineName, string manufacturer, string defaultFile, bool setToLoop, bool setToPlayAll, bool testNetwork)
        {
            //Connect to device
            ConnectionHandler connectionHandler = new ConnectionHandler();
            if (testNetwork)
            {
                connectionHandler.TestNetwork();
            }

            connectionHandler.GetList();

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
                if (item != null)
                {
                    found = true;
                    connectionHandler.PlayFile(item.id);
                }
            }
            if (!found)
            {
                if (!string.IsNullOrEmpty(manufacturer))
                {
                    HologramListItem item = hologramFileNames.FirstOrDefault(r => string.Equals(r.name, manufacturer + ".bin", StringComparison.OrdinalIgnoreCase));
                    if (item != null)
                    {
                        found = true;
                        connectionHandler.PlayFile(item.id);
                    }
                }
            }
            if (!found)
            {
                if (!string.IsNullOrEmpty(defaultFile))
                {
                    HologramListItem item = hologramFileNames.FirstOrDefault(r => string.Equals(r.name, defaultFile + ".bin", StringComparison.OrdinalIgnoreCase));
                    if (item != null)
                    {
                        found = true;
                        connectionHandler.PlayFile(item.id);
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
            List<HologramListItem> rtnVal = new List<HologramListItem>();

            string beginningChars = "c30c38";
            string endingChars = "a4a8c2e3";
            int idx = lastResponse.IndexOf(beginningChars);
            if (idx < 0)
                return rtnVal;
            int endLength = lastResponse.IndexOf(endingChars) - 4;  //Remove the HE10 or HE11 that says if the list is looping or not
            if (endLength <= 0)
                return rtnVal;

            idx += beginningChars.Length + 3; //There are 3 more chars after that I don't know what they represent
            while (idx < (endLength - 2))
            {
                int fileNameLength = GetNumberFromString(lastResponse.Substring(idx, 2));   //Two digit file number implies a limit of 99 files are allowed on the device.
                idx = idx + 2;
                string item = lastResponse.Substring(idx, fileNameLength);
                HologramListItem listItem = new HologramListItem()
                {
                    id = item.Substring(0, 2),
                    name = item.Substring(2)
                };
                rtnVal.Add(listItem);
                idx += fileNameLength;
            }

            return rtnVal;
        }

        private static int GetNumberFromString(string value)
        {
            int rtnVal = 0;

            int.TryParse(value, out rtnVal);

            return rtnVal;
        }


    }

    class HologramListItem
    {
        public string id { get; set; }
        public string name { get; set; }
    }
}
