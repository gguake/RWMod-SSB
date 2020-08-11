using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HarmonyLib;
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
            Harmony harmony = new Harmony(id: "rimworld.gguake.simplesearchbar.main");
            
            // Draw search bar
            harmony.Patch(original: AccessTools.Method(typeof(ThingFilterUI), "DoThingFilterConfigWindow"),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(DoThingFilterConfigWindowTranspiler)));

            harmony.Patch(original: AccessTools.Method(typeof(TransferableOneWayWidget), "FillMainRect"),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(DrawSearchBarTransfer)));

            harmony.Patch(original: AccessTools.Method(typeof(Dialog_Trade), "DoWindowContents"),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(Dialog_Trade_DoWindowContentsTranspiler)));

            // visibility
            harmony.Patch(original: AccessTools.Method(typeof(Listing_TreeThingFilter), "Visible", parameters: new[] { typeof(ThingDef) }),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(VisiblePostfix)));

            harmony.Patch(original: AccessTools.Method(typeof(Listing_TreeThingFilter), "DoCategory"),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(DoCategoryTranspiler)));
            

            harmony.Patch(original: AccessTools.Method(typeof(TransferableOneWayWidget), "FillMainRect"),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(FillMainRectTranspiler)));

            harmony.Patch(original: AccessTools.Method(typeof(Dialog_Trade), "FillMainRect"),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(Dialog_Trade_FillMainRectTranspiler)));
            
            // reset keywords
            harmony.Patch(original: AccessTools.Constructor(typeof(Dialog_BillConfig), parameters: new[] { typeof(Bill_Production), typeof(IntVec3) }),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(ResetKeyword)));

            harmony.Patch(original: AccessTools.Constructor(typeof(Dialog_ManageOutfits), parameters: new[] { typeof(Outfit) }),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(ResetKeyword)));

            harmony.Patch(original: AccessTools.Constructor(typeof(Dialog_ManageFoodRestrictions), parameters: new[] { typeof(FoodRestriction) }),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(ResetKeyword)));

            harmony.Patch(original: AccessTools.Method(typeof(InspectPaneUtility), "ToggleTab"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(ResetKeyword)));

            harmony.Patch(original: AccessTools.Method(typeof(Dialog_LoadTransporters), "PostOpen"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(ResetKeyword)));

            harmony.Patch(original: AccessTools.Method(typeof(Dialog_FormCaravan), "PostOpen"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(ResetKeyword)));


#if (V11)
            harmony.Patch(original: AccessTools.Method(typeof(Dialog_FormCaravan), "<DoWindowContents>b__76_0"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(ResetKeyword)));

            harmony.Patch(original: AccessTools.Method(typeof(Dialog_FormCaravan), "<DoWindowContents>b__76_1"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(ResetKeyword)));

            harmony.Patch(original: AccessTools.Method(typeof(Dialog_LoadTransporters), "<DoWindowContents>b__62_0"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(ResetKeyword)));

            harmony.Patch(original: AccessTools.Method(typeof(Dialog_LoadTransporters), "<DoWindowContents>b__62_1"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(ResetKeyword)));
#else
            harmony.Patch(original: AccessTools.Method(typeof(Dialog_FormCaravan), "<DoWindowContents>b__81_0"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(ResetKeyword)));

            harmony.Patch(original: AccessTools.Method(typeof(Dialog_FormCaravan), "<DoWindowContents>b__81_1"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(ResetKeyword)));

            harmony.Patch(original: AccessTools.Method(typeof(Dialog_FormCaravan), "<DoWindowContents>b__81_2"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(ResetKeyword)));

            harmony.Patch(original: AccessTools.Method(typeof(Dialog_LoadTransporters), "<DoWindowContents>b__62_0"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(ResetKeyword)));

            harmony.Patch(original: AccessTools.Method(typeof(Dialog_LoadTransporters), "<DoWindowContents>b__62_1"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(ResetKeyword)));
#endif
            
            harmony.Patch(original: AccessTools.Method(typeof(Dialog_Trade), "PostOpen"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(ResetKeyword)));

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
            if (__result && !string.IsNullOrEmpty(SearchUtility.Keyword) && !SearchUtility.CheckVisible(td))
            {
                __result = false;
            }
        }

        private static FieldInfo fieldFilter = AccessTools.Field(typeof(Listing_TreeThingFilter), "filter");
        private static FieldInfo fieldSettingsChangedCallback = AccessTools.Field(typeof(ThingFilter), "settingsChangedCallback");
        private static void SetAllowInCategory(Listing_TreeThingFilter instance, TreeNode_ThingCategory node, bool allow, List<ThingDef> exceptedDefs, List<SpecialThingFilterDef> exceptedFilters)
        {
            ThingFilter filter = fieldFilter.GetValue(instance) as ThingFilter;
            ThingCategoryDef categoryDef = node.catDef;
            if (!ThingCategoryNodeDatabase.initialized)
            {
                Log.Error("SetAllow categories won't work before ThingCategoryDatabase is initialized.", false);
            }

            foreach (ThingDef thingDef in categoryDef.DescendantThingDefs)
            {
                if ((exceptedDefs == null || !exceptedDefs.Contains(thingDef)) && SearchUtility.CheckVisible(thingDef))
                {
                    if (SearchUtility.CheckVisible(thingDef))
                    {
                        filter.SetAllow(thingDef, allow);
                    }
                }
            }
            foreach (SpecialThingFilterDef specialThingFilterDef in categoryDef.DescendantSpecialThingFilterDefs)
            {
                if (exceptedFilters == null || !exceptedFilters.Contains(specialThingFilterDef))
                {
                    filter.SetAllow(specialThingFilterDef, allow);
                }
            }
            
            (fieldSettingsChangedCallback.GetValue(filter) as Action)?.Invoke();
        }

        public static IEnumerable<CodeInstruction> DoCategoryTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            bool patch = false;
            List<CodeInstruction> instructionList = instructions.ToList();
            for (int i = 0; i < instructionList.Count; ++i)
            {
                CodeInstruction instruction = instructionList[i];
                if (instruction.opcode == OpCodes.Beq && !patch)
                {
                    patch = true;
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldloc_1);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                    yield return new CodeInstruction(OpCodes.Ceq);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Listing_TreeThingFilter), "forceHiddenDefs"));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Listing_TreeThingFilter), "hiddenSpecialFilters"));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatches), "SetAllowInCategory"));
                    i += 12;
                    continue;
                }

                yield return instruction;
            }
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
                    instruction.operand == AccessTools.Property(typeof(ThingCategoryNodeDatabase), nameof(ThingCategoryNodeDatabase.RootNode)).GetGetMethod() &&
                    !patched)
                {
                    yield return new CodeInstruction(OpCodes.Ldarga_S, 0);
                    yield return new CodeInstruction(OpCodes.Dup);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Property(typeof(Rect), nameof(Rect.yMax)).GetGetMethod());
                    yield return new CodeInstruction(OpCodes.Ldc_R4, 28f);
                    yield return new CodeInstruction(OpCodes.Sub);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Property(typeof(Rect), nameof(Rect.yMax)).GetSetMethod());
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Ldarg_3);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatches), nameof(HarmonyPatches.DrawSearchBarFilterConfig)));

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
