using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

public class NetworkPipelineManager : SimulationBehaviour, INetworkRunnerCallbacks
{
    [Header("Posição do Cubo Central")]
    [SerializeField] private NetworkPrefabRef cubePrefab;
    [SerializeField] private Vector3 spawnPosition = new Vector3(0f, 1.5f, 4f); // Cubo à altura dos olhos e à frente

    [Header("Posições dos Jogadores")]
    [SerializeField] private Vector3 player1Position = new Vector3(-1.5f, 1.5f, 0f); // Ligeiramente à esquerda
    [SerializeField] private Vector3 player2Position = new Vector3(1.5f, 1.5f, 0f);  // Ligeiramente à direita

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
            Debug.Log("Pipeline de Rede: Conectado com sucesso à sala!");
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

        // 1. Posições fixas para os jogadores
        Vector3 targetPos = runner.IsSharedModeMasterClient ? player1Position : player2Position;
        Camera.main.transform.position = targetPos;

        // CORREÇÃO: Apontar exatamente para a variável spawnPosition (e não para Vector3.zero)
        Camera.main.transform.LookAt(spawnPosition);

        // 3. Resetar o script da câmara para não haver erros
        var camController = Camera.main.GetComponent<ARTestCameraController>();
        if (camController != null) camController.ResetAngles();
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        // O Master Client cria o cubo exatamente na spawnPosition definida no topo do script
        if (runner.IsSharedModeMasterClient)
        {
            if (GameObject.FindWithTag("NetworkCubeTag") == null)
            {
                // CORREÇÃO: Usar a variável spawnPosition em vez de criar um novo Vector3 fixo
                NetworkObject spawnedCube = runner.Spawn(cubePrefab, spawnPosition, Quaternion.identity);
                spawnedCube.gameObject.tag = "NetworkCubeTag";
                Debug.Log("Pipeline de Rede: Cubo criado perfeitamente em frente!");
            }
        }
    }

    // --- CALLBACKS VAZIOS ---
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