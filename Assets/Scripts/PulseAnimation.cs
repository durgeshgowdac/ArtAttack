using UnityEngine;

public class PulseAnimation : MonoBehaviour
{
    public float scaleAmount = 1.1f;
    public float pulseSpeed = 2f;
    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        float scale = 1 + Mathf.Sin(Time.time * pulseSpeed) * (scaleAmount - 1);
        transform.localScale = originalScale * scale;
    }
}