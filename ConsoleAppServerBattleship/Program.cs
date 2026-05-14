using System.Net;
using System.Net.Sockets;

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