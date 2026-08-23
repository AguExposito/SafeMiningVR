using UnityEngine;
using UnityEngine.UI;
using NJ.Finder;

public class MenuInteractionManager : MonoBehaviour
{
    ref_MainMenu ref_mainMenu;
    ref_SceneViewer ref_sceneViewer;
    ref_Settings ref_settings;
    ref_Controls ref_controls;
    ref_Exit ref_exit;
    ref_Logout ref_logout;
    ControlCenter controlCenter;
    void Awake()
    {
        ref_mainMenu = ComponentFinder.FindComponentInScene<ref_MainMenu>();
        ref_sceneViewer = ComponentFinder.FindComponentInScene<ref_SceneViewer>();
        ref_settings = ComponentFinder.FindComponentInScene<ref_Settings>();
        ref_controls = ComponentFinder.FindComponentInScene<ref_Controls>();
        ref_exit = ComponentFinder.FindComponentInScene<ref_Exit>();
        ref_logout = ComponentFinder.FindComponentInScene<ref_Logout>();
        controlCenter = ComponentFinder.FindComponentInScene<ControlCenter>();
        Initialize();
        LotoInputModalityVisuals.OnHandsModeChanged += SyncControlsHelpToModality;
        SyncControlsHelpToModality(LotoInputModalityVisuals.IsHandsMode);
    }

    private void OnDestroy()
    {
        LotoInputModalityVisuals.OnHandsModeChanged -= SyncControlsHelpToModality;
    }
    void Initialize()
    {
        #region MainMenu Interactions
        ref_mainMenu.go_btn_play.GetComponent<Button>().onClick.AddListener(() =>
        {
            ref_sceneViewer.gameObject.SetActive(true);
            ref_mainMenu.gameObject.SetActive(false);
            if (ControlCenter.Instance != null && ControlCenter.Instance.ibManager != null)
            {
                ControlCenter.Instance.ibManager.RefreshHubContentAvailability();
            }
        });
        ref_mainMenu.go_btn_settings.GetComponent<Button>().onClick.AddListener(() =>
        {
            ref_settings.gameObject.SetActive(true);
            ref_mainMenu.gameObject.SetActive(false);
        }); 
        ref_mainMenu.go_btn_controls.GetComponent<Button>().onClick.AddListener(() =>
        {
            ref_controls.gameObject.SetActive(true);
            ref_mainMenu.gameObject.SetActive(false);
            SyncControlsHelpToModality(LotoInputModalityVisuals.IsHandsMode);
        });
        ref_mainMenu.go_btn_exit.GetComponent<Button>().onClick.AddListener(() =>
        {
            ref_exit.gameObject.SetActive(true);
        });
        if (ref_mainMenu.Go_btn_logout != null && ref_logout != null)
        {
            ref_mainMenu.Go_btn_logout.GetComponent<Button>().onClick.AddListener(() =>
            {
                UpdateLogoutUserDataText();
                if (ref_logout.logout_menu_panel != null)
                {
                    ref_logout.logout_menu_panel.SetActive(true);
                }
                else
                {
                    ref_logout.gameObject.SetActive(true);
                }
            });
        }
        #endregion

        #region SceneViewer Interactions
        ref_sceneViewer.go_btn_back.GetComponent<Button>().onClick.AddListener(() =>
        {
            ref_mainMenu.gameObject.SetActive(true);
            ref_sceneViewer.gameObject.SetActive(false);   
        });
        ref_sceneViewer.go_btn_select.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (ControlCenter.Instance != null && ControlCenter.Instance.ibManager != null)
            {
                ControlCenter.Instance.ibManager.HandleSceneViewerSelect(ref_sceneViewer != null ? ref_sceneViewer.uIContentCarousel : null);
            }
        });
        #endregion

        #region Settings Interactions
        ref_settings.go_btn_back.GetComponent<Button>().onClick.AddListener(() =>
        {
            ref_mainMenu.gameObject.SetActive(true);
            ref_settings.gameObject.SetActive(false);   
        });
        #endregion

        #region Controls Interactions
        ref_controls.go_btn_back.GetComponent<Button>().onClick.AddListener(() =>
        {
            ref_mainMenu.gameObject.SetActive(true);
            ref_controls.gameObject.SetActive(false);   
        });
        ref_controls.go_btn_handSwitch.GetComponent<Button>().onClick.AddListener(() =>{
            ref_controls.go_panel_contentHands.SetActive(true);
            ref_controls.go_panel_contentVR.SetActive(false);
        });
        ref_controls.go_btn_controlsSwitch.GetComponent<Button>().onClick.AddListener(() =>{
            ref_controls.go_panel_contentVR.SetActive(true);
            ref_controls.go_panel_contentHands.SetActive(false);
        });
        #endregion

        #region Exit Interactions
        ref_exit.go_btn_yes.GetComponent<Button>().onClick.AddListener(() =>
        {
            Application.Quit();
        });
        ref_exit.go_btn_no.GetComponent<Button>().onClick.AddListener(() =>{
            ref_exit.gameObject.SetActive(false);
        });
        #endregion

    }

    private void UpdateLogoutUserDataText()
    {
        if (ref_logout == null || ref_logout.text_userdata == null)
        {
            return;
        }
        if (ControlCenter.Instance == null || ControlCenter.Instance.persistantDataManager == null)
        {
            ref_logout.text_userdata.text = "- / PIN -";
            return;
        }
        UserPersistantData userData = ControlCenter.Instance.persistantDataManager.userPersistantData;
        string fullName = $"{userData.firstName} {userData.lastName}".Trim();
        if (string.IsNullOrEmpty(fullName))
        {
            fullName = "-";
        }
        string pinValue = string.IsNullOrEmpty(userData.userPin) ? "-" : userData.userPin;
        ref_logout.text_userdata.text = $"{fullName} / PIN {pinValue}";
    }

    private void SyncControlsHelpToModality(bool handsMode)
    {
        if (ref_controls == null)
        {
            return;
        }

        if (ref_controls.go_panel_contentHands != null)
        {
            ref_controls.go_panel_contentHands.SetActive(handsMode);
        }

        if (ref_controls.go_panel_contentVR != null)
        {
            ref_controls.go_panel_contentVR.SetActive(!handsMode);
        }
    }
}
