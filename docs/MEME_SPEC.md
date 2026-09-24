# MemeRoulette spec (v1)

## Goal

Pure silence, interrupted at random times by random meme sounds from `{game}/Mods/Memes/*`.

## Behavior

- Folder `Mods/Memes/` auto-created (+ README) on init; recursive scan for `.wav`/`.mp3`; `.ogg` warns + skips; rescan on enable + every 5 min.
- Roulette timer: `wait = Min + rand() * (Max - Min)` (defaults 60/300 s, min clamp 5 s); on fire: `PickRandom(NoRepeat)` → play → re-arm. Empty library → log + re-arm (silence continues).
- `F11` toggles (persists `Enabled`); disabling stops audio + disarms. F1 hub opener = `PlayNow` (instant meme + re-arm), closer = stop.
- Volume 0–1 (default 0.8); files over `MaxFileMb` (25) skipped; optional toast with meme name (gregCore only, default off).

## Audio path (verified MusicPlayer approach)

Engine decode stripped on IL2CPP → managed decode to float PCM → `AudioClip.Create` + `SetData` → `AudioSource.Play` (own `GameObject`, `DontDestroyOnLoad`). With gregCore: `GregAudioService.PlayFile` (cache). WAV: manual RIFF (PCM 8/16/24/32 + float32). MP3: NLayer.

## File map

- `src/MemeRouletteMod.cs` — entry, prefs, timer, toggle.
- `src/Core/MemeLibrary.cs`, `MemeDecoder.cs`, `MemePlayer.cs`, `MemeSettings.cs`, `MemeBridge.cs`, `GregHost.cs`.

## Manual tests (in game)

1. Empty `Mods/Memes` → log warns, silence. Add wav/mp3 → within 5 min (or re-toggle) picked up.
2. Wait for fire → meme plays, log shows title + length; next countdown logged.
3. Same file never twice in a row (with 2+ files).
4. `F11` off → silence + stop; on → rescan + re-arm. F1 hub: opener plays instantly.
5. With/without gregCore.dll: same behavior (service vs local source).
6. Oversized/corrupt/ogg files → skipped with warning, roulette continues.
