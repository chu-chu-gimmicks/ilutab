using System;
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
            if (!IsImageUrlValid(imageUrl)) { return; }

            Log.Info($"{nameof(ProcessUrl)}");

            if (!Networking.IsOwner(this.gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            }

            Log.Info($"{nameof(_imageDownloader.DownloadImage)}: Before");
            _downloadTask = _imageDownloader.DownloadImage(imageUrl, null, this);
            Log.Info($"{nameof(_imageDownloader.DownloadImage)}: After");

            _imageUrl = imageUrl;
            RequestSerialization();
        }

        private static bool IsImageUrlValid(VRCUrl imageUrl)
        {
            const string HttpsPrefix = "https://";

            if (VRCUrl.IsNullOrEmpty(imageUrl)) { return false; }

            string imageUrlString = imageUrl.Get();
            return imageUrlString.Length > HttpsPrefix.Length && imageUrlString.StartsWith(HttpsPrefix, StringComparison.OrdinalIgnoreCase);
        }

        public override void OnImageLoadSuccess(IVRCImageDownload result)
        {
            Log.Info($"{nameof(OnImageLoadSuccess)}: Start");

            if (!Utilities.IsValid(result)) { return; }
            if (result.State == VRCImageDownloadState.Unloaded) { return; }

            _downloadTask = null;

            SetImage(result);
            SetSuccessUI(result);

            Log.Info($"{nameof(OnImageLoadSuccess)}: End");
        }

        // "https://?" などの不完全な URL が入力された場合や、ダウンロードキューが上限に達した場合は、DownloadImage() が戻る前に呼ばれることがある
        public override void OnImageLoadError(IVRCImageDownload result)
        {
            Log.Info($"{nameof(OnImageLoadError)}: Start");

            if (!Utilities.IsValid(result)) { return; }
            if (result.State == VRCImageDownloadState.Unloaded) { return; }

            _downloadTask = null;

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
            if (Utilities.IsValid(_downloadTask) && _downloadTask.State != VRCImageDownloadState.Unloaded)
            {
                _downloadTask.Dispose();
            }
            _downloadTask = null;

            if (!IsImageUrlValid(_imageUrl)) { return; }

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
