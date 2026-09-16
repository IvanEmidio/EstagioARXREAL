using Fusion;
using UnityEngine;

public class Attack : NetworkBehaviour
{
    [SerializeField] private LayerMask mask;
    [SerializeField] private float Damage;

    private void OnTriggerEnter(Collider other)
    {
        // Deixamos de verificar HasStateAuthority aqui para permitir que a bala de qualquer jogador processe o impacto
        if (((1 << other.gameObject.layer) & mask) != 0)
        {
            Health health = other.GetComponent<Health>();
            if (health != null)
            {
                // Em vez de mexer direto na vida, chamamos a RPC do meteoro
                health.RPC_TakeHealth(Damage);
            }

            // Destrói a bala na rede após embater
            if (Runner != null && Object != null && Object.HasStateAuthority)
            {
                Runner.Despawn(Object);
            }
        }
    }
}