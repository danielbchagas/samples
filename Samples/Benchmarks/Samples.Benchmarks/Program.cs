using System.Net.Http;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Samples.Benchmarks.Http.Clients;

internal class Program
{
    private static void Main(string[] args)
    {
        var config = ManualConfig.CreateEmpty()
            .WithArtifactsPath(Path.Combine(Directory.GetCurrentDirectory(), "Artifacts"));

        var summary = BenchmarkRunner.Run<Native>(config);
    }
}