namespace ChuChuGimmicks.ILUTAB.Runtime
{
    public partial class Manager
    {
#if UNITY_EDITOR && !COMPILER_UDONSHARP
        public static class FieldNames
        {
            public const string Pickup = nameof(_pickup);
            public const string FrameEventHandler = nameof(_frameEventHandler);

            public const string Presets = nameof(_presets);

            public const string Canvas = nameof(_imageContainer);
            public const string RawImage = nameof(_rawImage);

            public const string InputField = nameof(_inputField);
            public const string BackgroundImage = nameof(_backgroundImage);
            public const string LogText = nameof(_logText);
            public const string CopyField = nameof(_copyField);
            public const string Animator = nameof(_animator);
            public const string AudioSouce = nameof(_audioSouce);
            public const string SuccessClip = nameof(_successClip);
            public const string ErrorClip = nameof(_errorClip);

            public const string SourceTransform = nameof(_sourceTransform);
            public const string TargetTransform = nameof(_targetTransform);
        }
#endif
    }
}
