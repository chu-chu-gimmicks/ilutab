using UnityEngine;
using VRC.SDKBase;

namespace ChuChuGimmicks.ILUTAB.Runtime
{
    public partial class Manager
    {
        [SerializeField] private Transform _sourceTransform;
        [SerializeField] private Transform _targetTransform;

        [SerializeField] private float _surfaceSnapMaxDistance;
        [SerializeField] private LayerMask _surfaceSnapLayerMask;

        private void OnEnableSnapHandler()
        {
            ResetSnapHandler();
        }

        private void OnDisableSnapHandler()
        {
            ResetSnapHandler();
        }

        private void OnDropSnapHandler(out Quaternion rotation)
        {
            rotation = _targetTransform.rotation;
            ResetSnapHandler();
        }

        private void ResetSnapHandler()
        {
            Vector3 position = _targetTransform.position;
            Quaternion rotation = _targetTransform.rotation;
            _sourceTransform.position = position;
            _sourceTransform.rotation = rotation;
            _targetTransform.position = position;
            _targetTransform.rotation = rotation;
        }

        private void UpdateSnap(bool inputPickupUse, out Quaternion rotation)
        {
            if (TryRaycastSurface(_sourceTransform, _surfaceSnapMaxDistance, _surfaceSnapLayerMask, out RaycastHit hit))
            {
                if (inputPickupUse)
                {
                    _targetTransform.position = hit.point;
                    _targetTransform.rotation = SnapOnSurface(AlignToSurface(_sourceTransform.rotation, -_sourceTransform.forward, hit.normal), hit.normal);
                }
                else
                {
                    _targetTransform.position = hit.point;
                    _targetTransform.rotation = AlignToSurface(_sourceTransform.rotation, -_sourceTransform.forward, hit.normal);
                }
            }
            else
            {
                if (inputPickupUse)
                {
                    _targetTransform.position = _sourceTransform.position;
                    _targetTransform.rotation = Snap(_sourceTransform.rotation);
                }
                else
                {
                    _targetTransform.position = _sourceTransform.position;
                    _targetTransform.rotation = _sourceTransform.rotation;
                }
            }

            rotation = _targetTransform.rotation;
        }

        private static bool TryRaycastSurface(Transform sourceTransform, float maxDistance, LayerMask layerMask, out RaycastHit hit)
        {
            const QueryTriggerInteraction QueryTriggerInteraction = QueryTriggerInteraction.Ignore;

            Vector3 origin = sourceTransform.position - sourceTransform.forward * maxDistance * 0.5f;
            Vector3 direction = sourceTransform.forward;
            return Physics.Raycast(origin, direction, out hit, maxDistance, layerMask, QueryTriggerInteraction) && Utilities.IsValid(hit.collider);
        }

        private static Quaternion AlignToSurface(Quaternion sourceRotation, Vector3 sourceForward, Vector3 hitNormal)
        {
            return Quaternion.FromToRotation(sourceForward, hitNormal) * sourceRotation;
        }

        private static Quaternion SnapOnSurface(Quaternion sourceRotation, Vector3 surfaceNormal)
        {
            if (Mathf.Abs(surfaceNormal.y) > Mathf.Cos(15f * Mathf.Deg2Rad)) { return sourceRotation; }

            Vector3 surfaceUp = Vector3.ProjectOnPlane(Vector3.up, surfaceNormal).normalized;
            Vector3 localSurfaceUp = Quaternion.Inverse(sourceRotation) * surfaceUp;
            float x = Mathf.Abs(localSurfaceUp.x);
            float y = Mathf.Abs(localSurfaceUp.y);

            Vector3 snapAxis;
            if (x >= y)
            {
                snapAxis = localSurfaceUp.x >= 0f ? Vector3.right : Vector3.left;
            }
            else
            {
                snapAxis = localSurfaceUp.y >= 0f ? Vector3.up : Vector3.down;
            }

            Quaternion correction = Quaternion.FromToRotation(snapAxis, localSurfaceUp);
            return sourceRotation * correction;
        }

        private static Quaternion Snap(Quaternion sourceRotation)
        {
            Vector3 localUp = Quaternion.Inverse(sourceRotation) * Vector3.up;
            float x = Mathf.Abs(localUp.x);
            float y = Mathf.Abs(localUp.y);
            float z = Mathf.Abs(localUp.z);

            Vector3 snapAxis;
            if (x >= y && x >= z)
            {
                snapAxis = localUp.x >= 0f ? Vector3.right : Vector3.left;
            }
            else if (y >= x && y >= z)
            {
                snapAxis = localUp.y >= 0f ? Vector3.up : Vector3.down;
            }
            else
            {
                snapAxis = localUp.z >= 0f ? Vector3.forward : Vector3.back;
            }

            Quaternion correction = Quaternion.FromToRotation(snapAxis, localUp);
            return sourceRotation * correction;
        }
    }
}
