// See https://aka.ms/new-console-template for more information
using AudioTest;

bool exit = false;
AudioWrapper audioTest = new AudioWrapper();

while (exit == false)
{
    Console.WriteLine("1. Play\n2. PlayBackTest\n3. Connect to voice (listen only)\n4. Connect to voice (speak only @ 127.0.0.1:7001)\n5. Connect to voice");

    while (exit == false)
    {
    var key = Console.ReadKey();
        switch (key.Key)
        {
            case ConsoleKey.D1:
                {
                    audioTest.PlayMP3("themoves.mp3");
                    break;
                }
            case ConsoleKey.D2:
                {
                    audioTest.PlayBackTest();
                    break;
                }
            case ConsoleKey.D3:
                {
                    audioTest.ConnectToVoiceListen();
                    break;
                }
            case ConsoleKey.D4:
                {
                    audioTest.ConnectToVoiceSpeak("127.0.0.1", 7001);
                    break;
                }
            case ConsoleKey.D5:
                {
                    ConnectToVoicePressed();

                    break;
                }
            case ConsoleKey.Escape:
                {
                    exit = true;
                    break;
                }
        }
    }

    audioTest.CleanUp();

}

void ConnectToVoicePressed()
{
    string listenPort;
    string remoteIp;
    string remotePort;
    Console.Clear();
    Console.WriteLine("Enter local listen port:");
    listenPort = Console.ReadLine();
    Console.WriteLine("Enter remote IP:");
    remoteIp = Console.ReadLine();
    Console.WriteLine("Enter remote Port:");
    remotePort = Console.ReadLine();

    Console.WriteLine("Connecting to voice..");


    audioTest.ConnectToVoice(remoteIp, remotePort, listenPort);
}

