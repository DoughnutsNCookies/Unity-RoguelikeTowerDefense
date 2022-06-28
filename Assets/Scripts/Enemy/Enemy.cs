using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    // public GameObject deathEffect;
    public float startSpeed = 10f;

    [HideInInspector] public float speed;
    public float startHealth = 100;
    private float health;
    public int value = 50;

    // [Header("Unity Stuff")]
    // public Image healthBar;

    void Start()
    {
        speed = startSpeed;
        health = startHealth;
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        // healthBar.fillAmount = health / startHealth;
        if (health <= 0f)
        {
            Die();
        }
    }

    public void Slow(float slowAmount)
    {
        speed = startSpeed * (1f - slowAmount);
    }

    void Die()
    {
        // PlayerStats.Money += value;
        // WaveSpawner.enemiesAlive--;

        // GameObject effect = Instantiate(deathEffect, transform.position, Quaternion.identity);
        // Destroy(effect, 5f);
        Destroy(gameObject);
    }
}
