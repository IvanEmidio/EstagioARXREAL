using Fusion;
using UnityEngine;
using UnityEngine.UI; // Se usares UI tradicional ou TextMeshPro

public class Health : NetworkBehaviour
{
    [Header("Configuração de Vida")]
    [SerializeField] private float maxHealth = 100f;

    // Esta variável é sincronizada automaticamente pelo Fusion para todos os PCs!
    [Networked, OnChangedRender(nameof(OnHealthChanged))]
    private float currentHealth { get; set; }

    [Header("UI (Opcional)")]
    [SerializeField] private Image healthBarFill; // Ou Text para mostrar o valor

    public override void Spawned()
    {
        // Quem cria o objeto inicializa a vida máxima
        if (Object.HasStateAuthority)
        {
            currentHealth = maxHealth;
        }
    }

    public float GetHealth()
    {
        return currentHealth;
    }

    // Método chamado quando a bala embate no meteoro (chamado pelo script Attack da bala)
    public void TakeHealth(float damage)
    {
        // Apenas quem tem autoridade de estado é que altera o valor da vida
        if (!Object.HasStateAuthority) return;

        currentHealth -= damage;

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    // Este método dispara automaticamente em TODOS os PCs quando a variável currentHealth muda na rede
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
        // Destrói o meteoro de forma sincronizada na rede para todos os jogadores
        if (Runner != null && Object != null)
        {
            Runner.Despawn(Object);
        }
    }
}