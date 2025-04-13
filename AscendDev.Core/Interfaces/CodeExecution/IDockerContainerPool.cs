using AscendDev.Core.Models.CodeExecution;

namespace AscendDev.Core.Interfaces.CodeExecution;

public interface IDockerContainerPool : IDisposable
{
    Task<PrewarmedContainer> GetContainerAsync(string language, string framework);
    Task ReturnContainerAsync(PrewarmedContainer container);
    Task InitializePoolAsync(string language, string framework, int initialCount = 0);
}