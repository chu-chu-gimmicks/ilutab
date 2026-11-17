using UnityEngine;

namespace ChuChuGimmicks.ILUTAB.Runtime
{
    public class Log : MonoBehaviour
    {
        public static void Info(string message)
        {
            string coloredProductName = $"<color=#FFBF00>{nameof(ILUTAB)}</color>";
            Debug.Log($"[{coloredProductName}] {message}");
        }


        //public static void Warning(string className, string message) { }
        //public static void Error(string className, string message) { }
        //public static void Assert(bool condition, string className, string message) { }
    }
}
