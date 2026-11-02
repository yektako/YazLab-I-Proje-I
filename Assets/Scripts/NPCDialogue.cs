using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Networking;
using System.Text;
using Player;
using System.Linq;

public class NPCDialogue : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PanelRenderer approachUI;
    [SerializeField] private PanelRenderer dialogueUI;
    [SerializeField] private PanelRenderer overlayUI;
    [SerializeField] private CinemachineCamera dialogueCamera;
    [SerializeField] private float minDistance = 6;
    [SerializeField] private string systemPrompt = "Only respond in short Turkish sentences.";
    private DialoguePrompt dialoguePrompt;
    private TextField dialogueField;
    private Label messageLogLabel;
    private List<string> messages;
    //private readonly string apiUrl = "http://localhost:9931/completions";
    private readonly string chatUrl = "http://localhost:9931/v1/chat/completions";
    [System.Serializable]
    public class DialogueMessage
    {
        public string role;
        public string content;
        public DialogueMessage(string role, string content)
        {
            this.role = role;
            this.content = content;
        }
    }
    [System.Serializable]
    public class DialoguePrompt
    {
        public string model = "default";
        public List<DialogueMessage> messages;
        public int max_tokens = 150;
        public bool stream = true;
        public DialoguePrompt(string systemPrompt)
        {
            messages = new List<DialogueMessage>();
            AddMessage("system",systemPrompt);
        }
        public void AddMessage(string role, string content)
        {
            DialogueMessage message = new DialogueMessage(role,content);
            messages.Add(message);
        }
    }
    [System.Serializable]
    public class DialogueResponseChunk
    {
        public DialogueResponseChoices[] choices;
        public int created;
        public string id;
        public string model;
    }
    [System.Serializable]
    public class DialogueResponseChoices
    {
        public string finish_reason;
        public int index;
        public DialogueResponseDelta delta;
    }
    [System.Serializable]
    public class DialogueResponseDelta
    {
        public string content;
    }
    void Start()
    {
        dialogueUI.RegisterUIReloadCallback(OnUIReload);
        StartCoroutine(GetApiHealth());
        dialoguePrompt = new DialoguePrompt(systemPrompt);
        Debug.Log($"diyalog promptu oluşturuldu: {JsonUtility.ToJson(dialoguePrompt)}");
    }
    void Update()
    {
        float distance = Vector3.Distance(transform.position,playerController.transform.position);
        if (distance <= minDistance && !approachUI.enabled && !dialogueUI.enabled)
        {
            approachUI.enabled = true;
        }
        else if (distance > minDistance && dialogueUI.enabled)
        {
            LeaveDialogue();
        }
        else if (distance > minDistance && approachUI.enabled)
        {
            approachUI.enabled = false;
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
            else if (Keyboard.current.enterKey.wasPressedThisFrame && messageLogLabel != null && dialogueField != null)
            {
                if (dialogueField.value != string.Empty)
                {
                    StartCoroutine(SendDialogueMessage(dialogueField.value));
                }
                MessageLogWriteLine($"► {dialogueField.value}");
                dialogueField.value = "";
            }
        }
    }
    private IEnumerator SendDialogueMessage(string messageContent)
    {
        dialoguePrompt.AddMessage("user",messageContent);
        string jsonRequest = JsonUtility.ToJson(dialoguePrompt);
        Debug.Log($"json oluşturuldu: {jsonRequest}");
        using (UnityWebRequest request = new UnityWebRequest(chatUrl, "POST"))
        {
            byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonRequest);
            request.uploadHandler = new UploadHandlerRaw(jsonBytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type","application/json");
            Debug.Log($"prompt gönderiliyor: {messageContent}");
            UnityWebRequestAsyncOperation stream = request.SendWebRequest();
            ulong responseLength = 0;
            string fullResponse = "";
            while (!stream.isDone)
            {
                if (responseLength < request.downloadedBytes)
                {
                    responseLength = request.downloadedBytes;
                    try
                    {
                        int startIndex = request.downloadHandler.text.IndexOf("{");
                        string lastChunk = request.downloadHandler.text.Trim().Split('\n').Last().Substring(startIndex);
                        Debug.Log($"chunk: {lastChunk}");
                        DialogueResponseChunk chunk = JsonUtility.FromJson<DialogueResponseChunk>(lastChunk);
                        string content = chunk.choices[0].delta.content;
                        if (content != "<think>" && content != "</think>" && content != "\n\n")
                        {
                            MessageLogWrite(content);
                            fullResponse += content;
                        }
                    }
                    catch (System.Exception exception)
                    {
                        Debug.LogError($"JSON okuma hatası: {exception.Message}");
                        MessageLogWriteLine($"\n❌ Json okuma hatası: {exception.Message}");
                        request.Abort();
                    }
                }
                yield return null;
            }
            if (request.result == UnityWebRequest.Result.Success)
            {
                MessageLogWriteLine($"\n- - - - - - - -");
                dialoguePrompt.AddMessage("assistant",fullResponse);
                Debug.Log($"başarılı, tam cevap: {fullResponse} --- mevcut prompt: {JsonUtility.ToJson(dialoguePrompt)}");
            }
            else
            {
                MessageLogWriteLine("\n❌ " + request.error);
                Debug.LogError($"prompt hatası: {request.error}");
            }
        }
    }
    private void MessageLogWrite(string message)
    {
        messageLogLabel.text += message;
    }
    private void MessageLogWriteLine(string message)
    {
        messageLogLabel.text += message + '\n';
    }
    private IEnumerator GetApiHealth()
    {
        string apiUrl = "http://localhost:9931/health";
        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "GET"))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return request.SendWebRequest();
            string response = request.downloadHandler.text;
            Debug.Log($"sağlık: {response} - kod: {request.responseCode}");
        }
    }
    private void EnterDialogue()
    {
        approachUI.enabled = false;
        overlayUI.enabled = false;
        dialogueUI.enabled = true;
        dialogueCamera.Priority = 100;
        UnityEngine.Cursor.visible = true;
        UnityEngine.Cursor.lockState = CursorLockMode.None;
    }
    private void LeaveDialogue()
    {
        overlayUI.enabled = true;
        dialogueUI.enabled = false;
        dialogueCamera.Priority = 0;
        UnityEngine.Cursor.visible = false;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
    }
    private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
    {
        dialogueField = root.Q<TextField>("InputDialogue");
        messageLogLabel = root.Q<Label>("MessageLog");
    }
}