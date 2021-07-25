using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace ChaserASEQuery
{
    class Program
    {
        private static string ip;
        private static int port;

        private IPEndPoint EndPoint = new IPEndPoint(IPAddress.Any, 0);
        private Byte[] message = Encoding.ASCII.GetBytes("s");

        #region server keys
        private string _serverName;
        private string _map;
        private int _online;
        private int _max;
        private string _serverVersion;
        private string _serverType;
        #endregion

        #region player keys
        private string _pName;
        private int _pScore;
        private string _pSkin;
        private string _pPing;
        private string _pTeam;
        #endregion


        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Enter server IP to Query: ");
                ip = Console.ReadLine();
                Console.WriteLine("Enter server port: ");
                port = Convert.ToInt32(Console.ReadLine());

                Program newq = new Program();
                newq.qServer(ip, port);
            }
        }

        private void qServer(string ip, int port)
        {
            UdpClient udp = new UdpClient(15000);
            udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 5000);

            try
            {
                udp.Connect(ip, port + 123);
                udp.Send(message, message.Length);

                Byte[] receiveBytes = udp.Receive(ref EndPoint);
                udp.Close();

                string response = Encoding.ASCII.GetString(receiveBytes);

                Parse(AddDeliminator(response));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                udp.Close();
            }
        }

        private void Parse(string response)
        {
            _serverName = response.Split('\\')[3];
            _map = response.Split('\\')[5];
            _online = Convert.ToInt32(response.Split('\\')[8]);
            _max = Convert.ToInt32(response.Split('\\')[9]);
            _serverVersion = response.Split('\\')[1];
            _serverType = response.Split('\\')[4];

            Console.WriteLine("###############################-Query Start-###############################");
            Console.WriteLine($"Sever name: { _serverName }");
            Console.WriteLine($"Players online: { _online }/{ _max }");
            Console.WriteLine($"Map: { _map }");
            Console.WriteLine($"Gametype: { _serverType }");
            Console.WriteLine($"Version: { _serverVersion }" + Environment.NewLine);

            if(_online > 0) { ParsePlayers(response); }
            else { Console.WriteLine("###############################-Query End-###############################"); }
        }

        static string AddDeliminator(string response)
        {
            try
            {
                return Regex.Replace(response, @"[^\w\.@*&^%$#!()= -]", "\\",
                                     RegexOptions.None, TimeSpan.FromSeconds(1.5));
            }
            catch (RegexMatchTimeoutException)
            {
                return String.Empty;
            }
        }

        private void ParsePlayers(string message)
        {
            if (Convert.ToInt32(_online) > 0)
            {
                int i = 12;
                int p = 0;

                for (int o = 0; o < _online; o++)
                {
                    _pName = message.Split('\\')[i];
                    _pScore = Convert.ToInt32(message.Split('\\')[i + 3]);
                    _pTeam = message.Split('\\')[i + 1];
                    _pSkin = message.Split('\\')[i + 2];
                    _pPing = message.Split('\\')[i + 4];

                    Console.WriteLine($"Player{ ++p }: {Environment.NewLine} Name: { _pName } {Environment.NewLine} Score: { _pScore } {Environment.NewLine} Team: { _pTeam } {Environment.NewLine} Skin: { _pSkin } {Environment.NewLine} Ping: { _pPing }");
                    Console.WriteLine("###############################-Query End-###############################");

                    i = i + 6;
                }
            }
            else
            {
                Console.WriteLine("No players currently online..");
            }
        }
    }
}
