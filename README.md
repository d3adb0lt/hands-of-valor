# Getting Started

## Project Overview

This is a Unity game prototype where the player fights a boss by performing sign language gestures recognized by a sign language recognizer gesture toolkit. During the fight, several word slots appear on screen and the player must sign the displayed words before they expire.

- A separate dodge word appears in the top right on its own timer and must be prioritized.
- Sometimes a critical word spawns that expires faster, shows as red text, and deals extra damage.
- Boss health decreases when the player correctly signs a normal slot word, and the fight ends when boss HP reaches zero.
- Player HP decreases if the player fails to sign the dodge word before it expires. If player HP reaches 0, the fight ends in defeat.
- If the player manages to sign the dodge word before it expires, their stamina will decrease. Once stamina reaches 0, they will no longer be able to dodge and subsequent boss attacks will deal damage regardless of whether or not the player signs the dodge word in time.
- A word that the boss can "parry" also has a chance to appear. If the player signs that word, they will lose stamina if they have it, or they will lose HP.

## How to Contribute

1. Find the Hands of Valor Kanban GitHub project.
2. Find a task you'd like to work on from the Ready section, drag it to In Progress, assign yourself to it, and put out a message in the Hands of Valor channel saying that you have chosen a task with the link to it. Make sure to tag the leads in this message.
3. Pull the latest changes from main: `git pull`
4. Create a new branch off of main in this format: `git checkout -b <your_github_username>-<issue_number>` (Ensure that this branch is linked to the task in the Hands of Valor Kanban)
5. Make your changes and test.
6. Add all of your changes: `git add .`
7. Create a commit: `git commit -m "short description of your changes"`
8. Push your changes: `git push` (The first time pushing will be `git push --set-upstream origin <your_branch>`. Ensure any merge conflicts are resolved)
9. From GitHub, open a PR. Write a more exhaustive description of your changes and what you have tested. Include a screen recording of your changes in action! Also, drag your Kanban task into In Review. (Ensure any merge conflicts are resolved)
10. Submit a link to your PR in the Hands of Valor channel and tag the leads. Merge your PR once you have their approval. After merging, move your Kanban task into Done.

## Setup and Run Instructions

### Prerequisites

1. Clone the repo.
2. Install Unity Hub.
3. Install the Unity Editor version required by the project. Open `ProjectSettings/ProjectVersion.txt` in the repo to see the exact Unity version string this project expects.
4. Install Visual Studio.
5. Follow the SLRTk setup instructions.

### Open the Project

1. In Unity Hub, click **Add** and select the repository folder.
2. Open the project using the Unity version from `ProjectSettings/ProjectVersion.txt`.
3. Use a phone simulator such as `Apple iPhone 13 Pro`.

### Run the Game in Editor

1. Open the scene named `StartScreenScene`.
2. Press **Play**.
3. Click the **Start** button.
4. The game loads `BossTemplateScene` and starts the boss fight automatically.

### Build Settings

1. Go to **File** then **Build Profiles**.
2. Select the iOS or Android profile under **Build Profiles** (not **Platforms**).
3. Click on **Switch Profile**.
4. Click **Build**.

## Repository Map

### Scenes

- **`Assets/Scenes/StartScreenScene.unity`** — Start screen UI. Includes the sign selection list, filter input, preview overlay, and the Start button.
- **`Assets/Scenes/BossTemplateScene.unity`** — Boss fight template scene. Contains the boss, word slots UI, dodge slot UI, and managers. `SimpleSLREngine` is present here.

### Data and Content

- **`Assets/SLR-GTk/Package/Reference Videos/`** — MP4 reference videos. These are imported as Unity VideoClip assets and are used by the start screen preview.
- **`Assets/Data/SignLibrary.asset`** — ScriptableObject that contains the sign entries used by the start screen list. Each entry includes a word name and a VideoClip reference.
- **`Assets/Scripts/SignLibrary.cs`** and **`Assets/Scripts/SignEntry.cs`** — Defines the data model for sign entries that the start screen uses.

### Start Screen Scripts

- **`Assets/Scripts/StartScreenController.cs`** — Handles the Start button. Reads the currently selected signs from `StartScreenListPopulator`, passes them into `LevelManager`, then loads the boss scene. Start does nothing if no words are selected.
- **`Assets/Scripts/StartScreenListPopulator.cs`** — Builds the scroll list from `SignLibrary`. Supports filtering via the text input, selection via checkboxes, reset selection, and video preview when clicking the word button. Also rehydrates selection from `LevelManager` when returning from the boss scene so previously selected words remain selected.
- **`Assets/Scripts/SignRowToggle.cs`** — Row prefab controller for each sign in the list. Connects the Toggle and the word button click to the list populator. This script is attached to the `SignRowToggle` prefab.

### Core Gameplay Scripts

- **`Assets/Scripts/LevelManager.cs`** — Singleton that persists across scenes. Receives the player selected word list from the start screen. Builds and refills the shuffled fight queue using bags so all words appear before repeats. Starts and stops the fight. Stop loads `StartScreenScene`.
- **`Assets/Scripts/WordSpawnManager.cs`** — Controls the main word slots. Spawns rounds, handles expiration timers, and clears words when recognized. Implements critical word chance and shorter lifetime. Applies boss damage when a normal slot is signed successfully. Enforces dodge priority so the dodge word clears first if duplicated.
- **`Assets/Scripts/BossAttackManager.cs`** — Controls the dodge slot. Spawns dodge words at random intervals with an initial delay. Clears dodge words when recognized.
- **`Assets/Scripts/WordSlot.cs`** — UI slot for normal words. Tracks current word and critical state. Sets text and changes color to red for critical words.
- **`Assets/Scripts/DodgeSlot.cs`** — UI slot for dodge words. Tracks current word and shows or clears it.
- **`Assets/Scripts/BossHealth.cs`** — Tracks boss HP. On reaching 0, stops the fight and transitions back to the start screen.

### If You Are Looking for X, Go Here

| Looking for | Go here |
| --- | --- |
| Start screen selection list and filtering | `Assets/Scripts/StartScreenListPopulator.cs` |
| Individual list row behavior | `Assets/Scripts/SignRowToggle.cs` |
| Sign data source and entry definitions | `Assets/Data/SignLibrary.asset`, `Assets/Scripts/SignLibrary.cs`, `Assets/Scripts/SignEntry.cs` |
| Video preview playback | `Assets/Scripts/StartScreenListPopulator.cs` and the VideoPlayer in `StartScreenScene` |
| Word spawning and timers | `Assets/Scripts/WordSpawnManager.cs` |
| Dodge spawning and timing ranges | `Assets/Scripts/BossAttackManager.cs` |
| Boss HP and win condition | `Assets/Scripts/BossHealth.cs` |
| Word queue creation and refill logic | `Assets/Scripts/LevelManager.cs` |
| Start button flow and scene load | `Assets/Scripts/StartScreenController.cs` |
| UI behavior of a slot | `Assets/Scripts/WordSlot.cs`, `Assets/Scripts/DodgeSlot.cs` |

## Toolkit Integration Summary

### Where the Sign Language Recognizer Gesture Toolkit Lives

The recognizer toolkit is integrated through the engine and recognizer components, referenced from the recognizer logic script. In code, this is represented by a `SimpleExecutionEngine` reference and its `engine.recognizer` pipeline, plus a recognizer callback registration that receives a recognized sign string.

### How Recognizer Output Flows into Gameplay

High level flow:

1. The recognizer produces an output string, for example `yellow`.
2. `RecognizerLogic` receives this string in a callback.
3. `RecognizerLogic` forwards the sign to gameplay managers. It enqueues the sign into `BossAttackManager` and `WordSpawnManager`.
4. Each manager processes those queued signs on the Unity main thread in `Update`:
   - `BossAttackManager` tries to clear the dodge word first.
   - `WordSpawnManager` then tries to clear a matching normal slot.
5. When a normal slot is cleared, boss damage is applied via `BossHealth.TakeDamage`.

### Where This Happens in Code

- **Recognizer callback forwarding:** `Assets/Scripts/RecognizerLogic.cs`
- **Thread safe sign inbox and processing:**
  - `Assets/Scripts/WordSpawnManager.cs` in `EnqueueRecognizedSign` and `Update`
  - `Assets/Scripts/BossAttackManager.cs` in `EnqueueRecognizedSign` and `Update`
- **Dodge priority check:** `Assets/Scripts/WordSpawnManager.cs` in `HandleRecognizedOnMainThread` calling `BossAttackManager.TryConsumeDodgeOnMainThread`

## Common Fixes and Troubleshooting

1. If words do not spawn, ensure you have added words in the Level Manager inspector.
2. If the scene does not transition to the boss fight scene when start is clicked, ensure that the scene order is Start Screen at 0 and Boss Fight at 1 in build settings.
3. If the camera does not appear, restart Unity.
4. If there are numerous console errors when starting the game from the boss fight scene, click stop and then start the game from the start screen.
5. If words spawn too fast or too slow, edit the timers in the Word Spawn Manager inspector.
