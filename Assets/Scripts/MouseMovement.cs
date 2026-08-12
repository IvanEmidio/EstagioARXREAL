using UnityEngine;

public class MouseMovement : MonoBehaviour
{
    [Header("Configurações do Rato")]
    public float sensitivity = 100f;

    private float xRotation = 0f; // Pitch (olhar para cima/baixo)
    private float yRotation = 0f; // Yaw (olhar para a esquerda/direita)

    void Start()
    {
        // Trava o cursor no centro da tela e oculta-o
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Captura a variação do movimento do rato
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        // Atualiza os ângulos de rotação
        yRotation += mouseX;
        xRotation -= mouseY; // Subtrai para que subir o rato olhe para cima

        // Limita a rotação vertical entre -90º e 90º (evita dar a volta completa)
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Aplica a rotação na câmera
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }
}
