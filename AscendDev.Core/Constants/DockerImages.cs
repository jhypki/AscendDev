namespace AscendDev.Core.Constants;

public static class DockerImages
{
    // Tester images for running tests
    public const string TypeScriptTester = "jhypki/ascenddev-typescript-tester:latest";
    public const string GoTester = "jhypki/ascenddev-go-tester:latest";
    public const string PythonTester = "jhypki/ascenddev-python-tester:latest";

    // Runner images for code playground
    public const string TypeScriptRunner = "jhypki/ascenddev-typescript-runner:latest";
    public const string JavaScriptRunner = "jhypki/ascenddev-javascript-runner:latest";
    public const string GoRunner = "jhypki/ascenddev-go-runner:latest";
    public const string PythonRunner = "jhypki/ascenddev-python-runner:latest";
}