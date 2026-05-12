using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace RaisingSim.UI.Editor
{
    public static class MvpSceneBuilder
    {
        [MenuItem("Tools/RaisingSim/Create MVP Scene")]
        public static void CreateMvpScene()
        {
            Canvas canvas = CreateCanvas();
            EnsureEventSystem();

            Text dateText = CreateTopArea(canvas.transform);
            Text statsText = CreateStatsArea(canvas.transform);
            Transform actionContainer = CreateActionArea(canvas.transform);
            Text logText = CreateLogArea(canvas.transform);
            PopupRefs popupRefs = CreatePopup(canvas.transform);

            GameObject uiObject = new GameObject("RaisingSimMvpUI");
            Undo.RegisterCreatedObjectUndo(uiObject, "Create RaisingSim MVP UI");
            RaisingSimMvpUI ui = uiObject.AddComponent<RaisingSimMvpUI>();
            ui.dateText = dateText;
            ui.statsText = statsText;
            ui.actionButtonContainer = actionContainer;
            ui.logText = logText;
            ui.popupPanel = popupRefs.panel;
            ui.popupTitleText = popupRefs.titleText;
            ui.popupBodyText = popupRefs.bodyText;
            ui.popupContinueButton = popupRefs.continueButton;

            Selection.activeGameObject = uiObject;
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private static Canvas CreateCanvas()
        {
            GameObject canvasObject = new GameObject("RaisingSimMvpCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Undo.RegisterCreatedObjectUndo(canvasObject, "Create RaisingSim MVP Canvas");

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);

            return canvas;
        }

        private static void EnsureEventSystem()
        {
            if (FindFirst<EventSystem>() != null)
            {
                return;
            }

#if ENABLE_INPUT_SYSTEM
            GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
#else
            GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
#endif
            Undo.RegisterCreatedObjectUndo(eventSystemObject, "Create EventSystem");
        }

        private static T FindFirst<T>() where T : Object
        {
            return Object.FindFirstObjectByType<T>();
        }

        private static Text CreateTopArea(Transform parent)
        {
            GameObject panel = CreatePanel("TopArea", parent, new Color(0.15f, 0.15f, 0.15f, 1f));
            RectTransform rect = panel.GetComponent<RectTransform>();
            Stretch(rect, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -60f), Vector2.zero);

            Text text = CreateText("DateText", panel.transform, "Year 1 / spring / Actions 0/6", 22, TextAnchor.MiddleLeft);
            RectTransform textRect = text.GetComponent<RectTransform>();
            Stretch(textRect, Vector2.zero, Vector2.one, new Vector2(18f, 0f), new Vector2(-18f, 0f));
            text.color = Color.white;
            return text;
        }

        private static Text CreateStatsArea(Transform parent)
        {
            GameObject panel = CreatePanel("LeftStatsArea", parent, new Color(0.93f, 0.93f, 0.93f, 1f));
            RectTransform rect = panel.GetComponent<RectTransform>();
            Stretch(rect, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 170f), new Vector2(260f, -60f));

            Text title = CreateText("StatsTitle", panel.transform, "Stats", 20, TextAnchor.UpperLeft);
            Stretch(title.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(12f, -42f), new Vector2(-12f, -10f));

            Text stats = CreateText("StatsText", panel.transform, "", 16, TextAnchor.UpperLeft);
            Stretch(stats.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(12f, 12f), new Vector2(-12f, -52f));
            return stats;
        }

        private static Transform CreateActionArea(Transform parent)
        {
            GameObject panel = CreatePanel("RightActionArea", parent, new Color(0.88f, 0.90f, 0.93f, 1f));
            RectTransform rect = panel.GetComponent<RectTransform>();
            Stretch(rect, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-340f, 170f), new Vector2(0f, -60f));

            Text title = CreateText("ActionsTitle", panel.transform, "Actions", 20, TextAnchor.UpperLeft);
            Stretch(title.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(12f, -42f), new Vector2(-12f, -10f));

            GameObject scrollObject = new GameObject("ActionScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            scrollObject.transform.SetParent(panel.transform, false);
            scrollObject.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.35f);
            RectTransform scrollRectTransform = scrollObject.GetComponent<RectTransform>();
            Stretch(scrollRectTransform, Vector2.zero, Vector2.one, new Vector2(10f, 10f), new Vector2(-10f, -52f));

            GameObject viewportObject = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewportObject.transform.SetParent(scrollObject.transform, false);
            viewportObject.GetComponent<Image>().color = Color.white;
            viewportObject.GetComponent<Mask>().showMaskGraphic = false;
            Stretch(viewportObject.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            GameObject contentObject = new GameObject("ActionButtonContent", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
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
            RectTransform rect = panel.GetComponent<RectTransform>();
            Stretch(rect, new Vector2(0f, 0f), new Vector2(1f, 0f), Vector2.zero, new Vector2(0f, 170f));

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

            Button button = CreateButton("ContinueButton", panel.transform, "Continue");
            RectTransform buttonRect = button.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(1f, 0f);
            buttonRect.anchorMax = new Vector2(1f, 0f);
            buttonRect.pivot = new Vector2(1f, 0f);
            buttonRect.sizeDelta = new Vector2(140f, 42f);
            buttonRect.anchoredPosition = new Vector2(-18f, 18f);

            panel.SetActive(false);
            return new PopupRefs
            {
                panel = panel,
                titleText = title,
                bodyText = body,
                continueButton = button
            };
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
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.black;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static Button CreateButton(string name, Transform parent, string label)
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

        private struct PopupRefs
        {
            public GameObject panel;
            public Text titleText;
            public Text bodyText;
            public Button continueButton;
        }
    }
}
