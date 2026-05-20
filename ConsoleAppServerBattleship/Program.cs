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

while (true)
{
    Console.WriteLine("Waiting for players to place ships...");
    
    bool player1Ready = false;
    bool player2Ready = false;
    int[][] player1Board = null;
    int[][] player2Board = null;
    int player1Hits = 0;
    int player2Hits = 0;
    
    while (!player1Ready || !player2Ready)
    {
        if (!player1Ready && client1.GetStream().DataAvailable)
        {
            var msg = await NetworkHelper.ReceiveMessageAsync(client1.GetStream());
            if (msg?.Tip == "Ready")
            {
                player1Board = msg.Board;
                player1Ready = true;
                Console.WriteLine("Player 1 is ready.");
            }
        }
        if (!player2Ready && client2.GetStream().DataAvailable)
        {
            var msg = await NetworkHelper.ReceiveMessageAsync(client2.GetStream());
            if (msg?.Tip == "Ready")
            {
                player2Board = msg.Board;
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
    
    await NetworkHelper.SendMessageAsync(client1.GetStream(), new GameMessage { Tip = "SchimbareTura", Status = currentPlayer == 1 ? "MyTurn" : "Wait" });
    await NetworkHelper.SendMessageAsync(client2.GetStream(), new GameMessage { Tip = "SchimbareTura", Status = currentPlayer == 2 ? "MyTurn" : "Wait" });
    
    bool gameOver = false;
    while (!gameOver)
    {
        NetworkStream activeStream = currentPlayer == 1 ? client1.GetStream() : client2.GetStream();
        NetworkStream otherStream = currentPlayer == 1 ? client2.GetStream() : client1.GetStream();
    
        GameMessage? message = await NetworkHelper.ReceiveMessageAsync(activeStream);
        if (message == null) { gameOver = true; break; }
    
        Console.WriteLine($"Player {currentPlayer} attacks ({message.X},{message.Y})");
    
        int[][] opponentBoard = currentPlayer == 1 ? player2Board : player1Board;
        string status = "Miss";
        
        if (opponentBoard != null && opponentBoard[message.X][message.Y] == 1)
        {
            status = "Hit";
            opponentBoard[message.X][message.Y] = 2; // Mark as hit
            if (currentPlayer == 1) player1Hits++; else player2Hits++;
        }
    
        // Send result to attacker
        await NetworkHelper.SendMessageAsync(activeStream, new GameMessage { Tip = "RezultatAtac", X = message.X, Y = message.Y, Status = status });
    
        // Notify other player
        await NetworkHelper.SendMessageAsync(otherStream, new GameMessage { Tip = "NotificareAtacPrimit", X = message.X, Y = message.Y });
    
        // Check win condition (17 hits)
        if (player1Hits == 17 || player2Hits == 17)
        {
            int winner = player1Hits == 17 ? 1 : 2;
            Console.WriteLine($"Player {winner} wins!");
            await NetworkHelper.SendMessageAsync(client1.GetStream(), new GameMessage { Tip = "GameOver", Status = winner == 1 ? "Win" : "Lose" });
            await NetworkHelper.SendMessageAsync(client2.GetStream(), new GameMessage { Tip = "GameOver", Status = winner == 2 ? "Win" : "Lose" });
            gameOver = true;
            break;
        }
    
        // Switch turn
        currentPlayer = currentPlayer == 1 ? 2 : 1;
    
        // Tell both players whose turn it is
        await NetworkHelper.SendMessageAsync(client1.GetStream(), new GameMessage { Tip = "SchimbareTura", Status = currentPlayer == 1 ? "MyTurn" : "Wait" });
        await NetworkHelper.SendMessageAsync(client2.GetStream(), new GameMessage { Tip = "SchimbareTura", Status = currentPlayer == 2 ? "MyTurn" : "Wait" });
    }
}