namespace WinFormsAppClientBattleship
{
    internal static class WavHelper
    {
        public static int? GetDurationMilliseconds(string path)
        {
            try
            {
                using var stream = File.OpenRead(path);
                if (stream.Length < 44)
                    return null;

                using var reader = new BinaryReader(stream);
                stream.Position = 28;
                int byteRate = reader.ReadInt32();
                if (byteRate <= 0)
                    return null;

                stream.Position = 40;
                int dataSize = reader.ReadInt32();
                if (dataSize <= 0)
                    return null;

                return (int)(dataSize * 1000L / byteRate);
            }
            catch
            {
                return null;
            }
        }
    }
}
