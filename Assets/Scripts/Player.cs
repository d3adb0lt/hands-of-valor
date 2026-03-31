using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private int maxHp = 5;
    [SerializeField] private int maxStamina = 5;
    [SerializeField] private BarUI hpBar;
    [SerializeField] private BarUI staminaBar;

    public int Hp { get; private set; }
    public int Stamina { get; private set; }
    public int MaxHp { get; private set; }

    private void Awake()
    {
        ResetStats();
    }

    public void ResetStats()
    {
        Hp = maxHp;
        Stamina = maxStamina;
        MaxHp = maxHp;
        hpBar.SetMaxValue(maxHp);
        staminaBar.SetMaxValue(maxStamina);
        Debug.Log($"Player HP = {Hp} / {maxHp}, Stamina = {Stamina} / {maxStamina}");
    }

    public void ResolveBossAttack(bool dodgedInTime)
    {
        if (Hp <= 0) return;

        if (Stamina <= 0)
        {
            TakeDamage(1);
            return;
        }

        if (dodgedInTime)
        {
            UseStamina(1);
        }
        else
        {
            TakeDamage(1);
        }
    }

    private void UseStamina(int amount)
    {
        Stamina = Mathf.Max(0, Stamina - amount);
        staminaBar.SetValue(Stamina);
        Debug.Log($"Player stamina = {Stamina} / {maxStamina}");
    }

    private void TakeDamage(int amount)
    {
        Hp = Mathf.Max(0, Hp - amount);
        hpBar.SetValue(Hp);
        Debug.Log($"Player HP = {Hp} / {maxHp}");

        if (Hp <= 0)
        {
            Debug.Log("Player defeated");
            if (LevelManager.Instance != null) LevelManager.Instance.StopBossFight(false);
        }
    }

    public void ResolveParry()
    {
        if (Hp <= 0) return;

        if (Stamina <= 0)
        {
            TakeDamage(1);
        }
        else
        {
            UseStamina(1);
        }
    }
}