using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Harmony;
using RimWorld;
using RimWorld.Planet;
using Verse;
using UnityEngine;
using System.Reflection;
using System.Reflection.Emit;

namespace SimpleSearchBar
{
    [StaticConstructorOnStartup]
    public class HarmonyPatches
    {
        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create(id: "rimworld.gguake.simplesearchbar.main");

            harmony.Patch(original: AccessTools.Method(type: typeof(Listing_TreeThingFilter), name: "Visible", parameters: new[] { typeof(ThingDef) }),
                prefix: null, postfix: new HarmonyMethod(type: typeof(HarmonyPatches), name: nameof(VisiblePostfix)));

            harmony.Patch(original: AccessTools.Method(type: typeof(ThingFilter), name: "SetAllow", parameters: new[] { typeof(ThingDef), typeof(bool) }),
                prefix: new HarmonyMethod(type: typeof(HarmonyPatches), name: nameof(SetAllowPrefix)));

            harmony.Patch(original: AccessTools.Constructor(type: typeof(Dialog_BillConfig), parameters: new[] { typeof(Bill_Production), typeof(IntVec3) }),
                postfix: new HarmonyMethod(type: typeof(HarmonyPatches), name: nameof(Dialog_BillConfigPostfix)));

            harmony.Patch(original: AccessTools.Method(type: typeof(InspectPaneUtility), name: "ToggleTab"),
                postfix: new HarmonyMethod(type: typeof(HarmonyPatches), name: nameof(ToggleTabPostfix)));

            harmony.Patch(original: AccessTools.Method(type: typeof(TransferableOneWayWidget), name: "FillMainRect"),
                prefix: new HarmonyMethod(type: typeof(HarmonyPatches), name: nameof(DrawSearchBarTransfer)));

            harmony.Patch(original: AccessTools.Method(type: typeof(ThingFilterUI), name: "DoThingFilterConfigWindow"),
                transpiler: new HarmonyMethod(type: typeof(HarmonyPatches), name: nameof(DoThingFilterConfigWindowTranspiler)));
            
            harmony.Patch(original: AccessTools.Method(type: typeof(TransferableOneWayWidget), name: "FillMainRect"),
                transpiler: new HarmonyMethod(type: typeof(HarmonyPatches), name: nameof(FillMainRectTranspiler)));

            harmony.Patch(original: AccessTools.Method(type: typeof(Dialog_LoadTransporters), name: "PostOpen"),
                postfix: new HarmonyMethod(type: typeof(HarmonyPatches), name: nameof(ResetKeyword)));

            harmony.Patch(original: AccessTools.Method(type: typeof(Dialog_FormCaravan), name: "PostOpen"),
                postfix: new HarmonyMethod(type: typeof(HarmonyPatches), name: nameof(ResetKeyword)));
            
            harmony.Patch(original: AccessTools.Method(type: typeof(Dialog_FormCaravan), name: "<DoWindowContents>m__0"),
                postfix: new HarmonyMethod(type: typeof(HarmonyPatches), name: nameof(ResetKeyword)));

            harmony.Patch(original: AccessTools.Method(type: typeof(Dialog_FormCaravan), name: "<DoWindowContents>m__1"),
                postfix: new HarmonyMethod(type: typeof(HarmonyPatches), name: nameof(ResetKeyword)));

            harmony.Patch(original: AccessTools.Method(type: typeof(Dialog_LoadTransporters), name: "<DoWindowContents>m__0"),
                postfix: new HarmonyMethod(type: typeof(HarmonyPatches), name: nameof(ResetKeyword)));

            harmony.Patch(original: AccessTools.Method(type: typeof(Dialog_LoadTransporters), name: "<DoWindowContents>m__1"),
                postfix: new HarmonyMethod(type: typeof(HarmonyPatches), name: nameof(ResetKeyword)));
        }

        public static void DrawSearchBarFilterConfig(Rect rect, ThingFilter filter, ThingFilter parentFilter)
        {
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleLeft;

            Rect rtText = new Rect(rect.x + 1f, rect.yMax + 1f, rect.width - 1f, 27f);
            SearchUtility.Keyword = Widgets.TextField(rtText, SearchUtility.Keyword);

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
        }
        
        public static bool DrawSearchBarTransfer(Rect mainRect)
        {
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleLeft;

            Rect rtText = new Rect(mainRect.width - 195f, 0f, 180f, 27f);
            SearchUtility.Keyword = Widgets.TextField(rtText, SearchUtility.Keyword);

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;

            return true;
        }

        public static void VisiblePostfix(ref bool __result, ThingDef td)
        {
            if (__result)
            {
                if (!string.IsNullOrEmpty(SearchUtility.Keyword) && !SearchUtility.CheckVisible(td))
                {
                    __result = false;
                }
            }
        }

        public static bool SetAllowPrefix(ThingFilter __instance, ThingDef thingDef)
        {
            return SearchUtility.CheckVisible(thingDef);
        }

        public static void Dialog_BillConfigPostfix()
        {
            SearchUtility.Reset();
        }

        public static void ToggleTabPostfix()
        {
            SearchUtility.Reset();
        }

        public static void ResetKeyword()
        {
            SearchUtility.Reset();
        }

        public static IEnumerable<CodeInstruction> DoThingFilterConfigWindowTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            bool patched = false;
            List<CodeInstruction> instructionList = instructions.ToList();
            for (int i = 0; i < instructionList.Count; ++i)
            {
                CodeInstruction instruction = instructionList[i];
                if (instruction.opcode == OpCodes.Call &&
                    instruction.operand == AccessTools.Property(type: typeof(ThingCategoryNodeDatabase), name: nameof(ThingCategoryNodeDatabase.RootNode)).GetGetMethod() &&
                    !patched)
                {
                    yield return new CodeInstruction(OpCodes.Ldarga_S, 0);
                    yield return new CodeInstruction(OpCodes.Dup);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Property(type: typeof(Rect), name: nameof(Rect.yMax)).GetGetMethod());
                    yield return new CodeInstruction(OpCodes.Ldc_R4, 28f);
                    yield return new CodeInstruction(OpCodes.Sub);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Property(type: typeof(Rect), name: nameof(Rect.yMax)).GetSetMethod());
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Ldarg_3);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(type: typeof(HarmonyPatches), name: nameof(HarmonyPatches.DrawSearchBarFilterConfig)));

                    patched = true;
                }

                yield return instruction;
            }
        }
        
        private static List<TransferableOneWay> QueryTransferable(List<TransferableOneWay> cachedTransferables)
        {
            if (!string.IsNullOrEmpty(SearchUtility.Keyword))
            {
                return (from x in cachedTransferables where SearchUtility.CheckVisible(x) select x).ToList();
            }
            else
            {
                return cachedTransferables;
            }
        }

        public static IEnumerable<CodeInstruction> FillMainRectTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            bool patch1 = false;
            List<CodeInstruction> instructionList = instructions.ToList();
            for (int i = 0; i < instructionList.Count; ++i)
            {
                CodeInstruction instruction = instructionList[i];

                if (!patch1)
                {
                    CodeInstruction targetSymbol = instructionList[i + 3];
                    if (targetSymbol.opcode == OpCodes.Call && targetSymbol.operand is MethodInfo &&
                        ((MethodInfo)targetSymbol.operand).Name == "Any")
                    {
                        patch1 = true;
                        yield return instruction;
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatches), nameof(QueryTransferable)));
                        continue;
                    }
                }
                
                yield return instruction;
            }
        }
    }
}
