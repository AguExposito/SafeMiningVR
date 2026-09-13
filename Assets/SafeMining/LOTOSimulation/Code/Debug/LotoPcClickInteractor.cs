using UnityEngine;
using UnityEngine.EventSystems;

public class LotoPcClickInteractor : MonoBehaviour
{
    [SerializeField] private float maxDistance = 3.5f;

    private Camera viewCamera;
    private readonly RaycastHit[] hits = new RaycastHit[16];

    private void Awake()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        enabled = false;
#endif
        viewCamera = Camera.main;
    }

    private void Update()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return;
#endif
        if (!Application.isPlaying || !Input.GetMouseButtonDown(0))
        {
            return;
        }

        if (viewCamera == null)
        {
            viewCamera = Camera.main;
        }

        if (viewCamera == null)
        {
            Debug.Log("[LOTO-PC] Click ignorado: no hay camera");
            return;
        }

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("[LOTO-PC] Click sobre UI");
            return;
        }

        Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
        int count = Physics.RaycastNonAlloc(ray, hits, maxDistance, ~0, QueryTriggerInteraction.Collide);
        if (count <= 0)
        {
            Debug.Log("[LOTO-PC] Click sin hit");
            return;
        }

        System.Array.Sort(hits, 0, count, HitDistanceComparer.Instance);
        for (int i = 0; i < count; i++)
        {
            Transform hitTransform = hits[i].transform;
            if (hitTransform == null || hitTransform.root == transform.root)
            {
                continue;
            }

            if (TryActivate(hitTransform.gameObject, hits[i].distance))
            {
                return;
            }
        }

        Debug.Log($"[LOTO-PC] Click en '{hits[0].transform.name}' sin interactable ({hits[0].distance:F2}m)");
    }

    private static bool TryActivate(GameObject target, float distance)
    {
        ref_EppItem epp = target.GetComponentInParent<ref_EppItem>();
        if (epp != null)
        {
            EppEquipmentManager equipment = Object.FindFirstObjectByType<EppEquipmentManager>();
            Debug.Log($"[LOTO-PC] Click EPP {EppEquipmentManager.GetDisplayName(epp.itemType)} ({epp.condition}) a {distance:F2}m");

            LotoGameManager board = LotoGameManager.Instance;
            if (board != null)
            {
                board.NotifyEppGrabbed(epp.itemType, epp.condition);
            }

            if (epp.condition == EppItemCondition.Defective)
            {
                return true;
            }

            if (equipment != null)
            {
                equipment.TryEquip(epp, null);
            }

            return true;
        }

        LotoS1ContinueInteractable s1 = target.GetComponentInParent<LotoS1ContinueInteractable>();
        if (s1 != null)
        {
            Debug.Log($"[LOTO-PC] Click continuar S1 a {distance:F2}m");
            s1.ActivateFromPc();
            return true;
        }

        LotoS2ContinueInteractable s2 = target.GetComponentInParent<LotoS2ContinueInteractable>();
        if (s2 != null)
        {
            Debug.Log($"[LOTO-PC] Click continuar S2 a {distance:F2}m");
            s2.ActivateFromPc();
            return true;
        }

        HazardReportInteractable hazard = target.GetComponentInParent<HazardReportInteractable>();
        if (hazard != null)
        {
            Debug.Log($"[LOTO-PC] Click hazard a {distance:F2}m");
            hazard.ActivateFromPc();
            return true;
        }

        LotoPanelDoorInteractable panel = target.GetComponentInParent<LotoPanelDoorInteractable>();
        if (panel != null)
        {
            Debug.Log($"[LOTO-PC] Click puerta panel a {distance:F2}m");
            panel.ActivateFromPc();
            return true;
        }

        LotoBreakerInteractable breaker = target.GetComponentInParent<LotoBreakerInteractable>();
        if (breaker != null)
        {
            Debug.Log($"[LOTO-PC] Click breaker a {distance:F2}m");
            breaker.ActivateFromPc();
            return true;
        }

        LotoControlButtonInteractable button = target.GetComponentInParent<LotoControlButtonInteractable>();
        if (button != null)
        {
            Debug.Log($"[LOTO-PC] Click boton LOTO a {distance:F2}m");
            button.ActivateFromPc();
            return true;
        }

        return false;
    }

    private sealed class HitDistanceComparer : System.Collections.Generic.IComparer<RaycastHit>
    {
        public static readonly HitDistanceComparer Instance = new HitDistanceComparer();

        public int Compare(RaycastHit a, RaycastHit b)
        {
            return a.distance.CompareTo(b.distance);
        }
    }
}
