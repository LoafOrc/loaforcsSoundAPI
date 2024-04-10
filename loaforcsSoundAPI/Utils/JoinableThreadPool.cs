using Discord;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using UnityEngine;

namespace loaforcsSoundAPI.Utils;

// if theres a better way to do this do help, idk what im doing in c#
// we don't really need this to be always ready to go, just during loading the replacers so i didn't bother implemting min threads
internal class JoinableThreadPool {
    readonly ConcurrentQueue<Action> ActionQueue = new ConcurrentQueue<Action>();

    const int DEFAULT_MAX = 4;
    int max;
    Stopwatch timer;

    public void Queue(Action action) {
        ActionQueue.Enqueue(action);
    }

    public void Start() {
        timer = Stopwatch.StartNew();

        for(int i = 0; i < Mathf.Min(max, ActionQueue.Count); i++) {
            new Thread(new ThreadStart(RunThroughQueue)).Start();
            Thread.Sleep(10);

            SoundPlugin.logger.LogDebug("[Multithreading] Started thread " + i + "/" + max);
        }
    }

    public void Join() {
        SoundPlugin.logger.LogDebug("[Multithreading] Joined JoinableThreadPool.");
        RunThroughQueue();
    }

    void RunThroughQueue() {
        while(ActionQueue.TryDequeue(out Action action)) {

            try {
                action();
            } catch(Exception ex) {
                SoundPlugin.logger.LogError(ex);
            }

            Thread.Yield();
            
            SoundPlugin.logger.LogDebug("[Multithreading] Finished processing an action.");
        }
    }

    public JoinableThreadPool() {
        max = DEFAULT_MAX;
    }

    public JoinableThreadPool(int max) {
        this.max = max;
    }
}
