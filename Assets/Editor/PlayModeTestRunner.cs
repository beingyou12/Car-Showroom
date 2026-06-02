using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Unity.AI.Assistant.PlayModeTest
{
    [InitializeOnLoad]
    internal static class PlayModeTestRunner
    {
        private const string StateKey = "PlayModeTest.State";
        private const string ResultKey = "PlayModeTest.Result";
        private const string ScriptPathKey = "PlayModeTest.ScriptPath";

        private static readonly int WaitFrames = SessionState.GetInt("PlayModeTest.WaitFrames", 30);
        private static readonly float TestTimeout = 5.0f;

        private static List<string> _capturedLogs = new List<string>();

        static PlayModeTestRunner()
        {
            string state = SessionState.GetString(StateKey, "Idle");
            if (state == "WaitingForCompile") {
                EditorApplication.delayCall += () => {
                    SessionState.SetString(StateKey, "EnteringPlayMode");
                    EditorApplication.isPlaying = true;
                };
            } else if (state == "EnteringPlayMode" && EditorApplication.isPlaying) {
                SessionState.SetString(StateKey, "InPlayMode");
                EditorApplication.update += WaitFramesThenRun;
            } else if (state == "InPlayMode" && EditorApplication.isPlaying) {
                EditorApplication.update += WaitFramesThenRun;
            } else if (state == "Done") {
                EditorApplication.delayCall += SelfDestruct;
            }
        }

        private static int _frameCount = 0;
        private static bool _setupDone = false;
        private static bool _testDone = false;
        private static double _testStartTime = 0;

        private static GameObject _frontDoor;
        private static GameObject _rearDoor;
        private static float _maxFrontJitter = 0;
        private static float _maxRearJitter = 0;
        private static Vector3 _frontStartPos;
        private static Vector3 _rearStartPos;

        private static void WaitFramesThenRun()
        {
            _frameCount++;
            if (_frameCount < WaitFrames) return;
            if (_testDone) return;

            if (!_setupDone) {
                _setupDone = true;
                _frontDoor = GameObject.Find("RMCar26_DoorFrontRight");
                _rearDoor = GameObject.Find("RMCar26_DoorRearRight");
                if (_frontDoor) _frontStartPos = _frontDoor.transform.localPosition;
                if (_rearDoor) _rearStartPos = _rearDoor.transform.localPosition;
                _testStartTime = EditorApplication.timeSinceStartup;
                return;
            }

            float elapsed = (float)(EditorApplication.timeSinceStartup - _testStartTime);
            if (_frontDoor) {
                float dist = Vector3.Distance(_frontDoor.transform.localPosition, _frontStartPos);
                if (dist > _maxFrontJitter) _maxFrontJitter = dist;
            }
            if (_rearDoor) {
                float dist = Vector3.Distance(_rearDoor.transform.localPosition, _rearStartPos);
                if (dist > _maxRearJitter) _maxRearJitter = dist;
            }

            if (elapsed >= 2.0f) {
                _testDone = true;
                string res = JsonUtility.ToJson(new TestResult {
                    success = true,
                    maxFrontJitter = _maxFrontJitter,
                    maxRearJitter = _maxRearJitter,
                    frontPos = _frontDoor ? _frontDoor.transform.localPosition : Vector3.zero,
                    rearPos = _rearDoor ? _rearDoor.transform.localPosition : Vector3.zero,
                    frontKinematic = _frontDoor ? _frontDoor.GetComponent<Rigidbody>().isKinematic : false,
                    rearKinematic = _rearDoor ? _rearDoor.GetComponent<Rigidbody>().isKinematic : false
                });
                SessionState.SetString(ResultKey, res);
                SessionState.SetString(StateKey, "Done");
                EditorApplication.isPlaying = false;
            }
        }

        private static void SelfDestruct() {
            string scriptPath = SessionState.GetString(ScriptPathKey, "");
            if (!string.IsNullOrEmpty(scriptPath) && AssetDatabase.AssetPathExists(scriptPath)) AssetDatabase.DeleteAsset(scriptPath);
            SessionState.EraseString(StateKey);
            SessionState.EraseString(ScriptPathKey);
        }

        [System.Serializable]
        private class TestResult {
            public bool success;
            public float maxFrontJitter;
            public float maxRearJitter;
            public Vector3 frontPos;
            public Vector3 rearPos;
            public bool frontKinematic;
            public bool rearKinematic;
        }
    }
}
