using NAudio.Wave;
using System;

namespace AudioPlayer.Models
{
    public class AudioPlayerModel
    {
        private WaveOutEvent _waveOut;
        private AudioFileReader _audioFileReader;
        private bool _isPlaying;

        public bool IsPlaying => _isPlaying;

        public void LoadFile(string filePath)
        {
            Stop();
            _audioFileReader = new AudioFileReader(filePath);
            _waveOut = new WaveOutEvent();
            _waveOut.Init(_audioFileReader);
        }

        public void PlayPause()
        {
            if (_waveOut == null) return;

            if (_isPlaying)
            {
                _waveOut.Pause();
                _isPlaying = false;
            }
            else
            {
                _waveOut.Play();
                _isPlaying = true;
            }
        }

        public void Stop()
        {
            if (_waveOut != null)
            {
                _waveOut.Stop();
                _waveOut.Dispose();
                _audioFileReader?.Dispose();
                _waveOut = null;
                _audioFileReader = null;
                _isPlaying = false;
            }
        }

        public void SetVolume(float volume)
        {
            if (_waveOut != null)
            {
                _waveOut.Volume = volume;
            }
        }

        public double GetCurrentTime()
        {
            return _audioFileReader?.CurrentTime.TotalSeconds ?? 0;
        }

        public double GetDuration()
        {
            return _audioFileReader?.TotalTime.TotalSeconds ?? 1;
        }
    }
}