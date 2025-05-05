using Apps.AzureOpenAI.Models.PostEdit;

namespace Apps.AzureOpenAI.Services.Abstract;

public interface IResponseDeserializationService
{
    DeserializeTranslationEntitiesResult DeserializeResponse(string content);
}
