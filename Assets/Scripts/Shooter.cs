using Fusion;
using UnityEngine;

public class Shooter : MonoBehaviour // Mudado de volta para MonoBehaviour para não exigir Network Object
{
    [Header("Configurações do Disparo")]
    [SerializeField] private NetworkPrefabRef objectToShoot;
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
        // Encontra o Runner ativo na sessão se ainda não o tiver guardado
        if (_runner == null || !_runner.IsRunning)
        {
            _runner = NetworkRunner.GetRunnerForGameObject(gameObject);
            if (_runner == null)
            {
                _runner = FindFirstObjectByType<NetworkRunner>();
            }
            return;
        }

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
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return;

        // Dispara o raio a partir do centro do ecrã
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

        // Faz o spawn da bala através do NetworkRunner encontrado
        if (_runner != null)
        {
            _runner.Spawn(objectToShoot, spawnPosition, spawnRotation);
        }
    }
}