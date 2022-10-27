using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace ZenjectSignalHelpers.UniTask
{
    public class AutomaticSignalHandlers : ZenjectSignalHelpers.AutomaticSignalHandlers
    {
        public async UniTask<object> Wait<T1>()
            where T1 : struct, ISignal =>
            await Wait(typeof(T1));

        public async UniTask<object> Wait<T1, T2>()
            where T1 : struct, ISignal
            where T2 : struct, ISignal =>
            await Wait(typeof(T1), typeof(T2));

        public async UniTask<object> Wait<T1, T2, T3>()
            where T1 : struct, ISignal
            where T2 : struct, ISignal
            where T3 : struct, ISignal =>
            await Wait(typeof(T1), typeof(T2), typeof(T3));

        public async UniTask<object> Wait<T1, T2, T3, T4>()
            where T1 : struct, ISignal
            where T2 : struct, ISignal
            where T3 : struct, ISignal
            where T4 : struct, ISignal =>
            await Wait(typeof(T1), typeof(T2), typeof(T3), typeof(T4));

        public async UniTask<object> Wait(params Type[] types)
        {
            var subscriptions = new List<(Type type, Action<object> subscription)>();
            var completion = new UniTaskCompletionSource<object>();

            foreach (var type in types)
            {
                var subscription = SignalSubscription(completion);
                SignalBus.Subscribe(type, subscription);
                subscriptions.Add((type, subscription));
            }

            try
            {
                return await completion.Task;
            }
            finally
            {
                await Cysharp.Threading.Tasks.UniTask.Yield(); // avoid unsubscribing in signal handler

                foreach (var (type, subscription) in subscriptions)
                {
                    SignalBus.Unsubscribe(type, subscription);
                }
            }
        }


        private Action<object> SignalSubscription(UniTaskCompletionSource<object> completion) =>
            signal => completion.TrySetResult(signal);
    }
}
