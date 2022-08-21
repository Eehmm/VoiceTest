// See https://aka.ms/new-console-template for more information
using AudioTest;

bool exit = false;
AudioWrapper audioTest = new AudioWrapper();

while (exit == false)
{
    Console.WriteLine("1. Play\n2. RecordTest\n3. Connect to voice (listen)\n4. Connect to voice (speak)");

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
                    audioTest.RecordTest();
                    break;
                }
            case ConsoleKey.D3:
                {
                    audioTest.ConnectToVoiceListen();
                    break;
                }
            case ConsoleKey.D4:
                {
                    audioTest.ConnectToVoiceSpeak();
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

