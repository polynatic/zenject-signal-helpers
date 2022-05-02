namespace ZenjectSignalHelpers
{
    /// <summary>
    /// General interface for all signals.
    /// </summary>
    public interface ISignal { }

    /// <summary>
    /// Interface for signals that are intended as commands and should only be processed by exactly one consumer.
    /// </summary>
    public interface ICommandSignal : ISignal { }

    /// <summary>
    /// Interface for signals that are intended as events and can be processed by an arbitrary number of consumers.
    /// </summary>
    public interface IEventSignal : ISignal { }
}