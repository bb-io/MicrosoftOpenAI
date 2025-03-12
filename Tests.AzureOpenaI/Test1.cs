using Apps.AzureOpenAI.Actions;
using Apps.AzureOpenAI.Models.Requests.Chat;
using Tests.AzureOpenAI.Base;

namespace Tests.AzureOpenAI
{
    [TestClass]
    public sealed class Test1:TestBase
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            var xliffActions = new XliffActions(InvocationContext, FileManager);

            await xliffActions.ScoreXLIFF(new Apps.AzureOpenAI.Models.Requests.Xliff.ScoreXliffRequest()
            {
                SourceLanguage = "en_GB",
                TargetLanguage = "fr_FR",
                File = new Blackbird.Applications.Sdk.Common.Files.FileReference() { Name = "test.csv" }
            }, " [INSTRUCTIONS]\r\nFirst, determine the overarching domain, field or topic of the entire set of sentences considered as a whole.\r\n\r\nNext, assess the style, accuracy and appropriateness of the translation within the specific domain, field or topic identified in the previous step.\r\n\r\nSpecifically, assign a score of '0' if:\r\n\r\n- the translation contains a term, keyword or phrase that should be rendered differently for the identified domain, field or topic\r\n- the source content is contextually undetermined, such as single terms, keywords or phrases present in the source text lack sufficient context for the accuracy of their translation to be determined\r\n- the translation contains a phrase or formulation that sounds unnatural or awkward (to assess this, consider whether the phrase or formulation is commonly used by native writers, taking into account idiomatic expressions and contextually appropriate language)\r\n- the translation is hard to understand, such as a competent reader would have difficulty to comprehend the meaning of the translation upon first reading. For example, if the translation contains overly embedded clauses that would be due to an inappropriate retention of the structure of the source text\r\n- the translation introduces a potential semantic ambiguity that is not present in the source text- the tone or formality level of the translation is not consistent with the other translations of the current dataset input (only flag as inconsistent the sentences which are outliers)", new BaseChatRequest(),100);

        }
    }
}
