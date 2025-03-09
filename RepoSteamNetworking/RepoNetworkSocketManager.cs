using System;
using System.Runtime.InteropServices;
using System.Text;
using Steamworks;
using Steamworks.Data;

namespace RepoSteamNetworking;

public class RepoNetworkSocketManager : SocketManager
{
    public RepoNetworkSocketManager()
    {
        Logging.Info("Creating new SocketManager");
    }
    
    public override void OnConnecting(Connection connection, ConnectionInfo info)
    {
        base.OnConnecting(connection, info);
        
        Logging.Info($"{info.Identity} is connecting...");

        connection.Accept();
    }

    public override void OnConnected(Connection connection, ConnectionInfo info)
    {
        base.OnConnected(connection, info);
        
        Logging.Info($"{info.Identity} has connected!");
    }

    public override void OnDisconnected(Connection connection, ConnectionInfo info)
    {
        base.OnDisconnected(connection, info);
        
        Logging.Info($"{info.Identity} has disconnected!");
    }

    public override void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel)
    {
        base.OnMessage(connection, identity, data, size, messageNum, recvTime, channel);
        
        var bytes = new byte[size];
        Marshal.Copy(data, bytes, 0, size);
        
        var message = Encoding.UTF8.GetString(bytes);
        
        Logging.Info($"Message received from {identity}!\nMessage: {message}");

        var result = connection.SendMessage(data, size);
        Logging.Info($"Send message (On Message): {result}");
    }
}