using System.Collections.Generic;
using UnityEngine;

namespace DialogueJSON
{
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
}
