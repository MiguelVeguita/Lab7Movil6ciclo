using System;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;

public class UILogin : MonoBehaviour
{
    [SerializeField] private Transform loginPanel;
    [SerializeField] private Transform userPanel;

    [SerializeField] private Button loginButton;
    [SerializeField] private TMP_Text playerIDTxt;
    [SerializeField] private TMP_Text playerNameTxt;

    [SerializeField] private TMP_InputField UpdateNameIF;
    [SerializeField] private Button updateNameBtn;
    [SerializeField] private ClickerManager clickerManager;

    [SerializeField] private UnityPlayerAuth unityPlayerAuth;


    void Start()
    {
        loginPanel.gameObject.SetActive(true);
        userPanel.gameObject.SetActive(false);
    }
    private void OnEnable()
    {
        loginButton?.onClick.AddListener(LoginButton);
        updateNameBtn.onClick.AddListener(UpdateName);

        if (unityPlayerAuth != null)
        {
            unityPlayerAuth.OnSingedIn += UnityPlayerOnSignedIn;
            unityPlayerAuth.OnUpdateName += UpdateNameVisual;

            if (unityPlayerAuth.IsPlayerSignedIn)
            {
                Debug.Log("UI detected player is already signed in. Updating visuals.");
                UnityPlayerOnSignedIn(unityPlayerAuth.PlayerInfo, unityPlayerAuth.PlayerName);
            }
        }
    }
    private async void UpdateName()
    {
        if (string.IsNullOrEmpty(UpdateNameIF.text)) return; 

        await unityPlayerAuth.UpdateName(UpdateNameIF.text);

        clickerManager.SaveChanges(); 
    }
    private void UpdateNameVisual(string newName)
    {
        playerNameTxt.text = newName;
    }


    private async void UnityPlayerOnSignedIn(PlayerInfo playerInfo, string PlayerName)
    {
        Debug.Log("Login UI updated.");
        loginPanel.gameObject.SetActive(false);
        userPanel.gameObject.SetActive(true);

        playerIDTxt.text = "ID: " + playerInfo.Id;
        playerNameTxt.text = PlayerName; 

        PlayerData loadedData = await unityPlayerAuth.LoadPlayerData();

        if (loadedData != null)
        {
            Debug.Log("Datos guardados encontrados, actualizando UI.");

            if (!string.IsNullOrEmpty(loadedData.playerName))
            {
                playerNameTxt.text = loadedData.playerName;
            }

            clickerManager.LoadStats(loadedData);
        }
        else
        {
            Debug.Log("No se encontraron datos guardados, usando valores por defecto.");
            clickerManager.UpdateStatsUI();
        }
    }

    private async void LoginButton()
    {
        await unityPlayerAuth.InitSignIn();
    }

    private void OnDisable()
    {
        loginButton?.onClick.RemoveListener(LoginButton);
        unityPlayerAuth.OnSingedIn -= UnityPlayerOnSignedIn;

        updateNameBtn.onClick.RemoveListener(UpdateName);
        unityPlayerAuth.OnUpdateName -= UpdateNameVisual;
    }
}