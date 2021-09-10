using System;
using System.IO.Pipes;
using System.Security.Principal;
using System.Text;
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
        private SpriteText text;

        public TestSceneGameplayEventBroadcaster()
        {
            Add(text = new SpriteText()
            {
                Text = "Nothing here yet"
            });

            AddStep("Start broadcaster", () => broadcaster = new GameplayEventBroadcaster());
            AddStep("Send message", () => broadcaster.Broadcast("Testing1"));
            AddStep("CreateClient", () => client = new TestBroadcastClient(text));
            AddStep("Send message 1", () => broadcaster.Broadcast("Testing1"));
            AddAssert("Client received message 1", () => text.Text == "Testing1");
            AddStep("Send message 2", () => broadcaster.Broadcast("Testing2"));
            AddAssert("Client received message 2", () => text.Text == "Testing2");
            AddStep("Send message 3", () => broadcaster.Broadcast("Testing3"));
            AddAssert("Client received message 3", () => text.Text == "Testing3");
            AddStep("Kill client", () => { client?.Dispose(); client = null; });
            AddStep("Kill broadcaster", () => broadcaster?.Dispose());
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            client?.Dispose();
            broadcaster?.Dispose();
        }

        private class TestBroadcastClient : IDisposable
        {
            private NamedPipeClientStream pipeServer;

            private SpriteText text;

            private bool running = true;

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
                        var len = pipeServer.ReadByte();
                        if (len > 0)
                        {
                            var buffer = new byte[len];

                            pipeServer.Read(buffer, 0, len);
                            text.Text = Encoding.UTF8.GetString(buffer);
                        }
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
                pipeServer.Close();
                pipeServer.Dispose();
            }
        }
    }
}
