using UnityEngine;

namespace XRStandardMenu
{
    public enum FollowMode
    {
        Advanced,   // Modo avanzado con tolerancia y seguimiento complejo
        Simple      // Modo simple: mira al target y sigue al cebo
    }

    public class Util_PanelFollower : MonoBehaviour
    {
        [Header("Follow Mode")]
        [SerializeField] private FollowMode followMode = FollowMode.Advanced;
        
        [Header("Target Settings")]
        [SerializeField] private Transform target;
        [SerializeField] private string defaultTargetName = "Main Camera";
        
        [Header("Follow Settings")]
        [SerializeField] private float followSpeed = 8f;
        [SerializeField] private float rotationSpeed = 12f;
        [SerializeField] private float toleranceRadius = 0.08f;
        [SerializeField] private float fixedDistance = 0.85f;
        [SerializeField] private bool invertLookDirection = true;
        
        [Header("Simple Mode Settings")]
        [SerializeField] private Transform cebo; // Objeto que seguirá en modo simple
        [SerializeField] private string defaultCeboName = "cebo";
        [SerializeField] private float simpleFollowSpeed = 2f; // Velocidad de seguimiento en modo simple
        
        [Header("Offset Settings")]
        [SerializeField] private Vector3 offset = new Vector3(-0.22f, -0.12f, 0.85f);
        
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;
        [SerializeField] private bool showToleranceRadius = false;
        
        private Vector3 targetPosition;
        private bool isFollowing = false;
        private float currentDistance;
        private Vector3 initialPosition; // Posición inicial del canvas
        private Camera cachedCamera;
        
        private void Start()
        {
            // Capturar la posición inicial del canvas
            initialPosition = transform.position;
            InitializeTarget();
        }
        
        private void InitializeTarget()
        {
            // Si no hay target asignado, buscar el target por defecto
            if (target == null)
            {
                if (cachedCamera == null)
                {
                    cachedCamera = Camera.main;
                }

                if (cachedCamera != null)
                {
                    target = cachedCamera.transform;
                }
                else
                {
                    GameObject defaultTarget = GameObject.Find(defaultTargetName);
                    if (defaultTarget != null)
                    {
                        target = defaultTarget.transform;
                    }
                }

                if (target == null)
                {
                    Debug.LogWarning($"PanelFollower: No se encontró el target '{defaultTargetName}'. El panel no seguirá a ningún objeto.");
                    return;
                }
            }
            
            // Si no hay cebo asignado, buscar el cebo por defecto
            if (cebo == null)
            {
                GameObject defaultCebo = GameObject.Find(defaultCeboName);
                if (defaultCebo != null)
                {
                    cebo = defaultCebo.transform;
                    if (showDebugInfo)
                    {
                        Debug.Log($"PanelFollower: Cebo encontrado automáticamente - {defaultCeboName}");
                    }
                }
                else
                {
                    Debug.LogWarning($"PanelFollower: No se encontró el cebo '{defaultCeboName}'. En modo Simple no funcionará correctamente.");
                }
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"PanelFollower: Target configurado - {target.name}");
                Debug.Log($"PanelFollower: Modo configurado - {followMode}");
            }
        }
        
        private void Update()
        {
            if (target == null) return;
            
            if (followMode == FollowMode.Simple)
            {
                // Modo Simple: seguir al cebo y mirar al target
                if (cebo != null)
                {
                    FollowCeboSimple();
                }
                LookAtTargetSimple();
            }
            else
            {
                UpdateTargetPosition();
                FollowTarget();
                LookAtTarget();
            }
            
            if (showDebugInfo)
            {
                UpdateDebugInfo();
            }
        }
        
        private void UpdateTargetPosition()
        {
            Vector3 localOffset = offset;
            if (localOffset.z < 0.4f)
            {
                localOffset.z = fixedDistance;
            }

            targetPosition = target.TransformPoint(localOffset);
        }
        
        private void CheckIfShouldFollow()
        {
            currentDistance = Vector3.Distance(transform.position, targetPosition);
            isFollowing = currentDistance > toleranceRadius;
        }
        
        private void FollowTarget()
        {
            float t = 1f - Mathf.Exp(-followSpeed * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, targetPosition, t);
        }
        
        private void FollowCeboSimple()
        {
            // Seguir al cebo con Lerp simple
            Vector3 currentPosition = transform.position;
            Vector3 targetPosition = cebo.position;
            
            // Aplicar Lerp en todos los ejes
            Vector3 newPosition = Vector3.Lerp(currentPosition, targetPosition, simpleFollowSpeed * Time.deltaTime);
            transform.position = newPosition;
        }
        
        private void LookAtTargetSimple()
        {
            // Mirar al target invertidamente (siempre invertido en modo simple)
            Vector3 lookDirection = (target.position - transform.position).normalized;
            lookDirection = -lookDirection; // Siempre invertido en modo simple
            
            // Crear una rotación que mire hacia el target usando la velocidad de rotación separada
            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
        
        private void LookAtTarget()
        {
            // Hacer que el panel siempre mire hacia el target (sin offset)
            Vector3 lookDirection = (target.position - transform.position).normalized;
            
            // Invertir la dirección si está habilitado
            if (invertLookDirection)
            {
                lookDirection = -lookDirection;
            }
            
            // Crear una rotación que mire hacia el target usando la velocidad de rotación separada
            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
        
        private void UpdateDebugInfo()
        {
            if (showDebugInfo)
            {
                Debug.Log($"PanelFollower - Distancia: {currentDistance:F2} | Siguiendo: {isFollowing} | Target: {target.name}");
            }
        }
        
        // Métodos públicos para control desde otros scripts
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            if (showDebugInfo)
            {
                Debug.Log($"PanelFollower: Target cambiado a {newTarget.name}");
            }
        }
        
        public void SetFollowSpeed(float speed)
        {
            followSpeed = speed;
        }
        
        public void SetRotationSpeed(float speed)
        {
            rotationSpeed = speed;
        }
        
        public void SetToleranceRadius(float radius)
        {
            toleranceRadius = radius;
        }
        
        public void SetFixedDistance(float distance)
        {
            fixedDistance = distance;
        }
        
        public void SetOffset(Vector3 newOffset)
        {
            offset = newOffset;
        }
        
        public void SetInvertLookDirection(bool invert)
        {
            invertLookDirection = invert;
        }
        
        public void SetFollowMode(FollowMode mode)
        {
            followMode = mode;
            if (showDebugInfo)
            {
                Debug.Log($"PanelFollower: Modo cambiado a {mode}");
            }
        }
        
        public void SetCebo(Transform newCebo)
        {
            cebo = newCebo;
            if (showDebugInfo)
            {
                Debug.Log($"PanelFollower: Cebo cambiado a {newCebo.name}");
            }
        }
        
        public void SetSimpleFollowSpeed(float speed)
        {
            simpleFollowSpeed = speed;
        }
        
        public void ForceFollow()
        {
            isFollowing = true;
        }
        
        public void StopFollowing()
        {
            isFollowing = false;
        }
        
        public bool IsFollowing => isFollowing;
        public float CurrentDistance => currentDistance;
        public Transform CurrentTarget => target;
        public Transform CurrentCebo => cebo;
        public float FixedDistance => fixedDistance;
        public FollowMode CurrentFollowMode => followMode;
        
        // Método para dibujar el radio de tolerancia en el Scene View
        private void OnDrawGizmosSelected()
        {
            if (showToleranceRadius)
            {
                // Dibujar radio de tolerancia en 2D (solo X e Y)
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, toleranceRadius);
                
                if (target != null)
                {
                    // Dibujar línea hacia el target (para la mirada)
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(transform.position, target.position);
                    
                    // Dibujar línea hacia la posición con offset (para el seguimiento)
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(transform.position, targetPosition);
                    
                    // Dibujar la distancia fija
                    Gizmos.color = Color.blue;
                    Vector3 directionToTarget = (targetPosition - transform.position).normalized;
                    Vector3 fixedDistancePos = targetPosition - (directionToTarget * fixedDistance);
                    Gizmos.DrawWireSphere(fixedDistancePos, 0.1f);
                    
                    // Dibujar offset
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireSphere(target.position, 0.05f);
                    Gizmos.DrawLine(target.position, targetPosition);
                }
            }
        }
    }
}
