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

    [Header("Rotações Iniciais (Euler Angles) para olhar para o Spawner")]
    [SerializeField] private Vector3 player1Rotation = new Vector3(0f, 0f, 0f);     // Ajusta o Y se precisar de rodar (ex: 0, 90, 180, 270)
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

        // 1. Define a posição baseada no jogador
        Camera.main.transform.position = isMaster ? player1Position : player2Position;

        // 2. Define a rotação inicial exata através de ângulos limpos no Inspector
        Vector3 targetRot = isMaster ? player1Rotation : player2Rotation;
        Camera.main.transform.rotation = Quaternion.Euler(targetRot);

        // Se quiseres usar o LookAt mas notas que fica ao contrário, podes inverter a rotação ou usar os ângulos de Euler acima.
        // Dica: Se o jogador estiver de costas, basta ires ao campo 'Player 1 Rotation' no Inspector e mudares o eixo Y para 180.

        Debug.Log($"Câmara posicionada para: {(isMaster ? "Jogador 1 (Esquerda)" : "Jogador 2 (Direita)")}");
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