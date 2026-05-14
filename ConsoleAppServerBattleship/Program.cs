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

Console.WriteLine("Both players connected. Game can begin!");

// Keep server alive
Console.ReadLine();

int currentPlayer = 1; // Player 1 starts

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