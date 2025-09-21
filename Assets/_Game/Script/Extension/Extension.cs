using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DG.Tweening;
using UnityEditor;
using UnityEngine;

namespace TrungKien
{
    public static class Extension
    {
        const float epsilon = 0.005f;
        #region Enum
        static Dictionary<Type, Dictionary<Enum, string>> dicEnumToString = new();
        static Dictionary<Type, Dictionary<string, object>> dicStringToEnum = new();
        static Dictionary<string, int> dicStringToInt = new();
        static Dictionary<int, string> dicIntToString = new();
        public static int StringToInt(this string str)
        {
            if (!dicStringToInt.ContainsKey(str))
            {
                dicStringToInt.Add(str, int.Parse(str));
            }
            return dicStringToInt[str];
        }
        public static string ExToString(this int val)
        {
            if (!dicIntToString.ContainsKey(val))
            {
                dicIntToString.Add(val, val.ToString());
            }
            return dicIntToString[val];
        }

        public static string ExToString(this Enum enumValue)
        {
            Type enumType = enumValue.GetType();
            if (!dicEnumToString.TryGetValue(enumType, out var enumDict))
            {
                enumDict = new Dictionary<Enum, string>();
                foreach (Enum val in Enum.GetValues(enumType))
                {
                    enumDict[val] = val.ToString();
                }
                dicEnumToString[enumType] = enumDict;
            }
            return dicEnumToString[enumType][enumValue];
        }

        public static T ToEnum<T>(this string value) where T : struct, Enum
        {
            Type enumType = typeof(T);

            if (!dicStringToEnum.TryGetValue(enumType, out var enumMap))
            {
                enumMap = new Dictionary<string, object>();
                dicStringToEnum[enumType] = enumMap;
            }

            if (!enumMap.TryGetValue(value, out var enumValue))
            {
                enumValue = Enum.Parse(enumType, value, true);
                enumMap[value] = enumValue;
            }

            return (T)enumValue;
        }
        public static bool TryToEnum<T>(this string value, out T _type) where T : struct
        {
            if (Enum.TryParse<T>(value, out _type))
            {
                return true;
            }
            Debug.LogWarning($"Fail to parse {value}");
            return false;
        }

        public static List<T> GetListEnum<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>().ToList();
        }
        #endregion
        #region CSVString
        static void ReplaceChar(ref string stringInput, string stringCharRemove, string newChar)
        {
            if (stringInput.Contains(stringCharRemove)) stringInput = stringInput.Replace(stringCharRemove, newChar);
        }
        static void CSVRemoveChar(ref string stringInput)
        {
            ReplaceChar(ref stringInput, "x", string.Empty);
            ReplaceChar(ref stringInput, ",", string.Empty);
            ReplaceChar(ref stringInput, "%", string.Empty);
        }
        public static float ParseFloat(string value)
        {
            CSVRemoveChar(ref value);
            float result;
            bool isCheck = float.TryParse(value, out result);
            if (!isCheck)
            {
                Debug.LogWarning(value + " not correct");
            }
            return result;
        }
        public static double ParseDouble(string value)
        {
            CSVRemoveChar(ref value);
            double result;
            bool isCheck = double.TryParse(value, out result);
            if (!isCheck)
            {
                Debug.LogWarning(value + " not correct");
            }
            return result;
        }
        public static double ParseTimeSecond(string value)
        {
            string[] splitString = value.Split(' ');
            double result = 0;
            for (int i = 0; i < splitString.Length; i++)
            {
                if (splitString[i].Contains('d'))
                {
                    result += ParseInt(splitString[i].Replace("d", "")) * 86400; // second of one day
                }
                else if (splitString[i].Contains('h'))
                {
                    result += ParseInt(splitString[i].Replace("h", "")) * 3600; // second of one hour
                }
                else if (splitString[i].Contains('m'))
                {
                    result += ParseInt(splitString[i].Replace("m", "")) * 60; // second of one minus
                }
                else if (splitString[i].Contains('s'))
                {
                    result += ParseInt(splitString[i].Replace("s", ""));
                }
            }
            return result;
        }

        public static int ParseInt(string value)
        {
            CSVRemoveChar(ref value);
            int val = 0;
            if (int.TryParse(value, out val))
                return val;
            Debug.LogError("Wrong Input " + value);
            return val;
        }
        public static long ParseLong(string value)
        {
            CSVRemoveChar(ref value);
            long val = 0;
            if (long.TryParse(value, out val))
                return val;
            Debug.LogError("Wrong Input " + value);
            return val;
        }

        public static bool ParseBool(string data)
        {
            bool val = false;
            if (bool.TryParse(data, out val))
                return val;
            Debug.LogError("Wrong Input" + data);
            return val;
        }
        #endregion
#if UNITY_EDITOR
        public static Transform[] GetTransformsChild(this Transform parent, bool status = false)
        {
            return parent.GetComponentsInChildren<Transform>(status).Where(t => t != parent).ToArray();
        }
#endif

#if UNITY_EDITOR
        public static IEnumerator IELoadData(string urlData, System.Action<string> actionComplete, bool showAlert = false)
        {
            var www = new WWW(urlData);
            float time = 0;
            //TextAsset fileCsvLevel = null;
            while (!www.isDone)
            {
                time += 0.001f;
                if (time > 10000)
                {
                    yield return null;
                    Debug.Log("Downloading...");
                    time = 0;
                }
            }
            if (!string.IsNullOrEmpty(www.error))
            {
#if UNITY_EDITOR
                UnityEditor.EditorUtility.DisplayDialog("Notice", "Load CSV Fail", "OK");
#endif
                yield break;
            }
            yield return null;
            actionComplete?.Invoke(www.text);
            yield return null;

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.SaveAssets();
            if (showAlert)
                UnityEditor.EditorUtility.DisplayDialog("Notice", "Load Data Success", "OK");
            else
                Debug.Log("<color=yellow>Download Data Complete</color>");
#endif
        }
#endif
        public static void SaveAssetEditor(UnityEngine.Object go)
        {
#if UNITY_EDITOR

            UnityEditor.Undo.RegisterCompleteObjectUndo(go, "Save level data");
            UnityEditor.EditorUtility.SetDirty(go);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
#endif
        }
        public static bool EqualFloat(this float tmpNum1, float tmpNum2)
        {
            if (Mathf.Abs(tmpNum1 - tmpNum2) < 0.02f)
            {
                return true;
            }
            else return false;
        }
        public static bool EqualOrGreaterFloat(this float tmpNum1, float tmpNum2)
        {
            if (tmpNum1 > tmpNum2 || EqualFloat(tmpNum1, tmpNum2))
            {
                return true;
            }
            else return false;
        }
        public static bool EqualOrLessFloat(this float tmpNum1, float tmpNum2)
        {
            if (EqualFloat(tmpNum1, tmpNum2) || tmpNum1 < tmpNum2)
            {
                return true;
            }
            else return false;
        }

        public static bool EqualVector(this Vector3 a, Vector3 b)
        {
            return Mathf.Abs(a.x - b.x) < epsilon && Mathf.Abs(a.y - b.y) < epsilon && Mathf.Abs(a.z - b.z) < epsilon;
        }
        public static void ExDoShake(this Transform tf, SkakeType type, float duration, float delay = 0)
        {
            (float strength, int vibrato) = type switch
            {
                SkakeType.Fry => (0.01f, 15),
                SkakeType.Riped => (0.02f, 10),
                SkakeType.Overripe => (0.05f, 10),
                SkakeType.Griller => (0.01f, 10),
                SkakeType.Heavy => (0.3f, 30),
                _ => (0.1f, 10)
            };
            tf.DOShakePosition(duration, strength, vibrato).SetEase(Ease.Linear).SetDelay(delay);
        }
        public enum State
        {
            MoveIn, MoveOut, None
        }
        const float rateIn = 0.8f, rateOut = 0.2f;
        public static void ExDoMove(this Transform TF, Vector2 offset, float time, State state, System.Action doneAction = null, bool isActiveAtBegin = true, System.Action updateAction = null)
        {
            if (isActiveAtBegin)
            {
                TF.gameObject.SetActive(true);
            }
            Vector2 basePos = TF.position;
            Vector2 visualPos = (Vector2)TF.position + offset;
            Vector2 destinationPos;
            if (state == State.MoveIn)
            {
                TF.position = visualPos;
                destinationPos = basePos;
            }
            else
            {
                destinationPos = visualPos;
            }
            float rate = (state == State.MoveIn ? rateIn : rateOut);
            TF.DOMove(destinationPos + (state == State.MoveIn ? -(offset * 0.05f) : Vector2.zero), time * rate).SetEase(Ease.Linear).OnUpdate(() => updateAction?.Invoke()).OnComplete(() => TF.DOMove(destinationPos, time * (1 - rate)).OnComplete(() => doneAction?.Invoke()));
        }
        public static T GetItemCanInteract<T>(Dictionary<Collider2D, T> dic, Collider2D collision)
        {
            if (!dic.ContainsKey(collision))
            {
                T item = collision.GetComponent<T>();
                if (item != null)
                {
                    dic.Add(collision, item);
                }
            }
            if (dic.ContainsKey(collision))
            {
                return dic[collision];
            }
            else
            {
                return default;
            }
        }
        public static T GetItemCanInteract<T>(Dictionary<Collider, T> dic, Collider collision)
        {
            if (!dic.ContainsKey(collision))
            {
                T item = collision.GetComponent<T>();
                if (item != null)
                {
                    dic.Add(collision, item);
                }
            }
            if (dic.ContainsKey(collision))
            {
                return dic[collision];
            }
            else
            {
                return default;
            }
        }
        public static T Find<T>(this T[] array, Func<T, bool> condition)
        {
            foreach (var item in array)
            {
                if (condition(item))
                    return item;
            }
            return default;
        }
        public static void ForEach<T>(this T[] array, Action<T> action)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (action == null) throw new ArgumentNullException(nameof(action));
            for (int i = 0; i < array.Length; i++)
            {
                action(array[i]);
            }
        }
        public static string RemoveNumber(this string inputString)
        {
            return Regex.Replace(inputString, @"[0-9]", string.Empty);
        }
        // public static void ExPlay(this Animation anim, System.Action actionDone = null)
        // {
        //     LevelControl.Ins.StartCoroutine(IEExPlay(anim, actionDone));
        // }
        // static IEnumerator IEExPlay(Animation anim, System.Action actionDone)
        // {
        //     anim.Play();
        //     yield return new WaitForSeconds(anim.clip.length);
        //     actionDone?.Invoke();
        // }
#if UNITY_EDITOR
        public static void ForceSavePrefab(GameObject prefabAsset)
        {
            if (prefabAsset == null)
            {
                Debug.LogWarning("❌ PrefabAsset is null!");
                return;
            }

            string path = AssetDatabase.GetAssetPath(prefabAsset);
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning($"⚠ {prefabAsset.name} is not a prefab asset in Project.");
                return;
            }

            PrefabUtility.SavePrefabAsset(prefabAsset);
            AssetDatabase.ImportAsset(path);

            Debug.Log($"💾 Force saved prefab asset: <color=yellow>{path}</color>");
        }
        public static void ApplyToPrefab(List<GameObject> prefabAssets)
        {
            if (prefabAssets == null || prefabAssets.Count == 0)
            {
                Debug.LogWarning("⚠ PrefabAssets array is empty!");
                return;
            }

            foreach (var prefab in prefabAssets)
            {
                ForceSavePrefab(prefab);
            }

            // Refresh project sau khi xong tất cả
            AssetDatabase.Refresh();
        }
        public static void RevertPrefabs(List<GameObject> instances)
        {
            foreach (var go in instances)
            {
                if (go == null) continue;
                PrefabUtility.RevertPrefabInstance(go, InteractionMode.UserAction);
                Debug.Log($"↩ Reverted {go.name} to prefab values.");
            }
        }
#endif
        public static float GetValue(float value, float floor, float ceiling, float min, float max)
        {
            return Mathf.Lerp(min, max, (value - floor) / (ceiling - floor));
        }
    }
}