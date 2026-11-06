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
using DialogueJSON;

public class NPCDialogue : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private PanelRenderer approachUI;
    [SerializeField] private PanelRenderer dialogueUI;
    [SerializeField] private PanelRenderer overlayUI;
    [SerializeField] private CinemachineCamera dialogueCamera;
    [SerializeField] private DoorController doorController;
    [SerializeField] private float minDistance = 6;
    protected string systemPromptDoor = "Sen bir robotsun. Arkanda açılmayan bir kapı var. Kapının açılması için 5 adet dişli bulmak gerekli. Kullanıcıdan bu dişlileri bulmasını iste. Hayır derse sinirlen. Bulduysa sevin.";
    protected string systemPromptDoorQuestion = "Bu diyaloga göre kapı açılmalı mı?";
    protected string systemPromptDoorUpdate = "Kullanıcının bulduğu dişli sayısı:";
    private bool isFirstMessage = true;
    private bool isDoorOpen = false;
    private bool isThinking = false;
    private DialoguePrompt dialoguePrompt;
    private GameStatePrompt gameStatePrompt;
    private TextField dialogueField;
    private Label messageLogLabel;
    private Label labelServerReady;
    private Label labelServerNotReady;
    private Label labelThinking;
    private List<string> messages;
    //private readonly string apiUrl = "http://localhost:9931/completions";
    private readonly string chatUrl = "http://localhost:9931/v1/chat/completions";
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
        gameStatePrompt = new GameStatePrompt(systemPromptDoorQuestion);
        InvokeRepeating(nameof(GetApiHealth), 1, 5);
    }
    void Update()
    {
        float distance = Vector3.Distance(transform.position,player.transform.position);
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
                    dialogueField.SetEnabled(false);
                }
                MessageLogWriteLine($"► {dialogueField.value}");
                dialogueField.value = "";
            }
        }

        if (labelThinking != null && !isThinking) labelThinking.visible = false;
    }
    private IEnumerator SendDialogueMessage(string userMessage)
    {
        if (!isFirstMessage)
        {
            dialoguePrompt.AddMessage("assistant",systemPromptDoorUpdate + player.collectibleCount);
        }
        else isFirstMessage = false;
        dialoguePrompt.AddMessage("user",userMessage);
        string jsonRequest = JsonUtility.ToJson(dialoguePrompt);
        Debug.Log($"json oluşturuldu: {jsonRequest}");
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
            bool boldTextStarted = false;
            while (!stream.isDone)
            {
                if (!isThinking) isThinking = true;
                if (responseLength < request.downloadedBytes)
                {
                    responseLength = request.downloadedBytes;
                    try
                    {
                        int startIndex = request.downloadHandler.text.IndexOf("{");
                        string lastChunk = request.downloadHandler.text.Trim().Split('\n').Last().Substring(startIndex);
                        DialogueResponseChunk chunk = JsonUtility.FromJson<DialogueResponseChunk>(lastChunk);
                        string content = chunk.choices[0].delta.content;
                        if (content.Contains("**"))
                        {
                            content = content.Replace("**"," ");
                            if (!boldTextStarted)
                            {
                                MessageLogWrite("<b>");
                                boldTextStarted = true;
                            }
                            else
                            {
                                MessageLogWrite("</b>");
                                boldTextStarted = false;
                            }
                            MessageLogWrite(content);
                        }
                        else if (content != "<think>" && content != "</think>" && content != "\n\n")
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
                dialoguePrompt.messages.RemoveAt(dialoguePrompt.messages.Count-1); // dişli sayısı mesajını sil
                dialoguePrompt.AddMessage("assistant",fullResponse);
                Debug.Log($"başarılı, tam cevap: {fullResponse} --- mevcut prompt: {JsonUtility.ToJson(dialoguePrompt)}");

                if (dialoguePrompt.messages.Count > 6) {dialoguePrompt.messages.RemoveAt(1);}

                if (!isDoorOpen)
                {
                    gameStatePrompt = new GameStatePrompt(systemPromptDoorQuestion);
                    gameStatePrompt.AddUserMessage(player.collectibleCount.ToString());
                    StartCoroutine(SendGameStatePrompt());
                }
                else dialogueField.SetEnabled(true);
            }
            else
            {
                if (request.responseCode == 0) MessageLogWriteLine("\n❌ Sunucuya bağlanılamadı.");
                else MessageLogWriteLine("\n❌ Sunucu hatası: " + request.error);
                Debug.LogError($"sunucu hatası: {request.error}, kod: {request.responseCode}");
                dialoguePrompt.messages.Clear();
                dialoguePrompt.AddSystemMessage(systemPromptDoor);
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
            }
            else
            {
                MessageLogWriteLine("\n❌ Sunucu hatası: " + request.error);
                Debug.LogError($"gamestate sunucu hatası: {request.error}, kod: {request.responseCode}");
            }
        }
        gameStatePrompt.messages.Clear();
        dialogueField.SetEnabled(true);
        isThinking = false;
    }
    private void UpdateGameState(bool result)
    {
        if (result)
        {
            Debug.Log($"gamestate doğru yanıt alındı, kapı açılıyor");
            doorController.Open();
            isDoorOpen = true;
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
            if (labelServerReady != null & labelServerNotReady != null)
            {
                if (request.responseCode == 200)
                {
                    labelServerReady.visible = true;
                    labelServerNotReady.visible = false;
                }
                else
                {
                    labelServerReady.visible = false;
                    labelServerNotReady.visible = true;
                }
            }
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
        labelServerReady = root.Q<Label>("Ready");
        labelServerNotReady = root.Q<Label>("NotReady");
        labelThinking = root.Q<Label>("Thinking");
    }
}