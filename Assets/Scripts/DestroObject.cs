using Fusion;
using UnityEngine;

public class DestroObject : NetworkBehaviour // Mudado para NetworkBehaviour
{
    [SerializeField] private float timeToSelfDestruct = 10f;
    private float timer;

    public override void FixedUpdateNetwork()
    {
        // Apenas quem tem autoridade de estado gere a destruição temporizada da bala
        if (Object.HasStateAuthority)
        {
            timer += Runner.DeltaTime;
            if (timer >= timeToSelfDestruct)
            {
                Runner.Despawn(Object);
            }
        }
    }
}