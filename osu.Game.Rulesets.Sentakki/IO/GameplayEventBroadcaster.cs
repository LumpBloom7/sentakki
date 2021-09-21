using System;
using System.IO.Pipes;

namespace osu.Game.Rulesets.Sentakki.IO
{
    public class GameplayEventBroadcaster : IDisposable
    {
        private readonly NamedPipeServerStream pipeServer;

        // This is used to store the message that needs to be sent
        // In the event that a broadcast fails, we can resend this message once a new connection is established.
        private TransmissionData queuedData;

        private bool isDisposed;

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

                if (queuedData != TransmissionData.Empty)
                    Broadcast(queuedData);
            }
            catch
            {
                // If the pipe is closed before a client ever connects,
                // EndWaitForConnection() will throw an exception.

                // If we are in here that is probably the case so just return.
                return;
            }
        }

        public void Broadcast(TransmissionData packet)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(GameplayEventBroadcaster));

            queuedData = packet;
            if (!connectionValid()) return;

            pipeServer.WriteByte(packet.RawData);
            queuedData = TransmissionData.Empty;
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
            isDisposed = true;
            pipeServer.Dispose();
        }
    }
}
