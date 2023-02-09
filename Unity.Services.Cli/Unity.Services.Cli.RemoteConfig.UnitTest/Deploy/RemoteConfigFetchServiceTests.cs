using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Spectre.Console;
using Unity.Services.Cli.Common.Utils;
using Unity.Services.Cli.Deploy.Input;
using Unity.Services.Cli.Deploy.Model;
using Unity.Services.Cli.Deploy.Service;
using Unity.Services.Cli.RemoteConfig.Deploy;
using Unity.Services.Cli.RemoteConfig.Service;
using Unity.Services.RemoteConfig.Editor.Authoring.Core.Deployment;
using Unity.Services.RemoteConfig.Editor.Authoring.Core.ErrorHandling;
using Unity.Services.RemoteConfig.Editor.Authoring.Core.Fetch;
using Unity.Services.RemoteConfig.Editor.Authoring.Core.Model;

namespace Unity.Services.Cli.RemoteConfig.UnitTest.Deploy;

[TestFixture]
public class RemoteConfigFetchServiceTests
{
    RemoteConfigFetchService? m_RemoteConfigDeploymentService;
    const string k_ValidProjectId = "a912b1fd-541d-42e1-89f2-85436f27aabd";
    const string k_ValidEnvironmentId = "00000000-0000-0000-0000-000000000000";
    const string k_DeployFileExtension = ".rc";

    static readonly List<string> k_ValidFilePaths = new()
    {
        "test_a.rc",
        "test_b.rc"
    };

    private List<IRemoteConfigFile> m_RemoteConfigFiles = new();

    readonly Mock<IUnityEnvironment> m_MockUnityEnvironment = new();
    readonly Mock<ICliRemoteConfigClient> m_MockCliRemoteConfigClient = new();
    readonly Mock<IDeployFileService> m_MockDeployFileService = new();
    readonly Mock<IRemoteConfigScriptsLoader> m_MockRemoteConfigScriptsLoader = new();
    readonly Mock<IRemoteConfigFetchHandler> m_MockRemoteConfigFetchHandler = new();

    readonly Mock<IRemoteConfigServicesWrapper> m_MockRemoteConfigServicesWrapper = new();
    readonly Mock<ILogger> m_MockLogger = new();

    FetchInput m_DefaultInput = new()
    {
        CloudProjectId = k_ValidProjectId ,
        Reconcile = false
    };

    private Result m_Result;

    [SetUp]
    public void SetUp()
    {
        m_MockUnityEnvironment.Reset();
        m_MockCliRemoteConfigClient.Reset();
        m_MockDeployFileService.Reset();
        m_MockRemoteConfigScriptsLoader.Reset();
        m_MockRemoteConfigFetchHandler.Reset();
        m_MockRemoteConfigServicesWrapper.Reset();
        m_MockLogger.Reset();

        m_RemoteConfigDeploymentService =
            new RemoteConfigFetchService(
                m_MockUnityEnvironment.Object,
                m_MockRemoteConfigFetchHandler.Object,
                m_MockCliRemoteConfigClient.Object,
                m_MockDeployFileService.Object,
                m_MockRemoteConfigScriptsLoader.Object);

        m_MockUnityEnvironment.Setup(x => x.FetchIdentifierAsync())
            .ReturnsAsync(k_ValidEnvironmentId);
        m_MockDeployFileService.Setup(d => d.ListFilesToDeploy(new [] {m_DefaultInput.Path}, k_DeployFileExtension))
            .Returns(k_ValidFilePaths);

        m_RemoteConfigFiles = new List<IRemoteConfigFile>(k_ValidFilePaths.Count);
        foreach (var filePath in k_ValidFilePaths)
        {
            var rcFile = new RemoteConfigFile(filePath, filePath, new RemoteConfigFileContent());
            m_RemoteConfigFiles.Add(rcFile);
        }

        m_Result = new Result(
            Array.Empty<(string,string)>(),
            new [] {("updated key", "updated file")},
            new [] {("deleted key", "deleted file")},
            m_RemoteConfigFiles,
            Array.Empty<IRemoteConfigFile>()
        );
        m_MockRemoteConfigFetchHandler
            .Setup(ex => ex
                .FetchAsync(
                    It.IsAny<string>(),
                    It.IsAny<IReadOnlyList<IRemoteConfigFile>>(),
                    false,
                    false,
                    CancellationToken.None))
            .Returns(Task.FromResult<Result>(m_Result));
    }

    [Test]
    public void FetchAsync_MapsResultProperly()
    {
        var res = m_RemoteConfigDeploymentService!.FetchAsync(
            m_DefaultInput,
            (StatusContext)null!,
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(res.Result.Deleted, Has.Count.EqualTo(1));
            Assert.That(res.Result.Updated, Has.Count.EqualTo(1));
            Assert.That(res.Result.Created, Is.Empty);
        });
    }

    [Test]
    public void FetchAsync_FormatsKeyFilePair()
    {
        var res = m_RemoteConfigDeploymentService!.FetchAsync(
            m_DefaultInput,
            (StatusContext)null!,
            CancellationToken.None);

        var deletedKeyStr = string.Format(
            m_RemoteConfigDeploymentService.m_KeyFileMessageFormat,
            m_Result.Deleted[0].Key,
            m_Result.Deleted[0].File);

        var updatedKeyStr = string.Format(
            m_RemoteConfigDeploymentService.m_KeyFileMessageFormat,
            m_Result.Updated[0].Key,
            m_Result.Updated[0].File);


        Assert.Multiple(() =>
        {
            Assert.That(res.Result.Deleted[0], Is.EqualTo(deletedKeyStr));
            Assert.That(res.Result.Updated[0], Is.EqualTo(updatedKeyStr));
        });
    }
}