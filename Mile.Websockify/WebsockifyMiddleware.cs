using Microsoft.AspNetCore.Http;
using System;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Mile.Websockify
{
    public class WebsockifyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _hostname;
        private readonly int _port;

        public WebsockifyMiddleware(
            RequestDelegate next,
            string hostname,
            int port)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            if (hostname == null)
            {
                throw new ArgumentNullException(nameof(hostname));
            }

            _next = next;
            _hostname = hostname;
            _port = port;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket =
                    await context.WebSockets.AcceptWebSocketAsync();

                TcpClient tcpClient = new TcpClient();

                tcpClient.Connect(_hostname, _port);

                NetworkStream networkStream = tcpClient.GetStream();

                Task receiveTask = Task.Run(async () =>
                {
                    byte[] buffer = new byte[1024 * 1024];

                    try
                    {
                        for (; ; )
                        {
                            int size = await networkStream.ReadAsync(
                                buffer,
                                CancellationToken.None);

                            await webSocket.SendAsync(
                                new ArraySegment<byte>(buffer, 0, size),
                                WebSocketMessageType.Binary,
                                true,
                                CancellationToken.None);
                        }
                    }
                    catch (Exception)
                    {

                    }
                });

                Task sendTask = Task.Run(async () =>
                {
                    WebSocketReceiveResult result = null;
                    byte[] buffer = new byte[1024 * 1024];

                    try
                    {
                        for (; ; )
                        {
                            result = await webSocket.ReceiveAsync(
                                new ArraySegment<byte>(buffer),
                                CancellationToken.None);
                            if (result.CloseStatus.HasValue)
                            {
                                break;
                            }

                            await networkStream.WriteAsync(
                                buffer,
                                0,
                                result.Count,
                                CancellationToken.None);
                        }
                    }
                    catch (Exception)
                    {

                    }

                    networkStream.Close();

                    tcpClient.Close();

                    await webSocket.CloseAsync(
                        result.CloseStatus.Value,
                        result.CloseStatusDescription,
                        CancellationToken.None);
                });

                Task.WaitAll(receiveTask, sendTask);
            }
            else
            {
                context.Response.StatusCode = 400;
            }

            // Call the next delegate/middleware in the pipeline.
            await _next(context);
        }
    }
}
