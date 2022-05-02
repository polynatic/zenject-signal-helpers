using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ZenjectSignalHelpers.Utils;

// ReSharper disable HeapView.DelegateAllocation

namespace ZenjectSignalHelpers.Editor
{
    /// <summary>
    /// Check all signal names if they comply with the naming scheme to distinguish the usage of commands and events
    /// and notifies the author in the debug console.
    /// </summary>
    public class EditorSignalValidation
    {
        [InitializeOnLoadMethod]
        private static void ValidateSignalsAfterReloading()
        {
            TypeNamesThatImplementInterface<ICommandSignal>(WithBadCommandName)
                .ForEach(LogBadCommandName);

            TypeNamesThatImplementInterface<IEventSignal>(WithBadEventName)
                .ForEach(LogBadEventName);

            SignalsThatAreNeitherCommandNorEvent()
                .ForEach(LogUnspecifiedSignal);
        }

        /// <summary>
        /// Find all type names with the interface TType and a name that conforms to a validator function.
        /// </summary>
        private static IEnumerable<string> TypeNamesThatImplementInterface<TType>(Func<string, bool> isInvalidName) =>
            from assembly in AppDomain.CurrentDomain.GetAssemblies()
            from type in assembly.GetTypes()
            where typeof(TType) != type && typeof(TType).IsAssignableFrom(type)
            where isInvalidName(type.Name)
            select type.Name;

        /// <summary>
        /// Find and return all ISignal type names that are neither ICommand nor IEvent.
        /// </summary>
        private static IEnumerable<string> SignalsThatAreNeitherCommandNorEvent() =>
            from assembly in AppDomain.CurrentDomain.GetAssemblies()
            from type in assembly.GetTypes()
            where typeof(ISignal) != type
                  && typeof(ISignal).IsAssignableFrom(type)
                  && !typeof(ICommandSignal).IsAssignableFrom(type)
                  && !typeof(IEventSignal).IsAssignableFrom(type)
            select type.Name;

        private static bool WithBadCommandName(string name) => !name.StartsWith("Do");

        private static bool WithBadEventName(string name) => !name.StartsWith("On");

        private static void LogBadCommandName(string name) => Debug.LogError(
            $"{name.Italic()} is not a good command name. Command signal names should start with 'Do...'."
        );

        private static void LogBadEventName(string name) => Debug.LogError(
            $"{name.Italic()} is not a good event name. Event signal names should start with 'On...'."
        );

        private static void LogUnspecifiedSignal(string name) => Debug.LogError(
            $"{name.Italic()} signal intent is not clear. Please specify by using ICommand or IEvent instead of ISignal."
        );
    }
}

// ReSharper restore HeapView.DelegateAllocation