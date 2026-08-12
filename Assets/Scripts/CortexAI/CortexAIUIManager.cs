using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

namespace CortexAI
{
    public class CortexAIUIManager : MonoBehaviour
    {
        public CortexAIClient Client;

        [Header("Login Screen")]
        public GameObject LoginPanel;
        public TMP_InputField TokenInput;
        public Button GoogleLoginButton;

        [Header("Main Chat Screen")]
        public GameObject MainPanel;
        public TextMeshProUGUI UserInfoText;
        public Button LogoutButton;
        public Button NewChatButton;
        public TMP_Dropdown AgentModeDropdown;
        
        [Header("Sidebar")]
        public GameObject SidePanel;
        public Button OpenSidebarButton;
        public Button CloseSidebarButton;
        public Button SubscriptionButton;
        
        [Header("Conversations List")]
        public RectTransform ConversationsContent;
        public GameObject ConversationButtonPrefab; // A UI button prefab

        [Header("Chat Area")]
        public RectTransform MessagesContent;
        public GameObject MessagePrefab; // A UI Text/Panel prefab
        public TMP_InputField PromptInput;
        public Button SendButton;

        private CortexAIConversation _activeConv;
        public FirebaseAuthManager AuthManager;

        private CortexAIAgentMode _currentMode = CortexAIAgentMode.Auto;

        private void Start()
        {
            Client = new CortexAIClient();
            GoogleLoginButton.onClick.AddListener(OnGoogleLoginClicked);
            LogoutButton.onClick.AddListener(OnLogoutClicked);
            NewChatButton.onClick.AddListener(OnNewChatClicked);
            SendButton.onClick.AddListener(OnSendClicked);
            
            if (OpenSidebarButton != null) OpenSidebarButton.onClick.AddListener(() => SidePanel.SetActive(true));
            if (CloseSidebarButton != null) CloseSidebarButton.onClick.AddListener(() => SidePanel.SetActive(false));

            // Runtime fix for older generated UI layouts to ensure messages stretch
            if (MessagesContent != null)
            {
                var vlg = MessagesContent.GetComponent<VerticalLayoutGroup>();
                if (vlg != null) { vlg.childControlWidth = true; vlg.childForceExpandWidth = true; }
            }
            if (MessagePrefab != null)
            {
                var txtObj = MessagePrefab.transform.Find("MsgText");
                if (txtObj != null)
                {
                    if (txtObj.GetComponent<ContentSizeFitter>() == null)
                    {
                        var csf = txtObj.gameObject.AddComponent<ContentSizeFitter>();
                        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                    }
                    var tmp = txtObj.GetComponent<TextMeshProUGUI>();
                    if (tmp != null) tmp.textWrappingMode = TextWrappingModes.Normal;
                }
            }

            if (AgentModeDropdown != null)
            {
                AgentModeDropdown.ClearOptions();
                var modes = new List<string>(System.Enum.GetNames(typeof(CortexAIAgentMode)));
                AgentModeDropdown.AddOptions(modes);
                
                AgentModeDropdown.value = modes.IndexOf(CortexAIAgentMode.Auto.ToString());
                AgentModeDropdown.onValueChanged.AddListener(OnAgentModeChanged);
            }

            _ = InitializeAsync();
        }

        private void OnAgentModeChanged(int index)
        {
            _currentMode = (CortexAIAgentMode)index;
        }

        private async Task InitializeAsync()
        {
            bool loggedIn = await Client.CheckSessionAsync();
            ShowPanel(loggedIn);
            if (loggedIn) await RefreshConversations();
        }

        private void ShowPanel(bool loggedIn)
        {
            LoginPanel.SetActive(!loggedIn);
            MainPanel.SetActive(loggedIn);
            
            if (loggedIn && Client.CurrentUser != null)
            {
                UserInfoText.text = $"{Client.CurrentUser.DisplayName} (Credits: {Client.CurrentUser.credits})";
            }
        }

        private async void OnGoogleLoginClicked()
        {
            GoogleLoginButton.interactable = false;

            // 1. Check if a token was manually pasted (for Editor testing)
            string firebaseToken = TokenInput != null ? TokenInput.text : "";

            // 2. If no token was pasted, attempt native Google Sign-In
            if (string.IsNullOrEmpty(firebaseToken))
            {
                firebaseToken = await AuthManager.SignInWithGoogleAsync();
            }
            
            if (!string.IsNullOrEmpty(firebaseToken))
            {
                // 3. Send the token to your Python Backend using your existing Client
                bool success = await Client.LoginAsync(firebaseToken);
                
                ShowPanel(success);
                if (success) await RefreshConversations();
            }
            else
            {
                Debug.LogError("Failed to get Firebase token.");
            }

            GoogleLoginButton.interactable = true;
        }


        private void OnLogoutClicked()
        {
            AuthManager.SignOut(); // Sign out of Firebase/Google
            Client.Logout();       // Clear your custom Python session
            ShowPanel(false);
            ClearChat();
        }


        private async void OnNewChatClicked()
        {
            var conv = await Client.CreateConversationAsync();
            if (conv != null)
            {
                await RefreshConversations();
                _activeConv = conv;
                ClearChat();

                if (SidePanel != null) SidePanel.SetActive(false); 
            }
        }

        private async Task RefreshConversations()
        {
            // Deactivate existing buttons (Pool them)
            foreach (Transform child in ConversationsContent)
            {
                child.gameObject.SetActive(false);
            }

            var convs = await Client.GetConversationsAsync();
            Debug.Log($"[CortexAI] Loaded {convs?.Length ?? 0} conversations from API.");
            if (convs == null || convs.Length == 0) return;

            foreach (var c in convs)
            {
                GameObject btnObj = null;
                
                // Find an inactive button in the pool
                foreach (Transform child in ConversationsContent)
                {
                    if (!child.gameObject.activeSelf)
                    {
                        btnObj = child.gameObject;
                        break;
                    }
                }

                // Create a new one if pool is empty
                if (btnObj == null)
                {
                    btnObj = Instantiate(ConversationButtonPrefab, ConversationsContent);
                }

                btnObj.SetActive(true);
                btnObj.transform.SetAsLastSibling();
                
                // Update Title Text
                var txtTransform = btnObj.transform.Find("Text");
                if (txtTransform != null) 
                    txtTransform.GetComponent<TextMeshProUGUI>().text = c.DisplayTitle;
                else 
                    btnObj.GetComponentInChildren<TextMeshProUGUI>().text = c.DisplayTitle;

                // Clear old pooled listeners before adding the new one
                Button btn = btnObj.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => _ = SelectConversation(c));
            }

            // Auto-select the first (most recent) conversation by default
            if (convs.Length > 0)
                _ = SelectConversation(convs[0]);
        }

        private async Task SelectConversation(CortexAIConversation c)
        {
            _activeConv = c;
            ClearChat();

            if (SidePanel != null) SidePanel.SetActive(false);
            
            var msgs = await Client.GetMessagesAsync(c.Id);
            if (msgs == null) return;

            foreach (var m in msgs) AddMessageToUI(m.role == "user" ? "You" : "CortexAI", m.content, m.images);
        }

        private async void OnSendClicked()
        {
            Debug.Log("[CortexAI] Send button clicked.");

            if (string.IsNullOrEmpty(PromptInput.text))
            {
                Debug.LogWarning("[CortexAI] Send aborted: Prompt input is empty.");
                return;
            }

            if (_activeConv == null)
            {
                Debug.LogWarning("[CortexAI] Send aborted: No active conversation selected.");
                return;
            }

            string prompt = PromptInput.text;
            PromptInput.text = "";
            SendButton.interactable = false;

            Debug.Log($"[CortexAI] Sending prompt: {prompt}");
            AddMessageToUI("You", prompt);

            var res = await Client.SendPromptAsync(prompt, _activeConv.Id, _currentMode);
            
            if (res != null) AddMessageToUI("CortexAI", res.answer, res.images);
            else AddMessageToUI("Error", "Failed to get a response from the server.");

            SendButton.interactable = true;
        }

        private void AddMessageToUI(string sender, string text, string[] images = null)
        {
            GameObject msgObj = null;
    
            // 1. Try to find an inactive pooled object
            foreach (Transform child in MessagesContent)
            {
                if (!child.gameObject.activeSelf)
                {
                    msgObj = child.gameObject;
                    break;
                }
            }
            
            // 2. Expand pool if no inactive objects are found
            if (msgObj == null)
            {
                msgObj = Instantiate(MessagePrefab, MessagesContent);
            }

            // 3. Activate and move to bottom
            msgObj.SetActive(true);
            msgObj.transform.SetAsLastSibling();
            
            // 4. Set the text
            var txtObj = msgObj.transform.Find("MsgText");
            if (txtObj != null)
                txtObj.GetComponent<TextMeshProUGUI>().text = $"<b>{sender}</b>\n{text}";
            else
                msgObj.GetComponentInChildren<TextMeshProUGUI>().text = $"<b>{sender}</b>\n{text}";

            // Handle Images
            var imgObj = msgObj.transform.Find("MsgImage");
            if (imgObj != null)
            {
                if (images != null && images.Length > 0 && !string.IsNullOrEmpty(images[0]))
                {
                    imgObj.gameObject.SetActive(true);
                    _ = DownloadAndApplyImageAsync(images[0], imgObj.GetComponent<Image>(), imgObj.GetComponent<AspectRatioFitter>());
                }
                else
                {
                    imgObj.gameObject.SetActive(false);
                }
            }

        }

        private async Task DownloadAndApplyImageAsync(string url, Image targetImage, AspectRatioFitter fitter)
        {
            if (string.IsNullOrEmpty(url) || targetImage == null) return;

            if (url.StartsWith("/")) url = Client.BaseUrl + url;

            using (UnityEngine.Networking.UnityWebRequest req = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(url))
            {
                var op = req.SendWebRequest();
                while (!op.isDone) await Task.Delay(10);

        #if UNITY_2020_1_OR_NEWER
                if (req.result == UnityEngine.Networking.UnityWebRequest.Result.ConnectionError || req.result == UnityEngine.Networking.UnityWebRequest.Result.ProtocolError)
        #else
                if (req.isNetworkError || req.isHttpError)
        #endif
                {
                    Debug.LogError($"[CortexAI] Failed to download image from {url}: {req.error}");
                    return;
                }

                Texture2D texture = UnityEngine.Networking.DownloadHandlerTexture.GetContent(req);
                if (texture != null)
                {
                    targetImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    if (fitter != null)
                    {
                        fitter.aspectRatio = (float)texture.width / texture.height;
                    }
                }
            }
        }


        private void ClearChat()
        {
            foreach (Transform child in MessagesContent)
            {
                child.gameObject.SetActive(false);
            }
        }

    }
}