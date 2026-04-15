using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class CharacterStats : MonoBehaviour
{

    public string characterName;
    public int maxHealth = 100;
    public int currentHealth;

    public TextMeshProUGUI healthText;
    public Slider healthBar;

    //새로 추가되는 마나 변수
    public int maxMana = 10;
    public int currentMana;
    public TextMeshProUGUI manaText;
    public Slider manaBar;

    void Start()
    {
        currentHealth = maxHealth;
        currentMana = maxMana;
        UpdateUI();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

    }
    public void Heal(int amount)
    {
        currentHealth += amount;

    }

    public void UseMana(int amount)
    {
        currentMana -= amount;
        if (currentMana < 0)
        {
            currentMana = 0;
        }
        UpdateUI();
    }

    public void GainMana(int amount) 
    {
        currentMana += amount;
        if (currentMana > maxMana)
        {
            currentMana = maxMana;
        }
        UpdateUI();
    }

    private void UpdateUI()
    {
        throw new NotImplementedException();
    }

    private void Update()
    {
        {
            if (healthBar != null)
            {
                healthBar.value = (float)currentHealth / maxHealth;
            }
            if (healthText != null)
            {
                healthText.text = $"{currentHealth} / {maxHealth}";
            }
            if (manaText != null)
            {
                manaBar.value = (float)currentMana / maxMana;
            }
            if (manaText != null)
            {
                manaText.text = $"{currentMana} / {maxMana}";
            }
        }
    }
}
