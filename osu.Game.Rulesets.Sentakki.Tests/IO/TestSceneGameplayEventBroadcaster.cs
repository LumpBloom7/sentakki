using System;
using System.IO.Pipes;
using System.Security.Principal;
using System.Threading;
using NUnit.Framework;
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
        }

        [SetUp]
        public void SetUpClient()
        {
            // Dispose existing broadcaster and client
            client?.Dispose();
            broadcaster?.Dispose();
        }

        [Test]
        public void TestNormalOperation()
        {
            AddStep("Start broadcaster", () => broadcaster = new GameplayEventBroadcaster());
            AddStep("Create Client", () => client = new TestBroadcastClient(text));
            AddUntilStep("Client connected", () => client.IsClientConnected);
            AddStep("Send message 1", () => broadcaster.Broadcast(new TransmissionData(TransmissionData.InfoType.HitPerfect, 3)));
            AddUntilStep("Client received message 1", () => text.Text == new TransmissionData(TransmissionData.InfoType.HitPerfect, 3).ToString());
            AddStep("Send message 2", () => broadcaster.Broadcast(new TransmissionData(TransmissionData.InfoType.MetaEndPlay, 3)));
            AddUntilStep("Client received message 2", () => text.Text == new TransmissionData(TransmissionData.InfoType.MetaEndPlay, 3).ToString());
            AddStep("Send message 3", () => broadcaster.Broadcast(new TransmissionData(TransmissionData.InfoType.Miss, 3)));
            AddUntilStep("Client received message 3", () => text.Text == new TransmissionData(TransmissionData.InfoType.Miss, 3).ToString());
            AddStep("Client disconnect", () => client?.Dispose());
            AddStep("Dispose broadcaster", () => broadcaster.Dispose());
        }

        [Test]
        public void TestOperationWithoutClient()
        {
            AddStep("Start broadcaster", () => broadcaster = new GameplayEventBroadcaster());
            AddStep("Send message 1", () => broadcaster.Broadcast(new TransmissionData(TransmissionData.InfoType.HitPerfect, 3)));
            AddStep("Dispose broadcaster", () => broadcaster.Dispose());
        }

        [Test]
        public void TestOperationWithClientDisconnect()
        {
            AddStep("Start broadcaster", () => broadcaster = new GameplayEventBroadcaster());
            AddStep("Create Client", () => client = new TestBroadcastClient(text));
            AddUntilStep("Client connected", () => client.IsClientConnected);
            AddStep("Client disconnect", () => client?.Dispose());
            AddStep("Send message 1", () => broadcaster.Broadcast(new TransmissionData(TransmissionData.InfoType.HitPerfect, 3)));
            AddStep("Dispose broadcaster", () => broadcaster.Dispose());
        }

        // This is just to ensure my sample client implementation holds up
        // So others can be confident they aren't getting a sample that doesn't work
        [Test]
        public void TestClientOperationWithServerReconnect()
        {
            AddStep("Start broadcaster", () => broadcaster = new GameplayEventBroadcaster());
            AddStep("Create Client", () => client = new TestBroadcastClient(text));
            AddUntilStep("Client connected", () => client.IsClientConnected);
            AddStep("Dispose broadcaster", () => broadcaster.Dispose());
            AddStep("Start new broadcaster", () => broadcaster = new GameplayEventBroadcaster());
            AddUntilStep("Client connected", () => client.IsClientConnected);
            AddStep("Send message 1", () => broadcaster.Broadcast(new TransmissionData(TransmissionData.InfoType.HitPerfect, 3)));
            AddUntilStep("Client received message 1", () => text.Text == new TransmissionData(TransmissionData.InfoType.HitPerfect, 3).ToString());
            AddStep("Dispose broadcaster", () => broadcaster.Dispose());
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            client?.Dispose();
            broadcaster?.Dispose();
        }

        private class TestBroadcastClient : IDisposable
        {
            private readonly NamedPipeClientStream pipeClient;

            private readonly SpriteText text;

            private bool running = true;

            public bool IsClientConnected => pipeClient.IsConnected;

            private readonly Thread readThread;

            public TestBroadcastClient(SpriteText outputText)
            {
                text = outputText;
                pipeClient = new NamedPipeClientStream(".", "senPipe",
                        PipeDirection.In, PipeOptions.Asynchronous,
                        TokenImpersonationLevel.Impersonation);

                readThread = new Thread(clientLoop);
                readThread.Start();
            }

            private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            private CancellationToken cancellationToken => cancellationTokenSource.Token;

            private void clientLoop()
            {
                while (running)
                {
                    try
                    {
                        if (!pipeClient.IsConnected)
                            pipeClient.ConnectAsync().Wait(cancellationToken);

                        TransmissionData packet = new TransmissionData((byte)pipeClient.ReadByte());

                        // Server has shut down
                        if (packet == TransmissionData.Kill)
                            continue;

                        if (packet != TransmissionData.Empty)
                            text.Text = packet.ToString();
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }

            public void Dispose()
            {
                running = false;
                cancellationTokenSource.Cancel();
                pipeClient.Dispose();
            }
        }
    }
}
