
using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;

namespace YMH219_SpeechApp.Services
{
    public class SpeechSynthesisService : IDisposable
    {
        private readonly SpeechSynthesizer synthesizer;

        public event EventHandler<SpeakCompletedEventArgs> SpeakCompleted;

        public SpeechSynthesisService()
        {
            synthesizer = new SpeechSynthesizer();

            synthesizer.SetOutputToDefaultAudioDevice();

            synthesizer.SpeakCompleted += (sender, e) =>
            {
                SpeakCompleted?.Invoke(this, e);
            };
        }

        // Get available voices
        public List<VoiceInfo> GetAvailableVoices()
        {
            return synthesizer.GetInstalledVoices()
                .Where(voice => voice.Enabled)
                .Select(voice => voice.VoiceInfo)
                .ToList();
        }

        // Select voice
        public void SelectVoice(string voiceName)
        {
            synthesizer.SelectVoice(voiceName);
        }

        // Speak text
        public void Speak(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            synthesizer.SpeakAsyncCancelAll();
            synthesizer.SpeakAsync(text);
        }

        // Pause speech
        public void Pause()
        {
            if (synthesizer.State == SynthesizerState.Speaking)
            {
                synthesizer.Pause();
            }
        }

        // Resume speech
        public void Resume()
        {
            if (synthesizer.State == SynthesizerState.Paused)
            {
                synthesizer.Resume();
            }
        }

        // Stop speech
        public void Stop()
        {
            synthesizer.SpeakAsyncCancelAll();
        }

        // Get current speech state
        public SynthesizerState GetState()
        {
            return synthesizer.State;
        }

        // Release resources
        public void Dispose()
        {
            synthesizer.SpeakAsyncCancelAll();
            synthesizer.Dispose();
        }
    }
}
