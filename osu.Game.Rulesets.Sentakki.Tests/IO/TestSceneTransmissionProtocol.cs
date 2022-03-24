using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Rulesets.Sentakki.IO;

namespace osu.Game.Rulesets.Sentakki.Tests.IO
{
    [HeadlessTest]
    public class TestSceneTransmissionProtocol
    {
        private const TransmissionData.InfoType testtype = TransmissionData.InfoType.Miss;
        private const int testvalue = 7;
        private const byte testbyte = ((byte)testtype << 3) + 7;

        [Test]
        public void TestEncode()
        {
            var encoded = new TransmissionData(testtype, testvalue);
            Assert.AreEqual(testbyte, encoded.RawData);
        }

        [Test]
        public void TestDecode()
        {
            var encoded = new TransmissionData(testbyte);
            Assert.AreEqual(testtype, encoded.Type);
            Assert.AreEqual(testvalue, encoded.Value);
        }

        [Test]
        public void TestKillEquality()
        {
            var packet1 = new TransmissionData(TransmissionData.InfoType.Kill, 7); // Baseline
            var packet2 = new TransmissionData(TransmissionData.InfoType.Kill, 0); // Lightly altered, but should still be considered the same, because of the kill flag

            Assert.AreEqual(packet1, packet2);
        }
    }
}
