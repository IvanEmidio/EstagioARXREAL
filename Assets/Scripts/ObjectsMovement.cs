using Fusion;
using UnityEngine;

public class ObjectsMovement : NetworkBehaviour
{
    [SerializeField] private float speed = 20f; // Podes aumentar a velocidade da bala aqui se estiver lenta
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private bool rotation;

    [Networked] private Vector3 moveDirection { get; set; }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            moveDirection = transform.forward;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority)
        {
            transform.position += moveDirection * speed * Runner.DeltaTime;

            if (rotation)
            {
                transform.rotation *= Quaternion.Euler(Vector3.right * rotationSpeed * Runner.DeltaTime);
            }
        }
    }
}