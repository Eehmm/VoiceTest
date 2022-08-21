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

        private WaveFileWriter writer;
        private BufferedWaveProvider bufferedWaveProvider;
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

        public void RecordTest()
        {

            waveOut = new WaveOutEvent();
            waveProvider = new BufferedWaveProvider(codec);
            waveOut.Init(waveProvider);
            waveOut.Play();

            inputDevice.StopRecording();
            inputDevice.StartRecording();
            inputDevice.DataAvailable += OnDataAvailable;

        }

        public void ConnectToVoiceListen()
        {
            if (network == null)
            {
                waveOut = new WaveOutEvent();
                waveProvider = new BufferedWaveProvider(codec);
                waveProvider.BufferDuration = new TimeSpan(0, 0, 1);
                waveOut.Init(waveProvider);
                waveOut.Play();
                network = new NetworkWrapper();
                ReceiveUdp();
            }
        }

        public void ConnectToVoiceSpeak()
        {
            if (network == null)
            {
                network = new NetworkWrapper();
                network.ConnectUdp("127.0.0.1", 7001);
                inputDevice.StopRecording();
                inputDevice.StartRecording();
                inputDevice.DataAvailable += _sendUdp;
            }
        }

        void _sendUdp(object sender, WaveInEventArgs e)
        {
            network.udpClient.Send(e.Buffer);
        }


        public void ReceiveUdp()
        {
            Task t = new Task(() => _receiveUdp());
            t.Start();

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
            _receivingUdp = false;
            waveOut?.Dispose();
            inputDevice?.Dispose();
        }

    }
}
