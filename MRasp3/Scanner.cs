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
            lookFor(path);
        }
        private void lookFor(string path)
        {
            var list = Directory.GetFiles(path).Where(x=>x.EndsWith(".mp3"));
            MusicFiles.AddRange(list);
            foreach (var i in Directory.GetDirectories(path))
            {
                lookFor(i);
            }
        }
    }
}
