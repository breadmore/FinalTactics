#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Networking;

namespace GoogleSheetsToUnity.ThirdPary
{
    public class EditorCoroutineRunner
    {
        private static List<EditorCoroutineState> coroutineStates;
        private static List<EditorCoroutineState> finishedThisUpdate;
        private static EditorCoroutineState uiCoroutineState;

        public static EditorCoroutine StartCoroutine(IEnumerator coroutine)
        {
            return StoreCoroutine(new EditorCoroutineState(coroutine));
        }

        public static EditorCoroutine StartCoroutineWithUI(IEnumerator coroutine, string title, bool isCancelable = false)
        {
            if (uiCoroutineState != null)
            {
                Debug.LogError("EditorCoroutineRunner only supports running one coroutine that draws a GUI! [" + title + "]");
                return null;
            }
            EditorCoroutineRunner.uiCoroutineState = new EditorCoroutineState(coroutine, title, isCancelable);
            return StoreCoroutine(uiCoroutineState);
        }

        private static EditorCoroutine StoreCoroutine(EditorCoroutineState state)
        {
            if (coroutineStates == null)
            {
                coroutineStates = new List<EditorCoroutineState>();
                finishedThisUpdate = new List<EditorCoroutineState>();
            }

            if (coroutineStates.Count == 0)
                EditorApplication.update += Runner;

            coroutineStates.Add(state);

            return state.editorCoroutineYieldInstruction;
        }

        public static void UpdateUILabel(string label)
        {
            if (uiCoroutineState != null && uiCoroutineState.showUI)
            {
                uiCoroutineState.Label = label;
            }
        }

        public static void UpdateUIProgressBar(float percent)
        {
            if (uiCoroutineState != null && uiCoroutineState.showUI)
            {
                uiCoroutineState.PercentComplete = percent;
            }
        }

        public static void UpdateUI(string label, float percent)
        {
            if (uiCoroutineState != null && uiCoroutineState.showUI)
            {
                uiCoroutineState.Label = label;
                uiCoroutineState.PercentComplete = percent;
            }
        }

        private static void Runner()
        {
            for (int i = 0; i < coroutineStates.Count; i++)
            {
                TickState(coroutineStates[i]);
            }

            for (int i = 0; i < finishedThisUpdate.Count; i++)
            {
                coroutineStates.Remove(finishedThisUpdate[i]);

                if (uiCoroutineState == finishedThisUpdate[i])
                {
                    uiCoroutineState = null;
                    EditorUtility.ClearProgressBar();
                }
            }
            finishedThisUpdate.Clear();

            if (coroutineStates.Count == 0)
            {
                EditorApplication.update -= Runner;
            }
        }

        private static void TickState(EditorCoroutineState state)
        {
            if (state.IsValid)
            {
                state.Tick();

                if (state.showUI && uiCoroutineState == state)
                {
                    uiCoroutineState.UpdateUI();
                }
            }
            else
            {
                finishedThisUpdate.Add(state);
            }
        }
    }

    internal class EditorCoroutineState
    {
        private IEnumerator coroutine;
        public bool IsValid => coroutine != null;
        public EditorCoroutine editorCoroutineYieldInstruction;

        private object current;
        private Type currentType;
        private float timer;
        private EditorCoroutine nestedCoroutine;
        private DateTime lastUpdateTime;

        public bool showUI;
        private bool cancelable;
        private bool canceled;
        private string title;
        public string Label;
        public float PercentComplete;

        public void UpdateUI()
        {
            if (showUI)
            {
                EditorUtility.DisplayProgressBar(title, Label, PercentComplete);
            }
        }
        public EditorCoroutineState(IEnumerator coroutine)
        {
            this.coroutine = coroutine;
            editorCoroutineYieldInstruction = new EditorCoroutine();
            showUI = false;
            lastUpdateTime = DateTime.Now;
        }

        public EditorCoroutineState(IEnumerator coroutine, string title, bool isCancelable)
        {
            this.coroutine = coroutine;
            editorCoroutineYieldInstruction = new EditorCoroutine();
            showUI = true;
            cancelable = isCancelable;
            this.title = title;
            Label = "initializing....";
            PercentComplete = 0.0f;
            lastUpdateTime = DateTime.Now;
        }

        public void Tick()
        {
            if (coroutine != null)
            {
                if (canceled)
                {
                    Stop();
                    return;
                }

                bool isWaiting = false;
                var now = DateTime.Now;
                if (current != null)
                {
                    if (current is WaitForSeconds wait)
                    {
                        FieldInfo m_Seconds = typeof(WaitForSeconds).GetField("m_Seconds", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (m_Seconds != null)
                        {
                            timer = (float)m_Seconds.GetValue(wait);
                        }
                        isWaiting = timer > 0.0f;
                    }
                    else if (current is UnityWebRequestAsyncOperation webRequest)
                    {
                        isWaiting = !webRequest.isDone;
                    }
                    else if (current is CustomYieldInstruction yieldInstruction)
                    {
                        isWaiting = yieldInstruction.keepWaiting;
                    }
                }
                lastUpdateTime = now;

                if (canceled)
                {
                    Stop();
                    return;
                }

                if (!isWaiting)
                {
                    bool update = coroutine.MoveNext();

                    if (update)
                    {
                        current = coroutine.Current;
                        currentType = current?.GetType();
                    }
                    else
                    {
                        Stop();
                    }
                }
            }
        }

        private void Stop()
        {
            coroutine = null;
            editorCoroutineYieldInstruction.HasFinished = true;
        }
    }

    public class EditorCoroutine : YieldInstruction
    {
        public bool HasFinished;
    }
}
#endif
