using System;
using UnityEngine;
using Zenject;

namespace ZenjectSignalHelpers
{
    public class ZenjectSignalHelpersInstaller : ZenjectSignalHelpersInstallerBase<AutomaticSignalHandlers> { }

    /// <summary>
    /// A MonoInstaller that can be used for example in the ProjectContext to enable automatic full project
    /// wide support for signal helper functionality. It installs:
    ///  * SignalBus
    ///  * All ISignals found in all assemblies
    ///  * AutomaticSignalHandlers injection
    /// 
    /// This installer is not required, all functions below can also be called manually somewhere to install
    /// signals or [SignalHandler] support.
    /// </summary>
    public class ZenjectSignalHelpersInstallerBase<T> : MonoInstaller where T : IAutomaticSignalHandler, new()
    {
        [Tooltip("Should a SignalBus be installed")] [SerializeField]
        private bool InstallSignalBus = true;

        [Tooltip("Should AutomaticSignalHandlers be installed")] [SerializeField]
        private bool InstallAutomaticSignalHandlers = true;

        [Tooltip("Should all signals (ISignal) be declared")] [SerializeField]
        private bool DeclareAllSignals = true;


        [Tooltip("Install a logger that logs all signals on the signal bus")] [SerializeField]
        private bool InstallSignalLogger = true;

        public override void InstallBindings()
        {
            if (InstallSignalBus)
                SignalBusInstaller.Install(Container);

            if (InstallAutomaticSignalHandlers)
            {
                if (!Container.HasBinding<SignalBus>())
                    throw new Exception("AutomaticSignalHandlers require SignalBus to be installed");

                AutomaticSignalHandlers.Install<T>(Container);
            }

            if (DeclareAllSignals)
                AutomaticSignalInstaller.InstallAllSignals(Container);

            if (InstallSignalLogger)
            {
                if (!Container.HasBinding<SignalBus>())
                    throw new Exception("ZenjectSignalLogger requires SignalBus to be installed");

                ZenjectSignalLogger.Install(Container);
            }
        }
    }
}
