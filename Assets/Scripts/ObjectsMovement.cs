using Fusion;
using UnityEngine;
using UnityEngine.UIElements;
using static Unity.Collections.Unicode;

public class ObjectsMovement : NetworkBehaviour
{
    [SerializeField] private float speed = 50f;
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private bool rotation;

    [Networked] private Vector3 moveDirection { get; set; }

    public override void Spawned()
    {
        // Apenas o dono/criador define a direção inicial
        if (Object.HasStateAuthority)
        {
            moveDirection = transform.forward;
        }
    }

    public override void FixedUpdateNetwork()
    {
        // Em Shared Mode, permitimos que quem tem autoridade (ou todos, se quisermos movimento fluido) processe o movimento.
        // Como estamos a usar o NetworkTransform no objeto, alterar o transform.position dentro do FixedUpdateNetwork 
        // requer que o objeto permita escrita local ou que a State Authority esteja livre.

        // Vamos forçar o movimento para que corra independentemente de quem olha, 
        // desde que o objeto seja válido:
        if (Object.HasStateAuthority || Runner.IsSharedModeMasterClient)
        {
            transform.position -= moveDirection * speed * Runner.DeltaTime;

            if (rotation)
            {
                transform.rotation *= Quaternion.Euler(Vector3.right * rotationSpeed * Runner.DeltaTime);
            }
        }
    }
}