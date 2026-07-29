using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

public class NetworkPipelineManager : SimulationBehaviour, INetworkRunnerCallbacks
{
    [Header("Configurações da Pipeline")]
    [SerializeField] private NetworkPrefabRef cubePrefab;
    [SerializeField] private Vector3 spawnPosition = new Vector3(0, 0, 1f);

    private async void Start()
    {
        await InitNetworkPipeline();
    }

    private async Task InitNetworkPipeline()
    {
        // Usa o runner do próprio GameObject ou adiciona um novo
        var runner = gameObject.GetComponent<NetworkRunner>();
        if (runner == null)
        {
            runner = gameObject.AddComponent<NetworkRunner>();
        }

        runner.ProvideInput = true;
        runner.AddCallbacks(this);

        var result = await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = "SalaAR_Testes",
            PlayerCount = 4
        });

        if (result.Ok)
        {
            Debug.Log("Pipeline de Rede: Conectado com sucesso à sala!");
        }
        else
        {
            Debug.LogError($"Erro ao conectar à rede: {result.ShutdownReason}");
        }
    }

    // --- EVENTO PRINCIPAL ---
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsSharedModeMasterClient)
        {
            if (GameObject.FindWithTag("NetworkCubeTag") == null)
            {
                NetworkObject spawnedCube = runner.Spawn(cubePrefab, spawnPosition, Quaternion.identity);
                spawnedCube.gameObject.tag = "NetworkCubeTag";
                Debug.Log("Pipeline de Rede: Cubo criado e sincronizado!");
            }
        }
    }

    // --- CALLBACKS OBRIGATÓRIOS VAZIOS ---
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ReadOnlySpan<byte> data) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason cause) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
}