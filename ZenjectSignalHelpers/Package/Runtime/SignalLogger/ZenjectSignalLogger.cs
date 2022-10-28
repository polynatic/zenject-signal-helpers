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
            var title = fields.Length == 1 ? $" <color=grey>{{<i>{fields[0].Replace(" =", ":")}</i>}}</color>" : "";
            var joinedFields = $"{{ {String.Join(", ", fields)} }}";

            Debug.Log($"<b><color={color}>{category}: </Color> {type}{title}</b>\n{joinedFields}\n");
        }

        private string[] FormatFields(object obj) =>
            obj
                .GetType()
                .GetFields()
                .Select(field => $"{field.Name} = {field.GetValue(obj)}")
                .ToArray();

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
