using Apps.AzureOpenAI.Models.PostEdit;
using Apps.AzureOpenAI.Services;
using Apps.AzureOpenAI.Services.Abstract;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Tests.AzureOpenAI.Base;

namespace Tests.AzureOpenAI;

[TestClass]
public class ProcessXliffServiceTests : TestBase
{
    private ProcessXliffService _processXliffService = null!;
    private FileReference _xliffFile = null!;
    private FileReference _glossaryFile = null!;

    [TestInitialize]
    public void Setup()
    {
        // Initialize the services needed for ProcessXliffService
        IXliffService xliffService = new XliffService(FileManager);
        IGlossaryService glossaryService = new GlossaryService(FileManager);
        IOpenAICompletionService openaiService = new OpenAICompletionService(new Apps.AzureOpenAI.Api.AzureOpenAIRestClient(Creds), Creds);
        IResponseDeserializationService deserializationService = new ResponseDeserializationService();
        IPromptBuilderService promptBuilderService = new PromptBuilderService();
        IFileManagementClient fileManagementClient = FileManager;

        _processXliffService = new ProcessXliffService(
            xliffService,
            glossaryService,
            openaiService,
            deserializationService,
            promptBuilderService,
            fileManagementClient
        );

        // Initialize test files
        _xliffFile = new FileReference { Name = "Markdown entry #1_en-US-Default_HTML-nl-NL#TR_FQTF#.html.txlf" };
        _glossaryFile = new FileReference { Name = "glossary.tbx" };
    }

    [TestMethod]
    public async Task ProcessXliffAsync_WithValidRequest_ReturnsSuccessfulResult()
    {
        // Arrange
        var request = new OpenAiXliffInnerRequest
        {
            XliffFile = _xliffFile,
            NeverFail = true,
            BucketSize = 50
        };

        // Act
        var result = await _processXliffService.ProcessXliffAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.File);
        Assert.IsNotNull(result.Usage);
        Assert.IsTrue(result.ProcessedBatchesCount > 0);
        Assert.IsTrue(result.TotalSegmentsCount > 0);

        // Output results for manual inspection
        Console.WriteLine($"Processed batches: {result.ProcessedBatchesCount}");
        Console.WriteLine($"Total segments: {result.TotalSegmentsCount}");
        Console.WriteLine($"Updated segments: {result.TargetsUpdatedCount}");
        Console.WriteLine($"Errors count: {result.ErrorMessages.Count}");
        Console.WriteLine($"Locked segments excluded count: {result.LockedSegmentsExcludeCount}");

        foreach (var error in result.ErrorMessages)
        {
            Console.WriteLine($"Error: {error}");
        }
    }

    [TestMethod]
    public async Task ProcessXliffAsync_WithGlossary_UsesGlossaryForTranslation()
    {
        // Arrange
        var request = new OpenAiXliffInnerRequest
        {
            XliffFile = _xliffFile,
            Glossary = _glossaryFile,
            FilterGlossary = true,
            BucketSize = 1000,
            NeverFail = true
        };

        // Act
        var result = await _processXliffService.ProcessXliffAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.File);
        Assert.IsTrue(result.TargetsUpdatedCount > 0, "No segments were updated");

        // Output results for manual inspection
        Console.WriteLine($"Processed batches: {result.ProcessedBatchesCount}");
        Console.WriteLine($"Total segments: {result.TotalSegmentsCount}");
        Console.WriteLine($"Updated segments: {result.TargetsUpdatedCount}");
        Console.WriteLine($"Usage - Prompt tokens: {result.Usage.PromptTokens}");
        Console.WriteLine($"Usage - Completion tokens: {result.Usage.CompletionTokens}");
        Console.WriteLine($"Usage - Total tokens: {result.Usage.TotalTokens}");
        Console.WriteLine($"Locked segments excluded count: {result.LockedSegmentsExcludeCount}");
    }
}
