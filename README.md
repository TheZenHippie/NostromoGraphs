<p align="center">
  <h3>📺 Click below to watch a demonstration video on YouTube!</h3>
  <a href="https://youtu.be/_lhA_St6_nY" target="_blank">
    <img src="https://youtube.com" alt="Watch the demonstration video" width="640" style="max-width: 100%; border-radius: 8px;" />
  </a>
</p>


# NostromoGraphs // MUTHUR 6000 Telemetry Visualizer

A retro-futuristic animated telemetry, radar, and sensor graphics visualizer for Windows based in the universe of the 1979 film *Alien* and the Weyland-Yutani **MUTHUR-6000** mainframe.

Built with **C# (.NET 10)**, **WPF**, and **SkiaSharp** for high-performance, 60 FPS vector animations.

---

## Features

- **16 Lore-Accurate Procedural Displays**:
  1. **Environmental Gasses Mix** — Cabin atmosphere gas chromatography (N2, O2, Ar, CO2) with animated donut charts & life support scrubbers.
  2. **Stellar Plot To Earth** — 3D rotating starfield wireframe and trajectory vector from Zeta II Reticuli (LV-426) to Sol (Earth).
  3. **Crewmember Vital Signs** — 8-channel bio-telemetry monitor with real-time scrolling ECG waveforms for Dallas, Ripley, Kane, Lambert, Parker, Brett, Ash, and Jonesy (Ship's Cat).
  4. **Radar Display of LV-426** — 360° sweeping polar radar with surface terrain contours and derelict acoustic beacon lock.
  5. **Stellar Cartography** — Deep space Sector 04 astrometric grid, orbital lanes, and gyro-stabilized compass.
  6. **Alien Signal Analysis** — Dual-channel oscilloscope, 32-band FFT harmonic spectrum analyzer, and alien packet decryption stream.
  7. **Atmospheric Analysis of LV-426** — Barometric pressure vs altitude decay curve, corrosive toxic aerosol ratings, and surface squalls.
  8. **Ash Uplink Signal Strength** — Special Order 937 priority transmission, RF sub-space carrier wave, and directive telemetry.
  9. **Local Space Analysis** — Calpamos gas giant gravitational well, moon orbits (LV-426 & LV-223), and insertion vectors.
  10. **Hyperdrive Output & Propulsion** — 2.8 TW bimodal reactor tachometers, FTL tachyon flux harmonics, and RCS manifold balance.
  11. **Refinery Processing Status** — 20 Million Tons mineral cargo refining schematic, catalytic cracking towers, and mineral purity gauges.
  12. **Cryosleep Status** — 8 cryogenic stasis capsules, core temperatures, and wake-up purge status.
  13. **Time to Earth / Time from Thedus** — Relativistic time dilation clocks ($\gamma = 1.042$), journey timeline, and deceleration checkpoints.
  14. **Composition of LV-426** — Planetary geological strata cutaway (crust, mantle, core) and real-time subsurface seismograph.
  15. **Orbit Insertion & Surface Descent** — Vintage 1979 Alien MUTHUR CRT screen with 3D rotating planetoid, multi-tier elevation contour loops, approach corridor tunnel reticles, and live landing altitude descent simulation.
  16. **Preliminary Survey & Surface Expedition** — Authentic 1979 Alien LV-426 cartography CRT screen with vector red elevation contours, curved perspective grid, Position 1 crater massif, Position 2 Derelict rocky outcrop, and animated Dallas, Kane, and Lambert EVA tracking path.

- **Screen Clear Glitch Transitions**:
  - Authentic analog CRT screen clear / glitch sequence before and after each display switch.
  - Features horizontal slice tearing, raster beam wipe, phosphor flash bloom, and hex matrix data burst.

- **User Selectable Hold Duration (10s – 60s)**:
  - Custom display duration slider (10 to 60 seconds).
  - Quick presets: 10s, 15s, 20s, 30s, 45s, 60s.
  - Header quick-cycle button `[ 20s CYCLE ]`.

- **Display Color Modes**:
  - **Vintage Monochrome (Classic Single Phosphor)** *(Default)* — All graphics, graphs, alert highlights, gauges, and biometrics strictly adhere to shades/luminance levels of the active phosphor color for authentic 1979 CRT fidelity.
  - **Modern Multi-Color (Full Accent HUD)** — Rich multi-colored telemetry with vivid red alerts, cyan highlights, amber warnings, and distinct gas/mineral spectrum colors.

- **Phosphor Color Themes**:
  - **Matrix / Nostromo Green** (`#00FF66`) *(Default)*
  - **Solar Amber (Alien Gold)** (`#FFB000`)
  - **Nostromo Cyan** (`#00F0FF`)
  - **Phosphor White** (`#F0F0F0`)
  - **Glitch Red** (`#FF2244`)
  - **Night City Violet** (`#A040FF`)
  - **Synthwave Magenta** (`#FF007F`)
  - Custom Phosphor Color picker

- **Pacing & Sweep Speeds**:
  - **Calibrated ECG Sweep**: Crew vitals waveforms sweep at a calm, authentic clinical CRT monitor speed (~0.35x sweep speed) with natural heart rate variability.

- **CRT Visual Effects Suite**:
  - Phosphor Bloom Glow
  - Vintage CRT Scanlines (Adjustable thickness)
  - CRT Monochrome Glitch
  - CRT Snow Static Noise

- **Window & Layout Controls**:
  - Frameless draggable window with resize grip.
  - Always on Top toggle.
  - Window Shadow toggle.
  - Glass Opacity (30% – 100%) and HUD Brightness sliders.
  - Auto-saved settings to `%APPDATA%\NostromoGraphs\settings.json`.

---

## Build & Run

```powershell
# Run in Debug mode
dotnet run --project "D:\Other Coding Projects\NostromoGraphs\NostromoGraphs.csproj"

# Build Release single-file executable
dotnet publish -c Release -r win-x64 --self-contained false
```

