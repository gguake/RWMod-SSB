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

            // Draw search bar
            harmony.Patch(original: AccessTools.Method(type: typeof(ThingFilterUI), name: "DoThingFilterConfigWindow"),
                transpiler: new HarmonyMethod(type: typeof(HarmonyPatches), name: nameof(DoThingFilterConfigWindowTranspiler)));

            harmony.Patch(original: AccessTools.Method(type: typeof(TransferableOneWayWidget), name: "FillMainRect"),
                prefix: new HarmonyMethod(type: typeof(HarmonyPatches), name: nameof(DrawSearchBarTransfer)));

            harmony.Patch(original: AccessTools.Method(type: typeof(Dialog_Trade), name: "DoWindowContents"),
                transpiler: new HarmonyMethod(type: typeof(HarmonyPatches), name: nameof(Dialog_Trade_DoWindowContentsTranspiler)));

            // visibility
            harmony.Patch(original: AccessTools.Method(type: typeof(Listing_TreeThingFilter), name: "Visible", parameters: new[] { typeof(ThingDef) }),
                prefix: null, postfix: new HarmonyMethod(type: typeof(HarmonyPatches), name: nameof(VisiblePostfix)));

            harmony.Patch(original: AccessTools.Method(type: typeof(ThingFilter), name: "SetAllow", parameters: new[] { typeof(ThingDef), typeof(bool) }),
                prefix: new HarmonyMethod(type: typeof(HarmonyPatches), name: nameof(SetAllowPrefix)));
            
            harmony.Patch(original: AccessTools.Method(type: typeof(TransferableOneWayWidget), name: "FillMainRect"),
                transpiler: new HarmonyMethod(type: typeof(HarmonyPatches), name: nameof(FillMainRectTranspiler)));

            harmony.Patch(original: AccessTools.Method(type: typeof(Dialog_Trade), name: "FillMainRect"),
                transpiler: new HarmonyMethod(type: typeof(HarmonyPatches), name: nameof(Dialog_Trade_FillMainRectTranspiler)));

            // reset keywords
            harmony.Patch(original: AccessTools.Constructor(type: typeof(Dialog_BillConfig), parameters: new[] { typeof(Bill_Production), typeof(IntVec3) }),
                postfix: new HarmonyMethod(type: typeof(HarmonyPatches), name: nameof(ResetKeyword)));

            harmony.Patch(original: AccessTools.Method(type: typeof(InspectPaneUtility), name: "ToggleTab"),
                postfix: new HarmonyMethod(type: typeof(HarmonyPatches), name: nameof(ResetKeyword)));

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

            harmony.Patch(original: AccessTools.Method(type: typeof(Dialog_Trade), name: "PostOpen"),
                postfix: new HarmonyMethod(type: typeof(HarmonyPatches), name: nameof(ResetKeyword)));
        }

        public static void DrawSearchBarFilterConfig(Rect rect, ThingFilter filter, ThingFilter parentFilter)
        {
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleLeft;

            Rect rtText = new Rect(rect.x + 1f, rect.yMax + 1f, rect.width - 1f, 27f);
            SearchUtility.Keyword = Widgets.TextField(rtText.LeftPartPixels(rtText.width - 27f), SearchUtility.Keyword);

            if (Widgets.ButtonText(rtText.RightPartPixels(27f), "X", true, true, true))
            {
                SearchUtility.Reset();
                Event.current.Use();
            }

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
        }
        
        public static bool DrawSearchBarTransfer(Rect mainRect)
        {
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleLeft;

            Rect rtText = new Rect(mainRect.width - 195f, 0f, 180f, 27f);
            SearchUtility.Keyword = Widgets.TextField(rtText.LeftPartPixels(rtText.width - 27f), SearchUtility.Keyword);

            if (Widgets.ButtonText(rtText.RightPartPixels(27f), "X", true, true, true))
            {
                SearchUtility.Reset();
                Event.current.Use();
            }

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;

            return true;
        }

        public static void DrawSearchBarTrade(Rect inRect)
        {
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleLeft;

            Rect rtText = new Rect(12f, inRect.height - 49f, 200f, 27f);
            SearchUtility.Keyword = Widgets.TextField(rtText.LeftPartPixels(rtText.width - 27f), SearchUtility.Keyword);

            if (Widgets.ButtonText(rtText.RightPartPixels(27f), "X", true, true, true))
            {
                SearchUtility.Reset();
                Event.current.Use();
            }

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
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

        private static List<Tradeable> QueryTradable(List<Tradeable> cachedTradeables)
        {
            return cachedTradeables.Where(x => SearchUtility.CheckVisible(x)).ToList();
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

        public static IEnumerable<CodeInstruction> Dialog_Trade_DoWindowContentsTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatches), nameof(DrawSearchBarTrade)));

            List<CodeInstruction> instructionList = instructions.ToList();
            for (int i = 0; i < instructionList.Count; ++i)
            {
                CodeInstruction instruction = instructionList[i];
                
                yield return instruction;
            }
        }

        private static List<CodeInstruction> __tempLocal = null;
        public static IEnumerable<CodeInstruction> Dialog_Trade_FillMainRectTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var fieldCachedTradeables = AccessTools.Field(typeof(Dialog_Trade), "cachedTradeables");

            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, fieldCachedTradeables);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatches), nameof(QueryTradable)));
            yield return new CodeInstruction(OpCodes.Stsfld, AccessTools.Field(typeof(HarmonyPatches), nameof(__tempLocal)));

            List<CodeInstruction> instructionList = instructions.ToList();
            for (int i = 0; i < instructionList.Count; ++i)
            {
                CodeInstruction instruction = instructionList[i];
                if (i < instructionList.Count - 1 && 
                    instructionList[i].opcode == OpCodes.Ldarg_0 &&
                    instructionList[i + 1].opcode == OpCodes.Ldfld && instructionList[i + 1].operand == fieldCachedTradeables)
                {
                    yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(HarmonyPatches), nameof(__tempLocal)));
                    i++;
                    continue;
                }

                yield return instruction;
            }
        }
    }
}
