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

        [Test]
        public void TestNormalOperation()
        {
            AddStep("Start broadcaster", () => createBroadcaster());
            AddStep("Create Client", () => createTestClient());
            AddUntilStep("Client connected", () => client.IsClientConnected);
            AddStep("Send message 1", () => broadcaster.Broadcast(new TransmissionData(TransmissionData.InfoType.HitPerfect, 3)));
            AddUntilStep("Client received message 1", () => text.Text == new TransmissionData(TransmissionData.InfoType.HitPerfect, 3).ToString());
            AddStep("Send message 2", () => broadcaster.Broadcast(new TransmissionData(TransmissionData.InfoType.MetaEndPlay, 3)));
            AddUntilStep("Client received message 2", () => text.Text == new TransmissionData(TransmissionData.InfoType.MetaEndPlay, 3).ToString());
            AddStep("Send message 3", () => broadcaster.Broadcast(new TransmissionData(TransmissionData.InfoType.Miss, 3)));
            AddUntilStep("Client received message 3", () => text.Text == new TransmissionData(TransmissionData.InfoType.Miss, 3).ToString());
            AddStep("Dispose client", () => client?.Dispose());
            AddStep("Dispose broadcaster", () => broadcaster.Dispose());
        }

        [Test]
        public void TestOperationWithoutClient()
        {
            AddStep("Start broadcaster", () => createBroadcaster());
            AddStep("Send message 1", () => broadcaster.Broadcast(new TransmissionData(TransmissionData.InfoType.HitPerfect, 3)));
            AddStep("Dispose broadcaster", () => broadcaster.Dispose());
        }

        [Test]
        public void TestOperationWithClientDisconnect()
        {
            AddStep("Start broadcaster", () => createBroadcaster());
            AddStep("Create Client", () => createTestClient());
            AddUntilStep("Client connected", () => client.IsClientConnected);
            AddStep("Client disconnect", () => client?.Dispose());
            AddStep("Send message 1", () => broadcaster.Broadcast(new TransmissionData(TransmissionData.InfoType.HitPerfect, 3)));
            AddStep("Dispose broadcaster", () => broadcaster.Dispose());
        }

        [Test]
        public void TestRetryBroadcastOnClientReconnect()
        {
            AddStep("Start broadcaster", () => createBroadcaster());
            AddStep("Send message 1", () => broadcaster.Broadcast(new TransmissionData(TransmissionData.InfoType.HitPerfect, 3)));
            AddStep("Create Client", () => createTestClient());
            AddUntilStep("Client connected", () => client.IsClientConnected);
            AddUntilStep("Client received message 1", () => text.Text == new TransmissionData(TransmissionData.InfoType.HitPerfect, 3).ToString());
            AddStep("Dispose broadcaster", () => broadcaster.Dispose());
            AddStep("Dispose client", () => client?.Dispose());
        }

        // This is just to ensure my sample client implementation holds up
        // So others can be confident they aren't getting a sample that doesn't work
        [Test]
        public void TestClientOperationWithServerReconnect()
        {
            AddStep("Start broadcaster", () => createBroadcaster());
            AddStep("Create Client", () => createTestClient());
            AddUntilStep("Client connected", () => client.IsClientConnected);
            AddStep("Dispose broadcaster", () => broadcaster.Dispose());
            AddStep("Start new broadcaster", () => broadcaster = new GameplayEventBroadcaster());
            AddUntilStep("Client connected", () => client.IsClientConnected);
            AddStep("Send message 1", () => broadcaster.Broadcast(new TransmissionData(TransmissionData.InfoType.HitPerfect, 3)));
            AddUntilStep("Client received message 1", () => text.Text == new TransmissionData(TransmissionData.InfoType.HitPerfect, 3).ToString());
            AddStep("Dispose broadcaster", () => broadcaster.Dispose());
            AddStep("Dispose client", () => client?.Dispose());
        }

        private void createBroadcaster()
        {
            broadcaster?.Dispose();
            broadcaster = new GameplayEventBroadcaster();
        }

        private void createTestClient()
        {
            client?.Dispose();
            client = new TestBroadcastClient(text);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            client?.Dispose();
            broadcaster?.Dispose();
        }

        private class TestBroadcastClient : IDisposable
        {
            private NamedPipeClientStream pipeClient;

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

            private async void clientLoop()
            {
                byte[] buffer = new byte[1];
                while (running)
                {
                    try
                    {
                        if (!pipeClient.IsConnected)
                            await pipeClient.ConnectAsync(cancellationToken).ConfigureAwait(false);

                        int result = await pipeClient.ReadAsync(new Memory<byte>(buffer), cancellationToken).ConfigureAwait(false);

                        if (result > 0)
                        {
                            TransmissionData packet = new TransmissionData(buffer[0]);

                            if (packet != TransmissionData.Empty)
                                text.Text = packet.ToString();
                        }
                        else if (result == 0) // End of stream reached, meaning that the server disconnected
                        {
                            text.Text = TransmissionData.Kill.ToString();

                            // On non-Windows platforms, the client doesn't automatically reconnect
                            // So we must recreate the client to ensure safety;
                            pipeClient.Dispose();
                            pipeClient = new NamedPipeClientStream(".", "senPipe",
                                    PipeDirection.In, PipeOptions.Asynchronous,
                                    TokenImpersonationLevel.Impersonation);
                        }
                    }
                    catch
                    {
                        break;
                    }
                }
            }

            public void Dispose()
            {
                running = false;
                cancellationTokenSource.Cancel();
                readThread.Join();
                pipeClient.Dispose();
            }
        }
    }
}
