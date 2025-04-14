using System.Net.Http;
using BenchmarkDotNet.Running;
using Benchmarks.Http.Clients;

internal class Program
{
    private static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<Native>();
    }
}