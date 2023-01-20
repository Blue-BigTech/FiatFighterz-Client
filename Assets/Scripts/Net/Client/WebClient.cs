using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Xml.Serialization;
using TitanCore.Core;
using TitanCore.Net;
using TitanCore.Net.Web;
using UnityEngine;
using System.Runtime.InteropServices;

public static class WebClient
{
    public class Response<T>
    {
        public Exception exception;

        public T item;

        public Response(Exception exception)
        {
            this.exception = exception;
            item = default;
        }

        public Response(T item)
        {
            this.item = item;
            exception = null;
        }
    }
    private static string Web_Server_Url = "http://ec2-54-196-46-239.compute-1.amazonaws.com/";
    private static string Debug_Web_Server_Url = "http://localhost/";

    private static HttpClient client = new HttpClient()
    {
        Timeout = TimeSpan.FromSeconds(15)
    };

    // this should make the string encoded in the way that the post request asks for ????
    private static string MakeQueryString(Dictionary<string, string> query)
    {
        var builder = new StringBuilder();
        foreach (var pair in query)
        {
            if (builder.Length > 0)
                builder.Append('&');
            builder.Append(pair.Key);
            builder.Append('=');
            builder.Append(pair.Value);
        }
        return builder.ToString();
    }


    private static async void SendRequest<T>(string path, Dictionary<string, string> query, Action<Response<T>> resultCallback)
    {
        T result = default;
        try
        {
            string url;
            if (Debug.isDebugBuild || Application.isEditor)
            {
                url = Debug_Web_Server_Url + path;
            }
            else
            {
                url = Web_Server_Url + path;
            }

            var content = new FormUrlEncodedContent(query);
            var response = await client.PostAsync(url, content);
            var ser = new XmlSerializer(typeof(T));
            result = (T)ser.Deserialize(await response.Content.ReadAsStreamAsync());
        }
        catch (Exception e)
        {
            resultCallback(new Response<T>(e));
            return;
        }

        resultCallback(new Response<T>(result));
    }

    public static void SendForgotPassword(string email, Action<Response<WebLoginResponse>> callback)
    {
        SendRequest("v1/account/forgot", new Dictionary<string, string>()
        {
            { "email", Client.RsaEncrypt(email) },
        }, callback);
    }

    public static void SendWebLogin(string email, string hash, string cryptoAddress, Action<Response<WebLoginResponse>> callback)
    {
        SendRequest("v1/account/login", new Dictionary<string, string>()
        {
            { "email", Client.RsaEncrypt(email) },
            { "hash", Client.RsaEncrypt(hash) },
            { "cryptoAddress", Client.RsaEncrypt(cryptoAddress)}
        }, callback);
    }

    public static void SendWebRegister(string email, string hash, string message, string signedmessage,  Action<Response<WebRegisterResponse>> callback)
    {
        Debug.Log(email);
        string encryptedText = Client.RsaEncrypt(email);
        Debug.Log("email: " + encryptedText);

        SendRequest("v1/account/register", new Dictionary<string, string>()
        {
            { "email", Client.RsaEncrypt(email) },
            { "hash", Client.RsaEncrypt(hash) },
            { "message", Client.RsaEncrypt(message) },
            { "signedmessage", Client.RsaEncrypt(signedmessage)}
        }, callback);
    }

    public static void SendWebDescribe(string accessToken, Action<Response<WebDescribeResponse>> callback)
    {
        SendRequest("v1/account/describe", new Dictionary<string, string>()
        {
            { "token", Client.RsaEncrypt(accessToken) },
            { "version", NetConstants.Build_Version },
        }, callback);
    }

    public static void SendPurchaseSlot(string accessToken, Action<Response<WebPurchaseSlotResponse>> callback)
    {
        SendRequest("v1/account/purchaseslot", new Dictionary<string, string>()
        {
            { "token", Client.RsaEncrypt(accessToken) },
        }, callback);
    }

    public static void SendChangeName(string accessToken, string fromName, string toName, string reservation, Action<Response<WebNameChangeResponse>> callback)
    {
        var dict = new Dictionary<string, string>()
        {
            { "token", Client.RsaEncrypt(accessToken) },
            { "fromName", fromName },
            { "toName", toName }
        };
        if (!string.IsNullOrEmpty(reservation))
            dict.Add("reservation", Client.RsaEncrypt(reservation));

        SendRequest("v1/account/changename", dict, callback);
    }

    public static void SendLeaderboardDescribe(LeaderboardType type, Action<Response<WebLeaderboardResponse>> callback)
    {
        var dict = new Dictionary<string, string>()
        {
            { "type", ((int)type).ToString() }
        };

        SendRequest("v1/leaderboard/describe", dict, callback);
    }

    public static void SendServerList(Action<Response<WebServerListResponse>> callback)
    {
        var dict = new Dictionary<string, string>()
        {
            { "version", NetConstants.Build_Version },
        };

        SendRequest("v1/server/list", dict, callback);
    }

    public static void SendDiscordPurchaseVerify(string id, Action<Response<WebVerifyResponse>> callback)
    {
        var dict = new Dictionary<string, string>()
        {
            { "id", id },
            { "accountId", Account.describe.accountId.ToString() }
        };

        SendRequest("v1/purchase/discord/verify", dict, callback);
    }

    public static void SendiOSPurchaseVerify(string receipt, Action<Response<WebVerifyResponse>> callback)
    {
        var dict = new Dictionary<string, string>()
        {
            { "receipt", receipt },
            { "accountId", Account.describe.accountId.ToString() }
        };

        SendRequest("v1/purchase/ios/verify", dict, callback);
    }

    public static void SendAndroidPurchaseVerify(string token, string productId, Action<Response<WebVerifyResponse>> callback)
    {
        var dict = new Dictionary<string, string>()
        {
            { "token", token },
            { "productId", productId },
            { "accountId", Account.describe.accountId.ToString() }
        };

        SendRequest("v1/purchase/android/verify", dict, callback);
    }

    public static void SendSteamPurchaseStart(string steamId, string languageCode, string productId, Action<Response<WebSteamInitTxnResponse>> callback)
    {
        var dict = new Dictionary<string, string>()
        {
            { "steamId", steamId },
            { "lan", languageCode },
            { "productId", productId },
            { "accountId", Account.describe.accountId.ToString() }
        };

        SendRequest("v1/purchase/steam/start", dict, callback);
    }

    public static void SendSteamPurchaseVerify(string orderId, Action<Response<WebVerifyResponse>> callback)
    {
        var dict = new Dictionary<string, string>()
        {
            { "orderId", orderId },
            { "accountId", Account.describe.accountId.ToString() }
        };

        SendRequest("v1/purchase/steam/verify", dict, callback);
    }

    public static void SendGetOwnedNFTs(string accessToken, Action<Response<WebDescribeResponse>> callback)
    {
        SendRequest("v1/account/getownednfts", new Dictionary<string, string>()
        {
            { "token", Client.RsaEncrypt(accessToken) },
            { "signedMessage", Client.RsaEncrypt(PlayerPrefs.GetString("signedCryptoMessage", "")) },
            { "accountId", Client.RsaEncrypt(Account.describe.accountId.ToString())}
        }, callback);
    }

    public static void SendSellItemRequest(string accessToken, string accountId, string characterId, string itemId, Action<Response<WebSellItemResponse>> callback)
    {
        SendRequest("v1/account/sellitem", new Dictionary<string, string>()
        {
            { "token", Client.RsaEncrypt(accessToken) },
            { "accountId", Client.RsaEncrypt(Account.describe.accountId.ToString()) },
            { "characterId", Client.RsaEncrypt(characterId)},
            { "itemId", Client.RsaEncrypt(itemId)}
        }, callback);
    }

    public static void SendMessageRequest(Action<Response<WebMessageResponse>> callback)
    {
        SendRequest("v1/account/generatemessage", new Dictionary<string, string>()
        {  }, callback);
    }
    /*
    public static void SendSignedMessageRequest(string originMessage, string signedMessage, Action<Response<WebSignedMessageResponse>> callback)
    {
        SendRequest("v1/account/verifymessage", new Dictionary<string, string>()
        {
            { "originMessage", Client.RsaEncrypt(originMessage) },
            { "signedMessage", Client.RsaEncrypt(signedMessage) }
        }, callback);
    }*/

    // Two stage verification a username and password
    public static void SendTwoStageVerificaitonRequest(string originMessage, string signedMessage, string email, string hash, Action<Response<WebLoginResponse>> callback)
    {
        // string encryptedSignedMessage = Client.RsaEncrypt(signedMessage);
        
        SendRequest("v1/account/tswlogin", new Dictionary<string, string>()
        {
            { "originMessage", Client.RsaEncrypt(originMessage) },
            { "signedMessage", Client.RsaEncrypt(signedMessage) },
            { "email", Client.RsaEncrypt(email) },
            { "hash", Client.RsaEncrypt(hash) },

        }, callback);
    }

    // Two Stage Verification with a verificaiton token
    public static void SendTwoStageVerificaitonTokenRequest(string originMessage, string signedMessage, string token, Action<Response<WebDescribeResponse>> callback)
    {
        SendRequest("v1/account/tswdescribe", new Dictionary<string, string>()
        {
            { "originMessage", Client.RsaEncrypt(originMessage) },
            { "signedMessage", Client.RsaEncrypt(signedMessage) },
            { "token", Client.RsaEncrypt(token) },
            { "version", NetConstants.Build_Version }

        }, callback);
    }

}
