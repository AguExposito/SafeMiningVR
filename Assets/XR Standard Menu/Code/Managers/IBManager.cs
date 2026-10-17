using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using NJ.Finder;
using XRStandardMenu;
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using System.Diagnostics;
using System.IO;
#endif

public class IBManager : MonoBehaviour
{
    private const int MaxPinPanelInputLength = 8;

    public enum ConnectionMode
    {
        Offline,
        Online
    }

    public enum SimulatorType
    {
        Hub,
        PackagePart,
        Singular
    }

    public enum UrlType
    {
        Develop,
        Production,
        Custom
    }

    public enum SessionStartMode
    {
        AutoStartSession,
        LoadActivationCodeOnly,
        AlwaysLogin
    }

    [Serializable]
    public class CloudActivationCodeRecordFormat
    {
        public string URLMIL;
        public string codigoActivacion;
        public string fechaVigencia;
    }

    [Serializable]
    public class CloudUserPINRecordFormat
    {
        public string IDParticipante;
        public string IDRecurso;
        public string firstName;
        public string lastName;
    }

    [Serializable]
    public class HubSimulatorLaunchData
    {
        public string packageName;
        public string activationCodeHub;
        public string resourceID;
        public string androidActivityName = "com.unity3d.player.UnityPlayerGameActivity";
        public string windowsExecutableRelativePath;
    }

    private enum InputStage
    {
        None,
        ActivationCode,
        UserPin
    }

    // Cloud endpoints intentionally empty: project runs isolated until a new backend is wired.
    public const string DevelopURL = "";
    public const string ProductionURL = "";

    [Header("Config")]
    [SerializeField] private ConnectionMode connectionMode = ConnectionMode.Offline;
    [SerializeField] private SimulatorType simulatorType = SimulatorType.Singular;
    [SerializeField] private UrlType urlType = UrlType.Custom;
    [SerializeField] private SessionStartMode sessionStartMode = SessionStartMode.AutoStartSession;
    [SerializeField] private bool sendResultsToCloud = false;
    [SerializeField] private bool verboseLoginLogs = true;
    [SerializeField] private string customUrl = string.Empty;
    [SerializeField] private string simulatorID = string.Empty;
    [SerializeField] private string packagePassArg = "pass";
    [SerializeField] private bool packagePartOpenSceneViewerAfterLogin = false;
    [SerializeField] private int packagePartSceneStartIndex = 0;
    [Tooltip("Offset de build index al seleccionar desde el carousel (escena 0 = menú, por lo que el valor por defecto es 1).")]
    [SerializeField] private int packagePartCarouselSceneOffset = 1;
    [SerializeField] private bool packagePartTrustHubSessionArgs = true;
    [Header("Android Manifest Presets")]
    [SerializeField] private bool autoUpdateAndroidManifest = false;
    [SerializeField] private string hubPackageName = "";
    [SerializeField] private List<string> hubDependentPackages = new List<string>();
    [Header("Hub Launch Data")]
    [SerializeField] private List<HubSimulatorLaunchData> hubSimulators = new List<HubSimulatorLaunchData>();

    private ref_Login refLogin;
    private ref_MainMenu refMainMenu;
    private ref_Logout refLogout;
    private string cloudBaseUrl = string.Empty;
    private InputStage inputStage;
    private string currentInput = string.Empty;
    private bool isRequestRunning;
    private Color defaultDisplayColor = Color.white;
    private bool loginReady;
    private bool clearDisplayOnNextInput;
    private string launchResourceIdOverride = string.Empty;
    private bool receivedHubLaunchArgs;

    public bool AutoUpdateAndroidManifest => autoUpdateAndroidManifest;
    public SimulatorType ManifestSimulatorType => simulatorType;
    public string HubPackageName => hubPackageName;
    public IReadOnlyList<string> HubDependentPackages => hubDependentPackages;
    public IReadOnlyList<HubSimulatorLaunchData> HubSimulatorLaunchDataList => hubSimulators;

    public void Initialize()
    {
        cloudBaseUrl = ResolveCloudUrl();
        LogInfo("Initialize", $"mode={connectionMode}, simulator={simulatorType}, sessionStartMode={sessionStartMode}, cloudBaseUrl={cloudBaseUrl}, simulatorID={simulatorID}");
        TryConsumeExternalLaunchArgumentsForPackagePart();
        refLogin = ComponentFinder.FindComponentInScene<ref_Login>();
        refMainMenu = ComponentFinder.FindComponentInScene<ref_MainMenu>();
        refLogout = ComponentFinder.FindComponentInScene<ref_Logout>();
        if (refLogin != null && refLogin.displayPin != null)
        {
            defaultDisplayColor = refLogin.displayPin.color;
        }
        HandleSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    public bool ShouldSendResults()
    {
        return sendResultsToCloud && connectionMode == ConnectionMode.Online && ControlCenter.Instance.persistantDataManager.userPersistantData.isPlayerLoggedIn;
    }

    public void LogoutFromMenu()
    {
        LotoVrLoginController vrLogin = GetComponent<LotoVrLoginController>();
        if (vrLogin != null)
        {
            vrLogin.Logout();
            return;
        }

        if (ControlCenter.Instance == null || ControlCenter.Instance.persistantDataManager == null)
        {
            LogError("LogoutFromMenu", "No se pudo ejecutar logout: PersistantDataManager no disponible.");
            return;
        }

        bool keepActivationCode = sessionStartMode == SessionStartMode.LoadActivationCodeOnly;
        UserPersistantData previousUserData = ControlCenter.Instance.persistantDataManager.userPersistantData;
        UserPersistantData logoutUserData = new UserPersistantData();
        if (keepActivationCode)
        {
            logoutUserData.activationCode = previousUserData.activationCode;
            logoutUserData.iaCloudUrl = previousUserData.iaCloudUrl;
            logoutUserData.isLicenseValid = !string.IsNullOrEmpty(previousUserData.activationCode) && previousUserData.isLicenseValid;
        }
        ControlCenter.Instance.persistantDataManager.userPersistantData = logoutUserData;
        ControlCenter.Instance.persistantDataManager.scorePersistantData = new ScorePersistantData
        {
            sceneScores = new System.Collections.Generic.List<SceneScore>()
        };
        ControlCenter.Instance.persistantDataManager.SaveScoreData();

        refLogin = ComponentFinder.FindComponentInScene<ref_Login>();
        refMainMenu = ComponentFinder.FindComponentInScene<ref_MainMenu>();
        loginReady = false;
        SetMainMenuActive(false);
        if (refLogin != null)
        {
            refLogin.gameObject.SetActive(true);
            if (refLogin.welcome_panel != null)
            {
                refLogin.welcome_panel.SetActive(false);
            }
            if (refLogin.activationBtn != null)
            {
                refLogin.activationBtn.interactable = !logoutUserData.isLicenseValid;
            }
            if (refLogin.pinBtn != null)
            {
                refLogin.pinBtn.interactable = logoutUserData.isLicenseValid;
            }
        }
        currentInput = string.Empty;
        clearDisplayOnNextInput = false;
        SetPinPanelVisible(false);
        UpdateDisplay(currentInput, defaultDisplayColor);
        SetActivationState(logoutUserData.isLicenseValid);
        RefreshCredentialTexts();
        if (ControlCenter.Instance.scoreManager != null)
        {
            ControlCenter.Instance.scoreManager.RefreshLobbyScoreView();
        }
        LogInfo("LogoutFromMenu", $"Logout OK. keepActivationCode={keepActivationCode}");
    }

    public void HandleSceneViewerSelect(UIContentCarousel carousel)
    {
        int carouselIndex = carousel != null ? carousel.currentIndex : 0;
        if (simulatorType == SimulatorType.Hub)
        {
            if (ShouldLaunchExternalHub(carouselIndex))
            {
                OpenHubSimulatorByCarouselIndex(carouselIndex);
                return;
            }

            LogInfo("HandleSceneViewerSelect", $"Hub sin paquete externo para index={carouselIndex}. Cargando simulacion local.");
            LoadLocalSimulationScene();
            return;
        }
        if (simulatorType == SimulatorType.PackagePart)
        {
            int targetBuildIndex = Mathf.Max(0, packagePartCarouselSceneOffset + carouselIndex);
            LoadSimulationSceneByBuildIndex(targetBuildIndex);
            return;
        }

        LoadLocalSimulationScene();
    }

    public void RefreshHubContentAvailability()
    {
        ref_SceneViewer sceneViewer = ComponentFinder.FindComponentInScene<ref_SceneViewer>();
        if (sceneViewer == null || sceneViewer.uIContentCarousel == null || sceneViewer.uIContentCarousel.gridLayoutGroup == null)
        {
            return;
        }
        Transform carouselRoot = sceneViewer.uIContentCarousel.gridLayoutGroup.transform;
        for (int i = 0; i < carouselRoot.childCount; i++)
        {
            ref_CarouselElement carouselElement = carouselRoot.GetChild(i).GetComponent<ref_CarouselElement>();
            if (carouselElement == null || carouselElement.contentNotInstalled == null)
            {
                continue;
            }
            bool showNotInstalled = simulatorType == SimulatorType.Hub &&
                                   ShouldLaunchExternalHub(i) &&
                                   !IsHubSimulatorInstalled(i);
            carouselElement.contentNotInstalled.SetActive(showNotInstalled);
        }
    }

    public async Task<bool> SendScoreToCloud(SceneScore sceneScore, float elapsedSeconds, DateTime startDate, DateTime endDate)
    {
        if (!ShouldSendResults())
        {
            bool isOnline = connectionMode == ConnectionMode.Online;
            bool canSendFlag = sendResultsToCloud;
            bool isLogged = ControlCenter.Instance != null && ControlCenter.Instance.persistantDataManager != null
                && ControlCenter.Instance.persistantDataManager.userPersistantData.isPlayerLoggedIn;
            LogInfo("SendScoreToCloud", $"Envio omitido. sendResultsToCloud={canSendFlag}, connectionOnline={isOnline}, isPlayerLoggedIn={isLogged}");
            return false;
        }
        UserPersistantData userData = ControlCenter.Instance.persistantDataManager.userPersistantData;
        string targetMilUrl = string.IsNullOrEmpty(userData.iaCloudUrl) ? cloudBaseUrl : userData.iaCloudUrl;
        if (string.IsNullOrEmpty(targetMilUrl))
        {
            LogError("SendScoreToCloud", "Envio omitido. URL destino vacia (proyecto aislado).");
            return false;
        }
        int objectiveCount = sceneScore != null && sceneScore.objectives != null ? sceneScore.objectives.Count : 0;
        int achievedCount = sceneScore != null ? sceneScore.points : 0;
        int scoreNormalized = objectiveCount > 0 ? Mathf.Clamp(Mathf.RoundToInt(((float)achievedCount / objectiveCount) * 10f), 0, 10) : 0;
        string category = sceneScore != null ? sceneScore.id : string.Empty;
        if (ControlCenter.Instance != null && ControlCenter.Instance.languageManager != null)
        {
            string translatedCategory = ControlCenter.Instance.languageManager.GetText(category, LanguageName.Spanish);
            if (!string.IsNullOrEmpty(translatedCategory))
            {
                category = translatedCategory;
            }
        }
        WWWForm formData = new WWWForm();
        formData.AddField("category", category);
        formData.AddField("attempts", sceneScore != null ? sceneScore.attempts : 0);
        formData.AddField("IDParticipante", userData.participantID);
        formData.AddField("totalScore", 10);
        formData.AddField("score", scoreNormalized);
        formData.AddField("time", Mathf.Max(0, Mathf.RoundToInt(elapsedSeconds)));
        formData.AddField("start", startDate.ToUniversalTime().ToString("u"));
        formData.AddField("end", endDate.ToUniversalTime().ToString("u"));
        formData.AddField("IDRecurso", userData.resourceID);
        formData.AddField("codigoActivacion", userData.activationCode);
        string endpoint = targetMilUrl + "/crearSimReporte";
        UnityWebRequest request = UnityWebRequest.Post(endpoint, formData);
        var operation = request.SendWebRequest();
        while (!operation.isDone)
        {
            await Task.Yield();
        }
        bool success = request.responseCode == 200;
        if (!success)
        {
            LogRequestFailure("SendScoreToCloud", endpoint, request);
            return false;
        }
        LogInfo("SendScoreToCloud", $"Reporte enviado OK. scene={sceneScore?.id}, score={scoreNormalized}, elapsedSeconds={Mathf.Max(0, Mathf.RoundToInt(elapsedSeconds))}");
        return true;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        refLogin = ComponentFinder.FindComponentInScene<ref_Login>();
        refMainMenu = ComponentFinder.FindComponentInScene<ref_MainMenu>();
        refLogout = ComponentFinder.FindComponentInScene<ref_Logout>();
        if (refLogin != null && refLogin.displayPin != null)
        {
            defaultDisplayColor = refLogin.displayPin.color;
        }
        SetupLoginFlowForCurrentScene();
        RefreshHubContentAvailability();
    }

    private void SetupLoginFlowForCurrentScene()
    {
        if (GetComponent<LotoVrLoginController>() != null)
        {
            return;
        }

        if (refLogin == null)
        {
            return;
        }
        SetLogoutMenuPanelVisible(connectionMode != ConnectionMode.Offline);
        BindButtons();
        SetLoginStatusText();
        RefreshCredentialTexts();
        if (connectionMode == ConnectionMode.Offline)
        {
            ConfigureOfflineMode();
            return;
        }
        if (simulatorType == SimulatorType.PackagePart && !HasPackageAccess())
        {
            LogError("SetupLoginFlowForCurrentScene", $"Sin acceso de paquete. Arg esperado: -{packagePassArg}=true");
            Application.Quit();
            return;
        }
        loginReady = false;
        SetMainMenuActive(false);
        refLogin.gameObject.SetActive(true);
        refLogin.welcome_panel.SetActive(false);
        SetPinPanelVisible(false);
        ApplySessionStartMode();
    }

    private void ConfigureOfflineMode()
    {
        loginReady = true;
        SetLogoutMenuPanelVisible(false);
        SetPinPanelVisible(false);
        if (refLogin.activationBtn != null)
        {
            refLogin.activationBtn.interactable = false;
        }
        if (refLogin.pinBtn != null)
        {
            refLogin.pinBtn.interactable = false;
        }
        if (refMainMenu != null && refMainMenu.go_btn_login != null)
        {
            Button loginButton = refMainMenu.go_btn_login.GetComponent<Button>();
            if (loginButton != null)
            {
                loginButton.interactable = false;
            }
        }
        SetMainMenuActive(true);
        refLogin.gameObject.SetActive(false);
    }

    private void ApplySessionStartMode()
    {
        UserPersistantData userData = ControlCenter.Instance.persistantDataManager.userPersistantData;
        bool activationVerified = !string.IsNullOrEmpty(userData.activationCode) && userData.isLicenseValid;
        LogInfo("ApplySessionStartMode", $"sessionStartMode={sessionStartMode}, simulatorType={simulatorType}, activationVerified={activationVerified}, hasPin={!string.IsNullOrEmpty(userData.userPin)}, isLogged={userData.isPlayerLoggedIn}");
        if (simulatorType == SimulatorType.PackagePart && packagePartTrustHubSessionArgs && receivedHubLaunchArgs)
        {
            ApplyTrustedHubSession();
            return;
        }
        if (sessionStartMode == SessionStartMode.AlwaysLogin)
        {
            ClearPersistedLoginData(clearActivationCode: true);
            loginReady = false;
            SetMainMenuActive(false);
            if (refLogin != null)
            {
                refLogin.gameObject.SetActive(true);
                if (refLogin.welcome_panel != null)
                {
                    refLogin.welcome_panel.SetActive(false);
                }
                if (refLogin.activationBtn != null)
                {
                    refLogin.activationBtn.interactable = true;
                }
            }
            SetActivationState(false);
            RefreshCredentialTexts();
            return;
        }
        if (sessionStartMode == SessionStartMode.LoadActivationCodeOnly)
        {
            SetActivationState(activationVerified);
            return;
        }
        if (activationVerified && !string.IsNullOrEmpty(userData.userPin))
        {
            _ = RunAutoLoginStartupAsync(userData.activationCode, userData.userPin);
            return;
        }
        SetActivationState(activationVerified);
    }

    private void ApplyTrustedHubSession()
    {
        if (ControlCenter.Instance == null || ControlCenter.Instance.persistantDataManager == null)
        {
            return;
        }
        UserPersistantData userData = ControlCenter.Instance.persistantDataManager.userPersistantData;
        if (string.IsNullOrEmpty(userData.activationCode))
        {
            SetActivationState(false);
            return;
        }
        userData.isLicenseValid = true;
        userData.isPlayerLoggedIn = !string.IsNullOrEmpty(userData.participantID);
        ControlCenter.Instance.persistantDataManager.userPersistantData = userData;
        SetActivationState(true);
        RefreshCredentialTexts();
        if (!userData.isPlayerLoggedIn)
        {
            return;
        }
        loginReady = true;
        if (refLogin != null && refLogin.pinCheck != null)
        {
            refLogin.pinCheck.SetActive(true);
        }
        if (refLogin != null && refLogin.activationBtn != null)
        {
            refLogin.activationBtn.interactable = false;
        }
        if (refLogin != null && refLogin.pinBtn != null)
        {
            refLogin.pinBtn.interactable = false;
        }
        _ = ShowWelcomeAndMainMenu(userData.firstName, 0);
        LogInfo("ApplyTrustedHubSession", "Sesion iniciada desde argumentos HUB sin verificacion remota.");
    }

    private async Task RunAutoLoginStartupAsync(string activationCode, string userPin)
    {
        bool autoLoginSuccess = false;
        async Task ValidateSessionAsync()
        {
            Task<bool> loginTask = TryAutoLoginAsync(activationCode, userPin, false);
            Task minimumDelay = Task.Delay(3000);
            await Task.WhenAll(loginTask, minimumDelay);
            autoLoginSuccess = loginTask.Result;
        }
        if (ControlCenter.Instance != null && ControlCenter.Instance.loadManager != null)
        {
            await ControlCenter.Instance.loadManager.WaitTasks_VisualProgress("Verificando sesion...", ValidateSessionAsync);
        }
        else
        {
            await ValidateSessionAsync();
        }
        if (!autoLoginSuccess)
        {
            HandleAutoLoginFailure("No se pudo validar sesion en AutoStartSession.");
            return;
        }
        string firstName = string.Empty;
        if (ControlCenter.Instance != null && ControlCenter.Instance.persistantDataManager != null)
        {
            firstName = ControlCenter.Instance.persistantDataManager.userPersistantData.firstName;
        }
        if (refLogin != null)
        {
            refLogin.gameObject.SetActive(true);
        }
        await ShowWelcomeAndMainMenu(firstName, 0);
        LogInfo("RunAutoLoginStartupAsync", "Auto login exitoso. Se mostro bienvenida antes del menu principal.");
    }

    private async Task<bool> TryAutoLoginAsync(string activationCode, string userPin, bool showWelcomePanel)
    {
        bool activationOk = await VerifyActivationCodeAsync(activationCode, false);
        if (!activationOk || !IsLicenseValid())
        {
            return false;
        }
        return await VerifyUserPinAsync(userPin, showWelcomePanel, false);
    }

    private void BindButtons()
    {
        if (refLogin.activationBtn != null)
        {
            refLogin.activationBtn.onClick.RemoveAllListeners();
            refLogin.activationBtn.onClick.AddListener(OnActivationButtonClicked);
        }
        if (refLogin.pinBtn != null)
        {
            refLogin.pinBtn.onClick.RemoveAllListeners();
            refLogin.pinBtn.onClick.AddListener(OnPinButtonClicked);
        }
        if (refLogin.closePinPanelBtn != null)
        {
            refLogin.closePinPanelBtn.onClick.RemoveAllListeners();
            refLogin.closePinPanelBtn.onClick.AddListener(() => SetPinPanelVisible(false));
        }
        BindKeypadButton(refLogin.Btn_0, "0");
        BindKeypadButton(refLogin.Btn_1, "1");
        BindKeypadButton(refLogin.Btn_2, "2");
        BindKeypadButton(refLogin.Btn_3, "3");
        BindKeypadButton(refLogin.Btn_4, "4");
        BindKeypadButton(refLogin.Btn_5, "5");
        BindKeypadButton(refLogin.Btn_6, "6");
        BindKeypadButton(refLogin.Btn_7, "7");
        BindKeypadButton(refLogin.Btn_8, "8");
        BindKeypadButton(refLogin.Btn_9, "9");
        if (refLogin.Btn_clear != null)
        {
            refLogin.Btn_clear.onClick.RemoveAllListeners();
            refLogin.Btn_clear.onClick.AddListener(ClearInput);
        }
        if (refLogin.Btn_enter != null)
        {
            refLogin.Btn_enter.onClick.RemoveAllListeners();
            refLogin.Btn_enter.onClick.AddListener(() => _ = SubmitPinAsync());
        }
    }

    private void BindKeypadButton(Button button, string value)
    {
        if (button == null)
        {
            return;
        }
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => AppendInput(value));
    }

    private void SetLoginStatusText()
    {
        if (refLogin != null && refLogin.login_status != null)
        {
            refLogin.login_status.text = Translate("login_status");
        }
    }

    private void OnActivationButtonClicked()
    {
        inputStage = InputStage.ActivationCode;
        currentInput = string.Empty;
        clearDisplayOnNextInput = false;
        SetPinPanelVisible(true);
        UpdateDisplay(currentInput, defaultDisplayColor);
    }

    private void OnPinButtonClicked()
    {
        if (!IsLicenseValid())
        {
            return;
        }
        inputStage = InputStage.UserPin;
        currentInput = string.Empty;
        clearDisplayOnNextInput = false;
        SetPinPanelVisible(true);
        UpdateDisplay(currentInput, defaultDisplayColor);
    }

    private void AppendInput(string value)
    {
        if (isRequestRunning)
        {
            return;
        }
        if (clearDisplayOnNextInput)
        {
            currentInput = string.Empty;
            clearDisplayOnNextInput = false;
            UpdateDisplay(currentInput, defaultDisplayColor);
        }
        if (currentInput.Length >= MaxPinPanelInputLength)
        {
            return;
        }
        currentInput += value;
        UpdateDisplay(currentInput, defaultDisplayColor);
    }

    private void ClearInput()
    {
        if (isRequestRunning)
        {
            return;
        }
        if (clearDisplayOnNextInput)
        {
            clearDisplayOnNextInput = false;
            currentInput = string.Empty;
            UpdateDisplay(currentInput, defaultDisplayColor);
            return;
        }
        if (!string.IsNullOrEmpty(currentInput))
        {
            currentInput = currentInput.Substring(0, currentInput.Length - 1);
        }
        UpdateDisplay(currentInput, defaultDisplayColor);
    }

    private async Task SubmitPinAsync()
    {
        if (isRequestRunning || string.IsNullOrEmpty(currentInput))
        {
            return;
        }
        if (inputStage == InputStage.ActivationCode)
        {
            await VerifyActivationCodeAsync(currentInput);
            return;
        }
        if (inputStage == InputStage.UserPin)
        {
            await VerifyUserPinAsync(currentInput);
        }
    }

    private async Task<bool> VerifyActivationCodeAsync(string code, bool manageUiInteractions = true)
    {
        if (string.IsNullOrEmpty(cloudBaseUrl))
        {
            ShowDisplayError(Translate("error"));
            LogError("VerifyActivationCodeAsync", "Proyecto aislado: no hay URL de backend configurada.");
            return false;
        }
        isRequestRunning = true;
        if (manageUiInteractions)
        {
            DisableAllInteractions();
            SetKeypadInteractable(false);
        }
        UpdateDisplay(Translate("verificar"), defaultDisplayColor);
        string endpoint = cloudBaseUrl + "/buscarURL";
        try
        {
            WWWForm formData = new WWWForm();
            formData.AddField("version", Application.version);
            formData.AddField("codigoActivador", code);
            string identifierToSend = simulatorID;
            if (simulatorType == SimulatorType.PackagePart && !string.IsNullOrEmpty(launchResourceIdOverride))
            {
                identifierToSend = launchResourceIdOverride;
                formData.AddField("resourceID", launchResourceIdOverride);
            }
            formData.AddField("identificador", identifierToSend);
            UnityWebRequest request = UnityWebRequest.Post(endpoint, formData);
            var operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
            }
            if (request.responseCode != 200)
            {
                HandleActivationValidationFailure("HTTP response != 200");
                ShowDisplayError(Translate("error"));
                LogRequestFailure("VerifyActivationCodeAsync", endpoint, request);
                return false;
            }
            CloudActivationCodeRecordFormat activationResponse = JsonUtility.FromJson<CloudActivationCodeRecordFormat>(request.downloadHandler.text);
            if (activationResponse == null || string.IsNullOrEmpty(activationResponse.codigoActivacion))
            {
                HandleActivationValidationFailure("Respuesta de activacion vacia o invalida");
                ShowDisplayError(Translate("error"));
                LogError("VerifyActivationCodeAsync", $"Respuesta invalida o vacia. Body={request.downloadHandler.text}");
                return false;
            }
            if (!string.IsNullOrEmpty(activationResponse.fechaVigencia))
            {
                DateTime expirationDate;
                if (!DateTime.TryParse(activationResponse.fechaVigencia, out expirationDate))
                {
                    HandleActivationValidationFailure("No se pudo parsear fechaVigencia");
                    ShowDisplayError(Translate("error"));
                    LogError("VerifyActivationCodeAsync", $"No se pudo parsear fechaVigencia={activationResponse.fechaVigencia}");
                    return false;
                }
                if (DateTime.Compare(DateTime.Now, expirationDate) >= 0)
                {
                    HandleActivationValidationFailure($"Licencia expirada. fechaVigencia={activationResponse.fechaVigencia}");
                    ShowDisplayError(Translate("expired"));
                    return false;
                }
            }
            UserPersistantData userData = ControlCenter.Instance.persistantDataManager.userPersistantData;
            userData.activationCode = activationResponse.codigoActivacion;
            userData.iaCloudUrl = activationResponse.URLMIL;
            userData.isLicenseValid = true;
            ControlCenter.Instance.persistantDataManager.userPersistantData = userData;
            SetActivationState(true);
            RefreshCredentialTexts();
            if (refLogin != null && refLogin.activationCheck != null)
            {
                refLogin.activationCheck.SetActive(true);
            }
            if (refLogin != null && refLogin.activationBtn != null)
            {
                refLogin.activationBtn.interactable = false;
            }
            clearDisplayOnNextInput = false;
            UpdateDisplay(Translate("active"), Color.green);
            LogInfo("VerifyActivationCodeAsync", $"Activacion OK. URLMIL={activationResponse.URLMIL}");
            await Task.Delay(500);
            SetPinPanelVisible(false);
            return true;
        }
        catch (Exception ex)
        {
            HandleActivationValidationFailure("Excepcion en verificacion de activacion");
            ShowDisplayError(Translate("error"));
            LogError("VerifyActivationCodeAsync", $"Excepcion al verificar codigo de activacion. endpoint={endpoint} ex={ex}");
            return false;
        }
        finally
        {
            if (manageUiInteractions)
            {
                SetKeypadInteractable(true);
                EnableAllInteractions();
            }
            isRequestRunning = false;
        }
    }

    private async Task<bool> VerifyUserPinAsync(string pinCode, bool showWelcomePanel = true, bool manageUiInteractions = true)
    {
        UserPersistantData userData = ControlCenter.Instance.persistantDataManager.userPersistantData;
        if (string.IsNullOrEmpty(userData.activationCode))
        {
            ShowDisplayError(Translate("error"));
            LogError("VerifyUserPinAsync", "Se intento login sin activationCode cargado.");
            return false;
        }
        isRequestRunning = true;
        if (manageUiInteractions)
        {
            DisableAllInteractions();
            SetKeypadInteractable(false);
        }
        UpdateDisplay(Translate("verificar"), defaultDisplayColor);
        string targetUrl = string.IsNullOrEmpty(userData.iaCloudUrl) ? cloudBaseUrl : userData.iaCloudUrl;
        if (string.IsNullOrEmpty(targetUrl))
        {
            ShowDisplayError(Translate("error"));
            LogError("VerifyUserPinAsync", "Proyecto aislado: no hay URL de backend configurada.");
            if (manageUiInteractions)
            {
                SetKeypadInteractable(true);
                EnableAllInteractions();
            }
            isRequestRunning = false;
            return false;
        }
        string endpoint = targetUrl + "/loginSim";
        try
        {
            WWWForm formData = new WWWForm();
            formData.AddField("pin", pinCode);
            formData.AddField("codigoActivacion", userData.activationCode);
            UnityWebRequest request = UnityWebRequest.Post(endpoint, formData);
            var operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
            }
            if (request.responseCode != 200)
            {
                ShowDisplayError(Translate("error"));
                LogRequestFailure("VerifyUserPinAsync", endpoint, request);
                return false;
            }
            CloudUserPINRecordFormat userResponse = JsonUtility.FromJson<CloudUserPINRecordFormat>(request.downloadHandler.text);
            if (userResponse == null || string.IsNullOrEmpty(userResponse.IDParticipante))
            {
                ShowDisplayError(Translate("error"));
                LogError("VerifyUserPinAsync", $"Respuesta invalida o incompleta. Body={request.downloadHandler.text}");
                return false;
            }
            userData.participantID = userResponse.IDParticipante;
            userData.resourceID = userResponse.IDRecurso;
            userData.firstName = userResponse.firstName;
            userData.lastName = userResponse.lastName;
            userData.userPin = pinCode;
            userData.isPlayerLoggedIn = true;
            userData.isLicenseValid = true;
            ControlCenter.Instance.persistantDataManager.userPersistantData = userData;
            loginReady = true;
            RefreshCredentialTexts();
            if (refLogin != null && refLogin.pinCheck != null)
            {
                refLogin.pinCheck.SetActive(true);
            }
            if (refLogin != null && refLogin.pinBtn != null)
            {
                refLogin.pinBtn.interactable = false;
            }
            clearDisplayOnNextInput = false;
            UpdateDisplay(Translate("active"), Color.green);
            SetPinPanelVisible(false);
            if (showWelcomePanel)
            {
                await ShowWelcomeAndMainMenu(userData.firstName);
            }
            LogInfo("VerifyUserPinAsync", $"Login PIN OK para participante={userResponse.IDParticipante}");
            return true;
        }
        catch (Exception ex)
        {
            ShowDisplayError(Translate("error"));
            LogError("VerifyUserPinAsync", $"Excepcion durante login PIN. endpoint={endpoint} ex={ex}");
            return false;
        }
        finally
        {
            if (manageUiInteractions)
            {
                SetKeypadInteractable(true);
                EnableAllInteractions();
            }
            isRequestRunning = false;
        }
    }

    private async Task ShowWelcomeAndMainMenu(string firstName, int delayBeforeWelcomeMs = 2000)
    {
        if (delayBeforeWelcomeMs > 0)
        {
            await Task.Delay(delayBeforeWelcomeMs);
        }
        if (refLogin != null && refLogin.welcome_text != null)
        {
            string prefix = Translate("first_welcome");
            refLogin.welcome_text.text = string.IsNullOrEmpty(firstName) ? prefix : prefix + " " + firstName;
        }
        if (refLogin != null && refLogin.welcome_panel != null)
        {
            refLogin.welcome_panel.SetActive(true);
        }
        await Task.Delay(3000);
        if (refLogin != null && refLogin.welcome_panel != null)
        {
            refLogin.welcome_panel.SetActive(false);
        }
        if (refLogin != null)
        {
            refLogin.gameObject.SetActive(false);
        }
        SetMainMenuActive(true);
        SetLogoutMenuPanelVisible(connectionMode != ConnectionMode.Offline);
        if (simulatorType == SimulatorType.PackagePart && packagePartOpenSceneViewerAfterLogin)
        {
            ref_SceneViewer sceneViewer = ComponentFinder.FindComponentInScene<ref_SceneViewer>();
            if (sceneViewer != null)
            {
                sceneViewer.gameObject.SetActive(true);
                SetMainMenuActive(false);
            }
        }
    }

    private void SetActivationState(bool activated)
    {
        UserPersistantData userData = ControlCenter.Instance.persistantDataManager.userPersistantData;
        if (refLogin != null && refLogin.activationCheck != null)
        {
            refLogin.activationCheck.SetActive(activated);
        }
        if (refLogin != null && refLogin.pinBtn != null)
        {
            refLogin.pinBtn.interactable = activated;
        }
        if (refLogin != null && refLogin.pinCheck != null)
        {
            refLogin.pinCheck.SetActive(userData.isPlayerLoggedIn);
        }
        LogInfo("SetActivationState", $"activated={activated}, isPlayerLoggedIn={userData.isPlayerLoggedIn}");
    }

    private bool IsLicenseValid()
    {
        return ControlCenter.Instance.persistantDataManager.userPersistantData.isLicenseValid;
    }

    private bool HasPackageAccess()
    {
        if (string.IsNullOrEmpty(packagePassArg))
        {
            return true;
        }
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject intent = currentActivity.Call<AndroidJavaObject>("getIntent");
        return intent.Call<bool>("getBooleanExtra", packagePassArg, false);
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].Equals("-" + packagePassArg + "=true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
#else
        return false;
#endif
    }

    private bool IsHubSimulatorInstalled(int carouselIndex)
    {
        string packageName = GetHubSimulatorPackageName(carouselIndex);
        if (string.IsNullOrEmpty(packageName))
        {
            return false;
        }
#if UNITY_ANDROID
        try
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager");
            AndroidJavaObject launchIntent = packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", packageName);
            return launchIntent != null;
        }
        catch (Exception ex)
        {
            LogError("IsHubSimulatorInstalled", $"Error consultando paquete {packageName}. ex={ex.Message}");
            return false;
        }
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        HubSimulatorLaunchData launchData = ResolveHubLaunchData(carouselIndex);
        if (launchData == null || string.IsNullOrEmpty(launchData.windowsExecutableRelativePath))
        {
            return false;
        }
        string simsPath = Path.Combine(Application.dataPath, "../sims");
        string exePath = Path.Combine(simsPath, launchData.windowsExecutableRelativePath.TrimStart('/', '\\'));
        return File.Exists(exePath);
#else
        return false;
#endif
    }

    private void OpenHubSimulatorByCarouselIndex(int carouselIndex)
    {
        HubSimulatorLaunchData launchData = ResolveHubLaunchData(carouselIndex);
        if (launchData == null || string.IsNullOrWhiteSpace(launchData.packageName))
        {
            LogError("OpenHubSimulatorByCarouselIndex", $"No hay paquete HUB para index={carouselIndex}. Configura Hub Dependent Packages o Hub Simulator Launch Data alineado al carrusel.");
            SetCurrentCarouselNotInstalledVisible(true);
            return;
        }
        if (!IsHubSimulatorInstalled(carouselIndex))
        {
            SetCurrentCarouselNotInstalledVisible(true);
            return;
        }
        SetCurrentCarouselNotInstalledVisible(false);
        UserPersistantData userData = ControlCenter.Instance != null && ControlCenter.Instance.persistantDataManager != null
            ? ControlCenter.Instance.persistantDataManager.userPersistantData
            : new UserPersistantData();
        string milUrl = string.IsNullOrEmpty(userData.iaCloudUrl) ? cloudBaseUrl : userData.iaCloudUrl;
        string targetPackage = launchData.packageName.Trim();

#if UNITY_ANDROID
        try
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager");
            AndroidJavaObject launchIntent = packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", targetPackage);

            void ApplyHubIntentExtras(AndroidJavaObject intentObj)
            {
                void PutExtra(string key, string value)
                {
                    intentObj.Call<AndroidJavaObject>("putExtra", key, value ?? string.Empty);
                }

                PutExtra("participantID", userData.participantID);
                PutExtra("milURL", milUrl);
                PutExtra("activationCodeHub", launchData.activationCodeHub);
                PutExtra("resourceID", launchData.resourceID);
                PutExtra("userPIN", userData.userPin);
                intentObj.Call<AndroidJavaObject>("putExtra", packagePassArg, true);
                PutExtra("name", userData.firstName);
                PutExtra("lastname", userData.lastName);
            }

            if (launchIntent != null)
            {
                launchIntent.Call<AndroidJavaObject>("addFlags", 0x10000000);
                ApplyHubIntentExtras(launchIntent);
                currentActivity.Call("startActivity", launchIntent);
                LogInfo("OpenHubSimulatorByCarouselIndex", $"startActivity OK -> {targetPackage} (launcher intent del sistema)");
            }
            else
            {
                LogInfo("OpenHubSimulatorByCarouselIndex", $"getLaunchIntentForPackage null para {targetPackage}, probando activities Unity explicitas.");
                string[] activityCandidates;
                if (!string.IsNullOrWhiteSpace(launchData.androidActivityName))
                {
                    activityCandidates = new[] { launchData.androidActivityName.Trim() };
                }
                else
                {
                    activityCandidates = new[]
                    {
                        "com.unity3d.player.UnityPlayerGameActivity",
                        "com.unity3d.player.UnityPlayerActivity"
                    };
                }

                Exception lastEx = null;
                bool started = false;
                foreach (string activityName in activityCandidates)
                {
                    try
                    {
                        AndroidJavaObject explicitIntent = new AndroidJavaObject("android.content.Intent");
                        explicitIntent.Call<AndroidJavaObject>("setAction", "android.intent.action.MAIN");
                        explicitIntent.Call<AndroidJavaObject>("addCategory", "android.intent.category.LAUNCHER");
                        explicitIntent.Call<AndroidJavaObject>("setFlags", 0x10000000);
                        AndroidJavaObject componentName = new AndroidJavaObject("android.content.ComponentName", targetPackage, activityName);
                        explicitIntent.Call<AndroidJavaObject>("setComponent", componentName);
                        ApplyHubIntentExtras(explicitIntent);
                        currentActivity.Call("startActivity", explicitIntent);
                        LogInfo("OpenHubSimulatorByCarouselIndex", $"startActivity OK -> {targetPackage} activity={activityName}");
                        started = true;
                        break;
                    }
                    catch (Exception ex)
                    {
                        lastEx = ex;
                        LogInfo("OpenHubSimulatorByCarouselIndex", $"Intent explicito fallo con {activityName}: {ex.Message}");
                    }
                }

                if (!started)
                {
                    LogError("OpenHubSimulatorByCarouselIndex", $"No se pudo abrir paquete {targetPackage} con ninguna activity candidata. lastEx={lastEx}");
                }
            }
        }
        catch (Exception ex)
        {
            LogError("OpenHubSimulatorByCarouselIndex", $"No se pudo abrir paquete {targetPackage}. ex={ex}");
        }
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        try
        {
            string simsPath = Path.Combine(Application.dataPath, "../sims");
            string executableRelativePath = launchData.windowsExecutableRelativePath != null
                ? launchData.windowsExecutableRelativePath.TrimStart('/', '\\')
                : string.Empty;
            string exePath = Path.Combine(simsPath, executableRelativePath);
            if (!File.Exists(exePath))
            {
                LogError("OpenHubSimulatorByCarouselIndex", $"No se encontro ejecutable en {exePath}");
                return;
            }
            string args = string.Join(" ",
                $"-participantID=\"{userData.participantID}\"",
                $"-milURL=\"{milUrl}\"",
                $"-activationCodeHub=\"{launchData.activationCodeHub}\"",
                $"-resourceID=\"{launchData.resourceID}\"",
                $"-userPIN=\"{userData.userPin}\"",
                $"-{packagePassArg}=true",
                $"-name=\"{userData.firstName}\"",
                $"-lastname=\"{userData.lastName}\"");
            Process.Start(exePath, args);
        }
        catch (Exception ex)
        {
            LogError("OpenHubSimulatorByCarouselIndex", $"No se pudo abrir simulador en Windows. ex={ex}");
        }
#endif
    }

    private bool ShouldLaunchExternalHub(int carouselIndex)
    {
        if (connectionMode == ConnectionMode.Offline)
        {
            return false;
        }

        HubSimulatorLaunchData launchData = ResolveHubLaunchData(carouselIndex);
        return launchData != null && !string.IsNullOrWhiteSpace(launchData.packageName);
    }

    private void LoadLocalSimulationScene()
    {
        SetCurrentCarouselNotInstalledVisible(false);
        LoadSimulationSceneByBuildIndex(LotoSimulationConstants.LotoHubBuildIndex);
    }

    private void LoadSimulationSceneByBuildIndex(int buildIndex)
    {
        if (ControlCenter.Instance == null || ControlCenter.Instance.scoreManager == null)
        {
            LogError("LoadSimulationSceneByBuildIndex", "ScoreManager no disponible.");
            return;
        }

        ControlCenter.Instance.scoreManager.LoadSceneByBuildIndex(buildIndex);
    }

    private HubSimulatorLaunchData ResolveHubLaunchData(int carouselIndex)
    {
        if (carouselIndex < 0)
        {
            return null;
        }
        HubSimulatorLaunchData entry = null;
        if (hubSimulators != null && carouselIndex < hubSimulators.Count)
        {
            entry = hubSimulators[carouselIndex];
        }
        string packageFromList = string.Empty;
        if (hubDependentPackages != null && carouselIndex < hubDependentPackages.Count && !string.IsNullOrWhiteSpace(hubDependentPackages[carouselIndex]))
        {
            packageFromList = hubDependentPackages[carouselIndex].Trim();
        }
        if (entry == null)
        {
            if (string.IsNullOrEmpty(packageFromList))
            {
                return null;
            }
            return new HubSimulatorLaunchData { packageName = packageFromList };
        }
        string pkg = !string.IsNullOrWhiteSpace(entry.packageName) ? entry.packageName.Trim() : packageFromList;
        if (string.IsNullOrEmpty(pkg))
        {
            return null;
        }
        if (!string.IsNullOrWhiteSpace(entry.packageName))
        {
            return entry;
        }
        return new HubSimulatorLaunchData
        {
            packageName = pkg,
            activationCodeHub = entry.activationCodeHub,
            resourceID = entry.resourceID,
            androidActivityName = entry.androidActivityName,
            windowsExecutableRelativePath = entry.windowsExecutableRelativePath
        };
    }

    private string GetHubSimulatorPackageName(int carouselIndex)
    {
        HubSimulatorLaunchData resolved = ResolveHubLaunchData(carouselIndex);
        if (resolved != null && !string.IsNullOrWhiteSpace(resolved.packageName))
        {
            return resolved.packageName.Trim();
        }
        return string.Empty;
    }

    private void SetCurrentCarouselNotInstalledVisible(bool visible)
    {
        ref_SceneViewer sceneViewer = ComponentFinder.FindComponentInScene<ref_SceneViewer>();
        if (sceneViewer == null || sceneViewer.uIContentCarousel == null || sceneViewer.uIContentCarousel.gridLayoutGroup == null)
        {
            return;
        }
        int currentIndex = sceneViewer.uIContentCarousel.currentIndex;
        Transform root = sceneViewer.uIContentCarousel.gridLayoutGroup.transform;
        if (currentIndex < 0 || currentIndex >= root.childCount)
        {
            return;
        }
        ref_CarouselElement carouselElement = root.GetChild(currentIndex).GetComponent<ref_CarouselElement>();
        if (carouselElement != null && carouselElement.contentNotInstalled != null)
        {
            carouselElement.contentNotInstalled.SetActive(visible);
        }
    }

    private void TryConsumeExternalLaunchArgumentsForPackagePart()
    {
        if (simulatorType != SimulatorType.PackagePart || ControlCenter.Instance == null || ControlCenter.Instance.persistantDataManager == null)
        {
            return;
        }
        string participantId = GetLaunchArgValue("participantID");
        string milUrl = GetLaunchArgValue("milURL");
        string activationCodeHub = GetLaunchArgValue("activationCodeHub");
        string resourceId = GetLaunchArgValue("resourceID");
        string firstName = GetLaunchArgValue("name");
        string lastName = GetLaunchArgValue("lastname");
        string userPin = GetLaunchArgValue("userPIN");

        bool hasRelevantArg = !string.IsNullOrEmpty(participantId) || !string.IsNullOrEmpty(activationCodeHub) || !string.IsNullOrEmpty(userPin);
        if (!hasRelevantArg)
        {
            receivedHubLaunchArgs = false;
            return;
        }
        receivedHubLaunchArgs = true;
        UserPersistantData userData = ControlCenter.Instance.persistantDataManager.userPersistantData;
        if (!string.IsNullOrEmpty(activationCodeHub))
        {
            userData.activationCode = activationCodeHub;
            userData.isLicenseValid = true;
            launchResourceIdOverride = activationCodeHub;
        }
        else if (!string.IsNullOrEmpty(resourceId))
        {
            launchResourceIdOverride = resourceId;
        }
        if (!string.IsNullOrEmpty(milUrl))
        {
            userData.iaCloudUrl = milUrl;
        }
        if (!string.IsNullOrEmpty(participantId))
        {
            userData.participantID = participantId;
        }
        if (!string.IsNullOrEmpty(resourceId))
        {
            userData.resourceID = resourceId;
        }
        if (!string.IsNullOrEmpty(firstName))
        {
            userData.firstName = firstName;
        }
        if (!string.IsNullOrEmpty(lastName))
        {
            userData.lastName = lastName;
        }
        if (!string.IsNullOrEmpty(userPin))
        {
            userData.userPin = userPin;
        }
        userData.isPlayerLoggedIn = false;
        ControlCenter.Instance.persistantDataManager.userPersistantData = userData;
        LogInfo("TryConsumeExternalLaunchArgumentsForPackagePart", "Se cargaron argumentos externos de HUB para login.");
    }

    private string GetLaunchArgValue(string key)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject intent = currentActivity.Call<AndroidJavaObject>("getIntent");
            bool hasExtra = intent.Call<bool>("hasExtra", key);
            if (!hasExtra)
            {
                return string.Empty;
            }
            AndroidJavaObject extras = intent.Call<AndroidJavaObject>("getExtras");
            string value = extras.Call<string>("getString", key);
            return value ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        string prefix = "-" + key + "=";
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return args[i].Substring(prefix.Length).Trim('"');
            }
        }
        return string.Empty;
#else
        return string.Empty;
#endif
    }

    private string ResolveCloudUrl()
    {
        if (urlType == UrlType.Custom && !string.IsNullOrEmpty(customUrl))
        {
            return customUrl.TrimEnd('/');
        }
        if (urlType == UrlType.Production)
        {
            return string.IsNullOrEmpty(ProductionURL) ? string.Empty : ProductionURL;
        }
        return string.IsNullOrEmpty(DevelopURL) ? string.Empty : DevelopURL;
    }

    private void SetPinPanelVisible(bool visible)
    {
        if (refLogin == null)
        {
            return;
        }
        if (refLogin.pin_panel != null)
        {
            refLogin.pin_panel.SetActive(visible);
            return;
        }
        if (refLogin.displayPin != null)
        {
            Transform panelRoot = refLogin.displayPin.transform.parent != null ? refLogin.displayPin.transform.parent.parent : null;
            if (panelRoot != null)
            {
                panelRoot.gameObject.SetActive(visible);
            }
        }
    }

    private void SetMainMenuActive(bool value)
    {
        if (refMainMenu != null)
        {
            refMainMenu.gameObject.SetActive(value);
        }
    }

    private void SetLogoutMenuPanelVisible(bool visible)
    {
        if (refLogout != null && refLogout.logout_menu_panel != null)
        {
            refLogout.logout_menu_panel.SetActive(visible);
        }
        if (refLogout != null && !visible)
        {
            refLogout.gameObject.SetActive(false);
        }
    }

    private void SetKeypadInteractable(bool enabled)
    {
        if (refLogin == null)
        {
            return;
        }
        SetButtonState(refLogin.Btn_0, enabled);
        SetButtonState(refLogin.Btn_1, enabled);
        SetButtonState(refLogin.Btn_2, enabled);
        SetButtonState(refLogin.Btn_3, enabled);
        SetButtonState(refLogin.Btn_4, enabled);
        SetButtonState(refLogin.Btn_5, enabled);
        SetButtonState(refLogin.Btn_6, enabled);
        SetButtonState(refLogin.Btn_7, enabled);
        SetButtonState(refLogin.Btn_8, enabled);
        SetButtonState(refLogin.Btn_9, enabled);
        SetButtonState(refLogin.Btn_clear, enabled);
        SetButtonState(refLogin.Btn_enter, enabled);
        SetButtonState(refLogin.closePinPanelBtn, enabled);
    }

    private void SetButtonState(Button button, bool enabled)
    {
        if (button != null)
        {
            button.interactable = enabled;
        }
    }

    private void UpdateDisplay(string value, Color color)
    {
        if (refLogin == null || refLogin.displayPin == null)
        {
            return;
        }
        refLogin.displayPin.color = color;
        refLogin.displayPin.text = value;
    }

    private void ShowDisplayError(string errorText)
    {
        clearDisplayOnNextInput = true;
        UpdateDisplay(errorText, Color.red);
    }

    private void RefreshCredentialTexts()
    {
        if (ControlCenter.Instance == null || ControlCenter.Instance.persistantDataManager == null)
        {
            return;
        }
        UserPersistantData userData = ControlCenter.Instance.persistantDataManager.userPersistantData;
        if (refLogin != null && refLogin.activation_text != null)
        {
            refLogin.activation_text.text = string.IsNullOrEmpty(userData.activationCode) ? "-" : userData.activationCode;
        }
        if (refLogin != null && refLogin.pin_text != null)
        {
            refLogin.pin_text.text = string.IsNullOrEmpty(userData.userPin) ? "-" : userData.userPin;
        }
        if (refLogout != null && refLogout.text_userdata != null)
        {
            string displayName = string.Empty;
            if (LotoVrLoginController.Instance != null
                && LotoVrLoginController.Instance.IsLoggedIn
                && LotoVrLoginController.Instance.Session != null)
            {
                displayName = LotoVrLoginController.Instance.Session.username ?? string.Empty;
            }

            if (string.IsNullOrEmpty(displayName))
            {
                displayName = $"{userData.firstName} {userData.lastName}".Trim();
            }

            if (string.IsNullOrEmpty(displayName))
            {
                displayName = "-";
            }

            refLogout.text_userdata.text = displayName;
        }
    }

    private void HandleActivationValidationFailure(string reason)
    {
        ClearPersistedLoginData(clearActivationCode: true);
        loginReady = false;
        SetMainMenuActive(false);
        if (refLogin != null)
        {
            refLogin.gameObject.SetActive(true);
            if (refLogin.welcome_panel != null)
            {
                refLogin.welcome_panel.SetActive(false);
            }
            if (refLogin.activationBtn != null)
            {
                refLogin.activationBtn.interactable = true;
            }
        }
        SetActivationState(false);
        RefreshCredentialTexts();
        LogError("HandleActivationValidationFailure", $"Se limpio sesion por fallo de activacion. reason={reason}");
    }

    private void ClearPersistedLoginData(bool clearActivationCode)
    {
        if (ControlCenter.Instance == null || ControlCenter.Instance.persistantDataManager == null)
        {
            return;
        }
        UserPersistantData userData = ControlCenter.Instance.persistantDataManager.userPersistantData;
        if (clearActivationCode)
        {
            userData.activationCode = string.Empty;
            userData.iaCloudUrl = string.Empty;
        }
        userData.participantID = string.Empty;
        userData.resourceID = string.Empty;
        userData.firstName = string.Empty;
        userData.lastName = string.Empty;
        userData.userPin = string.Empty;
        userData.isLicenseValid = false;
        userData.isPlayerLoggedIn = false;
        ControlCenter.Instance.persistantDataManager.userPersistantData = userData;
    }

    private void HandleAutoLoginFailure(string reason)
    {
        ClearPersistedLoginData(clearActivationCode: true);
        loginReady = false;
        SetMainMenuActive(false);
        if (refLogin != null)
        {
            refLogin.gameObject.SetActive(true);
            if (refLogin.welcome_panel != null)
            {
                refLogin.welcome_panel.SetActive(false);
            }
            if (refLogin.activationBtn != null)
            {
                refLogin.activationBtn.interactable = true;
            }
            if (refLogin.pinBtn != null)
            {
                refLogin.pinBtn.interactable = false;
            }
        }
        currentInput = string.Empty;
        clearDisplayOnNextInput = false;
        SetPinPanelVisible(false);
        UpdateDisplay(currentInput, defaultDisplayColor);
        SetActivationState(false);
        RefreshCredentialTexts();
        LogError("HandleAutoLoginFailure", reason);
    }

    private string Translate(string id)
    {
        if (ControlCenter.Instance == null || ControlCenter.Instance.languageManager == null)
        {
            return id;
        }
        return ControlCenter.Instance.languageManager.GetText(id);
    }

    private void DisableAllInteractions()
    {
        UIInteractable[] uiInteractables = FindObjectsByType<UIInteractable>(FindObjectsSortMode.None);
        for (int i = 0; i < uiInteractables.Length; i++)
        {
            uiInteractables[i].DisableInteractions();
        }
    }

    private void EnableAllInteractions()
    {
        UIInteractable[] uiInteractables = FindObjectsByType<UIInteractable>(FindObjectsSortMode.None);
        for (int i = 0; i < uiInteractables.Length; i++)
        {
            uiInteractables[i].EnableInteractions();
        }
    }

    private void LogRequestFailure(string context, string endpoint, UnityWebRequest request)
    {
        string body = request != null && request.downloadHandler != null ? request.downloadHandler.text : string.Empty;
        string error = request != null ? request.error : "null request";
        long responseCode = request != null ? request.responseCode : -1;
        string result = request != null ? request.result.ToString() : "Unknown";
        LogError(context, $"HTTP fallo. endpoint={endpoint}, code={responseCode}, result={result}, error={error}, body={body}");
    }

    private void LogInfo(string context, string message)
    {
        if (!verboseLoginLogs)
        {
            return;
        }
        UnityEngine.Debug.Log($"[IBManager::{context}] {message}");
    }

    private void LogError(string context, string message)
    {
        UnityEngine.Debug.LogError($"[IBManager::{context}] {message}");
    }
}
