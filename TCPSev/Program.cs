//using System;
//using System.Collections.Generic;
//using System.Net;
//using System.Net.Sockets;
//using System.Text;
//using System.Threading;

//class Program
//{
//    static TcpListener server;
//    static List<TcpClient> clients = new List<TcpClient>();
//    static int port = 8000;

//    static void Main(string[] args)
//    {
//        try
//        {
//            server = new TcpListener(IPAddress.Any, port);
//            server.Start();
//            Console.WriteLine($"Server started on port {port}...");

//            while (true)
//            {
//                TcpClient client = server.AcceptTcpClient();
//                lock (clients) clients.Add(client);
//                Console.WriteLine("New client connected.");

//                Thread t = new Thread(HandleClient);
//                t.Start(client);
//            }
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine("Server error: " + ex.Message);
//        }
//    }

//    static void HandleClient(object obj)
//    {
//        TcpClient client = (TcpClient)obj;
//        NetworkStream stream = client.GetStream();
//        byte[] buffer = new byte[1024];
//        int byteCount;

//        try
//        {
//            while ((byteCount = stream.Read(buffer, 0, buffer.Length)) > 0)
//            {
//                string message = Encoding.UTF8.GetString(buffer, 0, byteCount);
//                Console.WriteLine("Received: " + message);
//                Broadcast(message, client);
//            }
//        }
//        catch { }
//        finally
//        {
//            lock (clients) clients.Remove(client);
//            client.Close();
//            Console.WriteLine("Client disconnected.");
//        }
//    }

//    static void Broadcast(string message, TcpClient sender)
//    {
//        byte[] buffer = Encoding.UTF8.GetBytes(message);
//        lock (clients)
//        {
//            foreach (var client in clients)
//            {
//                if (client != sender)
//                {
//                    try
//                    {
//                        client.GetStream().Write(buffer, 0, buffer.Length);
//                    }
//                    catch { }
//                }
//            }
//        }
//    }
//}

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace TCPSev
{
    class Program
{
    static TcpListener server;
    static List<TcpClient> clients = new List<TcpClient>();
    static int port = 8000;

    static void Main(string[] args)
    {
        try
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();
            Logger.Info($"TCP server started successfully on port {port}");

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                
                // Enable keep-alive for the client connection
                client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                
                lock (clients) clients.Add(client);
                Logger.Info($"New client connected. Total clients: {clients.Count}");

                Thread t = new Thread(HandleClient);
                t.Start(client);
            }
        }
        catch (SocketException ex)
        {
            Logger.Error($"Socket error: {ex.Message} (Error Code: {ex.ErrorCode})");
            Logger.Warning("Make sure the port 8000 is not already in use.");
        }
        catch (Exception ex)
        {
            Logger.Error($"Unexpected server error: {ex.Message}");
            Logger.Debug($"Stack trace: {ex.StackTrace}");
        }
        finally
        {
            server?.Stop();
            Logger.Info("Server stopped. Press any key to exit.");
            Console.ReadKey();
        }
    }

    static void HandleClient(object obj)
    {
        TcpClient client = (TcpClient)obj;
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];
        int byteCount;

        try
        {
            while ((byteCount = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                string jsonMessage = Encoding.UTF8.GetString(buffer, 0, byteCount);
                Logger.Debug($"Received message: {jsonMessage}");
                Broadcast(jsonMessage, client);
            }
        }
        catch (ObjectDisposedException)
        {
            Logger.Info("Client connection was closed gracefully.");
        }
        catch (IOException ex)
        {
            Logger.Warning($"Network error with client: {ex.Message}");
        }
        catch (Exception ex)
        {
            Logger.Error($"Error handling client: {ex.Message}");
        }
        finally
        {
            lock (clients) 
            { 
                clients.Remove(client);
                Logger.Info($"Client disconnected. Remaining clients: {clients.Count}");
            }
            client.Close();
        }
    }

    static void Broadcast(string message, TcpClient sender)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(message);
        int broadcastCount = 0;
        
        lock (clients)
        {
            Logger.Debug($"Broadcasting to {clients.Count} total clients");
            
            foreach (var client in clients)
            {
                if (client != sender) // don't send back to sender
                {
                    try
                    {
                        NetworkStream stream = client.GetStream();
                        stream.Write(buffer, 0, buffer.Length);
                        stream.Flush(); // Ensure data is sent immediately
                        broadcastCount++;
                        Logger.Debug($"Message sent to client {broadcastCount}");
                    }
                    catch (ObjectDisposedException)
                    {
                        Logger.Warning("Attempted to send to disconnected client.");
                    }
                    catch (IOException ex)
                    {
                        Logger.Warning($"Failed to send message to client: {ex.Message}");
                    }
                }
            }
        }
        
        Logger.Info($"Broadcasted message to {broadcastCount} clients");
    }
}
}
