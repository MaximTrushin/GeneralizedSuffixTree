using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using SuffixTree;

namespace TestProject.Core
{
    public class IndexingHelper
    {
        private readonly string _path;
        private readonly HttpApplicationState _app;
        private bool _ready;
        private GeneralizedSuffixTree<WordLocation> _suffixTree;
        public const string AppPropertyName = "Indexer";

        public IndexingHelper(string path, HttpApplicationState app)
        {
            _path = path;
            _app = app;
        }

        public async void Index()
        {
            if (_ready) return;
            await Task.Run(() =>
            {
                _suffixTree = new GeneralizedSuffixTree<WordLocation>(3);
                foreach (var word in GetWordsFromFiles(_path))
                {
                    _suffixTree.AddWord(word.Item2.ToLower(), word.Item1);
                }
                _app[AppPropertyName] = this;
                _ready = true;
            });

        }

        public IEnumerable<WordLocation> FindWordsWith(string id)
        {
            if (!_ready || id == null) yield break;
            foreach (var location in _suffixTree.Retrieve(id))
            {
                yield return location;
            }
        }

        private static IEnumerable<Tuple<WordLocation, string>> GetWordsFromFiles(string path)
        {
            if (!Directory.Exists(path)) yield break;
            foreach (var file in Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories))
            {
                foreach (var w in GetWordsFromFile(file))
                {
                    yield return w;
                }
            }
        }

        private static IEnumerable<Tuple<WordLocation, string>> GetWordsFromFile(string file)
        {
            using (var stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var word = new StringBuilder();
                while (true)
                {
                    long position = stream.Position;
                    var data = stream.ReadByte();
                    {
                        if (data == -1) break;
                        var ch = (char)data;
                        if (char.IsLetter(ch))
                        {
                            word.Append(char.ToLowerInvariant(ch));
                        }
                        else
                        {
                            if (word.Length == 0) continue;
                            yield return new Tuple<WordLocation, string>(new WordLocation(position, file), word.ToString().ToLower());
                            word.Clear();
                        }
                    }
                }
            }
        }


    }
}