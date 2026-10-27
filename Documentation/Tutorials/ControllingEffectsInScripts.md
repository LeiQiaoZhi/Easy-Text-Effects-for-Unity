# Tutorial: Controlling Effects in Scripts

## Modifying Parameters

There are situations where you need to change effect parameters in scripts -- for example, changing the color of an effect based on user input.


Thanks to @nnra6864's contribution, you can now easily control effect parameters in scripts:
1. set the parameter value directly in the script
2. call `HandleValueChanged()` to apply the change immediately
- note: you don't need references to any `TextEffect` component, just the effect itself. The effect will automatically find the `TextEffect` components that use it.

```csharp
public Effect_Move moveEffect;

public void SetOffset(float _offset)
{
    moveEffect.endOffset.y = _offset;
    moveEffect.HandleValueChanged();
}
```

This gif shows an example of using a slider to control the offset of a move effect. But you can apply the same logic to any effect parameter.

<img src="../Images/parameter.gif" alt="" width="400">

## Stopping An Effect

Given a Text Effect component, you can call `StopAllEffects()`, `StopManualEffects()`, `StopManualEffect(string effectName)`, or `StopOnStartEffects()`. `StopManualEffect` finds the manual effect by name the same way `StartManualEffect(string)` does, and logs a warning when that name is missing. 

"Stop" here means immediately return to the neutral state of the text.

<img src="../Images/stop.gif" alt="" width="400">


