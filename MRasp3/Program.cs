using System;
using System.IO;
using System.Diagnostics;
using System.Device.Gpio;
using System.Linq;
using System.Threading;

namespace MRasp3
{
    class Program
    {
        static Stopwatch stopwatch;
        static Stopwatch songStopwatch;
        static string path = File.Exists("path") ? File.ReadAllText("path") : "/media";
        static FileSystemWatcher watcher = new FileSystemWatcher(path);
        static string[] songs;
        static int index = -1;
        static Process process = new System.Diagnostics.Process();
        private static bool programIsWorking = true;

        static void Main(string[] args)
        {
            Console.WriteLine("Working...");
            watcher.Created += HandlePendrive;
            watcher.Deleted += ClearApp;
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;
            GpioController controller = new GpioController();
            controller.OpenPin(3, PinMode.Input);
            controller.OpenPin(5, PinMode.Input);
            controller.OpenPin(7, PinMode.Input);
            controller.OpenPin(11, PinMode.Input);
            controller.RegisterCallbackForPinValueChangedEvent(3, PinEventTypes.Falling, (a,o) => { 
                stopwatch = Stopwatch.StartNew();
            });
            controller.RegisterCallbackForPinValueChangedEvent(3, PinEventTypes.Rising, TurnOff);
            controller.RegisterCallbackForPinValueChangedEvent(5, PinEventTypes.Rising, PlayStop);
            controller.RegisterCallbackForPinValueChangedEvent(7, PinEventTypes.Rising, Next);
            controller.RegisterCallbackForPinValueChangedEvent(11, PinEventTypes.Rising, Prev);
            process.Exited += GetNextSong;
            Console.WriteLine("Subbed, starging loop...");
            HandlePendrive(null,null);
            while(programIsWorking) {}
            Console.WriteLine("Exiting...");
            controller.ClosePin(3);
            controller.ClosePin(5);
            controller.ClosePin(7);
            controller.ClosePin(11);
            return;
        }

        private static void ClearApp(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("Clearing");
            process.Kill();
            index = -1;
        }

        private static void GetNextSong(object sender = null, EventArgs e = null)
        {
            if (index < 0)
                return;
            if (++index >= songs.Length)
                index = 0;
            PlaySong(songs[index]);
        }

        private static void GetPrevSong(object sender = null, EventArgs e = null)
        {
            if (index < 0)
                return;
            if (--index < 0)
                index = songs.Length - 1;
            PlaySong(songs[index]);
        }


        private static void PlaySong(string song,int from = 0)
        {
            if(songStopwatch == null)
            {
                songStopwatch = Stopwatch.StartNew();
            }
            if(from == 0)
                songStopwatch.Restart();
            Console.WriteLine($"Playing song... {song}");
            process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"ffplay '{song}' -vn -autoexit -ss {from}\"", //-nodisp
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            process.Start();
        }


        private static void Next(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            Console.WriteLine($"checking index...");
            if (index < 0)
                return;
            Console.WriteLine($"Founded, Next()");
            process.Kill();
            Thread.Sleep(1000);
            GetNextSong();
        }

        private static void Prev(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            Console.WriteLine($"checking index...");
            if (index < 0)
                return;
            Console.WriteLine($"Founded, Prev()");
            process.Kill();
            Thread.Sleep(1000);
            GetPrevSong();
        }

        private static void PlayStop(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            Console.WriteLine($"checking index...");
            if (index < 0)
                return;
            if (songStopwatch != null && !songStopwatch.IsRunning)
            {
                PlaySong(songs[index], Convert.ToInt32(songStopwatch.Elapsed.TotalSeconds));
                songStopwatch.Start();
            }
            if (songStopwatch != null && songStopwatch.IsRunning)
            {
                songStopwatch.Stop();
                process.Kill();
            }
            Console.WriteLine($"Founded, Pause()");
        }

        private static void TurnOff(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            Console.WriteLine($"TimeEnd... {stopwatch.Elapsed.TotalSeconds}");
            if (stopwatch.Elapsed.TotalSeconds <= 3)
            {
                stopwatch.Stop();
                return;
            }
            Console.WriteLine("Zzzz...");
            Thread.Sleep(1000);
            process.Kill();
            var p = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"shutdown -f\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            p.Start();
            programIsWorking = false;
        }

        private static void HandlePendrive(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("Zzzz...");
            Thread.Sleep(2000);
            Console.WriteLine("Filewatcher starts working...");

            Scanner scanner = new Scanner(path);
            songs = scanner.MusicFiles.ToArray();
            Console.WriteLine($"Found {songs}");
            if (songs.Length > 0)
            {
                index = 0;
                PlaySong(songs[index]);
            }
            else
            {
                Console.WriteLine($"Not found, -1");
                index = -1;
            }
        }
    }
}
