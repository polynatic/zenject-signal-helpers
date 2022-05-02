namespace ZenjectSignalHelpers.Experimental
{
    struct DoShow : ICommandSignal
    {
        public static implicit operator DoShow(string name) => new DoShow();
    }

    struct DoClose : ICommandSignal
    {
        public static implicit operator DoClose(string name) => new DoClose();
    }

    struct DoUseItem : ICommandSignal
    {
        public static implicit operator DoUseItem(string name) => new DoUseItem();
    }

    struct OnDone : IEventSignal { }

    struct OnPressUseItem : IEventSignal
    {
        public string Name;
    }

    struct OnItemUsed : IEventSignal
    {
        public string Name;
        public static implicit operator OnItemUsed(string name) => new OnItemUsed {Name = name};
    }

    struct OnPressComplicatedThing : IEventSignal { }

    struct OnPressCancel : IEventSignal { }

    struct OnFail : IEventSignal { }

    struct OnAnimationsFinished : IEventSignal { }
}