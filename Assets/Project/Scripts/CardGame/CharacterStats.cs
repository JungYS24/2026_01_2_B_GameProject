using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CharacterStats : MonoBehaviour
{

    public string characterName;
    public int maxHealth = 100;
    public int currentHealth;

    public TextMeshProUGUI healthText;
    public Slider healthBar;

    void Start()
    {
        
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
       
    }
    public void Heal (int amount)
    {
        currentHealth += amount;
        
    }
}
