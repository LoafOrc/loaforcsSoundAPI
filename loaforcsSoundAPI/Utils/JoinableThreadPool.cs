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
    Thread threadScheduler;

    const int DEFAULT_MAX = 4;
    int active = 0;
    int max;

    public void Queue(Action action) {
        ActionQueue.Enqueue(action);
    }

    public void Start() {
        threadScheduler = new Thread(new ThreadStart(() => {
            SoundPlugin.logger.LogInfo("[Multithreading] Multithreading started, " + ActionQueue.Count + " actions are queued.");
            Stopwatch threadPoolTime = Stopwatch.StartNew();

            while(ActionQueue.Count > 0 || Volatile.Read(ref active) != 0) {
                if(active >= max) continue;

                if(!ActionQueue.TryDequeue(out Action action)) {
                    continue;
                }

                Interlocked.Increment(ref active); // this seems bad
                Thread thread = new Thread(new ThreadStart(() => {
                    SoundPlugin.logger.LogDebug("[Multithreading] Queued a new thread, at " + active + " out of " + max);

                    try {
                        action();
                    } catch (Exception ex) {
                        SoundPlugin.logger.LogError(ex);
                    }
                    Interlocked.Decrement(ref active);
                    SoundPlugin.logger.LogDebug("[Multithreading] Thread finished. " + ActionQueue.Count + " actions are left.");
                }));
                thread.Start();

                Thread.Yield();
                Thread.Sleep(10);
            }

            threadPoolTime.Stop();
            SoundPlugin.logger.LogInfo("[Multithreading] Multithreading finished. JoinableThreadPool ran for " + threadPoolTime.ElapsedMilliseconds + " ms.");
        }));
        threadScheduler.Start();
    }

    public void Join() {
        SoundPlugin.logger.LogDebug("[Multithreading] Joined JoinableThreadPool.");
        threadScheduler.Join();
    }

    public JoinableThreadPool() {
        max = DEFAULT_MAX;
    }

    public JoinableThreadPool(int max) {
        this.max = max;
    }
}
