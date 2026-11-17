
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetTest : SimulationBehaviour, INetworkRunnerCallbacks
{
    [Header("Posição do Jogador 1 (Base)")]
    [SerializeField]
    private Vector3 player1Position =
        new Vector3(-1.5f, 1.5f, 0f);

    [Header("Desvio Relativo para o Jogador 2")]
    [SerializeField]
    private Vector3 player2Offset =
        new Vector3(2.0f, 0f, 0f);

    [Header("Rotações Iniciais")]
    [SerializeField]
    private Vector3 player1Rotation =
        new Vector3(0f, 0f, 0f);

    [SerializeField]
    private Vector3 player2Rotation =
        new Vector3(0f, 0f, 0f);

    [Header("Centro do Jogo")]
    public Vector3 lookAtCenter = new Vector3(0f, 1.5f, 4f);

    private NetworkRunner runner;

    private async void Start()
    {
        await InitNetworkPipeline();
    }

    private async Task InitNetworkPipeline()
    {
        runner = GetComponent<NetworkRunner>();

        if (runner == null)
        {
            runner = gameObject.AddComponent<NetworkRunner>();
        }

        runner.ProvideInput = true;
        runner.AddCallbacks(this);

        // Gestor de cenas do Photon Fusion.
        NetworkSceneManagerDefault sceneManager =
            GetComponent<NetworkSceneManagerDefault>();

        if (sceneManager == null)
        {
            sceneManager =
                gameObject.AddComponent<NetworkSceneManagerDefault>();
        }

        // Obter a cena atualmente aberta.
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;

        if (sceneIndex < 0)
        {
            Debug.LogError("[NetTest] Cena não encontrada no Build Settings!");
            return;
        }

        Debug.Log("[NetTest] A iniciar Fusion...");

        var result = await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = "SalaAR_Testes",
            PlayerCount = 2,

            // Carregamento e sincronização da cena.
            SceneManager = sceneManager,
            Scene = SceneRef.FromIndex(sceneIndex)
        });

        if (result.Ok)
        {
            Debug.Log(
                $"[SUCESSO] Conectado à sala! " +
                $"Sou Master Client? {runner.IsSharedModeMasterClient}"
            );

            SetupLocalCameraTransform(runner);
        }
        else
        {
            Debug.LogError(
                $"[ERRO] Falha ao conectar: {result.ShutdownReason}"
            );
        }
    }

    private void SetupLocalCameraTransform(NetworkRunner runner)
    {
        if (Camera.main == null)
            return;

        bool isMaster = runner.IsSharedModeMasterClient;

        if (isMaster)
        {
            Camera.main.transform.position = player1Position;
            Camera.main.transform.rotation =
                Quaternion.Euler(player1Rotation);
        }
        else
        {
            Camera.main.transform.position =
                player1Position + player2Offset;

            Camera.main.transform.rotation =
                Quaternion.Euler(player2Rotation);
        }

        Debug.Log(
            $"Câmara posicionada para: " +
            $"{(isMaster ? "Jogador 1" : "Jogador 2")}"
        );
    }

    // CALLBACKS DO FUSION

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }

    public void OnReliableDataReceived(
        NetworkRunner runner, PlayerRef player,
        ReliableKey key, ReadOnlySpan<byte> data)
    { }

    public void OnConnectRequest(
        NetworkRunner runner,
        NetworkRunnerCallbackArgs.ConnectRequest request,
        byte[] token)
    { }

    public void OnHostMigration(
        NetworkRunner runner,
        HostMigrationToken hostMigrationToken)
    { }

    public void OnObjectExitAOI(
        NetworkRunner runner, NetworkObject obj, PlayerRef player)
    { }

    public void OnObjectEnterAOI(
        NetworkRunner runner, NetworkObject obj, PlayerRef player)
    { }

    public void OnInput(NetworkRunner runner, NetworkInput input) { }

    public void OnInputMissing(
        NetworkRunner runner, PlayerRef player, NetworkInput input)
    { }

    public void OnShutdown(
        NetworkRunner runner, ShutdownReason shutdownReason)
    { }

    public void OnConnectedToServer(NetworkRunner runner) { }

    public void OnDisconnectedFromServer(
        NetworkRunner runner, NetDisconnectReason cause)
    { }

    public void OnConnectFailed(
        NetworkRunner runner, NetAddress remoteAddress,
        NetConnectFailedReason reason)
    { }

    public void OnUserSimulationMessage(
        NetworkRunner runner, SimulationMessagePtr message)
    { }

    public void OnSessionListUpdated(
        NetworkRunner runner, List<SessionInfo> sessionList)
    { }

    public void OnCustomAuthenticationResponse(
        NetworkRunner runner, Dictionary<string, object> data)
    { }

    public void OnReliableDataProgress(
        NetworkRunner runner, PlayerRef player,
        ReliableKey key, float progress)
    { }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        Debug.Log("[NetTest] Carregamento da cena concluído!");
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        Debug.Log("[NetTest] A carregar a cena...");
    }
}
