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
            }
        }

        private async Task RefreshConversations()
        {
            // Clear existing buttons
            foreach (Transform child in ConversationsContent) Destroy(child.gameObject);

            var convs = await Client.GetConversationsAsync();
            Debug.Log($"[CortexAI] Loaded {convs?.Length ?? 0} conversations from API.");
            if (convs == null || convs.Length == 0) return;

            foreach (var c in convs)
            {
                var btnObj = Instantiate(ConversationButtonPrefab, ConversationsContent);
                btnObj.SetActive(true); // Crucial: Prefab is inactive by default!
                
                var txtTransform = btnObj.transform.Find("Text");
                if (txtTransform != null) 
                    txtTransform.GetComponent<TextMeshProUGUI>().text = c.DisplayTitle;
                else 
                    btnObj.GetComponentInChildren<TextMeshProUGUI>().text = c.DisplayTitle;

                btnObj.GetComponent<Button>().onClick.AddListener(() => _ = SelectConversation(c));
            }

            // Auto-select the first (most recent) conversation by default
            if (convs.Length > 0)
                _ = SelectConversation(convs[0]);
        }

        private async Task SelectConversation(CortexAIConversation c)
        {
            _activeConv = c;
            ClearChat();
            
            var msgs = await Client.GetMessagesAsync(c.Id);
            if (msgs == null) return;

            foreach (var m in msgs) AddMessageToUI(m.role == "user" ? "You" : "CortexAI", m.content);
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
            
            if (res != null) AddMessageToUI("CortexAI", res.answer);
            else AddMessageToUI("Error", "Failed to get a response from the server.");

            SendButton.interactable = true;
        }

        private void AddMessageToUI(string sender, string text)
        {
            var msgObj = Instantiate(MessagePrefab, MessagesContent);
            msgObj.GetComponentInChildren<TextMeshProUGUI>().text = $"<b>{sender}</b>\n{text}";
        }

        private void ClearChat()
        {
            foreach (Transform child in MessagesContent) Destroy(child.gameObject);
        }
    }
}