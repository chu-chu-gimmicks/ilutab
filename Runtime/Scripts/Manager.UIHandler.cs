using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDK3.Image;
using VRC.SDKBase;
using static VRC.SDK3.Image.VRCImageDownloader;

namespace ChuChuGimmicks.ILUTAB.Runtime
{
    public partial class Manager
    {
        [SerializeField] private VRCUrlInputField _inputField;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private TMP_InputField _copyField; // 実行時には string から VRCUrlを生成できないので TMP_InputField を使う
        [SerializeField] private TextMeshProUGUI _logText;

        [SerializeField] private Animator _animator;

        [SerializeField] private AudioSource _audioSouce;
        [SerializeField] private AudioClip _successClip;
        [SerializeField] private AudioClip _errorClip;

        private string _boothLanguage = string.Empty;

        private void OnEnableUIHandler()
        {
            ResetUIHandler();
        }

        private void OnDisableUIHandler()
        {
            ResetUIHandler();
        }

        private void OnDropUIHandler()
        {
        }

        private void ResetUIHandler()
        {
            UpdateLanguage();
        }

        private void UpdateLanguage()
        {
            string vrcLanguage = VRCPlayerApi.GetCurrentLanguage();
            switch (vrcLanguage)
            {
                case "ja": _boothLanguage = "ja"; break;
                case "ko": _boothLanguage =  "ko"; break;
                case "zh-CN": _boothLanguage =  "zh-cn"; break;
                case "zh-HK": _boothLanguage =  "zh-tw"; break;
                default: _boothLanguage =  "en"; break;
            }
        }

        private VRCUrl GetUrl()
        {
            VRCUrl url = _inputField.GetUrl();
            _inputField.SetUrl(VRCUrl.Empty);
            return url;
        }

        private void SetSuccessUI(IVRCImageDownload imageDownload)
        {
            if (!Utilities.IsValid(imageDownload)) { return; }

            Texture texture = imageDownload.Result;

            _backgroundImage.color = Color.black;
            _copyField.interactable = TryGetItemUrl(imageDownload.Url, _boothLanguage, out string itemUrl);
            _copyField.text = itemUrl;
            _logText.text = $"[LOG] Download Success: {texture.width}x{texture.height} ({(float)imageDownload.SizeInMemoryBytes / 1_000_000f:F2} MB)";
            DisableUI();
            _audioSouce.PlayOneShot(_successClip);
        }

        private static bool TryGetItemUrl(VRCUrl imageUrl, string language, out string itemUrl)
        {
            itemUrl = string.Empty;

            if (VRCUrl.IsNullOrEmpty(imageUrl)) { return false; }

            const string prefix = "https://booth.pximg.net/";
            const string marker = "/i/";

            string imageUrlString = imageUrl.Get();
            if (!imageUrlString.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) { return false; }

            int markerStartIndex = imageUrlString.IndexOf(marker);
            if (markerStartIndex == -1) { return false; }

            int idStartIndex = markerStartIndex + marker.Length;
            int idEndIndex = imageUrlString.IndexOf("/", idStartIndex);
            if (idEndIndex == -1) { return false; }

            string id = imageUrlString.Substring(idStartIndex, idEndIndex - idStartIndex);
            itemUrl = $"https://booth.pm/{language}/items/{id}";
            return true;
        }

        private void SetErrorUI(IVRCImageDownload imageDownload)
        {
            if (!Utilities.IsValid(imageDownload)) { return; }

            _logText.text = $"[LOG] Download Error: {imageDownload.ErrorMessage}";
            EnableUI();
            _audioSouce.PlayOneShot(_errorClip);
        }

        private void ToggleUI()
        {
            _animator.SetBool("IsUIVisible", !_animator.GetBool("IsUIVisible"));
        }

        private void EnableUI()
        {
            //if (!_animator.GetBool("IsUIVisible"))
            //{
            _animator.SetBool("IsUIVisible", true);
            //}
        }

        private void DisableUI()
        {
            //if (_animator.GetBool("IsUIVisible"))
            //{
            _animator.SetBool("IsUIVisible", false);
            //}
        }
    }
}
