# AGENTS.md — Notes for AI agents (gregMod.MemeRoulette)

Repo: gregMod.MemeRoulette · License: Apache-2.0 · Version: see `VERSION`.

## Duties

1. **Read first:** `README.md`, `docs/INDEX.md` — only then make changes.
2. **Do not commit secrets** (keys, tokens, `.env`). Use keys only via environment variables.
3. **Preserve history:** no `push --force`, no history rewrite without instruction.
4. **Back up changes:** before reporting done, build the mod (`dotnet build gregMod.MemeRoulette.csproj -c Release` or `./build.sh MemeRoulette`).
5. **Keep docs in sync:** for new features update `README.md` + `docs/` + `CHANGELOG.md` (Unreleased).
6. **Conventions:** Conventional Commits (`feat:`, `fix:`, `docs:`, `chore:` …), one logical change per commit.
7. **When unsure:** stop and ask instead of guessing — especially for deletes, migrations, CI.

## Hard rules

- **Never** engine audio decode (`UnityWebRequestAudio`, `AudioClip` streaming) — stripped on IL2CPP. Managed decode only (`MemeDecoder`).
- **Never** touch gregCore types outside `src/Core/MemeBridge.cs` (JIT split — mod must run without gregCore.dll).
- **Never** commit `references/*.dll`, `bin/`, `obj/`, or meme audio files (see `.gitignore`).
- NuGet (`NLayer`, MP3 decode) is NOT copied to `bin/` (net35 asset) — deploy `NLayer.dll` next to the mod DLL in `Mods/` manually (see README). Never commit DLLs.

## Layout

- `src/MemeRouletteMod.cs` — MelonMod entry, prefs, roulette timer, toggle.
- `src/Core/GregHost.cs` — soft probe (no gregCore types).
- `src/Core/MemeBridge.cs` — ONLY gregCore-touching code.
- `src/Core/MemeSettings.cs` — pref data (no Unity refs).
- `src/Core/MemeLibrary.cs` — `Mods/Memes` scan + no-repeat pick.
- `src/Core/MemeDecoder.cs` — WAV/MP3 decode (MusicPlayer coverage).
- `src/Core/MemePlayer.cs` — playback dispatcher (core service / local source).
