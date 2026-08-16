using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;
using loaforcsSoundAPI.Core;
using loaforcsSoundAPI.SoundPacks.Data;
using UnityEngine;
using UnityEngine.Networking;

namespace loaforcsSoundAPI.SoundPacks.AudioClipLoading;

// TODO: fix me, this is not good logic.
// im going to be so real i no longer understand whats happening here
// I think it's better nooooooooow, probably, maybe...
class MultithreadedAudioClipLoader : IAudioClipLoader {
	static volatile int _activeThreads, _clipsGenerated;

	readonly ConcurrentBag<Exception> _threadPoolExceptions = [];
	readonly ConcurrentQueue<LoadSoundOperation> _queuedOperations = [];

	bool _threadsShouldExit = false, _displayedHalfwayMessage = false;

	public int Count => _queuedOperations.Count;
	int totalClips;

	public void LoadAllBlocking() {
		Stopwatch timer = Stopwatch.StartNew();

		loaforcsSoundAPI.Logger.LogInfo($"(Step 5) All file reads are done, waiting for the audio clips conversions.");

		totalClips = Count;
		for(int i = 0; i < Environment.ProcessorCount * 2; i++) { // Twice the number of CPU cores, instead of a fixed value.
			new Thread(threadIndex => {
				Interlocked.Increment(ref _activeThreads);
				Debuggers.SoundReplacementLoader?.Log($"active threads at {_activeThreads}");

				/* while(!_threadsShouldExit) { // Allowing Threads to start working immediately appears marginally faster.
					Thread.Yield();
				} */

				while(_queuedOperations.TryDequeue(out LoadSoundOperation operation)) {
					try {
						using UnityWebRequest webRequest = operation.WebRequest;
						while(webRequest.result is UnityWebRequest.Result.InProgress) { // Had it error out once due to requests not being ready yet (for some reason).
							Thread.Yield();
						}
						if(webRequest.result is not UnityWebRequest.Result.Success) return;
						using DownloadHandlerAudioClip downloadHandler = DownloadHandler.GetCheckedDownloader<DownloadHandlerAudioClip>(webRequest);
						downloadHandler.compressed = true;

						AudioClip clip = downloadHandler.audioClip;
						operation.Sound.Clip = clip;
						Interlocked.Increment(ref _clipsGenerated);
						Debuggers.SoundReplacementLoader?.Log($"clip #{_clipsGenerated} out of {totalClips} generated: {clip.name} on thread {threadIndex}");
						operation.IsDone = true;
					} catch(Exception exception) {
						_threadPoolExceptions.Add(exception);
					}
				}

				Interlocked.Decrement(ref _activeThreads);
			}).Start(i + 1);
		}

		// _threadsShouldExit = true;
		while(_activeThreads > 0) {
			if(!_displayedHalfwayMessage && Count < totalClips / 2) {
				_displayedHalfwayMessage = true;
				loaforcsSoundAPI.Logger.LogInfo($"(Step 5) Queued half of the needed operations!");
			}

			Thread.Yield();
		}

		loaforcsSoundAPI.Logger.LogInfo($"(Step 6) Took {timer.ElapsedMilliseconds}ms to finish loading audio clips from files");
		if(_threadPoolExceptions.Count != 0) {
			loaforcsSoundAPI.Logger.LogError($"(Step 6) {_threadPoolExceptions.Count} internal error(s) happened while loading:");
			foreach(Exception poolException in _threadPoolExceptions) {
				loaforcsSoundAPI.Logger.LogError(poolException.ToString());
			}
		}
	}

	public void Queue(SoundInstance sound) {
		_queuedOperations.Enqueue(StartWebRequestOperation(sound));
	}

	LoadSoundOperation StartWebRequestOperation(SoundInstance sound) {
		string fullPath = Path.Combine(sound.Pack.PackFolder, "sounds", sound.Sound);

		UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(
			fullPath,
			IAudioClipLoader.audioExtensions[Path.GetExtension(sound.Sound)]
		);

		return new LoadSoundOperation(sound, www.SendWebRequest());
	}
}