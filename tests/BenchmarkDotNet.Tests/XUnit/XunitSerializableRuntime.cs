using BenchmarkDotNet.Environments;
using Xunit.Abstractions;

namespace BenchmarkDotNet.Tests.XUnit 
{
    public class XunitSerializableRuntime : IXunitSerializable
    {
        // ReSharper disable once UnusedMember.Global
        // Needed for deserializer
        public XunitSerializableRuntime() { } 

        public XunitSerializableRuntime(Runtime runtime)
        {
            Runtime = runtime;
        }

        public Runtime Runtime { get; private set; }

        public void Deserialize(IXunitSerializationInfo info)
        {
            Runtime = info.GetValue<Runtime>("Runtime");
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue("Runtime", Runtime);
        }

        public override string ToString()
        {
            return "Runtime = " + Runtime.Name;
        }
    }
}