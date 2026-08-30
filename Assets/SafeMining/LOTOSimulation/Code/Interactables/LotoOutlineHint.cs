using System.Collections.Generic;
using UnityEngine;

public class LotoOutlineHint : MonoBehaviour
{
    [SerializeField] private Color outlineColor = new Color(0.976f, 0.451f, 0.086f, 1f);
    [SerializeField] private float outlineWidth = 4f;
    [SerializeField] private Outline.Mode outlineMode = Outline.Mode.OutlineVisible;

    private readonly List<Outline> activeOutlines = new List<Outline>();

    #region Public API

    public void Show(params GameObject[] targets)
    {
        Clear();
        if (targets == null)
        {
            return;
        }

        for (int i = 0; i < targets.Length; i++)
        {
            GameObject target = targets[i];
            if (target == null)
            {
                continue;
            }

            ApplyOutline(target);
            return;
        }
    }

    public void Show(IList<GameObject> targets)
    {
        Clear();
        if (targets == null)
        {
            return;
        }

        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] == null)
            {
                continue;
            }

            ApplyOutline(targets[i]);
            return;
        }
    }

    public void Clear()
    {
        for (int i = 0; i < activeOutlines.Count; i++)
        {
            if (activeOutlines[i] != null)
            {
                activeOutlines[i].enabled = false;
            }
        }

        activeOutlines.Clear();
    }

    #endregion

    #region Lifecycle

    private void OnDisable()
    {
        Clear();
    }

    #endregion

    #region Helpers

    private void ApplyOutline(GameObject target)
    {
        Outline outline = target.GetComponent<Outline>();
        if (outline == null)
        {
            outline = target.AddComponent<Outline>();
        }

        outline.OutlineMode = outlineMode;
        outline.OutlineColor = ResolveOutlineColor();
        outline.OutlineWidth = outlineWidth;
        outline.enabled = true;
        activeOutlines.Add(outline);
    }

    private Color ResolveOutlineColor()
    {
        if (Brand.HasConfig)
        {
            return Brand.GetColor(BrandColorRole.Secondary);
        }

        return outlineColor;
    }

    #endregion
}
