using System;
using System.Threading.Tasks;
using UnityEngine;

namespace ZenjectSignalHelpers.Experimental
{
    public class ExampleUsage
    {
        async Task ExampleWaitForSwitchPatternMatching()
        {
            var character = "Elliot";

            try
            {
                await Signals.Fire<DoShow>("Inventory");
                await Signals.WaitFor<OnAnimationsFinished>();

                switch (await Signals.WaitAny<OnPressUseItem, OnPressCancel, OnFail>())
                {
                    case OnPressUseItem onPressUseItem:
                    {
                        Debug.Log("Pressed Use Item");
                        var item = onPressUseItem.Name;
                        await UseItemOnCharacter(character, item);
                        break;
                    }
                    case OnPressCancel _:
                    {
                        Debug.Log("Pressed Cancel");
                        break;
                    }
                    case OnFail _: throw new Exception("Something failed"); // not caught by try, so will bubble
                }
            }
            finally
            {
                await Signals.Fire<DoClose>("Inventory");
                await Signals.WaitFor<OnAnimationsFinished>();
                await Signals.Fire<OnDone>();
            }
        }

        async Task ExampleWaitForSwitchLambda()
        {
            var character = "Elliot";

            try
            {
                await Signals.Fire<DoShow>("Inventory");
                await Signals.WaitFor<OnAnimationsFinished>();

                await Signals.WaitSwitch(
                    async (OnPressUseItem onPressUseItem) =>
                    {
                        Debug.Log($"Pressed Use Item");
                        var item = onPressUseItem.Name;
                        await UseItemOnCharacter(character, item);
                    },
                    async (OnPressCancel _) => { Debug.Log($"Pressed Cancel"); },
                    async (OnFail _) => throw new Exception("Something failed") // not caught by try, so will bubble
                );
            }
            finally
            {
                await Signals.Fire<DoClose>("Inventory");
                await Signals.WaitFor<OnAnimationsFinished>();
                await Signals.Fire<OnDone>();
            }
        }

        async Task UseItemOnCharacter(string character, string item)
        {
            Debug.Log($"Using item {item} on {character}");

            await Signals.Fire<DoUseItem>(item);
            await Task.Delay(5); // example item using task taking some time
            await Signals.Fire<OnItemUsed>(item);
        }
    }
}