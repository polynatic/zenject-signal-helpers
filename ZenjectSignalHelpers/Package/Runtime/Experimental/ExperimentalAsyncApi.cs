using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ZenjectSignalHelpers.Experimental
{
    /// <summary>
    /// Experimental Api
    /// </summary>
    public class Signals
    {
        public static async Task<TSignal> WaitFor<TSignal>()
            where TSignal : ISignal
        {
            await new Task(() => { });
            return default;
        }

        public static async Task Fire<TSignal>() => await new Task(() => { });

        public static async Task Fire<TSignal>(TSignal signal) => await new Task(() => { });


        public static async Task WaitSwitch<F1, F2, F3>(Func<F1, Task> f1, Func<F2, Task> f2, Func<F3, Task> f3) =>
            await new Task(() => { });

        public static async Task WaitSwitch<F1, F2>(Func<F1, Task> f1, Func<F2, Task> f2) => await new Task(() => { });

        public static async Task<object> WaitAny<F1, F2, F3>() => await new Task<object>(() => default);

        public static async Task<object> WaitAny<F1, F2>() => await new Task<object>(() => default);
    }
}