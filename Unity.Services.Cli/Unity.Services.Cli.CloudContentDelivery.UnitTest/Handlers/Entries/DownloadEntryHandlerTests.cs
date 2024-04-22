using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Spectre.Console;
using Unity.Services.Cli.CloudContentDelivery.Handlers.Entries;
using Unity.Services.Cli.CloudContentDelivery.Input;
using Unity.Services.Cli.CloudContentDelivery.Service;
using Unity.Services.Cli.Common.Console;
using Unity.Services.Cli.Common.Utils;

namespace CloudContentDeliveryTest.Handlers.Entries;

[TestFixture]
public class DownloadEntryHandlerTests
{
    readonly Mock<IUnityEnvironment> m_MockUnityEnvironment = new();
    readonly Mock<IEntryClient> m_MockEntryClient = new();
    readonly Mock<IBucketClient> m_MockBucketClient = new();
    readonly Mock<ILogger> m_MockLogger = new();

    [SetUp]
    public void Setup()
    {
        m_MockUnityEnvironment.Reset();
        m_MockEntryClient.Reset();
        m_MockLogger.Reset();

        m_MockBucketClient.Setup(
                c =>
                    c.GetBucketIdByName(
                        CloudContentDeliveryTestsConstants.ProjectId,
                        CloudContentDeliveryTestsConstants.EnvironmentId,
                        CloudContentDeliveryTestsConstants.BucketName,
                        CancellationToken.None))
            .ReturnsAsync(CloudContentDeliveryTestsConstants.BucketId);
    }

    [Test]
    public async Task DownloadAsync_CallsLoadingIndicatorStartLoading()
    {
        var input = new CloudContentDeliveryInput();
        var mockLoadingIndicator = new Mock<ILoadingIndicator>();
        var cancellationToken = CancellationToken.None;

        mockLoadingIndicator.Setup(
                li => li.StartLoadingAsync(It.IsAny<string>(), It.IsAny<Func<StatusContext?, Task>>()))
            .Returns(Task.CompletedTask);
        await DownloadEntryHandler.DownloadAsync(
            input,
            m_MockUnityEnvironment.Object,
            m_MockEntryClient.Object,
            m_MockBucketClient.Object,
            m_MockLogger.Object,
            mockLoadingIndicator.Object,
            cancellationToken);
        mockLoadingIndicator.Verify(
            ex => ex.StartLoadingAsync(It.IsAny<string>(), It.IsAny<Func<StatusContext?, Task>>()),
            Times.Once);
    }

    [Test]
    public async Task DownloadHandler_ValidInputLogsResult()
    {
        var cloudContentDeliveryInput = new CloudContentDeliveryInput
        {
            CloudProjectId = CloudContentDeliveryTestsConstants.ProjectId,
            BucketNameOpt = CloudContentDeliveryTestsConstants.BucketName,
            EntryPath = CloudContentDeliveryTestsConstants.Path,
            VersionId = CloudContentDeliveryTestsConstants.VersionId

        };
        var cancellationToken = CancellationToken.None;

        m_MockEntryClient.Setup(
                c =>
                    c.DownloadEntryAsync(
                        CloudContentDeliveryTestsConstants.ProjectId,
                        CloudContentDeliveryTestsConstants.EnvironmentId,
                        CloudContentDeliveryTestsConstants.BucketId,
                        CloudContentDeliveryTestsConstants.Path,
                        CloudContentDeliveryTestsConstants.VersionId,
                        CancellationToken.None))
            .ReturnsAsync(new MemoryStream());

        m_MockUnityEnvironment.Setup(x => x.FetchIdentifierAsync(CancellationToken.None))
            .ReturnsAsync(CloudContentDeliveryTestsConstants.EnvironmentId);

        await DownloadEntryHandler.DownloadAsync(
            cloudContentDeliveryInput,
            m_MockUnityEnvironment.Object,
            m_MockEntryClient.Object,
            m_MockBucketClient.Object,
            m_MockLogger.Object,
            cancellationToken);
        m_MockUnityEnvironment.Verify(x => x.FetchIdentifierAsync(cancellationToken), Times.Once);
        m_MockEntryClient.Verify(
            api =>
                api.DownloadEntryAsync(
                    CloudContentDeliveryTestsConstants.ProjectId,
                    CloudContentDeliveryTestsConstants.EnvironmentId,
                    CloudContentDeliveryTestsConstants.BucketId,
                    CloudContentDeliveryTestsConstants.Path,
                    CloudContentDeliveryTestsConstants.VersionId,
                    cancellationToken),
            Times.Once);

    }
}
