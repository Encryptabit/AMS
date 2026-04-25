using System.Reflection;
using Ams.Workstation.Server.Components.Shared;
using Ams.Workstation.Server.Models;
using Microsoft.AspNetCore.Components;

namespace Ams.Tests.Services;

public class CrxChapterMatcherTests
{
    [Fact]
    public void Matches_ChapterAndSameNumberChapter_ReturnsTrue()
    {
        Assert.True(CrxChapterMatcher.Matches(
            "Chapter 4: The Birth of an Empire",
            "Chapter 4: The Birth of an Empire"));
    }

    [Fact]
    public void Matches_EpilogueAndChapterWithSameNumber_ReturnsFalse()
    {
        Assert.False(CrxChapterMatcher.Matches(
            "Epilogue 4: Entity",
            "Chapter 4: The Birth of an Empire"));
    }

    [Fact]
    public void TryParseShouldBe_PrefersCorrectedText()
    {
        const string comments = "Should be: at the proclamation every [nearby] legionnaire stood at attention\r\nRead as: at the proclamation [nearly] every legionnaire stood at attention";

        Assert.Equal(
            "at the proclamation every nearby legionnaire stood at attention",
            CrxCommentParser.TryParseShouldBe(comments));
        Assert.Equal(
            "at the proclamation nearly every legionnaire stood at attention",
            CrxCommentParser.TryParseReadAs(comments));
    }

    [Fact]
    public void CrxModal_FallbackRange_UneditedConfirmation_AllowsSubmitRequestPreparation()
    {
        var modal = new CrxModal();
        SeedFallbackRangeState(modal, isRangeConfirmed: false);

        // Simulate stored sub-millisecond drift from prior drag math while inputs remain unedited.
        SetPrivateField(modal, "_startTime", 0.1509);
        SetPrivateField(modal, "_endTime", 1.1509);

        InvokePrivateIgnoringRenderHandle(
            modal,
            "OnRangeConfirmationChanged",
            new ChangeEventArgs { Value = true });

        Assert.True(GetPrivateField<bool>(modal, "_isRangeConfirmed"));

        var canSubmit = modal.TryCommitPendingRangeInputsForTest();
        Assert.True(canSubmit);
        Assert.True(GetPrivateField<bool>(modal, "_isRangeConfirmed"));

        var request = modal.BuildSubmitRequestForTest();
        Assert.Equal(0.15, request.Start, precision: 3);
        Assert.Equal(1.15, request.End, precision: 3);
        Assert.Equal(101, request.SentenceId);
        Assert.Equal("MR", request.ErrorType);
        Assert.Equal(0, request.PaddingMs);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CrxModal_FallbackRangeConfirmation_ResetsOnAnyParsedNumericBoundChange(bool mutateStart)
    {
        var modal = new CrxModal();
        SeedFallbackRangeState(modal, isRangeConfirmed: true);

        if (mutateStart)
        {
            SetPrivateField(modal, "_startInput", "0:00.1505");
        }
        else
        {
            SetPrivateField(modal, "_endInput", "0:01.1505");
        }

        var baselineFormatted = mutateStart ? InvokeFormatTime(0.15) : InvokeFormatTime(1.15);
        var changedFormatted = mutateStart ? InvokeFormatTime(0.1505) : InvokeFormatTime(1.1505);
        Assert.Equal(baselineFormatted, changedFormatted);

        var canSubmit = modal.TryCommitPendingRangeInputsForTest();

        Assert.False(canSubmit);
        Assert.False(GetPrivateField<bool>(modal, "_isRangeConfirmed"));

        if (mutateStart)
        {
            Assert.Equal(0.15, GetPrivateField<double>(modal, "_startTime"), precision: 3);
        }
        else
        {
            Assert.Equal(1.15, GetPrivateField<double>(modal, "_endTime"), precision: 3);
        }
    }

    private static void SeedFallbackRangeState(CrxModal modal, bool isRangeConfirmed)
    {
        SetPrivateField(modal, "_chapterName", "chapter-01");
        SetPrivateField(modal, "_sentenceId", 101);
        SetPrivateField(modal, "_errorType", "MR");
        SetPrivateField(modal, "_excerpt", "Batch export sentences #101 through #202 (2 total).");
        SetPrivateField(modal, "_comments", "Fallback range confirmation test");
        SetPrivateField(modal, "_startTime", 0.15);
        SetPrivateField(modal, "_endTime", 1.15);
        SetPrivateField(modal, "_startInput", "0:00.150");
        SetPrivateField(modal, "_endInput", "0:01.150");
        SetPrivateField(modal, "_requiresRangeConfirmation", true);
        SetPrivateField(modal, "_isRangeConfirmed", isRangeConfirmed);
        SetPrivateField<string?>(modal, "_rangeValidationMessage", null);
    }

    private static string InvokeFormatTime(double seconds)
    {
        var method = typeof(CrxModal).GetMethod("FormatTime", BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new Xunit.Sdk.XunitException("Missing private static FormatTime helper on CrxModal.");

        var value = method.Invoke(null, [seconds]);
        return value as string
            ?? throw new Xunit.Sdk.XunitException("FormatTime returned unexpected null/non-string value.");
    }

    private static void InvokePrivateIgnoringRenderHandle(CrxModal modal, string methodName, params object?[]? args)
    {
        var method = typeof(CrxModal).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new Xunit.Sdk.XunitException($"Missing private method '{methodName}' on CrxModal.");

        try
        {
            method.Invoke(modal, args);
        }
        catch (TargetInvocationException ex)
            when (ex.InnerException is InvalidOperationException invalidOperation
                  && invalidOperation.Message.Contains("render handle is not yet assigned", StringComparison.OrdinalIgnoreCase))
        {
            // Tests exercise modal logic without mounting a renderer.
        }
    }

    private static T GetPrivateField<T>(CrxModal modal, string fieldName)
    {
        var field = typeof(CrxModal).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new Xunit.Sdk.XunitException($"Missing private field '{fieldName}' on CrxModal.");

        var value = field.GetValue(modal);
        if (value is T typed)
        {
            return typed;
        }

        throw new Xunit.Sdk.XunitException(
            $"Unable to read private field '{fieldName}' as {typeof(T).Name}; actual value type was '{value?.GetType().Name ?? "null"}'.");
    }

    private static void SetPrivateField<T>(CrxModal modal, string fieldName, T value)
    {
        var field = typeof(CrxModal).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new Xunit.Sdk.XunitException($"Missing private field '{fieldName}' on CrxModal.");

        field.SetValue(modal, value);
    }
}
