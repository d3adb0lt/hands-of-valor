GETTING STARTED

PROJECT OVERVIEW
This is a Unity game prototype where the player fights a boss by performing sign language gestures recognized by a sign language recognizer gesture toolkit. During the fight, several word slots appear on screen and the player must sign the displayed words before they expire. A separate dodge word appears in the top right on its own timer and must be prioritized. Sometimes a critical word spawns that expires faster, shows as red text, and deals extra damage. Boss health decreases when the player correctly signs a normal slot word, and the fight ends when boss HP reaches zero. Player HP decreases if the player fails to sign the dodge word before it expires. If player HP reaches 0, the fight ends in defeat. If the player manages to sign the dodge word before it expires, their stamina will decrease. Once stamina reaches 0, they will no longer be able to dodge and subsequent boss attacks will deal damage regardless of whether or not the player signs the dodge word in time.

HOW TO CONTRIBUTE

1. Find the Hands of Valor Kanban GitHub project.
2. Find a task you'd like to work on from the Ready section, drag it to In Progress, assign yourself to it, and put out a message in the Hands of Valor channel saying that you have chosen a task with the link to it. Make sure to tag the leads in this message.
4. Pull the latest changes from main: git pull
5. Create a new branch off of main in this format: git checkout -b <your_github_username>-<issue_number> (Ensure that this branch is linked to the task in the Hands of Valor Kanban)
6. Make your changes and test.
7. Create a commit with all of your changes: git commit -a -m "short description of your changes"
8. Push your changes: git push (Ensure any merge conflicts are resolved)
9. From GitHub, open a PR. Write a more exhaustive description of your changes and what you have tested. Include a screen recording of your changes in action! Also, drag your Kanban task into In Review. (Ensure any merge conflicts are resolved)
10. Submit a link to your PR in the Hands of Valor channel and tag the leads. Merge your PR once you have their approval. After merging, move your Kanban task into Done.

SETUP AND RUN INSTRUCTIONS

Prerequisites 
1. Clone the Repo.
2. Install Unity Hub.
3. Install the Unity Editor version required by the project.
   Open ProjectSettings/ProjectVersion.txt in the repo to see the exact Unity version string this project expects.
4. Install Visual Studio.
5. Follow the SLRTk setup instructions.

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
