namespace AscendDev.Core.Constants;

public static class DockerImages
{
    // TODO rename image names to include "tests", because there will be different images for "tests" and "runner"
    public const string TypeScript = "jhypki/ascenddev-typescript-runner:latest";
    public const string CSharp = "jhypki/ascenddev-csharp-tester:latest";
    public const string Python = "jhypki/ascenddev-python-tester:latest";
}