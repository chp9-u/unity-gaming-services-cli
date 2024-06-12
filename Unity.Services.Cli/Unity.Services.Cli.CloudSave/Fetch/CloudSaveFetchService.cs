using Spectre.Console;
using Unity.Services.Cli.Authoring.Input;
using Unity.Services.Cli.Authoring.Service;
using Unity.Services.DeploymentApi.Editor;
using Unity.Services.CloudSave.Authoring.Core.Fetch;
using Unity.Services.CloudSave.Authoring.Core.IO;
using Unity.Services.CloudSave.Authoring.Core.Model;
using Unity.Services.CloudSave.Authoring.Core.Service;
using FetchResult = Unity.Services.Cli.Authoring.Model.FetchResult;

namespace Unity.Services.Cli.CloudSave.Fetch;

class CloudSaveFetchService : IFetchService
{
    readonly ICloudSaveFetchHandler m_FetchHandler;
    readonly ICloudSaveClient m_Client;
    readonly ICloudSaveSimpleResourceLoader m_ResourceLoader;
    readonly string m_ServiceType = "CloudSave";
    readonly string m_ServiceName = "module-template";

    public CloudSaveFetchService(
        ICloudSaveFetchHandler fetchHandler,
        ICloudSaveClient client,
        ICloudSaveSimpleResourceLoader resourceLoader)
    {
        m_FetchHandler = fetchHandler;
        m_Client = client;
        m_ResourceLoader = resourceLoader;
    }

    public string ServiceType => m_ServiceType;
    public string ServiceName => m_ServiceName;

    public IReadOnlyList<string> FileExtensions => new[]
    {
        Constants.SimpleFileExtension
    };

    public async Task<FetchResult> FetchAsync(
        FetchInput input,
        IReadOnlyList<string> filePaths,
        string projectId,
        string environmentId,
        StatusContext? loadingContext,
        CancellationToken cancellationToken)
    {
        await m_Client.Initialize(environmentId, projectId, cancellationToken);
        loadingContext?.Status($"Reading {m_ServiceType} files...");
        var (loadedSuccessfully, failedToLoad) = await GetResourcesFromFiles(filePaths);

        loadingContext?.Status($"Fetching {m_ServiceType} files...");
        var res = await m_FetchHandler.FetchAsync(
            input.Path,
            loadedSuccessfully,
            input.DryRun,
            input.Reconcile,
            cancellationToken);

        return new SimpleFetchResult(
            res.Fetched.Concat(failedToLoad).ToList(),
            ServiceType,
            input.DryRun
        );
    }

    async Task<(IReadOnlyList<IResourceDeploymentItem>,  IReadOnlyList<IResourceDeploymentItem>)> GetResourcesFromFiles(IReadOnlyList<string> filePaths)
    {
        var resources = await Task.WhenAll(filePaths.Select(f => m_ResourceLoader.ReadResource(f, CancellationToken.None)));

        return (resources.Where(r => r.Status.MessageSeverity != SeverityLevel.Error).ToList(),
            resources.Where(r => r.Status.MessageSeverity == SeverityLevel.Error).ToList());
    }

    class SimpleFetchResult : FetchResult
    {
        public SimpleFetchResult(IReadOnlyList<IDeploymentItem> authored, string service, bool dryRun)
            : base(GetItemsOfType(authored, Constants.Updated),
                GetItemsOfType(authored, Constants.Deleted),
                GetItemsOfType(authored, Constants.Created),
                GetItemsOfType(authored, string.Empty),
                authored.Where(f => f.Status.MessageSeverity == SeverityLevel.Error).ToList(),
                dryRun) { }

        static IReadOnlyList<IDeploymentItem> GetItemsOfType(IReadOnlyList<IDeploymentItem> source, string action)
        {
            return source.Where(f => IsDeployedPredicate(f, action)).ToList();
        }

        static bool IsDeployedPredicate(IDeploymentItem item, string action)
        {
            return item.Status.MessageSeverity == SeverityLevel.Success
                   && (item.Status.MessageDetail?.StartsWith(action) ?? false);
        }
    }
}
