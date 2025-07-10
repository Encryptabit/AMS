using System;
using System.Collections.Generic;

namespace AMS.Models
{
    /// <summary>
    /// Represents an audiobook project containing chapters and metadata
    /// </summary>
    public class AudioProject
    {   
        /// <summary>
        /// Name of the project
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Base directory path for the project
        /// </summary>
        public string BasePath { get; set; } = string.Empty;

        /// <summary>
        /// Collection of chapters in the project
        /// </summary>
        public List<AudioChapter> Chapters { get; set; } = new List<AudioChapter>();

        /// <summary>
        /// Date the project was created
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Date the project was last modified
        /// </summary>
        public DateTime LastModifiedDate { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Represents a single chapter in an audiobook project
    /// </summary>
    public class AudioChapter
    {   
        /// <summary>
        /// Name of the chapter
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Path to the chapter's WAV file
        /// </summary>
        public string WavPath { get; set; } = string.Empty;

        /// <summary>
        /// Path to the chapter's transcript JSON file
        /// </summary>
        public string TranscriptPath { get; set; } = string.Empty;

        /// <summary>
        /// Path to the chapter's script file
        /// </summary>
        public string ScriptPath { get; set; } = string.Empty;

        /// <summary>
        /// Indicates whether the chapter has been processed with WhisperX
        /// </summary>
        public bool IsProcessed => !string.IsNullOrEmpty(TranscriptPath);

        /// <summary>
        /// Duration of the audio in seconds
        /// </summary>
        public double DurationInSeconds { get; set; }

        /// <summary>
        /// Word count from the transcript
        /// </summary>
        public int WordCount { get; set; }
    }
}
