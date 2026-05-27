namespace WinFormsAppClientBattleship
{
    public partial class MenuForm : Form
    {
        public MenuForm()
        {
            InitializeComponent();
            LoadBackgroundImage();
        }

        private void LoadBackgroundImage()
        {
            BackgroundImage = AssetPaths.LoadImage("intro.jpg");
            BackgroundImageLayout = ImageLayout.Stretch;
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
            Close();
        }
    }
}
