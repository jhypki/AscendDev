using System.Collections.Concurrent;
using AscendDev.Core.Interfaces.CodeExecution;
using AscendDev.Core.Models.CodeExecution;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;

namespace AscendDev.Core.CodeExecution;

public class DockerContainerPool : IDockerContainerPool, IDisposable
{
    private readonly TimeSpan _containerIdleTimeout;
    private readonly ConcurrentDictionary<string, DateTime> _containerLastUsed = new();
    private readonly ConcurrentDictionary<string, ConcurrentQueue<PrewarmedContainer>> _containerPools = new();
    private readonly DockerClient _dockerClient;
    private readonly ILogger<DockerContainerPool> _logger;
    private readonly Timer _maintenanceTimer;
    private readonly int _maxContainersPerPool;
    private readonly int _minContainersPerPool;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _poolSemaphores = new();
    private bool _disposed;

    public DockerContainerPool(
        DockerClient dockerClient,
        ILogger<DockerContainerPool> logger,
        int minContainersPerPool = 2,
        int maxContainersPerPool = 10,
        int maintenanceIntervalSeconds = 60,
        int containerIdleTimeoutMinutes = 10)
    {
        _dockerClient = dockerClient ?? throw new ArgumentNullException(nameof(dockerClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _minContainersPerPool = minContainersPerPool;
        _maxContainersPerPool = maxContainersPerPool;
        _containerIdleTimeout = TimeSpan.FromMinutes(containerIdleTimeoutMinutes);

        // Start the maintenance timer to periodically clean up and replenish containers
        _maintenanceTimer = new Timer(
            MaintenanceCallback,
            null,
            TimeSpan.FromSeconds(maintenanceIntervalSeconds),
            TimeSpan.FromSeconds(maintenanceIntervalSeconds));
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Initializes a pool for a specific language/framework
    /// </summary>
    public async Task InitializePoolAsync(string language, string framework, int initialCount = 0)
    {
        var poolKey = GetPoolKey(language, framework);

        // Initialize pool if it doesn't exist
        EnsurePoolExists(poolKey);

        // If initial count is specified, warm up the pool
        var toCreate = Math.Max(0, initialCount - _containerPools[poolKey].Count);

        if (toCreate > 0)
        {
            _logger.LogInformation("Warming up pool {PoolKey} with {Count} containers", poolKey, toCreate);

            // Create containers in parallel with a limit of maxDegreeOfParallelism
            var tasks = new List<Task>();
            var semaphore = new SemaphoreSlim(4); // Max 4 parallel container creations

            for (var i = 0; i < toCreate; i++)
                tasks.Add(Task.Run(async () =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        var container = await CreateContainerAsync(language, framework);
                        _containerPools[poolKey].Enqueue(container);
                        _containerLastUsed[container.ContainerId] = DateTime.UtcNow;
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));

            await Task.WhenAll(tasks);

            _logger.LogInformation("Pool {PoolKey} warmed up with {Count} containers",
                poolKey, _containerPools[poolKey].Count);
        }
    }

    /// <summary>
    ///     Gets a prewarmed container for the specified language/framework
    /// </summary>
    public async Task<PrewarmedContainer> GetContainerAsync(string language, string framework)
    {
        var poolKey = GetPoolKey(language, framework);

        // Initialize pool if it doesn't exist
        EnsurePoolExists(poolKey);

        // Try to get a container from the pool
        if (_containerPools[poolKey].TryDequeue(out var container))
        {
            _logger.LogDebug("Retrieved prewarmed container {ContainerId} from pool {PoolKey}",
                container.ContainerId, poolKey);

            return container;
        }

        // If no container is available, create a new one
        _logger.LogInformation("No prewarmed container available for {PoolKey}, creating new container", poolKey);
        return await CreateContainerAsync(language, framework);
    }

    /// <summary>
    ///     Returns a container to the pool for reuse, or disposes it if the pool is full
    /// </summary>
    public async Task ReturnContainerAsync(PrewarmedContainer container)
    {
        if (container == null) throw new ArgumentNullException(nameof(container));

        var poolKey = GetPoolKey(container.Language, container.Framework);

        // Initialize pool if it doesn't exist (shouldn't happen, but just in case)
        EnsurePoolExists(poolKey);

        // Check if the pool is full
        if (_containerPools[poolKey].Count >= _maxContainersPerPool)
        {
            // Pool is full, dispose the container
            _logger.LogDebug("Pool {PoolKey} is full, disposing container {ContainerId}",
                poolKey, container.ContainerId);

            await DisposeContainerAsync(container);
            return;
        }

        // Reset the container for reuse
        try
        {
            await ResetContainerAsync(container);

            // Add the container back to the pool
            _containerPools[poolKey].Enqueue(container);
            _containerLastUsed[container.ContainerId] = DateTime.UtcNow;

            _logger.LogDebug("Returned container {ContainerId} to pool {PoolKey}",
                container.ContainerId, poolKey);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to reset container {ContainerId}, disposing it", container.ContainerId);
            await DisposeContainerAsync(container);
        }
    }

    /// <summary>
    ///     Creates a new prewarmed container for the specified language/framework
    /// </summary>
    private async Task<PrewarmedContainer> CreateContainerAsync(string language, string framework)
    {
        // Get the base image for the language/framework
        var baseImage = GetBaseImage(language, framework);

        // Generate a unique container name
        var containerId = Guid.NewGuid().ToString("N");
        var containerName = $"prewarmed-{language}-{framework}-{containerId}";

        try
        {
            // Create the container without starting it
            var createParams = new CreateContainerParameters
            {
                Image = baseImage,
                Name = containerName,
                Tty = true,
                OpenStdin = true,
                AttachStdin = true,
                AttachStdout = true,
                AttachStderr = true,
                // Use a sleep command to keep the container running
                Cmd = new[] { "sh", "-c", "tail -f /dev/null" }
            };

            var createResponse = await _dockerClient.Containers.CreateContainerAsync(createParams);

            // Start the container
            var started = await _dockerClient.Containers.StartContainerAsync(createResponse.ID, null);

            if (!started) throw new InvalidOperationException($"Failed to start container {createResponse.ID}");

            _logger.LogInformation("Created and started prewarmed container {ContainerId} for {Language}/{Framework}",
                createResponse.ID, language, framework);

            return new PrewarmedContainer
            {
                ContainerId = createResponse.ID,
                ContainerName = containerName,
                Language = language,
                Framework = framework,
                BaseImage = baseImage,
                CreatedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create prewarmed container for {Language}/{Framework}",
                language, framework);
            throw;
        }
    }

    /// <summary>
    ///     Resets a container to its initial state so it can be reused
    /// </summary>
    private async Task ResetContainerAsync(PrewarmedContainer container)
    {
        // For simple reset, we can just run a cleanup command
        var execConfig = new ContainerExecCreateParameters
        {
            Cmd = new[] { "sh", "-c", "rm -rf /app/* /app/.[!.]* 2>/dev/null || true" },
            AttachStdout = true,
            AttachStderr = true
        };

        var execId = await _dockerClient.Exec.ExecCreateContainerAsync(
            container.ContainerId, execConfig);

        await _dockerClient.Exec.StartContainerExecAsync(
            execId.ID);

        _logger.LogDebug("Reset container {ContainerId} for reuse", container.ContainerId);
    }

    /// <summary>
    ///     Disposes a container that is no longer needed
    /// </summary>
    private async Task DisposeContainerAsync(PrewarmedContainer container)
    {
        try
        {
            await _dockerClient.Containers.RemoveContainerAsync(
                container.ContainerId,
                new ContainerRemoveParameters
                {
                    Force = true
                });

            _containerLastUsed.TryRemove(container.ContainerId, out _);

            _logger.LogDebug("Disposed container {ContainerId}", container.ContainerId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to dispose container {ContainerId}", container.ContainerId);
        }
    }

    /// <summary>
    ///     Ensures a pool exists for the specified language/framework
    /// </summary>
    private void EnsurePoolExists(string poolKey)
    {
        _containerPools.TryAdd(poolKey, new ConcurrentQueue<PrewarmedContainer>());
        _poolSemaphores.TryAdd(poolKey, new SemaphoreSlim(1, 1));
    }

    /// <summary>
    ///     Gets the pool key for a language/framework combination
    /// </summary>
    private string GetPoolKey(string language, string framework)
    {
        return $"{language.ToLowerInvariant()}_{framework.ToLowerInvariant()}";
    }

    /// <summary>
    ///     Gets the appropriate Docker base image for a language/framework
    /// </summary>
    private string GetBaseImage(string language, string framework)
    {
        // This could be configurable or retrieved from a database/config
        return language.ToLowerInvariant() switch
        {
            "typescript" when framework.ToLowerInvariant() == "jest" => "ascenddev-typescript-runner",
            "javascript" when framework.ToLowerInvariant() == "jest" => "ascenddev-javascript-runner",
            "go" when framework.ToLowerInvariant() == "testing" => "jhypki/ascenddev-go-tester:latest",
            "python" when framework.ToLowerInvariant() == "pytest" => "python:3.10-slim",
            _ => throw new NotSupportedException($"Language {language} with framework {framework} is not supported")
        };
    }

    /// <summary>
    ///     Maintenance callback to clean up idle containers and ensure minimum pool size
    /// </summary>
    private async void MaintenanceCallback(object state)
    {
        try
        {
            _logger.LogDebug("Starting container pool maintenance");

            foreach (var poolEntry in _containerPools)
            {
                var poolKey = poolEntry.Key;
                var pool = poolEntry.Value;

                // Skip if the pool is empty
                if (pool.IsEmpty) continue;

                await _poolSemaphores[poolKey].WaitAsync();
                try
                {
                    // Check for idle containers to remove
                    var now = DateTime.UtcNow;
                    var containers = pool.ToList(); // Create a copy of the queue for inspection
                    pool.Clear(); // Clear the queue to rebuild it

                    foreach (var container in containers)
                        if (_containerLastUsed.TryGetValue(container.ContainerId, out var lastUsed) &&
                            now - lastUsed > _containerIdleTimeout &&
                            pool.Count > _minContainersPerPool)
                        {
                            // Container is idle and we have more than the minimum, dispose it
                            _logger.LogInformation("Removing idle container {ContainerId} from pool {PoolKey}",
                                container.ContainerId, poolKey);

                            await DisposeContainerAsync(container);
                        }
                        else
                        {
                            // Keep the container in the pool
                            pool.Enqueue(container);
                        }

                    // Ensure minimum container count
                    var toCreate = _minContainersPerPool - pool.Count;
                    if (toCreate > 0)
                    {
                        _logger.LogInformation("Replenishing pool {PoolKey} with {Count} containers",
                            poolKey, toCreate);

                        var parts = poolKey.Split('_');
                        if (parts.Length == 2)
                        {
                            var language = parts[0];
                            var framework = parts[1];

                            for (var i = 0; i < toCreate; i++)
                                try
                                {
                                    var container = await CreateContainerAsync(language, framework);
                                    pool.Enqueue(container);
                                    _containerLastUsed[container.ContainerId] = DateTime.UtcNow;
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "Failed to create container for pool {PoolKey}", poolKey);
                                }
                        }
                    }
                }
                finally
                {
                    _poolSemaphores[poolKey].Release();
                }
            }

            _logger.LogDebug("Container pool maintenance completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during container pool maintenance");
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            _maintenanceTimer?.Dispose();

            // Dispose all containers
            Task.Run(async () =>
            {
                foreach (var pool in _containerPools.Values)
                    while (pool.TryDequeue(out var container))
                        await DisposeContainerAsync(container);
            }).GetAwaiter().GetResult();

            foreach (var semaphore in _poolSemaphores.Values) semaphore.Dispose();

            _containerPools.Clear();
            _poolSemaphores.Clear();
            _containerLastUsed.Clear();
        }

        _disposed = true;
    }
}