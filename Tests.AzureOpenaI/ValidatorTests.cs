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
        // Arrange
        var validator = new ConnectionValidator();

        // Act
        var result = await validator.ValidateConnection(Creds, CancellationToken.None);
        
        // Assert
        Console.WriteLine(result.Message);
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task ValidateConnection_WithInvalidCredentials_ReturnsInvalid()
    {
        // Arrange
        var validator = new ConnectionValidator();
        var newCreds = Creds.Select(x => new AuthenticationCredentialsProvider(
            x.KeyName, 
            x.Value + "_incorrect"));

        // Act
        var result = await validator.ValidateConnection(newCreds, CancellationToken.None);
        
        // Assert
        Console.WriteLine(result.Message);
        Assert.IsFalse(result.IsValid);
    }
}