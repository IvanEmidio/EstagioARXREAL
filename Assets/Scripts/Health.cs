using Fusion;
using System.Collections.Generic;
using System.Linq;
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

    [Header("Co-op Hit")]
    [SerializeField] private bool allPlayerNeedToHit;
    [SerializeField] private float resetTime = 3f;

    [Header("VFX de Morte")]
    [SerializeField] private GameObject deathVfxPrefab;

    // Guarda os jogadores únicos que já acertaram (não repete o mesmo jogador)
    private HashSet<PlayerRef> playersWhoHit = new HashSet<PlayerRef>();
    private float timer;

    public override void Spawned()
    {
        if (Object == null) return;
        if (Object.HasStateAuthority)
        {
            currentHealth = maxHealth;
            timer = resetTime;
        }
    }

    public float GetHealth()
    {
        return currentHealth;
    }

    private void Update()
    {
        // Só quem tem StateAuthority gere a lógica de reset
        if (Object == null) return;
        if (!Object.HasStateAuthority) return;

        if (playersWhoHit.Count > 0)
        {
            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                playersWhoHit.Clear();
                timer = resetTime;
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_TakeHealth(float damage, RpcInfo info = default)
    {
        if (allPlayerNeedToHit)
        {
            if (playersWhoHit.Count == 0)
            {
                timer = resetTime;
            }

            playersWhoHit.Add(info.Source);

            if (playersWhoHit.Count < Runner.ActivePlayers.Count())
            {
                return;
            }

            playersWhoHit.Clear();
        }

        currentHealth -= damage;

        if (currentHealth <= 0f)
        {
            SpawnDeathVFX();
            Die();
        }
    }

    private void OnHealthChanged()
    {
        UpdateUI();
    }

    private void SpawnDeathVFX()
    {
        if (deathVfxPrefab != null)
        {
            Instantiate(deathVfxPrefab, transform.position, Quaternion.identity);
        }
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