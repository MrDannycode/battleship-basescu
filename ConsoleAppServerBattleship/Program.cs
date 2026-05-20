using System.Net;
using System.Net.Sockets;
using ConsoleAppServerBattleship;

TcpListener server = new TcpListener(IPAddress.Any, 5000);
server.Start();
Console.WriteLine("Server started. Waiting for players...");

TcpClient client1 = await server.AcceptTcpClientAsync();
Console.WriteLine("Player 1 connected!");

TcpClient client2 = await server.AcceptTcpClientAsync();
Console.WriteLine("Player 2 connected!");

Console.WriteLine("Both players connected. Waiting for players to place ships...");

bool player1Ready = false;
bool player2Ready = false;

while (!player1Ready || !player2Ready)
{
    if (!player1Ready && client1.GetStream().DataAvailable)
    {
        var msg = await NetworkHelper.ReceiveMessageAsync(client1.GetStream());
        if (msg?.Tip == "Ready")
        {
            player1Ready = true;
            Console.WriteLine("Player 1 is ready.");
        }
    }
    if (!player2Ready && client2.GetStream().DataAvailable)
    {
        var msg = await NetworkHelper.ReceiveMessageAsync(client2.GetStream());
        if (msg?.Tip == "Ready")
        {
            player2Ready = true;
            Console.WriteLine("Player 2 is ready.");
        }
    }
    await Task.Delay(100);
}

Console.WriteLine("Both players are ready. Game starts!");

int currentPlayer = 1; // Player 1 starts

await NetworkHelper.SendMessageAsync(client1.GetStream(), new GameMessage { Tip = "Start" });
await NetworkHelper.SendMessageAsync(client2.GetStream(), new GameMessage { Tip = "Start" });

await NetworkHelper.SendMessageAsync(client1.GetStream(), new GameMessage { Tip = "SchimbareTura", JucatorActiv = currentPlayer });
await NetworkHelper.SendMessageAsync(client2.GetStream(), new GameMessage { Tip = "SchimbareTura", JucatorActiv = currentPlayer });

while (true)
{
    // Listen from current player
    NetworkStream activeStream = currentPlayer == 1
        ? client1.GetStream()
        : client2.GetStream();

    NetworkStream otherStream = currentPlayer == 1
        ? client2.GetStream()
        : client1.GetStream();

    GameMessage? message = await NetworkHelper.ReceiveMessageAsync(activeStream);
    if (message == null) break;

    Console.WriteLine($"Player {currentPlayer} attacks ({message.X},{message.Y})");

    // For now, randomly decide Hit or Miss (real logic comes later)
    string status = new Random().Next(2) == 0 ? "Hit" : "Miss";

    // Send result to attacker
    await NetworkHelper.SendMessageAsync(activeStream, new GameMessage
    {
        Tip = "RezultatAtac",
        X = message.X,
        Y = message.Y,
        Status = status
    });

    // Notify other player
    await NetworkHelper.SendMessageAsync(otherStream, new GameMessage
    {
        Tip = "NotificareAtacPrimit",
        X = message.X,
        Y = message.Y
    });

    // Switch turn
    currentPlayer = currentPlayer == 1 ? 2 : 1;

    // Tell both players whose turn it is
    await NetworkHelper.SendMessageAsync(client1.GetStream(), new GameMessage
    {
        Tip = "SchimbareTura",
        JucatorActiv = currentPlayer
    });

    await NetworkHelper.SendMessageAsync(client2.GetStream(), new GameMessage
    {
        Tip = "SchimbareTura",
        JucatorActiv = currentPlayer
    });
}