using System;
using System.IO.Pipes;
using System.Security.Principal;
using System.Threading;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Sentakki.IO;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Sentakki.Tests.IO
{
    public class TestSceneGameplayEventBroadcaster : OsuTestScene
    {
        private GameplayEventBroadcaster broadcaster;
        private TestBroadcastClient client;
        private readonly SpriteText text;

        public TestSceneGameplayEventBroadcaster()
        {
            Add(text = new SpriteText()
            {
                Text = "Nothing here yet"
            });

            AddStep("Start broadcaster", () => broadcaster = new GameplayEventBroadcaster());
            AddStep("Send message", () => broadcaster.Broadcast(new TransmissionData(TransmissionData.InfoType.MetaStartPlay, 3)));
            AddStep("Create Client", () => client = new TestBroadcastClient(text));
            AddUntilStep("Client connected", () => client.IsClientConnected);
            AddStep("Send message 1", () => broadcaster.Broadcast(new TransmissionData(TransmissionData.InfoType.HitPerfect, 3)));
            AddAssert("Client received message 1", () => text.Text == new TransmissionData(TransmissionData.InfoType.HitPerfect, 3).ToString());
            AddStep("Send message 2", () => broadcaster.Broadcast(new TransmissionData(TransmissionData.InfoType.MetaEndPlay, 3)));
            AddAssert("Client received message 2", () => text.Text == new TransmissionData(TransmissionData.InfoType.MetaEndPlay, 3).ToString());
            AddStep("Send message 3", () => broadcaster.Broadcast(new TransmissionData(TransmissionData.InfoType.Miss, 3)));
            AddAssert("Client received message 3", () => text.Text == new TransmissionData(TransmissionData.InfoType.Miss, 3).ToString());
            AddStep("Dispose broadcaster", () => broadcaster.Dispose());
        }

        private class TestBroadcastClient : IDisposable
        {
            private NamedPipeClientStream pipeServer;

            private SpriteText text;

            private bool running = true;

            public bool IsClientConnected => pipeServer.IsConnected;

            private Thread readThread;

            public TestBroadcastClient(SpriteText outputText)
            {
                text = outputText;
                pipeServer = new NamedPipeClientStream(".", "senPipe",
                        PipeDirection.In, PipeOptions.Asynchronous,
                        TokenImpersonationLevel.Impersonation);

                readThread = new Thread(clientLoop);
                readThread.Start();
            }

            private void clientLoop()
            {
                while (running)
                {
                    if (!pipeServer.IsConnected)
                        pipeServer.Connect();

                    try
                    {
                        TransmissionData packet = new TransmissionData((byte)pipeServer.ReadByte());
                        if (packet != TransmissionData.Empty)
                            text.Text = packet.ToString();
                    }
                    catch
                    {
                        pipeServer.Close();
                    }
                }
                pipeServer.Close();
                pipeServer.Dispose();
            }

            public void Dispose()
            {
                running = false;
                pipeServer.Dispose();
            }
        }
    }
}
