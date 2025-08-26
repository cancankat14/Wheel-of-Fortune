# Wheel of Fortune — README

## Overview
Lightweight “spin the wheel” demo with **safe/super** zones, **walk-away** flow, and **revive**. Data-driven via **ScriptableObjects**; UI motion via **DOTween** (see **licensing note**). Built for **clean UI**, **fast iteration**, and **easy balancing**.

## Tech
- **Unity:** 2022 LTS
- **Tweening:** Built with **DOTween Pro**; repository ships with **DOTween (free)** for licensing reasons.
- **UI:** TextMeshPro
- **Art pipeline:** all resizable UI sprites are **9-sliced** in Sprite Editor and packed in a **Sprite Atlas**

## How to Play
- Tap **Spin** → land on a **reward** or a **bomb**.
- **Every 5th zone** = **Silver (safe)** *(no bomb)*.
- **Every 30th zone** = **Gold (super)** *(no bomb, special rewards)*.
- On **safe/super** (and **not spinning**) you may **Leave** to bank your stash.
- Hitting a **bomb** wipes the run; **Revive** with **Gold** (cost **doubles** each time). **“Revive Ad”** is a stub.

## Systems (short)
- **Progression:** `ProgressionConfig` (SO) defines zone cadence (safe **5**, super **30**), **per-zone multiplier**, **tier factors**, and **cap**. Multiplier is computed once per zone and applied to numeric rewards.
- **Wheel & Slices:** `Wheel` assigns `Slot.Slice { reward, baseAmount, amount }`. Each `Slot` renders its icon/text from its **Slice**.
  - **Bronze:** injects exactly **N** bombs from config.
  - **Silver/Gold:** never contain bombs.
- **Inventory:** run **stash** + ordered pickup **history**; **Gold** tracked separately; call `BankStash()` on **Leave**.
- **Animations:** all runtime UI motion uses **DOTween** via the `IUIAnimation` interface.

## Animator note (one-off tutorial)
There is **one Animator** used only for the first-time tutorial. It sits on the **arrow images** to preserve exact layering easily, plays once for each player, then is destroyed forever. All other UI motion uses **DOTween**.

## UI & Architecture
- **Panels instead of game states:** each panel drives itself with a small script. Panels: `LeavePanelController`, `EndPanel`, `ZoneEnter` animation panel.
- **Spin panel** is passive; **Wheel** logic lives on **`ui_wheel_root`**.

**Wheel hierarchy**
- `ui_wheel_root` (has `Wheel.cs`)
- `ui_wheel_group_slots / ui_slot_rot_0 ... ui_slot_rot_N` (each has `Slot.cs`)

- **Managers:** Classes implement `IManager` and derive from `Singleton<T>`. `UIManager` lives on the **Canvas**; other managers (`GameManager`, `LevelManager`, `InventoryManager`, `TutorialManager`) live under a **GameManager** object.

## UI Compliance (per brief)
- Canvas Scaler: **Scale With Screen Size → Match = Expand**
- **TMP** everywhere; dynamic labels end with **`_value`**
- Naming: `ui_` prefix, general → specific (e.g., `ui_image_spin_silver`)
- **No inspector OnClick**; buttons wired in code / `OnValidate`
- Animators **not on root transforms** (except the one-off tutorial noted above)
- Correct **anchors/pivots** for **20:9 / 16:9 / 4:3**
- Non-interactive Images: **RaycastTarget / Maskable OFF**
- **Sliced** images use **9-slice**; sprites packed in **Sprite Atlas**

## Project Layout
Assets/
  Scripts/
    Core/            (Enums, IManager, IUIAnimation, Singleton, UtilityHelper)
    Game/            (EndPanel, LeavePanelController, Reward, RewardCardView, ProgressionConfig)
    Wheel/           (Wheel, Slot, WheelConfig)
    Managers/        (GameManager, InventoryManager, LevelManager, UIManager, TutorialManager)
    UIAnimations/    (FadeOut, Pulse, UIPunchScale, UIFlyToTarget, UISizeChange)
  Resources/GameData/
    ProgressionConfig_Default.asset
    Rewards/         (Reward_* assets)
    WheelConfigs/    (Bronze.asset, Silver.asset, Golden.asset)

## Dev Shortcuts
- **R**: random spin
- **D**: force bomb (if present)
- **G**: quick-generate Bronze wheel

## DOTween licensing note
The project was authored with **DOTween Pro** (preferred for editor utilities). To make cloning/building easy without paid assets, the repository includes **DOTween (free)**, and all runtime tweens are compatible. If you import **Pro** locally, everything continues to work; **no code changes required**.
