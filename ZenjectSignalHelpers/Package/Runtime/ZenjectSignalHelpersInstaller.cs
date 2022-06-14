using UnityEngine;
using Zenject;

namespace ZenjectSignalHelpers
{
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
    public class ZenjectSignalHelpersInstaller : MonoInstaller
    {
        [Tooltip("Should a SignalBus be installed")] [SerializeField]
        private bool InstallSignalBus = true;

        [Tooltip("Should AutomaticSignalHandlers be installed")] [SerializeField]
        private bool InstallAutomaticSignalHandlers = true;

        [Tooltip("Should all signals (ISignal) be declared")] [SerializeField]
        private bool DeclareAllSignals = true;

        public override void InstallBindings()
        {
            if (InstallSignalBus)
                SignalBusInstaller.Install(Container);

            if (InstallAutomaticSignalHandlers)
                AutomaticSignalHandlers.Install(Container);

            if (DeclareAllSignals)
                AutomaticSignalInstaller.InstallAllSignals(Container);
        }
    }
}