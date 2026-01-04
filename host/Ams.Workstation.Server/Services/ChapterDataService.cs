using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ams.Workstation.Server.Models;

namespace Ams.Workstation.Server.Services;

/// <summary>
/// Service for loading chapter data including sentences with timing information.
/// Currently provides mock data; will be extended to load from Ams.Core hydrate.json files.
/// </summary>
public class ChapterDataService
{
    /// <summary>
    /// Gets the list of sentences for a chapter.
    /// Currently returns mock data for testing UI functionality.
    /// </summary>
    /// <param name="chapterName">The name of the chapter to load.</param>
    /// <returns>A list of sentences with timing information.</returns>
    public Task<List<SentenceViewModel>> GetSentencesAsync(string chapterName)
    {
        // Mock data for testing - simulates a chapter with varied sentence lengths and statuses
        var sentences = new List<SentenceViewModel>
        {
            new() { Id = 1, Text = "It was a bright cold day in April, and the clocks were striking thirteen.", StartTime = 0.0, EndTime = 4.2, Status = "ok" },
            new() { Id = 2, Text = "Winston Smith, his chin nuzzled into his breast in an effort to escape the vile wind, slipped quickly through the glass doors of Victory Mansions.", StartTime = 4.2, EndTime = 10.8, Status = "ok" },
            new() { Id = 3, Text = "The hallway smelt of boiled cabbage and old rag mats.", StartTime = 10.8, EndTime = 14.1, Status = "ok" },
            new() { Id = 4, Text = "At one end of it a coloured poster, too large for indoor display, had been tacked to the wall.", StartTime = 14.1, EndTime = 19.5, Status = "warning", Wer = 0.12 },
            new() { Id = 5, Text = "It depicted simply an enormous face, more than a metre wide: the face of a man of about forty-five, with a heavy black moustache and ruggedly handsome features.", StartTime = 19.5, EndTime = 28.3, Status = "ok" },
            new() { Id = 6, Text = "Winston made for the stairs.", StartTime = 28.3, EndTime = 30.1, Status = "ok" },
            new() { Id = 7, Text = "It was no use trying the lift.", StartTime = 30.1, EndTime = 32.4, Status = "ok" },
            new() { Id = 8, Text = "Even at the best of times it was seldom working, and at present the electric current was cut off during daylight hours.", StartTime = 32.4, EndTime = 39.2, Status = "error", Wer = 0.25, HasDiff = true, DiffHtml = "Even at the best of times it was <span class=\"diff-delete\">almost</span><span class=\"diff-insert\">seldom</span> working, and at present the electric current was cut off during daylight hours." },
            new() { Id = 9, Text = "It was part of the economy drive in preparation for Hate Week.", StartTime = 39.2, EndTime = 43.5, Status = "ok" },
            new() { Id = 10, Text = "The flat was seven flights up, and Winston, who was thirty-nine and had a varicose ulcer above his right ankle, went slowly.", StartTime = 43.5, EndTime = 51.2, Status = "ok" },
            new() { Id = 11, Text = "Resting several times on the way.", StartTime = 51.2, EndTime = 53.8, Status = "ok" },
            new() { Id = 12, Text = "On each landing, opposite the lift-shaft, the poster with the enormous face gazed from the wall.", StartTime = 53.8, EndTime = 59.6, Status = "ok" },
            new() { Id = 13, Text = "It was one of those pictures which are so contrived that the eyes follow you about when you move.", StartTime = 59.6, EndTime = 65.4, Status = "warning", Wer = 0.08 },
            new() { Id = 14, Text = "BIG BROTHER IS WATCHING YOU, the caption beneath it ran.", StartTime = 65.4, EndTime = 69.2, Status = "ok" },
            new() { Id = 15, Text = "Inside the flat a fruity voice was reading out a list of figures which had something to do with the production of pig-iron.", StartTime = 69.2, EndTime = 76.8, Status = "ok" },
        };

        return Task.FromResult(sentences);
    }

    /// <summary>
    /// Gets a specific sentence by ID.
    /// </summary>
    /// <param name="chapterName">The chapter name.</param>
    /// <param name="sentenceId">The sentence ID.</param>
    /// <returns>The sentence, or null if not found.</returns>
    public async Task<SentenceViewModel?> GetSentenceAsync(string chapterName, int sentenceId)
    {
        var sentences = await GetSentencesAsync(chapterName);
        return sentences.FirstOrDefault(s => s.Id == sentenceId);
    }

    /// <summary>
    /// Gets the total duration of a chapter based on its sentences.
    /// </summary>
    /// <param name="chapterName">The chapter name.</param>
    /// <returns>The total duration in seconds.</returns>
    public async Task<double> GetChapterDurationAsync(string chapterName)
    {
        var sentences = await GetSentencesAsync(chapterName);
        return sentences.Any() ? sentences.Max(s => s.EndTime) : 0;
    }
}
