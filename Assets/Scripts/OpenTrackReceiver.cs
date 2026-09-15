using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class OpenTrackReceiver : MonoBehaviour
{
    [Header("Configurações de Rede")]
    public int port = 4242;

    [Header("Objeto Alvo")]
    public Transform targetTransform;

    [Header("Tecla para Centrar")]
    public KeyCode centerKey = KeyCode.C;

    [Header("Ajuste Manual")]
    public float manualPitchOffset = 0f;

    [Header("Suavização (Aumenta se tremer, diminui se ficar pesado)")]
    [Range(5f, 50f)]
    public float smoothingSpeed = 25f;

    private UdpClient udpClient;
    private Thread receiveThread;
    private bool isRunning = true;

    private Vector3 rawRotation;
    private readonly object lockObject = new object();

    private float pitchCenter = 0f;
    private float yawCenter = 0f;
    private bool isCalibrated = false;

    // Valores suavizados anteriores para evitar saltos bruscos
    private float smoothPitch = 0f;
    private float smoothYaw = 0f;

    void Start()
    {
        if (targetTransform == null)
            targetTransform = transform;

        receiveThread = new Thread(ReceiveData);
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    void ReceiveData()
    {
        try
        {
            udpClient = new UdpClient(port);
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

            while (isRunning)
            {
                byte[] data = udpClient.Receive(ref remoteEndPoint);

                if (data.Length >= 48)
                {
                    double yaw = BitConverter.ToDouble(data, 24);
                    double pitch = BitConverter.ToDouble(data, 32);

                    lock (lockObject)
                    {
                        rawRotation = new Vector3((float)pitch, (float)yaw, 0f);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("UDP: " + e.Message);
        }
    }

    void LateUpdate()
    {
        Vector3 rot;
        lock (lockObject)
        {
            rot = rawRotation;
        }

        float currentPitch = -rot.x;
        float currentYaw = rot.y;

        // Calibragem no início ou ao carregar no 'C'
        if (!isCalibrated || Input.GetKeyDown(centerKey))
        {
            pitchCenter = currentPitch;
            yawCenter = currentYaw;
            smoothPitch = 0f;
            smoothYaw = 0f;
            isCalibrated = true;
            Debug.Log("Centro calibrado!");
        }

        // Calcula os ângulos relativos
        float targetPitch = (currentPitch - pitchCenter) + manualPitchOffset;
        float targetYaw = currentYaw - yawCenter;

        // Suaviza os valores numéricos ANTES de criar o Quaternion (evita o jitter bruto)
        smoothPitch = Mathf.Lerp(smoothPitch, targetPitch, Time.deltaTime * smoothingSpeed);
        smoothYaw = Mathf.Lerp(smoothYaw, targetYaw, Time.deltaTime * smoothingSpeed);

        // Aplica a rotação de forma limpa usando Euler (Pitch no X, Yaw no Y, Roll a 0 para não inclinar de lado)
        Quaternion targetRotation = Quaternion.Euler(smoothPitch, smoothYaw, 0f);

        // Se a câmara for um objeto raiz na cena, usamos rotation. Se for filha de outro objeto, usamos localRotation.
        // Vamos garantir que aplicamos diretamente no transform alvo:
        targetTransform.localRotation = targetRotation;
    }

    void OnApplicationQuit()
    {
        isRunning = false;
        if (udpClient != null) udpClient.Close();
        if (receiveThread != null && receiveThread.IsAlive) receiveThread.Abort();
    }
}