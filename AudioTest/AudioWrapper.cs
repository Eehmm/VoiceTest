using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AudioTest
{
    class AudioWrapper
    {

        private IWavePlayer waveOut;
        private WaveInEvent inputDevice;
        private BufferedWaveProvider waveProvider;
        private WaveFormat codec;

        private NetworkWrapper network;

        private bool _receivingUdp = false;

        public AudioWrapper()
        {
            //waveOut = new WaveOutEvent();
            inputDevice = new WaveInEvent();
            codec = inputDevice.WaveFormat;
            waveProvider = new BufferedWaveProvider(codec);
            waveOut = new WaveOutEvent();
            network = new NetworkWrapper();
        }

        public void PlayMP3(string path)
        {
            using (var audioFile = new AudioFileReader(path))
            using (var outputDevice = new WaveOutEvent())
            {
                outputDevice.Init(audioFile);
                outputDevice.Play();
                while (outputDevice.PlaybackState == PlaybackState.Playing)
                {
                    Thread.Sleep(1000);
                }
            }
        }

        public void PlayBackTest()
        {



            waveOut.Init(waveProvider);
            waveOut.Play();

            inputDevice.StopRecording();
            inputDevice.StartRecording();
            inputDevice.DataAvailable += OnDataAvailable;

        }

        public void ConnectToVoice(string remoteIp, string remotePort, string listenPort)
        {


            network.UdpListenPort = Int32.Parse(listenPort);
            network.TcpListenPort = Int32.Parse(listenPort);
            var port = Int32.Parse(remotePort);
            StartTcpListener(network.TcpListenPort);
            Console.WriteLine("Connecting to tcp");
            while (network.tcpClient == null)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(16.66667));
                try
                {
                    network.tcpClient = new TcpClient(remoteIp, port);
                }
                catch
                {
                    Console.WriteLine("Retrying tcp connection..");
                }
            }

            Console.WriteLine("Tcp connected!");

            ConnectToVoiceListen();
            ConnectToVoiceSpeak(remoteIp, port);

        }



        public void ConnectToVoiceListen()
        {

            waveProvider.BufferDuration = new TimeSpan(0, 0, 1);
            waveOut.Init(waveProvider);
            waveOut.Play();
            ReceiveUdp();

        }

        public void ConnectToVoiceSpeak(string remoteIp, int port)
        {

            network.ConnectUdp(remoteIp, port);
            inputDevice.StopRecording();
            inputDevice.StartRecording();
            inputDevice.DataAvailable += _sendUdp;

        }

        void _sendUdp(object sender, WaveInEventArgs e)
        {
            try
            {
                network.udpClient.Send(e.Buffer);
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
            }
        }

        public void StartTcpListener(int port)
        {
            Task t = new Task(() =>
            {
            network.tcpListener = new TcpListener(port);
            network.tcpListener.Start();
                network.tcpClient = network.tcpListener.AcceptTcpClient();
            });
            t.Start();
        }

        public void ReceiveTcp(TcpClient client)
        {
            Task t = new Task(() => _receiveTcp(client));
            t.Start();

        }


        public void ReceiveUdp()
        {
            Task t = new Task(() => _receiveUdp());
            t.Start();

        }

        private void _receiveTcp(TcpClient client)
        {
            while (NetworkWrapper.Connected(client))
            {

            }
        }

        private void _receiveUdp()
        {
            var udpClient = new UdpClient(network.UdpListenPort);
            var endPoint = new IPEndPoint(IPAddress.Any, 0);
            _receivingUdp = true;
            while (_receivingUdp)
            {
                try
                {
                    // Blocks until a message returns on this socket from a remote host.
                    byte[] receiveBytes = udpClient.Receive(ref endPoint);
                    waveProvider.AddSamples(receiveBytes, 0, receiveBytes.Length);

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }

            udpClient.Close();

        }


        public void UdpReceiveCallback(IAsyncResult ar)
        {
            UdpClient client = ((UdpState)(ar.AsyncState)).u;
            IPEndPoint e = ((UdpState)(ar.AsyncState)).e;

            byte[] receiveBytes = client.EndReceive(ar, ref e);
            waveProvider.AddSamples(receiveBytes, 0, receiveBytes.Length);
            network.BeginRecieveUdp(UdpReceiveCallback);
        }

        void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            waveProvider.AddSamples(e.Buffer, 0, e.Buffer.Length);
        }

        public void CleanUp()
        {
            network.CleanUp();
            _receivingUdp = false;
            waveOut?.Dispose();
            inputDevice?.Dispose();
        }

    }
}
