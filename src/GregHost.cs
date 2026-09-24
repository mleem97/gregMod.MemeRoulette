using System;

namespace GregMod.MemeRoulette;

// Soft-dependency probe (pure type-name lookup): methods touching gregCore
// must ONLY run when HasCore is true (otherwise JIT TypeLoad without the DLL).
internal static class GregHost
{
    private const string ProbeType = "gregCore.UI.GregNotificationManager, gregCore";
    private static bool? _hasCore;

    public static bool HasCore
    {
        get
        {
            if (_hasCore == null)
            {
                try { _hasCore = Type.GetType(ProbeType) != null; }
                catch { _hasCore = false; }
            }
            return _hasCore.Value;
        }
    }
}
