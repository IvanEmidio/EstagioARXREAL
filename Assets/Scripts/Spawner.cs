
using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpawnObject
{
    public GameObject prefab;

    [Min(0)]
    public float chance = 1f;
}

[RequireComponent(typeof(BoxCollider))]
public class Spawner : MonoBehaviour
{
    [Header("Objetos para Spawn")]
    [SerializeField]
    private List<SpawnObject> objetos =
        new List<SpawnObject>();

    [Header("Tempo entre Spawns")]
    [SerializeField] private float tempoMinimo = 1f;
    [SerializeField] private float tempoMaximo = 3f;

    [Header("Dificuldade da Ronda")]
    [SerializeField] private float tempoParaDificuldadeMaxima = 120f;
    [SerializeField] private float intervaloMinimoMultiplicador = 0.3f;

    private BoxCollider boxCollider;
    private NetworkRunner runner;

    // Meteoritos criados por este spawner.
    private readonly List<NetworkObject> meteoritosSpawnados =
        new List<NetworkObject>();

    // Tempo decorrido desde o início da ronda.
    private float tempoDaRonda;

    // Countdown até ao próximo spawn.
    private float countdownSpawn;

    // Aumenta sempre que a ronda é reiniciada.
    private int roundGeneration;

    private void Start()
    {
        boxCollider = GetComponent<BoxCollider>();

        ProcurarRunner();

        countdownSpawn = Random.Range(tempoMinimo, tempoMaximo);
    }

    private void Update()
    {
        ProcurarRunner();

        if (runner == null || !runner.IsRunning)
            return;

        // Só o Master Client cria meteoritos.
        if (!runner.IsSharedModeMasterClient)
            return;

        // Não avançar ronda nem criar meteoritos durante o Game Over.
        if (GameHealth.Instance != null &&
            GameHealth.Instance.GetGameOver())
        {
            return;
        }

        tempoDaRonda += Time.deltaTime;
        countdownSpawn -= Time.deltaTime;

        if (countdownSpawn <= 0f)
        {
            Spawn();

            countdownSpawn = CalcularTempoSpawn();
        }
    }

    private void ProcurarRunner()
    {
        if (runner != null && runner.IsRunning)
            return;

        runner = NetworkRunner.GetRunnerForGameObject(gameObject);

        if (runner == null)
            runner = FindFirstObjectByType<NetworkRunner>();
    }

    // =====================================================
    // SPAWN
    // =====================================================

    private float CalcularTempoSpawn()
    {
        float progresso = Mathf.Clamp01(
            tempoDaRonda / Mathf.Max(1f, tempoParaDificuldadeMaxima)
        );

        float fatorDificuldade = Mathf.Lerp(
            1f,
            intervaloMinimoMultiplicador,
            progresso
        );

        float intervaloBase = Random.Range(
            tempoMinimo,
            tempoMaximo
        );

        return intervaloBase * fatorDificuldade;
    }

    private void Spawn()
    {
        if (objetos.Count == 0)
            return;

        ProcurarRunner();

        if (runner == null || !runner.IsRunning)
            return;

        if (!runner.IsSharedModeMasterClient)
            return;

        GameObject prefabToSpawn = EscolherPrefab();

        if (prefabToSpawn == null)
            return;

        NetworkObject networkPrefab =
            prefabToSpawn.GetComponent<NetworkObject>();

        if (networkPrefab == null)
        {
            Debug.LogError(
                "[SPAWNER] Prefab sem NetworkObject!"
            );
            return;
        }

        Bounds bounds = boxCollider.bounds;

        Vector3 posicao = new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );

        NetworkObject meteorito = runner.Spawn(
            networkPrefab,
            posicao,
            Quaternion.identity
        );

        if (meteorito != null)
        {
            meteoritosSpawnados.Add(meteorito);

            Debug.Log(
                $"[SPAWNER] Meteorito criado. " +
                $"Total rastreados: {meteoritosSpawnados.Count}"
            );
        }
    }

    // =====================================================
    // ESCOLHA DO PREFAB
    // =====================================================

    private GameObject EscolherPrefab()
    {
        float somaChances = 0f;

        foreach (SpawnObject obj in objetos)
        {
            if (obj.prefab != null && obj.chance > 0f)
                somaChances += obj.chance;
        }

        if (somaChances <= 0f)
            return null;

        float valor = Random.Range(0f, somaChances);

        foreach (SpawnObject obj in objetos)
        {
            if (obj.prefab == null || obj.chance <= 0f)
                continue;

            valor -= obj.chance;

            if (valor <= 0f)
                return obj.prefab;
        }

        // Fallback seguro.
        for (int i = objetos.Count - 1; i >= 0; i--)
        {
            if (objetos[i].prefab != null)
                return objetos[i].prefab;
        }

        return null;
    }

    // =====================================================
    // RESET DA RONDA
    // =====================================================

    public void ResetRound()
    {
        ProcurarRunner();

        // O reset só pode ser executado pelo Master Client.
        if (runner == null || !runner.IsSharedModeMasterClient)
        {
            Debug.LogWarning(
                "[SPAWNER] ResetRound ignorado. " +
                "Não sou Master Client."
            );
            return;
        }

        Debug.Log("[SPAWNER] RESET DA RONDA!");

        // Invalida o ciclo/timer atual.
        roundGeneration++;

        // Reiniciar dificuldade.
        tempoDaRonda = 0f;

        // Reiniciar countdown.
        countdownSpawn = Random.Range(tempoMinimo, tempoMaximo);

        // Destruir meteoritos criados por este spawner.
        LimparMeteoritos();

        Debug.Log("[SPAWNER] Timer e dificuldade resetados.");
    }

    private void LimparMeteoritos()
    {
        for (int i = meteoritosSpawnados.Count - 1; i >= 0; i--)
        {
            NetworkObject meteorito = meteoritosSpawnados[i];

            if (meteorito == null)
                continue;

            if (!meteorito.IsValid)
                continue;

            if (meteorito.HasStateAuthority)
            {
                runner.Despawn(meteorito);
            }
            else
            {
                Debug.LogWarning(
                    "[SPAWNER] Não tenho State Authority " +
                    "sobre um meteorito. Não foi possível despawn."
                );
            }
        }

        meteoritosSpawnados.Clear();

        Debug.Log("[SPAWNER] Lista de meteoritos limpa.");
    }

    // =====================================================
    // GIZMOS
    // =====================================================

    private void OnDrawGizmosSelected()
    {
        BoxCollider box = GetComponent<BoxCollider>();

        if (box == null)
            return;

        Gizmos.color = Color.green;
        Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.DrawWireCube(box.center, box.size);
    }
}
