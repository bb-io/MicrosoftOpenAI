using Apps.AzureOpenAI.Actions;
using Apps.AzureOpenAI.Models.Requests.Chat;
using Tests.AzureOpenAI.Base;

namespace Tests.AzureOpenAI;

[TestClass]
public class ChatActionsTests : TestBase
{
    [TestMethod]
    public async Task GetTranslationIssues_IsSuccess()
    {
		// Arrange
		var action = new ChatActions(InvocationContext, FileManager);
		var request = new GetTranslationIssuesRequest
		{
			SourceText = "Learning to code can be fun and rewarding, but it requires patience and practice.",
			TargetText = "Oppiminen koodaaminen voi olla hauskaa ja palkitseva, mutta se vaatii kärsivällisyys ja harjoittaa.",
			MaximumTokens = 20
        };

		// Act
		var result = await action.GetTranslationIssues(request);

        // Assert
        Console.WriteLine(result.Message);

		Assert.IsNotNull(result);
	}
}
