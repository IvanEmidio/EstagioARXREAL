using System.Collections;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ChargeLaser : MonoBehaviour
{
    [Header("Referências")]
    public Camera mainCamera; // Atribuído automaticamente pelo Shooter

    [Header("Configurações Gerais")]
    public float maxDistance = 50f;
    public LayerMask hitLayers; // O que o laser pode atingir (ex: Inimigos, Paredes)
    public float damage = 50f;

    [Header("Tempos das Fases (Segundos)")]
    public float aimDuration = 1.0f;      // Tempo da linha fina de aviso
    public float blastDuration = 0.25f;   // Tempo que o feixe grosso fica ativo
    public float fadeDuration = 0.15f;    // Tempo a encolher até sumir

    [Header("Larguras do Feixe")]
    public float aimWidth = 0.05f;
    public float blastWidth = 1.2f;       // Largura da "explosão" do laser

    [Header("Cores (HDR para brilhar com Bloom)")]
    [ColorUsage(true, true)] public Color aimColor = new Color(1f, 0f, 0f, 0.5f);
    [ColorUsage(true, true)] public Color blastColor = new Color(3f, 0.5f, 0.5f, 1f);

    private LineRenderer lineRenderer;
    private bool isFiring = false;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;
        if (mainCamera == null) mainCamera = Camera.main;
    }

    public void TriggerLaser()
    {
        if (!isFiring)
        {
            StartCoroutine(FireSequence());
            Debug.Log("shoot");
        }
    }

    private IEnumerator FireSequence()
    {
        isFiring = true;
        lineRenderer.enabled = true;

        // --- FASE 1: MIRA / AVISO (Linha fina) ---
        lineRenderer.startWidth = aimWidth;
        lineRenderer.endWidth = aimWidth;
        lineRenderer.startColor = aimColor;
        lineRenderer.endColor = aimColor;

        float timer = 0f;
        Vector3 lockedDirection = transform.forward;
        Vector3 endPoint = Vector3.zero;

        while (timer < aimDuration)
        {
            timer += Time.deltaTime;

            // Calcula o ângulo desde a arma até ao centro do ecrã a cada frame
            lockedDirection = GetAimDirection();
            endPoint = CalculateEndPoint(0.01f, lockedDirection);

            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, endPoint);

            yield return null;
        }

        // --- FASE 2: DISPARO / EXPANSÃO (Explosão) ---
        lineRenderer.startWidth = blastWidth;
        lineRenderer.endWidth = blastWidth;
        lineRenderer.startColor = blastColor;
        lineRenderer.endColor = blastColor;

        // Usa a última direção travada (lockedDirection) com a largura da explosão
        endPoint = CalculateEndPoint(blastWidth * 0.4f, lockedDirection);
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, endPoint);

        // Aplica o dano na área do feixe grosso
        DealBlastDamage(endPoint, lockedDirection);

        yield return new WaitForSeconds(blastDuration);

        // --- FASE 3: DISSIPAÇÃO (Encolhe até desaparecer) ---
        float fadeTimer = 0f;
        while (fadeTimer < fadeDuration)
        {
            fadeTimer += Time.deltaTime;
            float progress = fadeTimer / fadeDuration;

            float currentWidth = Mathf.Lerp(blastWidth, 0f, progress);
            lineRenderer.startWidth = currentWidth;
            lineRenderer.endWidth = currentWidth;

            // Mantém o ponto inicial colado à arma caso o jogador se mexa enquanto o feixe desaparece
            lineRenderer.SetPosition(0, transform.position);

            yield return null;
        }

        lineRenderer.enabled = false;
        isFiring = false;
    }

    // Calcula a direção da ponta da arma (transform.position) até ao centro do ecrã
    private Vector3 GetAimDirection()
    {
        if (mainCamera == null) return transform.forward;

        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 targetPoint;

        // Ignora o próprio jogador se a layer do player não estiver incluída em hitLayers
        if (Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance, hitLayers))
        {
            targetPoint = hitInfo.point;
        }
        else
        {
            targetPoint = ray.GetPoint(maxDistance);
        }

        return (targetPoint - transform.position).normalized;
    }

    private Vector3 CalculateEndPoint(float radius, Vector3 direction)
    {
        if (Physics.SphereCast(transform.position, radius, direction, out RaycastHit hit, maxDistance, hitLayers))
        {
            return hit.point;
        }
        return transform.position + direction * maxDistance;
    }

    private void DealBlastDamage(Vector3 endPoint, Vector3 direction)
    {
        float distance = Vector3.Distance(transform.position, endPoint);

        RaycastHit[] hits = Physics.SphereCastAll(transform.position, blastWidth * 0.5f, direction, distance, hitLayers);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.TryGetComponent(out Health targetHealth))
            {
                targetHealth.RPC_TakeHealth(damage);
            }
        }
    }
}