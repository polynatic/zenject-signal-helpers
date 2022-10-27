using System;
using System.Linq;
using UnityEngine;
using Zenject;
using static ZenjectSignalHelpers.ZenjectSignalLoggerSettings;

namespace ZenjectSignalHelpers
{
    public class ZenjectSignalLogger
    {
        [Inject] private readonly AutomaticSignalHandlers Signals;

        public static void Install(DiContainer container) =>
            container
                .Bind<ZenjectSignalLogger>()
                .AsSingle()
                .NonLazy();


        [SignalHandler]
        private void OnSignal(ISignal signal) => LogSignal(signal);

        private void LogSignal(object signal)
        {
            if (!ShouldLogSignal(signal.GetType()))
                return;

            var category = SignalTypeCategory(signal);
            var color = SignalTypeColor(signal);
            var fields = FormatFields(signal);
            var type = signal.GetType().Name;

            Debug.Log($"<b><color={color}>{category}: </Color> {type}</b>\n{fields}\n");
        }

        private string FormatFields(object obj)
        {
            var fields = obj
                         .GetType()
                         .GetFields()
                         .Select(
                             field =>
                             {
                                 var name = field.Name;
                                 var value = field.GetValue(obj);
                                 return $"{name} = {value}";
                             }
                         )
                         .ToArray();

            return $"{{ {String.Join(", ", fields)} }}";
        }

        private string SignalTypeCategory(object signal) =>
            signal switch
            {
                ICommandSignal => "Command",
                IEventSignal => "Event",
                ISignal => "Signal",
                _ => "Untyped",
            };


        private string SignalTypeColor(object signal) =>
            signal switch
            {
                ICommandSignal => "lime",
                IEventSignal => "lime",
                _ => "red",
            };
    }
}
