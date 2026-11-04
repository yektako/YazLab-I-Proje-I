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
    [SerializeField] private DoorController doorController;
    [SerializeField] private float minDistance = 6;
    public string systemPromptDoor = "Only respond in Turkish. You really need to get five motherboards from a depot. Ask the user to retrieve them. If user says yes, send just 'OPENING'. Otherwise, get angry at user.";
    public string systemPromptDoorQuestion = "Is the following message agreeing to collect motherboards? If user explicitly said yes, 'is_door_opened' is true, otherwise false.";
    public string systemPromptDoorOpen = "Only respond in Turkish. A depot door has opened. Ask the user to rush into the depot and collect five motherboards.";
    private DialoguePrompt dialoguePrompt;
    private GameStatePrompt gameStatePrompt;
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
            AddSystemMessage(systemPrompt);
        }
        public void AddMessage(string role, string content)
        {
            DialogueMessage message = new DialogueMessage(role,content);
            messages.Add(message);
        }
        public void AddSystemMessage(string content)
        {
            DialogueMessage message = new DialogueMessage("system",content);
            messages.Add(message);
        }
        public void AddUserMessage(string content)
        {
            DialogueMessage message = new DialogueMessage("user",content);
            messages.Add(message);
        }
    }
    [System.Serializable]
    public class GameStatePrompt : DialoguePrompt
    {
        public DialogueJSONSchema json_schema;
        public GameStatePrompt(string systemPrompt) : base(systemPrompt)
        {
            var properties = new DialogueJSONProperties();
            string[] required = {"is_door_opened"};
            json_schema = new DialogueJSONSchema(properties, required);
            stream = false;
        }
    }
    [System.Serializable]
    public class DialogueJSONSchema
    {
        public string type = "object";
        public DialogueJSONProperties properties;
        public string[] required;
        public DialogueJSONSchema(DialogueJSONProperties properties, string[] required)
        {
            this.properties = properties;
            this.required = required;
        }
    }
    [System.Serializable]
    public class DialogueJSONProperties
    {
        public DialogueJSONPropertyType is_door_opened = new DialogueJSONPropertyType("boolean");
    }
    [System.Serializable]
    public class DialogueJSONPropertyType
    {
        public string type;
        public DialogueJSONPropertyType(string type)
        {
            this.type = type;
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
    [System.Serializable]
    public class GameStateResponse
    {
        public GameStateResponseChoices[] choices;
        public int created;
        public string model;
    }
    [System.Serializable]
    public class GameStateResponseChoices
    {
        public string finish_reason;
        public int index;
        public GameStateResponseMessage message;
    }
    [System.Serializable]
    public class GameStateResponseMessage
    {
        public string role;
        public string content;
    }
    [System.Serializable]
    public class GameStateDoor
    {
        public bool is_door_opened;
    }
    void Start()
    {
        dialogueUI.RegisterUIReloadCallback(OnUIReload);
        StartCoroutine(GetApiHealth());
        dialoguePrompt = new DialoguePrompt(systemPromptDoor);
        gameStatePrompt = new GameStatePrompt(systemPromptDoor);
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
                    gameStatePrompt = new GameStatePrompt(systemPromptDoorQuestion);
                    gameStatePrompt.AddUserMessage(dialogueField.value);
                    StartCoroutine(SendGameStatePrompt());
                }
                MessageLogWriteLine($"► {dialogueField.value}");
                dialogueField.value = "";
            }
        }
    }
    private IEnumerator SendDialogueMessage(string userMessage)
    {
        dialoguePrompt.AddMessage("user",userMessage);
        string jsonRequest = JsonUtility.ToJson(dialoguePrompt);
        //Debug.Log($"json oluşturuldu: {jsonRequest}");
        using (UnityWebRequest request = new UnityWebRequest(chatUrl, "POST"))
        {
            byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonRequest);
            request.uploadHandler = new UploadHandlerRaw(jsonBytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type","application/json");
            //Debug.Log($"prompt gönderiliyor: {messageContent}");
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

                if (dialoguePrompt.messages.Count > 6) {dialoguePrompt.messages.RemoveAt(1);}
            }
            else
            {
                MessageLogWriteLine("\n❌ " + request.error);
                Debug.LogError($"prompt hatası: {request.error}");
            }
        }
    }
    private IEnumerator SendGameStatePrompt()
    {
        string jsonRequest = JsonUtility.ToJson(gameStatePrompt);
        Debug.Log($"gamestate json oluşturuldu: {jsonRequest}");
        using (UnityWebRequest request = new UnityWebRequest(chatUrl, "POST"))
        {
            byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonRequest);
            request.uploadHandler = new UploadHandlerRaw(jsonBytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type","application/json");
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"gamestate yanıt alındı: {request.downloadHandler.text}");
                string responseContent = JsonUtility.FromJson<GameStateResponse>(request.downloadHandler.text).choices[0].message.content;
                bool result = JsonUtility.FromJson<GameStateDoor>(responseContent).is_door_opened;
                UpdateGameState(result);
                StartCoroutine(SendDialogueMessage(dialogueField.value));
            }
            else
            {
                MessageLogWriteLine("\n❌ " + request.error);
                Debug.LogError($"gamestate prompt hatası: {request.error}");
            }
        }
        gameStatePrompt.messages.Clear();
    }
    private void UpdateGameState(bool result)
    {
        if (result)
        {
            Debug.Log($"gamestate doğru yanıt alındı, kapı açılıyor");
            doorController.Open();
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