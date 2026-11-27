
using System;

namespace YMH219_SpeechApp.Models
{
    public class SpeechLanguage
    {
        public string DisplayName { get; set; }

        public string CultureName { get; set; }

        public SpeechLanguage(
            string displayName,
            string cultureName)
        {
            DisplayName = displayName;
            CultureName = cultureName;
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
