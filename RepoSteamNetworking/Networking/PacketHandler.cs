using System.Linq;
using RepoSteamNetworking.API;
using RepoSteamNetworking.API.VersionCompat;
using RepoSteamNetworking.Networking.Packets;
using RepoSteamNetworking.Networking.Unity;
using RepoSteamNetworking.Utils;

namespace RepoSteamNetworking.Networking;

internal static class PacketHandler
{
    public static void OnHandshakeStatusReceived(HandshakeStatusPacket packet)
    {
        if (!packet.Success)
        {
            Logging.Error("Failed to verify with server! Either something has gone terribly wrong, or steam is down!");
            return;
        }
        
        Logging.Info("Successfully verified connection with server!");
        
        // Send server the version registry of the client to verify mod compatibility.
        Logging.Info("Sending mod compat registry to server to verify mod list compatibility.");
        var registryPacket = VersionCompatRegistry.CreateRegistryPacket();
        RepoSteamNetwork.SendPacket(registryPacket, NetworkDestination.HostOnly);
    }

    public static void OnClientModVersionRegistryReceived(ClientModVersionRegistryPacket packet)
    {
        if (!RepoNetworkingServer.Instance.ServerActive)
        {
            Logging.Warn("Client registry was received on an invalid server?!");
            return;
        }

        var responsePacket = new ServerModVersionRegistryStatusPacket();

        var clientName = packet.Header.Sender.GetLobbyName();

        foreach (var (clientModGuid, clientModVersion) in packet.ModVersions)
        {
            if (VersionCompatRegistry.IsCompatible(clientModGuid, clientModVersion, out var serverModVersion, out var serverCompatibility))
                continue;
            
            if (serverModVersion is null)
            {
                Logging.Warn($"Client {clientName} has mod {clientModGuid} that requires RepoSteamNetworking, but said mod is not present (or failed to register) on Server!");
                continue;
            }
            
            Logging.Warn($"Client {clientName} has mod {clientModGuid} with version {clientModVersion}, They need to have {serverCompatibility.CreateVersionRequirementString(serverModVersion)}!");
            
            responsePacket.AddIncompatible(clientModGuid, serverModVersion, serverCompatibility);
        }

        var missingRequiredMods = VersionCompatRegistry.ModGuids.Where(guid => !packet.ModVersions.ContainsKey(guid))
            .Where(guid => !VersionCompatRegistry.IsOptional(guid))
            .ToArray();

        foreach (var modGuid in missingRequiredMods)
        {
            Logging.Warn($"Client {clientName} is missing required mod {modGuid}!");
            
            if (!VersionCompatRegistry.TryGetCompatInfo(modGuid, out var compatInfo))
                continue;

            if (!VersionCompatRegistry.TryGetVersion(modGuid, out var version))
                continue;

            responsePacket.AddIncompatible(modGuid, version, compatInfo.Compatibility);
        }

        // Todo: in game prompt
        if (responsePacket.HasIncompatibilities())
            Logging.Warn($"Client {clientName} has a mismatch in mod versions or is missing required mods! There may be issues during play, Please ensure that all players have the same mod list and versions!");

        Logging.Info($"Sending mod compat response to client {clientName}.");
        responsePacket.SetTarget(packet.Header.Sender);
        RepoSteamNetwork.SendPacket(responsePacket, NetworkDestination.PacketTarget);

        if (!RepoNetworkingServer.Instance.SocketManager!.TryGetSteamUserConnection(packet.Header.Sender, out var userConnection))
            return;
        
        userConnection.SetValidated();
    }

    public static void OnServerModVersionRegistryReceived(ServerModVersionRegistryStatusPacket packet)
    {
        // Everything is compatible! Yippie!
        if (packet.ModIncompatibilities.Length == 0)
        {
            Logging.Info("Mod lists are compatible between server and client!");
            return;
        }

        // Log incompatibilities.
        foreach (var entry in packet.ModIncompatibilities)
        {
            if (!VersionCompatRegistry.TryGetVersion(entry.Guid, out var version))
            {
                Logging.Warn($"Server requires a mod that isn't installed (or failed to register)! Mod Id: {entry.Guid} Version: {entry.Compatibility.CreateVersionRequirementString(entry.Version)}!");
                continue;
            }
            
            Logging.Warn($"Mod Id: {entry.Guid} Version: {version} requires {entry.Compatibility.CreateVersionRequirementString(entry.Version)}");
        }
        
        // Todo: in game prompt
        Logging.Warn("There's a mismatch in mods and or mod versions! You are responsible for any issues you may encounter!");
    }

    public static void OnCallRPCPacketReceived(CallRPCPacket packet)
    {
        RepoSteamNetwork.InvokeRPC(packet.NetworkId, packet.SubId, packet.MethodName, packet.Parameters);
    }
}