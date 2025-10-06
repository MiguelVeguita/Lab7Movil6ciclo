// UnityPlayerAuth.cs (versión modificada)
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using System.Threading.Tasks;
using System;
using Unity.Services.Authentication.PlayerAccounts;
using Unity.Services.CloudSave;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class UnityPlayerAuth : MonoBehaviour
{
    public event Action<PlayerInfo, string> OnSingedIn;
    public event Action<String> OnUpdateName;
    public PlayerInfo PlayerInfo { get; private set; }
    public string PlayerName { get; private set; }
    public bool IsPlayerSignedIn => AuthenticationService.Instance.IsSignedIn;

    private const string PlayerProfileKey = "playerProfile"; 


    private async void Start()
    {
        await UnityServices.InitializeAsync();
        SetupEvents();
        PlayerAccountService.Instance.SignedIn += SignIn;

        if (AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("Player already signed in. Fetching info...");
            PlayerInfo = AuthenticationService.Instance.PlayerInfo;
            PlayerName = await AuthenticationService.Instance.GetPlayerNameAsync();
            OnSingedIn?.Invoke(PlayerInfo, PlayerName);
        }
    }
    private void SetupEvents()
    {
        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Player ID " + AuthenticationService.Instance.PlayerId);
        };
        AuthenticationService.Instance.SignInFailed += (err) => { Debug.Log(err); };
        AuthenticationService.Instance.SignedOut += () => { Debug.Log("Player log out"); };
        AuthenticationService.Instance.Expired += () => { Debug.Log("Player session expired"); };
    }
    public async Task InitSignIn()
    {
        await PlayerAccountService.Instance.StartSignInAsync();
    }
    private async void SignIn()
    {
        try
        {
            await SignInWithUnityAuth();
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
        }
    }

    private async Task SignInWithUnityAuth()
    {
        if (AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("Sign-in skipped: Already signed in. Refreshing player data...");

            PlayerInfo = AuthenticationService.Instance.PlayerInfo;
            PlayerName = await AuthenticationService.Instance.GetPlayerNameAsync();

            OnSingedIn?.Invoke(PlayerInfo, PlayerName);
            return;
        }
        try
        {
            string accessToken = PlayerAccountService.Instance.AccessToken;
            await AuthenticationService.Instance.SignInWithUnityAsync(accessToken);
            Debug.Log("Login Succ");

            PlayerInfo = AuthenticationService.Instance.PlayerInfo;
            PlayerName = await AuthenticationService.Instance.GetPlayerNameAsync();

            OnSingedIn?.Invoke(PlayerInfo, PlayerName);
            Debug.Log("Sign In Successful ");
        }
        catch (AuthenticationException ex) { Debug.LogException(ex); }
        catch (RequestFailedException ex) { Debug.Log(ex); }
    }
    public async Task UpdateName(string newName)
    {
        await AuthenticationService.Instance.UpdatePlayerNameAsync(newName);
        var name = await AuthenticationService.Instance.GetPlayerNameAsync();

        OnUpdateName?.Invoke(name);
    }

    [Button]
    public async Task<bool> SavePlayerData(PlayerData data)
    {
        try
        {
            data.playerName = this.PlayerName;
            string jsonData = JsonUtility.ToJson(data);
            var dataToSave = new Dictionary<string, object> { { PlayerProfileKey, jsonData } };
            await CloudSaveService.Instance.Data.Player.SaveAsync(dataToSave);
            Debug.Log($"Datos (con nombre: {data.playerName}) guardados en la nube como JSON.");
            return true; 
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save data: {e}");
            return false; 
        }
    }
    [Button]
    public async Task<PlayerData> LoadPlayerData()
    {
        var keysToLoad = new HashSet<string> { PlayerProfileKey };

        var serverData = await CloudSaveService.Instance.Data.Player.LoadAsync(keysToLoad);

        if (serverData.TryGetValue(PlayerProfileKey, out var value))
        {
            string jsonData = value.Value.GetAsString();
            return JsonUtility.FromJson<PlayerData>(jsonData);
        }

        return null;
    }

    [Button]
    public async void DeleteData(string key)
    {
        await CloudSaveService.Instance.Data.Player.DeleteAsync(key);
    }
}