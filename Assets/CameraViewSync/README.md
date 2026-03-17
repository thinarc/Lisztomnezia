# CamViewSync – Camera Piloting for Unity Scene View

CamViewSync is a lightweight Unity Editor extension that lets you pilot any selected camera using the Scene View. This gives you intuitive control over camera positioning, inspired by Unreal Engine's camera piloting system.


## Features

- Sync a selected in-scene Camera to the Scene View camera 
- Real-time transform updates while you move around
- Undo/redo support for camera movement
- State persists between domain reloads and editor restarts

## How to Use

First select a GameObject with a `Camera` component, then there are 2 options.

### Option 1: Using the Toolbar Overlay (Recommended)

1. Open the **Scene View** window.
2. Click the CVS button on Unity Scene View's overlay toolbar to open the CVS overlay toolbar.
   - *Or click the **Overlay menu** (the vertical dots) button in the top-left of the Scene View tab, and enable "Camera View Sync" to open the toolbar.*
3. In the opened toolbar, click the 🎥 **Start Piloting** button to begin syncing with the selected camera.
4. To stop piloting, click the 🛑 **Stop Piloting** button.

> The toolbar icon can also be pinned for quick access, and will reflect the current piloting state.

### Option 2: Using the Menu
 
1. Go to `Tools → Camera View Sync → Start Piloting`.
2. Move around in the Scene view, and the selected camera will follow.
3. Use `Tools → Camera View Sync → Stop Piloting` to stop.

## Compatibility

- Unity **6.1**
- Editor-only (no runtime dependencies)
- No additional input system required

## Known Limitations

- Piloting works only in the Scene View (not Game View or Play mode).
- Does not support piloting multiple cameras at once.
- Transform syncing only happens during editor camera movement.
