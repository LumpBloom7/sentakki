using System;
using System.IO;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

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
            pipeServer = new NamedPipeServerStream("senPipe", PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            attemptConnection();
        }

        private bool isWaitingForClient;

        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private CancellationToken cancellationToken => cancellationTokenSource.Token;

        private async void attemptConnection()
        {
            if (isWaitingForClient) return;

            isWaitingForClient = true;

            try { await pipeServer.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false); }
            catch (Exception e)
            {
                // The operation was canceled. Gracefully shutdown;
                if (e is TaskCanceledException || (e is SocketException se && se.SocketErrorCode == SocketError.OperationAborted))
                    return;

                throw;
            }

            isWaitingForClient = false;

            if (queuedData != TransmissionData.Empty)
                Broadcast(queuedData);
        }

        private readonly byte[] buffer = new byte[1];

        public async void Broadcast(TransmissionData packet)
        {
            buffer[0] = packet.RawData;

            if (isDisposed)
                throw new ObjectDisposedException(nameof(GameplayEventBroadcaster));

            queuedData = packet;

            if (isWaitingForClient) return;

            try
            {
                await pipeServer.WriteAsync(new Memory<byte>(buffer), cancellationToken).ConfigureAwait(false);
            }
            catch (IOException)
            {
                // The client has suddenly disconnected, we must disconnect on our end, and wait for a new connection.
                pipeServer.Disconnect();
                attemptConnection();

                return;
            }

            queuedData = TransmissionData.Empty;
        }

        public void Dispose()
        {
            isDisposed = true;
            cancellationTokenSource.Cancel();
            pipeServer.Dispose();
        }
    }
}
