// MemePlayer — one-shot meme playback.
// With gregCore: GregAudioService (cache + optimized path, same as
// MusicPlayer). Without: local AudioSource (own GameObject, sync file read +
// managed decode + AudioClip, fire-and-forget). Dispatcher touches ONLY
// mod-local types (JIT-safe); core calls live in CorePlay (own method).
using System;
using System.IO;
using MelonLoader;
using UnityEngine;

namespace GregMod.MemeRoulette.Core
{
    internal static class MemePlayer
    {
        private static GameObject _obj;
        private static AudioSource _src;
        private static AudioClip _clip;
        internal static int MaxFileMb = 25;

        internal static bool IsPlaying
        {
            get { try { return _src != null && _src.isPlaying; } catch { return false; } }
        }

        internal static void Play(string filePath, string title, float volume)
        {
            if (string.IsNullOrEmpty(filePath)) return;
            if (string.IsNullOrEmpty(title))
            {
                try { title = Path.GetFileNameWithoutExtension(filePath); } catch { title = "meme"; }
            }
            try
            {
                if (GregHost.HasCore) { CorePlay(filePath, title, volume); return; }
                LocalPlay(filePath, title, volume);
            }
            catch (Exception ex)
            {
                MelonLogger.Error("[MemeRoulette] Play failed: " + ex.GetBaseException().Message);
            }
        }

        internal static void Stop()
        {
            try
            {
                if (GregHost.HasCore) { try { CoreStop(); } catch { } }
            }
            catch { }
            try
            {
                if (_src != null)
                {
                    try { _src.Stop(); } catch { }
                    try { _src.clip = null; } catch { }
                }
                if (_clip != null)
                {
                    try { UnityEngine.Object.Destroy(_clip); } catch { }
                    _clip = null;
                }
            }
            catch { }
        }

        // ONLY with gregCore (own method = JIT split).
        private static void CorePlay(string filePath, string title, float volume)
        {
            gregCore.PublicApi.Audio.GregAudioService.PlayFile(filePath, title, volume);
        }

        private static void CoreStop()
        {
            gregCore.PublicApi.Audio.GregAudioService.Stop();
        }

        private static AudioSource Source()
        {
            if (_src != null) return _src;
            try
            {
                _obj = new GameObject("MemeRoulette_Source");
                UnityEngine.Object.DontDestroyOnLoad(_obj);
                _src = _obj.AddComponent<AudioSource>();
                _src.playOnAwake = false;
                _src.loop = false;
            }
            catch { _src = null; }
            return _src;
        }

        private static void LocalPlay(string filePath, string title, float volume)
        {
            var src = Source();
            if (src == null) return;
            byte[] data = null;
            try
            {
                var fi = new FileInfo(filePath);
                if (!fi.Exists || fi.Length <= 0 || fi.Length > (long)Math.Max(1, MaxFileMb) * 1024L * 1024L)
                {
                    MelonLogger.Warning("[MemeRoulette] Skipped (missing/empty/too big): " + title);
                    return;
                }
                data = File.ReadAllBytes(filePath);
            }
            catch (Exception ex)
            {
                MelonLogger.Warning("[MemeRoulette] Read failed (" + title + "): " + ex.GetBaseException().Message);
                return;
            }

            float[] samples = null;
            int channels = 0, frequency = 0;
            try
            {
                if (!MemeDecoder.TryDecode(filePath, data, out samples, out channels, out frequency)
                    || samples == null || samples.Length == 0 || channels <= 0 || frequency <= 0)
                {
                    MelonLogger.Warning("[MemeRoulette] Decode failed (" + title + ")");
                    return;
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error("[MemeRoulette] Decode: " + ex.GetBaseException().Message);
                return;
            }

            AudioClip clip = null;
            try
            {
                clip = AudioClip.Create(title, samples.Length / channels, channels, frequency, false);
                if (clip == null || !clip.SetData(samples, 0))
                {
                    MelonLogger.Warning("[MemeRoulette] Clip creation failed (" + title + ")");
                    try { if (clip != null) UnityEngine.Object.Destroy(clip); } catch { }
                    return;
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error("[MemeRoulette] Clip: " + ex.GetBaseException().Message);
                return;
            }

            try
            {
                if (_clip != null)
                {
                    try { UnityEngine.Object.Destroy(_clip); } catch { }
                }
                _clip = clip;
                try { src.Stop(); } catch { }
                src.clip = clip;
                src.volume = Math.Max(0f, Math.Min(1f, volume));
                src.loop = false;
                src.Play();
                MelonLogger.Msg("[MemeRoulette] Playing: '" + title + "' (" + clip.length.ToString("F1") + "s)");
            }
            catch (Exception ex)
            {
                MelonLogger.Error("[MemeRoulette] Play: " + ex.GetBaseException().Message);
            }
        }
    }
}
