using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Unity.AI.Assistant.PlayModeTest
{
    [InitializeOnLoad]
    internal static class PlayModeTestRunner
    {
        private const string StateKey = "PlayModeTest.State";
        private const string ResultKey = "PlayModeTest.Result";
        private const string ScriptPathKey = "PlayModeTest.ScriptPath";
        private const string SentinelLog = "PLAY_MODE_TEST_COMPLETE";

        private static readonly int WaitFrames = SessionState.GetInt("PlayModeTest.WaitFrames", 30);
        private static readonly float TestTimeout = SessionState.GetFloat("PlayModeTest.TestTimeout", 10f);

        private static List<string> _capturedLogs = new List<string>();
        private const int MaxCapturedLogs = 80;

        static PlayModeTestRunner()
        {
            string state = SessionState.GetString(StateKey, "Idle");
            switch (state)
            {
                case "Idle": break;
                case "WaitingForCompile":
                    Debug.Log("[PlayModeTest] Bootstrap compiled. Scheduling Play Mode entry.");
                    EditorApplication.delayCall += () =>
                    {
                        SessionState.SetString(StateKey, "EnteringPlayMode");
                        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
                        EditorApplication.isPlaying = true;
                    };
                    break;
                case "EnteringPlayMode":
                    EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
                    if (EditorApplication.isPlaying)
                    {
                        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                        SessionState.SetString(StateKey, "InPlayMode");
                        EditorApplication.update += WaitFramesThenRun;
                    }
                    break;
                case "InPlayMode":
                    if (EditorApplication.isPlaying)
                        EditorApplication.update += WaitFramesThenRun;
                    break;
                case "Done":
                    Debug.Log(SentinelLog);
                    EditorApplication.delayCall += SelfDestruct;
                    break;
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredPlayMode)
            {
                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                SessionState.SetString(StateKey, "InPlayMode");
                EditorApplication.update += WaitFramesThenRun;
            }
        }

        private static int _frameCount = 0;
        private static bool _setupDone = false;
        private static bool _testDone = false;
        private static double _t0;

        private static void WaitFramesThenRun()
        {
            if (!Application.isPlaying) return;
            _frameCount++;
            if (_frameCount < WaitFrames) return;
            if (_testDone) return;
            if (!_setupDone)
            {
                _setupDone = true;
                Application.logMessageReceived += OnLogMessage;
                _t0 = EditorApplication.timeSinceStartup;
                try { Setup(); } catch (System.Exception e) { Debug.LogError("[Test] Setup ex: " + e); Finish(true, e.Message); }
                return;
            }
            float elapsed = (float)(EditorApplication.timeSinceStartup - _t0);
            try
            {
                bool done = Tick(elapsed);
                if (done || elapsed >= TestTimeout) Finish(elapsed >= TestTimeout && !done, "timeout");
            }
            catch (System.Exception e) { Debug.LogError("[Test] Tick ex: " + e); Finish(true, e.Message); }
        }

        private static void Finish(bool isError, string err)
        {
            _testDone = true;
            EditorApplication.update -= WaitFramesThenRun;
            string json;
            try { json = GetResult(isError, err); }
            catch (System.Exception e) { json = JsonUtility.ToJson(new TestResult { success = false, error = "GetResult: " + e.Message, logs = _capturedLogs.ToArray() }); }
            Application.logMessageReceived -= OnLogMessage;
            SessionState.SetString(ResultKey, json);
            SessionState.SetString(StateKey, "Done");
            EditorApplication.isPlaying = false;
        }

        private static void SelfDestruct()
        {
            string scriptPath = SessionState.GetString(ScriptPathKey, "");
            if (!string.IsNullOrEmpty(scriptPath) && AssetDatabase.AssetPathExists(scriptPath))
                AssetDatabase.DeleteAsset(scriptPath);
            SessionState.EraseString(StateKey);
            SessionState.EraseString(ScriptPathKey);
        }

        private static void OnLogMessage(string message, string stackTrace, LogType type)
        {
            if (_capturedLogs.Count >= MaxCapturedLogs) return;
            if (type == LogType.Error || type == LogType.Exception ||
                message.Contains("[Test]") || message.Contains("TEST_RESULT"))
                _capturedLogs.Add("[" + type + "] " + message);
        }

        [System.Serializable]
        private class TestResult
        {
            public bool success;
            public string error;
            public string[] logs;
            public bool isPlaying;
            public bool startsInSocket;
            public bool nearHoverWhileSocketed;
            public bool canSelectWhileSocketed;
            public bool grabbedFromSocket;
            public bool grabbedWhenFree;
        }

        private static NearFarInteractor _nf;
        private static XRGrabInteractable _remote;
        private static XRSocketInteractor _socket;
        private static XRInteractionManager _mgr;
        private static Vector3 _remotePos;
        private static int _phase = 0;
        private static bool _nearHoverSocketed = false;
        private static TestResult _r = new TestResult();

        private static void Setup()
        {
            _r.isPlaying = Application.isPlaying;
            _mgr = Object.FindAnyObjectByType<XRInteractionManager>();
            _nf = GameObject.Find("Right_NearFarInteractor")?.GetComponent<NearFarInteractor>();
            _socket = Object.FindAnyObjectByType<XRSocketInteractor>();
            foreach (var g in Object.FindObjectsByType<XRGrabInteractable>(FindObjectsSortMode.None))
                if (g.gameObject.name == "Remote") { _remote = g; break; }
            if (_nf == null || _remote == null || _mgr == null) { Debug.LogError("[Test] missing refs"); Finish(true, "missing refs"); return; }

            var parent = _nf.transform.parent;
            if (parent != null)
                foreach (var b in parent.GetComponents<Behaviour>())
                    if (b.GetType().Name.Contains("TrackedPoseDriver")) b.enabled = false;

            bool inSocket = false;
            foreach (var s in _remote.interactorsSelecting) if (s is XRSocketInteractor) inSocket = true;
            _r.startsInSocket = inSocket;
            _remotePos = _remote.transform.position;
            // Move hand onto the remote for NEAR detection
            if (parent != null) parent.position = _remotePos;
            _nf.transform.position = _remotePos;
            Debug.Log("[Test] isPlaying=" + _r.isPlaying + " startsInSocket=" + inSocket);
        }

        private static bool Tick(float elapsed)
        {
            var parent = _nf.transform.parent;
            if (parent != null) parent.position = _remotePos;
            _nf.transform.position = _remotePos;

            // Phase 0 (until 1.5s): while socketed, observe near hover + canSelect
            if (_phase == 0)
            {
                var hov = _nf.interactablesHovered;
                if (hov != null) foreach (var h in hov) if (h == (IXRHoverInteractable)_remote) _nearHoverSocketed = true;
                if (elapsed >= 1.5f)
                {
                    _r.nearHoverWhileSocketed = _nearHoverSocketed;
                    _r.canSelectWhileSocketed = _mgr.CanSelect((IXRSelectInteractor)_nf, (IXRSelectInteractable)_remote);
                    Debug.Log("[Test] nearHoverSocketed=" + _r.nearHoverWhileSocketed + " canSelectSocketed=" + _r.canSelectWhileSocketed);

                    // Try real-path grab from socket (only if canSelect allows)
                    if (_r.canSelectWhileSocketed)
                    {
                        _mgr.SelectEnter((IXRSelectInteractor)_nf, (IXRSelectInteractable)_remote);
                        foreach (var s in _remote.interactorsSelecting) if (s == (IXRSelectInteractor)_nf) _r.grabbedFromSocket = true;
                        // release
                        if (_r.grabbedFromSocket) _mgr.SelectExit((IXRSelectInteractor)_nf, (IXRSelectInteractable)_remote);
                    }
                    Debug.Log("[Test] grabbedFromSocket=" + _r.grabbedFromSocket);

                    // Now eject remote from socket to test FREE grab
                    foreach (var s in new List<IXRSelectInteractor>(_remote.interactorsSelecting))
                        if (s is XRSocketInteractor) _mgr.SelectExit(s, (IXRSelectInteractable)_remote);
                    var rb = _remote.GetComponent<Rigidbody>();
                    if (rb != null) { rb.isKinematic = true; rb.useGravity = false; }
                    _phase = 1;
                    _t0 = EditorApplication.timeSinceStartup; // reset timer for phase 1
                    return false;
                }
                return false;
            }

            // Phase 1: remote free — check canSelect + grab
            if (elapsed >= 1.0f)
            {
                bool canSelectFree = _mgr.CanSelect((IXRSelectInteractor)_nf, (IXRSelectInteractable)_remote);
                if (canSelectFree)
                {
                    _mgr.SelectEnter((IXRSelectInteractor)_nf, (IXRSelectInteractable)_remote);
                    foreach (var s in _remote.interactorsSelecting) if (s == (IXRSelectInteractor)_nf) _r.grabbedWhenFree = true;
                }
                Debug.Log("[Test] canSelectFree=" + canSelectFree + " grabbedWhenFree=" + _r.grabbedWhenFree);
                return true;
            }
            return false;
        }

        private static string GetResult(bool isError, string err)
        {
            _r.success = !isError;
            if (string.IsNullOrEmpty(_r.error)) _r.error = err;
            _r.logs = _capturedLogs.ToArray();
            return JsonUtility.ToJson(_r);
        }
    }
}
