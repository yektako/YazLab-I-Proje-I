using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Networking;
using System.Text.Json;
using System.Text;

public class NPCDialogue : MonoBehaviour
{
    [SerializeField] private Transform npcTransform;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private PanelRenderer approachUI;
    [SerializeField] private PanelRenderer dialogueUI;
    [SerializeField] private PanelRenderer overlayUI;
    [SerializeField] private CinemachineCamera dialogueCamera;
    [SerializeField] private float minDistance = 6;
    private PlayerInput playerInput;
    private TextField dialogueField;
    private Label messageLog;
    private List<string> messages;
    void Start()
    {
        dialogueUI.RegisterUIReloadCallback(OnUIReload);
        playerInput = playerTransform.gameObject.GetComponent<PlayerInput>();
    }
    void Update()
    {
        float distance = Vector3.Distance(npcTransform.position,playerTransform.position);
        if (distance <= minDistance && !approachUI.enabled && !dialogueUI.enabled)
        {
            approachUI.enabled = true;
        }
        else if (distance > minDistance && dialogueUI.enabled)
        {
            LeaveDialogue();
        }

        if (approachUI.enabled && Keyboard.current.eKey.wasPressedThisFrame)
        {
            EnterDialogue();
        }

        if (dialogueUI.enabled)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                LeaveDialogue();
            }
            else if (Keyboard.current.enterKey.wasPressedThisFrame && messageLog != null && dialogueField != null)
            {
                if (dialogueField.value != "")
                {
                    StartCoroutine(SendPrompt(dialogueField.value));
                }
                messageLog.text += $"\n► {dialogueField.value}";
                dialogueField.value = "";
            }
        }
    }
    private IEnumerator SendPrompt(string text)
    {
        string url = "http://localhost:9931/completion";
        var data = new Dictionary<string, object>{
            {"model", "default"},
            {"prompt", text},
            {"max_tokens",600},
            {"stream",false}
        };
        string jsonString = JsonSerializer.Serialize(data);
        using (var request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonString);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type","application/json");
            Debug.Log($"prompt gönderiliyor: {text}");
            request.timeout = 60;
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = request.downloadHandler.text;
                JsonElement root = JsonDocument.Parse(response).RootElement;
                root.TryGetProperty("content", out JsonElement content);
                string responseString = content.GetString().Replace("<think>","").Replace("</think>","").Trim();
                messageLog.text += "\n🤖 " + responseString;
                Debug.Log($"yanıt alındı: {response}");
            }
            else
            {
                messageLog.text += "\n🤖❌ HATA: " + request.error;
                Debug.LogError($"prompt hatası: {request.error}");
            }
        }
    }
    private void EnterDialogue()
    {
        approachUI.enabled = false;
        overlayUI.enabled = false;
        dialogueUI.enabled = true;
        dialogueCamera.Priority = 100;
        playerInput.enabled = false;
        UnityEngine.Cursor.visible = true;
        UnityEngine.Cursor.lockState = CursorLockMode.None;
    }
    private void LeaveDialogue()
    {
        overlayUI.enabled = true;
        dialogueUI.enabled = false;
        dialogueCamera.Priority = 0;
        playerInput.enabled = true;
        UnityEngine.Cursor.visible = false;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
    }
    private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
    {
        if (dialogueField == null)
        {
            dialogueField = root.Q<TextField>("InputDialogue");
        }
        if (messageLog == null)
        {
            messageLog = root.Q<Label>("MessageLog");
        }
    }
}