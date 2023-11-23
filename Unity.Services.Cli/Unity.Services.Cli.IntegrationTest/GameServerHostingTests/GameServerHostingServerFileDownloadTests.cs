using System.Threading.Tasks;
using NUnit.Framework;
using Unity.Services.Cli.Common.Exceptions;
using Unity.Services.Cli.IntegrationTest.Common;
using Unity.Services.Cli.MockServer.Common;

namespace Unity.Services.Cli.IntegrationTest.GameServerHostingTests;

public partial class GameServerHostingTests
{
    static readonly string k_ServerFilesDownloadCommand = "gsh server files download --server-id 1212 --path /logs/error.log --output server.log";

    [Test]
    [Category("gsh")]
    [Category("gsh server")]
    [Category("gsh server files download")]
    public async Task ServerFilesDownload_Succeeds()
    {
        await GetFullySetCli()
            .Command(k_ServerFilesDownloadCommand)
            .AssertStandardOutput(
                str =>
                {
                    Assert.IsTrue(str.Contains("Downloading file..."));
                })
            .ExecuteAsync();
    }

    [Test]
    [Category("gsh")]
    [Category("gsh server")]
    [Category("gsh server files download")]
    public async Task ServerFilesDownload_ThrowsNotLoggedInException()
    {
        SetConfigValue("project-id", CommonKeys.ValidProjectId);
        SetConfigValue("environment-id", CommonKeys.ValidEnvironmentId);
        SetConfigValue("environment-name", CommonKeys.ValidEnvironmentName);

        await new UgsCliTestCase()
            .Command(k_ServerFilesDownloadCommand)
            .AssertExitCode(ExitCode.HandledError)
            .AssertStandardErrorContains(k_NotLoggedIn)
            .ExecuteAsync();
    }

    [Test]
    [Category("gsh")]
    [Category("gsh server")]
    [Category("gsh server files download")]
    public async Task ServerFilesDownload_ThrowsProjectIdNotSetException()
    {
        SetConfigValue("environment-id", CommonKeys.ValidEnvironmentId);
        SetConfigValue("environment-name", CommonKeys.ValidEnvironmentName);
        await GetLoggedInCli()
            .Command(k_ServerFilesDownloadCommand)
            .AssertExitCode(ExitCode.HandledError)
            .AssertStandardErrorContains(k_ProjectIdIsNotSet)
            .ExecuteAsync();
    }

    [Test]
    [Category("gsh")]
    [Category("gsh server")]
    [Category("gsh server files download")]
    public async Task ServerFilesDownload_ThrowsEnvironmentIdNotSetException()
    {
        SetConfigValue("project-id", CommonKeys.ValidProjectId);
        await GetLoggedInCli()
            .Command(k_ServerFilesDownloadCommand)
            .AssertExitCode(ExitCode.HandledError)
            .AssertStandardErrorContains(k_EnvironmentNameIsNotSet)
            .ExecuteAsync();
    }
}