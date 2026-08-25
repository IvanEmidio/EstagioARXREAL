using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

public class NetTest : SimulationBehaviour, INetworkRunnerCallbacks
{
    [Header("Posições Fixas dos Jogadores")]
    [SerializeField] private Vector3 player1Position = new Vector3(-1.5f, 1.5f, 0f); // Esquerda
    [SerializeField] private Vector3 player2Position = new Vector3(1.5f, 1.5f, 0f);  // Direita
    [SerializeField] private Vector3 lookAtCenter = new Vector3(0f, 1.5f, 4f);       // Ponto para onde olham

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
            Debug.Log("Rede: Conectado com sucesso à sala!");
            SetupLocalCameraPosition(runner);
        }
        else
        {
            Debug.LogError($"Erro ao conectar à rede: {result.ShutdownReason}");
        }
    }

    private void SetupLocalCameraPosition(NetworkRunner runner)
    {
        if (Camera.main == null) return;

        // Define a posição baseada em ser o 1º (Master) ou 2º jogador
        Vector3 targetPos = runner.IsSharedModeMasterClient ? player1Position : player2Position;
        Camera.main.transform.position = targetPos;

        // Faz a câmara olhar para o centro de jogo onde os meteoros vão aparecer
        Camera.main.transform.LookAt(lookAtCenter);

        // Reseta os ângulos da câmara
        var camController = Camera.main.GetComponent<ARTestCameraController>();
        if (camController != null)
        {
            camController.ResetAngles();
        }

        Debug.Log($"Câmara posicionada para: {(runner.IsSharedModeMasterClient ? "Jogador 1 (Esquerda)" : "Jogador 2 (Direita)")}");
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