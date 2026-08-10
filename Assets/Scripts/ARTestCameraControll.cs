using UnityEngine;
using UnityEngine.InputSystem;

public class ARTestCameraController : MonoBehaviour
{
    [SerializeField] private float lookSpeed = 0.2f;
    private float _yaw, _pitch;

    public void ResetAngles()
    {
        Vector3 angles = transform.eulerAngles;
        _yaw = angles.y; _pitch = angles.x;
    }

    private void Update()
    {
        // Rotação apenas com botão direito (ou sem botão, se preferires)
        if (Mouse.current != null && Mouse.current.rightButton.isPressed)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();
            _yaw += delta.x * lookSpeed;
            _pitch -= delta.y * lookSpeed;
            _pitch = Mathf.Clamp(_pitch, -85f, 85f);

            transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        }
    }
}