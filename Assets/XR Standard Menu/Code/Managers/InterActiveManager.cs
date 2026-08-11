using UnityEngine;

/// <summary>
/// Legacy stub kept for scene compatibility. No remote endpoints or package IPC.
/// Prefer IBManager for hub/login flow once a new backend is connected.
/// </summary>
public class InterActiveManager : MonoBehaviour
{
    public string keypass;
    public IActiveMode activeMode = IActiveMode.Offline;
    public string cache_arg;

    public void Initialize()
    {
        // Isolated: no network or external launch handling.
    }

    public void InitializeAsPackagePart()
    {
        // Isolated: package-part IPC disabled.
    }

    public void InitializeAsOnline()
    {
        // Isolated: online path disabled until a new backend is wired.
    }

    public enum IActiveMode
    {
        DEMO,
        Online,
        Offline,
        PackagePart,
        HUB
    }

    public class ExternalSimulatorInfo
    {
        public string user_id = "";
        public string ActivationCode = "";
    }
}
