# Call Graph Inventory

Inventory of existing call graphs in D:/Notes, mapped to corresponding source files.

**Source:** D:/Notes/*.md (137 files)
**Generator:** Method-level call graph generator (captures Calls → and Called-by ← relationships)

---

## Summary

| Metric | Count |
|--------|-------|
| Total call graph files | 137 |
| Mapped to existing source | 132 |
| Orphaned (no matching source) | 2 |
| Multiple matches (Program.md) | 1 |
| Test framework artifacts | 2 |

---

## Call Graphs by Project

### Ams.Core (89 graphs)

#### Application/Commands/
| Graph File | Source File | Status |
|------------|-------------|--------|
| BuildTranscriptIndexCommand.md | Application/Commands/BuildTranscriptIndexCommand.cs | Current |
| ComputeAnchorsCommand.md | Application/Commands/ComputeAnchorsCommand.cs | Current |
| GenerateTranscriptCommand.md | Application/Commands/GenerateTranscriptCommand.cs | Current |
| HydrateTranscriptCommand.md | Application/Commands/HydrateTranscriptCommand.cs | Current |
| MergeTimingsCommand.md | Application/Commands/MergeTimingsCommand.cs | Current |
| RunMfaCommand.md | Application/Commands/RunMfaCommand.cs | Current |

#### Application/Mfa/
| Graph File | Source File | Status |
|------------|-------------|--------|
| MfaDetachedProcessRunner.md | Application/Mfa/MfaDetachedProcessRunner.cs | Current |
| MfaPronunciationProvider.md | Application/Mfa/MfaPronunciationProvider.cs | Current |
| MfaService.md | Application/Mfa/MfaService.cs | Current |
| MfaWorkflow.md | Application/Mfa/MfaWorkflow.cs | Current |

#### Application/Pipeline/
| Graph File | Source File | Status |
|------------|-------------|--------|
| PipelineConcurrencyControl.md | Application/Pipeline/PipelineConcurrencyControl.cs | Current |

#### Application/Processes/
| Graph File | Source File | Status |
|------------|-------------|--------|
| AsrProcessSupervisor.md | Application/Processes/AsrProcessSupervisor.cs | Current |
| MfaProcessSupervisor.md | Application/Processes/MfaProcessSupervisor.cs | Current |

#### Artifacts/
| Graph File | Source File | Status |
|------------|-------------|--------|
| AudioBuffer.md | Artifacts/AudioBuffer.cs | Current |
| AudioBufferMetadata.md | Artifacts/AudioBufferMetadata.cs | Current |
| SentenceTiming.md | Artifacts/SentenceTiming.cs | Current |
| TimingOverrides.md | Artifacts/TimingOverrides.cs | Current |
| TimingRange.md | Artifacts/TimingRange.cs | Current |
| TranscriptModels.md | Artifacts/TranscriptModels.cs | Current |

#### Asr/
| Graph File | Source File | Status |
|------------|-------------|--------|
| AsrClient.md | Asr/AsrClient.cs | Current |
| AsrEngine.md | Asr/AsrEngine.cs | Current |
| AsrModels.md | Asr/AsrModels.cs | Current |
| AsrTranscriptBuilder.md | Asr/AsrTranscriptBuilder.cs | Current |

#### Audio/
| Graph File | Source File | Status |
|------------|-------------|--------|
| AudioIntegrityVerifier.md | Audio/AudioIntegrityVerifier.cs | Current |
| DspDemoRunner.md | Audio/DspDemoRunner.cs | Current |
| FeatureExtraction.md | Audio/FeatureExtraction.cs | Current |
| SentenceTimelineBuilder.md | Audio/SentenceTimelineBuilder.cs | Current |

#### Common/
| Graph File | Source File | Status |
|------------|-------------|--------|
| LevenshteinMetrics.md | Common/LevenshteinMetrics.cs | Current |
| Log.md | Common/Log.cs | Current |
| TextNormalizer.md | Common/TextNormalizer.cs | Current |

#### Pipeline/
| Graph File | Source File | Status |
|------------|-------------|--------|
| ManifestV2.md | Pipeline/ManifestV2.cs | Current |
| SentenceRefinementService.md | Pipeline/SentenceRefinementService.cs | Current |

#### Processors/
| Graph File | Source File | Status |
|------------|-------------|--------|
| AsrProcessor.md | Processors/AsrProcessor.cs | Current |
| AudioProcessor.md | Processors/AudioProcessor.cs | Current |
| AudioProcessor.Analysis.md | Processors/AudioProcessor.Analysis.cs | Current |

#### Processors/Alignment/Anchors/
| Graph File | Source File | Status |
|------------|-------------|--------|
| AnchorDiscovery.md | Processors/Alignment/Anchors/AnchorDiscovery.cs | Current |
| AnchorPipeline.md | Processors/Alignment/Anchors/AnchorPipeline.cs | Current |
| AnchorPreprocessor.md | Processors/Alignment/Anchors/AnchorPreprocessor.cs | Current |
| AnchorTokenizer.md | Processors/Alignment/Anchors/AnchorTokenizer.cs | Current |
| SectionLocator.md | Processors/Alignment/Anchors/SectionLocator.cs | Current |

#### Processors/Alignment/Mfa/
| Graph File | Source File | Status |
|------------|-------------|--------|
| MfaTimingMerger.md | Processors/Alignment/Mfa/MfaTimingMerger.cs | Current |
| TextGridParser.md | Processors/Alignment/Mfa/TextGridParser.cs | Current |

#### Processors/Alignment/Tx/
| Graph File | Source File | Status |
|------------|-------------|--------|
| TranscriptAligner.md | Processors/Alignment/Tx/TranscriptAligner.cs | Current |
| WindowBuilder.md | Processors/Alignment/Tx/WindowBuilder.cs | Current |

#### Processors/Diffing/
| Graph File | Source File | Status |
|------------|-------------|--------|
| TextDiffAnalyzer.md | Processors/Diffing/TextDiffAnalyzer.cs | Current |

#### Processors/DocumentProcessor/
| Graph File | Source File | Status |
|------------|-------------|--------|
| DocumentProcessor.Cache.md | Processors/DocumentProcessor/DocumentProcessor.Cache.cs | Current |
| DocumentProcessor.Indexing.md | Processors/DocumentProcessor/DocumentProcessor.Indexing.cs | Current |
| DocumentProcessor.Phonemes.md | Processors/DocumentProcessor/DocumentProcessor.Phonemes.cs | Current |

#### Processors/Validation/
| Graph File | Source File | Status |
|------------|-------------|--------|
| ValidationReportBuilder.md | Processors/Validation/ValidationReportBuilder.cs | Current |

#### Prosody/
| Graph File | Source File | Status |
|------------|-------------|--------|
| PauseAdjustmentsDocument.md | Prosody/PauseAdjustmentsDocument.cs | Current |
| PauseAnalysisReport.md | Prosody/PauseAnalysisReport.cs | Current |
| PauseCompressionMath.md | Prosody/PauseCompressionMath.cs | Current |
| PauseDynamicsService.md | Prosody/PauseDynamicsService.cs | Current |
| PauseMapBuilder.md | Prosody/PauseMapBuilder.cs | Current |
| PauseMapModels.md | Prosody/PauseMapModels.cs | Current |
| PauseModels.md | Prosody/PauseModels.cs | Current |
| PausePolicyStorage.md | Prosody/PausePolicyStorage.cs | Current |
| PauseTimelineApplier.md | Prosody/PauseTimelineApplier.cs | Current |

#### Runtime/
| Graph File | Source File | Status |
|------------|-------------|--------|
| FileArtifactResolver.md | Runtime/Artifacts/FileArtifactResolver.cs | Current |
| IArtifactResolver.md | Runtime/Artifacts/IArtifactResolver.cs | Current |
| AudioBufferContext.md | Runtime/Audio/AudioBufferContext.cs | Current |
| AudioBufferManager.md | Runtime/Audio/AudioBufferManager.cs | Current |
| BookCache.md | Runtime/Book/BookCache.cs | Current |
| BookContext.md | Runtime/Book/BookContext.cs | Current |
| BookDocuments.md | Runtime/Book/BookDocuments.cs | Current |
| BookIndexer.md | Runtime/Book/BookIndexer.cs | Current |
| BookManager.md | Runtime/Book/BookManager.cs | Current |
| BookParser.md | Runtime/Book/BookParser.cs | Current |
| BookPhonemePopulator.md | Runtime/Book/BookPhonemePopulator.cs | Current |
| IBookServices.md | Runtime/Book/IBookServices.cs | Current |
| IPronunciationProvider.md | Runtime/Book/IPronunciationProvider.cs | Current |
| PronunciationHelper.md | Runtime/Book/PronunciationHelper.cs | Current |
| ChapterContext.md | Runtime/Chapter/ChapterContext.cs | Current |
| ChapterContextHandle.md | Runtime/Chapter/ChapterContextHandle.cs | Current |
| ChapterDocuments.md | Runtime/Chapter/ChapterDocuments.cs | Current |
| ChapterManager.md | Runtime/Chapter/ChapterManager.cs | Current |
| DelegateDocumentSlotAdapter.md | Runtime/Common/DelegateDocumentSlotAdapter.cs | Current |
| DocumentSlot.md | Runtime/Common/DocumentSlot.cs | Current |
| IDocumentSlotAdapter.md | Runtime/Common/IDocumentSlotAdapter.cs | Current |
| IAudioBufferManager.md | Runtime/Interfaces/IAudioBufferManager.cs | Current |
| IBookManager.md | Runtime/Interfaces/IBookManager.cs | Current |
| IChapterManager.md | Runtime/Interfaces/IChapterManager.cs | Current |
| IWorkspace.md | Runtime/Workspace/IWorkspace.cs | Current |
| WorkspaceChapterDiscovery.md | Runtime/Workspace/WorkspaceChapterDiscovery.cs | Current |

#### Services/
| Graph File | Source File | Status |
|------------|-------------|--------|
| AlignmentService.md | Services/Alignment/AlignmentService.cs | Current |
| AsrService.md | Services/AsrService.cs | Current |
| AudioService.md | Services/AudioService.cs | Current |
| DocumentService.md | Services/Documents/DocumentService.cs | Current |
| PipelineService.md | Services/PipelineService.cs | Current |
| ValidationService.md | Services/ValidationService.cs | Current |

#### Services/Interfaces/
| Graph File | Source File | Status |
|------------|-------------|--------|
| IAlignmentService.md | Services/Interfaces/IAlignmentService.cs | Current |
| IAsrService.md | Services/Interfaces/IAsrService.cs | Current |
| IAudioService.md | Services/Interfaces/IAudioService.cs | Current |
| IDocumentService.md | Services/Interfaces/IDocumentService.cs | Current |

#### Services/Integrations/FFmpeg/
| Graph File | Source File | Status |
|------------|-------------|--------|
| FfDecoder.md | Services/Integrations/FFmpeg/FfDecoder.cs | Current |
| FfEncoder.md | Services/Integrations/FFmpeg/FfEncoder.cs | Current |
| FfFilterGraph.md | Services/Integrations/FFmpeg/FfFilterGraph.cs | Current |
| FfFilterGraphRunner.md | Services/Integrations/FFmpeg/FfFilterGraphRunner.cs | Current |
| FfLogCapture.md | Services/Integrations/FFmpeg/FfLogCapture.cs | Current |
| FfSession.md | Services/Integrations/FFmpeg/FfSession.cs | Current |
| FfUtils.md | Services/Integrations/FFmpeg/FfUtils.cs | Current |

---

### Ams.Cli (19 graphs)

| Graph File | Source File | Status |
|------------|-------------|--------|
| AlignCommand.md | Commands/AlignCommand.cs | Current |
| AsrCommand.md | Commands/AsrCommand.cs | Current |
| BookCommand.md | Commands/BookCommand.cs | Current |
| BuildIndexCommand.md | Commands/BuildIndexCommand.cs | Current |
| DspCommand.md | Commands/DspCommand.cs | Current |
| PipelineCommand.md | Commands/PipelineCommand.cs | Current |
| RefineSentencesCommand.md | Commands/RefineSentencesCommand.cs | Current |
| TextCommand.md | Commands/TextCommand.cs | Current |
| ValidateCommand.md | Commands/ValidateCommand.cs | Current |
| ValidateTimingSession.md | Commands/ValidateTimingSession.cs | Current |
| FilterChainConfig.md | Models/FilterChainConfig.cs | Current |
| TreatmentModels.md | Models/TreatmentModels.cs | Current |
| ReplContext.md | Repl/ReplContext.cs | Current |
| DspConfigService.md | Services/DspConfigService.cs | Current |
| PlugalyzerService.md | Services/PlugalyzerService.cs | Current |
| CommandInputResolver.md | Utilities/CommandInputResolver.cs | Current |
| PausePolicyResolver.md | Utilities/PausePolicyResolver.cs | Current |
| CliWorkspace.md | Workspace/CliWorkspace.cs | Current |

---

### Ams.Dsp.Native (2 graphs)

| Graph File | Source File | Status |
|------------|-------------|--------|
| AmsDsp.md | AmsDsp.cs | Current |
| Native.md | Native.cs | Current |

---

### Ams.Tests (8 graphs)

| Graph File | Source File | Status |
|------------|-------------|--------|
| AnchorDiscoveryTests.md | AnchorDiscoveryTests.cs | Current |
| AudioProcessorFilterTests.md | AudioProcessorFilterTests.cs | Current |
| BookParsingTests.md | BookParsingTests.cs | Current |
| TokenizerTests.md | TokenizerTests.cs | Current |
| TxAlignTests.md | TxAlignTests.cs | Current |
| WavIoTests.md | WavIoTests.cs | Current |
| PauseApplierTests.md | Prosody/PauseApplierTests.cs | Current |
| PauseDynamicsServiceTests.md | Prosody/PauseDynamicsServiceTests.cs | Current |

---

### Ams.UI.Avalonia (2 graphs)

| Graph File | Source File | Status |
|------------|-------------|--------|
| App.axaml.md | App.axaml.cs | Current |
| MainWindow.axaml.md | MainWindow.axaml.cs | Current |

---

### Ams.Web.Api (4 graphs)

| Graph File | Source File | Status |
|------------|-------------|--------|
| WorkspaceState.md | WorkspaceState.cs | Current |
| ValidationMapper.md | Mappers/ValidationMapper.cs | Current |
| ReviewedStateService.md | Services/ReviewedStateService.cs | Current |

---

### Ams.Web.Client (1 graph)

| Graph File | Source File | Status |
|------------|-------------|--------|
| ValidationApiClient.md | Services/ValidationApiClient.cs | Current |

---

### Multi-Match (1 graph)

| Graph File | Potential Sources | Notes |
|------------|-------------------|-------|
| Program.md | Ams.Cli/Program.cs, Ams.Web.Api/Program.cs, Ams.UI.Avalonia/Program.cs, Ams.Web/Program.cs, Ams.Web.Client/Program.cs, InspectDocX/Program.cs, OverlayTest/Program.cs | Graph likely represents Ams.Cli based on content depth |

---

### Orphaned Graphs (2 files)

| Graph File | Notes |
|------------|-------|
| Microsoft.NET.Test.Sdk.Program.md | Test SDK artifact, not actual source |
| ScriptValidator.md | No matching source file found - possibly deleted class |

---

## Graph Format Analysis

Each call graph file follows this structure:
- **Heading**: `# {ClassName}.cs`
- **Method entries**: `#### [[MethodName]]`
- **Sections per method**:
  - "What it does" (TODO placeholders)
  - "Improvements" (TODO placeholders)
  - Method signature in code block
  - `**Calls →**` - Outgoing dependencies
  - `**Called-by ←**` - Incoming dependencies

### Notable Characteristics

1. **Full method coverage**: Graphs include private methods, nested classes, and all call relationships
2. **Cross-class calls**: References like `[[ExecuteAsync]]` link to other files' methods
3. **TODO placeholders**: "What it does" sections are empty - descriptions not yet written
4. **Includes P/Invoke**: FFmpeg files (FfDecoder, FfEncoder, etc.) have method-level graphs but P/Invoke details not captured

---

## Coverage Statistics

| Project | Source Files | Graph Files | Coverage |
|---------|-------------|-------------|----------|
| Ams.Core | 96 | 89 | 93% |
| Ams.Cli | 22 | 19 | 86% |
| Ams.Tests | 9 | 8 | 89% |
| Ams.Dsp.Native | 2 | 2 | 100% |
| Ams.UI.Avalonia | 3 | 2 | 67% |
| Ams.Web.Api | 6 | 4 | 67% |
| Ams.Web.Client | 2 | 1 | 50% |
| Ams.Web.Shared | 3 | 0 | 0% |
| Ams.Web | 1 | 0 | 0% |
| Analysis tools | 2 | 0 | 0% |
| **Total** | **146** | **132** | **90%** |

---

*Generated: 2025-12-28*
