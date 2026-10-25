using System;
using System.Reflection;
using EditorGizmos;
using HarmonyLib;
using KSP.Localization;
using TMPro;
using UnityEngine;

namespace com.github.lhervier.ksp.fix.constructionbuttons
{
    /// <summary>
    /// Makes the coordinate space button of the EVA construction mode (Local or Absolute moves and rotations) show,
    /// translated, the space the gizmo actually uses. On stock, it reads "Absolute", in English in every language,
    /// until the first click, whatever the space: the move gizmo starts in Absolute, the rotate gizmo in Local.
    /// </summary>
    internal static class CoordSpaceLabel
    {
        private static readonly MethodInfo ToggleRotateGizmoMethod =
            AccessTools.Method(typeof(EVAConstructionModeEditor), "ToggleRotateGizmo", new[] { typeof(bool) });
        private static readonly MethodInfo ToggleOffsetGizmoMethod =
            AccessTools.Method(typeof(EVAConstructionModeEditor), "ToggleOffsetGizmo", new[] { typeof(bool) });
        // The spaces of the gizmos are static: shared by every gizmo of a kind, kept from one part to the next.
        private static readonly FieldInfo RotateCoordSpaceField = AccessTools.Field(typeof(GizmoRotate), "coordSpace");
        private static readonly FieldInfo OffsetCoordSpaceField = AccessTools.Field(typeof(GizmoOffset), "coordSpace");
        private static readonly AccessTools.FieldRef<EVAConstructionModeEditor, TextMeshProUGUI> CoordSpaceText =
            AccessTools.FieldRefAccess<EVAConstructionModeEditor, TextMeshProUGUI>("coordSpaceText");

        /// <summary>
        /// Patches the activation of the rotate and move gizmos of the EVA construction mode, so that the coordinate
        /// space button reads the space of the gizmo shown.
        /// </summary>
        /// <exception cref="MissingMemberException">A stock member the fix needs does not exist.</exception>
        public static void Install(Harmony harmony)
        {
            if (ToggleRotateGizmoMethod == null)
            {
                throw new MissingMemberException("EVAConstructionModeEditor.ToggleRotateGizmo");
            }
            if (ToggleOffsetGizmoMethod == null)
            {
                throw new MissingMemberException("EVAConstructionModeEditor.ToggleOffsetGizmo");
            }
            if (RotateCoordSpaceField == null || !RotateCoordSpaceField.IsStatic)
            {
                throw new MissingMemberException("static GizmoRotate.coordSpace");
            }
            if (OffsetCoordSpaceField == null || !OffsetCoordSpaceField.IsStatic)
            {
                throw new MissingMemberException("static GizmoOffset.coordSpace");
            }
            harmony.Patch(ToggleRotateGizmoMethod,
                postfix: new HarmonyMethod(typeof(CoordSpaceLabel), nameof(ToggleRotateGizmoPostfix)));
            harmony.Patch(ToggleOffsetGizmoMethod,
                postfix: new HarmonyMethod(typeof(CoordSpaceLabel), nameof(ToggleOffsetGizmoPostfix)));
        }

        /// <summary>When the rotate gizmo is asked for, shows the space of the rotate gizmos.</summary>
        private static void ToggleRotateGizmoPostfix(EVAConstructionModeEditor __instance, bool activate)
        {
            if (activate)
            {
                Show(CoordSpaceText(__instance), (Space)RotateCoordSpaceField.GetValue(null), "rotate");
            }
        }

        /// <summary>When the move gizmo is asked for, shows the space of the move gizmos.</summary>
        private static void ToggleOffsetGizmoPostfix(EVAConstructionModeEditor __instance, bool activate)
        {
            if (activate)
            {
                Show(CoordSpaceText(__instance), (Space)OffsetCoordSpaceField.GetValue(null), "move");
            }
        }

        /// <summary>Writes on <paramref name="text"/> the translated name of <paramref name="space"/>.</summary>
        private static void Show(TextMeshProUGUI text, Space space, string gizmo)
        {
            if (text == null)
            {
                return;
            }
            // Same strings as stock's EVAConstructionModeEditor.ChangeCoordSpace: Absolute for World, Local for Self.
            text.text = Localizer.Format(space == Space.World ? "#autoLOC_6001217" : "#autoLOC_6001218");
            Log.Debug($"Coordinate space button set to {space} ({gizmo} gizmo)");
        }
    }
}
