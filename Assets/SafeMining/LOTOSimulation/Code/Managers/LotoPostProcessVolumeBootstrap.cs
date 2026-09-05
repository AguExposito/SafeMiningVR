using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class LotoPostProcessVolumeBootstrap
{
    private const string VolumeProfileResourcePath = "VOL_LotoUnderground";
    private const string VolumeObjectName = "LotoPostProcessVolume";

    #region Public API

    public static void EnsureActive()
    {
        EnsureCameraPostProcessing();
        EnsureGlobalVolume();
    }

    #endregion

    #region Internals

    private static void EnsureCameraPostProcessing()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return;
        }

        UniversalAdditionalCameraData cameraData = mainCamera.GetUniversalAdditionalCameraData();
        if (cameraData == null)
        {
            return;
        }

        cameraData.renderPostProcessing = true;
    }

    private static void EnsureGlobalVolume()
    {
        VolumeProfile profile = Resources.Load<VolumeProfile>(VolumeProfileResourcePath);
        if (profile == null)
        {
            return;
        }

        Volume existingVolume = FindGlobalVolume();
        if (existingVolume != null)
        {
            if (existingVolume.sharedProfile == null)
            {
                existingVolume.sharedProfile = profile;
            }

            return;
        }

        GameObject volumeObject = new GameObject(VolumeObjectName);
        Volume globalVolume = volumeObject.AddComponent<Volume>();
        globalVolume.isGlobal = true;
        globalVolume.priority = 15f;
        globalVolume.weight = 1f;
        globalVolume.sharedProfile = profile;
    }

    private static Volume FindGlobalVolume()
    {
        Volume[] volumes = Object.FindObjectsByType<Volume>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < volumes.Length; i++)
        {
            Volume volume = volumes[i];
            if (volume != null && volume.isGlobal)
            {
                return volume;
            }
        }

        return null;
    }

    #endregion
}
