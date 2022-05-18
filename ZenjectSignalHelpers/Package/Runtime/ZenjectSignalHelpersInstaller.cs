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
        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);
            AutomaticSignalHandlers.Install(Container);
            AutomaticSignalInstaller.InstallAllSignals(Container);
        }
    }
}