using System;
using System.Reflection;
using HarmonyLib;
using KSP.UI;

namespace com.github.lhervier.ksp.fix.constructionbuttons
{
    /// <summary>
    /// Makes the snap button of the EVA construction mode (fine or snapped moves and rotations) show the step actually
    /// in use. On stock, its icon shows "fine" when the mode opens, while the parts move by steps, and the first click
    /// switches to fine moves without changing the icon.
    /// </summary>
    internal static class SnapIcon
    {
        private static readonly MethodInfo OnConstructionModeMethod =
            AccessTools.Method(typeof(EVAConstructionModeEditor), "OnConstructionMode", new[] { typeof(bool) });
        private static readonly MethodInfo OnGameUnPauseMethod =
            AccessTools.Method(typeof(UIStateImage), "onGameUnPause");
        private static readonly AccessTools.FieldRef<EVAConstructionModeEditor, UIStateImage> AngleSnapSprite =
            AccessTools.FieldRefAccess<EVAConstructionModeEditor, UIStateImage>("angleSnapSprite");

        /// <summary>
        /// Patches the opening of the EVA construction mode and the end of a pause, so that the snap icon follows
        /// the snap setting.
        /// </summary>
        /// <exception cref="MissingMemberException">A stock member the fix needs does not exist.</exception>
        public static void Install(Harmony harmony)
        {
            if (OnConstructionModeMethod == null)
            {
                throw new MissingMemberException("EVAConstructionModeEditor.OnConstructionMode");
            }
            if (OnGameUnPauseMethod == null)
            {
                throw new MissingMemberException("UIStateImage.onGameUnPause");
            }
            harmony.Patch(OnConstructionModeMethod,
                postfix: new HarmonyMethod(typeof(SnapIcon), nameof(OnConstructionModePostfix)));
            harmony.Patch(OnGameUnPauseMethod,
                postfix: new HarmonyMethod(typeof(SnapIcon), nameof(OnGameUnPausePostfix)));
        }

        /// <summary>When the EVA construction mode opens, shows the icon of the snap setting.</summary>
        private static void OnConstructionModePostfix(EVAConstructionModeEditor __instance, bool opened)
        {
            // Stock never sets the icon from the setting here (the VAB / SPH editor does, in EditorLogic.Start): it
            // keeps the start state of its prefab, the "fine" icon.
            if (opened)
            {
                Show(AngleSnapSprite(__instance), "construction mode opened");
            }
        }

        /// <summary>After a pause, shows again the icon of the snap setting, if the image is the snap button's.</summary>
        private static void OnGameUnPausePostfix(UIStateImage __instance)
        {
            // Every UIStateImage takes the state of the time warp rate index at the end of a pause (the images of
            // the time warp buttons): out of time warp, index 0, the "fine" icon of the snap button.
            EVAConstructionModeEditor editor = EVAConstructionModeController.Instance?.evaEditor;
            if (editor != null && AngleSnapSprite(editor) == __instance)
            {
                Show(__instance, "game unpaused");
            }
        }

        /// <summary>Shows on <paramref name="sprite"/> the icon of the snap setting.</summary>
        private static void Show(UIStateImage sprite, string when)
        {
            if (sprite == null)
            {
                return;
            }
            // Same states as stock's EVAConstructionModeEditor.SnapButton: 1 for snapped, 0 for fine.
            sprite.SetState(GameSettings.VAB_USE_ANGLE_SNAP ? 1 : 0);
            Log.Debug($"Snap icon set to {(GameSettings.VAB_USE_ANGLE_SNAP ? "snapped" : "fine")} ({when})");
        }
    }
}
