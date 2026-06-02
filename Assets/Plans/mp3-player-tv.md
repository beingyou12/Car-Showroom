# Project Overview
- Game Title: VR Hotel Room (UAS 1)
- High-Level Concept: An interactive hotel room scene where the user can control devices using a remote.
- Players: Single player (VR)
- Target Platform: Android (Meta Quest)
- Render Pipeline: URP (PC_RPAsset suggests URP or high-fidelity, but project settings say PC_RPAsset, and Explorer mentioned Built-In/Hotel Room. I will stick to the active rendering pipeline context.)
- Screen Orientation: Landscape (VR)

# Game Mechanics
## Core Gameplay Loop
- Exploration of the VR room.
- Interaction with objects (Remote, TV, Speaker).
- Media playback control.

## Controls and Input Methods
- XR Interaction Toolkit (Grab and Activate).
- Remote trigger (XR Activation) to cycle music tracks.

# UI
- **TV Viewer**: A World Space Canvas attached to the TV screen.
- **Header**: "Music Player"
- **Now Playing**: Text displaying the current track name.
- **Track List**: A list of available songs.
- **Visual Feedback**: The screen turns "on" (Canvas enabled) when the remote is activated.

# Key Asset & Context
- **Scripts**:
    - `MusicPlayerController.cs`: New script to manage the MP3 player state and UI.
    - `PlaySoundsFromList.cs`: Existing script on the speaker to handle the playlist.
- **Objects**:
    - `Stereo`: Holds the `AudioSource` and `PlaySoundsFromList`.
    - `TV/screen`: Holds the `Canvas` and `MusicPlayerController`.
    - `Remote`: XRGrabInteractable to trigger the music player.

# Implementation Steps
1. **Prepare Speaker (Stereo)**:
    - Locate the `Stereo` GameObject in `Assets/UAS/uas 1.unity`.
    - Add the `PlaySoundsFromList` component to it.
    - Populate the `audioClips` list with the 4 music tracks found in the project.
    - Set `shouldLoop` to true.
    - Assigned role: developer
    - Dependencies: None

2. **Modify `PlaySoundsFromList.cs`**:
    - Add a public property `public int CurrentIndex => index;` to allow the UI to know which song is playing.
    - Assigned role: developer
    - Dependencies: None

3. **Create `MusicPlayerController.cs`**:
    - Implement a script that references `PlaySoundsFromList`.
    - Handle `NextTrack()` logic (Power on if off, then cycle).
    - Update the TV Canvas UI (Text components) with song names.
    - Assigned role: developer
    - Dependencies: Step 2

4. **Setup TV UI (screen)**:
    - Locate `TV/screen` in the scene.
    - Disable or Remove `PlayVideo` and `VideoPlayer` components.
    - Create a **World Space Canvas** child on `screen`, scaled to fit the TV face.
    - Add an Image background and TextMeshPro elements for "Now Playing" and "Song List".
    - Attach the `MusicPlayerController` script to the `screen` or the Canvas.
    - Assigned role: developer
    - Dependencies: Step 3

5. **Wire the Remote**:
    - Locate the `Remote` GameObject.
    - In the `XRGrabInteractable` component, find the `Activated` event.
    - Remove the existing call to `PlayVideo.TogglePlayPause`.
    - Add a new call to `MusicPlayerController.NextTrack()` on the `screen` object.
    - Assigned role: developer
    - Dependencies: Step 4

# Verification & Testing
- **Play Mode Test**:
    - Grab the remote in VR.
    - Press the trigger (Activate).
    - **Verify**: The TV screen shows the "Music Player" UI.
    - **Verify**: Music starts playing from the `Stereo` (speaker).
    - **Verify**: Pressing trigger again changes the song and updates the "Now Playing" text.
    - **Verify**: The audio clip on the `Stereo` changes accordingly.
