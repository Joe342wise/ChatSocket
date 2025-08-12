# ChatSocket - TCP Chat Application

A robust real-time chat application built with C# demonstrating TCP socket programming, multi-threading, and Windows Forms GUI development. This project showcases client-server architecture with JSON message serialization and concurrent user support.

## 🚀 Features

- **Real-time messaging** between multiple clients
- **TCP socket-based** communication for reliable message delivery
- **Multi-threaded server** supporting concurrent connections
- **Windows Forms GUI** with intuitive user interface
- **JSON message protocol** with structured communication
- **Username-based** chat system with join/leave notifications
- **Connection status** indicators and error handling
- **Configurable settings** via App.config
- **Comprehensive logging** system for debugging and monitoring
- **Graceful disconnection** handling and resource cleanup

## 📸 Screenshots & Demo

### Server Application
![Server Running](screenshots/server-running.png)
*TCP Server console showing active connections and message broadcasting*

### Client Application - Login
![Client Login](screenshots/client-login.png)
*Username input dialog when connecting to server*

### Multiple Clients Chatting
![Multiple Clients](screenshots/multiple-clients-chatting.png)
*Three clients (Ben, Joe, Liv) connected and exchanging messages in real-time*

### Active Chat Session
![Active Chat](screenshots/active-chat-session.png)
*Real-time message exchange with timestamps and user identification*

### Server Logging Details
![Server Logs](screenshots/server-logs-detail.png)
*Detailed server logs showing connection management and message broadcasting*

### Connection States
![Connection Status](screenshots/connection-states.png)
*Client showing connected (green) and disconnected (red) status indicators*

**Key Visual Features:**
- ✅ **Color-coded status indicators** (Green = Connected, Red = Disconnected)
- ✅ **Timestamped messages** for chronological tracking
- ✅ **System notifications** for user join/leave events
- ✅ **Clean, intuitive interface** with message history and input field
- ✅ **Real-time server monitoring** with detailed logging output

## 📁 Project Structure

```
ChatSocket/
├── ChatApp/                    # Windows Forms Client Application
│   ├── Program.cs             # Client entry point
│   ├── Form1.cs               # Main chat GUI and networking logic
│   ├── Form1.Designer.cs      # Windows Forms designer code
│   ├── Config.cs              # Configuration management
│   ├── App.config             # Application settings
│   └── ChatApp.csproj         # Client project file
├── TCPSev/                    # Console Server Application
│   ├── Program.cs             # Server entry point and TCP handling
│   ├── Logger.cs              # Logging utility system
│   ├── server.log             # Runtime server logs
│   └── TCPSev.csproj          # Server project file
├── ChatApp.sln                # Visual Studio solution file
└── README.md                  # This documentation
```

## 🏗️ Architecture Overview

### Client-Server Model
- **Protocol**: TCP (Transmission Control Protocol) for reliable, ordered data delivery
- **Port**: 8000 (configurable)
- **Message Format**: JSON serialization with structured message objects
- **Concurrency**: Multi-threaded server with one thread per client connection

### Message Protocol
```json
{
  "id": "unique-guid-identifier",
  "from": "username|SYSTEM",
  "text": "message content"
}
```

### Key Components

#### Server (TCPSev)
- **TcpListener**: Accepts incoming client connections on port 8000
- **Thread-per-client**: Each client handled by dedicated background thread
- **Message Broadcasting**: Distributes messages to all connected clients except sender
- **Logging System**: Structured logging with timestamps and severity levels
- **Connection Management**: Tracks client connections and handles disconnections

#### Client (ChatApp)
- **Windows Forms GUI**: User-friendly interface with message display and input
- **TCP Connection**: Maintains persistent connection to server
- **Background Threading**: Non-blocking message reception using background threads
- **JSON Serialization**: Structured message creation and parsing
- **Thread-safe UI**: Proper marshaling of background thread updates to UI thread

## ⚙️ Technical Implementation

### Network Programming Concepts
- **TCP Socket Programming**: Reliable, connection-oriented communication
- **Multi-threading**: Concurrent handling of multiple client connections
- **Thread Safety**: Synchronized access to shared resources using locks
- **Stream Management**: Proper handling of NetworkStream for data transmission
- **Buffer Management**: Efficient byte array handling for network data

### Windows Forms Threading
- **UI Thread Marshaling**: Safe cross-thread UI updates using Control.Invoke
- **Background Threads**: Non-blocking network operations
- **Resource Disposal**: Proper cleanup of network and threading resources

### JSON Communication
- **System.Text.Json**: Modern JSON serialization/deserialization
- **Message Structure**: Consistent message format with unique IDs
- **Error Handling**: Graceful handling of malformed JSON messages

## 🔧 Detailed Technical Analysis

### Class Architecture & Responsibilities

#### **Server-Side Classes (TCPSev Project)**

##### **1. Program Class (`TCPSev/Program.cs`)**
```csharp
class Program
{
    static TcpListener server;           // Listens for incoming TCP connections
    static List<TcpClient> clients;      // Thread-safe collection of connected clients
    static int port = 8000;              // Default server port
}
```

**Responsibilities:**
- **Main Application Entry**: Bootstraps the TCP server application
- **Connection Management**: Maintains list of active client connections
- **Multi-threading Orchestration**: Spawns worker threads for each client
- **Server Lifecycle Management**: Handles startup, shutdown, and error recovery

**Key Methods:**
- `Main(string[] args)`: Entry point, initializes TcpListener and accepts connections
- `HandleClient(object obj)`: Thread worker method for individual client communication
- `Broadcast(string message, TcpClient sender)`: Distributes messages to all connected clients

**Socket Programming Elements:**
```csharp
// Socket configuration with keep-alive
client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

// Thread-safe client collection management
lock (clients) clients.Add(client);

// Network stream operations with error handling
NetworkStream stream = client.GetStream();
stream.Write(buffer, 0, buffer.Length);
stream.Flush(); // Ensures immediate transmission
```

##### **2. Logger Class (`TCPSev/Logger.cs`)**
```csharp
public static class Logger
{
    private static readonly object lockObj = new object();
    public enum LogLevel { INFO, WARNING, ERROR, DEBUG }
}
```

**OOP Concepts Demonstrated:**
- **Static Class**: Utility class with no instance state
- **Enumeration**: Strongly-typed log level constants
- **Thread Safety**: Lock-based synchronization for concurrent access
- **Method Overloading**: Multiple Log method signatures

**Responsibilities:**
- **Centralized Logging**: Single point for all server logging operations
- **Multi-destination Output**: Console and file logging simultaneously
- **Thread-safe Operations**: Concurrent logging from multiple threads
- **Structured Formatting**: Consistent timestamp and level formatting

#### **Client-Side Classes (ChatApp Project)**

##### **3. Form1 Class (`ChatApp/Form1.cs`)**
```csharp
public partial class Form1 : Form
{
    TcpClient client;                    // TCP connection to server
    NetworkStream stream;                // Bidirectional communication channel
    Thread receiveThread;                // Background message reception
    string username = "";                // User identification
    volatile bool disconnecting = false; // Thread-safe state flag
}
```

**OOP Concepts Demonstrated:**
- **Inheritance**: Inherits from Windows Forms `Form` base class
- **Partial Classes**: Form1.cs + Form1.Designer.cs separation of concerns
- **Encapsulation**: Private fields with controlled access through methods
- **Event-Driven Programming**: Event handlers for UI interactions
- **Composition**: Contains TcpClient and NetworkStream objects

**Key Methods Analysis:**

**Connection Management:**
```csharp
private void ConnectToServer()
{
    client = new TcpClient();
    client.ReceiveTimeout = 0;  // Infinite timeout for chat persistence
    client.SendTimeout = Config.ConnectionTimeoutMs;
    client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
    client.Connect(Config.ServerIP, Config.ServerPort);
    stream = client.GetStream();
}
```

**Thread-Safe UI Updates:**
```csharp
private void AppendMessage(string message)
{
    if (textMessages.InvokeRequired)  // Cross-thread operation check
    {
        textMessages.Invoke(new Action<string>(AppendMessage), message);
    }
    else
    {
        textMessages.AppendText(message + Environment.NewLine);
    }
}
```

**Background Message Reception:**
```csharp
private void ReceiveMessages()  // Runs on background thread
{
    byte[] buffer = new byte[1024];
    while (!disconnecting && (byteCount = stream.Read(buffer, 0, buffer.Length)) > 0)
    {
        string jsonMessage = Encoding.UTF8.GetString(buffer, 0, byteCount);
        var msg = JsonSerializer.Deserialize<ChatMessage>(jsonMessage);
        // Thread-safe UI update via AppendMessage
    }
}
```

##### **4. Config Class (`ChatApp/Config.cs`)**
```csharp
public static class Config
{
    public static string ServerIP => GetSetting("ServerIP", "127.0.0.1");
    public static int ServerPort => GetIntSetting("ServerPort", 8000);
}
```

**Design Patterns:**
- **Static Factory Pattern**: Configuration value creation with fallbacks
- **Facade Pattern**: Simplifies access to ConfigurationManager
- **Default Value Pattern**: Graceful fallback for missing configurations

##### **5. ChatMessage Class (`ChatApp/Form1.cs`)**
```csharp
public class ChatMessage
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";
    
    [JsonPropertyName("from")]
    public string From { get; set; } = "";
    
    [JsonPropertyName("text")]
    public string Text { get; set; } = "";
}
```

**OOP Concepts:**
- **Data Transfer Object (DTO)**: Pure data container with no business logic
- **Attributes**: JSON serialization metadata
- **Auto-implemented Properties**: Modern C# property syntax
- **Default Initialization**: Prevents null reference exceptions

##### **6. Program Class (`ChatApp/Program.cs`)**
```csharp
internal static class Program
{
    [STAThread]  // Single-threaded apartment for COM components
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new Form1());  // Windows Forms message loop
    }
}
```

### Object-Oriented Programming Concepts in Practice

#### **1. Encapsulation**
- **Private Fields**: Network resources (TcpClient, NetworkStream) are private to Form1
- **Property Accessors**: Config class uses properties with private implementation
- **Method Visibility**: Internal networking logic hidden from UI consumers

#### **2. Inheritance**
- **Form1 : Form**: Inherits Windows Forms functionality
- **Partial Classes**: Form1 spans multiple files (logic + designer)
- **Base Class Events**: Overriding FormClosing for cleanup

#### **3. Composition**
- **Form1 contains TcpClient**: "Has-a" relationship for network functionality
- **Logger contains LogLevel enum**: Structured internal data types
- **Complex Object Assembly**: NetworkStream from TcpClient.GetStream()

#### **4. Abstraction**
- **Socket Abstraction**: TcpClient hides low-level socket details
- **Stream Abstraction**: NetworkStream provides uniform read/write interface
- **Threading Abstraction**: Thread class abstracts OS thread management

#### **5. Polymorphism**
- **Event Handler Polymorphism**: Multiple methods can handle same event type
- **Interface Implementation**: TcpClient implements IDisposable
- **Method Overloading**: Logger.Log() with different parameter combinations

### Network Programming Deep Dive

#### **Socket Layer Architecture**
```
Application Layer (ChatApp/TCPSev)
    ↓
Transport Layer (TcpClient/TcpListener)
    ↓
Network Layer (IPAddress, Socket Options)
    ↓
Operating System (Winsock API)
```

#### **Threading Model**
```
Server Main Thread:
├── TcpListener.AcceptTcpClient() [Blocking]
├── For each connection:
│   ├── Thread.Start(HandleClient) [New Thread]
│   └── Continue accepting new connections

Client UI Thread:
├── Windows Forms Message Loop
├── Event Handlers (Connect, Send, etc.)
└── Background Thread: ReceiveMessages()
```

#### **Data Flow Analysis**
```
Client A → JSON Message → TCP Stream → Server → Broadcast → Client B,C,D
    ↑                                                           ↓
Form1.btnSend_Click()                              Form1.ReceiveMessages()
    ↑                                                           ↓
JsonSerializer.Serialize()                     JsonSerializer.Deserialize()
    ↑                                                           ↓
stream.Write() + Flush()                           stream.Read()
```

### Exception Handling Strategy

#### **Network Exception Hierarchy**
- **SocketException**: Low-level socket errors (port conflicts, network unavailable)
- **IOException**: Stream-level errors (connection lost, write failures)
- **ObjectDisposedException**: Resource cleanup race conditions
- **JsonException**: Message parsing errors

#### **Recovery Patterns**
```csharp
try
{
    // Network operation
}
catch (ObjectDisposedException)
{
    // Resource already disposed - cleanup state
}
catch (IOException ex)
{
    // Network error - attempt graceful degradation
    Logger.Warning($"Network error: {ex.Message}");
}
catch (Exception ex)
{
    // Unexpected error - log and continue
    Logger.Error($"Unexpected error: {ex.Message}");
}
finally
{
    // Resource cleanup - always executes
    client?.Dispose();
}
```

### Memory Management & Resource Disposal

#### **IDisposable Implementation Pattern**
```csharp
// Explicit disposal in Form1.DisconnectFromServer()
stream?.Dispose();
client?.Dispose();

// Using statements ensure cleanup (implicit in some areas)
using (var client = new TcpClient()) { /* automatic disposal */ }
```

#### **Thread Lifecycle Management**
```csharp
// Background thread cleanup
receiveThread = new Thread(ReceiveMessages);
receiveThread.IsBackground = true;  // Dies with main thread
receiveThread.Start();

// Graceful thread termination
disconnecting = true;  // Signal to stop
receiveThread.Join(1000);  // Wait up to 1 second
```

This comprehensive technical analysis demonstrates sophisticated use of C# OOP principles, network programming concepts, and multi-threading patterns in a real-world application context.

## 🛠️ Setup and Installation

### Prerequisites
- **.NET 8.0 or later**
- **Visual Studio 2022** or **Visual Studio Code**
- **Windows OS** (for Windows Forms client)

### Building the Project

1. **Clone or download** the project to your local machine
2. **Open the solution** in Visual Studio:
   ```
   ChatSocket/ChatApp/ChatApp.sln
   ```
3. **Build the solution** (Ctrl+Shift+B) to compile both projects

### Configuration

The client application uses `App.config` for customizable settings:

```xml
<appSettings>
    <add key="ServerIP" value="127.0.0.1" />
    <add key="ServerPort" value="8000" />
    <add key="ConnectionTimeoutMs" value="30000" />
    <add key="MaxMessageLength" value="1000" />
    <add key="KeepAliveIntervalMs" value="30000" />
</appSettings>
```

**Configuration Options:**
- `ServerIP`: Server IP address (default: localhost)
- `ServerPort`: TCP port number (default: 8000)
- `ConnectionTimeoutMs`: Send timeout in milliseconds
- `MaxMessageLength`: Maximum characters per message
- `KeepAliveIntervalMs`: Connection keep-alive interval

## 🚀 Usage Instructions

### Starting the Server

1. **Navigate** to the server executable:
   ```
   ChatSocket/TCPSev/bin/Debug/net8.0/TCPSev.exe
   ```
2. **Run** the server application
3. **Verify** server startup in console output:
   ```
   [INFO] TCP server started successfully on port 8000
   ```

### Connecting Clients

1. **Navigate** to the client executable:
   ```
   ChatSocket/ChatApp/bin/Debug/net8.0-windows/ChatApp.exe
   ```
2. **Run** multiple instances for different users
3. **Click "Connect"** button
4. **Enter username** when prompted
5. **Start chatting** once connected

### Chat Features

- **Send Messages**: Type in input field and press Enter or click Send
- **View Messages**: All messages appear in the main text area with timestamps
- **Connection Status**: Color-coded status indicator (Green=Connected, Red=Disconnected)
- **Join/Leave Notifications**: System messages when users connect/disconnect
- **Graceful Disconnect**: Click "Disconnect" button or close window

## 📊 Monitoring and Logging

### Server Logs
The server generates detailed logs in `TCPSev/server.log`:

```
[2025-08-12 01:05:20] [INFO] New client connected. Total clients: 1
[2025-08-12 01:05:27] [DEBUG] Broadcasting to 2 total clients
[2025-08-12 01:05:37] [INFO] Broadcasted message to 1 clients
```

**Log Levels:**
- `INFO`: General information (connections, disconnections)
- `DEBUG`: Detailed debugging information (message content, broadcast details)
- `WARNING`: Non-critical issues (network errors, failed sends)
- `ERROR`: Critical errors (socket exceptions, unexpected failures)

### Real-time Monitoring
- **Connection Count**: Server tracks and logs total connected clients
- **Message Broadcasting**: Detailed logs show message distribution
- **Network Errors**: Comprehensive error reporting and handling
- **Performance Metrics**: Connection timing and message throughput

## 🔧 Troubleshooting

### Common Issues and Solutions

#### Connection Problems

**Issue**: "Cannot connect to server"
- **Solution**: Ensure server is running before starting clients
- **Check**: Firewall settings aren't blocking port 8000
- **Verify**: Server IP and port configuration in App.config

**Issue**: "Connection lost after few seconds"
- **Solution**: Already fixed - timeout settings optimized for chat applications
- **Technical**: ReceiveTimeout set to 0 (infinite) for persistent connections

#### Message Not Appearing

**Issue**: Messages not showing on other clients
- **Solution**: Already resolved - JSON serialization and broadcasting fixed
- **Technical**: Proper message filtering and cross-client communication implemented

### Network Configuration

**Port Usage**: Default port 8000
- Change in `App.config` if port conflict occurs
- Ensure port is available and not blocked by firewall

**IP Configuration**: Default localhost (127.0.0.1)
- For network usage, change to actual server IP address
- Update all clients' App.config files accordingly

## 🛡️ Security Considerations

### Current Implementation
- **Local Network**: Designed for trusted local network environments
- **No Authentication**: Simple username-based identification
- **Plain Text**: Messages transmitted without encryption

### Production Recommendations
- **SSL/TLS**: Implement encrypted communication
- **Authentication**: Add user authentication system
- **Input Validation**: Enhanced message sanitization
- **Rate Limiting**: Prevent message flooding
- **Access Control**: IP-based or user-based access restrictions

## 📈 Performance Characteristics

### Scalability
- **Concurrent Connections**: Tested with multiple simultaneous clients
- **Memory Usage**: Efficient resource management with proper disposal
- **Thread Management**: One thread per client with proper lifecycle management

### Tested Scenarios
- ✅ **Multiple Clients**: 3+ concurrent users successfully tested
- ✅ **Message Broadcasting**: Real-time message distribution working
- ✅ **Connection Stability**: Persistent connections without timeouts
- ✅ **Graceful Disconnection**: Clean resource cleanup on client exit

## 🔄 Development Insights

### Learning Objectives Achieved
- **TCP Socket Programming**: Hands-on experience with low-level networking
- **Multi-threading**: Practical application of concurrent programming
- **Windows Forms**: GUI development with event-driven programming
- **JSON Serialization**: Modern data exchange formats
- **Error Handling**: Robust exception management and recovery
- **Resource Management**: Proper disposal of network and system resources

### Code Quality
- **Comprehensive Comments**: Detailed inline documentation
- **Error Handling**: Try-catch blocks for network operations
- **Resource Cleanup**: Using statements and proper disposal patterns
- **Thread Safety**: Locks and synchronized access to shared data
- **Configuration Management**: Externalized settings for flexibility

## 🧪 Testing

### Manual Testing Scenarios
1. **Single Client Connection**: Basic functionality verification
2. **Multiple Client Chat**: Cross-client message exchange
3. **Connection Stability**: Long-duration connections
4. **Disconnect/Reconnect**: Connection lifecycle testing
5. **Error Recovery**: Network interruption handling

### Test Results
- **Message Delivery**: 100% success rate in testing
- **Connection Stability**: No timeouts during extended sessions
- **Multi-client Support**: Successfully tested with 3+ concurrent users
- **Resource Cleanup**: No memory leaks observed during testing

## 📝 Future Enhancements

### Potential Improvements
- **File Transfer**: Support for sending files between clients
- **Private Messaging**: Direct client-to-client communication
- **Chat Rooms**: Multiple chat channels
- **Message History**: Persistent message storage
- **User Authentication**: Secure login system
- **Web Interface**: Browser-based client option
- **Mobile Support**: Cross-platform client applications

### Technical Upgrades
- **Database Integration**: Message persistence and user management
- **REST API**: Web service endpoints for extended functionality
- **WebSocket Support**: Browser compatibility
- **Docker Containerization**: Easy deployment and scaling
- **Configuration UI**: Graphical settings management

## 📄 License

This project is created for educational purposes demonstrating TCP socket programming concepts in C#.

---

**Author**: Educational Project  
**Technology Stack**: C#, .NET 8.0, Windows Forms, TCP Sockets, JSON  
**Last Updated**: August 2025  

For questions or support, refer to the inline code documentation and server logs for debugging information.