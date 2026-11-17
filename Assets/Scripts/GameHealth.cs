
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

    [Header("Barra de Vida")]
    [SerializeField] private Image healthBarFill;

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

            Debug.Log(
                $"[GAME HEALTH] VIDA INICIALIZADA: {currentHealth}"
            );
        }

        UpdateUI();

        // Verificar estado inicial, incluindo late joiners.
        if (IsGameOver)
        {
            TriggerGameOver();
        }
    }

    public float GetHealth()
    {
        return currentHealth;
    }

    public bool GetGameOver()
    {
        return IsGameOver;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_TakeSharedDamage(float damage, RpcInfo info = default)
    {
        Debug.Log(
            $"[DAMAGE] RPC RECEBIDA | Jogador: {info.Source} | " +
            $"Dano: {damage}"
        );

        if (!Object.HasStateAuthority)
            return;

        if (damage <= 0f || IsGameOver)
        {
            Debug.LogWarning("[DAMAGE] Dano ignorado!");
            return;
        }

        float oldHealth = currentHealth;

        currentHealth = Mathf.Max(0f, currentHealth - damage);

        Debug.LogError(
            $"[DAMAGE] VIDA: {oldHealth} -> {currentHealth}"
        );

        if (currentHealth <= 0f)
        {
            IsGameOver = true;

            Debug.LogError(
                "[GAME OVER] VIDA CHEGOU A ZERO!"
            );

            TriggerGameOver();
        }
    }

    private void OnHealthChanged()
    {
        Debug.Log(
            $"[GAME HEALTH] VIDA SINCRONIZADA: " +
            $"{currentHealth}/{maxHealth} | Jogador: {DebugPlayer}"
        );

        UpdateUI();
    }

    private void OnGameOverChanged()
    {
        Debug.LogError(
            $"[GAME OVER] ESTADO SINCRONIZADO! " +
            $"Jogador: {DebugPlayer}"
        );

        if (IsGameOver)
        {
            TriggerGameOver();
        }
    }

    private void UpdateUI()
    {
        if (healthBarFill == null)
            return;

        healthBarFill.fillAmount =
            Mathf.Clamp01(currentHealth / maxHealth);
    }

    private void TriggerGameOver()
    {
        Debug.LogError(
            $"[GAME OVER] ATIVADO NO JOGADOR {DebugPlayer}! " +
            "A BASE FOI DESTRUÍDA!"
        );

        // Aqui vamos posteriormente ativar o ecrã de Game Over.
    }

    public override void Despawned(
        NetworkRunner runner,
        bool hasState)
    {
        Debug.Log("[GAME HEALTH] Objeto removido da rede.");

        if (Instance == this)
        {
            Instance = null;
        }
    }
}
