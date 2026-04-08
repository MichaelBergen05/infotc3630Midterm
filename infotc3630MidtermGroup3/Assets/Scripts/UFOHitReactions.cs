using UnityEngine;
using System.Collections;


public class UFOHitReactions : MonoBehaviour
{
    [Header("Hit Shake")]
    public float shakeDuration  = 0.35f;   // How long the shake lasts
    public float shakeAngle     = 22f;      // Max tilt degrees side to side
    public float shakeSpeed     = 18f;      // Oscillations per second

    [Header("Death Fall")]
    public float fallDuration   = 1.1f;    // How long the fall takes before despawn
    public float fallGravity    = 18f;     // Downward acceleration (fake gravity, not Rigidbody)
    public float spinSpeed      = 280f;    // Degrees per second on the way down
    public float fallTiltAngle  = 35f;     // Forward tilt as it falls, sells the nose-dive

    // Cached
    private UFOMovement _movement;
    private Coroutine   _shakeRoutine;
    private Coroutine   _deathRoutine;
    private bool        _dying;

    void Awake()
    {
        _movement = GetComponent<UFOMovement>();
    }


    public void TriggerHit()
    {
        if (_dying) return;

        // Cancel any running shake and restart it so rapid hits feel responsive
        if (_shakeRoutine != null)
            StopCoroutine(_shakeRoutine);

        _shakeRoutine = StartCoroutine(ShakeRoutine());
    }

    public void TriggerDeath()
    {
        if (_dying) return;
        _dying = true;

        if (_shakeRoutine != null)
        {
            StopCoroutine(_shakeRoutine);
            _shakeRoutine = null;
        }

        _deathRoutine = StartCoroutine(DeathFallRoutine());
    }


    private IEnumerator ShakeRoutine()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;

            // Oscillate on Z axis (roll), fading out toward the end
            float fadeOut  = 1f - (elapsed / shakeDuration);
            float rollAngle = Mathf.Sin(elapsed * shakeSpeed * Mathf.PI * 2f) * shakeAngle * fadeOut;

            // Preserve existing euler angles, only override Z roll
            Vector3 euler = transform.eulerAngles;
            euler.z = rollAngle;
            transform.eulerAngles = euler;

            yield return null;
        }

        // Snap Z back to zero cleanly
        Vector3 final = transform.eulerAngles;
        final.z = 0f;
        transform.eulerAngles = final;

        _shakeRoutine = null;
    }

    private IEnumerator DeathFallRoutine()
    {
        // Disable patrol movement so we own the transform fully
        if (_movement != null)
            _movement.enabled = false;

        float elapsed      = 0f;
        float verticalVel  = 0f;
        Vector3 startPos   = transform.position;

        while (elapsed < fallDuration)
        {
            elapsed     += Time.deltaTime;
            float t      = elapsed / fallDuration;

            // Accelerating downward fall
            verticalVel += fallGravity * Time.deltaTime;
            transform.position -= new Vector3(0f, verticalVel * Time.deltaTime, 0f);

            // Spinning on Y (tumble)
            transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);

            // Progressive nose-tilt forward on X
            Vector3 euler = transform.eulerAngles;
            euler.x = Mathf.LerpAngle(euler.x, fallTiltAngle, Time.deltaTime * 6f);
            euler.z = Mathf.LerpAngle(euler.z, 0f, Time.deltaTime * 8f); // clear any shake roll
            transform.eulerAngles = euler;

            yield return null;
        }

        // UFOTarget.Die() already called SetActive(false) — but if it hasn't
        // yet (timing edge case), do it here as a fallback
        gameObject.SetActive(false);
    }
}
