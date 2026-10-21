<p align="center">
  <img src="banner.png" alt="Mahan Client Checker" width="100%">
</p>

<p align="center">
  <img src="https://img.shields.io/badge/platform-Windows-0078D6?style=for-the-badge&logo=windows&logoColor=white" alt="platform">
  <img src="https://img.shields.io/badge/language-C%23-239120?style=for-the-badge&logo=csharp&logoColor=white" alt="language">
  <img src="https://img.shields.io/badge/UI-WinForms-7B2FBE?style=for-the-badge" alt="ui">
  <img src="https://img.shields.io/badge/status-active-success?style=for-the-badge" alt="status">
</p>

# Mahan Client Checker (MahanSS)

A Windows desktop tool that scans a system for traces of **Minecraft cheat clients and modules** — whether they're currently running, sitting on disk, or already deleted.

> Built for server admins and anti-cheat operators who need a fast, deep check of a player's system.

---

## ✨ Features

- **Live RAM scan** — searches the memory of running `java` / `javaw` processes for known cheat-client signatures.
- **Targeted disk scan** — checks `.minecraft`, `.lunarclient`, `.tlauncher` and similar folders, plus `AppData`, `Temp`, `Prefetch`, and `Recent`.
- **Deleted-evidence recovery** — reads the Windows **USN Journal** and **MFT** directly to find cheat files a user tried to delete (fully read-only, no changes made to the system).
- **Running-process check** (`tasklist`) for cheats launched directly as an exe.
- **Recycle Bin recovery** for recently deleted files.
- **Timestamped reporting** — shows the last run/modified/installed time for every flagged item, plus a "running right now" tag.
- **Saveable report output** as a plain-text file.
- Minecraft-themed UI with neon glow visual effects.

## 🖥️ Requirements

- Windows 10/11
- Administrator access (required for the USN Journal, MFT, and reading other processes' memory)
- .NET Framework / .NET Desktop Runtime (specify the exact version you build against here)

## 🔧 Build

```bash
csc MahanSSChecker.cs -target:winexe -reference:System.Windows.Forms.dll,System.Drawing.dll,System.Management.dll
```

Or build via Visual Studio with the `WinExe` output type, and enable `requireAdministrator` in the application manifest.

## ⚠️ Important Notes

- This tool is intended for **legitimate anti-cheat and security purposes** (scanning a system you own or have consent to inspect). Do not use it to access someone else's system without authorization.
- Because this program reads other processes' memory, performs low-level disk scanning, and enables system privileges, **it may be flagged by antivirus software (especially Windows Defender)**. This is expected behavior for this category of tool, not a bug. To reduce false positives:
  - Digitally sign the compiled binary (code signing).
  - Don't use a packer/obfuscator.
  - If it gets flagged, submit it for review via [Microsoft Security Intelligence](https://www.microsoft.com/wdsi/filesubmission).
- The signature list (`StrongSigs`) and suspicious-keyword list (`SuspiciousKeywords`) are updated over time; if something gets flagged incorrectly (a false positive), please report it in Issues.

## 📜 License

Specify your project's license here (e.g. MIT).

## 👤 Developer

Developed by **Lord Mahan**
Telegram: [@orwinner](https://t.me/orwinner)
