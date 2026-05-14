using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace ConsoleAppServerBattleship
{
    public static class NetworkHelper
    {
        // Serialize and send a message
        public static async Task SendMessageAsync(NetworkStream stream, GameMessage message)
        {
            string json = JsonSerializer.Serialize(message);
            byte[] data = Encoding.UTF8.GetBytes(json + "\n"); // \n marks end of message
            await stream.WriteAsync(data, 0, data.Length);
        }

        // Read and deserialize a message
        public static async Task<GameMessage?> ReceiveMessageAsync(NetworkStream stream)
        {
            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

            if (bytesRead == 0) return null;

            string json = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
            return JsonSerializer.Deserialize<GameMessage>(json);
        }
    }
}
