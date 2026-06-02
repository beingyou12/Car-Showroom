using UnityEditor;
using UnityEngine;

namespace Unity.AI.Assistant.PlayModeTest
{
    [InitializeOnLoad]
    internal static class JitterTest
    {
        private const string StateKey = "PlayModeTest.State";
        private const string ResultKey = "PlayModeTest.Result";
        private const string ScriptPathKey = "PlayModeTest.ScriptPath";

        static JitterTest()
        {
            string state = SessionState.GetString(StateKey, "Idle");
            if (state == "WaitingForCompile")
            {
                EditorApplication.delayCall += () =>
                {
                    SessionState.SetString(StateKey, "EnteringPlayMode");
                    EditorApplication.isPlaying = true;
                };
            }
            else if (state == "EnteringPlayMode" && EditorApplication.isPlaying)
            {
                SessionState.SetString(StateKey, "InPlayMode");
                EditorApplication.update += RunTest;
            }
        }

        private static int _frameCount = 0;
        private static Vector3 _minPos = Vector3.one * float.MaxValue;
        private static Vector3 _maxPos = Vector3.one * float.MinValue;
        private static float _maxRotDiff = 0;
        private static Quaternion _startRot;

        private static void RunTest()
        {
            _frameCount++;
            var door = GameObject.Find("RMCar26_DoorFrontRight");
            if (door != null)
            {
                if (_frameCount == 1) _startRot = door.transform.rotation;

                Vector3 pos = door.transform.position;
                _minPos = Vector3.Min(_minPos, pos);
                _maxPos = Vector3.Max(_maxPos, pos);
                _maxRotDiff = Mathf.Max(_maxRotDiff, Quaternion.Angle(_startRot, door.transform.rotation));
            }

            if (_frameCount >= 120) // 2 seconds at 60fps
            {
                EditorApplication.update -= RunTest;
                
                float jitterPos = Vector3.Distance(_minPos, _maxPos);
                
                var res = new TestResult { 
                    success = jitterPos < 0.01f && _maxRotDiff < 0.5f,
                    jitterPos = jitterPos,
                    maxRotDiff = _maxRotDiff
                };
                
                SessionState.SetString(ResultKey, JsonUtility.ToJson(res));
                SessionState.SetString(StateKey, "Done");
                EditorApplication.isPlaying = false;
                EditorApplication.delayCall += SelfDestruct;
            }
        }

        private static void SelfDestruct()
        {
            string path = SessionState.GetString(ScriptPathKey, "");
            if (AssetDatabase.AssetPathExists(path)) AssetDatabase.DeleteAsset(path);
        }

        [System.Serializable]
        private class TestResult { public bool success; public float jitterPos; public float maxRotDiff; }
    }
}
