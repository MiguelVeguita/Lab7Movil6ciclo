using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ClickerManager : MonoBehaviour
{
    private int clickCount = 0;
    public int level = 1;
    public int health = 100;
    public int strength = 10;
    private const int ClicksToLevelUp = 5;

    [Header("UI References")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text strengthText;
    [SerializeField] private TMP_Text clickInfoText;

    [Header("Authentication")]
    [SerializeField] private UnityPlayerAuth playerAuth;
    [SerializeField] private TMP_Text saveStatusText;

    void Start()
    {
        UpdateStatsUI();
    }

    public void OnScreenClicked()
    {
        clickCount++;

        if (clickCount >= ClicksToLevelUp)
        {
            LevelUp(); 
            clickCount = 0;
        }

        UpdateStatsUI();
    }

    private void LevelUp()
    {
        level++;
        health += 50;
        strength += 5;

        Debug.Log("¡Subiste de Nivel! Nuevo Nivel: " + level);

        SaveChanges(); 
    }

    public void UpdateStatsUI()
    {
        if (levelText) levelText.text = "Nivel: " + level;
        if (healthText) healthText.text = "Vida: " + health;
        if (strengthText) strengthText.text = "Fuerza: " + strength;
        if (clickInfoText) clickInfoText.text = "Clics: " + clickCount + "/" + ClicksToLevelUp;
    }


    public void LoadStats(PlayerData data)
    {
        level = data.level;
        health = data.health;
        strength = data.strength;

        Debug.Log("Datos del jugador cargados desde la nube.");
        UpdateStatsUI();
    }

    public async void SaveChanges()
    {
        var dataToSave = new PlayerData
        {
            level = this.level,
            health = this.health,
            strength = this.strength
        };

        bool success = await playerAuth.SavePlayerData(dataToSave);

        if (success && saveStatusText != null)
        {
            StartCoroutine(ShowSaveStatus());
        }
    }

    private System.Collections.IEnumerator ShowSaveStatus()
    {
        saveStatusText.text = "¡Progreso Guardado!";
        saveStatusText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2); 
        saveStatusText.gameObject.SetActive(false);
    }

}