using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDK3.Image;
using VRC.SDK3.UdonNetworkCalling;
using VRC.SDKBase;
using VRC.Udon.Common;

namespace ChuChuGimmicks.ILUTAB.Runtime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public partial class Manager : UdonSharpBehaviour
    {
        [SerializeField] private VRCPickup _pickup;
        [SerializeField] private FrameEventReceiver _frameEventHandler;

        [SerializeField] private VRCUrl[] _presets;

        [UdonSynced] private VRCUrl _imageUrl;

        private VRCImageDownloader _imageDownloader;
        private IVRCImageDownload _downloadTask;

        private bool _inputPickupUse = false;

        private void OnEnable()
        {
            _pickup.AutoHold = Networking.LocalPlayer.IsUserInVR()
                ? VRC_Pickup.AutoHoldMode.No
                : VRC_Pickup.AutoHoldMode.Yes;

            if (!Utilities.IsValid(_imageDownloader))
            {
                _imageDownloader = new VRCImageDownloader();
            }

            OnEnableUIHandler();
            OnEnableSnapHandler();
            OnEnableImageHandler();
        }

        private void OnDisable()
        {
            _inputPickupUse = false;

            OnDisableUIHandler();
            OnDisableSnapHandler();
            OnDisableImageHandler();
        }

        public void _OnScreenTouched()
        {
            if (_pickup.IsHeld && !Networking.LocalPlayer.IsUserInVR()) { return; }

            ToggleUI();
        }

        public void _OnEndEdit()
        {
            ProcessUrl(GetUrl());
        }

        public void _OnPreset1Selected()
        {
            ProcessUrl(_presets[0]);
        }

        public void _OnPreset2Selected()
        {
            ProcessUrl(_presets[1]);
        }

        public void _OnPreset3Selected()
        {
            ProcessUrl(_presets[2]);
        }

        public void _OnPreset4Selected()
        {
            ProcessUrl(_presets[3]);
        }

        private void ProcessUrl(VRCUrl imageUrl)
        {
            if (Utilities.IsValid(_downloadTask) && _downloadTask.State == VRCImageDownloadState.Pending) { return; }
            if (!IsImageUrlValid(imageUrl, _imageUrl)) { return; }

            Log.Info($"{nameof(ProcessUrl)}");

            if (!Networking.IsOwner(this.gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            }

            _downloadTask = _imageDownloader.DownloadImage(imageUrl, null, this);
            _imageUrl = imageUrl;
            RequestSerialization();
        }

        private static bool IsImageUrlValid(VRCUrl imageUrl, VRCUrl prevImageUrl)
        {
            if (!Utilities.IsValid(imageUrl)) { return false; }
            string imageUrlString = imageUrl.Get();
            if (imageUrlString.Length < 4 || imageUrlString.Substring(0, 4) != "http") { return false; }
            if (Utilities.IsValid(prevImageUrl) && imageUrlString == prevImageUrl.Get()) { return false; }
            return true;
        }

        public override void OnImageLoadSuccess(IVRCImageDownload result)
        {
            _downloadTask = null;

            if (!Utilities.IsValid(result)) { return; }
            if (result.Url.Get() != _imageUrl.Get()) { return; }

            SetImage(result);
            SetSuccessUI(result);
        }

        // DownloadImage() の開始と終了の間に呼ばれることがある
        public override void OnImageLoadError(IVRCImageDownload result)
        {
            _downloadTask = null;

            if (!Utilities.IsValid(result)) { return; }

            SetErrorUI(result);

            result.Dispose();

            Log.Info($"{nameof(OnImageLoadError)}: End");
        }

        public override void OnPostSerialization(SerializationResult result)
        {
            Log.Info($"{nameof(OnPostSerialization)}: {nameof(result.success)}={result.success}, {nameof(result.byteCount)}={result.byteCount}");
        }

        public override void OnDeserialization()
        {
            _downloadTask = _imageDownloader.DownloadImage(_imageUrl, null, this);
        }

        [NetworkCallable]
        public void _OnPickup()
        {
            Log.Info($"{nameof(_OnPickup)}");

            _frameEventHandler.enabled = true;

            if (!NetworkCalling.InNetworkCall)
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Others, nameof(_OnPickup));
            }
        }

        [NetworkCallable]
        public void _OnPickupUseDown()
        {
            Log.Info($"{nameof(_OnPickupUseDown)}");

            _inputPickupUse = true;

            if (!NetworkCalling.InNetworkCall)
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Others, nameof(_OnPickupUseDown));
            }
        }

        [NetworkCallable]
        public void _OnPickupUseUp()
        {
            Log.Info($"{nameof(_OnPickupUseUp)}");

            _inputPickupUse = false;

            if (!NetworkCalling.InNetworkCall)
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Others, nameof(_OnPickupUseUp));

                // 処理順序に注意が必要
                _pickup.Drop();
            }
        }

        [NetworkCallable]
        public void _OnDrop()
        {
            Log.Info($"{nameof(_OnDrop)}");

            _frameEventHandler.enabled = false;

            OnDropUIHandler();
            OnDropSnapHandler(out Quaternion rotation);
            OnDropImageHandler(rotation);

            if (!NetworkCalling.InNetworkCall)
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Others, nameof(_OnDrop));
            }
        }

        public void _LateUpdate()
        {
            if (!_pickup.IsHeld) { return; }

            float deltaTime = Time.deltaTime;
            UpdateSnap(_inputPickupUse, out Quaternion rotation);
            UpdateImage(deltaTime, rotation);
        }

        public override void OnLanguageChanged(string language)
        {
            UpdateLanguage();
        }

        private void OnDestroy()
        {
            if (Utilities.IsValid(_imageDownloader))
            {
                // 紐づいているすべての IVRCImageDownload とテクスチャを開放
                _imageDownloader.Dispose();
            }
        }
    }
}
