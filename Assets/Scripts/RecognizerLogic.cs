using Common;
using Engine;
using Mediapipe.Tasks.Vision.HandLandmarker;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class RecognizerLogic : MonoBehaviour
{
    public SimpleExecutionEngine engine;
    public WordSpawnManager spawner;
    public BossAttackManager bossAttacks;
    private bool init;
    public int frames = 30;
    public float threshold = 0.5f;

    void Awake()
    {
        if (engine == null) engine = FindFirstObjectByType<SimpleExecutionEngine>();

        if (spawner == null) spawner = FindFirstObjectByType<WordSpawnManager>();

        if (bossAttacks == null) bossAttacks = FindFirstObjectByType<BossAttackManager>();
    }
    void Update()
    {
        if (!init)
        {
            engine.recognizer.AddCallback("print", sign =>
            {
                Debug.Log("Got sign " + sign);

                if (bossAttacks != null) bossAttacks.EnqueueRecognizedSign(sign);
                if (spawner != null) spawner.EnqueueRecognizedSign(sign);
            });
            engine.recognizer.outputFilters.Clear();
            engine.recognizer.outputFilters.Add(new Thresholder<string>(threshold));
            engine.recognizer.outputFilters.Add(new FocusSublistFilter<string>(
                new List<string>(LevelManager.Instance.SelectedWords)
            ));
            engine.buffer.trigger = new CapacityFullTrigger<HandLandmarkerResult>(frames);
            init = true;
        }
    }
}
