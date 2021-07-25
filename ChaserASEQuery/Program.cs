using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace ChaserASEQuery
{
    //This is a super simplified version of how to query a Chaser sever with all seeing eye protocol
    //This method works at anytime, it isn't as clean as gamespy (which only works when initiated)

    internal class Program
    {
        private static string ip; //Storing IP from console
        private static int port; //Storing Port from console

        private IPEndPoint EndPoint = new IPEndPoint(IPAddress.Any, 0); //Just the endpoint being used for udp response
        private readonly byte[] message = Encoding.ASCII.GetBytes("s"); //Message we send to server, all seeing eye is a simple 's' to query results

        #region server keys
        private string _serverName;
        private string _map;
        private int _online;
        private int _max;
        private string _serverVersion;
        private string _serverType;
        #endregion  //All the server keys

        #region player keys
        private string _pName;
        private int _pScore;
        private string _pSkin;
        private string _pPing;
        private string _pTeam;
        #endregion //All the player keys


        private static void Main(string[] args)
        {
            while (true) //Keeping the app running in a loop
            {
                Console.WriteLine("Press any key to continue or 'q' to stop the application..."); //q exits application, else continue
                for (; ; )
                {
                    string line = Console.ReadLine();
                    if (string.IsNullOrEmpty(line))
                        break;
                    if (line == "q")
                    {
                        Environment.Exit(0);
                    }
                }  

                Console.WriteLine("Enter server IP to Query: "); //Writing to console requesting server IP
                ip = Console.ReadLine(); //Reading response and saving to string 'ip'
                Console.WriteLine("Enter server port: "); //Writing to console requesting server port
                port = Convert.ToInt32(Console.ReadLine()); //Reading response and saving to string 'port'

                Program newq = new Program(); //Initiating new instance of the program
                newq.qServer(ip, port); //Sending ip and port to method 'qServer'          
            }
        }

        private void qServer(string ip, int port)
        {
            UdpClient udp = new UdpClient(15000); //Crearting new udp client under port 1500, port is irrelevent
            udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 5000); //Setting socket options, time out after 5 seconds

            try
            {
                udp.Connect(ip, port + 123); //Connecting to server (all seeing eye queries under game port + 123, ex. port: 3004 -- all seeing eye query port is 3127)
                udp.Send(message, message.Length); //Sending out message 's' as defined above

                byte[] receivedResponse = udp.Receive(ref EndPoint); //Receiving response
                udp.Close(); //Closing connection to server

                string response = Encoding.ASCII.GetString(receivedResponse); //Converting response to string
                //Typical response example with no players "EYE1 08chaserd 053004 11chasergaming.com 03ST rFinal Strike 061.490 020 020 0320 01"
                //Response with 1 player "EYE1 08chaserd 053004 11chasergaming.com 03ST rFinal Strike 061.490 020 021 0320 01 1f 07jessie 020 06Gomez 020 0357"

                Parse(AddDeliminator(response)); //Sending to Parse method after cleaning string up in 'AddDeliminator' method, this is completely optional, I do it for ease of parsing response string
            }
            catch (Exception e) //Catching exception if connection failed, and writing to console
            {
                Console.WriteLine(e);
                udp.Close();
            }
        }

        private void Parse(string response)
        {
            _serverName = response.Split('\\')[3]; //This index will always be the server name
            _map = response.Split('\\')[5]; //This index will always be the server map
            _online = Convert.ToInt32(response.Split('\\')[8]); //This index will always be the server players online
            _max = Convert.ToInt32(response.Split('\\')[9]); //This index will always be the server max players
            _serverVersion = response.Split('\\')[1]; //This index will always be the server server
            _serverType = response.Split('\\')[4]; //This index will always be the server type (ex. ST, DM, CTF)

            //Lets print the results to the console
            Console.WriteLine("###############################-Query Start-###############################");
            Console.WriteLine($"Sever name: { _serverName }");
            Console.WriteLine($"Players online: { _online }/{ _max }");
            Console.WriteLine($"Map: { _map }");
            Console.WriteLine($"Gametype: { _serverType }");
            Console.WriteLine($"Version: { _serverVersion }");

            if (_online > 0) { ParsePlayers(response); } //Check if there are players online, if greater than 0, go to ParsePlayers method
            else { Console.WriteLine("###############################-Query End-###############################"); } //Else, end query
        }

        private static string AddDeliminator(string response) //This is optional, I do this because all seeing eye responds with blank spaces and char counts, so I clean the reponse and add a '\\' deliminator similar to gamespy, C# we can easily just split this string at the deliminator
        {
            try
            {
                return Regex.Replace(response, @"[^\w\.@*&^%$#!()= -]", "\\",      //Replacing blank spaces with '\\'
                                     RegexOptions.None, TimeSpan.FromSeconds(1.5));
            }
            catch (RegexMatchTimeoutException)
            {
                return string.Empty;
            }
        }

        private void ParsePlayers(string message)
        {
            int i = 12; //This is the default index of the first players name
            int p = 0; //Keep track of player count

            for (int o = 0; o < _online; o++) //Looping through players
            {
                _pName = message.Split('\\')[i]; //This is out default start index of 12
                _pScore = Convert.ToInt32(message.Split('\\')[i + 3]); //Index for current player score
                _pTeam = message.Split('\\')[i + 1]; //Index for current player team (0 = deathmatch or law breaker, 1 = government forces)
                _pSkin = message.Split('\\')[i + 2]; //Index for current player skin
                _pPing = message.Split('\\')[i + 4]; //Index for current player ping
                //Print out all of this information on new line
                Console.WriteLine($"Player{ ++p }: { Environment.NewLine } Name: { _pName } { Environment.NewLine } Score: { _pScore } { Environment.NewLine}  Team: { _pTeam } { Environment.NewLine } Skin: { _pSkin } { Environment.NewLine } Ping: { _pPing }");

                i = i + 6; //After each player, add 6 to the default index, this will start for the next player
            }
            Console.WriteLine("###############################-Query End-###############################"); //End player query
        }
    }
}
