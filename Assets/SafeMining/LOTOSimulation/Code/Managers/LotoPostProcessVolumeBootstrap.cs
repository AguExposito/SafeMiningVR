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
        DisableNearBlur();
    }

    #endregion

    #region Internals

    private static void EnsureCameraPostProcessing()
    {
        Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < cameras.Length; i++)
        {
            Camera camera = cameras[i];
            if (camera == null)
            {
                continue;
            }

            UniversalAdditionalCameraData cameraData = camera.GetUniversalAdditionalCameraData();
            if (cameraData == null || cameraData.renderType != CameraRenderType.Base)
            {
                continue;
            }

            cameraData.renderPostProcessing = true;
        }
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

    private static void DisableNearBlur()
    {
        Volume[] volumes = Object.FindObjectsByType<Volume>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < volumes.Length; i++)
        {
            Volume volume = volumes[i];
            if (volume == null)
            {
                continue;
            }

            OverrideDepthOfFieldOff(volume.profile);
        }
    }

    private static void OverrideDepthOfFieldOff(VolumeProfile profile)
    {
        if (profile == null)
        {
            return;
        }

        if (!profile.TryGet(out DepthOfField depthOfField))
        {
            depthOfField = profile.Add<DepthOfField>(true);
        }

        depthOfField.active = true;
        depthOfField.mode.overrideState = true;
        depthOfField.mode.value = DepthOfFieldMode.Off;
        depthOfField.gaussianStart.overrideState = true;
        depthOfField.gaussianStart.value = 1000f;
        depthOfField.gaussianEnd.overrideState = true;
        depthOfField.gaussianEnd.value = 1000f;
        depthOfField.focusDistance.overrideState = true;
        depthOfField.focusDistance.value = 1000f;
    }

    #endregion
}
