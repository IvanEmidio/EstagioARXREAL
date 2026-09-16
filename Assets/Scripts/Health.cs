using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class Health : NetworkBehaviour
{
    [Header("Configuração de Vida")]
    [SerializeField] private float maxHealth = 100f;

    [Networked, OnChangedRender(nameof(OnHealthChanged))]
    private float currentHealth { get; set; }

    [Header("UI (Opcional)")]
    [SerializeField] private Image healthBarFill;

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            currentHealth = maxHealth;
        }
    }

    public float GetHealth()
    {
        return currentHealth;
    }

    // Esta função passa a ser uma RPC (Remote Procedure Call)
    // RpcSources.All significa que QUALQUER jogador pode enviar o pedido de dano
    // RpcTargets.StateAuthority significa que é executado no computador que tem autoridade sobre o meteoro
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_TakeHealth(float damage)
    {
        // Como corre apenas na máquina com autoridade, não há conflitos de concorrência!
        currentHealth -= damage;

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void OnHealthChanged()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = currentHealth / maxHealth;
        }
    }

    private void Die()
    {
        if (Runner != null && Object != null)
        {
            Runner.Despawn(Object);
        }
    }
}