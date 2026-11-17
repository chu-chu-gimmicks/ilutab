using UdonSharp;
using UnityEngine;

namespace ChuChuGimmicks.ILUTAB.Runtime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
    public class PickupEventHandler : UdonSharpBehaviour
    {
        [SerializeField] Manager _manager;

        public override void OnPickup()
        {
            _manager._OnPickup();
        }

        public override void OnPickupUseDown()
        {
            _manager._OnPickupUseDown();
        }

        public override void OnPickupUseUp()
        {
            _manager._OnPickupUseUp();
        }

        public override void OnDrop()
        {
            _manager._OnDrop();
        }
    }
}
