using System;
using Zenject;

namespace ZenjectSignalHelpers.Utils
{
    public static class ZenjectSignalExtensions
    {
        /// <summary>
        /// DeclareSignalWithInterfaces with type instead of generic and OptionalSubscriber on the declaration
        /// as most often not all interfaces have a subscriber.
        ///
        /// Is needed as long as it's not implemented in Zenject.
        /// </summary>
        public static DeclareSignalIdRequireHandlerAsyncTickPriorityCopyBinder DeclareSignalWithInterfaces(
            this DiContainer container,
            Type type)
        {
            var declaration = container.DeclareSignal(type);

            Type[] interfaces = type.GetInterfaces();
            int numOfInterfaces = interfaces.Length;
            for (int i = 0; i < numOfInterfaces; i++)
            {
                container.DeclareSignal(interfaces[i]).OptionalSubscriber();
            }

            return declaration;
        }
    }
}