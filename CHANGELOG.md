# Changelog

All notable changes to the Almedia Link Unity SDK are documented in this file. The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and this project adheres to [Semantic Versioning](https://semver.org/).

## [Unreleased]

## [0.2.0-preview.2] - 2026-05-07

First public preview of the upcoming v0.2.0 release.

### Changed
- SDK is now distributed through a public mirror repo with a clean Git URL install path. No GitHub authentication required for hosts.

### Added
- Each release publishes downloadable artifacts:
  - UPM tarball (`com.almedia.link-X.Y.Z.tgz`).
  - Legacy `.unitypackage`.
  - SHA-256 checksums for both artifacts.
  - Android `mapping.txt` for crash symbolication of SDK frames.
- Release notes for each version are sourced from `CHANGELOG.md` and shown directly on the GitHub Release page.

### Notes
- No SDK feature changes vs. v0.1.0. This preview validates the public distribution pipeline before cutting the v0.2.0 stable release.

## [0.1.0] - 2026-05-07

Initial public release of the Almedia Link Unity SDK.

### Added

**Core**
- iOS and Android native integration via a unified C# bridge.
- `AlmediaLinkSDK` static API for initialization, lifecycle, and event subscription.
- `AlmediaLinkConfig` initialization config object.
- `AlmediaLinkSettings` ScriptableObject for in-editor configuration: integration keys, UI text and colors, notification polling interval, ATT preferences.
- Public data models in `AlmediaLink.Models` (status, errors, notifications, link state).
- Static C# event API: SDK status changes, link completion, notifications received, errors, and log forwarding.
- Custom logger injection. Route SDK logs into your existing logging pipeline.
- iOS IDFA support for attribution.
- App Tracking Transparency (ATT) flow integrated with SDK initialization on iOS, including a pre-prompt screen.
- Event tracking for promo and notification interactions.

**UI**
- `LinkButton` prefab with four design variants (A, B, C, D) you can drop into your scenes.
- `LinkPopup`: the modal account-linking screen.
- `NotificationCard`: in-game notification overlay.
- `ActivityOverlay`: expanded notification list.
- `ATTPrePrompt`: iOS tracking-consent intro screen.
- **Almedia > Settings** editor window for one-stop SDK configuration.

**Distribution**
- Installable via Unity Package Manager (UPM) from a Git URL.
- Legacy `.unitypackage` artifact also available for projects not yet on UPM.
- SHA-256 checksums published alongside both artifacts.
- Android `mapping.txt` published with each release for crash symbolication of SDK frames in your stack traces. iOS dSYMs are bundled inside the included `.xcframework`.

### Requirements
- Unity 2022.3 or newer.
- iOS 13.0+ / Android API 21+.
