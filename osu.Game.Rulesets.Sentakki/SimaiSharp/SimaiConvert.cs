using System.IO;
using System.Text;
using SimaiSharp.Internal.LexicalAnalysis;
using SimaiSharp.Internal.SyntacticAnalysis;
using SimaiSharp.Structures;

namespace SimaiSharp
{
    /// <summary>
    /// Handles simai chart conversion to and from different formats
    /// </summary>
    public static class SimaiConvert
    {
        public static MaiChart Deserialize(string value)
        {
            var tokens = new Tokenizer(value).GetTokens();
            var chart = new Deserializer(tokens).GetChart();

            return chart;
        }

        public static string Serialize(MaiChart chart)
        {
            var serializer = new Serializer();
            var stringBuilder = new StringBuilder();
            using var stringWriter = new StringWriter(stringBuilder);
            serializer.Serialize(chart, stringWriter);
            return stringBuilder.ToString();
        }
    }
}
