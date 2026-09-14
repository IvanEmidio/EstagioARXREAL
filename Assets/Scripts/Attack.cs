using Fusion;
using UnityEngine;

public class Attack : NetworkBehaviour // Mudado para NetworkBehaviour
{
    [SerializeField] private LayerMask mask;
    [SerializeField] private float Damage;

    private void OnTriggerEnter(Collider other)
    {
        // Apenas o PC com autoridade processa a colisão para evitar duplicação de dano
        if (Object != null && !Object.HasStateAuthority) return;

        // Converte a camada do objeto em bitmask e verifica se ela está presente no LayerMask
        if (((1 << other.gameObject.layer) & mask) != 0)
        {
            Health health = other.GetComponent<Health>();
            if (health != null)
            {
                health.TakeHealth(Damage);
            }

            // Destrói a bala de forma sincronizada na rede após embater em algo
            if (Runner != null && Object != null)
            {
                Runner.Despawn(Object);
            }
        }
    }
}