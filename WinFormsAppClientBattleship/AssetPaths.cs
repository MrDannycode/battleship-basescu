namespace WinFormsAppClientBattleship
{
    internal static class AssetPaths
    {
        public static string ImagesDirectory =>
            Path.Combine(AppContext.BaseDirectory, "Assets", "Images");

        public static string AudioDirectory =>
            Path.Combine(AppContext.BaseDirectory, "Assets", "Audio");

        public static string ImagePath(string fileName) =>
            Path.Combine(ImagesDirectory, fileName);

        public static string AudioPath(string fileName) =>
            Path.Combine(AudioDirectory, fileName);

        public static Image? LoadImage(string fileName)
        {
            var path = ImagePath(fileName);
            if (!File.Exists(path))
                return null;

            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            return Image.FromStream(stream);
        }
    }
}
