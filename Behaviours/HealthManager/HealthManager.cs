using Godot;

public partial class HealthManager : Node
{
    [Export] public float MaxHealth { get; set; } = 100f;
    
    [Export] public float CurrentHealth { get; private set; } = 100f;

    [Export] public bool Invincible { get; set; } = false;

    // Emitted when health reaches 0
    [Signal]
    public delegate void DiedEventHandler();

    // Emitted any time health changes (useful for UI, sounds, etc.)
    [Signal]
    public delegate void HealthChangedEventHandler(float newHealth, float oldHealth, float maxHealth);

    public override void _Ready()
    {
        // Make sure we start with valid values
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, MaxHealth);
        
        // Optional: emit initial state so UI can sync immediately
        EmitSignal(SignalName.HealthChanged, CurrentHealth, CurrentHealth, MaxHealth);
    }

    /// <summary>
    /// Apply damage. Returns true if the entity died from this hit.
    /// </summary>
    public bool TakeDamage(float amount)
    {
        if (amount <= 0) return false;
        if (Invincible) return false;

        float oldHealth = CurrentHealth;
        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);

        EmitSignal(SignalName.HealthChanged, CurrentHealth, oldHealth, MaxHealth);

        if (CurrentHealth <= 0f)
        {
            EmitSignal(SignalName.Died);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Heal the entity. Clamped to max health.
    /// </summary>
    public void Heal(float amount)
    {
        if (amount <= 0) return;

        float oldHealth = CurrentHealth;
        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);

        EmitSignal(SignalName.HealthChanged, CurrentHealth, oldHealth, MaxHealth);
    }

    /// <summary>
    /// Instantly set health to a specific value (clamped)
    /// </summary>
    public void SetHealth(float value)
    {
        float oldHealth = CurrentHealth;
        CurrentHealth = Mathf.Clamp(value, 0f, MaxHealth);

        EmitSignal(SignalName.HealthChanged, CurrentHealth, oldHealth, MaxHealth);

        if (CurrentHealth <= 0f && oldHealth > 0f)
        {
            EmitSignal(SignalName.Died);
        }
    }

    /// <summary>
    /// Full heal to maximum health
    /// </summary>
    public void FullHeal()
    {
        SetHealth(MaxHealth);
    }

    /// <summary>
    /// Check if the entity is considered dead
    /// </summary>
    public bool IsDead => CurrentHealth <= 0f;

    /// <summary>
    /// Percentage of health remaining (0.0–1.0)
    /// </summary>
    public float HealthPercentage => MaxHealth > 0 ? CurrentHealth / MaxHealth : 0f;
}