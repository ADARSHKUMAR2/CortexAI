#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CortexAI.Editor
{
    public class CortexAIUIBuilder
    {
        [MenuItem("CortexAI/Generate Mobile UI")]
        public static void GenerateUI()
        {
            // 1. Create Canvas
            GameObject canvasObj = new GameObject("CortexAI_Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f; 
            
            canvasObj.AddComponent<GraphicRaycaster>();

            CortexAIUIManager uiManager = canvasObj.AddComponent<CortexAIUIManager>();

            // 2. Login Panel (Stretches to fill screen, uses SafeArea)
            // Vibrant Background: Deep Purple
            GameObject loginPanel = CreatePanel("LoginPanel", canvasObj.transform, new Color(0.15f, 0.05f, 0.25f)); 
            SetStretch(loginPanel);
            loginPanel.AddComponent<SafeArea>(); 
            
            // Login Content Container
            GameObject loginContent = CreatePanel("LoginContent", loginPanel.transform, Color.clear);
            SetRect(loginContent, new Vector2(0, 0), new Vector2(900, 600), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            
            GameObject loginText = CreateText("Title", loginContent.transform, "Login to CortexAI", 75);
            loginText.GetComponent<Text>().color = new Color(0.9f, 0.7f, 1.0f); // Bright Lilac
            SetRect(loginText, new Vector2(0, 200), new Vector2(800, 100));
            
            // Token Input Field for Editor Testing ---
            GameObject tokenInputObj = DefaultControls.CreateInputField(new DefaultControls.Resources());
            tokenInputObj.name = "TokenInput";
            tokenInputObj.transform.SetParent(loginContent.transform, false);
            tokenInputObj.GetComponent<Image>().color = new Color(0.2f, 0.1f, 0.3f); // Dark purple input
            SetRect(tokenInputObj, new Vector2(0, 50), new Vector2(800, 100));
            
            Text tokenInputText = tokenInputObj.GetComponentInChildren<Text>();
            tokenInputText.fontSize = 35;
            tokenInputText.color = Color.white;
            
            var tokenPlaceholder = tokenInputObj.transform.Find("Placeholder")?.GetComponent<Text>();
            if(tokenPlaceholder != null) {
                tokenPlaceholder.text = "Paste Token here (Editor only)...";
                tokenPlaceholder.color = new Color(0.6f, 0.5f, 0.8f);
            }
            // ------------------------------------------------


            GameObject loginBtnObj = DefaultControls.CreateButton(new DefaultControls.Resources());
            loginBtnObj.name = "LoginButton";
            loginBtnObj.transform.SetParent(loginContent.transform, false);
            loginBtnObj.GetComponent<Image>().color = new Color(0.9f, 0.1f, 0.5f); // Vibrant Pink
            Text btnText = loginBtnObj.GetComponentInChildren<Text>();
            btnText.text = "Authenticate";
            btnText.fontSize = 45;
            btnText.color = Color.white;
            SetRect(loginBtnObj, new Vector2(0, -50), new Vector2(600, 120));

            uiManager.LoginPanel = loginPanel;
            uiManager.TokenInput = tokenInputObj.GetComponent<InputField>();
            uiManager.GoogleLoginButton = loginBtnObj.GetComponent<Button>();

            // 3. Main Panel (Stretches to fill screen, uses SafeArea)
            GameObject mainPanel = CreatePanel("MainPanel", canvasObj.transform, new Color(0.05f, 0.08f, 0.15f)); // Rich Navy
            SetStretch(mainPanel);
            mainPanel.AddComponent<SafeArea>(); 
            mainPanel.SetActive(false);

            // Sidebar (Top on mobile)
            GameObject topBar = CreatePanel("TopBar", mainPanel.transform, new Color(0.1f, 0.12f, 0.2f)); // Lighter Navy
            SetRect(topBar, new Vector2(0, 0), new Vector2(0, 320), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            
            GameObject userText = CreateText("UserInfo", topBar.transform, "User Info", 35);
            userText.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
            userText.GetComponent<Text>().color = new Color(0.6f, 0.8f, 1.0f); // Soft Blue
            SetRect(userText, new Vector2(50, -40), new Vector2(600, 60), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1));

            GameObject logoutBtn = DefaultControls.CreateButton(new DefaultControls.Resources());
            logoutBtn.transform.SetParent(topBar.transform, false);
            logoutBtn.GetComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f); // Red
            logoutBtn.GetComponentInChildren<Text>().text = "Logout";
            logoutBtn.GetComponentInChildren<Text>().fontSize = 35;
            logoutBtn.GetComponentInChildren<Text>().color = Color.white;
            SetRect(logoutBtn, new Vector2(-50, -40), new Vector2(250, 80), new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1));

            GameObject newChatBtn = DefaultControls.CreateButton(new DefaultControls.Resources());
            newChatBtn.transform.SetParent(topBar.transform, false);
            newChatBtn.GetComponent<Image>().color = new Color(0.2f, 0.8f, 0.4f); // Mint Green
            newChatBtn.GetComponentInChildren<Text>().text = "+ New Chat";
            newChatBtn.GetComponentInChildren<Text>().fontSize = 35;
            newChatBtn.GetComponentInChildren<Text>().color = Color.white;
            SetRect(newChatBtn, new Vector2(50, -140), new Vector2(300, 80), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1));

            // Agent Mode Dropdown
            GameObject modeDropdownObj = DefaultControls.CreateDropdown(new DefaultControls.Resources());
            modeDropdownObj.name = "AgentModeDropdown";
            modeDropdownObj.transform.SetParent(topBar.transform, false);
            modeDropdownObj.GetComponent<Image>().color = new Color(0.3f, 0.5f, 0.9f); // Bright Blue
            Dropdown modeDropdown = modeDropdownObj.GetComponent<Dropdown>();
            modeDropdown.captionText.fontSize = 35;
            modeDropdown.captionText.color = Color.white;
            modeDropdown.itemText.fontSize = 30;
            SetRect(modeDropdownObj, new Vector2(-50, -140), new Vector2(350, 80), new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1));

            // Conversations Scroll (Horizontal for mobile)
            GameObject convScroll = DefaultControls.CreateScrollView(new DefaultControls.Resources());
            convScroll.transform.SetParent(topBar.transform, false);
            SetRect(convScroll, new Vector2(0, 10), new Vector2(0, 100), new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0));
            convScroll.GetComponent<Image>().color = Color.clear; // Hide background
            ScrollRect convScrollRect = convScroll.GetComponent<ScrollRect>();
            convScrollRect.vertical = false;
            convScrollRect.horizontal = true;
            RectTransform convContent = convScrollRect.content;
            var hl = convContent.gameObject.AddComponent<HorizontalLayoutGroup>();
            hl.childControlWidth = false;
            hl.childForceExpandWidth = false;
            hl.spacing = 20;
            hl.padding = new RectOffset(50, 50, 0, 0);

            // Chat Scroll
            GameObject chatScroll = DefaultControls.CreateScrollView(new DefaultControls.Resources());
            chatScroll.transform.SetParent(mainPanel.transform, false);
            chatScroll.GetComponent<Image>().color = Color.clear; // Hide background
            SetRect(chatScroll, new Vector2(0, -100), new Vector2(0, -570), new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f));
            chatScroll.GetComponent<RectTransform>().offsetMax = new Vector2(0, -320); // Push down below top bar
            chatScroll.GetComponent<RectTransform>().offsetMin = new Vector2(0, 250); // Pull up above input area
            
            RectTransform chatContent = chatScroll.GetComponent<ScrollRect>().content;
            var cvl = chatContent.gameObject.AddComponent<VerticalLayoutGroup>();
            cvl.childControlHeight = true;
            cvl.childForceExpandHeight = false;
            cvl.spacing = 30;
            cvl.padding = new RectOffset(40, 40, 40, 40);

            // Input Area
            GameObject inputArea = CreatePanel("InputArea", mainPanel.transform, new Color(0.1f, 0.12f, 0.2f)); // Matches top bar
            SetRect(inputArea, Vector2.zero, new Vector2(0, 250), new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0));

            GameObject promptInput = DefaultControls.CreateInputField(new DefaultControls.Resources());
            promptInput.transform.SetParent(inputArea.transform, false);
            promptInput.GetComponent<Image>().color = new Color(0.2f, 0.25f, 0.4f); // Input background
            SetRect(promptInput, new Vector2(40, 40), new Vector2(-300, -80), new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f));
            promptInput.GetComponent<RectTransform>().offsetMin = new Vector2(40, 40);
            promptInput.GetComponent<RectTransform>().offsetMax = new Vector2(-280, -40);
            Text inputText = promptInput.GetComponentInChildren<Text>();
            inputText.fontSize = 40;
            inputText.color = Color.white;
            
            var placeholder = promptInput.transform.Find("Placeholder")?.GetComponent<Text>();
            if(placeholder != null) {
                placeholder.color = new Color(0.6f, 0.6f, 0.8f);
                placeholder.text = "Type a message...";
            }

            GameObject sendBtn = DefaultControls.CreateButton(new DefaultControls.Resources());
            sendBtn.transform.SetParent(inputArea.transform, false);
            sendBtn.GetComponent<Image>().color = new Color(0.9f, 0.1f, 0.5f); // Vibrant Pink
            sendBtn.GetComponentInChildren<Text>().text = "Send";
            sendBtn.GetComponentInChildren<Text>().fontSize = 40;
            sendBtn.GetComponentInChildren<Text>().color = Color.white;
            SetRect(sendBtn, new Vector2(-40, 40), new Vector2(200, -80), new Vector2(1, 0), new Vector2(1, 1), new Vector2(1, 0.5f));
            sendBtn.GetComponent<RectTransform>().offsetMin = new Vector2(-240, 40);
            sendBtn.GetComponent<RectTransform>().offsetMax = new Vector2(-40, -40);

            // Prefabs
            GameObject convPrefab = DefaultControls.CreateButton(new DefaultControls.Resources());
            convPrefab.GetComponent<Image>().color = new Color(0.3f, 0.15f, 0.5f); // Deep Violet
            convPrefab.GetComponent<RectTransform>().sizeDelta = new Vector2(350, 80);
            Text convPrefabText = convPrefab.GetComponentInChildren<Text>();
            convPrefabText.fontSize = 35;
            convPrefabText.color = Color.white;
            convPrefab.SetActive(false); 

            GameObject msgPrefab = CreatePanel("Message", null, new Color(0.2f, 0.4f, 0.8f, 0.8f)); // Translucent Blue
            var le = msgPrefab.AddComponent<LayoutElement>();
            le.minHeight = 150;
            var msgTextObj = CreateText("MsgText", msgPrefab.transform, "Message", 40);
            msgTextObj.GetComponent<Text>().alignment = TextAnchor.UpperLeft;
            SetRect(msgTextObj, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            msgTextObj.GetComponent<RectTransform>().offsetMin = new Vector2(30, 30);
            msgTextObj.GetComponent<RectTransform>().offsetMax = new Vector2(-30, -30);
            msgPrefab.SetActive(false); 

            // Wire up UI Manager
            uiManager.MainPanel = mainPanel;
            uiManager.UserInfoText = userText.GetComponent<Text>();
            uiManager.LogoutButton = logoutBtn.GetComponent<Button>();
            uiManager.NewChatButton = newChatBtn.GetComponent<Button>();
            uiManager.AgentModeDropdown = modeDropdown; 
            uiManager.ConversationsContent = convContent;
            uiManager.ConversationButtonPrefab = convPrefab;
            uiManager.MessagesContent = chatContent;
            uiManager.MessagePrefab = msgPrefab;
            uiManager.PromptInput = promptInput.GetComponent<InputField>();
            uiManager.SendButton = sendBtn.GetComponent<Button>();

            // Organize Prefabs in Canvas
            convPrefab.transform.SetParent(canvasObj.transform, false);
            msgPrefab.transform.SetParent(canvasObj.transform, false);

            Selection.activeGameObject = canvasObj;
            Debug.Log("CortexAI Colorful Mobile UI Generated successfully!");
        }

        private static GameObject CreatePanel(string name, Transform parent, Color color)
        {
            GameObject obj = new GameObject(name);
            if (parent != null) obj.transform.SetParent(parent, false);
            Image img = obj.AddComponent<Image>();
            img.color = color;
            return obj;
        }

        private static GameObject CreateText(string name, Transform parent, string text, int fontSize)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            Text txt = obj.AddComponent<Text>();
            txt.text = text;
            txt.fontSize = fontSize;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            
            // Fix for Unity's built-in font changes in newer versions
            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont == null) defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.font = defaultFont;
            
            return obj;
        }

        private static void SetRect(GameObject obj, Vector2 pos, Vector2 size, Vector2 minAnchor = default, Vector2 maxAnchor = default, Vector2 pivot = default)
        {
            RectTransform rt = obj.GetComponent<RectTransform>();
            if (rt == null) rt = obj.AddComponent<RectTransform>();
            
            rt.anchorMin = minAnchor;
            rt.anchorMax = maxAnchor;
            rt.pivot = pivot;
            
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
        }

        private static void SetStretch(GameObject obj)
        {
            RectTransform rt = obj.GetComponent<RectTransform>();
            if (rt == null) rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
        }
    }
}
#endif
