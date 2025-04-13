namespace AscendDev.Core.Models.CodeExecution;

public class PrewarmedContainer
{
    public string ContainerId { get; set; }
    public string ContainerName { get; set; }
    public string Language { get; set; }
    public string Framework { get; set; }
    public string BaseImage { get; set; }
    public DateTime CreatedAt { get; set; }
}