using Microsoft.WindowsAPICodePack.Net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace PlayHologramFiles
{
    class ConnectionHandler
    {
        private static string hologramIpAddress= "192.168.4.1";
        private static int hologramPort = 5233;
        private static int hologramSendPort = 5499;

        private static string commandEndingCharacters = "a4a8c2e3";

        public string LastResponse { get; set; } 

        public bool GetList()
        {
            bool rtnVal = false;

            try
            {
                IPEndPoint localEndPoint = CreateLocalEndPoint();
                using (TcpClient client = new TcpClient(localEndPoint))
                {
                    client.Connect(hologramIpAddress, hologramPort);
                    NetworkStream stream = client.GetStream();

                    string request = "c31c38abc";
                    byte[] buffer;
                    using (MemoryStream command = new MemoryStream())
                    {
                        command.Write(Encoding.ASCII.GetBytes(request));
                        command.Write(Encoding.ASCII.GetBytes(commandEndingCharacters));
                        buffer = command.ToArray();
                    }

                    stream.Write(buffer, 0, buffer.Length);

                    //Receive the response
                    System.Threading.Thread.Sleep(500);//wait just a second to get stuff back

                    StringBuilder totalResponse = new StringBuilder();
                    int dataRead = 0;
                    byte[] resp = new byte[2048];
                    while (stream.DataAvailable)
                    {
                        dataRead = stream.Read(resp, 0, resp.Length);
                        totalResponse.Append(Encoding.ASCII.GetString(resp));
                    }

                    LastResponse = totalResponse.ToString();
                }

                rtnVal = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now + " " + "Error in ConnectionHandler.GetList(). The error message is: " + ex.Message);
                throw;
            }

            return rtnVal;
        }

        public bool PlayFile(string id)
        {
            bool rtnVal = false;

            try
            {
                IPEndPoint localEndPoint = CreateLocalEndPoint();
                using (TcpClient client = new TcpClient(localEndPoint))
                {
                    client.Connect(hologramIpAddress, hologramPort);
                    NetworkStream stream = client.GetStream();

                    string request = "c31c40abe" + id + commandEndingCharacters;
                    byte[] buffer;
                    using (MemoryStream command = new MemoryStream())
                    {
                        command.Write(Encoding.ASCII.GetBytes(request));
                        buffer = command.ToArray();
                    }
                    stream.Write(buffer, 0, buffer.Length);

                    //Receive the response
                    System.Threading.Thread.Sleep(500);//wait just a second to get stuff back

                    StringBuilder totalResponse = new StringBuilder();
                    int dataRead = 0;
                    byte[] resp = new byte[2048];
                    while (stream.DataAvailable)
                    {
                        dataRead = stream.Read(resp, 0, resp.Length);
                        totalResponse.Append(Encoding.ASCII.GetString(resp));
                    }

                    LastResponse = totalResponse.ToString();
                    int idx = LastResponse.IndexOf(commandEndingCharacters);
                    if (idx > 0)
                        LastResponse = LastResponse.Substring(0, idx);

                }

                rtnVal = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now + " " + "Error in ConnectionHandler.PlayFile(). The error message is: " + ex.Message);
                throw;
            }

            return rtnVal;
        }


        public bool LoopFile()
        {
            bool rtnVal = false;

            try
            {
                IPEndPoint localEndPoint = CreateLocalEndPoint();
                using (TcpClient client = new TcpClient(localEndPoint))
                {
                    client.Connect(hologramIpAddress, hologramPort);
                    NetworkStream stream = client.GetStream();

                    string request = "c31c37abc" + commandEndingCharacters;
                    byte[] buffer;
                    using (MemoryStream command = new MemoryStream())
                    {
                        command.Write(Encoding.ASCII.GetBytes(request));
                        buffer = command.ToArray();
                    }
                    stream.Write(buffer, 0, buffer.Length);

                    //Receive the response
                    System.Threading.Thread.Sleep(500);//wait just a second to get stuff back

                    StringBuilder totalResponse = new StringBuilder();
                    int dataRead = 0;
                    byte[] resp = new byte[2048];
                    while (stream.DataAvailable)
                    {
                        dataRead = stream.Read(resp, 0, resp.Length);
                        totalResponse.Append(Encoding.ASCII.GetString(resp));
                    }

                    LastResponse = totalResponse.ToString();
                    int idx = LastResponse.IndexOf(commandEndingCharacters);
                    if (idx > 0)
                        LastResponse = LastResponse.Substring(0, idx);

                }

                rtnVal = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now + " " + "Error in ConnectionHandler.LoopFile(). The error message is: " + ex.Message);
                throw;
            }

            return rtnVal;
        }

        public bool PlayAll()
        {
            bool rtnVal = false;

            try
            {
                IPEndPoint localEndPoint = CreateLocalEndPoint();
                using (TcpClient client = new TcpClient(localEndPoint))
                {
                    client.Connect(hologramIpAddress, hologramPort);
                    NetworkStream stream = client.GetStream();

                    string request = "c31c36abc" + commandEndingCharacters;
                    byte[] buffer;
                    using (MemoryStream command = new MemoryStream())
                    {
                        command.Write(Encoding.ASCII.GetBytes(request));
                        buffer = command.ToArray();
                    }
                    stream.Write(buffer, 0, buffer.Length);

                    //Receive the response
                    System.Threading.Thread.Sleep(500);//wait just a second to get stuff back

                    StringBuilder totalResponse = new StringBuilder();
                    int dataRead = 0;
                    byte[] resp = new byte[2048];
                    while (stream.DataAvailable)
                    {
                        dataRead = stream.Read(resp, 0, resp.Length);
                        totalResponse.Append(Encoding.ASCII.GetString(resp));
                    }

                    LastResponse = totalResponse.ToString();
                    int idx = LastResponse.IndexOf(commandEndingCharacters);
                    if (idx > 0)
                        LastResponse = LastResponse.Substring(0, idx);

                }

                rtnVal = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now + " " + "Error in ConnectionHandler.PlayAll(). The error message is: " + ex.Message);
                throw;
            }

            return rtnVal;
        }


        private static IPEndPoint CreateLocalEndPoint()
        {
            string wanAdapter = "";
            wanAdapter = FindAdapter();
            if(string.IsNullOrEmpty(wanAdapter))
            {
                wanAdapter = Program.configSettings.WifiName;
                Console.WriteLine(DateTime.Now + " " + "Used config for wifi name");
            }
            string wanIpAddress = "";   // "192.168.4.2";   //Need to set this??? - not really

            try
            {
                foreach (NetworkInterface nwInterface in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (string.IsNullOrWhiteSpace(wanAdapter) || string.Compare(nwInterface.Name, wanAdapter, true) == 0)
                    {
                        IPInterfaceProperties ipProperties = nwInterface.GetIPProperties();
                        foreach (UnicastIPAddressInformation unicastIp in ipProperties.UnicastAddresses)
                            if (unicastIp.Address.AddressFamily == AddressFamily.InterNetwork && (string.IsNullOrWhiteSpace(wanIpAddress) || unicastIp.Address.ToString() == wanIpAddress))
                                return new IPEndPoint(unicastIp.Address, 0);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now + " " + "Error in ConnectionHandler.CreateLocalEndPoint(). The error message is: " + ex.Message);
                throw;
            }
            return null;
        }


        /// <summary>
        /// Finds the network adapter that is connected to the hologram fan
        /// </summary>
        /// <returns></returns>
        private static string FindAdapter()
        {
            string rtnVal = "";

            string adapterId = "";
            var networks = NetworkListManager.GetNetworks(NetworkConnectivityLevels.Connected);

            foreach (Network network in networks)
            {
                if (network.Name == "3D-U9w68jYS")
                {
                    foreach (NetworkConnection conn in network.Connections)
                    {
                        adapterId = conn.AdapterId.ToString();
                    }
                    break;
                }
            }


            NetworkInterface[] nwInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface nwInterface in nwInterfaces)
            {
                Guid nwIntGuid = Guid.Parse(nwInterface.Id);
                if (string.Equals(adapterId, Guid.Parse(nwInterface.Id).ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    rtnVal = nwInterface.Name;
                    break;
                }
            }


            return rtnVal;
        }

        public void TestNetwork()
        {
            try
            {
                string adapterId = "";
                
                var networks = NetworkListManager.GetNetworks(NetworkConnectivityLevels.Connected);

                foreach (Network network in networks)
                {
                    //Name property corresponds to the name I originally asked about
                    Console.WriteLine("[" + network.Name + "]");
                    if(network.Name == "3D-U9w68jYS")
                    {
                        Console.WriteLine("\t[NetworkConnections]");
                        foreach (NetworkConnection conn in network.Connections)
                        {
                            //Print network interface's GUID
                            Console.WriteLine("\t\t" + conn.AdapterId.ToString());
                            adapterId = conn.AdapterId.ToString();
                        }
                   }
                }


                NetworkInterface[] nwInterfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface nwInterface in nwInterfaces)
                {
                    Guid nwIntGuid = Guid.Parse(nwInterface.Id);
                    if (string.Equals(adapterId, Guid.Parse(nwInterface.Id).ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("Found it " + nwInterface.Name + " " + nwInterface.Id);
                    }

                    IPInterfaceProperties ipProperties = nwInterface.GetIPProperties();
                    foreach (UnicastIPAddressInformation unicastIp in ipProperties.UnicastAddresses)
                    {
                        if (unicastIp.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            Console.WriteLine(nwInterface.Name + " " + nwInterface.Id + " " + unicastIp.Address.ToString() + " ");
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now + " " + "Error in ConnectionHandler.TestNetwork(). The error message is: " + ex.Message);
                throw;
            }

        }


        #region NoLongerUsed
        public bool DeleteFile(string id)
        {
            bool rtnVal = false;

            try
            {
                using (TcpClient client = new TcpClient())
                {
                    client.Connect(hologramIpAddress, hologramPort);
                    NetworkStream stream = client.GetStream();

                    string request = "c31c39abe" + id + commandEndingCharacters;
                    byte[] buffer;
                    using (MemoryStream command = new MemoryStream())
                    {
                        command.Write(Encoding.ASCII.GetBytes(request));
                        buffer = command.ToArray();
                    }
                    stream.Write(buffer, 0, buffer.Length);

                    //Receive the response
                    System.Threading.Thread.Sleep(500);//wait just a second to get stuff back


                    StringBuilder totalResponse = new StringBuilder();
                    int dataRead = 0;
                    byte[] resp = new byte[2048];
                    while (stream.DataAvailable)
                    {
                        dataRead = stream.Read(resp, 0, resp.Length);
                        totalResponse.Append(Encoding.ASCII.GetString(resp));
                    }

                    LastResponse = totalResponse.ToString();
                    int idx = LastResponse.IndexOf(commandEndingCharacters);
                    if (idx > 0)
                        LastResponse = LastResponse.Substring(0, idx);

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now + " " + "Error in ConnectionHandler.DeleteFile(). The error message is: " + ex.Message);
                throw;
            }

            return rtnVal;
        }

        public bool SendFile(string filePath)
        {
            bool rtnVal = false;

            if (!File.Exists(filePath))
            {
                //Write error log
                return true;
            }


            try
            {
                using (TcpClient client = new TcpClient())
                {
                    client.NoDelay = true;
                    client.Connect(hologramIpAddress, hologramSendPort);
                    NetworkStream stream = client.GetStream();

                    FileInfo fileInfo = new FileInfo(filePath);

                    using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        //Write the initial send command with the file name
                        string request = "d31d66DEO";
                        byte[] buffer;
                        using (MemoryStream command = new MemoryStream())
                        {
                            request = "d31d66DEO";
                            string commandEndingCharacters = "a4a8c2e3";
                            command.Write(Encoding.ASCII.GetBytes(request));
                            command.WriteByte(0x00);
                            command.WriteByte(0x69);
                            command.WriteByte(0x12);
                            command.WriteByte(0xc0);
                            command.Write(Encoding.ASCII.GetBytes(fileInfo.Name));
                            command.Write(Encoding.ASCII.GetBytes(commandEndingCharacters));
                            buffer = command.ToArray();
                        }
                        stream.Write(buffer, 0, buffer.Length);
                        stream.Flush();


                        System.Threading.Thread.Sleep(500);//wait just a second to get stuff back
                        StringBuilder totalResponse = new StringBuilder();
                        byte[] resp = new byte[2048];
                        int dataRead = 0;
                        while (stream.DataAvailable)
                        {
                            dataRead = stream.Read(resp, 0, resp.Length);
                            totalResponse.Append(Encoding.ASCII.GetString(resp));
                        }
                        LastResponse = totalResponse.ToString();

                        int idx = LastResponse.IndexOf(commandEndingCharacters);
                        if (idx > 0)
                            LastResponse = LastResponse.Substring(0, idx);
                        //if(LastResponse == "d30d66DEJffff")
                        //{
                        //  It worked?
                        System.Threading.Thread.Sleep(200);//wait just a second to get stuff back
                        buffer = ReadFileIntoMemory(fileStream, stream);

                        System.Threading.Thread.Sleep(500);//wait just a second to get stuff back
                        dataRead = 0;
                        totalResponse.Clear();
                        while (stream.DataAvailable)
                        {
                            dataRead = stream.Read(resp, 0, resp.Length);
                            totalResponse.Append(Encoding.ASCII.GetString(resp));
                        }
                        LastResponse = Encoding.ASCII.GetString(resp);
                        idx = LastResponse.IndexOf(commandEndingCharacters);
                        if (idx > 0)
                            LastResponse = LastResponse.Substring(0, idx);


                        //Send the closing command
                        using (MemoryStream command = new MemoryStream())
                        {
                            request = "d31d88JMP";
                            command.Write(Encoding.ASCII.GetBytes(request));
                            command.WriteByte(0x00);
                            command.Write(Encoding.ASCII.GetBytes(commandEndingCharacters));
                            buffer = command.ToArray();
                        }
                        stream.Write(buffer, 0, buffer.Length);
                        stream.Flush();
                        //Sometimes they send this one instead, but why??? How would I know???
                        using (MemoryStream command = new MemoryStream())
                        {
                            request = "d31d88DEF";
                            command.Write(Encoding.ASCII.GetBytes(request));
                            //command.WriteByte(0x00);
                            command.Write(Encoding.ASCII.GetBytes(commandEndingCharacters));
                            buffer = command.ToArray();
                        }
                        stream.Write(buffer, 0, buffer.Length);
                        stream.Flush();

                        //}


                    }


                }
            }
            catch (Exception ex)
            {

                throw;
            }

            return rtnVal;
        }

        private static byte[] ReadFileIntoMemory(FileStream fileStream, NetworkStream stream)
        {
            byte[] buffer;
            byte[] block = new byte[1440];
            string sendCommand = "d31d88JMP";
            buffer = new byte[0];
            int count = 0;
            int totalcount = 0;
            while (fileStream.Read(block, 0, block.Length) > 0)
            {
                using (MemoryStream command = new MemoryStream())
                {
                    command.Write(Encoding.ASCII.GetBytes(sendCommand));
                    command.Write(block);
                    command.Write(Encoding.ASCII.GetBytes("000" + commandEndingCharacters));
                    buffer = buffer.Concat(command.ToArray()).ToArray();
                }
                count++;
                if (count == 15)
                {
                    stream.Write(buffer, 0, buffer.Length);
                    stream.Flush();
                    buffer = new byte[0];
                    count = 0;
                    System.Threading.Thread.Sleep(50);//wait just a second to get stuff back
                    byte[] resp = new byte[2048];
                    int dataRead = 0;
                    while (stream.DataAvailable)
                    {
                        dataRead = stream.Read(resp, 0, resp.Length);
                        totalcount++;
                        Debug.WriteLine(totalcount + " " + Encoding.ASCII.GetString(resp));
                        Debug.WriteLine("");
                    }

                }

            }

            //send the last bytes
            if(count > 0)
            {
                stream.Write(buffer, 0, buffer.Length);
                stream.Flush();
            }

            return buffer;
        }
        #endregion
    }
}
