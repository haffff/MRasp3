using System;
using System.IO;
using System.Diagnostics;
using System.Device.Gpio;
using System.Linq;

namespace MRasp3
{
    class Program
    {
        static Stopwatch stopwatch;
        static FileSystemWatcher watcher = new FileSystemWatcher(File.Exists("path") ? File.ReadAllText("path") : "/media");
        static string[] songs;
        static int index = -1;
        static Process process = new System.Diagnostics.Process();
        private static bool programIsWorking = true;

        static void Main(string[] args)
        {
            watcher.Created += HandlePendrive;
            watcher.Deleted += HandlePendrive;
            watcher.EnableRaisingEvents = true;
            GpioController controller = new GpioController();
            controller.RegisterCallbackForPinValueChangedEvent(3, PinEventTypes.Falling, (a,o) => Stopwatch.StartNew());
            controller.RegisterCallbackForPinValueChangedEvent(3, PinEventTypes.Rising, TurnOff);
            controller.RegisterCallbackForPinValueChangedEvent(5, PinEventTypes.Rising, PlayStop);
            controller.RegisterCallbackForPinValueChangedEvent(7, PinEventTypes.Rising, Next);
            controller.RegisterCallbackForPinValueChangedEvent(11, PinEventTypes.Rising, Prev);
            process.Exited += GetNextSong;
            while(programIsWorking)
            {

            }
            return;
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


        private static void PlaySong(string song)
        {
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"ffplay -vn -nodisp -autoexit\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            process.Start();
        }


        private static void Next(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            if (index < 0)
                return;
            process.Exited -= GetNextSong;
            process.StandardInput.Write('q');
            while (!process.HasExited) { }
            GetNextSong();
            process.Exited += GetNextSong;
        }

        private static void Prev(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            if (index < 0)
                return;
            process.Exited -= GetPrevSong;
            process.StandardInput.Write('q');
            while (!process.HasExited) { }
            GetPrevSong();
            process.Exited += GetPrevSong;
        }

        private static void PlayStop(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            if (index < 0)
                return;
            process.StandardInput.Write('p');
        }

        private static void TurnOff(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            if(stopwatch.Elapsed.TotalSeconds <= 3)
            {
                stopwatch.Stop();
                stopwatch = null;
                return;
            }
            var p = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"shutdown\"",
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
            Scanner scanner = new Scanner(e.FullPath);
            songs = scanner.MusicFiles.ToArray();
            if (songs.Length > 0)
            {
                index = 0;
                PlaySong(songs[index]);
            }
            else
                index = -1;
            
        }
    }
}
