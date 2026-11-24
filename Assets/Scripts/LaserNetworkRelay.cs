using Fusion;
using UnityEngine;

public class LaserNetworkRelay : NetworkBehaviour
{
    [SerializeField] private ChargeLaser chargeLaser;

    public void BroadcastLaser(Vector3 origin, Vector3 direction)
    {
        if (Object == null || !Object.IsValid)
        {
            Debug.LogWarning("[LaserRelay] NetworkObject inválido!");
            return;
        }

        Debug.Log("[LaserRelay] A ENVIAR laser RPC!");

        RPC_ShowLaser(origin, direction);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_ShowLaser(Vector3 origin, Vector3 direction, RpcInfo info = default)
    {
        Debug.Log($"[LaserRelay] RPC RECEBIDO! Source: {info.Source} | Local: {Runner.LocalPlayer}");

        // Não reproduzir novamente no jogador que disparou
        if (info.Source == Runner.LocalPlayer)
            return;

        if (chargeLaser == null)
            chargeLaser = GetComponentInChildren<ChargeLaser>();

        if (chargeLaser == null)
        {
            Debug.LogError("[LaserRelay] Não encontrei ChargeLaser neste jogador!");
            return;
        }

        Debug.Log("[LaserRelay] A reproduzir laser remoto!");

        chargeLaser.TriggerRemoteLaser(origin, direction);
    }
}