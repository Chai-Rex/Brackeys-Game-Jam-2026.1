using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;

public class VideoCanvas : MonoBehaviour {

    [SerializeField] private VideoPlayer _videoPlayer;

    public void PlayVideo(Action i_callback) {

        PlayVideo(_videoPlayer, i_callback);
    }

    private void PlayVideo(VideoPlayer player, Action onFinished) {
        if (player == null) {
            Debug.LogError("VideoPlayer is null.");
            return;
        }

        // Unsubscribe first to avoid duplicate calls
        player.loopPointReached -= HandleVideoFinished;

        void HandleVideoFinished(VideoPlayer vp) {
            vp.loopPointReached -= HandleVideoFinished; // cleanup
            onFinished?.Invoke();
        }

        player.loopPointReached += HandleVideoFinished;

        player.Play();
    }
}


public static class VideoPlayerExtensions {
    public static Task PlayAndWaitForEnd(this VideoPlayer player) {
        var tcs = new TaskCompletionSource<bool>();

        if (player == null) {
            tcs.SetException(new System.NullReferenceException("VideoPlayer is null."));
            return tcs.Task;
        }

        player.isLooping = false;

        void OnFinished(VideoPlayer vp) {
            vp.loopPointReached -= OnFinished;
            tcs.TrySetResult(true);
        }

        player.loopPointReached += OnFinished;
        player.Play();

        return tcs.Task;
    }
}