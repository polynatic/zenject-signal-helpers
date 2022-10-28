using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace ZenjectSignalHelpers
{
    public class ZenjectSignalLoggerSettings : ScriptableObject, ISerializationCallbackReceiver
    {
        [NonSerialized] private static ZenjectSignalLoggerSettings Instance;
        private const string SettingsPath = "ZenjectSignalLoggerSettings";

        public static bool ShouldLogSignal(Type type) => !GetInstance()?.SignalsExcludedFromLog.Contains(type) ?? false;

        private static ZenjectSignalLoggerSettings GetInstance()
        {
            if (!Instance)
            {
                Instance = Resources.Load<ZenjectSignalLoggerSettings>(SettingsPath);

#if UNITY_EDITOR
                if (!Instance)
                {
                    Instance = CreateInstance<ZenjectSignalLoggerSettings>();

                    AssetDatabase.CreateAsset(Instance, $"Assets/Resources/{SettingsPath}.asset");
                }
#endif
            }

            return Instance;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticSettings()
        {
            Instance = null;
            GetInstance();
        }

#if UNITY_EDITOR

        public static void ShouldLogSignal(Type type, bool shouldLog)
        {
            if (ShouldLogSignal(type) == shouldLog)
                return;

            var instance = GetInstance();

            if (!instance)
                return;

            var excludedSignals = instance.SignalsExcludedFromLog;

            if (!shouldLog)
                excludedSignals.Add(type);
            else
                excludedSignals.Remove(type);

            instance.Save();
        }

        private void Save()
        {
            if (SaveTask != null)
                return;

            SaveTask = SaveDeferred();
        }

        private async Task SaveDeferred()
        {
            await Task.Yield(); // wait until later to only set dirty once if saving multiple times

            EditorUtility.SetDirty(Instance);
            SaveTask = null;
        }
#endif


        [NonSerialized] private HashSet<Type> SignalsExcludedFromLog = new HashSet<Type>();

        [NonSerialized] private Task SaveTask;

        [SerializeField, HideInInspector] private List<string> SerializedSignalsExcludedFromLog;

        public void OnBeforeSerialize() =>
            SerializedSignalsExcludedFromLog = SignalsExcludedFromLog?
                                               .Select(type => type.AssemblyQualifiedName)
                                               .ToList()
                                               ?? new List<string>();

        public void OnAfterDeserialize() =>
            SignalsExcludedFromLog = SerializedSignalsExcludedFromLog?
                                     .Select(typeName => Type.GetType(typeName))
                                     .Where(type => type != null)
                                     .ToHashSet()
                                     ?? new HashSet<Type>();
    }
}
