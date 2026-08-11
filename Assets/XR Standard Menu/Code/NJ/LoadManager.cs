using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum LoadingMode
{
    Blur,           // Modo con blur y deshabilitar interacciones
    Simple,          // Modo simple sin blur
    Progress        // Modo con barra de progreso personalizada
}

public class LoadManager : MonoBehaviour
{
    public void Initialize(){}
    [SerializeField] private string globalVolumeName = "Global Volume";
    [SerializeField] private bool restoreVolumeStateOnFinish = true;
    [SerializeField] private float transitionDuration = 1f;
    [SerializeField] private string loadingTitle = "Cargando...";
    [SerializeField] private string loadingDescription = "Por favor espera...";

    private Volume cachedGlobalVolume;
    private bool volumeWasEnabledBefore;
    private GameObject loadingCanvas;
    private CanvasGroup canvasGroup;
    private int ppExcludeLayer;

    public async Task RunJobWithBlur<TJob>(TJob job) where TJob : struct, IJob
    {
        var handle = job.Schedule();
        await EnableBlurAsync(true);
        while (!handle.IsCompleted)
        {
            await Task.Yield();
        }
        handle.Complete();
        await EnableBlurAsync(false);
    }

    public async Task WaitConditionWithBlur(Func<bool> isDone)
    {
        await EnableBlurAsync(true);
        while (!isDone())
        {
            await Task.Yield();
        }
        await EnableBlurAsync(false);
    }

    public async Task WaitForParameterChange<T>(Func<T> getCurrentValue)
    {
        var initialValue = getCurrentValue();
        await EnableBlurAsync(true);
        while (EqualityComparer<T>.Default.Equals(getCurrentValue(), initialValue))
        {
            await Task.Yield();
        }
        await EnableBlurAsync(false);
    }

    private async Task EnableBlurAsync(bool enable)
    {
        var volume = EnsureGlobalVolume();
        if (volume == null)
            return;

        if (enable)
        {
            volumeWasEnabledBefore = volume.enabled;
            volume.enabled = true;

            var profile = EnsureProfile(volume);
            var dof = EnsureDepthOfField(profile);

            dof.active = true;
            dof.mode.value = DepthOfFieldMode.Bokeh;
            dof.focusDistance.value = 0.1f;
            dof.focalLength.value = 300f;
            dof.aperture.value = 32f;
            dof.bladeCount.value = 8;
            dof.bladeCurvature.value = 1f;
            dof.bladeRotation.value = 0f;
        }
        else
        {
            if (restoreVolumeStateOnFinish)
                volume.enabled = volumeWasEnabledBefore;

            if (volume.profile != null && volume.profile.TryGet<DepthOfField>(out var dof))
            {
                dof.active = false;
            }
        }
    }


    private Volume EnsureGlobalVolume()
    {
        if (cachedGlobalVolume != null)
            return cachedGlobalVolume;

        // Busca un Volume global existente
        var volumes = FindObjectsByType<Volume>(FindObjectsSortMode.None);
        foreach (var v in volumes)
        {
            if (v != null && v.isGlobal)
            {
                cachedGlobalVolume = v;
                return cachedGlobalVolume;
            }
        }

        // Si no existe, crea uno nuevo
        var go = new GameObject(string.IsNullOrWhiteSpace(globalVolumeName) ? "Global Volume" : globalVolumeName);
        cachedGlobalVolume = go.AddComponent<Volume>();
        cachedGlobalVolume.isGlobal = true;
        cachedGlobalVolume.priority = 0f;
        cachedGlobalVolume.weight = 1f;
        EnsureProfile(cachedGlobalVolume);
        return cachedGlobalVolume;
    }

    private static VolumeProfile EnsureProfile(Volume volume)
    {
        if (volume.profile == null)
        {
            volume.profile = ScriptableObject.CreateInstance<VolumeProfile>();
        }
        return volume.profile;
    }

    private static DepthOfField EnsureDepthOfField(VolumeProfile profile)
    {
        if (!profile.TryGet<DepthOfField>(out var dof))
        {
            dof = profile.Add<DepthOfField>(true);
        }
        return dof;
    }
    void DisableAllInteractions()
    {
        var uiInteractables = FindObjectsByType<UIInteractable>(FindObjectsSortMode.None);
        foreach (var uiInteractable in uiInteractables) uiInteractable.DisableInteractions();
    }
    void EnableAllInteractions()
    {
        var uiInteractables = FindObjectsByType<UIInteractable>(FindObjectsSortMode.None);
        foreach (var uiInteractable in uiInteractables) uiInteractable.EnableInteractions();
    }

    // Función genérica para manejar procesos de carga
    public async Task ExecuteWithLoading(LoadingMode mode, Func<Task> taskFunc, string title, string description = "", bool showProgressBar = false)
    {
        bool restoreBlur = false;
        // Configurar según el modo
        switch (mode)
        {
            case LoadingMode.Blur:
                DisableAllInteractions();
                await EnableBlurAsync(true);
                restoreBlur = true;
                break;
            case LoadingMode.Simple:
                // No hacer nada especial
                break;
            case LoadingMode.Progress:
                DisableAllInteractions();
                await EnableBlurAsync(true);
                restoreBlur = true;
                break;
        }
        ref_UIWaiterNormal uiWaiterNormal = null;
        try
        {
            // Crear UI de carga
            uiWaiterNormal = Instantiate(SBD_Prefab.Instance.UI_Waiter_normal, null).GetComponent<ref_UIWaiterNormal>();
            SetLayerRecursively(uiWaiterNormal.gameObject, 3);
            uiWaiterNormal.SetTitle(title);
            uiWaiterNormal.SetBody(string.IsNullOrEmpty(description) ? "0%" : description);

            // Ejecutar la tarea y monitorear progreso
            var task = taskFunc();
            await MonitorTaskProgress(task, uiWaiterNormal, showProgressBar);
        }
        finally
        {
            // Limpiar UI
            if (uiWaiterNormal != null)
            {
                Destroy(uiWaiterNormal.gameObject);
            }

            // Restaurar interacciones y blur aunque falle la tarea
            if (mode == LoadingMode.Blur || mode == LoadingMode.Progress)
            {
                EnableAllInteractions();
            }
            if (restoreBlur)
            {
                await EnableBlurAsync(false);
            }
        }
    }

    // Función separada para monitorear el progreso de una tarea
    private async Task MonitorTaskProgress(Task task, ref_UIWaiterNormal uiWaiterNormal, bool showProgressBar)
    {
        while (!task.IsCompleted)
        {
            float progress = 0f;
            
            if (task.IsCompleted)
            {
                progress = 1f;
            }
            else if (showProgressBar)
            {
                // Progreso simulado que va aumentando gradualmente
                progress = Mathf.Min(0.9f, Time.time * 0.1f);
            }

            // Actualizar UI desde el hilo principal
            if (uiWaiterNormal != null)
            {
                if (showProgressBar)
                {
                    uiWaiterNormal.Progress(progress, progress);
                    uiWaiterNormal.SetBody($"{(progress * 100):F0}%");
                }
            }

            await Task.Yield(); // Asegurar que volvemos al hilo principal
        }

        // Mostrar 100% al final si hay barra de progreso
        if (showProgressBar && uiWaiterNormal != null)
        {
            uiWaiterNormal.Progress(1f, 1f);
            uiWaiterNormal.SetBody("100%");
            await Task.Delay(500); // Esperar un momento para que el usuario vea el 100%
        }
    }

    private void SetLayerRecursively(GameObject target, int layerIndex)
    {
        if (target == null)
        {
            return;
        }
        target.layer = layerIndex;
        Transform targetTransform = target.transform;
        for (int i = 0; i < targetTransform.childCount; i++)
        {
            SetLayerRecursively(targetTransform.GetChild(i).gameObject, layerIndex);
        }
    }

    #region Presets

    public async Task LoadScene_VisualProgress(string sceneName) {
        await ExecuteWithLoading(LoadingMode.Blur, 
            async () => {
                var asyncOperation = SceneManager.LoadSceneAsync(sceneName);
                while (!asyncOperation.isDone)
                {
                    await Task.Yield();
                }
            }, 
            sceneName, 
            $"Cargando escena: {sceneName}", 
            true
        );
    }
    public async Task LoadScene_VisualProgress(int val) {
        await ExecuteWithLoading(LoadingMode.Blur, 
            async () => {
                var asyncOperation = SceneManager.LoadSceneAsync(val);
                while (!asyncOperation.isDone)
                {
                    await Task.Yield();
                }
            }, 
            "", 
            "Cargando escena...", 
            true
        );
    }

    public async Task WaitTasks_VisualProgress(string title, Func<Task> taskFunc) {
        await ExecuteWithLoading(LoadingMode.Blur, taskFunc, title, "", false);
    }

    #endregion
}
