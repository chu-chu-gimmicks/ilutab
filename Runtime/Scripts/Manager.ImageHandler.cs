using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Image;
using VRC.SDKBase;

namespace ChuChuGimmicks.ILUTAB.Runtime
{
    public partial class Manager
    {
        [SerializeField] private RectTransform _imageContainer;
        [SerializeField] private RawImage _rawImage;

        private IVRCImageDownload _imageDownlod;

        private Quaternion _targetImageRotation;
        private Vector3 _targetImageScale;

        private void OnEnableImageHandler()
        {
            ResetImageHandler();
        }

        private void OnDisableImageHandler()
        {
            ResetImageHandler();
        }

        private void OnDropImageHandler(Quaternion sourceRotation)
        {
            TryGetImageRotation(sourceRotation, out Quaternion rotation, out int turns);
            _targetImageRotation = rotation;
            _targetImageScale = GetImageScale(_imageContainer, _rawImage.texture, turns);

            ResetImageHandler();
        }

        private void ResetImageHandler()
        {
            _rawImage.rectTransform.localRotation = _targetImageRotation;
            _rawImage.rectTransform.localScale = _targetImageScale;
            // 次に Pickup したときに Image が初期化されてしまうので初期化しない
            //_targetImageRotation = Quaternion.identity;
            //_targetImageScale = Vector3.zero;
        }

        private void SetImage(IVRCImageDownload imageDownload)
        {
            if (!Utilities.IsValid(imageDownload)) { return; }

            _rawImage.texture = null;
            if (Utilities.IsValid(_imageDownlod))
            {
                _imageDownlod.Dispose();
                _imageDownlod = null;
            }

            _imageDownlod = imageDownload;
            _rawImage.texture = imageDownload.Result;

            TryGetImageRotation(_imageContainer.rotation, out Quaternion localRotation, out int turns);
            _targetImageRotation = localRotation;
            _targetImageScale = GetImageScale(_imageContainer, _rawImage.texture, turns);
            _rawImage.rectTransform.localRotation = _targetImageRotation;
            _rawImage.rectTransform.localScale = _targetImageScale;
        }

        private void UpdateImage(float deltaTime, Quaternion sourceRotation)
        {
            if (TryGetImageRotation(sourceRotation, out Quaternion rotation, out int turns))
            {
                _targetImageRotation = rotation;
                _targetImageScale = GetImageScale(_imageContainer, _rawImage.texture, turns);
            }

            _rawImage.rectTransform.localRotation = Quaternion.Lerp(_rawImage.rectTransform.localRotation, _targetImageRotation, deltaTime * 8f);
            _rawImage.rectTransform.localScale = Vector3.Lerp(_rawImage.rectTransform.localScale, _targetImageScale, deltaTime * 8f);
        }

        public static bool TryGetImageRotation(Quaternion sourceRotation, out Quaternion localRotation, out int turns)
        {
            localRotation = Quaternion.identity;
            turns = 0;

            Vector3 localUp = Quaternion.Inverse(sourceRotation) * Vector3.up;
            if (Mathf.Abs(localUp.z) > Mathf.Cos(15f * Mathf.Deg2Rad)) { return false; }

            turns = Mathf.Abs(localUp.x) >= Mathf.Abs(localUp.y)
                ? localUp.x >= 0f ? 3 : 1
                : localUp.y >= 0f ? 0 : 2;

            localRotation = Quaternion.Euler(0f, 0f, turns * 90f);
            return true;
        }

        public static Vector3 GetImageScale(RectTransform container, Texture content, int turns)
        {
            if (!Utilities.IsValid(content)) { return Vector3.zero; }

            float containerAspectRatio = container.rect.width / container.rect.height;
            float contentAspectRatio = (float)content.width / content.height;
            bool isSideways = (turns & 1) != 0;

            return GetFitScale(containerAspectRatio, contentAspectRatio, isSideways);
        }

        private static Vector3 GetFitScale(float containerAspectRatio, float contentAspectRatio, bool isSideways = false)
        {
            if (isSideways)
            {
                return contentAspectRatio >= 1f / containerAspectRatio
                    ? new Vector3(1f / containerAspectRatio, 1f / contentAspectRatio, 1f)
                    : new Vector3(1f * contentAspectRatio, 1f * containerAspectRatio, 1f);
            }
            else
            {
                return contentAspectRatio >= containerAspectRatio
                    ? new Vector3(1f, 1f / contentAspectRatio * containerAspectRatio, 1f)
                    : new Vector3(1f * contentAspectRatio / containerAspectRatio, 1f, 1f);
            }
        }
    }
}
