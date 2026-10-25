using System;
using HarmonyLib;
using UnityEngine;

namespace com.github.lhervier.ksp.fix.constructionbuttons
{
    /// <summary>
    /// Installs the fixes the settings turn on, once, when KSP starts: from then on, the snap and coordinate space
    /// buttons of the EVA construction mode show the mode actually in use.
    /// </summary>
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class KSPFixConstructionButtons : MonoBehaviour
    {
        private const string HarmonyId = "com.github.lhervier.ksp.fix.constructionbuttons";

        private void Start()
        {
            Settings.Load();
            Log.Info($"Version {typeof(KSPFixConstructionButtons).Assembly.GetName().Version}, " +
                $"snapIcon {Settings.SnapIcon}, coordSpaceLabel {Settings.CoordSpaceLabel}, log level {Log.Level}");
            Harmony harmony = new Harmony(HarmonyId);
            if (Settings.SnapIcon)
            {
                try
                {
                    SnapIcon.Install(harmony);
                    Log.Info("Snap icon fix installed");
                }
                catch (Exception e)
                {
                    Log.Error($"Could not install the snap icon fix, the icon shows \"fine\" by default, as on stock: {e}");
                }
            }
            if (Settings.CoordSpaceLabel)
            {
                try
                {
                    CoordSpaceLabel.Install(harmony);
                    Log.Info("Coordinate space label fix installed");
                }
                catch (Exception e)
                {
                    Log.Error("Could not install the coordinate space label fix, the button reads \"Absolute\" by " +
                        $"default, as on stock: {e}");
                }
            }
        }
    }
}
