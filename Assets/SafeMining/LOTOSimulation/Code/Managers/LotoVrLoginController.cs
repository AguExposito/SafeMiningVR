using System;
using System.Text;
using System.Threading.Tasks;
using NJ.Finder;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.UI;

[DisallowMultipleComponent]
public class LotoVrLoginController : MonoBehaviour
{
    public enum ConnectionMode
    {
        Offline,
        Online
    }

    [Serializable]
    public class VrLoginSession
    {
        public string username;
        public string token;
    }

    [Serializable]
    class VrLoginRequest
    {
        public string username;
        public string password;
    }

    [Serializable]
    class VrLoginResponse
    {
        public string token;
    }

    public static LotoVrLoginController Instance { get; private set; }

    [Header("Connection")]
    [SerializeField] private ConnectionMode connectionMode = ConnectionMode.Offline;
    [SerializeField] private string apiBaseUrl = "https:/" + "/safemining-back.nimsys.cloud";
    [SerializeField] private string loginPath = "/api/v1/auth/login";

    private VrLoginSession session = new VrLoginSession();
    private ref_Login loginView;
    private ref_MainMenu mainMenu;
    private LotoVrWorldKeyboard worldKeyboard;
    private bool requestRunning;
    private Color statusColor = Color.white;
    private bool statusColorCaptured;
    private TMP_InputField activeKeyboardField;
    private bool activeKeyboardSecure;
    private float lastConfirmClickTime;

    public ConnectionMode Mode => connectionMode;
    public bool IsLoggedIn => !string.IsNullOrEmpty(session.token);
    public VrLoginSession Session => session;
    public string ApiBaseUrl => GetNormalizedBaseUrl();

    #region Lifecycle

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnValidate()
    {
        if (!Application.isPlaying || (loginView == null && mainMenu == null))
        {
            return;
        }

        ApplyForActiveScene();
    }

    #endregion

    #region Public API

    public void Initialize()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        ApplyForActiveScene();
    }

    public void Logout()
    {
        ReleaseTrainingIdentity();
        ClearCredentialFields();
        ApplyForActiveScene();
    }

    public void ApplyBearer(UnityWebRequest request)
    {
        if (request == null || string.IsNullOrEmpty(session.token))
        {
            return;
        }

        request.SetRequestHeader("Authorization", "Bearer " + session.token);
    }

    #endregion

    #region Login Flow

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyForActiveScene();
    }

    private void ApplyForActiveScene()
    {
        loginView = ComponentFinder.FindComponentInScene<ref_Login>();
        mainMenu = ComponentFinder.FindComponentInScene<ref_MainMenu>();
        if (loginView == null && mainMenu == null)
        {
            return;
        }

        HideLegacyLoginWidgets();
        BindConfirm();
        ApplyPlaceholders();
        EnsureLoginCanvasCamera();
        EnsurePlayerFloorTracking();
        EnsureHandUiRaycast();

        if (connectionMode == ConnectionMode.Offline)
        {
            if (IsLoggedIn || HasPersistedParticipant())
            {
                ReleaseTrainingIdentity();
            }

            ShowMenu();
            return;
        }

        if (IsLoggedIn)
        {
            ShowMenu();
            return;
        }

        ShowLogin(Translate("loto_vr_login_hint"), false);
    }

    private async Task SubmitAsync()
    {
        if (loginView == null || loginView.usernameField == null || loginView.passwordField == null)
        {
            return;
        }

        string username = loginView.usernameField.text != null ? loginView.usernameField.text.Trim() : string.Empty;
        string password = loginView.passwordField.text ?? string.Empty;
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowLogin(Translate("loto_vr_login_hint"), false);
            return;
        }

        if (!TryBuildEndpoint(out string endpoint))
        {
            ShowLogin(Translate("loto_vr_login_network"), true);
            return;
        }

        requestRunning = true;
        HideWorldKeyboard();
        SetConfirmInteractable(false);
        ShowLogin(Translate("loto_vr_login_verifying"), false);
        try
        {
            VrLoginResponse response = await PostLoginAsync(endpoint, username, password);
            if (response == null || string.IsNullOrEmpty(response.token))
            {
                ClearPasswordField();
                ShowLogin(Translate("loto_vr_login_session_invalid"), true);
                return;
            }

            session = new VrLoginSession
            {
                username = username,
                token = response.token
            };
            WipeLegacyUserRecord();
            if (ControlCenter.Instance != null && ControlCenter.Instance.simulationSessionManager != null)
            {
                ControlCenter.Instance.simulationSessionManager.InvalidateActiveSession();
            }

            ClearCredentialFields();
            ShowMenu();
            if (ControlCenter.Instance != null && ControlCenter.Instance.syncQueueManager != null)
            {
                ControlCenter.Instance.syncQueueManager.RefreshActiveAdapter();
                _ = ControlCenter.Instance.syncQueueManager.TryFlushQueueAsync();
            }
        }
        catch (Exception)
        {
            ClearPasswordField();
            ShowLogin(Translate("loto_vr_login_network"), true);
        }
        finally
        {
            requestRunning = false;
            SetConfirmInteractable(true);
        }
    }

    private async Task<VrLoginResponse> PostLoginAsync(string endpoint, string username, string password)
    {
        string payload = JsonUtility.ToJson(new VrLoginRequest { username = username, password = password });
        byte[] body = Encoding.UTF8.GetBytes(payload);
        using (UnityWebRequest request = new UnityWebRequest(endpoint, UnityWebRequest.kHttpVerbPOST))
        {
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 15;
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.ConnectionError || request.responseCode == 0)
            {
                throw new InvalidOperationException("network");
            }

            if (request.responseCode != 200)
            {
                return null;
            }

            return JsonUtility.FromJson<VrLoginResponse>(request.downloadHandler.text);
        }
    }

    public bool TryBuildResultEndpoint(string simulationId, out string endpoint)
    {
        endpoint = string.Empty;
        if (string.IsNullOrWhiteSpace(simulationId))
        {
            return false;
        }

        string baseUrl = GetNormalizedBaseUrl();
        if (string.IsNullOrEmpty(baseUrl))
        {
            return false;
        }

        string encodedId = Uri.EscapeDataString(simulationId.Trim());
        endpoint = baseUrl + "/api/v1/result/" + encodedId;
        return true;
    }

    private bool TryBuildEndpoint(out string endpoint)
    {
        endpoint = string.Empty;
        string baseUrl = GetNormalizedBaseUrl();
        string path = string.IsNullOrWhiteSpace(loginPath) ? string.Empty : loginPath.Trim();
        if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(path))
        {
            return false;
        }

        if (!path.StartsWith("/"))
        {
            path = "/" + path;
        }

        endpoint = baseUrl + path;
        return true;
    }

    private string GetNormalizedBaseUrl()
    {
        string baseUrl = string.IsNullOrWhiteSpace(apiBaseUrl) ? string.Empty : apiBaseUrl.Trim().TrimEnd('/');
        if (string.IsNullOrEmpty(baseUrl))
        {
            return string.Empty;
        }

        int schemeEnd = baseUrl.IndexOf(':');
        if (schemeEnd < 0 || schemeEnd + 2 >= baseUrl.Length)
        {
            return string.Empty;
        }

        if (!baseUrl.Substring(0, schemeEnd).Equals("https", StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }

        if (baseUrl[schemeEnd + 1] != '/' || baseUrl[schemeEnd + 2] != '/')
        {
            return string.Empty;
        }

        return baseUrl;
    }

    #endregion

    #region UI

    private void ShowMenu()
    {
        HideWorldKeyboard();
        if (loginView != null)
        {
            if (loginView.welcome_panel != null)
            {
                loginView.welcome_panel.SetActive(false);
            }

            if (loginView.pin_panel != null)
            {
                loginView.pin_panel.SetActive(false);
            }

            loginView.gameObject.SetActive(false);
        }

        if (mainMenu != null)
        {
            mainMenu.gameObject.SetActive(true);
            if (mainMenu.Go_btn_logout != null)
            {
                mainMenu.Go_btn_logout.SetActive(connectionMode == ConnectionMode.Online && IsLoggedIn);
            }
        }

        RefreshMenuUserLabel();
    }

    private void RefreshMenuUserLabel()
    {
        MenuInteractionManager menuInteractions = ComponentFinder.FindComponentInScene<MenuInteractionManager>();
        if (menuInteractions != null)
        {
            menuInteractions.UpdateLogoutUserDataText();
            return;
        }

        ref_Logout logout = ComponentFinder.FindComponentInScene<ref_Logout>();
        if (logout == null || logout.text_userdata == null)
        {
            return;
        }

        string displayName = IsLoggedIn && session != null ? session.username : string.Empty;
        logout.text_userdata.text = string.IsNullOrEmpty(displayName) ? "-" : displayName;
    }

    private void ShowLogin(string message, bool isError)
    {
        if (mainMenu != null)
        {
            mainMenu.gameObject.SetActive(false);
        }

        if (loginView == null)
        {
            return;
        }

        loginView.gameObject.SetActive(true);
        if (loginView.welcome_panel != null)
        {
            loginView.welcome_panel.SetActive(false);
        }

        if (loginView.pin_panel != null)
        {
            loginView.pin_panel.SetActive(false);
        }

        EnsurePlayerFloorTracking();
        EnsureHandUiRaycast();
        EnsureLoginUiRaycasters();
        CaptureStatusColor();
        if (loginView.login_status != null)
        {
            loginView.login_status.text = message;
            loginView.login_status.color = isError ? new Color(0.937f, 0.267f, 0.267f, 1f) : statusColor;
        }

        if (!requestRunning)
        {
            SetConfirmInteractable(true);
        }
    }

    private void HideLegacyLoginWidgets()
    {
        if (loginView == null)
        {
            return;
        }

        if (loginView.activationBtn != null)
        {
            loginView.activationBtn.transform.parent.gameObject.SetActive(false);
        }

        if (loginView.pinBtn != null)
        {
            loginView.pinBtn.transform.parent.gameObject.SetActive(false);
        }

        if (loginView.pin_panel != null)
        {
            loginView.pin_panel.SetActive(false);
        }

        if (loginView.confirmLoginBtn != null)
        {
            loginView.confirmLoginBtn.gameObject.SetActive(true);
            loginView.confirmLoginBtn.interactable = true;
            UIInteractable confirmUi = loginView.confirmLoginBtn.GetComponent<UIInteractable>();
            if (confirmUi != null)
            {
                confirmUi.EnableInteractions();
            }
        }

        if (loginView.usernameField != null)
        {
            loginView.usernameField.gameObject.SetActive(true);
            loginView.usernameField.interactable = true;
        }

        if (loginView.passwordField != null)
        {
            loginView.passwordField.gameObject.SetActive(true);
            loginView.passwordField.interactable = true;
        }

        EnsurePlayerFloorTracking();
        EnsureHandUiRaycast();
    }

    private void EnsureHandUiRaycast()
    {
        NearFarInteractor[] nearFars = FindObjectsByType<NearFarInteractor>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);
        for (int i = 0; i < nearFars.Length; i++)
        {
            NearFarInteractor nearFar = nearFars[i];
            if (nearFar == null)
            {
                continue;
            }

            nearFar.enableUIInteraction = true;
            nearFar.enableFarCasting = true;
        }

        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            eventSystem = FindFirstObjectByType<EventSystem>();
        }

        if (eventSystem == null)
        {
            return;
        }

        XRUIInputModule xrUi = eventSystem.GetComponent<XRUIInputModule>();
        if (xrUi != null)
        {
            xrUi.enabled = true;
            xrUi.enableXRInput = true;
        }
    }

    private void EnsurePlayerFloorTracking()
    {
        XROrigin origin = FindFirstObjectByType<XROrigin>();
        if (origin == null)
        {
            return;
        }

        origin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;
        origin.CameraYOffset = 0f;
        if (origin.CameraFloorOffsetObject != null)
        {
            Transform floor = origin.CameraFloorOffsetObject.transform;
            floor.localPosition = new Vector3(floor.localPosition.x, 0f, floor.localPosition.z);
        }
    }

    private void EnsureLoginCanvasCamera()
    {
        if (loginView == null)
        {
            return;
        }

        Canvas canvas = loginView.GetComponent<Canvas>();
        if (canvas == null || canvas.renderMode != RenderMode.WorldSpace)
        {
            return;
        }

        if (canvas.worldCamera == null)
        {
            canvas.worldCamera = Camera.main;
        }
    }

    private void BindConfirm()
    {
        if (loginView == null || loginView.confirmLoginBtn == null)
        {
            return;
        }

        Button confirm = loginView.confirmLoginBtn;
        confirm.interactable = true;
        confirm.onClick.RemoveListener(OnConfirmClicked);
        confirm.onClick.AddListener(OnConfirmClicked);

        LotoVrKeyButton keyButton = confirm.GetComponent<LotoVrKeyButton>();
        if (keyButton == null)
        {
            keyButton = confirm.gameObject.AddComponent<LotoVrKeyButton>();
        }

        keyButton.Configure(OnConfirmClicked);

        UIInteractable confirmUi = confirm.GetComponent<UIInteractable>();
        if (confirmUi != null)
        {
            confirmUi.EnableInteractions();
        }

        Image confirmImage = confirm.targetGraphic as Image;
        if (confirmImage != null)
        {
            confirmImage.raycastTarget = true;
        }
    }

    private void OnConfirmClicked()
    {
        if (requestRunning || connectionMode != ConnectionMode.Online)
        {
            return;
        }

        if (Time.unscaledTime - lastConfirmClickTime < 0.25f)
        {
            return;
        }

        lastConfirmClickTime = Time.unscaledTime;
        HideWorldKeyboard();
        _ = SubmitAsync();
    }

    private void ApplyPlaceholders()
    {
        if (loginView == null)
        {
            return;
        }

        EnsureWorldKeyboard();
        BindField(loginView.usernameField, false, "loto_vr_login_placeholder");
        BindField(loginView.passwordField, true, "loto_vr_login_password_placeholder");
    }

    private void EnsureWorldKeyboard()
    {
        if (loginView == null)
        {
            return;
        }

        RectTransform panel = loginView.usernameField != null
            ? loginView.usernameField.transform.parent as RectTransform
            : loginView.transform as RectTransform;
        if (panel == null)
        {
            return;
        }

        worldKeyboard = panel.GetComponent<LotoVrWorldKeyboard>();
        if (worldKeyboard == null)
        {
            worldKeyboard = panel.GetComponentInChildren<LotoVrWorldKeyboard>(true);
        }

        if (worldKeyboard == null)
        {
            worldKeyboard = panel.gameObject.AddComponent<LotoVrWorldKeyboard>();
        }

        Sprite keySprite = null;
        TMP_FontAsset font = null;
        if (loginView.confirmLoginBtn != null)
        {
            Image confirmImage = loginView.confirmLoginBtn.GetComponent<Image>();
            if (confirmImage != null)
            {
                keySprite = confirmImage.sprite;
            }
        }

        if (loginView.usernameField != null && loginView.usernameField.textComponent != null)
        {
            font = loginView.usernameField.textComponent.font;
        }

        worldKeyboard.EnsureBuilt(panel, keySprite, font, HandleWorldKeyboardDone);
        EnsureLoginUiRaycasters();
    }

    private void EnsureLoginUiRaycasters()
    {
        if (loginView == null)
        {
            return;
        }

        Canvas canvas = loginView.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = loginView.GetComponentInParent<Canvas>();
        }

        if (canvas == null)
        {
            return;
        }

        if (canvas.worldCamera == null)
        {
            canvas.worldCamera = Camera.main;
        }

        if (canvas.GetComponent<GraphicRaycaster>() == null)
        {
            canvas.gameObject.AddComponent<GraphicRaycaster>();
        }

        if (canvas.GetComponent<TrackedDeviceGraphicRaycaster>() == null)
        {
            canvas.gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();
        }
    }

    private void BindField(TMP_InputField field, bool secure, string placeholderKey)
    {
        if (field == null)
        {
            return;
        }

        field.contentType = secure ? TMP_InputField.ContentType.Password : TMP_InputField.ContentType.Standard;
        field.shouldHideMobileInput = true;
        field.shouldHideSoftKeyboard = true;
        field.interactable = true;

        UnityEngine.UI.Image background = field.targetGraphic as UnityEngine.UI.Image;
        if (background != null)
        {
            background.raycastTarget = true;
        }

        field.onSelect.RemoveAllListeners();

        LotoVrLoginFieldActivator activator = field.GetComponent<LotoVrLoginFieldActivator>();
        if (activator == null)
        {
            activator = field.gameObject.AddComponent<LotoVrLoginFieldActivator>();
        }

        activator.Configure(secure, OpenKeyboardForField);

        TMP_Text placeholder = field.placeholder as TMP_Text;
        if (placeholder != null)
        {
            placeholder.text = Translate(placeholderKey);
            placeholder.raycastTarget = false;
        }

        if (field.textComponent != null)
        {
            field.textComponent.raycastTarget = false;
        }
    }

    private void OpenKeyboardForField(TMP_InputField field, bool secure)
    {
        if (field == null || !field.interactable || requestRunning)
        {
            return;
        }

        if (activeKeyboardField == field && worldKeyboard != null && worldKeyboard.IsVisible)
        {
            return;
        }

        EnsureWorldKeyboard();
        activeKeyboardField = field;
        activeKeyboardSecure = secure;
        field.shouldHideMobileInput = true;
        field.shouldHideSoftKeyboard = true;
        field.DeactivateInputField();
        if (UnityEngine.EventSystems.EventSystem.current != null
            && UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == field.gameObject)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }

        if (worldKeyboard != null)
        {
            worldKeyboard.Show(field);
        }
    }

    private void HideWorldKeyboard()
    {
        if (worldKeyboard != null)
        {
            worldKeyboard.Hide();
        }

        activeKeyboardField = null;
    }

    private void HandleWorldKeyboardDone()
    {
        TMP_InputField finished = activeKeyboardField;
        bool wasSecure = activeKeyboardSecure;
        if (worldKeyboard != null)
        {
            worldKeyboard.Hide();
        }

        activeKeyboardField = null;
        if (!wasSecure && loginView != null && loginView.passwordField != null && finished == loginView.usernameField)
        {
            OpenKeyboardForField(loginView.passwordField, true);
        }
    }

    private void SetConfirmInteractable(bool enabled)
    {
        if (loginView == null || loginView.confirmLoginBtn == null)
        {
            return;
        }

        loginView.confirmLoginBtn.interactable = enabled;
        UIInteractable confirmUi = loginView.confirmLoginBtn.GetComponent<UIInteractable>();
        if (confirmUi != null)
        {
            if (enabled)
            {
                confirmUi.EnableInteractions();
            }
            else
            {
                confirmUi.DisableInteractions();
            }
        }
    }

    private void CaptureStatusColor()
    {
        if (statusColorCaptured || loginView == null || loginView.login_status == null)
        {
            return;
        }

        statusColor = loginView.login_status.color;
        statusColorCaptured = true;
    }

    private void ClearCredentialFields()
    {
        HideWorldKeyboard();
        if (loginView == null)
        {
            return;
        }

        if (loginView.usernameField != null)
        {
            loginView.usernameField.text = string.Empty;
        }

        ClearPasswordField();
    }

    private void ClearPasswordField()
    {
        if (loginView != null && loginView.passwordField != null)
        {
            loginView.passwordField.text = string.Empty;
        }
    }

    #endregion

    #region Helpers

    private void ReleaseTrainingIdentity()
    {
        session = new VrLoginSession();
        WipeLegacyUserRecord();
        if (ControlCenter.Instance != null && ControlCenter.Instance.persistantDataManager != null)
        {
            ControlCenter.Instance.persistantDataManager.scorePersistantData = new ScorePersistantData
            {
                sceneScores = new System.Collections.Generic.List<SceneScore>()
            };
            ControlCenter.Instance.persistantDataManager.SaveScoreData();
        }

        if (ControlCenter.Instance != null && ControlCenter.Instance.scoreManager != null)
        {
            ControlCenter.Instance.scoreManager.RefreshLobbyScoreView();
        }

        if (ControlCenter.Instance != null && ControlCenter.Instance.simulationSessionManager != null)
        {
            ControlCenter.Instance.simulationSessionManager.InvalidateActiveSession();
        }
    }

    private bool HasPersistedParticipant()
    {
        if (ControlCenter.Instance == null || ControlCenter.Instance.persistantDataManager == null)
        {
            return false;
        }

        UserPersistantData userData = ControlCenter.Instance.persistantDataManager.userPersistantData;
        return userData.isPlayerLoggedIn || !string.IsNullOrEmpty(userData.participantID);
    }

    private void WipeLegacyUserRecord()
    {
        if (ControlCenter.Instance == null || ControlCenter.Instance.persistantDataManager == null)
        {
            return;
        }

        UserPersistantData userData = ControlCenter.Instance.persistantDataManager.userPersistantData;
        userData.participantID = string.Empty;
        userData.activationCode = string.Empty;
        userData.iaCloudUrl = string.Empty;
        userData.userPin = string.Empty;
        userData.firstName = string.Empty;
        userData.lastName = string.Empty;
        userData.resourceID = string.Empty;
        userData.isLicenseValid = false;
        userData.isPlayerLoggedIn = false;
        ControlCenter.Instance.persistantDataManager.userPersistantData = userData;
        ControlCenter.Instance.persistantDataManager.SaveUserData();
    }

    private string Translate(string id)
    {
        if (ControlCenter.Instance == null || ControlCenter.Instance.languageManager == null)
        {
            return id;
        }

        return ControlCenter.Instance.languageManager.GetText(id);
    }

    #endregion
}
