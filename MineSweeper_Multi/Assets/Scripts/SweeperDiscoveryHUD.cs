using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Mirror.Discovery;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(NetworkDiscovery))]
public class SweeperDiscoveryHUD : MonoBehaviour
{
    private readonly Dictionary<long, ServerResponse> _discoveredServers = new();

    public NetworkDiscovery _networkDiscovery;

    [SerializeField] private GameObject _connectionButton;
    [SerializeField] private GameObject[] _uiElements;
    [SerializeField] private Transform _scrollContent;

    private readonly string[] _buttonNames = { "find", "host", "stop_host", "stop_client" };

    private readonly Vector2 _startingConnectionPos = new(91.5f, -23f);
    // space between UI connection buttons
    private const float CONNECTION_DIFFERENCE = 40f;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_networkDiscovery == null)
        {
            _networkDiscovery = GetComponent<NetworkDiscovery>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(_networkDiscovery.OnServerFound, OnDiscoveredServer);
            UnityEditor.Undo.RecordObjects(new Object[] { this, _networkDiscovery }, "Set NetworkDiscovery");
        }
    }
#endif

    private void OnGUI()
    {
        if (!NetworkManager.singleton) return;

        // not connected
        if (!NetworkClient.isConnected && !NetworkServer.active && !NetworkClient.active)
        {
            _uiElements[0].SetActive(true);
            _uiElements[1].SetActive(true);
            _uiElements[2].SetActive(false);
            _uiElements[3].SetActive(false);
            _uiElements[4].SetActive(true);
        }

        // connected
        if (NetworkServer.active || NetworkClient.active)
        {
            _uiElements[0].SetActive(false);
            _uiElements[1].SetActive(false);

            if (NetworkServer.active && NetworkClient.isConnected) // host disconnect
                _uiElements[2].SetActive(true);
            else if (NetworkClient.isConnected)                    // client disconnect
                _uiElements[3].SetActive(true);

            _uiElements[4].SetActive(false);
        }
    }

    private void OnDiscoveredServer(ServerResponse info)
    {
        if (NetworkServer.active || NetworkClient.active) return;

        foreach (Transform child in _scrollContent)
            Destroy(child.gameObject);

        _discoveredServers[info.serverId] = info;

        Vector2 btnPos = _startingConnectionPos;
        foreach (ServerResponse response in _discoveredServers.Values)
        {
            GameObject newBtn = Instantiate(_connectionButton);
            newBtn.transform.SetParent(_scrollContent, false);
            newBtn.GetComponent<RectTransform>().localPosition = btnPos;

            newBtn.GetComponentInChildren<TextMeshProUGUI>().SetText(info.EndPoint.Address.ToString());
            newBtn.GetComponent<Button>().onClick.AddListener(() => ConnectionPressed(response));

            btnPos = new Vector2(0f, btnPos.y - CONNECTION_DIFFERENCE);
        }
    }

    public void ConnectionPressed(ServerResponse reponse)
    {
        _networkDiscovery.StopDiscovery();
        NetworkManager.singleton.StartClient(reponse.uri);
    }

    public void ButtonPressed(string name)
    {
        if (name.Equals(_buttonNames[0]))
        {
            _discoveredServers.Clear();
            _networkDiscovery.StartDiscovery();
        }
        else if (name.Equals(_buttonNames[1]))
        {
            _discoveredServers.Clear();
            NetworkManager.singleton.StartHost();
            _networkDiscovery.AdvertiseServer();
        }
        else if (name.Equals(_buttonNames[2]))
        {
            NetworkManager.singleton.StopHost();
            _networkDiscovery.StopDiscovery();
        }
        else if (name.Equals(_buttonNames[3]))
        {
            NetworkManager.singleton.StopClient();
            _networkDiscovery.StopDiscovery();
        }
    }
}
