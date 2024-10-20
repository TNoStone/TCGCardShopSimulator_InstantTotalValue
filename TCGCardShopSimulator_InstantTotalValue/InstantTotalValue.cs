using BepInEx;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using BepInEx.Logging;
using BepInEx.Configuration;

namespace InstantTotalValue
{
    [BepInPlugin("com.TNoStone.InstantTotalValue", "Instant Total Value", "1.0.2")]
    public class InstantTotalValue : BaseUnityPlugin
    {
        public static ManualLogSource InstanceLogger;
        public static FieldInfo TimerFieldInfo;

        public static ConfigEntry<bool> ModEnabled;

        private void Awake()
        {
            InstanceLogger = Logger;

            ModEnabled = Config.Bind("General", "ModEnabled", true, "Enable or disable the Instant Total Value mod.");

            var harmony = new Harmony("com.TNoStone.InstantTotalValue");
            harmony.PatchAll();

            var type = typeof(CardOpeningSequenceUI);
            TimerFieldInfo = type.GetField("m_TotalValueLerpTimer", BindingFlags.NonPublic | BindingFlags.Instance);

            InstanceLogger.LogInfo("Instant Total Value Mod Loaded by TNoStone");
        }
    }

    [HarmonyPatch(typeof(CardOpeningSequenceUI), "Update")]
    public static class CardOpeningSequenceUI_Update_Patch
    {
        [HarmonyPrefix]
        public static void Prefix(CardOpeningSequenceUI __instance)
        {
            if (!InstantTotalValue.ModEnabled.Value)
                return;

            FieldInfo timerField = typeof(CardOpeningSequenceUI).GetField("m_TotalValueLerpTimer", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo isShowingTotalValueField = typeof(CardOpeningSequenceUI).GetField("m_IsShowingTotalValue", BindingFlags.NonPublic | BindingFlags.Instance);

            if ((bool)isShowingTotalValueField.GetValue(__instance))
            {
                float lerpTimer = (float)timerField.GetValue(__instance);

                lerpTimer += Time.deltaTime * 1.95f;

                timerField.SetValue(__instance, lerpTimer);

                if (lerpTimer >= 1f)
                {
                    isShowingTotalValueField.SetValue(__instance, false);
                    SoundManager.SetEnableSound_ExpIncrease(isEnable: false);
                }
            }

            if ((bool)__instance.GetType().GetField("m_IsShowingTotalValue", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance))
            {
                SoundManager.SetEnableSound_ExpIncrease(isEnable: true);
            }
        }
    }

    [HarmonyPatch(typeof(CardOpeningSequence), "Update")]
    public static class CardOpeningSequence_State9_Patch
    {
        [HarmonyPrefix]
        public static void Prefix(CardOpeningSequence __instance)
        {
            if (!InstantTotalValue.ModEnabled.Value)
                return;

            if (__instance.m_StateIndex == 9)
            {
                __instance.m_Slider += Time.deltaTime;
                if (__instance.m_Slider >= 0.7f)
                {
                    __instance.m_Slider = 0f;
                    __instance.m_StateIndex++;
                }
            }
        }
    }
}
