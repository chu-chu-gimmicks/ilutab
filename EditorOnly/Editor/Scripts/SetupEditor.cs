using ChuChuGimmicks.ILUTAB.Runtime;
using UnityEditor;

namespace ChuChuGimmicks.ILUTAB.EditorOnly
{
    [CustomEditor(typeof(Setup))]
    public class SetupEditor : Editor
    {
        private SerializedProperty _managerProp;
        private SerializedProperty _presetUISetupProp;
        private SerializedProperty _presetsProp;

        private bool _isOpen = false;

        private void OnEnable()
        {
            _managerProp = serializedObject.FindProperty(Setup.FieldNames.Manager);
            _presetUISetupProp = serializedObject.FindProperty(Setup.FieldNames.PresetUISetup);
            _presetsProp = serializedObject.FindProperty(Setup.FieldNames.Presets);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.HelpBox(
                "URL プリセットを最大 4 件まで登録できます。\n" +
                "\n" +
                "You can register up to 4 URL presets."
                , MessageType.Info);

            EditorGUILayout.PropertyField(_presetsProp);
            if (_presetsProp.arraySize > 4)
            {
                _presetsProp.arraySize = 4;
            }
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);

            _isOpen = EditorGUILayout.Foldout(_isOpen, "References", true);
            if (_isOpen)
            {
                EditorGUILayout.PropertyField(_managerProp);
                EditorGUILayout.PropertyField(_presetUISetupProp);
            }

            bool hasChanged = EditorGUI.EndChangeCheck();
            if ( hasChanged)
            {
                serializedObject.ApplyModifiedProperties();

                UpdateManagerSerializeField((Manager)_managerProp.objectReferenceValue);

                Setup setup = (Setup)target;
                setup.SetUp();
            }
        }

        private void UpdateManagerSerializeField(Manager manager)
        {
            SerializedObject so = new SerializedObject(manager);
            SerializedProperty presetsProp = so.FindProperty(Manager.FieldNames.Presets);

            so.Update();

            presetsProp.arraySize = _presetsProp.arraySize;
            for (int i = 0; i < presetsProp.arraySize; i++)
            {
                presetsProp.GetArrayElementAtIndex(i).FindPropertyRelative("url").stringValue = _presetsProp.GetArrayElementAtIndex(i).FindPropertyRelative(Preset.FieldNames.Url).stringValue;
            }

            so.ApplyModifiedProperties();
        }
    }
}
