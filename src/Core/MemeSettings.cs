// MemeSettings — pref-backed config (plain data, no Unity refs).
using System;

namespace GregMod.MemeRoulette.Core
{
    internal sealed class MemeSettings
    {
        internal bool Enabled = true;
        internal string ToggleKeyText = "F11";
        internal float MinSilenceSec = 60f;
        internal float MaxSilenceSec = 300f;
        internal float Volume = 0.8f;
        internal bool NoRepeat = true;
        internal bool ToastOnMeme = false;
        internal int MaxFileMb = 25;
    }
}
