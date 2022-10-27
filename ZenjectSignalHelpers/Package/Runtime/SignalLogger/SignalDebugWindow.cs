using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using static ZenjectSignalHelpers.ZenjectSignalLoggerSettings;

namespace ZenjectSignalHelpers
{
    public class SignalDebugWindow : EditorWindow
    {
        private class Group
        {
            public string Name;
            public List<Type> Types = new List<Type>();
            public bool FoldOut;
        }

        private static List<Group> Groups;

        private const string GroupsPrefsKey = "ZenjectSignalHelpers.SignalDebugWindow.OpenGroups";
        private static List<string> OpenGroupsBeforeAssemblyReload;

        [MenuItem("Tools/Zenject Signal Helpers/Signal Logger", priority = 20000)]
        public static void ShowWindow() => GetWindow(typeof(SignalDebugWindow), false, "Signal Log");

        void OnEnable()
        {
            AssemblyReloadEvents.beforeAssemblyReload += RememberGroupFoldoutState;
            AssemblyReloadEvents.afterAssemblyReload += RestoreGroupFoldoutState;
        }

        void OnDisable()
        {
            AssemblyReloadEvents.beforeAssemblyReload -= RememberGroupFoldoutState;
            AssemblyReloadEvents.afterAssemblyReload -= RestoreGroupFoldoutState;
        }

        private void OnGUI()
        {
            Groups ??= CreateGroups();


            GUILayout.Label("Log Signals", EditorStyles.boldLabel);

            InfoBox();

            foreach (var group in Groups)
            {
                group.FoldOut = EditorGUILayout.BeginFoldoutHeaderGroup(group.FoldOut, group.Name);

                if (!group.FoldOut)
                {
                    EditorGUILayout.EndFoldoutHeaderGroup();
                    continue;
                }

                var indentLevel = EditorGUI.indentLevel;
                var wasPreviousCommand = false;
                var commandInterface = typeof(ICommandSignal);
                EditorGUI.indentLevel = 1;

                foreach (var signalType in group.Types)
                {
                    var isCommand = commandInterface.IsAssignableFrom(signalType);

                    if (!isCommand && wasPreviousCommand)
                    {
                        EditorGUILayout.Space();
                    }

                    var isToggled = EditorGUILayout.ToggleLeft(signalType.Name, ShouldLogSignal(signalType));
                    ShouldLogSignal(signalType, isToggled);

                    wasPreviousCommand = isCommand;
                }

                EditorGUILayout.EndFoldoutHeaderGroup();
                EditorGUI.indentLevel = indentLevel;
            }
        }

        private void InfoBox()
        {
            var indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 1;
            EditorGUILayout.Space(4);
            EditorGUILayout.HelpBox("All enabled signals will be logged to the console.", MessageType.Info);
            EditorGUILayout.Space(4);
            EditorGUI.indentLevel = indentLevel;
        }

        public void RememberGroupFoldoutState()
        {
            var openGroups = Groups?
                             .Where(group => group.FoldOut)
                             .Select(group => group.Name)
                             .ToList()
                             ?? new List<string>();

            var openGroupsString = String.Join(", ", openGroups);
            EditorPrefs.SetString(GroupsPrefsKey, openGroupsString);
        }

        public void RestoreGroupFoldoutState()
        {
            var openGroupsString = EditorPrefs.GetString(GroupsPrefsKey);

            if (openGroupsString == null)
                return;

            OpenGroupsBeforeAssemblyReload = openGroupsString.Split(", ").ToList();

            EditorPrefs.DeleteKey(GroupsPrefsKey);
        }

        private static List<Group> CreateGroups()
        {
            var signals = AutomaticSignalInstaller.AllSignals;
            var groups = new List<Group>();

            // separate signals into namespace groups
            foreach (var signal in signals)
            {
                var group = groups.Find(g => g.Name == signal.Namespace);
                if (group == null)
                {
                    group = new Group();
                    group.Name = signal.Namespace;

                    groups.Add(group);
                }

                group.Types.Add(signal);
            }

            // sort everything
            foreach (var group in groups)
            {
                group.Types.Sort(new TypeSort());
            }

            groups.Sort(new GroupSort());

            // restore foldout state
            if (OpenGroupsBeforeAssemblyReload != null)
            {
                foreach (var group in groups)
                {
                    if (OpenGroupsBeforeAssemblyReload.Contains(group.Name))
                    {
                        group.FoldOut = true;
                    }
                }
            }

            return groups;
        }

        private class TypeSort : Comparer<Type>
        {
            public override int Compare(Type x, Type y) => String.Compare(x?.Name, y?.Name, StringComparison.Ordinal);
        }

        private class GroupSort : Comparer<Group>
        {
            public override int Compare(Group x, Group y) => String.Compare(x?.Name, y?.Name, StringComparison.Ordinal);
        }
    }
}
