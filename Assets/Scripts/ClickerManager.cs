using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Sirenix.OdinInspector;
public class ClickerManager : MonoBehaviour
{
    public int level = 1;
    public int health = 100;
    public int strength = 10;
    public int skillPoints = 0; 

    private int clickCount = 0;
    private const int ClicksToLevelUp = 5;

    [Header("UI References")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text strengthText;
    [SerializeField] private TMP_Text clickInfoText;
    [SerializeField] private TMP_Text skillPointsText; 
    [SerializeField] private Button increaseHealthButton; 
    [SerializeField] private Button increaseStrengthButton; 


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
        skillPoints++; 

        Debug.Log("¡Subiste de Nivel! Nuevo Nivel: " + level + ". Puntos de habilidad: " + skillPoints);

        SaveChanges();
    }

    public void IncreaseHealth()
    {
        if (skillPoints > 0)
        {
            skillPoints--;
            health += 50; 
            UpdateStatsUI();
            SaveChanges();
        }
    }

    public void IncreaseStrength()
    {
        if (skillPoints > 0)
        {
            skillPoints--;
            strength += 5; 
            UpdateStatsUI();
            SaveChanges();
        }
    }

    public void UpdateStatsUI()
    {
        if (levelText) levelText.text = "Nivel: " + level;
        if (healthText) healthText.text = "Vida: " + health;
        if (strengthText) strengthText.text = "Fuerza: " + strength;
        if (clickInfoText) clickInfoText.text = "Clics: " + clickCount + "/" + ClicksToLevelUp;
        if (skillPointsText) skillPointsText.text = "Puntos: " + skillPoints;

        bool canUpgrade = skillPoints > 0;
        if (increaseHealthButton) increaseHealthButton.gameObject.SetActive(canUpgrade);
        if (increaseStrengthButton) increaseStrengthButton.gameObject.SetActive(canUpgrade);
    }
    [Button]

    public void LoadStats(PlayerData data)
    {
        //playerNam¡?
        level = data.level;
        health = data.health;
        strength = data.strength;
        skillPoints = data.skillPoints; 

        Debug.Log("Datos del jugador cargados desde la nube.");
        UpdateStatsUI();
    }
    [Button]

    public async void SaveChanges()
    {
        var dataToSave = new PlayerData
        {
            playerName=this.name,
            level = this.level,
            health = this.health,
            strength = this.strength,
            skillPoints = this.skillPoints
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