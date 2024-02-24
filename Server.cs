using System.Net;
using System.Net.WebSockets;
using System.Text;

public class HttpServer
{
    public HttpListener Server;
    public Func<string, string>? OnMessage = null;

    public HttpServer(params string[] _prefixes)
    {
        Server = new HttpListener();
        _prefixes = _prefixes.Length == 0 ? new[] { "http://127.0.0.1:3000/" } : _prefixes;
        _prefixes.ToList().ForEach(i => Server.Prefixes.Add(i));
    }

    public void Start()
    {
        if (Server.IsListening) return;
        Server.Start();
        Task.Run(Running);
    }

    public void Stop()
    {
        Server.Stop();
    }

    void Running()
    {
        Console.WriteLine("[HttpServer] Started running...");
        Console.WriteLine(string.Join(",", Server.Prefixes));

        while (Server.IsListening)
        {
            try
            {
                var context = Server.GetContext();

                var request = context.Request;
                using var input = request.InputStream;

                var buffer = new byte[input.Length];
                input.Read(buffer, 0, buffer.Length);
                var requestBody = Encoding.UTF8.GetString(buffer);

                using var response = context.Response;
                using var output = response.OutputStream;

                var responseBody = OnMessage?.Invoke(requestBody) ?? "";
                var sendBuffer = Encoding.UTF8.GetBytes(responseBody);
                output?.Write(sendBuffer, 0, sendBuffer.Length);
            }
            catch (Exception e) { Console.WriteLine($"[HttpServer] {e.Message}"); }
        }

        Console.WriteLine("[HttpServer] Stoped running.");
    }
}


public class WebSocketServer
{
    public HttpListener Server;
    public WebSocket WS;
    public Action<string>? OnMessage = null;

    public WebSocketServer(params string[] _prefixes)
    {
        Server = new HttpListener();
        _prefixes = _prefixes.Length == 0 ? new string[] { "http://+:5000/" } : _prefixes;
        _prefixes.ToList().ForEach(i => Server.Prefixes.Add(i));
    }

    public void Start()
    {
        if (Server.IsListening) return;
        Server.Start();
        Task.Run(Running);
    }

    public void Stop()
    {
        Server.Stop();
    }

    void Running()
    {
        Console.WriteLine("[WebsocketServer] Started running...");

        while (Server.IsListening)
        {
            try
            {
                var context = Server.GetContext();
                if (!context.Request.IsWebSocketRequest) continue;
                Console.WriteLine($"[WebsocketServer] Connected {context.Request.UserHostName}.");

                WS = context.AcceptWebSocketAsync(null).Result.WebSocket;
                var buffer = new ArraySegment<byte>(new byte[4096]);

                while (WS.State == WebSocketState.Open)
                {
                    var response = WS.ReceiveAsync(buffer, CancellationToken.None).Result;
                    OnMessage?.Invoke(Encoding.UTF8.GetString(buffer.Array, 0, response.Count));
                }

                WS.CloseAsync(WebSocketCloseStatus.Empty, null, CancellationToken.None);
                Console.WriteLine("[WebsocketServer] Connection closed.");
            }
            catch (Exception e) { Console.WriteLine($"[WebsocketServer] {e.Message}"); }
        }

        Console.WriteLine("[WebsocketServer] Stoped running.");
    }

    public void Send(string message)
    {
        if (message == "") return;
        var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
        WS.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
    }
}