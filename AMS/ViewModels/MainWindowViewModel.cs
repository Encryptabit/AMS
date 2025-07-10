using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AMS.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool _isPlaying;

    [ObservableProperty]
    private double _currentPosition;

    [ObservableProperty]
    private string _currentTime = "00:00:00.000";

    [ObservableProperty]
    private ObservableCollection<string> _chapters = new()
    {
        "Chapter 01",
        "Chapter 02",
        "Chapter 03"
    };

    [ObservableProperty]
    private string? _selectedChapter;

    [ObservableProperty]
    private int _wordCount = 5231;

    [ObservableProperty]
    private string _duration = "35:42";

    [ObservableProperty]
    private ObservableCollection<AMS.Models.TranscriptSentence> _transcriptSentences;

    public MainWindowViewModel()
    {
        InitializeTranscriptData();
    }

    private void InitializeTranscriptData()
    {
        // Sample transcript data with time-based spacing
        _transcriptSentences = new ObservableCollection<AMS.Models.TranscriptSentence>
        {
            new AMS.Models.TranscriptSentence
            {
                Text = "International Space Station. Almost twenty four hours since the astronauts arrived.",
                StartTime = "00:00:00.000",
                EndTime = "00:00:05.250",
                StartTimeMs = 0,
                EndTimeMs = 5250,
                MarginBeforeSentence = 0, // First sentence has no margin
                IsActive = true, // This is the active sentence
                Words = new List<AMS.Models.TranscriptWord>
                {
                    new AMS.Models.TranscriptWord { Text = "International", StartTimeMs = 0, EndTimeMs = 800, TimeMargin = "2,2,2,2" },
                    new AMS.Models.TranscriptWord { Text = "Space", StartTimeMs = 850, EndTimeMs = 1200, TimeMargin = "50,2,2,2", IsActive = true },
                    new AMS.Models.TranscriptWord { Text = "Station.", StartTimeMs = 1250, EndTimeMs = 1800, TimeMargin = "50,2,2,2" },
                    new AMS.Models.TranscriptWord { Text = "Almost", StartTimeMs = 2000, EndTimeMs = 2300, TimeMargin = "200,2,2,2" },
                    new AMS.Models.TranscriptWord { Text = "twenty", StartTimeMs = 2350, EndTimeMs = 2700, TimeMargin = "50,2,2,2" },
                    new AMS.Models.TranscriptWord { Text = "four", StartTimeMs = 2750, EndTimeMs = 3000, TimeMargin = "50,2,2,2" },
                    new AMS.Models.TranscriptWord { Text = "hours", StartTimeMs = 3050, EndTimeMs = 3400, TimeMargin = "50,2,2,2" },
                    new AMS.Models.TranscriptWord { Text = "since", StartTimeMs = 3450, EndTimeMs = 3800, TimeMargin = "50,2,2,2" },
                    new AMS.Models.TranscriptWord { Text = "the", StartTimeMs = 3850, EndTimeMs = 4000, TimeMargin = "50,2,2,2" },
                    new AMS.Models.TranscriptWord { Text = "astronauts", StartTimeMs = 4050, EndTimeMs = 4600, TimeMargin = "50,2,2,2" },
                    new AMS.Models.TranscriptWord { Text = "arrived.", StartTimeMs = 4650, EndTimeMs = 5250, TimeMargin = "50,2,2,2" }
                }
            },

            new AMS.Models.TranscriptSentence
            {
                Text = "The view from up here is absolutely breathtaking.",
                StartTime = "00:00:07.500",
                EndTime = "00:00:11.750",
                StartTimeMs = 7500,
                EndTimeMs = 11750,
                MarginBeforeSentence = 2250, // 2250ms = 2250px gap between sentences
                Words = new List<AMS.Models.TranscriptWord>
                {
                    new AMS.Models.TranscriptWord { Text = "The", StartTimeMs = 7500, EndTimeMs = 7700, TimeMargin = "2,2,2,2" },
                    new AMS.Models.TranscriptWord { Text = "view", StartTimeMs = 7750, EndTimeMs = 8000, TimeMargin = "50,2,2,2" },
                    new AMS.Models.TranscriptWord { Text = "from", StartTimeMs = 8050, EndTimeMs = 8300, TimeMargin = "50,2,2,2" },
                    new AMS.Models.TranscriptWord { Text = "up", StartTimeMs = 8350, EndTimeMs = 8500, TimeMargin = "50,2,2,2" },
                    new AMS.Models.TranscriptWord { Text = "here", StartTimeMs = 8550, EndTimeMs = 8800, TimeMargin = "50,2,2,2" },
                    new AMS.Models.TranscriptWord { Text = "is", StartTimeMs = 8850, EndTimeMs = 9000, TimeMargin = "50,2,2,2" },
                    new AMS.Models.TranscriptWord { Text = "absolutely", StartTimeMs = 9050, EndTimeMs = 9700, TimeMargin = "50,2,2,2" },
                    new AMS.Models.TranscriptWord { Text = "breathtaking.", StartTimeMs = 9750, EndTimeMs = 11750, TimeMargin = "50,2,2,2" }
                }
            },

            new AMS.Models.TranscriptSentence
            {
                Text = "Well uh thank you for tuning in to this second day of our expedition.",
                StartTime = "00:00:15.000",
                EndTime = "00:00:20.500",
                StartTimeMs = 15000,
                EndTimeMs = 20500,
                MarginBeforeSentence = 3250, // 3250ms = 3250px gap
                Words = new List<AMS.Models.TranscriptWord>
                {
                    new AMS.Models.TranscriptWord { Text = "Well", StartTimeMs = 15000, EndTimeMs = 15300, TimeMargin = "2,2,2,2" },
                    new AMS.Models.TranscriptWord { Text = "uh", StartTimeMs = 15350, EndTimeMs = 15500, TimeMargin = "50,2,2,2" },
                    new AMS.Models.TranscriptWord { Text = "thank", StartTimeMs = 15800, EndTimeMs = 16100, TimeMargin = "300,2,2,2" },
                    new AMS.Models.TranscriptWord { Text = "you", StartTimeMs = 16150, EndTimeMs = 16400, TimeMargin = "50,2,2,2" },
                    new AMS.Models.TranscriptWord { Text = "for", StartTimeMs = 16450, EndTimeMs = 16600, TimeMargin = "50,2,2,2" },
                    new AMS.Models.TranscriptWord { Text = "tuning", StartTimeMs = 16650, EndTimeMs = 17000, TimeMargin = "50,2,2,2" },
                    new AMS.Models.TranscriptWord { Text = "in", StartTimeMs = 17050, EndTimeMs = 17200, TimeMargin = "50,2,2,2" },
                    new AMS.Models.TranscriptWord { Text = "to", StartTimeMs = 17250, EndTimeMs = 17400, TimeMargin = "50,2,2,2" },
                    new AMS.Models.TranscriptWord { Text = "this", StartTimeMs = 17450, EndTimeMs = 17700, TimeMargin = "50,2,2,2" },
                    new AMS.Models.TranscriptWord { Text = "second", StartTimeMs = 17750, EndTimeMs = 18100, TimeMargin = "50,2,2,2" },
                    new AMS.Models.TranscriptWord { Text = "day", StartTimeMs = 18150, EndTimeMs = 18400, TimeMargin = "50,2,2,2" },
                    new AMS.Models.TranscriptWord { Text = "of", StartTimeMs = 18450, EndTimeMs = 18600, TimeMargin = "50,2,2,2" },
                    new AMS.Models.TranscriptWord { Text = "our", StartTimeMs = 18650, EndTimeMs = 18900, TimeMargin = "50,2,2,2" },
                    new AMS.Models.TranscriptWord { Text = "expedition.", StartTimeMs = 18950, EndTimeMs = 20500, TimeMargin = "50,2,2,2" }
                }
            }
        };
    }

    [RelayCommand]
    private void PlayPause()
    {
        IsPlaying = !IsPlaying;
    }

    [RelayCommand]
    private void Stop()
    {
        IsPlaying = false;
        CurrentPosition = 0;
        CurrentTime = "00:00:00.000";
    }

    [RelayCommand]
    private void RunWhisper()
    {
        // Will implement actual functionality later
    }

    [RelayCommand]
    private void RunAeneas()
    {
        // Will implement actual functionality later
    }

    [RelayCommand]
    private void ReloadChapter()
    {
        // Will implement actual functionality later
    }

    [RelayCommand]
    private void Split()
    {
        // Will implement actual functionality later
    }

    [RelayCommand]
    private void Trim()
    {
        // Will implement actual functionality later
    }

    [RelayCommand]
    private void Crossfade()
    {
        // Will implement actual functionality later
    }

    [RelayCommand]
    private void Normalize()
    {
        // Will implement actual functionality later
    }

    [RelayCommand]
    private void AddRoomTone()
    {
        // Will implement actual functionality later
    }

    [RelayCommand]
    private void OpenSettings()
    {
        // Will implement actual functionality later
    }
}