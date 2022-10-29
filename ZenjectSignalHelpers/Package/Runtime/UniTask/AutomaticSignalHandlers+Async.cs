#if UNITY_EDITOR
using UnityEditor;
#endif
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

#if UNITY_EDITOR
            PlayModePendingTasks.Add(completion);
#endif

            foreach (var type in types)
            {
                var subscription = SignalSubscription(completion);
                SignalBus.Subscribe(type, subscription);
                subscriptions.Add((type, subscription));
            }

            try
            {
                var result = await completion.Task;

#if UNITY_EDITOR
                PlayModePendingTasks.Remove(completion);
#endif
                return result;
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

#if UNITY_EDITOR
    [InitializeOnLoad]
    public static class PlayModePendingTasks
    {
        private static List<UniTaskCompletionSource<object>> ActiveTasks = new List<UniTaskCompletionSource<object>>();

        public static void Add(UniTaskCompletionSource<object> completion) => ActiveTasks.Add(completion);
        public static void Remove(UniTaskCompletionSource<object> completion) => ActiveTasks.Remove(completion);

        static PlayModePendingTasks() => EditorApplication.playModeStateChanged += CleanPendingTasks;

        private static void CleanPendingTasks(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingPlayMode)
                return;

            foreach (var completion in ActiveTasks)
            {
                completion.TrySetCanceled();
            }

            ActiveTasks.Clear();
        }
    }
#endif
}
