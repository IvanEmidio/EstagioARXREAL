using UnityEngine;

public class Shooter : MonoBehaviour
{
    [Header("Configurações do Disparo")]
    [SerializeField] private GameObject objectToShoot;
    [SerializeField] private float fireInterval = 0.15f;
    [SerializeField] private Transform firePoint; // Ponto de saída da arma
    [SerializeField] private Camera mainCamera; // Câmara do jogador (onde está a mira)
    [SerializeField] private LayerMask shootableLayers = ~0; // Para ignorar o próprio player se necessário

    private float timer;

    void Start()
    {
        // Se não atribuíres no Inspector, apanha a câmara principal automaticamente
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= fireInterval)
        {
            Shoot();
            timer = 0f;
        }
    }

    private void Shoot()
    {
        if (objectToShoot == null) return;

        // 1. Lança um raio a partir do centro exato do ecrã (crosshair)
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hitInfo, 1000f, shootableLayers))
        {
            // Ponto de impacto no cenário/inimigo onde a mira está a apontar
            targetPoint = hitInfo.point;
        }
        else
        {
            // Se estiver a apontar para o céu/nada, define um ponto distante à frente
            targetPoint = ray.GetPoint(1000f);
        }

        // 2. Define o ponto de saída (cano da arma ou câmara)
        Vector3 spawnPosition = firePoint != null ? firePoint.position : mainCamera.transform.position;

        // 3. Calcula a direção do cano da arma até ao ponto onde a mira está a apontar
        Vector3 direction = (targetPoint - spawnPosition).normalized;

        // 4. Cria a rotação correta apontada para a mira
        Quaternion spawnRotation = Quaternion.LookRotation(direction);

        // 5. Instancia a bala virada exatamente para o alvo
        Instantiate(objectToShoot, spawnPosition, spawnRotation);
    }
}