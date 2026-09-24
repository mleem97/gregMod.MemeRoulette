// gregMod.MemeRoulette — Random Meme Sound Roulette.
// Pure silence, interrupted at random times by meme sounds from
// {game}/Mods/Memes/* (.wav/.mp3, recursive). Standalone-safe (own decode +
// playback, no hard gregCore dependency): GregHost probe + JIT-split
// MemeBridge for F1-hub (opener = instant meme), key HUD and toasts.
using System;
using System.IO;
using MelonLoader;
using UnityEngine;
using UnityEngine.InputSystem;
using GregMod.MemeRoulette.Core;
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("ModCoverage.Tests")]

[assembly: MelonInfo(typeof(GregMod.MemeRoulette.MemeRouletteMod), "gregMod.MemeRoulette", "1.0.0", "TeamGreg Modding")]
[assembly: MelonGame("Waseku", "Data Center")]

namespace GregMod.MemeRoulette
{
    public sealed class MemeRouletteMod : MelonMod
    {
        internal static MemeRouletteMod Instance { get; private set; }

        private static readonly MemeSettings Settings = new MemeSettings();
        private static Key _toggleKey = Key.F11;
        private static MelonPreferences_Entry<bool> _enabledEntry;

        private static readonly System.Random _rng = new System.Random();
        private static float _nextMemeAt;
        private static float _nextRescanAt;
        private static bool _timerArmed;

        public override void OnInitializeMelon()
        {
            try
            {
                Instance = this;
                var cat = MelonPreferences.CreateCategory("gregMod.MemeRoulette", "MemeRoulette");

                _enabledEntry = cat.CreateEntry("Enabled", true, "Roulette enabled",
                    "Master switch for random meme sounds.");
                var keyEntry = cat.CreateEntry("ToggleKey", "F11", "Toggle key",
                    "Enables/disables the roulette at runtime.");
                cat.CreateEntry("MinSilenceSec", 60f, "Min silence (s)",
                    "Shortest silence between memes.");
                cat.CreateEntry("MaxSilenceSec", 300f, "Max silence (s)",
                    "Longest silence between memes.");
                cat.CreateEntry("Volume", 0.8f, "Meme volume", "0.0 - 1.0.");
                cat.CreateEntry("NoRepeat", true, "No immediate repeats",
                    "Never plays the same file twice in a row.");
                cat.CreateEntry("ToastOnMeme", false, "Toast on meme (gregCore)",
                    "Shows a notification with the meme name.");
                cat.CreateEntry("MaxFileMb", 25, "Max file size (MB)",
                    "Files larger than this are skipped.");
                cat.SaveToFile(false);

                ReadPrefs(cat, keyEntry);
                MemePlayer.MaxFileMb = Settings.MaxFileMb;

                int count = MemeLibrary.Rescan();
                ArmTimer(true);

                if (GregHost.HasCore)
                {
                    try { RegisterCoreExtras(); } catch { }
                }

                LoggerInstance.Msg("[MemeRoulette] Loaded. " + count + " meme(s) in Mods/Memes. Roulette "
                    + (Settings.Enabled ? "ON" : "OFF") + " (" + _toggleKey + " = toggle, F1 hub = instant meme).");
                if (count == 0)
                    LoggerInstance.Warning("[MemeRoulette] No .wav/.mp3 in Mods/Memes — drop files there, they are picked up automatically.");
            }
            catch (Exception ex)
            {
                LoggerInstance.Error("[MemeRoulette] OnInitializeMelon failed: " + ex.GetBaseException().Message);
            }
        }

        private static void ReadPrefs(MelonPreferences_Category cat, MelonPreferences_Entry<string> keyEntry)
        {
            try
            {
                Settings.Enabled = _enabledEntry.Value;
                if (Enum.TryParse<Key>(keyEntry.Value, true, out var k) && k != Key.None)
                {
                    _toggleKey = k;
                    Settings.ToggleKeyText = k.ToString();
                }
                else
                    MelonLogger.Warning("[MemeRoulette] Unknown ToggleKey '" + keyEntry.Value + "', defaulting to F11.");
                Settings.MinSilenceSec = Math.Max(5f, GetFloat(cat, "MinSilenceSec", 60f));
                float max = GetFloat(cat, "MaxSilenceSec", 300f);
                Settings.MaxSilenceSec = Math.Max(Settings.MinSilenceSec, max);
                Settings.Volume = Math.Max(0f, Math.Min(1f, GetFloat(cat, "Volume", 0.8f)));
                Settings.NoRepeat = GetBool(cat, "NoRepeat", true);
                Settings.ToastOnMeme = GetBool(cat, "ToastOnMeme", false);
                Settings.MaxFileMb = Math.Max(1, GetInt(cat, "MaxFileMb", 25));
            }
            catch { /* defaults stand */ }
        }

        private static int GetInt(MelonPreferences_Category cat, string key, int fallback)
        {
            try { return cat.GetEntry<int>(key).Value; } catch { return fallback; }
        }

        private static bool GetBool(MelonPreferences_Category cat, string key, bool fallback)
        {
            try { return cat.GetEntry<bool>(key).Value; } catch { return fallback; }
        }

        private static float GetFloat(MelonPreferences_Category cat, string key, float fallback)
        {
            try { return cat.GetEntry<float>(key).Value; } catch { return fallback; }
        }

        // ONLY with gregCore (JIT split).
        private void RegisterCoreExtras()
        {
            try
            {
                MemeBridge.RegisterMenu();
                MemeBridge.RegisterExtras(_toggleKey.ToString(), PlayNow, MemePlayer.Stop);
            }
            catch (Exception ex)
            {
                MelonLogger.Warning("[MemeRoulette] Hub registration failed: " + ex.GetBaseException().Message);
            }
        }

        public override void OnUpdate()
        {
            try
            {
                var kb = Keyboard.current;
                if (kb != null)
                {
                    try
                    {
                        var key = kb[_toggleKey];
                        if (key != null && key.wasPressedThisFrame && !IsPauseMenuActive())
                            Toggle();
                    }
                    catch { }
                }

                if (!Settings.Enabled) return;

                float now = Time.realtimeSinceStartup;
                if (now >= _nextRescanAt)
                {
                    _nextRescanAt = now + 300f;
                    try { MemeLibrary.Rescan(); } catch { }
                }

                if (!_timerArmed)
                {
                    ArmTimer(false);
                    return;
                }

                if (now >= _nextMemeAt)
                {
                    FireMeme();
                    ArmTimer(false);
                }
            }
            catch { }
        }

        private static void ArmTimer(bool first)
        {
            try
            {
                float wait = Settings.MinSilenceSec
                    + (float)_rng.NextDouble() * Math.Max(0f, Settings.MaxSilenceSec - Settings.MinSilenceSec);
                _nextMemeAt = Time.realtimeSinceStartup + wait;
                _timerArmed = true;
                _nextRescanAt = Time.realtimeSinceStartup + 300f;
                if (!first)
                {
                    try { MelonLogger.Msg("[MemeRoulette] Next meme in ~" + ((int)wait) + "s."); } catch { }
                }
            }
            catch { }
        }

        internal static void FireMeme()
        {
            try
            {
                string file = MemeLibrary.PickRandom(Settings.NoRepeat);
                if (string.IsNullOrEmpty(file))
                {
                    try { MelonLogger.Msg("[MemeRoulette] Silence continues — no memes in Mods/Memes."); } catch { }
                    return;
                }
                string title = "";
                try { title = Path.GetFileNameWithoutExtension(file); } catch { }
                MemePlayer.Play(file, title, Settings.Volume);
                if (Settings.ToastOnMeme && GregHost.HasCore)
                {
                    try { MemeBridge.Notify("Meme: " + title); } catch { }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning("[MemeRoulette] Fire failed: " + ex.GetBaseException().Message);
            }
        }

        internal static void PlayNow()
        {
            try
            {
                if (MemeLibrary.Count == 0)
                {
                    try { MemeLibrary.Rescan(); } catch { }
                }
                FireMeme();
                ArmTimer(false);
            }
            catch { }
        }

        internal static void Toggle()
        {
            try
            {
                Settings.Enabled = !Settings.Enabled;
                try
                {
                    if (_enabledEntry != null)
                    {
                        _enabledEntry.Value = Settings.Enabled;
                        MelonPreferences.Save();
                    }
                }
                catch { }
                if (!Settings.Enabled)
                {
                    try { MemePlayer.Stop(); } catch { }
                    _timerArmed = false;
                }
                else
                {
                    try { MemeLibrary.Rescan(); } catch { }
                    ArmTimer(true);
                }
                MelonLogger.Msg("[MemeRoulette] Roulette " + (Settings.Enabled ? "ON" : "OFF") + " (" + MemeLibrary.Count + " meme(s)).");
            }
            catch (Exception ex)
            {
                MelonLogger.Error("[MemeRoulette] Toggle failed: " + ex.GetBaseException().Message);
            }
        }

        internal static bool IsPauseMenuActive()
        {
            try
            {
                var all = Resources.FindObjectsOfTypeAll<Canvas>();
                if (all == null) return false;
                foreach (var c in all)
                {
                    if (c == null || !c.isActiveAndEnabled) continue;
                    var go = c.gameObject;
                    if (go == null) continue;
                    try { if (!go.scene.IsValid() || !go.scene.isLoaded) continue; } catch { continue; }
                    if (c.renderMode != RenderMode.ScreenSpaceOverlay) continue;
                    string n = go.name ?? "";
                    if (n.IndexOf("Pause", StringComparison.OrdinalIgnoreCase) >= 0
                        || n.IndexOf("EscapeMenu", StringComparison.OrdinalIgnoreCase) >= 0
                        || n.IndexOf("InGameMenu", StringComparison.OrdinalIgnoreCase) >= 0
                        || n.IndexOf("SystemMenu", StringComparison.OrdinalIgnoreCase) >= 0
                        || n.IndexOf("OptionsMenu", StringComparison.OrdinalIgnoreCase) >= 0
                        || n.IndexOf("SettingsMenu", StringComparison.OrdinalIgnoreCase) >= 0)
                        return true;
                }
            }
            catch { }
            return false;
        }
    }
}
