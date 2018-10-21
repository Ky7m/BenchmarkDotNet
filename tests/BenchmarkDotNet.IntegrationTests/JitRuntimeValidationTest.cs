using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Tests.Loggers;
using BenchmarkDotNet.Tests.XUnit;
using Xunit;
using Xunit.Abstractions;

namespace BenchmarkDotNet.IntegrationTests
{
    public class JitRuntimeValidationTest
    {
        protected readonly ITestOutputHelper Output;

        private class PlatformConfig : ManualConfig
        {
            public PlatformConfig(Runtime runtime, Jit jit, Platform platform)
            {
                Add(new Job(Job.Dry, new EnvironmentMode()
                {
                    Runtime = runtime,
                    Jit = jit,
                    Platform = platform
                }));
            }
        }

        private const string OkCaption = "// OkCaption";
        private const string LegacyJitNotAvailableForMono = "// ERROR:  LegacyJIT is requested but it is not available for Mono";
        private const string RyuJitNotAvailable = "// ERROR:  RyuJIT is requested but it is not available in current environment";
        private const string ToolchainSupportsOnlyRyuJit = "Currently dotnet cli toolchain supports only RyuJit";

        public JitRuntimeValidationTest(ITestOutputHelper outputHelper)
        {
            Output = outputHelper;
        }

        public static TheoryData<Runtime, Jit, Platform, string> DataForWindows = new TheoryData<Runtime, Jit, Platform, string>
        {
            // our CI would need to have Mono installed..
            //{ Runtime.Mono, Jit.LegacyJit, Platform.X86, LegacyJitNotAvailableForMono },
            //{ Runtime.Mono, Jit.LegacyJit, Platform.X64, LegacyJitNotAvailableForMono },
            //{ Runtime.Mono, Jit.RyuJit, Platform.X86, RyuJitNotAvailable },
            //{ Runtime.Mono, Jit.RyuJit, Platform.X64, RyuJitNotAvailable },
            { Runtime.Clr, Jit.LegacyJit, Platform.X86, OkCaption },
            { Runtime.Clr, Jit.LegacyJit, Platform.X64, OkCaption },
            { Runtime.Clr, Jit.RyuJit, Platform.X86, RyuJitNotAvailable },
            { Runtime.Clr, Jit.RyuJit, Platform.X64, OkCaption },
        };

        public static TheoryData<XunitSerializableRuntime, Jit, Platform, string> DataForCore = new TheoryData<XunitSerializableRuntime, Jit, Platform, string>
        {
            { new XunitSerializableRuntime(Runtime.Core), Jit.LegacyJit, Platform.X86, ToolchainSupportsOnlyRyuJit },
            { new XunitSerializableRuntime(Runtime.Core), Jit.LegacyJit, Platform.X64, ToolchainSupportsOnlyRyuJit },
            { new XunitSerializableRuntime(Runtime.Core), Jit.RyuJit, Platform.X64, OkCaption }
        };


        [TheoryWindowsOnly("CLR is a valid job only on Windows")]
        [MemberData(nameof(DataForWindows))]
        public void CheckWindows(Runtime runtime, Jit jit, Platform platform, string expectedText)
        {
            Verify(runtime, jit, platform, expectedText);
        }

        [Theory]
        [MemberData(nameof(DataForCore))]
        public void CheckCore(Runtime runtime, Jit jit, Platform platform, string expectedText)
        {
            Verify(runtime, jit, platform, expectedText);
        }

        private void Verify(Runtime runtime, Jit jit, Platform platform, string expectedText)
        {
            var logger = new OutputLogger(Output);
            var config = new PlatformConfig(runtime, jit, platform).With(logger).With(DefaultColumnProviders.Instance);

            BenchmarkRunner.Run(new[] { BenchmarkConverter.TypeToBenchmarks(typeof(TestBenchmark), config) }, config);

            Assert.Contains(expectedText, logger.GetLog());
        }

        public class TestBenchmark
        {
            [Benchmark]
            public void Benchmark()
            {
                Console.WriteLine(OkCaption);
            }
        }
    }
}