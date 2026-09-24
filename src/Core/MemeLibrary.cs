// MemeLibrary — scans {game}/Mods/Memes/** for playable meme sounds.
// Formats: .wav + .mp3 (same decoder coverage as MusicPlayer; .ogg and
// anything else warns + skips). Creates the folder + README on first run.
using System;
using System.Collections.Generic;
using System.IO;
using MelonLoader;

namespace GregMod.MemeRoulette.Core
{
    internal static class MemeLibrary
    {
        private static readonly List<string> _files = new List<string>();
        private static string _lastPlayed = "";
        private static readonly Random _rng = new Random();

        private const string ReadmeText =
            "MemeRoulette — drop your meme sounds here (.wav / .mp3).\r\n" +
            "Subfolders are scanned too. .ogg and other formats are skipped.\r\n" +
            "The mod plays a random file after a random silence (see prefs).\r\n";

        internal static int Count { get { try { return _files.Count; } catch { return 0; } } }

        internal static string MemesDir()
        {
            try
            {
                string gameDir = null;
                try
                {
                    string dataPath = UnityEngine.Application.dataPath;
                    if (!string.IsNullOrEmpty(dataPath))
                        gameDir = Path.GetDirectoryName(dataPath);
                }
                catch { }
                if (string.IsNullOrEmpty(gameDir))
                {
                    try { gameDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location); } catch { }
                }
                if (string.IsNullOrEmpty(gameDir)) return "Memes";
                return Path.Combine(gameDir, "Mods", "Memes");
            }
            catch { return "Memes"; }
        }

        internal static int Rescan()
        {
            _files.Clear();
            try
            {
                string dir = MemesDir();
                try { Directory.CreateDirectory(dir); } catch { }
                try
                {
                    string readme = Path.Combine(dir, "README.txt");
                    if (!File.Exists(readme))
                    {
                        try { File.WriteAllText(readme, ReadmeText); } catch { }
                    }
                }
                catch { }
                string[] found = null;
                try { found = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories); } catch { }
                if (found == null) return 0;
                foreach (var f in found)
                {
                    try
                    {
                        string ext = Path.GetExtension(f).ToLowerInvariant();
                        if (ext == ".wav" || ext == ".wave" || ext == ".mp3")
                            _files.Add(f);
                        else if (ext == ".ogg" || ext == ".oga")
                            MelonLogger.Warning("[MemeRoulette] Skipped (ogg unsupported, use .wav/.mp3): " + Path.GetFileName(f));
                    }
                    catch { }
                }
                _files.Sort(StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                try { MelonLogger.Warning("[MemeRoulette] Rescan failed: " + ex.GetBaseException().Message); } catch { }
            }
            return Count;
        }

        /// <summary>Random file, avoiding an immediate repeat when enabled.</summary>
        internal static string PickRandom(bool noRepeat)
        {
            try
            {
                if (_files.Count == 0) return "";
                if (_files.Count == 1 || !noRepeat)
                {
                    string only = _files[_rng.Next(_files.Count)];
                    _lastPlayed = only;
                    return only;
                }
                string pick = "";
                for (int i = 0; i < 5; i++)
                {
                    pick = _files[_rng.Next(_files.Count)];
                    if (!string.Equals(pick, _lastPlayed, StringComparison.OrdinalIgnoreCase)) break;
                }
                _lastPlayed = pick;
                return pick;
            }
            catch { return ""; }
        }
    }
}
