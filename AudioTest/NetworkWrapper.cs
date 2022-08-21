using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AudioTest
{
    class NetworkWrapper
    {
        string ip = "127.0.0.1";
        int tcpPort = 6000;
        int udpPort = 6001;

        public int UdpListenPort = 7001;

        public TcpClient tcpClient;
        public UdpClient udpClient;
        IPEndPoint udpEndPoint;

        bool connected = false;

        public NetworkWrapper()
        {
            //tcpClient = new TcpClient(ip, tcpPort);
            //udpClient = new UdpClient(ip, udpPort);
            udpEndPoint = new IPEndPoint(IPAddress.Any, UdpListenPort);
        }

        public void ConnectUdp(string ip, int port)
        {
            udpClient = new UdpClient(ip, port);
        }

        public void RecieveUdp(AsyncCallback callback)
        {
            Task t = new Task(() => _receiveUdp(callback));
            t.Start();

        }

        public void BeginRecieveUdp(AsyncCallback callback)
        {
            Task t = new Task(() => _beginReceiveUdp(callback));
            t.Start();

        }

        void _receiveUdp(AsyncCallback callback)
        {
            var udpClient = new UdpClient(UdpListenPort);
            var endPoint = new IPEndPoint(IPAddress.Any, 0);
            try
            {
                // Blocks until a message returns on this socket from a remote host.
                byte[] receiveBytes = udpClient.Receive(ref endPoint);


            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        void _beginReceiveUdp(AsyncCallback callback)
        {
            IPEndPoint e = udpEndPoint;
            UdpClient udpClient = new UdpClient(e);
            UdpState s = new UdpState();
            s.e = e;
            s.u = udpClient;
            udpClient.BeginReceive(callback, s);
        }

        public void ReceiveCallback(IAsyncResult ar)
        {
            UdpClient client = ((UdpState)(ar.AsyncState)).u;
            IPEndPoint e = ((UdpState)(ar.AsyncState)).e;

            byte[] receiveBytes = client.EndReceive(ar, ref e);

        }

        public void CleanUp()
        {
            tcpClient?.Close();
            udpClient?.Close();
        }

        private static byte[] AddSizeHeaderToPackage(byte[] _package)
        {
            //Create a uint containing the length of the package, and encode to byte array
            int pckLen = _package.Length;
            byte[] packageHeader = BitConverter.GetBytes(pckLen);
            byte[] totalPackage = new byte[packageHeader.Length + _package.Length];
            //Merge byte arrays
            System.Buffer.BlockCopy(packageHeader, 0, totalPackage, 0, packageHeader.Length);
            System.Buffer.BlockCopy(_package, 0, totalPackage, packageHeader.Length, _package.Length);

            return totalPackage;
        }
        public static bool Connected(TcpClient tcpClient)
        {
            try
            {
                if (tcpClient.Client != null && tcpClient.Client.Connected)
                {
                    if (tcpClient.Client.Poll(0, SelectMode.SelectRead))
                    {
                        byte[] buff = new byte[1];

                        if (tcpClient.Client.Receive(buff, SocketFlags.Peek) == 0)
                        {
                            return false;
                        }

                        return true;
                    }

                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }

    public struct UdpState
    {
        public UdpClient u;
        public IPEndPoint e;
    }
}
