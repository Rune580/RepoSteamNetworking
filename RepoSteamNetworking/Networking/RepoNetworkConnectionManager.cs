using System;
using System.Runtime.InteropServices;
using RepoSteamNetworking.API;
using RepoSteamNetworking.Utils;
using Steamworks;
using Steamworks.Data;

namespace RepoSteamNetworking.Networking;

internal class RepoNetworkConnectionManager : ConnectionManager
{
    public RepoNetworkConnectionManager()
    {
        Logging.Info("Creating new ConnectionManager");
    }
    
    public override void OnConnectionChanged(ConnectionInfo info)
    {
        base.OnConnectionChanged(info);
        
        Logging.Info($"Connection changed {info.Identity}");
    }

    public override void OnConnecting(ConnectionInfo info)
    {
        base.OnConnecting(info);
        
        Logging.Info($"Connecting to {info.Identity}");
    }

    public override void OnConnected(ConnectionInfo info)
    {
        base.OnConnected(info);
        
        Logging.Info($"Connected to {info.Identity}");
    }

    public override void OnDisconnected(ConnectionInfo info)
    {
        base.OnDisconnected(info);
        
        Logging.Info($"Disconnected from {info.Identity}");
    }

    public override void OnMessage(IntPtr data, int size, long messageNum, long recvTime, int channel)
    {
        base.OnMessage(data, size, messageNum, recvTime, channel);
        
        var bytes = new byte[size];
        Marshal.Copy(data, bytes, 0, size);
        
        RepoSteamNetwork.OnClientMessageReceived(bytes);
    }
}