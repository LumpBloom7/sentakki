using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace osu.Game.Rulesets.Sentakki.Benchmarks;

public static class Program
{
    public static void Main(string[] args)
    {
        BenchmarkSwitcher
            .FromAssembly(typeof(Program).Assembly)
            .Run(args, DefaultConfig.Instance.WithOption(ConfigOptions.DisableOptimizationsValidator, true));
    }
}
