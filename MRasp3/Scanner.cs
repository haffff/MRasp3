using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MRasp3
{
    public class Scanner
    {
        public List<string> MusicFiles { get; private set; }

        public Scanner(string path)
        {
            MusicFiles = new List<string>();
            Console.WriteLine("Stating scanner");
            lookFor(path);
        }
        private void lookFor(string path)
        {
            Console.WriteLine("Working in -> " + path);
            var list = Directory.GetFiles(path).Where(x=>x.EndsWith(".mp3"));
            Console.WriteLine("Added -> " + String.Join("\n",list));
            MusicFiles.AddRange(list);
            Console.WriteLine(String.Join("\n", Directory.GetDirectories(path)));
            foreach (var i in Directory.GetDirectories(path))
            {
                lookFor(i);
            }
        }
    }
}
