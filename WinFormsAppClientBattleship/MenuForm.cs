using System.Media;

namespace WinFormsAppClientBattleship
{
    public partial class MenuForm : Form
    {
        private SoundPlayer? _startupAudio;

        public MenuForm()
        {
            InitializeComponent();
            LoadBackgroundImage();
            PlayStartupAudio();
        }

        private void LoadBackgroundImage()
        {
            BackgroundImage = AssetPaths.LoadImage("intro.jpg");
            BackgroundImageLayout = ImageLayout.Stretch;
        }

        private void PlayStartupAudio()
        {
            var path = AssetPaths.AudioPath("abs1.wav");
            if (!File.Exists(path))
                return;

            _startupAudio = new SoundPlayer(path);
            _startupAudio.Play();
        }

        private void StopStartupAudio()
        {
            _startupAudio?.Stop();
            _startupAudio?.Dispose();
            _startupAudio = null;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Hide();
            var gameForm = new Form1();
            gameForm.FormClosed += (_, _) => Show();
            gameForm.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            StopStartupAudio();

            var path = AssetPaths.AudioPath("iesire.wav");
            if (File.Exists(path))
            {
                using var player = new SoundPlayer(path);
                player.PlaySync();
            }

            Close();
        }
    }
}
