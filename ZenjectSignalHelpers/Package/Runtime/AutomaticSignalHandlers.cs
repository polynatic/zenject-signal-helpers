using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;
using ZenjectSignalHelpers.Utils;

namespace ZenjectSignalHelpers
{
    /// <summary>
    /// Attribute marks a function as signal handler to be subscribed by AutomaticSignalHandlers.
    ///
    /// Example:
    ///   [SignalHandler]
    ///   void OnSignal(DoSomething signal) { }
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    [MeansImplicitUse(ImplicitUseKindFlags.Access)]
    public class SignalHandlerAttribute : Attribute { }

    /// <summary>
    /// An attribute that can be placed on AutomaticSignalHandlers fields. It prevents automatic handler subscription
    /// at initialization time. Call SubscribeAll() on the AutomaticSignalHandlers field to manually subscribe later.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class SubscribeManuallyAttribute : Attribute { }

    public interface IAutomaticSignalHandler
    {
        internal void Construct(
            SignalBus signalBus,
            object subscriber,
            bool subscribeManually = false);
    }

    /// <summary>
    /// Automatically subscribes all methods of a class marked with [SignalHandler] to a SignalBus and unsubscribes them
    /// on destruction.
    ///
    /// Usage: Create AutomaticSignalHandlers field on any object in the Zenject context and use the [Inject] attribute to
    /// initialize it.
    ///   [Inject] private readonly AutomaticSignalHandlers Signals;
    ///     or
    ///   [Inject] [ManualSubscribe] private readonly AutomaticSignalHandlers Signals; // and call Signals.SubscribeAll() later
    /// </summary>
    [MeansImplicitUse(ImplicitUseKindFlags.Access)]
    public class AutomaticSignalHandlers : IAutomaticSignalHandler
    {
        public SignalBus SignalBus { get; private set; }

        /// <summary>
        /// Actions to subscribe every [SignalHandler].
        /// </summary>
        private List<Action> SubscribeActions;

        /// <summary>
        /// Actions to unsubscribe every [SignalHandler].
        /// </summary>
        private List<Action> UnsubscribeActions;

        /// <summary>
        /// Are all [SignalHandlers] currently subscribed?
        /// </summary>
        public bool AreSubscribed { get; private set; }

        /// <summary>
        /// Install AutomaticSignalHandlers in the given DiContainer, so [Inject] can be used on
        /// AutomaticSignalHandlers fields. To be used within a Zenject dependency installer or
        /// using the ZenjectSignalHelpersInstaller.
        /// </summary>
        public static void Install<T>(DiContainer container) where T : IAutomaticSignalHandler, new()
        {
            if (typeof(T) != typeof(AutomaticSignalHandlers))
            {
                container
                    .BindInterfacesAndSelfTo<T>()
                    .FromMethod(
                        context =>
                        {
                            var hasManualSubscriptionAttribute = context.ObjectInstance
                                                                        .GetType()
                                                                        .GetField(
                                                                            context.MemberName,
                                                                            BindingFlags.NonPublic
                                                                            | BindingFlags.Instance
                                                                        )
                                                                        ?.GetCustomAttribute<SubscribeManuallyAttribute
                                                                        >()
                                                                 != null;

                            var signals = new T();
                            signals.Construct(
                                context.Container.Resolve<SignalBus>(),
                                context.ObjectInstance,
                                hasManualSubscriptionAttribute
                            );
                            return signals;
                        }
                    )
                    .AsTransient();
            }

            container
                .BindInterfacesAndSelfTo<AutomaticSignalHandlers>()
                .FromMethod(
                    context =>
                    {
                        var hasManualSubscriptionAttribute = context.ObjectInstance
                                                                    .GetType()
                                                                    .GetField(
                                                                        context.MemberName,
                                                                        BindingFlags.NonPublic | BindingFlags.Instance
                                                                    )
                                                                    ?.GetCustomAttribute<SubscribeManuallyAttribute>()
                                                             != null;

                        var signals = new AutomaticSignalHandlers();
                        signals.Construct(
                            context.Container.Resolve<SignalBus>(),
                            context.ObjectInstance,
                            hasManualSubscriptionAttribute
                        );
                        return signals;
                    }
                )
                .AsTransient();
        }


        /// <summary>
        /// Subscribe all [SignalHandler]s on the signal bus.
        /// </summary>
        public void SubscribeAll()
        {
            if (AreSubscribed)
                return;

            AreSubscribed = true;
            SubscribeActions?.ForEach(subscribe => subscribe());
        }

        /// <summary>
        /// Unsubscribe all [SignalHandler]s from the signal bus.
        /// </summary>
        public void UnsubscribeAll()
        {
            if (!AreSubscribed)
                return;

            AreSubscribed = false;
            UnsubscribeActions?.ForEach(unsubscribe => unsubscribe());
        }

        /// <summary>
        /// Fire a signal on the signal bus.
        /// </summary>
        public void Fire<TSignal>() where TSignal : struct => SignalBus.AbstractFire<TSignal>();

        /// <summary>
        /// Fire a signal on the signal bus.
        /// </summary>
        public void Fire<TSignal>(TSignal signal) where TSignal : struct => SignalBus.AbstractFire(signal);


        protected AutomaticSignalHandlers(
            SignalBus signalBus,
            object subscriber,
            bool subscribeManually = false) =>
            Construct(signalBus, subscriber, subscribeManually);

        void IAutomaticSignalHandler.Construct(
            SignalBus signalBus,
            object subscriber,
            bool subscribeManually) =>
            Construct(signalBus, subscriber, subscribeManually);

        private void Construct(SignalBus signalBus, object subscriber, bool subscribeManually)
        {
            SignalBus = signalBus;

            CreateActionsForAllHandlers(subscriber);

            if (!subscribeManually)
                SubscribeAll();
        }

        public AutomaticSignalHandlers() { }

        ~AutomaticSignalHandlers() => UnsubscribeAll();


        /// <summary>
        /// Creates Subscribe and Unsubscribe actions for the given signal handler.
        /// </summary>
        /// <param name="target">Target instance on which the handler will be called.</param>
        private Action<MethodInfo> CreateActions<TTarget>(TTarget target) where TTarget : class =>
            handler =>
            {
                var signalType = handler.GetParameters()[0].ParameterType;
                var handlerDelegate = BindDelegate(target, handler); // will be passed to subscribe/unsubscribe
                var callSubscribe = CallForSignalBus(SignalBusSubscribeMethod, handlerDelegate, signalType);
                var callUnsubscribe = CallForSignalBus(SignalBusUnsubscribeMethod, handlerDelegate, signalType);

                SubscribeActions.Add(callSubscribe);
                UnsubscribeActions.Add(callUnsubscribe);
            };

        /// <summary>
        /// Create actions for all handlers to call subscribe or unsubscribe on the signal bus.
        /// </summary>
        private void CreateActionsForAllHandlers(object subscriber)
        {
            SubscribeActions = new List<Action>();
            UnsubscribeActions = new List<Action>();

            AllSignalHandlersOf(subscriber.GetType())
                .ForEach(CreateActions(subscriber));
        }

        /// <summary>
        /// Creates an action that calls a given method on the SignalBus and passes a delegate parameter to it.
        /// </summary>
        /// <param name="signalBusMethod">The method to call on SignalBus.</param>
        /// <param name="parameter">The delegate that is passed to the call as parameter.</param>
        /// <param name="type">The generic type of the SignalBus function to call.</param>
        private Action CallForSignalBus(MethodInfo signalBusMethod, Delegate parameter, Type type)
        {
            var genericMethod = signalBusMethod.MakeGenericMethod(type);
            var genericDelegate = BindDelegate(SignalBus, genericMethod);
            var callSubscribe = GenericDelegateCall(type, genericDelegate, parameter);
            return callSubscribe;
        }

        /// <summary>
        /// Creates a simple Action that calls a delegate and passes a delegate as parameter of a given type.
        /// </summary>
        private static Action GenericDelegateCall(Type type, Delegate call, Delegate parameter)
        {
            var genericMethod = CallMethodInfo.MakeGenericMethod(type);
            var genericDelegate = (Action<Delegate, Delegate>) genericMethod.CreateDelegate(
                typeof(Action<Delegate, Delegate>),
                null
            );
            return () => genericDelegate(call, parameter);
        }

        /// <summary>
        /// Call a delegate of a generic type and pass a delegate with generic type.
        /// </summary>
        private static void DelegateCall<T>(Delegate callDelegate, Delegate parameterDelegate)
        {
            var call = (Action<Action<T>>) callDelegate;
            var parameter = (Action<T>) parameterDelegate;
            call(parameter);
        }

        /// <summary>
        /// Get a delegate for a handler on a target instance.
        /// </summary>
        private static Delegate BindDelegate(object target, MethodInfo handlerMethod)
        {
            var parameterTypes = handlerMethod.GetParameters().Select(parameter => parameter.ParameterType);
            var delegateType = Expression.GetActionType(parameterTypes.ToArray());
            return handlerMethod.CreateDelegate(delegateType, target);
        }

        /// <summary>
        /// Get all signal handlers of a subscriber.
        /// </summary>
        private static IEnumerable<MethodInfo> AllSignalHandlersOf(Type subscriberType) =>
            from potentialHandler in subscriberType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
            where IsSignalHandlerWithWarning(potentialHandler, subscriberType)
            select potentialHandler;


        /// <summary>
        /// Checks if the given method can be used as a signal handler and logs some warnings.
        /// </summary>
        private static bool IsSignalHandlerWithWarning(MethodInfo method, Type subscriberType)
        {
            if (method.GetCustomAttribute<SignalHandlerAttribute>() == null)
                return false;


            if (method.GetParameters().Length != 1)
            {
                Debug.LogError($"{subscriberType}.{method.Name} must only have one parameter: the signal type");
                return false;
            }

            var parameterType = method.GetParameters()[0].ParameterType;

            if (method.ReturnType != typeof(void))
            {
                Debug.LogError($"{subscriberType}.{method.Name}({parameterType.Name}) return type must be void");
                return false;
            }

            return true;
        }

        private static MethodInfo CallMethodInfo { get; } = typeof(AutomaticSignalHandlers)
                                                            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                                                            .Single(method => method.Name == nameof(DelegateCall));

        private static MethodInfo SignalBusSubscribeMethod { get; } = typeof(SignalBus)
                                                                      .GetMethods()
                                                                      .Single(
                                                                          method => method.Name
                                                                              == nameof(SignalBus.Subscribe)
                                                                              && method.IsGenericMethod
                                                                              && method.GetParameters().Length == 1
                                                                              && method.GetParameters()[0].ParameterType
                                                                              != typeof(Action) // not the non-generic
                                                                      );

        private static MethodInfo SignalBusUnsubscribeMethod { get; } = typeof(SignalBus)
                                                                        .GetMethods()
                                                                        .Single(
                                                                            method => method.Name
                                                                                == nameof(SignalBus.Unsubscribe)
                                                                                && method.IsGenericMethod
                                                                                && method.GetParameters().Length == 1
                                                                                && method.GetParameters()[0]
                                                                                    .ParameterType
                                                                                != typeof(Action) // not the non-generic
                                                                        );
    }
}
