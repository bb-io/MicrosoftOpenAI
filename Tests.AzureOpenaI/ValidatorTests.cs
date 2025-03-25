using Apps.AzureOpenAI.Connections;
using Blackbird.Applications.Sdk.Common.Authentication;
using Tests.AzureOpenAI.Base;

namespace Tests.AzureOpenAI;

[TestClass]
public class ConnectionValidatorTests : TestBase
{
    [TestMethod]
    public async Task ValidateConnection_WithValidCredentials_ReturnsValid()
    {
        var validator = new ConnectionValidator();

        var result = await validator.ValidateConnection(Creds, CancellationToken.None);
        Console.WriteLine(result.Message);
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task ValidateConnection_WithInvalidCredentials_ReturnsInvalid()
    {
        var validator = new ConnectionValidator();

        var newCreds = Creds.Select(x => new AuthenticationCredentialsProvider(x.KeyName, x.Value + "_incorrect"));
        var result = await validator.ValidateConnection(newCreds, CancellationToken.None);
        Console.WriteLine(result.Message);
        Assert.IsFalse(result.IsValid);
    }
}