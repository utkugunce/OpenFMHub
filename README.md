# OpenFM Hub ⚽📊

**Open-source, 100% free multi-season archive, Moneyball scouting engine, and companion tool for Football Manager 2026.**

Built with **C# / .NET 8 WPF**, **SQLite**, and **Windows 11 Fluent Dark UI**.

![Platform](https://img.shields.io/badge/Platform-Windows%2010%20%2F%2011-blue)
![Framework](https://img.shields.io/badge/Framework-.NET%208%20WPF-purple)
![License](https://img.shields.io/badge/License-MIT-success)
![Cost](https://img.shields.io/badge/Price-100%25%20Free%20%26%20Open%20Source-brightgreen)

---

## 💡 Why OpenFM Hub?

In **Football Manager (FM26 / FM24)**, when a season rolls over (every June/July), the game engine **permanently wipes match-by-match stats, xG maps, and granular per-90 metrics**, leaving only career aggregate totals.

Commercial tools like **FM Season Hub (FMSH)** lock this data preservation and Moneyball scouting behind a **paid monthly Patreon subscription**. 

**OpenFM Hub** solves this completely for the FM community:
- **Zero Subscription / 100% Free & Open Source:** Runs locally on your machine with no paywalls or license keys.
- **Permanent Local SQLite Archive:** Freezes and preserves full seasonal match ratings, per-90 stats, and attributes year-over-year.
- **Moneyball Role Scoring & Spider Radar:** Calculates position-specific role suitability (0-100) and displays a live 6-axis percentile radar.
- **Hidden Attributes & Potential:** Displays CA (Current Ability), PA (Potential Ability), Consistency, Big Match performance, and Injury Proneness.
- **Contract & Wage Efficiency Matrix:** Flags expiring contracts (< 12 months) and deadwood / overpaid players.
- **1-Click FM Ingestion:** Import any squad view or scouting search directly from FM (`Ctrl + P` -> Save as Web Page / HTML).

---

## 🚀 Key Features

### 1. 📊 Squad & Season Overview
Interactive grid with real-time sorting and filtering by:
- Player, Age, Club, Position, Nationality.
- **Role Score (Moneyball Index):** 0 to 100 suitability rating.
- Apps, Goals, Assists, xG, xA, Average Match Rating, Market Value.

### 2. 🎯 Moneyball Role Engine & Percentile Radar
Evaluates players against role-specific algorithms:
- **Pasör Stoper (Ball-Playing Defender):** Tackling, Passing, Vision, Decisions, Composure, Strength.
- **İki Yönlü Bek (Wing-Back):** Pace, Acceleration, Stamina, Tackling, Crossing, Dribbling.
- **Regista / DLP (Deep-Lying Playmaker):** Vision, Passing, Technique, Decisions, Composure.
- **Kutuya Kutu (Box-to-Box Midfielder):** Work Rate, Stamina, Passing, Tackling, Finishing.
- **Kanat Forvet (Inside Forward):** Dribbling, Acceleration, Pace, Finishing, Decisions.
- **Gelişmiş Forvet (Advanced Forward):** Finishing, Pace, Composure, Acceleration, Decisions.
- **Live 6-Axis Spider Radar Chart:** Visualizes Scoring, Creation, Possession, Defending, Physical, and Mental capabilities.

### 3. 📈 Multi-Season Progression Curves
- Group players across multiple saved seasons (e.g. 2024/25 vs 2025/26 vs 2026/27).
- Track attribute increases (+1 Pace, +2 Decisions) and rating trends.

### 4. ⚠️ Contract Expiry & Wage Efficiency
- Identifies players with $< 12$ months left on their deal to prevent free transfers.
- Evaluates €/Rating point to highlight bargain players vs expensive underperformers.

---

## 📥 How to Import from Football Manager 2026

1. Open Football Manager 2026.
2. Go to any screen: **Squad (Kadro)**, **Scouting (Gözlemcilik)**, or **Player Search (Oyuncu Arama)**.
3. Press **`Ctrl + P`** on your keyboard (or click the FM menu $\rightarrow$ Print / Yazdır).
4. Select **Web Page (*.html)** and save it.
5. In **OpenFM Hub**, click **"📥 FM HTML İçe Aktar"** and select that file.
6. The app instantly parses all players, calculates Moneyball indices, and stores them in your local archive!

---

## 🛠️ Tech Stack & Architecture

- **Language:** C# 12 / .NET 8.0 WPF
- **Database:** SQLite WAL (`Microsoft.Data.Sqlite`) in `%LOCALAPPDATA%\OpenFMHub\openfm_hub.db`
- **Styling:** Windows 11 Fluent Dark (`dwmapi.dll` Immersive Dark Mode & Rounded Corners)
- **Visuals:** Hardware-accelerated WPF Canvas geometry for radar spiders and progress meters.

---

## 📦 Building from Source

```powershell
# Restore & Build
dotnet build -c Release

# Publish Single-File Executable (~490 KB)
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o ./dist
```

---

## 📄 License
MIT License. © 2026 utkugunce
