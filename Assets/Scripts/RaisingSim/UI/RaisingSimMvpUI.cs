using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using RaisingSim;
using RaisingSim.Core;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace RaisingSim.UI
{
    public class RaisingSimMvpUI : MonoBehaviour
    {
        private const string CanvasName = "RaisingSimMvpCanvas";
        private const string ActionContentName = "ActionButtonContent";

        private static readonly string[] StatOrder =
        {
            StatManager.Money,
            StatManager.Intelligence,
            StatManager.Elegance,
            StatManager.Charm,
            StatManager.Reputation,
            StatManager.Stress,
            StatManager.Pride,
            StatManager.MotherHealth
        };

        [Header("Main")]
        public Text dateText;
        public Text statsText;
        public Transform actionButtonContainer;
        public Text logText;

        [Header("Popup")]
        public GameObject popupPanel;
        public Text popupTitleText;
        public Text popupBodyText;
        public Button popupContinueButton;

        private GameDataSet data;
        private RaisingSimGameController controller;
        private readonly List<Button> actionButtons = new List<Button>();
        private readonly List<ActionData> buttonActions = new List<ActionData>();
        private readonly List<PopupMessage> popupQueue = new List<PopupMessage>();
        private bool festivalRan;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void BootstrapIfMissing()
        {
            if (FindFirst<RaisingSimMvpUI>() != null)
            {
                return;
            }

            GameObject uiObject = new GameObject("RaisingSimMvpUI");
            RaisingSimMvpUI ui = uiObject.AddComponent<RaisingSimMvpUI>();
            CreateDefaultUi(ui);
            Debug.Log("[RaisingSimMvpUI] Runtime MVP UI was created because no RaisingSimMvpUI existed in the scene.");
        }

        private void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
            ResolveMissingReferences();
            EnsureEventSystemInputModule();

            data = GameDataLoader.LoadAll();
            controller = new RaisingSimGameController(data);
            festivalRan = false;

            if (popupContinueButton != null)
            {
                popupContinueButton.onClick.RemoveListener(ContinuePopup);
                popupContinueButton.onClick.AddListener(ContinuePopup);
            }

            HidePopup();
            BuildActionButtons();
            AppendLog("MVP raising simulation started.");
            RefreshAll();
        }

        private void ResolveMissingReferences()
        {
            if (dateText == null)
            {
                dateText = FindText("DateText");
            }

            if (statsText == null)
            {
                statsText = FindText("StatsText");
            }

            if (actionButtonContainer == null)
            {
                GameObject content = GameObject.Find(ActionContentName);
                if (content == null)
                {
                    content = GameObject.Find("Content");
                }

                if (content != null)
                {
                    actionButtonContainer = content.transform;
                }
            }

            if (logText == null)
            {
                logText = FindText("LogText");
            }

            if (popupPanel == null)
            {
                popupPanel = GameObject.Find("PopupPanel");
            }

            if (popupTitleText == null)
            {
                popupTitleText = FindText("PopupTitle");
            }

            if (popupBodyText == null)
            {
                popupBodyText = FindText("PopupBody");
            }

            if (popupContinueButton == null)
            {
                GameObject continueButton = GameObject.Find("ContinueButton");
                if (continueButton != null)
                {
                    popupContinueButton = continueButton.GetComponent<Button>();
                }
            }

            if (dateText == null || statsText == null || actionButtonContainer == null || logText == null)
            {
                Debug.LogWarning("[RaisingSimMvpUI] UI references were missing. Creating default MVP UI at runtime.");
                CreateDefaultUi(this);
            }
        }

        private Text FindText(string objectName)
        {
            GameObject found = GameObject.Find(objectName);
            return found == null ? null : found.GetComponent<Text>();
        }

        private void EnsureEventSystemInputModule()
        {
            EventSystem eventSystem = FindFirst<EventSystem>();
            if (eventSystem == null)
            {
                eventSystem = CreateEventSystem();
            }

            if (eventSystem == null)
            {
                return;
            }

            if (eventSystem.GetComponent<BaseInputModule>() == null)
            {
#if ENABLE_INPUT_SYSTEM
                eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
#else
                eventSystem.gameObject.AddComponent<StandaloneInputModule>();
#endif
            }
        }

        private static EventSystem CreateEventSystem()
        {
            GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            eventSystemObject.AddComponent<InputSystemUIInputModule>();
#else
            eventSystemObject.AddComponent<StandaloneInputModule>();
#endif
            return eventSystemObject.GetComponent<EventSystem>();
        }

        private static T FindFirst<T>() where T : Object
        {
            return Object.FindFirstObjectByType<T>();
        }

        private static void CreateDefaultUi(RaisingSimMvpUI ui)
        {
            Canvas canvas = CreateCanvas();
            CreateEventSystemIfMissing();

            ui.dateText = CreateTopArea(canvas.transform);
            ui.statsText = CreateStatsArea(canvas.transform);
            ui.actionButtonContainer = CreateActionArea(canvas.transform);
            ui.logText = CreateLogArea(canvas.transform);
            PopupRefs popupRefs = CreatePopup(canvas.transform);
            ui.popupPanel = popupRefs.panel;
            ui.popupTitleText = popupRefs.titleText;
            ui.popupBodyText = popupRefs.bodyText;
            ui.popupContinueButton = popupRefs.continueButton;
        }

        private static Canvas CreateCanvas()
        {
            GameObject existing = GameObject.Find(CanvasName);
            if (existing != null)
            {
                Canvas existingCanvas = existing.GetComponent<Canvas>();
                if (existingCanvas != null)
                {
                    return existingCanvas;
                }
            }

            GameObject canvasObject = new GameObject(CanvasName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);

            return canvas;
        }

        private static void CreateEventSystemIfMissing()
        {
            if (FindFirst<EventSystem>() == null)
            {
                CreateEventSystem();
            }
        }

        private static Text CreateTopArea(Transform parent)
        {
            GameObject panel = CreatePanel("TopArea", parent, new Color(0.15f, 0.15f, 0.15f, 1f));
            Stretch(panel.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -60f), Vector2.zero);

            Text text = CreateText("DateText", panel.transform, "Year 1 / spring / Actions 0/6", 22, TextAnchor.MiddleLeft);
            Stretch(text.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(18f, 0f), new Vector2(-18f, 0f));
            text.color = Color.white;
            return text;
        }

        private static Text CreateStatsArea(Transform parent)
        {
            GameObject panel = CreatePanel("LeftStatsArea", parent, new Color(0.93f, 0.93f, 0.93f, 1f));
            Stretch(panel.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 170f), new Vector2(260f, -60f));

            Text title = CreateText("StatsTitle", panel.transform, "Stats", 20, TextAnchor.UpperLeft);
            Stretch(title.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(12f, -42f), new Vector2(-12f, -10f));

            Text stats = CreateText("StatsText", panel.transform, "", 16, TextAnchor.UpperLeft);
            Stretch(stats.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(12f, 12f), new Vector2(-12f, -52f));
            return stats;
        }

        private static Transform CreateActionArea(Transform parent)
        {
            GameObject panel = CreatePanel("RightActionArea", parent, new Color(0.88f, 0.90f, 0.93f, 1f));
            Stretch(panel.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-360f, 170f), new Vector2(0f, -60f));

            Text title = CreateText("ActionsTitle", panel.transform, "Actions", 20, TextAnchor.UpperLeft);
            Stretch(title.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(12f, -42f), new Vector2(-12f, -10f));

            GameObject scrollObject = new GameObject("ActionScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            scrollObject.transform.SetParent(panel.transform, false);
            scrollObject.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.35f);
            Stretch(scrollObject.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(10f, 10f), new Vector2(-10f, -52f));

            GameObject viewportObject = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewportObject.transform.SetParent(scrollObject.transform, false);
            viewportObject.GetComponent<Image>().color = Color.white;
            viewportObject.GetComponent<Mask>().showMaskGraphic = false;
            Stretch(viewportObject.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            GameObject contentObject = new GameObject(ActionContentName, typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentObject.transform.SetParent(viewportObject.transform, false);

            RectTransform contentRect = contentObject.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.offsetMin = new Vector2(8f, 0f);
            contentRect.offsetMax = new Vector2(-8f, 0f);

            VerticalLayoutGroup layout = contentObject.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 6f;
            layout.padding = new RectOffset(0, 0, 8, 8);
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            ContentSizeFitter fitter = contentObject.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ScrollRect scrollRect = scrollObject.GetComponent<ScrollRect>();
            scrollRect.viewport = viewportObject.GetComponent<RectTransform>();
            scrollRect.content = contentRect;
            scrollRect.horizontal = false;

            return contentObject.transform;
        }

        private static Text CreateLogArea(Transform parent)
        {
            GameObject panel = CreatePanel("BottomLogArea", parent, new Color(0.1f, 0.1f, 0.1f, 1f));
            Stretch(panel.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(1f, 0f), Vector2.zero, new Vector2(0f, 170f));

            Text text = CreateText("LogText", panel.transform, "", 14, TextAnchor.UpperLeft);
            Stretch(text.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(12f, 8f), new Vector2(-12f, -8f));
            text.color = Color.white;
            return text;
        }

        private static PopupRefs CreatePopup(Transform parent)
        {
            GameObject panel = CreatePanel("PopupPanel", parent, new Color(0.96f, 0.96f, 0.96f, 1f));
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(560f, 330f);
            rect.anchoredPosition = Vector2.zero;

            Text title = CreateText("PopupTitle", panel.transform, "Event", 22, TextAnchor.UpperLeft);
            Stretch(title.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(18f, -54f), new Vector2(-18f, -12f));

            Text body = CreateText("PopupBody", panel.transform, "", 16, TextAnchor.UpperLeft);
            Stretch(body.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(18f, 70f), new Vector2(-18f, -64f));

            Button button = CreateRuntimeButton("ContinueButton", panel.transform, "Continue");
            RectTransform buttonRect = button.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(1f, 0f);
            buttonRect.anchorMax = new Vector2(1f, 0f);
            buttonRect.pivot = new Vector2(1f, 0f);
            buttonRect.sizeDelta = new Vector2(140f, 42f);
            buttonRect.anchoredPosition = new Vector2(-18f, 18f);

            panel.SetActive(false);
            return new PopupRefs { panel = panel, titleText = title, bodyText = body, continueButton = button };
        }

        private static GameObject CreatePanel(string name, Transform parent, Color color)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            panel.GetComponent<Image>().color = color;
            return panel;
        }

        private static Text CreateText(string name, Transform parent, string value, int fontSize, TextAnchor alignment)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);

            Text text = textObject.GetComponent<Text>();
            text.font = GetRuntimeFont();
            text.text = value;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.black;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static Font GetRuntimeFont()
        {
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        private static Button CreateRuntimeButton(string name, Transform parent, string label)
        {
            GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            buttonObject.GetComponent<Image>().color = new Color(0.8f, 0.86f, 0.95f, 1f);

            Text text = CreateText("Text", buttonObject.transform, label, 16, TextAnchor.MiddleCenter);
            Stretch(text.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return buttonObject.GetComponent<Button>();
        }

        private static void Stretch(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private void BuildActionButtons()
        {
            actionButtons.Clear();
            buttonActions.Clear();

            if (actionButtonContainer == null)
            {
                Debug.LogError("[RaisingSimMvpUI] actionButtonContainer is not assigned.");
                return;
            }

            for (int i = actionButtonContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(actionButtonContainer.GetChild(i).gameObject);
            }

            if (data == null || data.actions == null || data.actions.actions == null)
            {
                Debug.LogError("[RaisingSimMvpUI] Action data is empty.");
                return;
            }

            Debug.Log($"[RaisingSimMvpUI] Loaded actions: {data.actions.actions.Length}");

            for (int i = 0; i < data.actions.actions.Length; i++)
            {
                ActionData action = data.actions.actions[i];
                if (action == null)
                {
                    continue;
                }

                Button button = CreateActionButton(action);
                actionButtons.Add(button);
                buttonActions.Add(action);
            }

            RectTransform containerRect = actionButtonContainer as RectTransform;
            if (containerRect != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);
            }

            Debug.Log($"[RaisingSimMvpUI] Created action buttons: {actionButtons.Count}");
        }

        private Button CreateActionButton(ActionData action)
        {
            GameObject buttonObject = new GameObject(action.action_id, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            buttonObject.transform.SetParent(actionButtonContainer, false);

            RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(1f, 1f);
            rectTransform.pivot = new Vector2(0.5f, 1f);
            rectTransform.sizeDelta = new Vector2(0f, 36f);

            LayoutElement layoutElement = buttonObject.GetComponent<LayoutElement>();
            layoutElement.minHeight = 36f;
            layoutElement.preferredHeight = 36f;
            layoutElement.flexibleHeight = 0f;

            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.92f, 0.92f, 0.92f, 1f);

            Button button = buttonObject.GetComponent<Button>();

            GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(buttonObject.transform, false);

            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(8f, 2f);
            textRect.offsetMax = new Vector2(-8f, -2f);

            Text text = textObject.GetComponent<Text>();
            text.font = GetRuntimeFont();
            text.fontSize = 14;
            text.alignment = TextAnchor.MiddleLeft;
            text.color = Color.black;
            text.text = $"{action.name} ({action.time})";

            string capturedActionId = action.action_id;
            button.onClick.AddListener(() => OnActionButtonClicked(capturedActionId));

            return button;
        }

        private void OnActionButtonClicked(string actionId)
        {
            if (controller == null)
            {
                Debug.LogError("[RaisingSimMvpUI] Game controller is not initialized.");
                return;
            }

            GameplayActionResult result = controller.RunActionDetailed(actionId);
            if (!result.success)
            {
                Debug.LogError($"[RaisingSimMvpUI] Action failed: {result.errorMessage}");
                AppendLog($"Action failed: {result.errorMessage}");
                RefreshAll();
                return;
            }

            ActionData action = result.actionResult.action;
            AppendLog($"Action: {action.action_id} / {action.name}");
            AppendLog($"Changes: {result.actionResult.statChangesText}");
            if (!string.IsNullOrWhiteSpace(action.result_text))
            {
                AppendLog(action.result_text);
            }

            if (result.instantEndingEvent != null)
            {
                AppendEvent(result.instantEndingEvent, "Instant Ending");
            }
            else if (result.afterActionEvent != null)
            {
                AppendEvent(result.afterActionEvent, "Event");
            }

            RefreshAll();

            if (!controller.IsEnded && controller.PlayerState.actionCount >= 6 && !festivalRan)
            {
                RunFestival();
            }
        }

        private void RunFestival()
        {
            festivalRan = true;
            SetActionButtonsInteractable(false);

            FestivalRunResult festivalResult = controller.RunSpringFestivalIfReadyDetailed();
            if (!festivalResult.success)
            {
                Debug.LogError($"[RaisingSimMvpUI] Festival failed: {festivalResult.errorMessage}");
                AppendLog($"Festival failed: {festivalResult.errorMessage}");
                RefreshAll();
                return;
            }

            string rewardName = festivalResult.reward != null ? festivalResult.reward.result_name : "";
            string rewardDelta = festivalResult.reward != null ? festivalResult.reward.delta : "";

            AppendLog($"Festival score: {festivalResult.score}");
            AppendLog($"Festival result: {rewardName}");
            AppendLog($"Festival reward delta: {rewardDelta}");

            string popupBody =
                $"Score: {festivalResult.score}\n" +
                $"Result: {rewardName}\n" +
                $"Reward: {rewardDelta}";

            if (festivalResult.resultEvent != null)
            {
                popupBody += "\n\n" + festivalResult.resultEvent.dialogue_text;
            }

            QueuePopup("Festival Result", popupBody);
            AppendLog("Final stats:");
            AppendLog(BuildStatsText());
            RefreshAll();
        }

        private void AppendEvent(EventData eventData, string title)
        {
            AppendLog($"{title}: {eventData.event_id} / {eventData.name}");
            if (!string.IsNullOrWhiteSpace(eventData.dialogue_text))
            {
                AppendLog(eventData.dialogue_text);
            }

            QueuePopup(title, $"{eventData.name}\n\n{eventData.dialogue_text}");
        }

        private void RefreshAll()
        {
            RefreshHeader();
            RefreshStats();
            RefreshActionButtons();
        }

        private void RefreshHeader()
        {
            if (dateText == null || controller == null)
            {
                return;
            }

            PlayerState state = controller.PlayerState;
            dateText.text = $"Year {state.year} / {state.season} / Actions {state.actionCount}/6";
        }

        private void RefreshStats()
        {
            if (statsText == null || controller == null)
            {
                return;
            }

            statsText.text = BuildStatsText();
        }

        private string BuildStatsText()
        {
            if (controller == null)
            {
                return "";
            }

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < StatOrder.Length; i++)
            {
                string stat = StatOrder[i];
                builder.Append(stat);
                builder.Append(": ");
                builder.Append(controller.PlayerState.GetStat(stat));

                if (i < StatOrder.Length - 1)
                {
                    builder.AppendLine();
                }
            }

            return builder.ToString();
        }

        private void RefreshActionButtons()
        {
            bool canAct = controller != null && !controller.IsEnded && !festivalRan && controller.PlayerState.actionCount < 6;

            if (!canAct)
            {
                SetActionButtonsInteractable(false);
                return;
            }

            if (data == null || data.actions == null || data.actions.actions == null)
            {
                SetActionButtonsInteractable(false);
                return;
            }

            for (int i = 0; i < actionButtons.Count && i < buttonActions.Count; i++)
            {
                ActionData action = buttonActions[i];
                bool isUnlocked = action != null && ConditionEvaluator.Evaluate(action.unlock_condition, controller.PlayerState);
                actionButtons[i].interactable = isUnlocked;
            }
        }

        private void SetActionButtonsInteractable(bool interactable)
        {
            for (int i = 0; i < actionButtons.Count; i++)
            {
                if (actionButtons[i] != null)
                {
                    actionButtons[i].interactable = interactable;
                }
            }
        }

        private void AppendLog(string message)
        {
            if (logText == null)
            {
                Debug.Log(message);
                return;
            }

            if (string.IsNullOrEmpty(logText.text))
            {
                logText.text = message;
            }
            else
            {
                logText.text += "\n" + message;
            }
        }

        private void QueuePopup(string title, string body)
        {
            popupQueue.Add(new PopupMessage { title = title, body = body });
            if (popupPanel == null || !popupPanel.activeSelf)
            {
                ShowNextPopup();
            }
        }

        private void ContinuePopup()
        {
            ShowNextPopup();
        }

        private void ShowNextPopup()
        {
            if (popupQueue.Count == 0)
            {
                HidePopup();
                return;
            }

            PopupMessage message = popupQueue[0];
            popupQueue.RemoveAt(0);

            if (popupPanel != null)
            {
                popupPanel.SetActive(true);
            }

            if (popupTitleText != null)
            {
                popupTitleText.text = message.title;
            }

            if (popupBodyText != null)
            {
                popupBodyText.text = message.body;
            }
        }

        private void HidePopup()
        {
            if (popupPanel != null)
            {
                popupPanel.SetActive(false);
            }
        }

        private class PopupMessage
        {
            public string title;
            public string body;
        }

        private struct PopupRefs
        {
            public GameObject panel;
            public Text titleText;
            public Text bodyText;
            public Button continueButton;
        }
    }
}
