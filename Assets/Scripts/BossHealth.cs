using UnityEngine;

public class BossHealth : MonoBehaviour
{
    [SerializeField] private int maxHp = 10;
    
    public int CurrentHp { get; private set; }

    private void Awake()
    {
        CurrentHp = maxHp;
        Debug.Log("Boss HP = " + CurrentHp + " / " + maxHp);
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;
        if (CurrentHp <= 0) return;

        CurrentHp -= amount;
        if (CurrentHp < 0) CurrentHp = 0;

        Debug.Log("Damaged boss. HP = " + CurrentHp + " / " + maxHp);

        if (CurrentHp == 0)
        {
            LevelManager.Instance.StopBossFight(wonBattle: true);
            Debug.Log("Boss defeated.");
        }
    }
}