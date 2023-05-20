// Note: this script has to be on an always-active UI parent, so that we can
// always find it from other code. (GameObject.Find doesn't find inactive ones)
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Collections.Generic;

public partial class UILogin : MonoBehaviour
{
    public UIPopup uiPopup;
    public NetworkManagerMMO manager; // singleton=null in Start/Awake
    public NetworkAuthenticatorMMO auth;
    public GameObject panel;
    public Text statusText;
    public InputField accountInput;
    public InputField passwordInput;
    public Dropdown serverDropdown;
    public Button loginButton;
    public Button registerButton;
    [TextArea(1, 30)] public string registerMessage = "First time? Just log in and we will\ncreate an account automatically.";
    public Button hostButton;
    public Button dedicatedButton;
    public Button cancelButton;
    public Button quitButton;

    void Start()
    {
    }

    void OnDestroy()
    {
    }

    void Update()
    {
        // only show while offline
        // AND while in handshake since we don't want to show nothing while
        // trying to login and waiting for the server's response
        if (manager.state == NetworkState.Offline || manager.state == NetworkState.Handshake)
        {
            panel.SetActive(true);

            // status
            if (NetworkClient.isConnecting)
                statusText.text = "Connecting...";
            else if (manager.state == NetworkState.Handshake)
                statusText.text = "Handshake...";
            else
                statusText.text = "";

            if (!manager.isNetworkActive)
            {
                manager.networkAddress = manager.serverList[0].ip;
#if UNITY_SERVER && !UNITY_EDITOR
                manager.StartServer();
#elif UNITY_EDITOR
                auth.loginAccount = Random.Range(0, 999999).ToString(); // "UNITY_EDITORd";
                auth.loginPassword = "22021997";
                manager.StartHost();
#else
                auth.loginAccount = Random.Range(0,999999).ToString();
                auth.loginPassword = "22021997";
                //manager.StartHost();
                manager.StartClient();
#endif
            }
        }
        else
        {
            panel.SetActive(false);

        }
    }
}
