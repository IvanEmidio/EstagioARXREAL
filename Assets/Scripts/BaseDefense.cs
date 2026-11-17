using Fusion;
using UnityEngine;

public class BaseDefense : NetworkBehaviour
{
    [Header("Dano dos meteoritos")]
    [SerializeField] private float damageToTeam = 10f;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(
            $"<color=yellow>[BASE DEFENSE]</color> " +
            $"COLISÃO DETETADA! Objeto: {other.name}"
        );

        Health meteorHealth =
            other.GetComponentInParent<Health>();

        if (meteorHealth == null)
        {
            Debug.Log(
                "[BASE DEFENSE] Objeto não tem Health. Ignorado."
            );
            return;
        }

        Debug.Log(
            "<color=green>[BASE DEFENSE]</color> " +
            "METEORO IDENTIFICADO!"
        );

        NetworkObject meteorObject =
            other.GetComponentInParent<NetworkObject>();

        if (meteorObject == null)
        {
            Debug.LogError(
                "[BASE DEFENSE] Meteoro sem NetworkObject!"
            );
            return;
        }

        Debug.Log(
            $"[BASE DEFENSE] Autoridade do meteoro: " +
            $"{meteorObject.HasStateAuthority}"
        );

        // Apenas a autoridade do meteoro processa o impacto.
        if (!meteorObject.HasStateAuthority)
        {
            Debug.Log(
                "[BASE DEFENSE] Não tenho autoridade sobre este meteoro. " +
                "Outro jogador vai processar o impacto."
            );
            return;
        }

        Debug.Log(
            "<color=green>[BASE DEFENSE]</color> " +
            "Tenho autoridade sobre o meteoro!"
        );

        GameHealth gameHealth = GameHealth.Instance;

        if (gameHealth == null)
        {
            Debug.LogError(
                "[BASE DEFENSE] GameHealth.Instance está NULL!"
            );
            return;
        }

        Debug.Log(
            $"<color=orange>[BASE DEFENSE]</color> " +
            $"A ENVIAR PEDIDO DE DANO: -{damageToTeam} HP"
        );

        Debug.LogError($"[TESTE] ESTOU A TENTAR RETIRAR {damageToTeam} DE VIDA!");
        gameHealth.RPC_TakeSharedDamage(damageToTeam);
        Debug.LogError("[TESTE] TERMINEI A CHAMADA DA RPC!");

        Debug.Log(
            "[BASE DEFENSE] Pedido RPC enviado. " +
            "A destruir meteoro..."
        );

        Runner.Despawn(meteorObject);

        Debug.Log(
            "<color=red>[BASE DEFENSE]</color> " +
            "METEORO DESTRUÍDO!"
        );
    }
}
