using UnityEngine;
using UnityEngine.UI;

public class npchealth : MonoBehaviour
{
    public float maxHealth = 100f;
    float currentHealth;

    public Image healthFill;
    Transform cam;

    void Start()
    {
        currentHealth = maxHealth;
        cam = Camera.main.transform;
    }

    void Update()
    {
        if (healthFill != null)
        {
            healthFill.transform.parent.LookAt(cam);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthFill != null)
        {
            healthFill.fillAmount = currentHealth / maxHealth;
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
