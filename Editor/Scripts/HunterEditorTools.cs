using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
using System;

namespace GamersHunter
{
	public class CreateMarketButtonWindow : EditorWindow
	{
		private string[] options = new string[] {
			"Automatic Animated",
			"Automatic",
			"Manual Animated"
		};

		private string[] textOptions = new string[] {
			"Text",
			"TextMeshPro"
		};

		private AutoUpdate[] buttonTypes = new AutoUpdate[] {
			AutoUpdate.AutomaticAnimated,
			AutoUpdate.AutomaticNoAnimation,
			AutoUpdate.ManualAnimated
		};
		private string[] descriptions = new string[] {
			"Button will have a HunterCoin animation once it becomes visible.\nYou can set a GameObject from where coins spawn from\nand a delay in seconds for the animation.",
			"Button will have no animation.\nHunterCoins will be updated once session is stopped.",
			"Button will not animate and the counter will not update.\nManually call GamersHunter.MarketButton.Update() in your script.\nIn the inspector, you can set a GameObject from where coins spawn from,\nand a delay in seconds for the animation.\nYou can also pass a GameObject as the first parameter."
		};
		private int selectedIndex = 0;
		private int selectedTextIndex = 0;

		public void OnGUI()
		{
			GUILayout.BeginVertical();
			GUILayout.Space(20);

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Create new Marketplace Button", EditorStyles.whiteLargeLabel);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.Space(10);

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Update Type", EditorStyles.whiteLabel);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.Space(10);

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			GUI.contentColor = Color.gray;

			GUIStyle descriptionTextStyle = new GUIStyle(GUI.skin.label);
			descriptionTextStyle.alignment = TextAnchor.MiddleCenter;

			GUILayout.Label("This can be changed later in the MarketButton component", descriptionTextStyle);

			GUI.contentColor = Color.white;
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.Space(10);

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			selectedIndex = GUILayout.Toolbar(selectedIndex, options, GUILayout.Height(30));
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.Space(10);

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Text type", EditorStyles.whiteLabel);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			selectedTextIndex = GUILayout.Toolbar(selectedTextIndex, textOptions, GUILayout.Height(30));
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.Space(5);

			string description = descriptions[selectedIndex];

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUI.contentColor = Color.gray;

			GUIStyle descriptionStyle = new GUIStyle(GUI.skin.label);
			descriptionStyle.alignment = TextAnchor.MiddleCenter;
			descriptionStyle.fixedHeight = 70;

			GUILayout.Label(description, descriptionStyle);

			GUI.contentColor = Color.white;
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.Space(20);

			GUILayout.BeginHorizontal();
			GUILayout.Space(50);
			if (GUILayout.Button("Create", GUILayout.Height(30)))
			{
				HunterEditorTools.CreateMarketButton(buttonTypes[selectedIndex], selectedTextIndex == 1);
				Close();
			}
			GUILayout.Space(50);
			GUILayout.EndHorizontal();

			GUILayout.EndVertical();
		}

		public static void ShowWindow()
		{
			EditorWindow editorWindow = EditorWindow.GetWindow(typeof(CreateMarketButtonWindow), false, "Gamers Hunter", true);
			editorWindow.minSize = new Vector2(440, 360);
			editorWindow.maxSize = new Vector2(440, 360);
		}
	}

	[InitializeOnLoad]
	public class HunterEditorTools
	{
		private static readonly Color DEFAULT_TEXT_COLOR = new Color(143f / 255f, 101f / 255f, 87f / 255f);
		private static HunterLog logger = new HunterLog("HunterEditorTools");
		private static bool initialized = false;
		private const string keyProjectOpen = "gh_projectopen";
		private const string manifestPath = "Assets/Plugins/Android/AndroidManifest.xml";

		static HunterEditorTools()
		{
			if (!SessionState.GetBool(keyProjectOpen, false))
			{
				SessionState.SetBool(keyProjectOpen, true);
				EditorApplication.delayCall += Initialize;
			}

			if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
			{
				CheckAndroidRequirements();
			}
			else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
			{
				CheckIOSRequirements();
			}
			else
			{
				logger.Error("GamersHunter is not supported on this platform");
			}
		}

		private static void CheckIOSRequirements()
		{
			string targetVersion = PlayerSettings.iOS.targetOSVersionString;
			string majorVersion = targetVersion != null ? targetVersion.Split('.')[0] : "0";

			Array schemes = PlayerSettings.iOS.iOSUrlSchemes;

			bool hasUrlScheme = schemes != null && schemes.Length > 0;
			bool hasTargetVersion = int.Parse(majorVersion) >= 9;

			if (!(hasUrlScheme && hasTargetVersion))
			{
				string message = "Required iOS build settings are missing:\n";
				if (!hasUrlScheme)
				{
					message += "URL Scheme not found.\nYou can add one on iOS build settings.\n";
				}
				if (!hasTargetVersion)
				{
					message += "Target iOS version must be 9.0 or higher.\nYou can change this on iOS build settings.\n";
				}
				message += "\nPlease fix this before building for iOS. GamersHunter will not work correctly without these settings.";

				logger.Error(message);
				bool result = EditorUtility.DisplayDialog("GamersHunter iOS Error", message, "Ok");
				return;
			}
		}

		private static void CheckAndroidRequirements()
		{
			if (!File.Exists(manifestPath))
			{
				string message = "Android Custom Main Manifest not found.\nYou can add one on Android build settings > Publishing settings > Custom Main Manifest.\n";
				message += "Please fix before building for Android, GamersHunter will not work correctly without this.";
				logger.Error(message);
				bool result = EditorUtility.DisplayDialog("GamersHunter Android Error", message, "Ok");
				return;
			}

			string manifestContent = File.ReadAllText(manifestPath);

			bool hasMagicLogin = manifestContent.Contains("android:host=\"gh-magiclogin\"");
			bool hasClearText = manifestContent.Contains("android:usesCleartextTraffic=\"true\"");
			bool hasHardwareAcceleration = manifestContent.Contains("android:hardwareAccelerated=\"true\"");
			bool hasExported = manifestContent.Contains("android:exported=\"true\"");

			if (!(hasMagicLogin && hasClearText && hasHardwareAcceleration && hasExported))
			{
				string message = "Required flags are missing in the Android manifest:\n";
				if (!hasMagicLogin)
				{
					message += "- Magic Login Intent Filter\n  (Used for deep link login)\n";
				}
				if (!hasClearText)
				{
					message += "- Cleartext Traffic\n  (Used for market setup)";
				}
				if (!hasHardwareAcceleration)
				{
					message += "- Hardware Acceleration\n  (Used for a lag free webview)";
				}
				if (!hasExported)
				{
					message += "- Exported\n  (Used for deep links)\n";
				}
				message += "Please fix before building for Android, GamersHunter will not work correctly without these flags.";

				logger.Error(message);
				bool result = EditorUtility.DisplayDialog("GamersHunter Android Error", message, "Ok");
			}
		}

		private static void Initialize()
		{
			EditorApplication.delayCall -= Initialize;

			if (!HunterEditorTools.initialized)
			{
				logger.Info("Hunter Editor Tools Init");
				CreateSettings();
				HunterEditorTools.initialized = true;
			}
		}

		[MenuItem("GameObject/UI/GamersHunter/Marketplace Button", false, 10)]
		public static void CreateMarketButtonAutoOnGameObject()
		{
			CreateMarketButtonWindow.ShowWindow();
		}

		[MenuItem("GamersHunter/Create Marketplace Button", false, 10)]
		public static void CreateMarketButtonOnGamersHunter()
		{
			CreateMarketButtonWindow.ShowWindow();
		}

		[MenuItem("GamersHunter/Settings", false, 20)]
		public static void ShowHunterSettings()
		{
			HunterSettings settings = CreateSettings();
			Selection.activeObject = settings;
		}

		[MenuItem("GamersHunter/Documentation", false, 20)]
		public static void ShowHunterDocumentation()
		{
			Application.OpenURL("https://gamershunter.com/developer/docs/plugins");
		}

		[MenuItem("GamersHunter/Developer portal", false, 20)]
		public static void ShowHunterDeveloperPortal()
		{
			Application.OpenURL("https://gamershunter.com/developer/apps");
		}

		private static HunterSettings CreateSettings()
		{
			string path = HunterSettings.ASSET_PATH + HunterSettings.ASSET_FILENAME;
			HunterSettings settings = AssetDatabase.LoadAssetAtPath<HunterSettings>(path);
			if (settings == null)
			{
				logger.Info("Creating Gamers Hunter settings asset");
				if (!Directory.Exists(HunterSettings.ASSET_PATH))
				{
					Directory.CreateDirectory(Path.GetDirectoryName(HunterSettings.ASSET_PATH));
				}
				ScriptableObject settingsInstance = ScriptableObject.CreateInstance(typeof(HunterSettings));
				AssetDatabase.CreateAsset(settingsInstance, path);
				settings = AssetDatabase.LoadAssetAtPath<HunterSettings>(path);
			}

			if (settings != null)
			{
				logger.Info("Loaded GamersHunter settings asset");
				if (string.IsNullOrEmpty(settings.appKey) || string.IsNullOrEmpty(settings.appSecret))
				{
					bool result = EditorUtility.DisplayDialog("Gamers Hunter settings", "Missing Gamers Hunter App key or secret on settings asset. This is assigned on the Gamers Hunter developer portal.", "Open settings", "Cancel");
					if (result)
					{
						Selection.activeObject = settings;
					}
				}
			}
			else
			{
				logger.Info("Could not create GamersHunter settings asset");
			}

			return settings;
		}

		public static void CreateMarketButton(AutoUpdate autoUpdate = AutoUpdate.AutomaticAnimated, bool useTextMeshPro = false)
		{
			GameObject buttonObject = new GameObject("Market Button");
			Image image = buttonObject.AddComponent<Image>();
			Button button = buttonObject.AddComponent<Button>();
			AudioSource audioSource = buttonObject.AddComponent<AudioSource>();
			AudioClip coinSound = Resources.Load<AudioClip>("GamersHunter/Sounds/coins");

			MarketButton marketButton = buttonObject.AddComponent<MarketButton>();
			marketButton.autoUpdate = autoUpdate;

			Sprite defaultSprite = Resources.Load<Sprite>("GamersHunter/Images/gh-counter-u");
			Sprite pressedSprite = Resources.Load<Sprite>("GamersHunter/Images/gh-counter-p");

			image.sprite = defaultSprite;
			image.preserveAspect = true;
			image.raycastTarget = true;
			image.SetNativeSize();

			SpriteState spriteState = new SpriteState();
			spriteState.pressedSprite = pressedSprite;
			spriteState.disabledSprite = defaultSprite;

			button.transition = Selectable.Transition.SpriteSwap;
			button.spriteState = spriteState;

			// Hunter Coins counter text
			GameObject textObject = new GameObject("HunterCoins text");
			textObject.transform.SetParent(buttonObject.transform);

			if (useTextMeshPro)
			{
				TextMeshProUGUI textComponent = textObject.AddComponent<TextMeshProUGUI>();
				textComponent.text = "0";
				try
				{
					textComponent.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
				}
				catch (Exception e)
				{
					logger.Warn("Could not load TextMeshPro font, have you imported TMPro essentials? " + e.Message);
				}
				textComponent.fontSize = 40;
				textComponent.fontStyle = FontStyles.Bold;
				textComponent.alignment = TextAlignmentOptions.Center;
				textComponent.color = DEFAULT_TEXT_COLOR;
			}
			else
			{
				Text textComponent = textObject.AddComponent<Text>();
				textComponent.text = "0";
				textComponent.font = Font.CreateDynamicFontFromOSFont("Arial", 40);
				textComponent.fontSize = 40;
				textComponent.fontStyle = FontStyle.Bold;
				textComponent.alignment = TextAnchor.MiddleCenter;
				textComponent.color = DEFAULT_TEXT_COLOR;
			}

			RectTransform textRect = textObject.GetComponent<RectTransform>();
			textRect.anchoredPosition = new Vector2(28f, 0f);
			textRect.anchorMin = new Vector2(0.5f, 0.5f);
			textRect.anchorMax = new Vector2(0.5f, 0.5f);
			textRect.pivot = new Vector2(0.5f, 0.5f);

			// Notifications badge
			GameObject notificationsObject = new GameObject("Notifications badge");
			notificationsObject.transform.SetParent(buttonObject.transform);

			Image notificationsImage = notificationsObject.AddComponent<Image>();
			Sprite notificationsSprite = Resources.Load<Sprite>("GamersHunter/Images/gh-counter-n");
			notificationsImage.sprite = notificationsSprite;
			notificationsImage.enabled = true;

			RectTransform notificationsRect = notificationsObject.GetComponent<RectTransform>();
			notificationsRect.anchoredPosition = new Vector2(100f, -20f);
			notificationsRect.sizeDelta = new Vector2(30f, 30f);
			notificationsRect.anchorMin = new Vector2(0.5f, 0.5f);
			notificationsRect.anchorMax = new Vector2(0.5f, 0.5f);
			notificationsRect.pivot = new Vector2(0.5f, 0.5f);

			// Grouping
			GameObject hunterGroup = new GameObject("Gamers Hunter");
			RectTransform hunterGroupTransform = hunterGroup.AddComponent<RectTransform>();
			hunterGroup.AddComponent<CanvasGroup>();

			AttachToCanvas(hunterGroup);

			GameObject coinsGroup = new GameObject("HunterCoins Group");
			RectTransform coinsGroupTransform = coinsGroup.AddComponent<RectTransform>();

			CanvasGroup coinsCanvasGroup = coinsGroup.AddComponent<CanvasGroup>();
			coinsCanvasGroup.interactable = false;
			coinsCanvasGroup.blocksRaycasts = false;

			coinsGroup.transform.SetParent(hunterGroup.transform);
			marketButton.transform.SetParent(hunterGroup.transform);

			hunterGroupTransform.anchorMin = new Vector2(0f, 0f);
			hunterGroupTransform.anchorMax = new Vector2(0f, 0f);
			hunterGroupTransform.pivot = new Vector2(0.5f, 0.5f);
			hunterGroupTransform.position = new Vector3(0f, 0f, 0f);
			hunterGroupTransform.sizeDelta = new Vector2(0f, 0f);
			hunterGroupTransform.localScale = new Vector3(1f, 1f, 1f);

			coinsGroupTransform.anchoredPosition = new Vector2(0f, 0f);
			coinsGroupTransform.anchorMin = new Vector2(0f, 0f);
			coinsGroupTransform.anchorMax = new Vector2(0f, 0f);
			coinsGroupTransform.pivot = new Vector2(0.5f, 0.5f);
			coinsGroupTransform.sizeDelta = new Vector2(0f, 0f);
			coinsGroupTransform.localScale = new Vector3(1f, 1f, 1f);

			// Assign public fields
			marketButton.defaultSprite = defaultSprite;
			marketButton.pressedSprite = pressedSprite;
			marketButton.coinsTextObject = textObject;
			marketButton.coinsGroupObject = coinsGroup;
			marketButton.notificationsObject = notificationsObject;

			marketButton.audioSource = audioSource;
			marketButton.coinSound = coinSound;

			// Position
			RectTransform buttonTransform = buttonObject.GetComponent<RectTransform>();
			buttonTransform.anchoredPosition = new Vector2(100f, 100f); // border padding
			buttonTransform.anchorMin = new Vector2(0f, 0f);
			buttonTransform.anchorMax = new Vector2(0f, 0f);
			buttonTransform.pivot = new Vector2(0f, 0f);
			buttonTransform.localScale = new Vector3(1f, 1f, 1f);

			Selection.activeObject = buttonObject;
		}

		private static void AttachToCanvas(GameObject hunterGroup)
		{
			GameObject selectedObject = Selection.activeGameObject;
			Canvas canvas = (selectedObject != null) ? selectedObject.GetComponent<Canvas>() : null;
			if (canvas == null)
			{
				canvas = MonoBehaviour.FindObjectOfType<Canvas>(); // Active / enabled
			}

			if (canvas == null)
			{
				canvas = (Canvas)Resources.FindObjectsOfTypeAll(typeof(Canvas))[0]; // Inactive / Disabled
			}

			if (canvas == null)
			{
				if (EditorUtility.DisplayDialog("No Canvas found", "Create a new one?", "Create & add", "No, Add to root"))
				{
					canvas = CreateNewCanvas();
				}
			}

			if (canvas != null)
			{
				hunterGroup.transform.SetParent(canvas.transform);
			}
			else
			{
				hunterGroup.transform.SetParent(null);
			}
		}

		private static Canvas CreateNewCanvas()
		{
			// If no canvas is found, create a new one
			GameObject canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
			Canvas canvas = canvasObject.GetComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			canvas.sortingOrder = 0;

			CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
			scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			scaler.referenceResolution = new Vector2(1000, 1000);
			scaler.matchWidthOrHeight = 0.5f;

			GraphicRaycaster raycaster = canvasObject.GetComponent<GraphicRaycaster>();
			raycaster.ignoreReversedGraphics = true;
			raycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;

			canvasObject.transform.SetParent(null);

			return canvas;
		}
	}
}
