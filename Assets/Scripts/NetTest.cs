using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

public class NetTest : SimulationBehaviour, INetworkRunnerCallbacks
{
    [Header("Posição do Jogador 1 (Base)")]
    [SerializeField] private Vector3 player1Position = new Vector3(-1.5f, 1.5f, 0f);

    [Header("Desvio Relativo para o Jogador 2")]
    [Tooltip("Distância somada à posição do Jogador 1 para colocar o Jogador 2 ao lado.")]
    [SerializeField] private Vector3 player2Offset = new Vector3(2.0f, 0f, 0f);

    [Header("Rotações Iniciais (Euler Angles) para olhar para o Spawner")]
    [SerializeField] private Vector3 player1Rotation = new Vector3(0f, 0f, 0f);
    [SerializeField] private Vector3 player2Rotation = new Vector3(0f, 0f, 0f);

    [Header("Centro do Jogo (Referência)")]
    [SerializeField] public Vector3 lookAtCenter = new Vector3(0f, 1.5f, 4f);

    private async void Start()
    {
        await InitNetworkPipeline();
    }

    private async Task InitNetworkPipeline()
    {
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
            PlayerCount = 2
        });

        if (result.Ok)
        {
            Debug.Log($"[SUCESSO] Conectado à sala! Sou o MasterClient? {runner.IsSharedModeMasterClient}");
            SetupLocalCameraTransform(runner);
        }
        else
        {
            Debug.LogError($"[ERRO] Falha ao conectar: {result.ShutdownReason}");
        }
    }

    private void SetupLocalCameraTransform(NetworkRunner runner)
    {
        if (Camera.main == null) return;

        bool isMaster = runner.IsSharedModeMasterClient;

        // 1. Define a posição: O Master (Player 1) usa a posição base. O Player 2 usa a posição do P1 + o desvio (offset).
        if (isMaster)
        {
            Camera.main.transform.position = player1Position;
            Camera.main.transform.rotation = Quaternion.Euler(player1Rotation);
        }
        else
        {
            Camera.main.transform.position = player1Position + player2Offset;
            Camera.main.transform.rotation = Quaternion.Euler(player2Rotation);
        }

        Debug.Log($"Câmara posicionada para: {(isMaster ? "Jogador 1" : "Jogador 2 (Ao lado do Jogador 1)")}");
    }

    // --- CALLBACKS OBRIGATÓRIOS DO FUSION ---
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ReadOnlySpan<byte> data) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
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