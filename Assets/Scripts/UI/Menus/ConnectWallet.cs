using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TitanCore.Net.Web;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using UnityEngine.UI;
public class ConnectWallet : MonoBehaviour
{
    // this is a two stage login process
    // the first stage of this process requests a message from the server
    // this message is then signed by the wallet and returned to the server
    // the server will send back validaiton if this is correct
    // saltFIATFIGHTERZ Apr 2022

    // unity links
    public GameObject next;
    public TMP_InputField emailField;
    public TMP_InputField passwordField;
    public TextMeshProUGUI errorLabel;
    public Toggle remembermeToggle;
    public LoginMenu loginmenu;
    public GameObject overlay;


    public static string walletAddress
    {
        get => PlayerPrefs.GetString("walletAddress", "");
        set => PlayerPrefs.SetString("walletAddress", value);
    }
    
    public void Start()
    {
        remembermeToggle.isOn = PlayerPrefs.GetInt("RememberMe", 0) == 1 ? true : false;

        string accessToken = Account.savedAccessToken;
        if (!string.IsNullOrWhiteSpace(accessToken) && remembermeToggle.isOn == true)
        {
            Debug.Log("REMEMBER ME LOGIN");
            overlay.SetActive(true);
            WebClient.SendMessageRequest(OnMessageResponse);
        }
    }

    public float timeoutTimer = 60;
    public void Update()
    {
        if (overlay.activeSelf)
        {
            timeoutTimer -= Time.deltaTime;
            if (timeoutTimer < 0)
            {
                overlay.SetActive(false);
            }
        }else
        {
            timeoutTimer = 60;
        }
    }
    private bool upentered;
    // called  from a button 
    public void Connect()
    {
        // blank login but they have one of the tokens (accesstoken TOT)
        if (string.IsNullOrWhiteSpace(emailField.text) || string.IsNullOrWhiteSpace(passwordField.text))
        {
            if (!string.IsNullOrWhiteSpace(Account.savedAccessToken))
            {
                Debug.Log("REMEMBER ME LOGIN");
                overlay.SetActive(true);
                upentered = false;
                WebClient.SendMessageRequest(OnMessageResponse);
                return;
            }
            errorLabel.text = "Email and/or Password is blank.";
            return;
        }
        else
        {
            upentered = true;
        }
        
        WebClient.SendMessageRequest(OnMessageResponse);

        overlay.SetActive(true);
    }

    public void RememberMeToggle()
    {
        if (remembermeToggle.isOn) { PlayerPrefs.SetInt("RememberMe", 0); } else { PlayerPrefs.SetInt("RememberMe", 1); }
        Debug.Log(PlayerPrefs.GetInt("RememberMe", 0) + " " + remembermeToggle.isOn);
    }

    private async void OnMessageResponse(WebClient.Response<WebMessageResponse> response)
    {
        // Debug.Log(response.item.message);
        
        if (response.item == null)
        {
            overlay.SetActive(false);
            if (PlayerPrefs.GetInt("RememberMe", 0) == 1)
            {
                ApplicationAlert.Show("AutoLogin Failed", "Unable to get response from the server", (button) =>
                {
                    if (button != 1) return;
                    Connect();
                }, "Back", "Retry");
            }else
            {
                ApplicationAlert.Show("Uh oh spagetio!", "Unable to get response from the server", (button) =>
                {
                    if (button != 1) return;
                    Connect();
                }, "Back", "Retry");
            }
            return;
        }
        if (response.item.result != WebMessageResult.Success || response.item == null)
        {
            overlay.SetActive(false);
            if (response.item.result == WebMessageResult.RateLimitReached)
            {
                ApplicationAlert.Show("Oops!", $"You're doing that too much! Try again in a short time.", null, "Ok");
            }else if (response.item.result == WebMessageResult.UpdateRequired)
            {
                ApplicationAlert.Show("Oops!", $"It looks like this version is out of date, Please update your client", null, "Ok");
            }else
            {
                ApplicationAlert.Show("lol!", $"Unknown error, maybe uninstall and reinstall", null, "Ok");
            }
            return;
        }

        var message = response.item.message;
        string signature = "";
        string tempWalletAddress = "";

        try
        {
            signature = await Web3Wallet.Sign(message);
            tempWalletAddress = await EVM.Verify(message, signature);
        }
        catch
        {
            overlay.SetActive(false);
            signature = "";
            tempWalletAddress = "";
            ApplicationAlert.Show("Error", "Message was not signed correctly", (button) =>
            {
                if (button != 1) return;
                Connect();
            }, "Back", "Retry");

            return;
        }


        //overlay.SetActive(false);


        PlayerPrefs.SetString("signedCryptoMessage", signature + "_FF_" + message);

        if (upentered)
        {
            Debug.Log("up login");
            WebClient.SendTwoStageVerificaitonRequest(message, signature, emailField.text, passwordField.text, OnSignedMessageResponseLogin);
        }
        else
        {
            WebClient.SendTwoStageVerificaitonTokenRequest(message, signature, Account.savedAccessToken, OnSignedMessageResponseDescribe);

        }
    }



    private void OnSignedMessageResponseLogin(WebClient.Response<WebLoginResponse> response)
    {
        Debug.Log("up login");

        loginmenu.setLoginResponse(response);
    }

    private void OnSignedMessageResponseDescribe(WebClient.Response<WebDescribeResponse> response)
    {
        loginmenu.setdescriberesponse(response);
    }
}
