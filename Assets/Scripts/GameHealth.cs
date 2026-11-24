
using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class GameHealth : NetworkBehaviour
{
    public static GameHealth Instance { get; private set; }

    [Header("Vida da Equipa")]
    [SerializeField] private float maxHealth = 100f;

    [Networked, OnChangedRender(nameof(OnHealthChanged))]
    private float currentHealth { get; set; }

    [Networked, OnChangedRender(nameof(OnGameOverChanged))]
    private NetworkBool IsGameOver { get; set; }

    [Header("Health Slider")]
    [SerializeField] private Slider healthSlider;

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button restartButton;

    private string DebugPlayer =>
        Runner != null ? Runner.LocalPlayer.ToString() : "Sem Runner";

    public override void Spawned()
    {
        Instance = this;

        Debug.Log(
            $"[GAME HEALTH] SPAWNED | Jogador: {DebugPlayer} | " +
            $"State Authority: {Object.HasStateAuthority}"
        );

        if (Object.HasStateAuthority)
        {
            currentHealth = maxHealth;
            IsGameOver = false;
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartGame);
            restartButton.onClick.AddListener(RestartGame);
        }

        UpdateUI();

        if (IsGameOver)
            TriggerGameOver();
    }

    public float GetHealth()
    {
        return currentHealth;
    }

    public bool GetGameOver()
    {
        return IsGameOver;
    }

    // =====================================================
    // DANO PARTILHADO
    // =====================================================

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_TakeSharedDamage(float damage, RpcInfo info = default)
    {
        if (!Object.HasStateAuthority)
            return;

        if (damage <= 0f || IsGameOver)
            return;

        float oldHealth = currentHealth;

        currentHealth = Mathf.Max(0f, currentHealth - damage);

        Debug.Log(
            $"[DAMAGE] {oldHealth} -> {currentHealth} | " +
        $"Origem: {info.Source}"
    );

        if (currentHealth <= 0f)
        {
            IsGameOver = true;

            Debug.LogError("[GAME OVER] VIDA CHEGOU A ZERO!");

            TriggerGameOver();
        }
    }

    // =====================================================
    // GAME OVER
    // =====================================================

    private void OnHealthChanged()
    {
        UpdateUI();
    }

    private void OnGameOverChanged()
    {
        if (IsGameOver)
        {
            TriggerGameOver();
        }
        else
        {
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);

            UpdateUI();
        }
    }


    private void UpdateUI()
    {
        float normalizedHealth = Mathf.Clamp01(currentHealth / maxHealth);

        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = maxHealth;
            healthSlider.value = Mathf.Clamp(currentHealth, 0f, maxHealth);

            // Garantir que não resta qualquer preenchimento
            if (currentHealth <= 0f)
            {
                healthSlider.value = 0f;

                if (healthSlider.fillRect != null)
                    healthSlider.fillRect.localScale = new Vector3(0f, 1f, 1f);
            }
            else
            {
                if (healthSlider.fillRect != null)
                    healthSlider.fillRect.localScale = Vector3.one;
            }
        }
    }


    private void TriggerGameOver()
    {
        Debug.LogError(
            $"[GAME OVER] ATIVADO | Jogador: {DebugPlayer}"
    );

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    // =====================================================
    // RESTART
    // =====================================================

    public void RestartGame()
    {
        if (Object == null || !Object.IsValid)
            return;

        if (!IsGameOver)
            return;

        Debug.Log("[RESTART] Pedido de restart enviado.");

        RPC_RestartGame();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RestartGame()
    {
        if (!Object.HasStateAuthority)
            return;

        if (!IsGameOver)
            return;

        Debug.Log("[RESTART] A reiniciar a ronda...");

        // Reiniciar os spawners da cena.
        Spawner[] spawners = FindObjectsByType<Spawner>(
            FindObjectsSortMode.None
        );

        foreach (Spawner spawner in spawners)
        {
            spawner.ResetRound();
        }

        // Restaurar vida.
        currentHealth = maxHealth;

        // Terminar o estado de Game Over.
        IsGameOver = false;

        Debug.Log("[RESTART] RONDA REINICIADA COM SUCESSO!");
    }

    // =====================================================
    // LIMPEZA
    // =====================================================

    public override void Despawned(
        NetworkRunner runner,
        bool hasState)
    {
        if (restartButton != null)
            restartButton.onClick.RemoveListener(RestartGame);

        if (Instance == this)
            Instance = null;
    }
}
