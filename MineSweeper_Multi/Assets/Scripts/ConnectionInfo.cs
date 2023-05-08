using UnityEngine;
using Mirror;
using TMPro;

[DisallowMultipleComponent]
public class ConnectionInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _pingText, _addressText;

    private void Start()
    {
        _pingText = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        if (!NetworkClient.active) return;
        _pingText.SetText($"RTT: {System.Math.Round(NetworkTime.rtt * 1000)}ms");
        _addressText.SetText(NetworkManager.singleton.networkAddress);
    }
}
