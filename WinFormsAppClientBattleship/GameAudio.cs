using System.Media;
using System.Runtime.InteropServices;

namespace WinFormsAppClientBattleship
{
    internal static class GameAudio
    {
        private static readonly Random Random = new();
        private static readonly object Sync = new();
        private static SoundPlayer? _startupPlayer;
        private static SoundPlayer? _hitPlayer;
        private static CancellationTokenSource? _startupCts;
        private static bool _hitSoundsEnabled;

        public static bool IsMuted { get; private set; }

        [DllImport("winmm.dll", CharSet = CharSet.Unicode)]
        private static extern bool PlaySound(string? sound, IntPtr module, uint flags);

        private const uint SND_ASYNC = 0x0001;
        private const uint SND_NODEFAULT = 0x0002;
        private const uint SND_FILENAME = 0x00020000;

        public static void SetMuted(bool muted)
        {
            IsMuted = muted;
            if (muted)
                StopCurrentPlayback();
        }

        public static void PlayStartupAudio()
        {
            lock (Sync)
            {
                var path = AssetPaths.AudioPath("abs1.wav");
                if (!File.Exists(path))
                {
                    _hitSoundsEnabled = true;
                    return;
                }

                _startupCts?.Cancel();
                _startupCts = new CancellationTokenSource();
                var token = _startupCts.Token;

                _startupPlayer?.Stop();
                _startupPlayer?.Dispose();
                _startupPlayer = new SoundPlayer(path);

                if (!IsMuted)
                    _startupPlayer.Play();

                int durationMs = WavHelper.GetDurationMilliseconds(path) ?? 5000;
                _ = EnableHitSoundsAfterDelayAsync(durationMs, token);
            }
        }

        public static void StopAll()
        {
            lock (Sync)
            {
                _startupCts?.Cancel();
                _startupCts = null;
                StopCurrentPlayback();
            }
        }

        public static async Task PlayExitSoundAsync()
        {
            if (IsMuted)
                return;

            var path = AssetPaths.AudioPath("iesire.wav");
            if (!File.Exists(path))
                return;

            PlaySound(path, IntPtr.Zero, SND_ASYNC | SND_FILENAME | SND_NODEFAULT);

            int durationMs = WavHelper.GetDurationMilliseconds(path) ?? 3000;
            durationMs = Math.Clamp(durationMs + 150, 150, 30_000);
            await Task.Delay(durationMs).ConfigureAwait(true);
        }

        public static void PlayHitSound()
        {
            if (IsMuted || !_hitSoundsEnabled)
                return;

            int index = Random.Next(1, 13);
            var path = AssetPaths.AudioPath($"hbs{index}.wav");
            if (!File.Exists(path))
                return;

            lock (Sync)
            {
                _hitPlayer?.Stop();
                _hitPlayer?.Dispose();
                _hitPlayer = new SoundPlayer(path);
                _hitPlayer.Play();
            }
        }

        private static void StopCurrentPlayback()
        {
            _hitPlayer?.Stop();
            _hitPlayer?.Dispose();
            _hitPlayer = null;

            _startupPlayer?.Stop();
            _startupPlayer?.Dispose();
            _startupPlayer = null;

            PlaySound(null, IntPtr.Zero, 0);
        }

        private static async Task EnableHitSoundsAfterDelayAsync(int delayMs, CancellationToken token)
        {
            try
            {
                await Task.Delay(delayMs, token).ConfigureAwait(false);
                _hitSoundsEnabled = true;
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}
