using Fusion;
using UnityEngine;

public class LaserNetworkRelay : NetworkBehaviour
{
    [SerializeField] private ChargeLaser chargeLaser;

    public void BroadcastLaser(Vector3 origin, Vector3 direction)
    {
        if (Object == null || !Object.IsValid)
            return;

        RPC_ShowLaser(origin, direction);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_ShowLaser(Vector3 origin, Vector3 direction, RpcInfo info = default)
    {
        // Não reproduzir novamente o laser no jogador que o disparou.
        if (info.Source == Runner.LocalPlayer)
            return;

        if (chargeLaser == null)
            chargeLaser = GetComponentInChildren<ChargeLaser>();

        if (chargeLaser != null)
            chargeLaser.TriggerRemoteLaser(origin, direction);
    }
}
