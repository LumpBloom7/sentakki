using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Rulesets.Sentakki.IO;

namespace osu.Game.Rulesets.Sentakki.Tests.IO
{
    [HeadlessTest]
    public class TestSceneTransmissionProtocol
    {
        private const TransimssionData.InfoType testtype = TransimssionData.InfoType.Miss;
        private const int testvalue = 7;
        private const byte testbyte = ((byte)testtype << 3) + 7;

        [Test]
        public void TestEncode()
        {
            var encoded = new TransimssionData(testtype, testvalue);
            Assert.AreEqual(testbyte, encoded.RawData);
        }

        [Test]
        public void TestDecode()
        {
            var encoded = new TransimssionData(testbyte);
            Assert.AreEqual(testtype, encoded.Type);
            Assert.AreEqual(testvalue, encoded.Value);
        }
    }
}
