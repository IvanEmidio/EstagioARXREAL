using Fusion;
using UnityEngine;

public class Health : NetworkBehaviour // Mudado para NetworkBehaviour
{
    [SerializeField] private float Maxhealth = 100f;

    [Networked] private float health { get; set; } // Sincronizado automaticamente pelo Fusion

    [SerializeField] private HealthCanvas healthCanvas;

    public override void Spawned()
    {
        health = Maxhealth;
    }

    public void TakeHealth(float take)
    {
        // Apenas quem tem autoridade de estado altera a vida na rede
        if (Object.HasStateAuthority)
        {
            health -= take;
            Verify();
        }
        UpdateUi();
    }

    public float GetHealth() { return health; }

    private void UpdateUi()
    {
        if (healthCanvas != null) healthCanvas.UpdateHealth();
    }

    private void Verify()
    {
        if (health <= 0 && Object.HasStateAuthority)
        {
            // Destrói o objeto de forma sincronizada na rede
            Runner.Despawn(Object);
        }
    }
}