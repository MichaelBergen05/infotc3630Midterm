using UnityEngine;
using System;


public class UFOTarget : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 3;

    [Header("Effects")]
    public AudioClip hitSound;
    public AudioClip deathSound;

    public static event Action<UFOTarget> OnUFODestroyed;

    private int _currentHealth;
    private AudioSource _audio;
    private UFOHitReactions _reactions;
    private bool _dead;

    // The lane index this UFO belongs to (set by manager on spawn)
    [HideInInspector] public int laneIndex;

    void Awake()
    {
        _audio = GetComponent<AudioSource>();
        if (_audio == null)
            _audio = gameObject.AddComponent<AudioSource>();
        _reactions = GetComponent<UFOHitReactions>();
    }

    void OnEnable()
    {
        _currentHealth = maxHealth;
        _dead = false;
    }


    public void Damage(int amount)
    {
        if (_dead) return;

        _currentHealth -= amount;

        if (hitSound != null)
            _audio.PlayOneShot(hitSound);

        _reactions?.TriggerHit();

        if (_currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        _dead = true;

        if (deathSound != null)
            _audio.PlayOneShot(deathSound);

        // Notify the manager
        OnUFODestroyed?.Invoke(this);

        if (_reactions != null)
            _reactions.TriggerDeath();
        else
            gameObject.SetActive(false);
    }
}
