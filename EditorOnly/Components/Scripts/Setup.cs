using ChuChuGimmicks.ILUTAB.Runtime;
using UnityEngine;
using VRC.SDKBase;

namespace ChuChuGimmicks.ILUTAB.EditorOnly
{
    [System.Serializable]
    public struct Preset
    {
        public static class FieldNames
        {
            public const string Title = nameof(title);
            public const string Url = nameof(url);
        }

        public string title;
        public string url;
    }

    public class Setup : MonoBehaviour
    {
        public static class FieldNames
        {
            public const string Manager = nameof(_manager);
            public const string PresetUISetup = nameof(_presetUISetup);
            public const string Presets = nameof(_presets);
        }

        [SerializeField] private Manager _manager;
        [SerializeField] private PresetUISetup _presetUISetup;
        [SerializeField] private Preset[] _presets;

        public void SetUp()
        {
            _presetUISetup.SetUp(_presets);
        }
    }
}