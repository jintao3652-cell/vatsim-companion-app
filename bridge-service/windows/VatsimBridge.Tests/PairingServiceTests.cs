using Xunit;
using VatsimBridge.Services;

namespace VatsimBridge.Tests;

public class PairingServiceTests
{
    [Fact]
    public async Task StartPairing_GeneratesValidCode()
    {
        var service = new PairingService();
        var result = await service.StartPairingAsync("test-device");
        
        Assert.NotNull(result);
        Assert.Equal(6, result.Code.Length);
        Assert.All(result.Code, c => Assert.True(char.IsDigit(c) || char.IsUpper(c)));
    }

    [Fact]
    public async Task CompletePairing_WithValidCode_ReturnsToken()
    {
        var service = new PairingService();
        var pairingResult = await service.StartPairingAsync("test-device");
        
        var token = await service.CompletePairingAsync(pairingResult.Code);
        
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public async Task CompletePairing_WithInvalidCode_ReturnsNull()
    {
        var service = new PairingService();
        var token = await service.CompletePairingAsync("INVALID");
        
        Assert.Null(token);
    }

    [Fact]
    public async Task PairingCode_ExpiresAfterTimeout()
    {
        var service = new PairingService();
        var pairingResult = await service.StartPairingAsync("test-device");
        
        await Task.Delay(TimeSpan.FromMinutes(11));
        var token = await service.CompletePairingAsync(pairingResult.Code);
        
        Assert.Null(token);
    }
}
