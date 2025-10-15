using UnityEngine;
using Cinemachine;

public class Screenshake : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera; // Reference to the Cinemachine virtual camera
    private float shakeTimer; // Timer for the duration of the shake
    private float shakeIntensity; // Intensity of the shake
    private float shakeDuration; // Duration of the shake
    private CinemachineBasicMultiChannelPerlin noise; // Noise for the shake

    private void Awake()
    {
        // Get the CinemachineVirtualCamera and CinemachineBasicMultiChannelPerlin component
        if (virtualCamera == null)
        {
            virtualCamera = GetComponent<CinemachineVirtualCamera>();
        }

        noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    // Method to trigger the screen shake with intensity and duration parameters
    public void Shake(float intensity, float duration)
    {
        shakeIntensity = intensity;
        shakeDuration = duration;
        shakeTimer = duration;
    }

    private void Update()
    {
        if (shakeTimer > 0)
        {
            // Calculate the intensity of the noise based on the remaining time
            float normalizedTime = 1 - (shakeTimer / shakeDuration);
            float currentIntensity = Mathf.Lerp(shakeIntensity, 0f, normalizedTime);

            // Generate random noise for the shake
            Vector3 noiseVector = new Vector3(Random.Range(-1f, 1f) * currentIntensity, Random.Range(-1f, 1f) * currentIntensity, 0f);
            noise.m_AmplitudeGain = noiseVector.magnitude; // Assign magnitude of the noise vector to m_AmplitudeGain

            // Decrement the timer
            shakeTimer -= Time.deltaTime;
        }
        else
        {
            // Reset the noise when the shake is over
            noise.m_AmplitudeGain = 0f; // Set m_AmplitudeGain to 0
        }
    }
}