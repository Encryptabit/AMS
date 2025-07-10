using System;
using System.Collections.Generic;

namespace AMS.Models
{
    /// <summary>
    /// Represents a sentence in the transcript with timing information
    /// </summary>
    public class TranscriptSentence
    {
        /// <summary>
        /// The text content of the sentence
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Start time of the sentence in the audio (format: hh:mm:ss.ms)
        /// </summary>
        public string StartTime { get; set; } = "00:00:00.000";

        /// <summary>
        /// End time of the sentence in the audio (format: hh:mm:ss.ms)
        /// </summary>
        public string EndTime { get; set; } = "00:00:00.000";

        /// <summary>
        /// Start time in milliseconds for calculation purposes
        /// </summary>
        public int StartTimeMs { get; set; }

        /// <summary>
        /// End time in milliseconds for calculation purposes
        /// </summary>
        public int EndTimeMs { get; set; }

        /// <summary>
        /// Margin before this sentence in pixels (1ms = 1px)
        /// </summary>
        public int MarginBeforeSentence { get; set; }

        /// <summary>
        /// Indicates if this is the currently active sentence during playback
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// List of words in this sentence
        /// </summary>
        public List<TranscriptWord> Words { get; set; } = new List<TranscriptWord>();
    }

    /// <summary>
    /// Represents a word in the transcript with timing information
    /// </summary>
    public class TranscriptWord
    {
        /// <summary>
        /// The text of the word
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Start time of the word in milliseconds
        /// </summary>
        public int StartTimeMs { get; set; }

        /// <summary>
        /// End time of the word in milliseconds
        /// </summary>
        public int EndTimeMs { get; set; }

        /// <summary>
        /// Margin applied to this word based on time (left, top, right, bottom)
        /// </summary>
        public string TimeMargin { get; set; } = "2,2,2,2";

        /// <summary>
        /// Indicates if this is the currently active word during playback
        /// </summary>
        public bool IsActive { get; set; }
    }
}
