using UnityEngine;
using UnityEngine.UI;

public class BossHealthBarUI : MonoBehaviour
{
    public float health, maxHealth, width, height;
    [SerializeField] private RectTransform hpBar;

    public void setMaxHealth(float maxHp)
    {
        maxHealth = maxHp;
    }

    public void setHealth(float hp)
    {
        health = hp;
        float newWidth = (health / maxHealth) * width;
        hpBar.sizeDelta = new Vector2(newWidth, height);
    }


}
