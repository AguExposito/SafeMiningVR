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
        DisconnectHandsHelp();
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
            DisconnectHandsHelp();
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
        UpdateLogoutUserDataText();
        #endregion

        #region SceneViewer Interactions
        ref_sceneViewer.go_btn_back.GetComponent<Button>().onClick.AddListener(() =>
        {
            ref_mainMenu.gameObject.SetActive(true);
            ref_sceneViewer.gameObject.SetActive(false);   
        });
        if (ref_sceneViewer.GetComponent<SimulationCarouselPresenter>() == null)
        {
            ref_sceneViewer.gameObject.AddComponent<SimulationCarouselPresenter>();
        }
        ref_sceneViewer.GetComponent<SimulationCarouselPresenter>().Configure(ref_sceneViewer);
        ref_sceneViewer.go_btn_select.GetComponent<Button>().onClick.AddListener(() =>
        {
            SimulationCarouselPresenter presenter = ref_sceneViewer.GetComponent<SimulationCarouselPresenter>();
            if (presenter != null && !presenter.IsCurrentPlayable())
            {
                presenter.SimulateCurrentScore();
                return;
            }

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
        DisconnectHandsHelp();
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

    public void UpdateLogoutUserDataText()
    {
        if (ref_logout == null || ref_logout.text_userdata == null)
        {
            return;
        }

        string displayName = ResolveLoggedInDisplayName();
        ref_logout.text_userdata.text = string.IsNullOrEmpty(displayName) ? "-" : displayName;
    }

    private static string ResolveLoggedInDisplayName()
    {
        if (LotoVrLoginController.Instance != null
            && LotoVrLoginController.Instance.IsLoggedIn
            && LotoVrLoginController.Instance.Session != null
            && !string.IsNullOrEmpty(LotoVrLoginController.Instance.Session.username))
        {
            return LotoVrLoginController.Instance.Session.username;
        }

        if (ControlCenter.Instance == null || ControlCenter.Instance.persistantDataManager == null)
        {
            return string.Empty;
        }

        UserPersistantData userData = ControlCenter.Instance.persistantDataManager.userPersistantData;
        string fullName = $"{userData.firstName} {userData.lastName}".Trim();
        if (!string.IsNullOrEmpty(fullName))
        {
            return fullName;
        }

        if (!string.IsNullOrEmpty(userData.userPin))
        {
            return userData.userPin;
        }

        return string.Empty;
    }

    #region Controls Help

    private void DisconnectHandsHelp()
    {
        if (ref_controls == null)
        {
            return;
        }

        if (ref_controls.go_panel_contentHands != null)
        {
            ref_controls.go_panel_contentHands.SetActive(false);
        }

        if (ref_controls.go_btn_handSwitch != null)
        {
            ref_controls.go_btn_handSwitch.SetActive(false);
        }

        if (ref_controls.go_btn_controlsSwitch != null)
        {
            ref_controls.go_btn_controlsSwitch.SetActive(false);
        }

        if (ref_controls.go_panel_contentVR != null)
        {
            ref_controls.go_panel_contentVR.SetActive(true);
        }
    }

    #endregion
}
