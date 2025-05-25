// using AscendDev.Core.CodeExecution;
// using AscendDev.Core.Models.CodeExecution;
// using Docker.DotNet;
// using Docker.DotNet.Models;
// using Microsoft.Extensions.Logging;
// using Moq;
// using NUnit.Framework;
// using System.Collections.Concurrent;
// using System.Reflection;

// namespace AscendDev.Core.Test.TestsExecution;

// [TestFixture]
// public class DockerContainerPoolTests
// {
//     private Mock<DockerClient> _dockerClientMock;
//     private Mock<ILogger<DockerContainerPool>> _loggerMock;
//     private Mock<IContainersOperations> _containersOperationsMock;
//     private Mock<IExecOperations> _execOperationsMock;
//     private DockerContainerPool _containerPool;

//     [SetUp]
//     public void Setup()
//     {
//         _dockerClientMock = new Mock<DockerClient>();
//         _loggerMock = new Mock<ILogger<DockerContainerPool>>();
//         _containersOperationsMock = new Mock<IContainersOperations>();
//         _execOperationsMock = new Mock<IExecOperations>();

//         _dockerClientMock.Setup(x => x.Containers).Returns(_containersOperationsMock.Object);
//         _dockerClientMock.Setup(x => x.Exec).Returns(_execOperationsMock.Object);

//         _containerPool = new DockerContainerPool(
//             _dockerClientMock.Object,
//             _loggerMock.Object,
//             minContainersPerPool: 2,
//             maxContainersPerPool: 5,
//             maintenanceIntervalSeconds: 60,
//             containerIdleTimeoutMinutes: 10);
//     }

//     [TearDown]
//     public void TearDown()
//     {
//         _containerPool.Dispose();
//     }

//     [Test]
//     public void Constructor_WhenDockerClientIsNull_ThrowsArgumentNullException()
//     {
//         // Act & Assert
//         var ex = Assert.Throws<ArgumentNullException>(() => new DockerContainerPool(
//             null,
//             _loggerMock.Object));
//         Assert.That(ex.ParamName, Is.EqualTo("dockerClient"));
//     }

//     [Test]
//     public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
//     {
//         // Act & Assert
//         var ex = Assert.Throws<ArgumentNullException>(() => new DockerContainerPool(
//             _dockerClientMock.Object,
//             null));
//         Assert.That(ex.ParamName, Is.EqualTo("logger"));
//     }

//     [Test]
//     public async Task InitializePoolAsync_CreatesSpecifiedNumberOfContainers()
//     {
//         // Arrange
//         string language = "csharp";
//         string framework = "xunit";
//         int initialCount = 3;

//         SetupContainerCreationMocks();

//         // Act
//         await _containerPool.InitializePoolAsync(language, framework, initialCount);

//         // Assert
//         _containersOperationsMock.Verify(
//             x => x.CreateContainerAsync(
//                 It.IsAny<CreateContainerParameters>(),
//                 It.IsAny<CancellationToken>()),
//             Times.Exactly(initialCount));

//         _containersOperationsMock.Verify(
//             x => x.StartContainerAsync(
//                 It.IsAny<string>(),
//                 It.IsAny<ContainerStartParameters>(),
//                 It.IsAny<CancellationToken>()),
//             Times.Exactly(initialCount));
//     }

//     [Test]
//     public async Task GetContainerAsync_WhenPoolIsEmpty_CreatesNewContainer()
//     {
//         // Arrange
//         string language = "csharp";
//         string framework = "xunit";

//         SetupContainerCreationMocks();

//         // Act
//         var container = await _containerPool.GetContainerAsync(language, framework);

//         // Assert
//         Assert.That(container, Is.Not.Null);
//         Assert.That(container.Language, Is.EqualTo(language));
//         Assert.That(container.Framework, Is.EqualTo(framework));

//         _containersOperationsMock.Verify(
//             x => x.CreateContainerAsync(
//                 It.IsAny<CreateContainerParameters>(),
//                 It.IsAny<CancellationToken>()),
//             Times.Once);

//         _containersOperationsMock.Verify(
//             x => x.StartContainerAsync(
//                 It.IsAny<string>(),
//                 It.IsAny<ContainerStartParameters>(),
//                 It.IsAny<CancellationToken>()),
//             Times.Once);
//     }

//     [Test]
//     public async Task GetContainerAsync_WhenPoolHasContainer_ReturnsExistingContainer()
//     {
//         // Arrange
//         string language = "csharp";
//         string framework = "xunit";
//         string containerId = "test-container-id";

//         // Initialize pool with one container
//         SetupContainerCreationMocks(containerId);
//         await _containerPool.InitializePoolAsync(language, framework, 1);

//         // Reset mocks to verify they're not called again
//         _containersOperationsMock.Invocations.Clear();

//         // Act
//         var container = await _containerPool.GetContainerAsync(language, framework);

//         // Assert
//         Assert.That(container, Is.Not.Null);
//         Assert.That(container.ContainerId, Is.EqualTo(containerId));
//         Assert.That(container.Language, Is.EqualTo(language));
//         Assert.That(container.Framework, Is.EqualTo(framework));

//         _containersOperationsMock.Verify(
//             x => x.CreateContainerAsync(
//                 It.IsAny<CreateContainerParameters>(),
//                 It.IsAny<CancellationToken>()),
//             Times.Never);
//     }

//     [Test]
//     public async Task ReturnContainerAsync_WhenPoolIsFull_DisposesContainer()
//     {
//         // Arrange
//         string language = "csharp";
//         string framework = "xunit";
//         string containerId = "test-container-id";

//         // Create a container to return
//         var container = new PrewarmedContainer
//         {
//             ContainerId = containerId,
//             ContainerName = "test-container",
//             Language = language,
//             Framework = framework,
//             BaseImage = "test-image",
//             CreatedAt = DateTime.UtcNow
//         };

//         // Fill the pool to max capacity
//         var containerPoolsField = typeof(DockerContainerPool).GetField("_containerPools", BindingFlags.NonPublic | BindingFlags.Instance);
//         var containerPools = (ConcurrentDictionary<string, ConcurrentQueue<PrewarmedContainer>>)containerPoolsField.GetValue(_containerPool);
//         var poolKey = $"{language.ToLowerInvariant()}_{framework.ToLowerInvariant()}";
//         containerPools[poolKey] = new ConcurrentQueue<PrewarmedContainer>();

//         // Add max containers to the pool
//         for (int i = 0; i < 5; i++)
//         {
//             containerPools[poolKey].Enqueue(new PrewarmedContainer
//             {
//                 ContainerId = $"existing-container-{i}",
//                 Language = language,
//                 Framework = framework
//             });
//         }

//         // Act
//         await _containerPool.ReturnContainerAsync(container);

//         // Assert
//         _containersOperationsMock.Verify(
//             x => x.RemoveContainerAsync(
//                 containerId,
//                 It.IsAny<ContainerRemoveParameters>(),
//                 It.IsAny<CancellationToken>()),
//             Times.Once);
//     }

//     [Test]
//     public async Task ReturnContainerAsync_WhenPoolIsNotFull_ResetsAndReturnsContainer()
//     {
//         // Arrange
//         string language = "csharp";
//         string framework = "xunit";
//         string containerId = "test-container-id";

//         // Create a container to return
//         var container = new PrewarmedContainer
//         {
//             ContainerId = containerId,
//             ContainerName = "test-container",
//             Language = language,
//             Framework = framework,
//             BaseImage = "test-image",
//             CreatedAt = DateTime.UtcNow
//         };

//         // Setup exec operations for container reset
//         _execOperationsMock.Setup(x => x.ExecCreateContainerAsync(
//                 It.IsAny<string>(),
//                 It.IsAny<ContainerExecCreateParameters>(),
//                 It.IsAny<CancellationToken>()))
//             .ReturnsAsync(new ContainerExecCreateResponse { ID = "exec-id" });

//         // Act
//         await _containerPool.ReturnContainerAsync(container);

//         // Assert
//         _execOperationsMock.Verify(
//             x => x.ExecCreateContainerAsync(
//                 containerId,
//                 It.IsAny<ContainerExecCreateParameters>(),
//                 It.IsAny<CancellationToken>()),
//             Times.Once);

//         _execOperationsMock.Verify(
//             x => x.StartContainerExecAsync(
//                 "exec-id",
//                 It.IsAny<CancellationToken>()),
//             Times.Once);

//         // Verify container was not removed
//         _containersOperationsMock.Verify(
//             x => x.RemoveContainerAsync(
//                 containerId,
//                 It.IsAny<ContainerRemoveParameters>(),
//                 It.IsAny<CancellationToken>()),
//             Times.Never);
//     }

//     [Test]
//     public void ReturnContainerAsync_WhenContainerIsNull_ThrowsArgumentNullException()
//     {
//         // Act & Assert
//         var ex = Assert.ThrowsAsync<ArgumentNullException>(async () =>
//             await _containerPool.ReturnContainerAsync(null));
//         Assert.That(ex.ParamName, Is.EqualTo("container"));
//     }

//     [Test]
//     public async Task ReturnContainerAsync_WhenResetFails_DisposesContainer()
//     {
//         // Arrange
//         string language = "csharp";
//         string framework = "xunit";
//         string containerId = "test-container-id";

//         // Create a container to return
//         var container = new PrewarmedContainer
//         {
//             ContainerId = containerId,
//             ContainerName = "test-container",
//             Language = language,
//             Framework = framework,
//             BaseImage = "test-image",
//             CreatedAt = DateTime.UtcNow
//         };

//         // Setup exec operations to throw exception during reset
//         _execOperationsMock.Setup(x => x.ExecCreateContainerAsync(
//                 It.IsAny<string>(),
//                 It.IsAny<ContainerExecCreateParameters>(),
//                 It.IsAny<CancellationToken>()))
//             .ThrowsAsync(new Exception("Failed to reset container"));

//         // Act
//         await _containerPool.ReturnContainerAsync(container);

//         // Assert
//         _containersOperationsMock.Verify(
//             x => x.RemoveContainerAsync(
//                 containerId,
//                 It.IsAny<ContainerRemoveParameters>(),
//                 It.IsAny<CancellationToken>()),
//             Times.Once);
//     }

//     [Test]
//     public void Dispose_DisposesAllContainers()
//     {
//         // Arrange
//         string language = "csharp";
//         string framework = "xunit";

//         // Add containers to the pool
//         var containerPoolsField = typeof(DockerContainerPool).GetField("_containerPools", BindingFlags.NonPublic | BindingFlags.Instance);
//         var containerPools = (ConcurrentDictionary<string, ConcurrentQueue<PrewarmedContainer>>)containerPoolsField.GetValue(_containerPool);
//         var poolKey = $"{language.ToLowerInvariant()}_{framework.ToLowerInvariant()}";
//         containerPools[poolKey] = new ConcurrentQueue<PrewarmedContainer>();

//         // Add containers to the pool
//         for (int i = 0; i < 3; i++)
//         {
//             containerPools[poolKey].Enqueue(new PrewarmedContainer
//             {
//                 ContainerId = $"container-{i}",
//                 Language = language,
//                 Framework = framework
//             });
//         }

//         // Act
//         _containerPool.Dispose();

//         // Assert
//         _containersOperationsMock.Verify(
//             x => x.RemoveContainerAsync(
//                 It.IsAny<string>(),
//                 It.IsAny<ContainerRemoveParameters>(),
//                 It.IsAny<CancellationToken>()),
//             Times.Exactly(3));
//     }

//     private void SetupContainerCreationMocks(string containerId = "test-container-id")
//     {
//         _containersOperationsMock.Setup(x => x.CreateContainerAsync(
//                 It.IsAny<CreateContainerParameters>(),
//                 It.IsAny<CancellationToken>()))
//             .ReturnsAsync(new CreateContainerResponse { ID = containerId });

//         _containersOperationsMock.Setup(x => x.StartContainerAsync(
//                 It.IsAny<string>(),
//                 It.IsAny<ContainerStartParameters>(),
//                 It.IsAny<CancellationToken>()))
//             .ReturnsAsync(true);
//     }
// }