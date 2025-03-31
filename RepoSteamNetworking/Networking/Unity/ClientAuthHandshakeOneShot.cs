using System.Collections;
using RepoSteamNetworking.API;
using RepoSteamNetworking.Networking.Packets;
using RepoSteamNetworking.Utils;
using UnityEngine;

namespace RepoSteamNetworking.Networking.Unity;

internal class ClientAuthHandshakeOneShot : MonoBehaviour
{
    private string? _clientKey;
    private IEnumerator? _coroutine;

    private void Start()
    {
        _coroutine = SendAuthHandshake();
        if (_coroutine is null)
            return;
        
        StartCoroutine(_coroutine);
    }

    private IEnumerator? SendAuthHandshake()
    {
        if (string.IsNullOrEmpty(_clientKey))
        {
            Logging.Error("ClientAuthHandshakeOneShot: No client key provided!");
            yield break;
        }

        while (true)
        {
            var lobby = RepoNetworkingClient.Instance.CurrentLobby;
            var authKey = lobby.GetMemberData(lobby.Owner, _clientKey);

            if (!string.IsNullOrWhiteSpace(authKey))
                break;
            
            yield return new WaitForSeconds(0.5f);
        }
        
        var handshakePacket = new HandshakeAuthConnectionPacket();
        handshakePacket.SetData(RepoNetworkingClient.Instance.CurrentLobby, _clientKey);
        RepoSteamNetwork.SendPacket(handshakePacket, NetworkDestination.HostOnly);
        
        Destroy(gameObject, 1f);
    }

    public static void Run(string clientKey)
    {
        var gameObject = Instantiate(new GameObject("ClientAuthHandshakeOneShot"), RepoNetworkingClient.Instance.transform);
        gameObject.SetActive(false);

        var oneShot = gameObject.AddComponent<ClientAuthHandshakeOneShot>();
        oneShot._clientKey = clientKey;
        
        gameObject.SetActive(true);
    }
}