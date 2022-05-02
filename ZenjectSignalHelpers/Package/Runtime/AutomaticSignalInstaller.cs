using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;
using ZenjectSignalHelpers.Utils;

namespace ZenjectSignalHelpers
{
    /// <summary>
    /// Convenience methods to install signals.
    /// </summary>
    public static class AutomaticSignalInstaller
    {
        /// <summary>
        /// Install all signal types found in the complete assembly into the given container.
        /// </summary>
        public static void InstallAllSignals(DiContainer container)
        {
            AllSignals().ForEach(signal => container.DeclareSignal(signal).OptionalSubscriber());
        }

        /// <summary>
        /// Returns all signal types in all assemblies except the base types.
        /// </summary>
        private static IEnumerable<Type> AllSignals() =>
            from assembly in AppDomain.CurrentDomain.GetAssemblies()
            from type in assembly.GetTypes()
            where typeof(ISignal).IsAssignableFrom(type)
                  && typeof(ISignal) != type
                  && typeof(ICommandSignal) != type
                  && typeof(IEventSignal) != type
            select type;
    }
}