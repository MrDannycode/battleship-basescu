using System.Net.Sockets;
using System.Text;

namespace WinFormsAppClientBattleship
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            InitializeBoards();
        }

        private Button[,] ownBoard = new Button[10, 10];
        private Button[,] attackBoard = new Button[10, 10];

        private void InitializeBoards()
        {
            for (int row = 0; row < 10; row++)
            {
                for (int col = 0; col < 10; col++)
                {
                    // --- OWN BOARD buttons ---
                    Button ownBtn = new Button();
                    ownBtn.Dock = DockStyle.Fill;
                    ownBtn.BackColor = Color.LightBlue;
                    ownBtn.Margin = new Padding(1);
                    ownBtn.Tag = new Point(row, col); // store position
                    ownBtn.Click += OwnBoard_Click;
                    ownBoard[row, col] = ownBtn;
                    tableLayoutPanel1.Controls.Add(ownBtn, col, row);

                    // --- ATTACK BOARD buttons ---
                    Button attackBtn = new Button();
                    attackBtn.Dock = DockStyle.Fill;
                    attackBtn.BackColor = Color.LightGray;
                    attackBtn.Margin = new Padding(1);
                    attackBtn.Tag = new Point(row, col); // store position
                    attackBtn.Click += AttackBoard_Click;
                    attackBoard[row, col] = attackBtn;
                    tableLayoutPanel2.Controls.Add(attackBtn, col, row);
                }
            }
        }

        // Fires when player clicks their own board (ship placement)
        private void OwnBoard_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            Point pos = (Point)btn.Tag;
            Console.WriteLine($"Own board clicked: Row {pos.X}, Col {pos.Y}");
            // Ship placement logic will go here
        }

        // Fires when player clicks the attack board (shooting)
        private async void AttackBoard_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            Point pos = (Point)btn.Tag;

            GameMessage attackMessage = new GameMessage
            {
                Tip = "Atac",
                X = pos.X,
                Y = pos.Y
            };

            await NetworkHelper.SendMessageAsync(stream, attackMessage);
            btn.Enabled = false; // disable button after clicking
            labelStatus.Text = "Waiting for result...";
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private TcpClient client;
        private NetworkStream stream;
        private bool isConnected = false;

        private async Task ConnectToServer()
        {
            try
            {
                client = new TcpClient();
                await client.ConnectAsync("127.0.0.1", 5000);
                stream = client.GetStream();
                isConnected = true;

                labelStatus.Text = "Connected! Waiting for opponent...";
                Console.WriteLine("Connected to server!");

                // Start listening for messages from server
                _ = Task.Run(() => ListenForMessages());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not connect to server: {ex.Message}");
            }
        }
        private async Task ListenForMessages()
        {
            byte[] buffer = new byte[1024];

            while (isConnected)
            {
                try
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break; // server disconnected

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Received: {message}");

                    // Update UI safely from background thread
                    this.Invoke(() => HandleServerMessage(message));
                }
                catch
                {
                    isConnected = false;
                    this.Invoke(() => labelStatus.Text = "Disconnected from server.");
                    break;
                }
            }
        }
        private void HandleServerMessage(string message)
        {
            // We will parse JSON here later
            Console.WriteLine($"Handling message: {message}");
        }
        private async Task SendMessage(string message)
        {
            if (!isConnected) return;

            byte[] data = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(data, 0, data.Length);
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            await ConnectToServer();
        }

    }
}
