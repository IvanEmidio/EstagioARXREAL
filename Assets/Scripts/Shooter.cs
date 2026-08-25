using Fusion;
using UnityEngine;

public class Shooter : MonoBehaviour // Mantém-se como MonoBehaviour se estiver na câmara
{
    [Header("Configurações do Disparo")]
    [SerializeField] private NetworkPrefabRef objectToShoot; // Alterado para NetworkPrefabRef do Fusion
    [SerializeField] private float fireInterval = 0.15f;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask shootableLayers = ~0;

    private float timer;
    private NetworkRunner _runner;

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
    }

    void Update()
    {
        // Encontra o Runner ativo na sala
        if (_runner == null)
        {
            _runner = FindFirstObjectByType<NetworkRunner>();
            return;
        }

        if (_runner == null || !_runner.IsRunning) return;

        timer += Time.deltaTime;

        if (timer >= fireInterval)
        {
            Shoot();
            timer = 0f;
        }
    }

    private void Shoot()
    {
        if (!objectToShoot.IsValid) return;

        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hitInfo, 1000f, shootableLayers))
        {
            targetPoint = hitInfo.point;
        }
        else
        {
            targetPoint = ray.GetPoint(1000f);
        }

        Vector3 spawnPosition = firePoint != null ? firePoint.position : mainCamera.transform.position;
        Vector3 direction = (targetPoint - spawnPosition).normalized;
        Quaternion spawnRotation = Quaternion.LookRotation(direction);

        // Substituição do Instantiate local por Spawn em Rede do Fusion
        _runner.Spawn(objectToShoot, spawnPosition, spawnRotation);
    }
}