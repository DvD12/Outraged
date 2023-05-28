using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using UnityEngine.Profiling;
using Newtonsoft.Json;
using Photon.Pun;
using System.Net;
using Photon.Realtime;

namespace Outraged
{
    public static class ConverterExtensions
    {
        public static void SetActive(this Button value, bool active)
        {
            value.gameObject.SetActive(active);
        }

        public static int GetWidth(this string text, int fontSize = 24)
        {
            if (text == null) { return 0; }
            float width = 0;
            float approximateFactor = 0.5f;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (c == '<') // Tag encountered - jump to end ">"
                {
                    if (text.Substring(i).Contains("<sprite"))
                    {
                        width += 2 * approximateFactor;
                    }
                    for (int j = i; j < text.Length; j++)
                    {
                        if (text[j] == '>')
                        {
                            i = j;
                            break;
                        }
                    }
                    continue;
                }
                else
                {
                    CharacterInfo ci;
                    width += (int)(fontSize * approximateFactor); // Approximation
                }
            }
            return (int)width;
        }


        public static bool HasIndex<T>(this IEnumerable<T> enumeration, int i)
        {
            return i >= 0 && enumeration.Count() > i;
        }
        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            foreach (T item in enumeration)
            {
                action(item);
            }
        }
        public static IEnumerable<IEnumerable<T>> GetCombinations<T>(this IEnumerable<T> list, int num)
        {
            return num == 0 ? new[] { new T[0] } : list.SelectMany((e, i) => list.Skip(i + 1).GetCombinations(num - 1).Select(c => (new[] { e }).Concat(c)));
        }

        public static T GetRandomElement<T>(this IEnumerable<T> Source, string LogAppend = "", bool NetworkSync = false)
        {
            int count = Source.Count();
            if (count == 0) { return default(T); }
            return (T)Source.ElementAt(GameState.Instance == null ? new System.Random(Guid.NewGuid().GetHashCode()).Next(0, count) : GameState.Instance.GetRandomRange(0, count - 1, LogAppend, NetworkSync));
        }

        public static IEnumerable<T> RandomizeOrder<T>(this IEnumerable<T> Source, string LogAppend = "", bool NetworkSync = false)
        {
            int n = Source.Count();
            List<T> Result = Source.ToList();
            while (n > 1)
            {
                n--;
                int k = GameState.Instance.GetRandomRange(0, Source.Count() - 1, LogAppend, NetworkSync, n >= Source.Count() - 1 || n <= 2); // Only log first and last
                T value = Result[k];
                Result[k] = Result[n]; // O(1) access instead of O(n) retrieval in case of list.Remove(item)
                Result[n] = value;
            }
            return Result;
        }

        public static void DestroyChildren(this GameObject owner)
        {
            if (owner == null) { return; }
            int j = owner.transform.childCount;
            for (int i = j - 1; i >= 0; i--)
            {
                owner.transform.GetChild(i).gameObject.name += "_Deleted";
                UnityEngine.Object.Destroy(owner.transform.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Removes the substring placed between "begin" and "end", including "begin" and "end".
        /// </summary>
        public static String RemoveWithin(this String value, String begin, String end)
        {
            int beginIndex = value.IndexOf(begin);
            int endIndex = value.IndexOf(end);
            if (beginIndex < 0 || endIndex < 0) { return value; }
            return value.Remove(beginIndex, endIndex + end.Length - beginIndex);
        }

        public static Transform GetChildFromName(this GameObject instance, string name)
        {
            foreach (Transform w in instance.transform)
            {
                if (w.name == name) { return w; }
            }
            return null;
        }
        public static Rect GetScreenBoundaries()
        {
            // Account for canvas scaling - reference https://forum.unity.com/threads/canvasscaler-current-scale.285134/
            var canvasScaler = MenuHandler.CanvasFront.GetComponent<CanvasScaler>();
            float referenceWidth = canvasScaler.referenceResolution.x; // The resolution of the actual game canvas (CanvasBack and CanvasFront), not the background canvas (VectorCanvas)
            float referenceHeight = canvasScaler.referenceResolution.y;

            var canvasRect = MenuHandler.CanvasFront.GetComponent<CanvasScaler>().gameObject.GetComponent<RectTransform>();

            var screenBoundaryXLeft = (canvasRect.sizeDelta.x - referenceWidth) / 2; // If game is resized to non-standard ratio, VectorCanvas shows some padding; the "actual" game canvas is offset by (the width of the background canvas - the reference width) / 2
            var screenBoundaryXRight = canvasRect.sizeDelta.x - screenBoundaryXLeft;
            var screenBoundaryYBottom = (canvasRect.sizeDelta.y - referenceHeight) / 2; // Unity y axis goes UP, so bottom should be around 0
            var screenBoundaryYTop = canvasRect.sizeDelta.y - screenBoundaryYBottom;

            return new Rect(screenBoundaryXLeft, screenBoundaryYBottom, screenBoundaryXRight, screenBoundaryYTop);
        }
        public static Vector3 GetScreenScale()
        {
            return MenuHandler.VectorCanvas.GetComponent<CanvasScaler>().gameObject.GetComponent<RectTransform>().localScale; // VectorCanvas scales accordingly with screen resolution - scale * canvas size = Screen.width (or height)
        }
        public static Vector2 GetMousePosition(bool scaleToScreen = true)
        {
            Vector3 scale = scaleToScreen ? GetScreenScale() : new Vector3(1, 1, 1);
            if (Input.touchCount > 0) { return Input.touches.Last().position / scale; }
            else { return new Vector2(Input.mousePosition.x / scale.x, Input.mousePosition.y / scale.y); } // Mouse position is relative to screen coordinates, not world/Unity coordinates. As an object's position doesn't care about screen coordinates, we need to divide by scaling (which is the function that transformed world coords into screen coords afterall) to do the inverse operation
        }

        /// <summary>
        /// To be used in FixedUpdate to scroll a scrollbar by moving mouse off the screen edges
        /// </summary>
        public static void ScrollWithMouse(this Scrollbar scrollbar, float velocity, bool isHorizontal = false, bool alternateScroll = false)
        {
            velocity = alternateScroll ? -velocity : velocity;
            var scale = ConverterExtensions.GetScreenScale();
            var screenBoundaries = ConverterExtensions.GetScreenBoundaries();
            var scaledMousePosition = ConverterExtensions.GetMousePosition(true);
            int scaledMousePositionOrigin = isHorizontal ? (int)scaledMousePosition.x : (int)scaledMousePosition.y;
            int screenBoundaryLeft = isHorizontal ? (int)screenBoundaries.x : (int)screenBoundaries.y;
            int screenBoundaryRight = isHorizontal ? (int)screenBoundaries.width : (int)screenBoundaries.height;

            if (scaledMousePositionOrigin < screenBoundaryLeft + 30)
            {
                scrollbar.value -= velocity;
            }
            else if (scaledMousePositionOrigin > screenBoundaryRight - 30)
            {
                scrollbar.value += velocity;
            }
        }

        public static void OnValueChange(this TMP_InputField value, Action<string> action)
        {
            var evt = new TMP_InputField.OnChangeEvent();
            evt.AddListener((s) => action(s));
            value.onValueChanged = evt;
        }
        public static void OnValueChanged(this TMP_InputField value, Action<string> action)
        {
            var evt = new TMP_InputField.SubmitEvent();
            evt.AddListener((s) => action(s));
            value.onEndEdit = evt;
        }

        public static bool AddIfNotPresent<T, V>(this Dictionary<T, V> dict, T key, V val)
        {
            if (dict.ContainsKey(key)) { return false; }
            dict.Add(key, val);
            return true;
        }
        public static T Peek<T>(this List<T> value) => value[value.Count() - 1];
        public static T Pop<T>(this List<T> value)
        {
            T result = value.Peek();
            value.RemoveAt(value.Count - 1);
            return result;
        }
        public static void Push<T>(this List<T> value, T val) => value.Add(val);
        public static void ScrollToEnd(this Scrollbar scrollbar, bool end)
        {
            TaskController.Create(nameof(scrollbar.gameObject.name), ScrollInternal(scrollbar, end ? 0 : 1)); 
        }
        private static System.Collections.IEnumerator ScrollInternal(Scrollbar scrollbar, int position)
        {
            yield return new WaitForEndOfFrame(); // If we don't wait for end of frame scrollbar isn't aware of geometry changes
            scrollbar.value = position;
        }

        public static void SwitchMicrophone(this Photon.Voice.Unity.Recorder recorder, bool? val = null)
        {
            recorder.TransmitEnabled = (bool)GameState.Instance?.Rules.AllowVoiceChat && (val.HasValue ? val.Value : !recorder.TransmitEnabled);
            GameMenuSharedContainer.Instance?.UpdateMuteSelf();
        }
        public static bool IsSendingData(this Photon.Voice.Unity.Recorder recorder) => (bool)GameState.Instance?.Rules.AllowVoiceChat && recorder.TransmitEnabled && recorder.LevelMeter.CurrentPeakAmp > recorder.VoiceDetectionThreshold;
        public static bool IsSelf(this Player value) => value.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber;
        public static string GetID(this Player value) => (string)value.CustomProperties[Profile.PlayerPropertyGUID];
        public static RoomSettings DeserializeSettings(this RoomInfo value)
        {
            return JsonConvert.DeserializeObject<RoomSettings>(value.CustomProperties[RoomSettings.RoomSettingsString].ToString(), DataHeaders.GetFormatting());
        }
        public static GameState Deserialize(this string value) => JsonConvert.DeserializeObject<GameState>(value, DataHeaders.GetFormatting());
        public static string Serialize(this GameState instance) => JsonConvert.SerializeObject(instance, DataHeaders.GetFormatting());
        public static GameProgress GetState(this RoomInfo value) => !value.CustomProperties.ContainsKey(RoomSettings.GameProgressString) ? GameProgress.NotStarted : (GameProgress)Enum.Parse(typeof(GameProgress), value.CustomProperties[RoomSettings.GameProgressString].ToString());

        public static float Smooth(this float t) => ((float)t).Smooth(); // https://gamedevbeginner.com/the-right-way-to-lerp-in-unity-with-examples/#lerp_with_easing
        public static double Smooth(this double t) => t * t * (3 - 2 * t);
        public static float SmoothStart(this float t) => ((float)t).SmoothStart();
        public static double SmoothStart(this double t) => (t * t * t * t + t) / 2;
    }
}