//using System;
//using System.Net.Sockets;
//using System.Text;
//using System.Threading;
//using System.Windows.Forms;

//namespace ChatApp
//{
//    public partial class Form1 : Form
//    {
//        TcpClient? client;
//        NetworkStream? stream;
//        Thread? receiveThread;

//        public Form1()
//        {
//            InitializeComponent();
//        }

//        private void Form1_Load(object sender, EventArgs e)
//        {
//            lblStatus.Text = "Not connected";
//            lblStatus.ForeColor = System.Drawing.Color.Red;
//        }

//        private void btnConnect_Click(object sender, EventArgs e)
//        {
//            try
//            {
//                client = new TcpClient("127.0.0.1", 8000);
//                stream = client.GetStream();

//                lblStatus.Text = "Connected to server";
//                lblStatus.ForeColor = System.Drawing.Color.Green;

//                receiveThread = new Thread(ReceiveMessages);
//                receiveThread.IsBackground = true;
//                receiveThread.Start();
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show("Error connecting: " + ex.Message);
//            }
//        }

//        //private void button1_Click(object sender, EventArgs e)
//        //{
//        //    if (receiveThread != null && receiveThread.IsAlive)
//        //    {
//        //        receiveThread.Abort();
//        //    }
//        //    if (client != null)
//        //    {
//        //        client.Close();
//        //        client = null;
//        //        stream = null;
//        //    }
//        //    lblStatus.Text = "Disconnected";
//        //    lblStatus.ForeColor = System.Drawing.Color.Red;
//        //}

//        private void btnSend_Click(object sender, EventArgs e)
//        {
//            try
//            {
//                if (stream != null && stream.CanWrite)
//                {
//                    string message = textInput.Text;
//                    byte[] buffer = Encoding.UTF8.GetBytes(message);
//                    stream.Write(buffer, 0, buffer.Length);
//                    textInput.Clear();
//                }
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show("Error sending message: " + ex.Message);
//            }
//        }

//        private void ReceiveMessages()
//        {
//            byte[] buffer = new byte[1024];
//            int byteCount;

//            try
//            {
//                while ((byteCount = stream.Read(buffer, 0, buffer.Length)) > 0)
//                {
//                    string message = Encoding.UTF8.GetString(buffer, 0, byteCount);
//                    AppendMessage(message);
//                }
//            }
//            catch
//            {
//                AppendMessage("Disconnected from server.");
//            }
//        }

//        private void AppendMessage(string message)
//        {
//            if (textMessages.InvokeRequired)
//            {
//                textMessages.Invoke(new Action<string>(AppendMessage), message);
//            }
//            else
//            {
//                textMessages.AppendText(message + Environment.NewLine);
//            }
//        }
//    }
//}

using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Windows.Forms;

namespace ChatApp
{
    /// <summary>
    /// Main chat client form implementing TCP socket communication
    /// Demonstrates key networking concepts:
    /// - TCP client-server architecture
    /// - Asynchronous message receiving using background threads
    /// - JSON message serialization/deserialization
    /// - Proper resource management and cleanup
    /// </summary>
    public partial class Form1 : Form
    {
        // TCP client socket - provides connection to server
        TcpClient client;
        
        // Network stream - bidirectional communication channel over TCP
        NetworkStream stream;
        
        // Background thread for receiving messages without blocking UI
        Thread receiveThread;
        
        // Current user's display name
        string username = "";
        
        // Thread-safe flag to coordinate graceful disconnection
        volatile bool disconnecting = false;

        public Form1()
        {
            InitializeComponent();
            // Allow Enter key to send messages for better user experience
            textInput.KeyDown += TextInput_KeyDown;
            // Ensure proper cleanup when form is closed
            this.FormClosing += Form1_FormClosing;
        }

        /// <summary>
        /// Handles Enter key press in text input for quick message sending
        /// UX improvement: users expect Enter to send in chat applications
        /// </summary>
        private void TextInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true; // Prevent Windows beep sound
                btnSend_Click(sender, e);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            lblStatus.Text = "Not connected";
            lblStatus.ForeColor = System.Drawing.Color.Red;
        }

        private string GetUsername()
        {
            string input = "";
            if (InputBox("Enter Username", "Please enter your username:", ref input) == DialogResult.OK)
            {
                return input.Trim();
            }
            return "";
        }

        // Simple InputBox implementation
        public static DialogResult InputBox(string title, string promptText, ref string value)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (btnConnect.Text == "Disconnect")
            {
                DisconnectFromServer();
            }
            else
            {
                ConnectToServer();
            }
        }

        private void ConnectToServer()
        {
            try
            {
                // Get username before connecting
                if (string.IsNullOrWhiteSpace(username))
                {
                    username = GetUsername();
                    if (string.IsNullOrWhiteSpace(username))
                    {
                        MessageBox.Show("Username is required to connect.", "Username Required", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }

                // Create TCP connection using configured server settings
                // TCP (Transmission Control Protocol) provides reliable, ordered data delivery
                client = new TcpClient();
                client.ReceiveTimeout = 0; // No timeout for receiving (chat apps need persistent connections)
                client.SendTimeout = Config.ConnectionTimeoutMs; // Keep send timeout for write operations
                
                // Enable TCP keep-alive to prevent connection drops during idle periods
                client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                
                client.Connect(Config.ServerIP, Config.ServerPort);
                stream = client.GetStream();

                lblStatus.Text = $"Connected as: {username}";
                lblStatus.ForeColor = System.Drawing.Color.Green;
                btnConnect.Text = "Disconnect";

                receiveThread = new Thread(ReceiveMessages);
                receiveThread.IsBackground = true;
                receiveThread.Start();

                // Send join notification
                SendJoinMessage();
            }
            catch (SocketException ex)
            {
                MessageBox.Show($"Cannot connect to server: {ex.Message}\n\nMake sure the server is running on {Config.ServerIP}:{Config.ServerPort}", 
                    "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected connection error: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Gracefully disconnect from server with proper resource cleanup
        /// Demonstrates proper disposal patterns for network resources
        /// </summary>
        private void DisconnectFromServer()
        {
            try
            {
                disconnecting = true;
                
                // Send leave notification before disconnecting
                SendLeaveMessage();
                
                // Dispose network resources properly
                // Order matters: stream first, then client
                stream?.Dispose();
                stream = null;
                
                client?.Dispose();
                client = null;
                
                // Wait for receive thread to finish gracefully
                if (receiveThread != null && receiveThread.IsAlive)
                {
                    receiveThread.Join(1000); // Wait up to 1 second
                }
                receiveThread = null;
                
                // Update UI state
                lblStatus.Text = "Disconnected";
                lblStatus.ForeColor = System.Drawing.Color.Red;
                btnConnect.Text = "Connect";
                
                string timestamp = DateTime.Now.ToString("HH:mm:ss");
                AppendMessage($"[{timestamp}] --- Disconnected from server ---");
                
                disconnecting = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during disconnect: {ex.Message}", 
                    "Disconnect Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Override the form's dispose method to ensure proper cleanup
        /// of network resources when the form is closed
        /// </summary>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Ensure clean disconnection when form is closing
            if (client != null && client.Connected)
            {
                DisconnectFromServer();
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate input first - check for empty and length limits
                if (string.IsNullOrWhiteSpace(textInput.Text))
                {
                    MessageBox.Show("Please enter a message before sending.", 
                        "Empty Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    textInput.Focus();
                    return;
                }

                if (textInput.Text.Length > Config.MaxMessageLength)
                {
                    MessageBox.Show($"Message too long. Maximum {Config.MaxMessageLength} characters allowed.", 
                        "Message Too Long", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    textInput.Focus();
                    return;
                }

                if (stream == null || !stream.CanWrite || string.IsNullOrWhiteSpace(username))
                {
                    MessageBox.Show("Not connected to server. Please connect first.", 
                        "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (stream != null && stream.CanWrite && !string.IsNullOrWhiteSpace(textInput.Text))
                {
                    string messageId = Guid.NewGuid().ToString();
                    var messageObject = new
                    {
                        id = messageId,
                        from = username,
                        text = textInput.Text
                    };

                    // Convert to JSON
                    string json = JsonSerializer.Serialize(messageObject);
                    byte[] buffer = Encoding.UTF8.GetBytes(json);
                    stream.Write(buffer, 0, buffer.Length);
                    stream.Flush(); // Ensure immediate transmission

                    // Show your message immediately with timestamp
                    string timestamp = DateTime.Now.ToString("HH:mm:ss");
                    AppendMessage($"[{timestamp}] You: {textInput.Text}");
                    textInput.Clear();
                }
            }
            catch (ObjectDisposedException)
            {
                MessageBox.Show("Not connected to server. Please connect first.", 
                    "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                lblStatus.Text = "Not connected";
                lblStatus.ForeColor = System.Drawing.Color.Red;
            }
            catch (IOException ex)
            {
                MessageBox.Show($"Network error: {ex.Message}\nConnection may have been lost.", 
                    "Network Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Connection lost";
                lblStatus.ForeColor = System.Drawing.Color.Red;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending message: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Background thread method for receiving messages from server
        /// Key socket programming concepts demonstrated:
        /// - Blocking I/O: stream.Read() blocks until data arrives
        /// - Buffer management: fixed-size buffer for network data
        /// - Thread safety: runs on background thread to avoid UI freezing
        /// - Graceful shutdown: coordinated via 'disconnecting' flag
        /// </summary>
        private void ReceiveMessages()
        {
            // Buffer to hold incoming network data (1KB should handle most chat messages)
            byte[] buffer = new byte[1024];
            int byteCount;

            try
            {
                // Main message receiving loop - runs until disconnection
                // Read() is blocking - thread waits here until data arrives
                while (!disconnecting && (byteCount = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    // Convert received bytes to UTF-8 string
                    // Important: only convert the actual bytes received, not entire buffer
                    string jsonMessage = Encoding.UTF8.GetString(buffer, 0, byteCount);
                    
                    // Debug: Log received message
                    Console.WriteLine($"[CLIENT DEBUG] Received raw message: {jsonMessage}");

                    try
                    {
                        // Deserialize JSON message to strongly-typed object
                        // This demonstrates structured communication protocols
                        var msg = JsonSerializer.Deserialize<ChatMessage>(jsonMessage);
                        Console.WriteLine($"[CLIENT DEBUG] Deserialized message from {msg?.From}: {msg?.Text}");
                        if (msg != null)
                        {
                            string timestamp = DateTime.Now.ToString("HH:mm:ss");
                            string display;
                            
                            // Handle different message types with appropriate formatting
                            if (msg.From == "SYSTEM")
                            {
                                display = $"[{timestamp}] --- {msg.Text} ---";
                            }
                            else
                            {
                                // Display all user messages since server already filters out echo
                                display = $"[{timestamp}] {msg.From}: {msg.Text}";
                            }
                            
                            // Thread-safe UI update from background thread
                            AppendMessage(display);
                        }
                    }
                    catch (JsonException)
                    {
                        // If not JSON, just display raw text
                        AppendMessage($"Server: {jsonMessage}");
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                if (!disconnecting)
                {
                    AppendMessage("Connection closed.");
                }
            }
            catch (IOException)
            {
                if (!disconnecting)
                {
                    AppendMessage("Connection lost. Server may have stopped.");
                    // Update UI to show disconnected status
                    if (lblStatus.InvokeRequired)
                    {
                        lblStatus.Invoke(new Action(() => {
                            lblStatus.Text = "Connection lost";
                            lblStatus.ForeColor = System.Drawing.Color.Red;
                            btnConnect.Text = "Connect";
                        }));
                    }
                    else
                    {
                        lblStatus.Text = "Connection lost";
                        lblStatus.ForeColor = System.Drawing.Color.Red;
                        btnConnect.Text = "Connect";
                    }
                }
            }
            catch (Exception ex)
            {
                AppendMessage($"Error receiving messages: {ex.Message}");
            }
        }

        /// <summary>
        /// Thread-safe method to update UI from any thread
        /// Demonstrates Windows Forms threading model:
        /// - UI controls can only be accessed from the UI thread
        /// - InvokeRequired checks if we're on the wrong thread
        /// - Invoke marshals the call back to the UI thread safely
        /// This is crucial for socket programming where network events
        /// occur on background threads but need to update the UI
        /// </summary>
        private void AppendMessage(string message)
        {
            Console.WriteLine($"[CLIENT DEBUG] AppendMessage called: {message}");
            if (textMessages.InvokeRequired)
            {
                // We're on a background thread - marshal call to UI thread
                textMessages.Invoke(new Action<string>(AppendMessage), message);
            }
            else
            {
                // We're on UI thread - safe to update controls directly
                textMessages.AppendText(message + Environment.NewLine);
                Console.WriteLine($"[CLIENT DEBUG] Message appended to UI");
            }
        }

        private void SendJoinMessage()
        {
            try
            {
                var joinMessage = new
                {
                    id = Guid.NewGuid().ToString(),
                    from = "SYSTEM",
                    text = $"{username} joined the chat"
                };

                string json = JsonSerializer.Serialize(joinMessage);
                byte[] buffer = Encoding.UTF8.GetBytes(json);
                stream.Write(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending join message: {ex.Message}");
            }
        }

        private void SendLeaveMessage()
        {
            try
            {
                if (stream != null && stream.CanWrite)
                {
                    var leaveMessage = new
                    {
                        id = Guid.NewGuid().ToString(),
                        from = "SYSTEM",
                        text = $"{username} left the chat"
                    };

                    string json = JsonSerializer.Serialize(leaveMessage);
                    byte[] buffer = Encoding.UTF8.GetBytes(json);
                    stream.Write(buffer, 0, buffer.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending leave message: {ex.Message}");
            }
        }
    }

    public class ChatMessage
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";
        
        [JsonPropertyName("from")]
        public string From { get; set; } = "";
        
        [JsonPropertyName("text")]
        public string Text { get; set; } = "";
    }
}
