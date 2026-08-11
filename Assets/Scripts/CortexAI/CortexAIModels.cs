using System;
using UnityEngine;

namespace CortexAI
{
    public enum CortexAIAgentMode { Auto, Chat, Search, Coding, PDF, PPT, Image }

    public static class CortexAIAgentModeExtensions
    {
        public static string ToBackendValue(this CortexAIAgentMode mode)
        {
            switch (mode) {
                case CortexAIAgentMode.Chat: return "chat";
                case CortexAIAgentMode.Search: return "search";
                case CortexAIAgentMode.Coding: return "coding";
                case CortexAIAgentMode.PDF: return "pdf";
                case CortexAIAgentMode.PPT: return "ppt";
                case CortexAIAgentMode.Image: return "image";
                default: return "auto";
            }
        }
    }

    [Serializable]
    public class CortexAILoginRequest
    {
        public string token;
        public CortexAILoginRequest(string token) { this.token = token; }
    }

    [Serializable]
    public class CortexAIUser
    {
        public string id;
        public string userId;
        public string _id;
        public string name;
        public string email;
        public string plan;
        public int credits;
        public int total_credits;
        public int totalCredits;

        public string Id => !string.IsNullOrEmpty(userId) ? userId : (!string.IsNullOrEmpty(_id) ? _id : id);
        public string DisplayName => !string.IsNullOrEmpty(name) ? name : email;
        public int TotalCreditsValue => totalCredits > 0 ? totalCredits : total_credits;
    }

    [Serializable]
    public class CortexAIConversation
    {
        public string _id;
        public string id;
        public string title;
        
        public string Id => !string.IsNullOrEmpty(_id) ? _id : id;
        public string DisplayTitle => !string.IsNullOrEmpty(title) ? title : (Id != null && Id.Length > 8 ? "Chat " + Id.Substring(0, 8) : "New Conversation");
    }

    [Serializable]
    public class CortexAIArtifact
    {
        public string title;
        public string content;
        public string url;
        public string filename;
    }

    [Serializable]
    public class CortexAIMessage
    {
        public string role;
        public string content;
        public string[] images;
        public CortexAIArtifact[] artifacts;
    }

    [Serializable]
    public class CortexAIAgentResponse
    {
        public string answer;
        public string[] images;
        public CortexAIArtifact[] artifacts;
    }

    // Wrappers for parsing JSON Arrays in Unity
    [Serializable] public class ConvWrapper { public CortexAIConversation[] items; }
    [Serializable] public class MsgWrapper { public CortexAIMessage[] items; }
}
