using System.Collections.Generic;
using UnityEngine;
using Fusion;

[System.Serializable]
public class SpawnObject
{
    // Voltamos a usar o prefab normal do Unity para ser mais fácil de arrastar no Inspector
    public GameObject prefab;
    [Min(0)]
    public float chance = 1f;
}

[RequireComponent(typeof(BoxCollider))]
public class Spawner : MonoBehaviour // <- Mudado de volta para MonoBehaviour normal (evita o erro do Network Object)
{
    [Header("Objetos para Spawn")]
    [SerializeField] private List<SpawnObject> objetos = new List<SpawnObject>();

    [Header("Tempo entre Spawns")]
    [SerializeField] private float tempoMinimo = 1f;
    [SerializeField] private float tempoMaximo = 3f;

    private BoxCollider boxCollider;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        StartCoroutine(SpawnLoop());
    }

    System.Collections.IEnumerator SpawnLoop()
    {

        while (true)
        {
            yield return new WaitForSeconds(Random.Range(tempoMinimo, tempoMaximo));
            var runner = NetworkRunner.GetRunnerForGameObject(gameObject);
            if (runner == null || !runner.IsRunning)
            {
                continue; // Se a rede ainda não estiver pronta, salta este ciclo e tenta no próximo
            }
            Spawn();
        }
    }

    void Spawn()
    {
        if (objetos.Count == 0) return;

        // Procura o NetworkRunner ativo na cena para podermos fazer o Spawn em rede
        NetworkRunner runner = NetworkRunner.GetRunnerForGameObject(gameObject);

        // Se por acaso a rede ainda não estiver pronta, tenta na próxima vez
        if (runner == null || !runner.IsServer && !runner.IsSharedModeMasterClient)
        {
            // Nota: No Shared Mode, qualquer um pode dar spawn, mas para evitar duplicados 
            // podemos deixar apenas o MasterClient ou o primeiro cliente criar. 
            // Vamos garantir que apenas o MasterClient (Player 1) cria os meteoros para todos:
            if (runner != null && !runner.IsSharedModeMasterClient) return;
        }

        GameObject prefabToSpawn = EscolherPrefab();
        if (prefabToSpawn == null) return;

        Bounds bounds = boxCollider.bounds;
        Vector3 posicao = new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );

        // Se encontrarmos o Runner, usamos o Runner.Spawn mapeando o prefab pelo seu NetworkObject
        if (runner != null)
        {
            NetworkObject netObj = prefabToSpawn.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                runner.Spawn(netObj, posicao, Quaternion.identity);
            }
            else
            {
                Debug.LogError("O prefab do meteoro não tem o componente Network Object!");
            }
        }
    }

    GameObject EscolherPrefab()
    {
        float somaChances = 0f;

        foreach (var obj in objetos)
        {
            if (obj.prefab != null)
                somaChances += obj.chance;
        }

        if (somaChances <= 0)
            return null;

        float valor = Random.Range(0f, somaChances);

        foreach (var obj in objetos)
        {
            if (obj.prefab == null)
                continue;

            valor -= obj.chance;

            if (valor <= 0)
                return obj.prefab;
        }

        return objetos[objetos.Count - 1].prefab;
    }

    void OnDrawGizmosSelected()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (box == null) return;

        Gizmos.color = Color.green;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(box.center, box.size);
    }
}