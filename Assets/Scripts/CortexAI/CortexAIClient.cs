using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace CortexAI
{
    public class CortexAIClient
    {
        private const string SessionCookieKey = "CortexAI_Mobile_Session";
        public string BaseUrl = "http://192.168.1.9:8000"; // Update this to your deployed gateway URL before building
        
        public string SessionCookie { get; private set; }
        public CortexAIUser CurrentUser { get; private set; }

        public CortexAIClient()
        {
            SessionCookie = PlayerPrefs.GetString(SessionCookieKey, "");
        }

        public void SetManualCookie(string cookie)
        {
            SessionCookie = cookie.Contains("=") ? cookie : "session=" + cookie;
            PlayerPrefs.SetString(SessionCookieKey, SessionCookie);
            PlayerPrefs.Save();
        }

        public void Logout()
        {
            Debug.Log("[CortexAI] Logging out and clearing session cookie.");
            SessionCookie = "";
            CurrentUser = null;
            PlayerPrefs.DeleteKey(SessionCookieKey);
            PlayerPrefs.Save();
        }

        public async Task<bool> CheckSessionAsync()
        {
            if (string.IsNullOrEmpty(SessionCookie)) return false;
            var res = await GetAsync("/me");
            if (!string.IsNullOrEmpty(res))
            {
                CurrentUser = JsonUtility.FromJson<CortexAIUser>(res);
                return CurrentUser != null && !string.IsNullOrEmpty(CurrentUser.Id);
            }
            return false;
        }

        public async Task<bool> LoginAsync(string firebaseToken)
        {
            Debug.Log("[CortexAI] LoginAsync called with Firebase token.");
            string json = JsonUtility.ToJson(new CortexAILoginRequest(firebaseToken));
            string res = await PostJsonAsync("/auth/login", json);
            if (!string.IsNullOrEmpty(res))
            {
                CurrentUser = JsonUtility.FromJson<CortexAIUser>(res);
                Debug.Log($"[CortexAI] Login successful for user ID: {CurrentUser?.Id}");
                return true;
            }
            Debug.Log("[CortexAI] Login failed. Response was empty or error.");
            return false;
        }

        public async Task<CortexAIConversation> CreateConversationAsync()
        {
            string res = await PostJsonAsync("/chat/create-conversation", "{}");
            return string.IsNullOrEmpty(res) ? null : JsonUtility.FromJson<CortexAIConversation>(res);
        }

        public async Task<CortexAIConversation[]> GetConversationsAsync()
        {
            string res = await GetAsync("/chat/get-conversations");
            if (string.IsNullOrEmpty(res)) return new CortexAIConversation[0];
            var wrapper = JsonUtility.FromJson<ConvWrapper>("{\"items\":" + res + "}");
            return wrapper?.items ?? new CortexAIConversation[0];
        }

        public async Task<CortexAIMessage[]> GetMessagesAsync(string convId)
        {
            string res = await GetAsync($"/chat/message/get/{convId}");
            if (string.IsNullOrEmpty(res)) return new CortexAIMessage[0];
            var wrapper = JsonUtility.FromJson<MsgWrapper>("{\"items\":" + res + "}");
            return wrapper?.items ?? new CortexAIMessage[0];
        }

        public async Task<CortexAIAgentResponse> SendPromptAsync(string prompt, string convId, CortexAIAgentMode mode, string filePath = null)
        {
            Debug.Log($"[CortexAI] SendPromptAsync: prompt='{prompt}', agent='{mode.ToBackendValue()}', file='{filePath}'");
            List<IMultipartFormSection> form = new List<IMultipartFormSection>
            {
                new MultipartFormDataSection("prompt", prompt ?? ""),
                new MultipartFormDataSection("conversationId", convId ?? ""),
                new MultipartFormDataSection("agent", mode.ToBackendValue())
            };

            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                byte[] bytes = File.ReadAllBytes(filePath);
                form.Add(new MultipartFormFileSection("file", bytes, Path.GetFileName(filePath), "application/octet-stream"));
            }

            string res = await PostFormAsync("/agent/chat", form);
            return string.IsNullOrEmpty(res) ? null : JsonUtility.FromJson<CortexAIAgentResponse>(res);
        }

        // --- HTTP Helpers ---

        private async Task<string> GetAsync(string path)
        {
            using (UnityWebRequest req = UnityWebRequest.Get(BaseUrl + path))
            {
                return await SendRequest(req);
            }
        }

        private async Task<string> PostJsonAsync(string path, string json)
        {
            using (UnityWebRequest req = new UnityWebRequest(BaseUrl + path, "POST"))
            {
                req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                return await SendRequest(req);
            }
        }

        private async Task<string> PostFormAsync(string path, List<IMultipartFormSection> form)
        {
            using (UnityWebRequest req = UnityWebRequest.Post(BaseUrl + path, form))
            {
                return await SendRequest(req);
            }
        }

        private async Task<string> SendRequest(UnityWebRequest req)
        {
            Debug.Log($"[CortexAI] ---> HTTP {req.method} {req.url}");
            if (!string.IsNullOrEmpty(SessionCookie))
                req.SetRequestHeader("Cookie", SessionCookie);

            req.SetRequestHeader("Accept", "application/json");

            var op = req.SendWebRequest();
            while (!op.isDone) await Task.Delay(10);

            CaptureCookie(req.GetResponseHeader("Set-Cookie"));

#if UNITY_2020_1_OR_NEWER
            if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError)
#else
            if (req.isNetworkError || req.isHttpError)
#endif
            {
                Debug.LogError($"[CortexAI] <--- HTTP {req.responseCode} Error: {req.error}\nResponse: {req.downloadHandler?.text}");
                return null;
            }

            string responseText = req.downloadHandler?.text;
            Debug.Log($"[CortexAI] <--- HTTP {req.responseCode} {req.url}\nResponse: {responseText}");
            return responseText;
        }

        private void CaptureCookie(string header)
        {
            if (string.IsNullOrEmpty(header)) return;
            int start = header.IndexOf("session=", StringComparison.OrdinalIgnoreCase);
            if (start < 0) return;
            int end = header.IndexOf(';', start);
            SessionCookie = end >= 0 ? header.Substring(start, end - start) : header.Substring(start);
            PlayerPrefs.SetString(SessionCookieKey, SessionCookie);
            PlayerPrefs.Save();
        }
    }
}
