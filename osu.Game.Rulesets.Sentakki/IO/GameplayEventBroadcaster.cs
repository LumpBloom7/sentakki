using System;
using System.IO.Pipes;
using System.Text;

namespace osu.Game.Rulesets.Sentakki.IO
{
    public class GameplayEventBroadcaster : IDisposable
    {
        private NamedPipeServerStream pipeServer;

        public GameplayEventBroadcaster()
        {
            pipeServer = new NamedPipeServerStream("senPipe", PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            onClientDisconnected();
        }

        private bool isWaitingForClient;

        private void onClientDisconnected()
        {
            if (isWaitingForClient) return;

            isWaitingForClient = true;
            pipeServer.BeginWaitForConnection(new AsyncCallback(waitForConnectionCallBack), this);
        }

        private void waitForConnectionCallBack(IAsyncResult result)
        {
            try
            {
                pipeServer.EndWaitForConnection(result);
                isWaitingForClient = false;
            }
            catch
            {
                // If the pipe is closed before a client ever connects,
                // EndWaitForConnection() will throw an exception.

                // If we are in here that is probably the case so just return.
                return;
            }
        }

        public void Broadcast(string message)
        {
            if (!connectionValid()) return;

            var buffer = Encoding.UTF8.GetBytes(message);
            var len = (byte)buffer.Length;
            pipeServer.WriteByte(len);
            pipeServer.Write(buffer, 0, len);
        }

        private bool connectionValid()
        {
            if (isWaitingForClient) return false;

            try { pipeServer.WriteByte(0); }
            catch
            {
                pipeServer.Disconnect();
                onClientDisconnected();

                return false;
            }

            // We assume that connection is valid at this point
            return true;
        }

        public void Dispose()
        {
            pipeServer.Disconnect();
            pipeServer.Dispose();

            isWaitingForClient = true;
        }
    }
}
