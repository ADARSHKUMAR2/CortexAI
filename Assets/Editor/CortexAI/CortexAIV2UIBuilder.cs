#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CortexAI.Editor
{
    public class CortexAIV2UIBuilder
    {
        [MenuItem("CortexAI/Generate V2 Mobile UI (TMP)")]
        public static void GenerateUI()
        {
            // 1. Create Canvas (Portrait Mode)
            GameObject canvasObj = new GameObject("CortexAI_V2_Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f; 
            
            canvasObj.AddComponent<GraphicRaycaster>();

            CortexAIUIManager uiManager = canvasObj.AddComponent<CortexAIUIManager>();

            // Colors mapping the dark theme
            Color bgDark = new Color(0.05f, 0.05f, 0.07f);
            Color bgLighter = new Color(0.08f, 0.08f, 0.11f);
            Color accentPurple = new Color(0.45f, 0.2f, 0.9f); // New Chat Button
            Color textWhite = new Color(0.9f, 0.9f, 0.9f);
            Color textDim = new Color(0.6f, 0.6f, 0.6f);

            // -------------------------------------------------------------
            // LOGIN PANEL
            // -------------------------------------------------------------
            GameObject loginPanel = CreatePanel("LoginPanel", canvasObj.transform, new Color(0.1f, 0.05f, 0.15f)); 
            SetStretch(loginPanel);
            loginPanel.AddComponent<SafeArea>(); 
            
            GameObject loginContent = CreatePanel("LoginContent", loginPanel.transform, Color.clear);
            SetRect(loginContent, new Vector2(0, 0), new Vector2(900, 600), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            
            GameObject loginText = CreateText("Title", loginContent.transform, "Login to CortexAI", 75);
            SetRect(loginText, new Vector2(0, 200), new Vector2(800, 100));
            
            GameObject tokenInputObj = CreateTMPInputField("TokenInput", loginContent.transform, "Paste Token here (Editor only)...", 35, new Color(0.2f, 0.2f, 0.3f));
            SetRect(tokenInputObj, new Vector2(0, 50), new Vector2(800, 100));
            
            GameObject loginBtnObj = CreateTMPButton("LoginButton", loginContent.transform, "Authenticate", 45, accentPurple);
            SetRect(loginBtnObj, new Vector2(0, -100), new Vector2(600, 120));

            uiManager.LoginPanel = loginPanel;
            uiManager.TokenInput = tokenInputObj.GetComponent<TMP_InputField>();
            uiManager.GoogleLoginButton = loginBtnObj.GetComponent<Button>();

            // -------------------------------------------------------------
            // MAIN PANEL (Chat Area)
            // -------------------------------------------------------------
            GameObject mainPanel = CreatePanel("MainPanel", canvasObj.transform, bgDark);
            SetStretch(mainPanel);
            mainPanel.AddComponent<SafeArea>(); 
            mainPanel.SetActive(false);

            // Main Top Bar
            GameObject mainTopBar = CreatePanel("MainTopBar", mainPanel.transform, bgDark);
            SetRect(mainTopBar, new Vector2(0, 0), new Vector2(0, 150), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            
            // Hamburger Button
            GameObject hamburgerBtn = CreateTMPButton("HamburgerBtn", mainTopBar.transform, "≡", 80, Color.clear);
            hamburgerBtn.GetComponentInChildren<TextMeshProUGUI>().color = textWhite;
            SetRect(hamburgerBtn, new Vector2(40, -40), new Vector2(100, 100), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1));

            // Main Title
            GameObject mainTitle = CreateText("Title", mainTopBar.transform, "CortexAI", 50);
            mainTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
            SetRect(mainTitle, new Vector2(160, -40), new Vector2(400, 100), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1));
            
            // Agent Mode Dropdown
            GameObject modeDropdownObj = CreateTMPDropdown("AgentModeDropdown", mainTopBar.transform, bgLighter);
            SetRect(modeDropdownObj, new Vector2(-40, -40), new Vector2(350, 80), new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1));

            // Chat Scroll
            GameObject chatScroll = DefaultControls.CreateScrollView(new DefaultControls.Resources());
            chatScroll.name = "ChatScroll";
            chatScroll.transform.SetParent(mainPanel.transform, false);
            chatScroll.GetComponent<Image>().color = Color.clear;
            SetRect(chatScroll, Vector2.zero, Vector2.zero, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f));
            chatScroll.GetComponent<RectTransform>().offsetMax = new Vector2(0, -150); 
            chatScroll.GetComponent<RectTransform>().offsetMin = new Vector2(0, 250);  
            
            RectTransform chatContent = chatScroll.GetComponent<ScrollRect>().content;
            var cvl = chatContent.gameObject.AddComponent<VerticalLayoutGroup>();
            cvl.childControlHeight = true; cvl.childForceExpandHeight = false;
            cvl.spacing = 30; cvl.padding = new RectOffset(40, 40, 40, 40);

            // Input Area
            GameObject inputArea = CreatePanel("InputArea", mainPanel.transform, bgDark);
            SetRect(inputArea, Vector2.zero, new Vector2(0, 250), new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0));

            GameObject promptInput = CreateTMPInputField("PromptInput", inputArea.transform, "Type a message...", 40, bgLighter);
            SetRect(promptInput, new Vector2(40, 40), new Vector2(-250, -80), new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f));
            promptInput.GetComponent<RectTransform>().offsetMin = new Vector2(40, 40);
            promptInput.GetComponent<RectTransform>().offsetMax = new Vector2(-220, -40);
            
            GameObject sendBtn = CreateTMPButton("SendBtn", inputArea.transform, "↑", 60, accentPurple);
            SetRect(sendBtn, new Vector2(-40, 40), new Vector2(140, -80), new Vector2(1, 0), new Vector2(1, 1), new Vector2(1, 0.5f));
            sendBtn.GetComponent<RectTransform>().offsetMin = new Vector2(-180, 40);
            sendBtn.GetComponent<RectTransform>().offsetMax = new Vector2(-40, -40);

            // -------------------------------------------------------------
            // SIDE PANEL (Drawer)
            // -------------------------------------------------------------
            GameObject sidePanel = CreatePanel("SidePanel", canvasObj.transform, bgDark);
            // Anchor to left side, stretching from 0 to 0.9 in width (90% of screen)
            SetRect(sidePanel, Vector2.zero, Vector2.zero, new Vector2(0, 0), new Vector2(0.85f, 1), new Vector2(0, 0.5f));
            sidePanel.AddComponent<SafeArea>();
            sidePanel.SetActive(false); 

            // Sidebar Header (Top left)
            GameObject sideHeader = CreatePanel("Header", sidePanel.transform, Color.clear);
            SetRect(sideHeader, new Vector2(0, 0), new Vector2(0, 150), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            
            GameObject closeSideBtn = CreateTMPButton("CloseSidebarBtn", sideHeader.transform, "▣", 60, Color.clear);
            closeSideBtn.GetComponentInChildren<TextMeshProUGUI>().color = textDim;
            SetRect(closeSideBtn, new Vector2(40, -40), new Vector2(80, 80), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1));

            GameObject sideTitle = CreateText("Title", sideHeader.transform, "CortexAI", 50);
            var stText = sideTitle.GetComponent<TextMeshProUGUI>();
            stText.alignment = TextAlignmentOptions.Left;
            stText.fontStyle = FontStyles.Bold;
            SetRect(sideTitle, new Vector2(140, -40), new Vector2(250, 80), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1));

            GameObject planPill = CreatePanel("PlanPill", sideHeader.transform, new Color(0.15f, 0.15f, 0.25f));
            SetRect(planPill, new Vector2(-40, -40), new Vector2(160, 60), new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1));
            GameObject planText = CreateText("Text", planPill.transform, "starter", 30);
            planText.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 0.4f, 1.0f);
            SetStretch(planText);

            // Big New Chat Button
            GameObject newChatBtn = CreateTMPButton("NewChatBtn", sidePanel.transform, "+ New Chat", 45, accentPurple);
            SetRect(newChatBtn, new Vector2(40, -180), new Vector2(-80, 120), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            newChatBtn.GetComponent<RectTransform>().offsetMin = new Vector2(40, -300);
            newChatBtn.GetComponent<RectTransform>().offsetMax = new Vector2(-40, -180);

            // Recents Label
            GameObject recentsLabel = CreateText("RecentsLabel", sidePanel.transform, "RECENTS", 30);
            var rlText = recentsLabel.GetComponent<TextMeshProUGUI>();
            rlText.alignment = TextAlignmentOptions.Left;
            rlText.color = textDim;
            rlText.fontStyle = FontStyles.Bold;
            SetRect(recentsLabel, new Vector2(40, -350), new Vector2(400, 50), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1));

            // Recents Scroll View
            GameObject convScroll = DefaultControls.CreateScrollView(new DefaultControls.Resources());
            convScroll.name = "RecentsScroll";
            convScroll.transform.SetParent(sidePanel.transform, false);
            convScroll.GetComponent<Image>().color = Color.clear; 
            SetRect(convScroll, Vector2.zero, Vector2.zero, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f));
            convScroll.GetComponent<RectTransform>().offsetMax = new Vector2(0, -420); 
            convScroll.GetComponent<RectTransform>().offsetMin = new Vector2(0, 250);  

            RectTransform convContent = convScroll.GetComponent<ScrollRect>().content;
            // Stretch content horizontally to fill viewport width
            convContent.anchorMin = new Vector2(0, 1);
            convContent.anchorMax = new Vector2(1, 1);
            convContent.pivot = new Vector2(0.5f, 1);
            convContent.offsetMin = new Vector2(0, convContent.offsetMin.y);
            convContent.offsetMax = new Vector2(0, convContent.offsetMax.y);

            var cvl2 = convContent.gameObject.AddComponent<VerticalLayoutGroup>();
            cvl2.childControlWidth = true; cvl2.childForceExpandWidth = true; // Fixes crushed width
            cvl2.childControlHeight = true; cvl2.childForceExpandHeight = false;
            cvl2.spacing = 20; cvl2.padding = new RectOffset(40, 40, 10, 10);
            
            var csf = convContent.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Bottom Profile Area
            GameObject profileArea = CreatePanel("ProfileArea", sidePanel.transform, Color.clear);
            SetRect(profileArea, Vector2.zero, new Vector2(0, 220), new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0));
            
            GameObject sep = CreatePanel("Separator", profileArea.transform, new Color(0.2f, 0.2f, 0.2f));
            SetRect(sep, new Vector2(0, 218), new Vector2(0, 2), new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0));

            // Avatar
            GameObject avatar = CreatePanel("Avatar", profileArea.transform, new Color(0.15f, 0.15f, 0.18f));
            SetRect(avatar, new Vector2(40, 50), new Vector2(120, 120), new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0));
            CreateText("Icon", avatar.transform, "👤", 60);

            // User Info Name & Tier
            GameObject userText = CreateText("UserInfo", profileArea.transform, "Adarsh Kumar\n<color=#888>starter Tier</color>", 40);
            var uTxt = userText.GetComponent<TextMeshProUGUI>();
            uTxt.alignment = TextAlignmentOptions.Left;
            uTxt.richText = true;
            SetRect(userText, new Vector2(180, 50), new Vector2(350, 120), new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0));

            // Subscriptions/Link Button
            GameObject subBtn = CreateTMPButton("SubBtn", profileArea.transform, "🔗", 50, Color.clear);
            subBtn.GetComponentInChildren<TextMeshProUGUI>().color = new Color(0.8f, 0.6f, 0.1f);
            SetRect(subBtn, new Vector2(-150, 50), new Vector2(80, 120), new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0));

            // Logout Button
            GameObject logoutBtn = CreateTMPButton("LogoutBtn", profileArea.transform, "→", 60, Color.clear);
            logoutBtn.GetComponentInChildren<TextMeshProUGUI>().color = textDim;
            SetRect(logoutBtn, new Vector2(-50, 50), new Vector2(80, 120), new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0));


            // -------------------------------------------------------------
            // PREFABS
            // -------------------------------------------------------------
            GameObject convPrefab = CreateTMPButton("ConvItemPrefab", null, "Item", 35, bgLighter);
            var cpRt = convPrefab.GetComponent<RectTransform>();
            cpRt.sizeDelta = new Vector2(0, 100);
            
            // Add LayoutElement to prevent VerticalLayoutGroup from crushing it to 0 height
            var cpLayout = convPrefab.AddComponent<LayoutElement>();
            cpLayout.minHeight = 100;
            
            // Re-style the text and icon
            var convTextObj = convPrefab.transform.Find("Text").gameObject;
            var ct = convTextObj.GetComponent<TextMeshProUGUI>();
            ct.alignment = TextAlignmentOptions.Left;
            SetRect(convTextObj, new Vector2(110, 0), new Vector2(-120, 0), new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f));
            convTextObj.GetComponent<RectTransform>().offsetMin = new Vector2(110, 0);
            convTextObj.GetComponent<RectTransform>().offsetMax = new Vector2(-10, 0);
            
            GameObject cIcon = CreateText("Icon", convPrefab.transform, "💬", 35);
            cIcon.GetComponent<TextMeshProUGUI>().color = textDim;
            SetRect(cIcon, new Vector2(30, 0), new Vector2(60, 60), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f));
            
            convPrefab.SetActive(false); 

            GameObject msgPrefab = CreatePanel("Message", null, new Color(0.15f, 0.15f, 0.2f, 1f)); 
            var le = msgPrefab.AddComponent<LayoutElement>();
            le.minHeight = 150;
            var msgTextObj = CreateText("MsgText", msgPrefab.transform, "Message", 40);
            var mt = msgTextObj.GetComponent<TextMeshProUGUI>();
            mt.alignment = TextAlignmentOptions.TopLeft;
            mt.richText = true;
            SetRect(msgTextObj, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            msgTextObj.GetComponent<RectTransform>().offsetMin = new Vector2(30, 30);
            msgTextObj.GetComponent<RectTransform>().offsetMax = new Vector2(-30, -30);
            msgPrefab.SetActive(false); 

            // -------------------------------------------------------------
            // WIRE UP UIMANAGER
            // -------------------------------------------------------------
            uiManager.MainPanel = mainPanel;
            uiManager.UserInfoText = uTxt;
            uiManager.LogoutButton = logoutBtn.GetComponent<Button>();
            uiManager.NewChatButton = newChatBtn.GetComponent<Button>();
            uiManager.AgentModeDropdown = modeDropdownObj.GetComponent<TMP_Dropdown>(); 
            
            uiManager.SidePanel = sidePanel;
            uiManager.OpenSidebarButton = hamburgerBtn.GetComponent<Button>();
            uiManager.CloseSidebarButton = closeSideBtn.GetComponent<Button>();
            uiManager.SubscriptionButton = subBtn.GetComponent<Button>();

            uiManager.ConversationsContent = convContent;
            uiManager.ConversationButtonPrefab = convPrefab;
            uiManager.MessagesContent = chatContent;
            uiManager.MessagePrefab = msgPrefab;
            uiManager.PromptInput = promptInput.GetComponent<TMP_InputField>();
            uiManager.SendButton = sendBtn.GetComponent<Button>();

            // Organize Prefabs in Canvas
            convPrefab.transform.SetParent(canvasObj.transform, false);
            msgPrefab.transform.SetParent(canvasObj.transform, false);

            Selection.activeGameObject = canvasObj;
            Debug.Log("CortexAI V2 Side-Panel Mobile UI (TextMeshPro) Generated successfully!");
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
            if (parent != null) obj.transform.SetParent(parent, false);
            TextMeshProUGUI txt = obj.AddComponent<TextMeshProUGUI>();
            txt.text = text;
            txt.fontSize = fontSize;
            txt.alignment = TextAlignmentOptions.Center;
            txt.color = Color.white;
            return obj;
        }

        private static GameObject CreateTMPButton(string name, Transform parent, string text, int fontSize, Color color)
        {
            GameObject btnObj = new GameObject(name);
            if (parent != null) btnObj.transform.SetParent(parent, false);
            Image img = btnObj.AddComponent<Image>();
            img.color = color;
            btnObj.AddComponent<Button>();
            
            GameObject txtObj = CreateText("Text", btnObj.transform, text, fontSize);
            SetStretch(txtObj);
            return btnObj;
        }

        private static GameObject CreateTMPInputField(string name, Transform parent, string placeholderText, int fontSize, Color bgColor)
        {
            GameObject inputObj = new GameObject(name);
            if (parent != null) inputObj.transform.SetParent(parent, false);
            Image img = inputObj.AddComponent<Image>();
            img.color = bgColor;
            
            GameObject textArea = new GameObject("TextArea");
            textArea.transform.SetParent(inputObj.transform, false);
            textArea.AddComponent<RectMask2D>();
            SetStretch(textArea);
            var textAreaRt = textArea.GetComponent<RectTransform>();
            textAreaRt.offsetMin = new Vector2(20, 10);
            textAreaRt.offsetMax = new Vector2(-20, -10);

            GameObject placeholder = CreateText("Placeholder", textArea.transform, placeholderText, fontSize);
            var pText = placeholder.GetComponent<TextMeshProUGUI>();
            pText.color = new Color(0.6f, 0.6f, 0.8f, 0.5f);
            pText.alignment = TextAlignmentOptions.Left;
            pText.textWrappingMode = TextWrappingModes.NoWrap;
            SetStretch(placeholder);

            GameObject textContent = CreateText("Text", textArea.transform, "", fontSize);
            var tText = textContent.GetComponent<TextMeshProUGUI>();
            tText.alignment = TextAlignmentOptions.Left;
            tText.textWrappingMode = TextWrappingModes.NoWrap;
            SetStretch(textContent);

            TMP_InputField input = inputObj.AddComponent<TMP_InputField>();
            input.textViewport = textAreaRt;
            input.textComponent = tText;
            input.placeholder = pText;

            return inputObj;
        }

        private static GameObject CreateTMPDropdown(string name, Transform parent, Color bgColor)
        {
            GameObject dropdownObj = new GameObject(name);
            if (parent != null) dropdownObj.transform.SetParent(parent, false);
            Image img = dropdownObj.AddComponent<Image>();
            img.color = bgColor;
            
            TMP_Dropdown dropdown = dropdownObj.AddComponent<TMP_Dropdown>();
            
            // Main Label
            GameObject label = CreateText("Label", dropdownObj.transform, "Option A", 35);
            var lText = label.GetComponent<TextMeshProUGUI>();
            lText.alignment = TextAlignmentOptions.MidlineLeft;
            SetRect(label, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            label.GetComponent<RectTransform>().offsetMin = new Vector2(20, 0);
            label.GetComponent<RectTransform>().offsetMax = new Vector2(-60, 0);
            
            dropdown.captionText = lText;

            // Arrow
            GameObject arrow = CreateText("Arrow", dropdownObj.transform, "▼", 25);
            var aText = arrow.GetComponent<TextMeshProUGUI>();
            aText.alignment = TextAlignmentOptions.Center;
            SetRect(arrow, new Vector2(-10, 0), new Vector2(40, 40), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f));
            
            // Dropdown Template
            GameObject template = new GameObject("Template");
            template.transform.SetParent(dropdownObj.transform, false);
            template.SetActive(false);
            var templateImg = template.AddComponent<Image>();
            templateImg.color = new Color(0.1f, 0.1f, 0.15f);
            var scrollRect = template.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            SetRect(template, new Vector2(0, -2), new Vector2(0, 300), new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 1));
            
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(template.transform, false);
            var viewportImg = viewport.AddComponent<Image>();
            viewportImg.color = Color.white; // Solid white for mask
            var mask = viewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;
            SetStretch(viewport);
            
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            SetRect(content, Vector2.zero, new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            
            GameObject item = new GameObject("Item");
            item.transform.SetParent(content.transform, false);
            var toggle = item.AddComponent<Toggle>();
            // Item MUST have anchor middle stretch for TMP_Dropdown to calculate height correctly
            SetRect(item, Vector2.zero, new Vector2(0, 50), new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(0.5f, 0.5f));
            
            GameObject itemBackground = new GameObject("Item Background");
            itemBackground.transform.SetParent(item.transform, false);
            var itemBgImg = itemBackground.AddComponent<Image>();
            itemBgImg.color = Color.white; // Base color white for tinting
            SetStretch(itemBackground);
            
            GameObject itemLabel = CreateText("Item Label", item.transform, "Option", 30);
            var ilText = itemLabel.GetComponent<TextMeshProUGUI>();
            ilText.alignment = TextAlignmentOptions.MidlineLeft;
            ilText.color = Color.white;
            SetStretch(itemLabel);
            itemLabel.GetComponent<RectTransform>().offsetMin = new Vector2(20, 0);

            // Configure Toggle Colors for selection state
            toggle.targetGraphic = itemBgImg;
            ColorBlock cb = toggle.colors;
            cb.normalColor = new Color(0.1f, 0.1f, 0.15f); 
            cb.highlightedColor = new Color(0.25f, 0.25f, 0.35f);
            cb.pressedColor = new Color(0.3f, 0.3f, 0.4f);
            cb.selectedColor = new Color(0.25f, 0.25f, 0.35f);
            cb.colorMultiplier = 1f;
            toggle.colors = cb;
            
            scrollRect.content = content.GetComponent<RectTransform>();
            scrollRect.viewport = viewport.GetComponent<RectTransform>();
            
            dropdown.template = template.GetComponent<RectTransform>();
            dropdown.itemText = ilText;

            return dropdownObj;
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