
var http = new HttpServer();
http.Start();
http.OnMessage = (e) =>
{
    if (e != "") Console.WriteLine(e);
    if (e == "") http.Text = "say hi!";
    else http.Text = "hello!";
};


var ws = new WebSocketServer();
ws.Start();
ws.OnMessage = (e) =>
{
    if (e != "") Console.WriteLine(e);
    if (e == "") ws.Send("say hi!");
    else ws.Send("hello!");
};

Console.Read();