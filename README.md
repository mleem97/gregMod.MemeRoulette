# gregMod.MemeRoulette

Random Meme Sound Roulette: pure silence, interrupted at random times by meme sounds from `{game}/Mods/Memes/*` (.wav/.mp3, recursive). Standalone-safe, GregCore-integrated (F1 hub + key HUD + toasts).

## Setup

1. Drop `.wav` / `.mp3` files into `<Game>/Mods/Memes/` (created on first launch, with README).
2. Launch — log shows `[MemeRoulette] Loaded. N meme(s) in Mods/Memes.`
3. Wait. A random meme fires after a random silence (default 60–300 s).

## Controls & prefs (`MelonPreferences / gregMod.MemeRoulette`)

- `F11` = roulette on/off. F1-hub opener = instant meme, closer = stop.
- `MinSilenceSec` / `MaxSilenceSec` (defaults 60/300), `Volume` (0.8), `NoRepeat` (true), `ToastOnMeme` (false), `MaxFileMb` (25).
- New files are picked up automatically (rescan on enable + every 5 min).

## Audio path

Same verified approach as MusicPlayer (engine audio decode is stripped on IL2CPP): managed decode (WAV manual, MP3 via NLayer) → `AudioClip` → `AudioSource`. With gregCore it plays through `GregAudioService` (cache); without, through a local source. `.ogg` warns + skips.

## Build

```bash
dotnet build gregMod.MemeRoulette.csproj -c Release
cp bin/Release/net6.0/gregMod.MemeRoulette.dll "$DATACENTER_HOME/Mods/"
# MP3 support needs the managed decoder next to the mod (WAV works without it):
cp ~/.nuget/packages/nlayer/1.11.0/lib/net35/NLayer.dll "$DATACENTER_HOME/Mods/"
```
