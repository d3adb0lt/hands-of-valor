GETTING STARTED

PROJECT OVERVIEW
This is a Unity game prototype where the player fights a boss by performing sign language gestures recognized by a sign language recognizer gesture toolkit. The game is currently being built and tested in the Unity Editor and targets desktop for now. During the fight, several word slots appear on screen and the player must sign the displayed words before they expire. A separate dodge word appears in the top right on its own timer and must be prioritized. Some rounds can spawn a critical word that expires faster, shows as red text, and deals extra damage. Boss health decreases when the player correctly signs a normal slot word, and the fight ends when boss HP reaches zero. Current state is playable end to end with a Start Screen button, a boss fight loop, dodge spawns, critical words, and a scene transition back to the start screen on victory. Player HP and Stamina are currently not implemented.

SETUP AND RUN INSTRUCTIONS

Prerequisites
1. Install Unity Hub.
2. Install the Unity Editor version required by the project.
   Open ProjectSettings/ProjectVersion.txt in the repo to see the exact Unity version string this project expects.
3. Install Visual Studio.
4. Follow the SLRTk setup instructions.

Open the project
1. In Unity Hub, click Add and select the repository folder.
2. Open the project using the Unity version from ProjectSettings/ProjectVersion.txt.

Run the game in Editor
1. Open the scene named StartScreenScene.
2. Press Play.
3. Click the Start button.
4. The game loads BossTemplateScene and starts the boss fight automatically.

Build settings
1. Go to File then Build Settings.
2. Ensure these scenes are added to the build list in this order.
   StartScreenScene
   BossTemplateScene
3. If a scene fails to load at runtime, it usually means the scene is not in the build list or the name in code does not match.

REPOSITORY MAP

Scenes
Assets/Scenes/StartScreenScene.unity
Simple start screen UI with one button.

Assets/Scenes/BossTemplateScene.unity
Boss fight template scene. Contains the word slots UI, dodge slot UI, and managers.

Core gameplay scripts
Assets/Scripts/LevelManager.cs
Singleton that persists across scenes.
Receives the player selected word list.
Builds and refills the shuffled fight queue using bags so all words appear before repeats.
Starts and stops the fight. Stop loads StartScreenScene.

Assets/Scripts/WordSpawnManager.cs
Controls the main word slots.
Spawns rounds, handles expiration timers, and clears words when recognized.
Implements critical word chance and shorter lifetime.
Applies boss damage when a normal slot is signed successfully.
Enforces dodge priority so the dodge word clears first if duplicated.

Assets/Scripts/BossAttackManager.cs
Controls the dodge slot.
Spawns dodge words at random intervals, with an initial delay.
Clears dodge words when recognized.

Assets/Scripts/WordSlot.cs
UI slot for normal words.
Tracks current word and critical state.
Sets text and changes color to red for critical.

Assets/Scripts/DodgeSlot.cs
UI slot for dodge words.
Tracks current word and shows or clears it.

Assets/Scripts/BossHealth.cs
Tracks boss HP.
On reaching 0, the fight stops and transitions back to start screen.

Start screen script
Assets/Scripts/StartScreenController.cs
Handles the Start button.
Sets the hardcoded word list into LevelManager for now.
Loads the boss scene and starts the fight after the boss scene loads.

If you are looking for X, go here
Word spawning and timers: Assets/Scripts/WordSpawnManager.cs
Dodge spawning and timing ranges: Assets/Scripts/BossAttackManager.cs
Boss HP and win condition: Assets/Scripts/BossHealth.cs
Word queue creation and refill logic: Assets/Scripts/LevelManager.cs
Start button flow and scene load: Assets/Scripts/StartScreenController.cs
UI behavior of a slot: Assets/Scripts/WordSlot.cs and Assets/Scripts/DodgeSlot.cs

TOOLKIT INTEGRATION SUMMARY

Where the Sign Language Recognizer Gesture Toolkit lives
The recognizer toolkit is integrated through the engine and recognizer components, referenced from the recognizer logic script. In code, this is represented by a SimpleExecutionEngine reference and its engine.recognizer pipeline plus a recognizer callback registration that receives a recognized sign string.

How recognizer output flows into gameplay
High level flow
1. The recognizer produces an output string, for example yellow.
2. RecognizerLogic receives this string in a callback.
3. RecognizerLogic forwards the sign to gameplay managers.
   It enqueues the sign into BossAttackManager and WordSpawnManager.
4. Each manager processes those queued signs on the Unity main thread in Update.
   BossAttackManager tries to clear the dodge word first.
   WordSpawnManager then tries to clear a matching normal slot.
5. When a normal slot is cleared, boss damage is applied via BossHealth.TakeDamage.

Where this happens in code
Recognizer callback forwarding: Assets/Scripts/RecognizerLogic.cs
Thread safe sign inbox and processing
Assets/Scripts/WordSpawnManager.cs in EnqueueRecognizedSign and Update
Assets/Scripts/BossAttackManager.cs in EnqueueRecognizedSign and Update
Dodge priority check
Assets/Scripts/WordSpawnManager.cs in HandleRecognizedOnMainThread calling BossAttackManager.TryConsumeDodgeOnMainThread

COMMON FIXES AND TROUBLESHOOTING

1. If words do not spawn, ensure you have added words in the Level Mananager inspector.
2. If the scene does not transition to the boss fight scene when start is clicked, ensure that the scene order is Start Screen at 0 and Boss Fight at 1 in build settings.
3. If the camera does not appear, restart unity.
4. If the are numerous console errors when starting the game from the boss fight scene, click stop and then start the game from the start screen.
5. If words spawn too fast or too slow, edit the timers in the Word Spawn Manager inspector.


FIRST SAFE TASK, TINY CHANGE

Task: adjust the critical spawn chance
Goal: Make critical words appear more or less often.

Files to change
Assets/Scripts/WordSpawnManager.cs

What to change
1. Find the serialized field criticalChance.
2. Change it from 0.2f to a new value.
Example: set to 0.3f for 30 percent.

How to verify
1. Run StartScreenScene and start the fight.
2. Watch for red critical words to appear more frequently.

Alternative tiny task
Edit the hardcoded word list used by the start button.
File: Assets/Scripts/StartScreenController.cs
Change the hardcodedWords list to add or remove entries like down or ear.
