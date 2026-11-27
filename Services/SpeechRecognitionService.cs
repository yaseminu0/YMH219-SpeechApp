
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Speech.Recognition;
using YMH219_SpeechApp.Models;

namespace YMH219_SpeechApp.Services
{
    public class SpeechRecognitionService : IDisposable
    {
        private SpeechRecognitionEngine recognizer;
        private bool isListening;
        private bool isDisposed;

        public event EventHandler<SpeechRecognizedEventArgs>
            SpeechRecognized;

        public event EventHandler<RecognizeCompletedEventArgs>
            RecognitionCompleted;

        public bool IsListening
        {
            get { return isListening; }
        }

        public SpeechRecognitionService()
        {
            var languages = GetAvailableLanguages();

            if (languages.Count > 0)
            {
                InitializeRecognizer(languages[0].CultureName);
            }
        }

        // Get installed recognition languages
        public List<SpeechLanguage> GetAvailableLanguages()
        {
            return SpeechRecognitionEngine.InstalledRecognizers()
                .Select(item => new SpeechLanguage(
                    item.Culture.DisplayName,
                    item.Culture.Name))
                .ToList();
        }

        // Initialize recognition engine
        private void InitializeRecognizer(string cultureName)
        {
            CultureInfo culture =
                CultureInfo.GetCultureInfo(cultureName);

            recognizer = new SpeechRecognitionEngine(culture);

            recognizer.LoadGrammar(new DictationGrammar());

            recognizer.SetInputToDefaultAudioDevice();

            recognizer.SpeechRecognized += (sender, e) =>
            {
                SpeechRecognized?.Invoke(this, e);
            };

            recognizer.RecognizeCompleted += (sender, e) =>
            {
                isListening = false;

                RecognitionCompleted?.Invoke(this, e);
            };
        }

        // Change recognition language
        public void SelectLanguage(string cultureName)
        {
            if (isListening)
            {
                throw new InvalidOperationException(
                    "Stop listening before changing the language.");
            }

            recognizer?.Dispose();
            recognizer = null;

            InitializeRecognizer(cultureName);
        }

        // Start listening
        public void StartListening()
        {
            if (isDisposed)
                throw new ObjectDisposedException(
                    nameof(SpeechRecognitionService));

            if (recognizer == null)
            {
                throw new InvalidOperationException(
                    "No recognition language is available.");
            }

            if (isListening)
                return;

            try
            {
                isListening = true;

                recognizer.RecognizeAsync(
                    RecognizeMode.Multiple);
            }
            catch
            {
                isListening = false;
                throw;
            }
        }

        // Stop listening
        public void StopListening()
        {
            if (recognizer == null || !isListening)
                return;

            recognizer.RecognizeAsyncCancel();
        }

        // Release resources
        public void Dispose()
        {
            if (isDisposed)
                return;

            isDisposed = true;
            isListening = false;

            if (recognizer != null)
            {
                try
                {
                    recognizer.RecognizeAsyncCancel();
                }
                catch
                {
                    // Recognition may already be stopped.
                }

                recognizer.Dispose();
                recognizer = null;
            }
        }
    }
}
