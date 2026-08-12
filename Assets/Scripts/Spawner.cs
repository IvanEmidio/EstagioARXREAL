using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpawnObject
{
    public Objects prefab;
    [Min(0)]
    public float chance = 1f;
}

[RequireComponent(typeof(BoxCollider))]
public class Spawner : MonoBehaviour
{
    [Header("Objetos para Spawn")]
    public List<SpawnObject> objetos = new List<SpawnObject>();

    [Header("Tempo entre Spawns")]
    public float tempoMinimo = 1f;
    public float tempoMaximo = 3f;

    private BoxCollider boxCollider;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(tempoMinimo, tempoMaximo));
            Spawn();
        }
    }

    void Spawn()
    {
        if (objetos.Count == 0)
            return;

        GameObject prefab = EscolherPrefab();

        if (prefab == null)
            return;

        Bounds bounds = boxCollider.bounds;

        Vector3 posicao = new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );

        Instantiate(prefab, posicao, Quaternion.identity);
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
                return obj.prefab.GameObject;
        }

        return objetos[objetos.Count - 1].prefab.GameObject;
    }

    void OnDrawGizmosSelected()
    {
        BoxCollider box = GetComponent<BoxCollider>();

        if (box == null)
            return;

        Gizmos.color = Color.green;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(box.center, box.size);
    }
}