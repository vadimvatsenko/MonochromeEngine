namespace MonochromeEngine;

public class HealthComponent
{
    public float Health { get; private set; }
    public float MaxHealth { get; private set; }

    public HealthComponent(float health, float maxHealth)
    {
        Health = health;
        MaxHealth = maxHealth;
    }

    public void Heal(float amount)
    {
        Health += amount;
        if (Health > MaxHealth)
        {
            Health = MaxHealth;
        }
    }

    public void TakeDamage(float amount)
    {
        Health -= amount;
        if (Health <= 0)
        {
            Health = 0;
        }
    }
}