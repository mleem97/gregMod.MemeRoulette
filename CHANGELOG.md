# Changelog

All notable changes to this project will be documented in this file.

## [Unreleased]

### Added

- Random Meme Sound Roulette (`F11`): random silence (60–300 s prefs) interrupted by a random meme from `Mods/Memes/*` (.wav/.mp3 recursive, no-repeat pick, auto-rescan).
- Managed decode (WAV manual + MP3 NLayer, same coverage as MusicPlayer); playback via `GregAudioService` with gregCore, local `AudioSource` fallback standalone.
- GregCore wiring behind soft probe: F1 menu (opener = instant meme, closer = stop) + key HUD + optional toasts.
- Docs: `docs/MEME_SPEC.md`.
