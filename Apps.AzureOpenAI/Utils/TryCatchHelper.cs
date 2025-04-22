using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Apps.AzureOpenAI.Utils;

public static class TryCatchHelper
{
    private static readonly List<string> MisconfigurationErrorMessages =
    [
        "The completion operation does not work with the specified model"
    ];
    
    public static void TryCatch(Action action, string message)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            throw new PluginApplicationException($"Exception message: {ex.Message}. {message}");
        }
    }
    
    public static async Task<T> ExecuteWithErrorHandling<T>(Func<Task<T>> func)
    {
        try
        {
            return await func.Invoke();
        }
        catch (Exception e)
        {
            if (MisconfigurationErrorMessages.Any(x => e.Message.Contains(x)))
            {
                throw new PluginMisconfigurationException(e.Message);
            }

            throw new PluginApplicationException(e.Message);
        }
    }
    
    public static async Task ExecuteWithErrorHandling(Func<Task> func)
    {
        try
        {
            await func.Invoke();
        }
        catch (Exception e)
        {
            if (MisconfigurationErrorMessages.Any(x => e.Message.Contains(x)))
            {
                throw new PluginMisconfigurationException(e.Message);
            }

            throw new PluginApplicationException(e.Message);
        }
    }
}