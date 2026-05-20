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

        public static async Task<GameMessage?> ReceiveMessageAsync(NetworkStream stream)
        {
            try
            {
                byte[] buffer = new byte[1024];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                if (bytesRead == 0) return null;

                string rawString = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                
                // Rapid clicks might concatenate messages with \n. Split and take the first valid one.
                string[] parts = rawString.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0)
                {
                    return JsonSerializer.Deserialize<GameMessage>(parts[0].Trim());
                }
                return null;
            }
            catch
            {
                // Prevent server crash on malformed JSON
                return null;
            }
        }
    }
}
