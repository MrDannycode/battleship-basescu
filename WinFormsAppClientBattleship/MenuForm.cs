namespace WinFormsAppClientBattleship
{
    public partial class MenuForm : Form
    {
        public MenuForm()
        {
            InitializeComponent();
            LoadBackgroundImage();
            GameAudio.PlayStartupAudio();
        }

        private void LoadBackgroundImage()
        {
            BackgroundImage = AssetPaths.LoadImage("intro2.jpg");
            BackgroundImageLayout = ImageLayout.Stretch;
        }

        private void btnMute_Click(object sender, EventArgs e)
        {
            GameAudio.SetMuted(!GameAudio.IsMuted);
            btnMute.Text = GameAudio.IsMuted ? "Unmute" : "Mute";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Hide();
            var gameForm = new Form1();
            gameForm.FormClosed += (_, _) => Show();
            gameForm.Show();
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            button2.Enabled = false;

            foreach (Form openForm in Application.OpenForms.Cast<Form>().ToList())
            {
                if (openForm != this)
                    openForm.Close();
            }

            GameAudio.StopAll();

            try
            {
                await GameAudio.PlayExitSoundAsync();
            }
            catch
            {
                // ignore playback errors so the app still exits
            }

            Application.Exit();
        }
    }
}
