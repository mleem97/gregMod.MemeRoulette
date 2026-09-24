// MemeBridge — the ONLY file that touches gregCore types.
// RULE (Greg-Vertrag): call only when GregHost.HasCore is true (JIT split).
using System;

namespace GregMod.MemeRoulette.Core
{
    internal static class MemeBridge
    {
        private const string MenuId = "memeroulette";
        private const string ModId = "gregMod.MemeRoulette";

        internal static void RegisterMenu()
        {
            gregCore.UI.GregMenuRegistry.RegisterMenu(MenuId,
                new gregCore.UI.GregMenuOptions
                {
                    LockCamera = false,
                    LockMovement = false,
                    LockInteract = false,
                    ShowCursor = false,
                });
        }

        internal static void RegisterExtras(string toggleKeyText, Action playNow, Action stop)
        {
            gregCore.Core.Mods.GregModRegistry.Register(
                ModId, "MemeRoulette", "1.0.0", new string[] { "memeroulette" });
            gregCore.UI.GregHudRegistry.Register(MenuId, toggleKeyText ?? "F11", "Memes");
            // Hub opener = instant meme (roulette keeps running); closer = stop.
            gregCore.UI.GregMenuRegistry.RegisterOpener(MenuId, () => { try { playNow(); } catch { } });
            gregCore.UI.GregMenuRegistry.RegisterCloser(MenuId, () => { try { stop(); } catch { } });
        }

        internal static void Notify(string msg)
        {
            try { gregCore.PublicApi.greg.UI.ShowNotification(msg); }
            catch { /* best-effort: log path covers it */ }
        }
    }
}
