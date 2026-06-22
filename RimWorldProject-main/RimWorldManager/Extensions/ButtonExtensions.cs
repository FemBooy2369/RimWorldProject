using Microsoft.Maui.Controls;

namespace RimworldModManager.Extensions;

public static class ButtonExtensions
{
    public static async Task PlayNeonClickAnimation(this Button button)
    {
        if (button == null) return;

        // Неоновый "press" эффект
        await button.ScaleTo(0.92, 80, Easing.CubicOut);
        await button.ScaleTo(1.0, 120, Easing.CubicIn);

        // Лёгкий glow (через opacity + scale)
        await button.FadeTo(0.85, 60);
        await button.FadeTo(1.0, 80);
    }
}