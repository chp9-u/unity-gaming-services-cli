using System.Threading.Tasks;
using NUnit.Framework;
using Unity.Services.Cli.Common.Exceptions;
using Unity.Services.Cli.IntegrationTest.Common;
using Unity.Services.Cli.MockServer.Common;

namespace Unity.Services.Cli.IntegrationTest.GameServerHostingTests;

public partial class GameServerHostingTests
{
    [Test]
    [Category("gsh")]
    [Category("gsh server")]
    [Category("gsh server files")]
    public async Task ServerFiles_Succeeds()
    {
        await GetFullySetCli()
            .Command("gsh server files list --server-id 123")
            .AssertStandardOutput(
                str =>
                {
                    Assert.IsTrue(str.Contains("Fetching files list..."));
                    Assert.IsTrue(str.Contains("filename: error.log"));
                })
            .AssertNoErrors()
            .ExecuteAsync();
    }

    [Test]
    [Category("gsh")]
    [Category("gsh server")]
    [Category("gsh server files")]
    public async Task ServerFiles_ThrowsNotLoggedInException()
    {
        SetConfigValue("project-id", CommonKeys.ValidProjectId);
        SetConfigValue("environment-id", CommonKeys.ValidEnvironmentId);
        SetConfigValue("environment-name", CommonKeys.ValidEnvironmentName);

        await new UgsCliTestCase()
            .Command("gsh server files list --server-id 123")
            .AssertExitCode(ExitCode.HandledError)
            .AssertStandardErrorContains(k_NotLoggedIn)
            .ExecuteAsync();
    }

    [Test]
    [Category("gsh")]
    [Category("gsh server")]
    [Category("gsh server files")]
    public async Task ServerFiles_ThrowsProjectIdNotSetException()
    {
        SetConfigValue("environment-id", CommonKeys.ValidEnvironmentId);
        SetConfigValue("environment-name", CommonKeys.ValidEnvironmentName);
        await GetLoggedInCli()
            .Command("gsh server files list --server-id 123")
            .AssertExitCode(ExitCode.HandledError)
            .AssertStandardErrorContains(k_ProjectIdIsNotSet)
            .ExecuteAsync();
    }

    [Test]
    [Category("gsh")]
    [Category("gsh server")]
    [Category("gsh server files")]
    public async Task ServerFiles_ThrowsEnvironmentIdNotSetException()
    {
        SetConfigValue("project-id", CommonKeys.ValidProjectId);
        await GetLoggedInCli()
            .Command("gsh server files list --server-id 123")
            .AssertExitCode(ExitCode.HandledError)
            .AssertStandardErrorContains(k_EnvironmentNameIsNotSet)
            .ExecuteAsync();
    }
}
