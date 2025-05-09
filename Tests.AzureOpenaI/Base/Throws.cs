﻿using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Tests.AzureOpenAI.Base;
public static class Throws
{
    public async static Task ApplicationException(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (PluginApplicationException ex)
        {
            Console.WriteLine(ex.Message);
            return;
        }
        Assert.Fail();
    }

    public async static Task MisconfigurationException(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (PluginMisconfigurationException ex)
        {
            Console.WriteLine(ex.Message);
            return;
        }
        Assert.Fail();
    }
}
