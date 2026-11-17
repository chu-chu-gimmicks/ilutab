using TMPro;
using UnityEngine;

namespace ChuChuGimmicks.ILUTAB.EditorOnly
{
    public class PresetUISetup : MonoBehaviour
    {
        [SerializeField] private GameObject[] _presetButtons;
        [SerializeField] private TextMeshProUGUI[] _buttonTexts;

        public void SetUp(Preset[] presets)
        {
            int presetslength = presets.Length;
            for (int i = 0; i < _presetButtons.Length; i++)
            {
                bool canEnable = i < presetslength;
                _presetButtons[i].SetActive(canEnable);
                if (canEnable)
                {
                    _buttonTexts[i].text = presets[i].title;
                }
            }
        }
    }
}
