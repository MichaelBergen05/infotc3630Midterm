using UnityEngine;


public class UFOMovement : MonoBehaviour
{
    [Header("Patrol Bounds (World X)")]
    public float leftBound  = -10f;
    public float rightBound =  10f;

    [Header("Base Speed")]
    public float baseSpeed = 4f;

    [Header("Visual Tilt")]
    public float maxTiltDegrees = 15f;   // How far the UFO banks into turns
    public float tiltSmoothing  = 5f;

    private float _speedMultiplier = 1f;
    private float _direction = 1f;       // 1 = moving right, -1 = moving left

    void Update()
    {
        float speed = baseSpeed * _speedMultiplier;
        transform.position += Vector3.right * (_direction * speed * Time.deltaTime);

        // Reverse at bounds
        if (transform.position.x >= rightBound)
        {
            _direction = -1f;
            transform.position = new Vector3(rightBound, transform.position.y, transform.position.z);
        }
        else if (transform.position.x <= leftBound)
        {
            _direction = 1f;
            transform.position = new Vector3(leftBound, transform.position.y, transform.position.z);
        }

        // Smooth banking tilt
        float targetRoll = -_direction * maxTiltDegrees;
        float currentRoll = transform.eulerAngles.z > 180f
            ? transform.eulerAngles.z - 360f
            : transform.eulerAngles.z;
        float smoothedRoll = Mathf.Lerp(currentRoll, targetRoll, Time.deltaTime * tiltSmoothing);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, smoothedRoll);
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        _speedMultiplier = multiplier;
    }

    public void RandomizeStartDirection()
    {
        _direction = Random.value > 0.5f ? 1f : -1f;
    }
}
