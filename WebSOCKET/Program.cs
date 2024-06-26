﻿ using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class TcpSocketServer : IDisposable
{
    private TcpListener _listener;
    private ConcurrentDictionary<Guid, TcpClient> _clients;

    public TcpSocketServer(string ipAddress, int port)
    {
        _listener = new TcpListener(IPAddress.Parse(ipAddress), port);
        _clients = new ConcurrentDictionary<Guid, TcpClient>();
    }

    public async Task Start()
    {
        _listener.Start();
        Console.WriteLine("TCP socket server listening...");
            
        try
        {
            while (true)
            {
                TcpClient client = await _listener.AcceptTcpClientAsync();
                Guid clientId = Guid.NewGuid();
                _clients.TryAdd(clientId, client);

                Console.WriteLine($"Client {clientId} connected");

                _ = Task.Run(() => HandleClient(clientId, client));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"TCP socket server error: {ex.Message}");
        }
    }

    private async Task HandleClient(Guid clientId, TcpClient client)
    {
        try
        {
            NetworkStream stream = client.GetStream();

            byte[] buffer = new byte[1024];
            int bytesRead;

            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                byte[] messageBytes = new byte[bytesRead];
                Array.Copy(buffer, messageBytes, bytesRead);

                await BroadcastMessage(clientId, messageBytes);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling client {clientId}: {ex.Message}");
        }
        finally
        {
            await RemoveClient(clientId);
        }
    }

    private async Task BroadcastMessage(Guid senderId, byte[] message)
    {
        foreach (var clientPair in _clients)
        {
            TcpClient receiver = clientPair.Value;
            if (receiver.Connected)
            {
                try
                {
                    await receiver.GetStream().WriteAsync(message, 0, message.Length);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending message to client {clientPair.Key}: {ex.Message}");
                }
            }
        }
    }


    private async Task RemoveClient(Guid clientId)
    {
        if (_clients.TryRemove(clientId, out TcpClient removedClient))
        {
            removedClient.Close();
            Console.WriteLine($"Client {clientId} disconnected");
        }
    }

    public void Dispose()
    {
        _listener.Stop();

        foreach (var clientPair in _clients)
        {
            clientPair.Value.Close();
        }
        _clients.Clear();
    }
}


class Program
{
    static async Task Main(string[] args)
    {
        string ipAddress = "0.0.0.0";
        int port = 49153;

        using (var server = new TcpSocketServer(ipAddress, port))
        {
            await server.Start();

            Console.WriteLine("Press ENTER to stop the server.");
            Console.ReadLine();
        }
    }
}
