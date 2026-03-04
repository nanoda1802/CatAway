using System;
using Cysharp.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SF = UnityEngine.SerializeField;


public class TempConnector : MonoBehaviour
{
    [SF] private UnityTransport utp;
    [SF] private NetworkManager netManager;
    
    [SF] private TMP_InputField ipInputField;
    [SF] private TMP_InputField portInputField;

    [SF] private Button connectBtn;
    [SF] private Button hostBtn;

    // 입력 한 값 확인, 거기로 네트워크 매니저 주소 바꾸고
    // 스타트 클라이언트

    private void Start()
    {
        connectBtn.onClick.AddListener(StartClient);
        hostBtn.onClick.AddListener(StartHost);
    }


    private void StartClient()
    {
        if (!netManager.isActiveAndEnabled) return;
        
        var ip = ipInputField.text.Replace(" ", "");
        var port = ushort.Parse(portInputField.text.Replace(" ", ""));
        
        Debug.Log($"[StartClient] Connecting to {ip}:{port}");
        
        utp.SetConnectionData(ip, port);        
        netManager.StartClient();
        
        Deactivate().Forget();
    }

    private void StartHost()
    {
        if (!netManager.isActiveAndEnabled) return;
        
        netManager.StartHost();
        
        netManager.SceneManager.LoadScene("Level_Dev", LoadSceneMode.Additive);
        
        Deactivate().Forget();
    }

    private async UniTaskVoid Deactivate()
    {
        await UniTask.Delay(1);
        
        this.gameObject.SetActive(false);
    }
}
