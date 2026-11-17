using UdonSharp;
using UnityEngine;

namespace ChuChuGimmicks.ILUTAB.Runtime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class FrameEventReceiver : UdonSharpBehaviour
    {
        [SerializeField] private Manager _manager;

        private void LateUpdate()
        {
            _manager._LateUpdate();
        }
    }
}
