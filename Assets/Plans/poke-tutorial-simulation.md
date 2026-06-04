# Project Overview

- **Game Title:** VR Programming (UAS scene)
- **High-Level Concept:** A VR environment where the existing static "tips" overlay is upgraded into an *interactive tutorial simulation* — a world-space panel the player can read and operate with their own hand using a Poke (finger-touch) interaction.
- **Players:** Single player (VR / OpenXR)
- **Inspiration / Reference Games:** Standard XR onboarding panels (XRI World Space UI sample, Meta first-time tutorials)
- **Tone / Art Direction:** Functional VR UI; clean readable panel
- **Target Platform:** Android (Quest-class) via OpenXR — also runs on PCVR
- **Screen Orientation / Resolution:** N/A (VR HMD)
- **Render Pipeline:** URP (PC_RPAsset active)
- **Unity / Packages:** Unity 6000.4.0f1, XR Interaction Toolkit **3.4.1**, XR Hands 1.8.0, OpenXR 1.16.1. Active Input Handling = **Both**.

## Goal (from request)
Turn the current "tips" HUD into a **tutorial simulation** meeting three criteria:
1. The UI is a tutorial **with buttons**.
2. The UI **can be operated by the player**.
3. Operation is done **using the Poke Interactor** (finger touch).

## Confirmed design decisions
- **Placement:** Spawn the panel in front of the player when toggled on, then **freeze it in place** (stops head-following) so it can be poked comfortably.
- **Structure:** **Multi-page** tutorial with **Next / Back / Close** buttons (existing tips split across pages).
- **Toggle:** **Keep** the existing controller-button toggle (VRHUDController) to open/close the tutorial, in addition to the poke-driven Close button.

---

# Current State (verified)

- **Scene:** `Assets/UAS/uas 3 Wall collider.unity` (loaded). (`Assets/UAS/uas 4.unity` is the attached duplicate target — see note in Implementation Steps.)
- **`TipsCanvas`** (`UAS Scene/TipsCanvas`): World-Space `Canvas` + `CanvasScaler` + **`GraphicRaycaster`**, with a single child `Text (TMP)` containing the tips text. No buttons. Driven by `VRHUDController` on **HUDManager**.
- **`HUDManager`** → `VRHUDController.cs` (`Assets/UAS/scripts/VRHUDController.cs`): toggles `tipsUI` via an InputActionProperty button and **continuously lerps** the canvas to float in front of the head (`Update()`), making it unusable for poking as-is.
- **EventSystem:** uses **`InputSystemUIInputModule`** → *incompatible* with XR UI poke; must be replaced by **`XRUIInputModule`**.
- **XR Rig** (`XR Origin (VR)`): per hand has `Teleport Interactor` (XRRayInteractor), `NearFarInteractor`, animated hand model with finger bones. **No Poke Interactor exists.**
- Index fingertip bones available: `hands:b_r_index3`, `hands:b_l_index3` (ideal poke points).
- Starter Assets present: `Assets/Samples/XR Interaction Toolkit/3.4.1/Starter Assets/Presets/XRI Default XR UI Input Module.preset` and `.../XRI Default Input Actions.inputactions`.

---

# Game Mechanics

## Core Gameplay Loop (tutorial interaction)
1. Player presses the controller toggle button → tutorial panel appears in front of them and **locks in place**.
2. Player reaches out and **pokes** the on-screen **Next / Back** buttons with a fingertip to page through tutorial content.
3. Player pokes **Close** (or presses the toggle again) to dismiss the panel and continue exploring.

## Controls and Input Methods
- **Poke (primary):** `XRPokeInteractor` on each hand, poke point at the index fingertip bone. Pokes uGUI Buttons via the canvas's `TrackedDeviceGraphicRaycaster` + `XRUIInputModule`.
- **Controller toggle (secondary):** Existing `VRHUDController.toggleButton` opens/closes the panel.
- Active Input Handling is **Both** → the `XRUIInputModule`'s **Active Input Mode** must be set to **Input System Actions** explicitly.

---

# UI

World-space tutorial panel (rebuild of `TipsCanvas`):

```
+------------------------------------------------+
|  TUTORIAL                         (Page 1/3)   |   <- Title + page indicator
|------------------------------------------------|
|                                                |
|   [ Body text for the current page.            |
|     Multi-line tips content. ]                 |
|                                                |
|                                                |
|------------------------------------------------|
|   [  Back  ]        [ Close ]       [  Next  ] |   <- Poke-able buttons (large)
+------------------------------------------------+
```

- Buttons sized **large for poke** (recommended ≥ 0.04 m physical; with the current world-space scale ~0.001 that is ≥ ~40 px height, e.g. 80×40 px each with generous spacing).
- `Back` disabled (greyed) on first page; `Next` becomes `Finish`/disabled on last page.
- Optional `XRPokeFollowAffordance` on each button for a press-in visual cue.

### Proposed page content (split from current tips text; editable in inspector)
- **Page 1 – Welcome / Movement:** Overview of moving and teleporting around the scene.
- **Page 2 – Remote TV:** "The Remote TV on the table below the TV is a grabbable object; interacting with the *Trigger* button teleports you inside one of the cars."
- **Page 3 – Teleport Targets:** "The pot and the sofa (marked with an arrow above it) are also objects you can teleport to."

---

# Key Asset & Context

### Files to create
- `Assets/UAS/scripts/TutorialPanelController.cs` — manages pages, Next/Back/Close, title/page-indicator, button enable state.

### Files to modify
- `Assets/UAS/scripts/VRHUDController.cs` — change from continuous head-follow to **position-once-then-freeze** on open; keep toggle behavior.

### Scene objects to modify / create (in `uas 3 Wall collider` and/or `uas 4`)
- `EventSystem`: remove `InputSystemUIInputModule`, add `XRUIInputModule` (apply preset; set Active Input Mode = Input System).
- `TipsCanvas`: replace `GraphicRaycaster` with **`TrackedDeviceGraphicRaycaster`**; add background Image/Panel, Title (TMP), Body (TMP), and three Buttons; add `TutorialPanelController`.
- `Right Hand Controller` / `Left Hand Controller`: add child `Poke Interactor` GameObject with `XRPokeInteractor` (Enable UI Interaction = on; Poke Point = corresponding `hands:b_*_index3` fingertip bone).

### API / component references (XRI 3.4.1)
- `UnityEngine.XR.Interaction.Toolkit.Interactors.XRPokeInteractor`
- `UnityEngine.XR.Interaction.Toolkit.UI.XRUIInputModule`
- `UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster`
- `UnityEngine.XR.Interaction.Toolkit.AffordanceSystem` / `XRPokeFollowAffordance` (optional press visual)
- `XRPokeInteractor` key fields: `enableUIInteraction`, `pokePoint`, `pokeDepth`, `pokeWidth`.

`TutorialPanelController` sketch:
```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialPanelController : MonoBehaviour
{
    [TextArea(2,6)] public string[] pages;
    public TMP_Text bodyText, pageIndicator;
    public Button nextButton, backButton, closeButton;
    public GameObject panelRoot;   // object to hide on Close (the TipsCanvas)
    int index;

    void OnEnable() { index = 0; Refresh(); }
    public void Next() { if (index < pages.Length - 1) { index++; Refresh(); } }
    public void Back() { if (index > 0) { index--; Refresh(); } }
    public void Close() { panelRoot.SetActive(false); }

    void Refresh()
    {
        bodyText.text = pages.Length > 0 ? pages[index] : "";
        if (pageIndicator) pageIndicator.text = $"{index + 1}/{pages.Length}";
        if (backButton) backButton.interactable = index > 0;
        if (nextButton) nextButton.interactable = index < pages.Length - 1;
    }
}
```

`VRHUDController` change (position-once-then-freeze) — outline:
```csharp
// On toggle ON: compute target once, place panel there, DO NOT follow afterwards.
void Update()
{
    if (toggleButton.action != null && toggleButton.action.WasPressedThisFrame() && tipsUI != null)
    {
        bool turningOn = !tipsUI.activeSelf;
        tipsUI.SetActive(turningOn);
        if (turningOn && playerCamera != null)
        {
            Vector3 target = playerCamera.position
                + playerCamera.forward * forwardDistance
                + playerCamera.right * leftOffset
                + playerCamera.up * upOffset;
            tipsUI.transform.position = target;                       // place once
            tipsUI.transform.LookAt(target + playerCamera.forward);   // face the user, then freeze
        }
    }
    // No continuous follow → panel stays put so it can be poked.
}
```

---

# Implementation Steps

> Scene note: The attached asset is `uas 4.unity`, but the **loaded/edited** scene is `uas 3 Wall collider.unity`. **Step 0** confirms which scene to apply changes in before any edits.

### Step 0 — Confirm target scene
- **Description:** Verify whether changes go into `uas 3 Wall collider.unity` (currently loaded) or `uas 4.unity` (attached). Open the correct scene before editing.
- **Assigned role:** developer
- **Dependencies:** None
- **Parallelizable:** No

### Step 1 — Replace input module on EventSystem
- **Description:** Remove `InputSystemUIInputModule` from `EventSystem`; add `XRUIInputModule`; apply `XRI Default XR UI Input Module.preset`. Because Active Input Handling = Both, set **Active Input Mode = Input System Actions** and wire the navigation/point/click actions from `XRI Default Input Actions.inputactions`.
- **Assigned role:** developer
- **Dependencies:** Step 0
- **Parallelizable:** Yes (with Step 2, 3)

### Step 2 — Make TipsCanvas poke-ready
- **Description:** On `TipsCanvas`, remove `GraphicRaycaster` and add **`TrackedDeviceGraphicRaycaster`**. Keep render mode World Space. Verify canvas scale/size remain correct for VR.
- **Assigned role:** developer
- **Dependencies:** Step 0
- **Parallelizable:** Yes (with Step 1, 3)

### Step 3 — Add Poke Interactors to both hands
- **Description:** Under `Right Hand Controller` and `Left Hand Controller`, create a `Poke Interactor` child with `XRPokeInteractor` (Enable UI Interaction = ON). Set each interactor's **Poke Point** to the matching index fingertip bone (`hands:b_r_index3` / `hands:b_l_index3`); tune `pokeDepth`/`pokeWidth` for finger size. Optionally add a small visual at the fingertip.
- **Assigned role:** developer
- **Dependencies:** Step 0
- **Parallelizable:** Yes (with Step 1, 2)

### Step 4 — Build the tutorial UI layout
- **Description:** In `TipsCanvas`, add a background panel, Title (TMP), page indicator (TMP), Body (TMP, reusing/replacing the existing `Text (TMP)`), and three poke-sized Buttons (Back, Close, Next). Optionally add `XRPokeFollowAffordance` to buttons.
- **Assigned role:** developer
- **Dependencies:** Step 2
- **Parallelizable:** No

### Step 5 — Create TutorialPanelController script
- **Description:** Add `Assets/UAS/scripts/TutorialPanelController.cs` (see sketch). Prefill `pages` with the three content pages from current tips. Attach to `TipsCanvas`; assign body/indicator/buttons/panelRoot; hook Button `onClick` → `Next/Back/Close`.
- **Assigned role:** developer
- **Dependencies:** Step 4
- **Parallelizable:** No

### Step 6 — Update VRHUDController (freeze on open)
- **Description:** Modify `VRHUDController.cs` to position the panel once in front of the player on toggle-ON and stop continuous following, while keeping the toggle open/close behavior. Verify `tipsUI` still points at `TipsCanvas`.
- **Assigned role:** developer
- **Dependencies:** Step 0
- **Parallelizable:** Yes (with Steps 1–5, but test together in Step 7)

### Step 7 — Integration wiring & cleanup
- **Description:** Confirm references: `VRHUDController.tipsUI` = TipsCanvas; `TutorialPanelController.panelRoot` = TipsCanvas; buttons wired; single active input module in scene; no leftover `InputSystemUIInputModule`. Ensure no console errors on enter Play.
- **Assigned role:** developer
- **Dependencies:** Steps 1–6
- **Parallelizable:** No

---

# Verification & Testing

**Editor checks**
- Only **one** BaseInputModule active in scene (the `XRUIInputModule`); no `InputSystemUIInputModule` remains.
- `TipsCanvas` has `TrackedDeviceGraphicRaycaster` (not plain `GraphicRaycaster`).
- Each hand has an `XRPokeInteractor` with Enable UI Interaction ON and a valid Poke Point.
- No compile errors (Console clean).

**Play-mode (XR Device Simulator / headset)**
- Press toggle → panel appears in front and **stays still** (does not chase the head).
- Move a hand's fingertip into a button → button shows hover/press; **poke triggers** Next/Back/Close.
- Next advances pages; Back returns; Back disabled on page 1; Next disabled on last page; page indicator updates.
- Close hides the panel; toggle re-opens it at page 1 in front of the player.

**Edge cases**
- Rapid poking doesn't double-trigger (rely on Button click threshold).
- Toggling off mid-tutorial then on resets to page 1 (via `OnEnable`).
- Both hands can poke; no conflict with NearFar/Teleport ray interactors.
- Verify behavior is identical in whichever scene (Step 0) the changes were applied to.
