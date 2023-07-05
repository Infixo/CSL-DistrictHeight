using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
using HarmonyLib;

namespace DistrictHeight
{
    public static class DistrictHeightPatcher
    {
        private const string HarmonyId = "Infixo.DistrictHeight";
        private static bool patched = false;

        public static void PatchAll()
        {
            if (patched) { Debug.Log($"{HarmonyId}.PatchAll: already patched!"); return; }
            //Harmony.DEBUG = true;
            var harmony = new Harmony(HarmonyId);
            harmony.PatchAll();
            if (Harmony.HasAnyPatches(HarmonyId))
            {
                Debug.Log($"{HarmonyId}.PatchAll: OK methods patched");
                patched = true;
                var myOriginalMethods = harmony.GetPatchedMethods();
                foreach (var method in myOriginalMethods)
                    Debug.Log($"{HarmonyId}.PatchAll: ...method {method.Name}");
            }
            else
                Debug.Log($"{HarmonyId}.PatchAll: ERROR methods not patched");
            //Harmony.DEBUG = false;
        }

        public static void UnpatchAll()
        {
            if (!patched) { Debug.Log($"{HarmonyId}.UnpatchAll: not patched!"); return; }
            //Harmony.DEBUG = true;
            var harmony = new Harmony(HarmonyId);
            harmony.UnpatchAll(HarmonyId);
            Debug.Log($"{HarmonyId}.UnpatchAll: OK methods unpatched");
            patched = false;
            //Harmony.DEBUG = false;
        }
    }

    [HarmonyPatch(typeof(BuildingManager))]
    public static class BuildingManager_Patches
    {
        [HarmonyReversePatch, HarmonyPatch("GetAreaIndex")]
        public static int BuildingManager_GetAreaIndex_Reverse(ItemClass.Service service, ItemClass.SubService subService, ItemClass.Level level, int width, int length, BuildingInfo.ZoningMode zoningMode)
        {
            Debug.Log("ERROR: GetVisitBehaviour reverse patch not applied");
            return 0;
        }
    }
    /* NOT USED    
        //[HarmonyPrefix, HarmonyPatch(nameof(BuildingManager.GetRandomBuildingInfo))]
        public static bool BuildingManager_GetRandomBuildingInfo_Prefix(BuildingManager __instance, ref BuildingInfo __result,
            // original arguments
            ref Randomizer r, ItemClass.Service service, ItemClass.SubService subService, ItemClass.Level level, int width, int length, BuildingInfo.ZoningMode zoningMode, int style,
            // private members that are used in the routine - start with 3 underscores
            bool ___m_buildingsRefreshed,
            Dictionary<int, FastList<ushort>>[] ___m_styleBuildings,
            FastList<ushort>[] ___m_areaBuildings)
        {
            __result = null;
            if (!___m_buildingsRefreshed)
            {
                CODebugBase<LogChannel>.Error(LogChannel.Core, "Random buildings not refreshed yet!");
                return false;
            }
            // find a list of possible upgrades using preprepared data in m_areaBuildings or m_styleBuildings
            int areaIndex = BuildingManager_GetAreaIndex_Reverse(service, subService, level, width, length, zoningMode);
            FastList<ushort> possibleUpgrades;
            if (style > 0)
            {
                style--;
                DistrictStyle districtStyle = Singleton<DistrictManager>.instance.m_Styles[style];
                possibleUpgrades = ((style > ___m_styleBuildings.Length || ___m_styleBuildings[style] == null || ___m_styleBuildings[style].Count <= 0 || !districtStyle.AffectsService(service, subService, level)) ? ___m_areaBuildings[areaIndex] : ((!___m_styleBuildings[style].ContainsKey(areaIndex)) ? null : ___m_styleBuildings[style][areaIndex]));
            }
            else
                possibleUpgrades = ___m_areaBuildings[areaIndex];
            if (possibleUpgrades == null || possibleUpgrades.m_size == 0)
                return false;
            // this is core of the mod - filter out buildings using height
            // TODO: get settings from district
            FastList<ushort> allowedUpgrades = new FastList<ushort>();
            foreach (ushort item in possibleUpgrades)
            {
                float height = PrefabCollection<BuildingInfo>.GetPrefab(item).GetHeight(); // TODO: this should be stored and reused later (dynamic late storage)
                if (50f < height && height <= 70f) // TESTING HARDCODED VALUES - TODO get them from district data; what if there is no district? for city - no restrictions?
                    allowedUpgrades.Add(item);
            }
            // is there anything possible?
            if (allowedUpgrades.m_size == 0)
                // no building that matches height criteria
                // set building to Historical and return "normal" building instead
                // PrivateBuildingAI
                //public override void SetHistorical(ushort buildingID, ref Building data, bool historical)
                // PROBLEM - here we don't know which building exactly is upgraded...
                // need to modify public override BuildingInfo GetUpgradeInfo(ushort buildingID, ref Building data)
                return false;   
            areaIndex = r.Int32((uint)allowedUpgrades.m_size);
            __result = PrefabCollection<BuildingInfo>.GetPrefab(allowedUpgrades.m_buffer[areaIndex]);
            //areaIndex = r.Int32((uint)possibleUpgrades.m_size);
            //__result = PrefabCollection<BuildingInfo>.GetPrefab(possibleUpgrades.m_buffer[areaIndex]);
            return false;
        }
    */

    // an extension method is needed to be able to retrieve District data and set the building as historical when necessary
    public static class BuildingManager_Extensions
    {
        public static BuildingInfo GetRandomBuildingInfoExt(this BuildingManager instance,
            // original arguments
            ref Randomizer r, ItemClass.Service service, ItemClass.SubService subService, ItemClass.Level level, int width, int length, BuildingInfo.ZoningMode zoningMode, int style,
            // extra params
            byte district, bool isUpgrade, ushort buildingID, ref Building data, PrivateBuildingAI buildingAI)
        {
            // private variables we need to access in BuildingManager
            // this is a very hacky method...
            bool m_buildingsRefreshed = ExtensionsHelper.GetPrivateField<bool>(instance, "m_buildingsRefreshed");
            Dictionary<int, FastList<ushort>>[] m_styleBuildings = ExtensionsHelper.GetPrivateField<Dictionary<int, FastList<ushort>>[]>(instance, "m_styleBuildings");
            FastList<ushort>[] m_areaBuildings = ExtensionsHelper.GetPrivateField<FastList<ushort>[]>(instance, "m_areaBuildings");
            // original code
            if (!m_buildingsRefreshed)
            {
                CODebugBase<LogChannel>.Error(LogChannel.Core, "Random buildings not refreshed yet!");
                return null;
            }
            //int areaIndex = GetAreaIndex(service, subService, level, width, length, zoningMode);
            int areaIndex = BuildingManager_Patches.BuildingManager_GetAreaIndex_Reverse(service, subService, level, width, length, zoningMode);
            FastList<ushort> possibleUpgrades;
            if (style > 0)
            {
                style--;
                DistrictStyle districtStyle = Singleton<DistrictManager>.instance.m_Styles[style];
                possibleUpgrades = ((style > m_styleBuildings.Length || m_styleBuildings[style] == null || m_styleBuildings[style].Count <= 0 || !districtStyle.AffectsService(service, subService, level)) ? m_areaBuildings[areaIndex] : ((!m_styleBuildings[style].ContainsKey(areaIndex)) ? null : m_styleBuildings[style][areaIndex]));
            }
            else
            {
                possibleUpgrades = m_areaBuildings[areaIndex];
            }
            if (possibleUpgrades == null || possibleUpgrades.m_size == 0)
                return null;
            // this is the core of the mod - filter out buildings using height
            // get settings from the district
            FastList<ushort> allowedUpgrades = new FastList<ushort>();
            float minH = DistrictHeightManager.Min[district];
            float maxH = DistrictHeightManager.Max[district];
            foreach (ushort item in possibleUpgrades)
            {
                float height = PrefabCollection<BuildingInfo>.GetPrefab(item).GetHeight(); // TODO: this should be stored and reused later (dynamic late storage)
                if (height >= minH && (maxH == 0f || height <= maxH)) // 0.0f means unlimited height actually
                        allowedUpgrades.Add(item);
            }
            // is there anything possible?
            //if (!isUpgrade) DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, $"DH: new {subService} {width}x{length}");
            if (allowedUpgrades.m_size == 0)
            {
                // 0 means none, but 1 means that ALL will be the same... a bit weird... could be parameterized
                // no building that matches height criteria
                // set building to Historical and return "normal" building instead
                // PrivateBuildingAI
                //public override void SetHistorical(ushort buildingID, ref Building data, bool historical)
                // PROBLEM - here we don't know which building exactly is upgraded...
                // need to modify public override BuildingInfo GetUpgradeInfo(ushort buildingID, ref Building data)
                //return null;
                // set current building to Historical
                //BuildingInfo info = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].Info;
                if (isUpgrade)
                {
                    buildingAI.SetHistorical(buildingID, ref data, true);
                    allowedUpgrades = possibleUpgrades; // for historical doesn't matter - can be anything
                }
                else
                {
                    //DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, "...no suitable buildings");
                    // TODO: what happes when there is no option for a new building?
                    //allowedUpgrades = possibleUpgrades;
                    return null;
                }
            }
            //areaIndex = r.Int32((uint)possibleUpgrades.m_size);
            //__result = PrefabCollection<BuildingInfo>.GetPrefab(possibleUpgrades.m_buffer[areaIndex]);
            areaIndex = r.Int32((uint)allowedUpgrades.m_size);
            return PrefabCollection<BuildingInfo>.GetPrefab(allowedUpgrades.m_buffer[areaIndex]);
        }

        public static BuildingInfo GetRandomBuildingInfoDis(this BuildingManager instance,
            // original arguments
            ref Randomizer r, ItemClass.Service service, ItemClass.SubService subService, ItemClass.Level level, int width, int length, BuildingInfo.ZoningMode zoningMode, int style,
            // extra params
            byte district)
        {
            Building data = new Building();
            return instance.GetRandomBuildingInfoExt(ref r, service, subService, level, width, length, zoningMode, style,
                district, false, 0, ref data, null);
        }

    }

    // code generated by ILSpy
    [HarmonyPatch(typeof(PrivateBuildingAI))]
    public static class PrivateBuildingAI_Patches
    {
        [HarmonyPrefix, HarmonyPatch(nameof(PrivateBuildingAI.GetUpgradeInfo))]
        public static bool PrivateBuildingAI_GetUpgradeInfo_Prefix(PrivateBuildingAI __instance, ref BuildingInfo __result, ushort buildingID, ref Building data)
        {
            if (data.m_level == 4)
            {
                __result = null;
                return false;
            }
            Randomizer r = new Randomizer(buildingID);
            for (int i = 0; i <= data.m_level; i++)
            {
                r.Int32(1000u);
            }
            ItemClass.Level level = (ItemClass.Level)(data.m_level + 1);
            DistrictManager instance = Singleton<DistrictManager>.instance;
            byte district = instance.GetDistrict(data.m_position);
            ushort style = instance.m_districts.m_buffer[district].m_Style;
            //__result = Singleton<BuildingManager>.instance.GetRandomBuildingInfo(ref r, __instance.m_info.m_class.m_service, __instance.m_info.m_class.m_subService, level, data.Width, data.Length, __instance.m_info.m_zoningMode, style);
            __result = Singleton<BuildingManager>.instance.GetRandomBuildingInfoExt(
                // original arguments
                ref r, __instance.m_info.m_class.m_service, __instance.m_info.m_class.m_subService, level, data.Width, data.Length, __instance.m_info.m_zoningMode, style,
                // extra arguments
                district, true, buildingID, ref data, __instance);
            return false;
        }
    }

    /* 
     * ZoneBlock::SimulationStep(ushort blockID)
     * Long method, a Transpiler is best approach here
     * 
// ORIGINAL CODE 
// buildingInfo = Singleton<BuildingManager>.instance.GetRandomBuildingInfo(ref Singleton<SimulationManager>.instance.m_randomizer, service, subService, level, num26, num25, zoningMode3, style);
IL_0e6c: call !0 class [ColossalManaged]ColossalFramework.Singleton`1<class BuildingManager>::get_instance()
IL_0e71: call !0 class [ColossalManaged]ColossalFramework.Singleton`1<class SimulationManager>::get_instance()
IL_0e76: ldflda valuetype [ColossalManaged]ColossalFramework.Math.Randomizer SimulationManager::m_randomizer
IL_0e7b: ldloc.s 54
IL_0e7d: ldloc.s 55
IL_0e7f: ldloc.s 56
IL_0e81: ldloc.s 61
IL_0e83: ldloc.s 60
IL_0e85: ldloc.s 62
IL_0e87: ldloc.s 65
IL_0e89: callvirt instance class BuildingInfo BuildingManager::GetRandomBuildingInfo(valuetype [ColossalManaged]ColossalFramework.Math.Randomizer&, valuetype ItemClass/Service, valuetype ItemClass/SubService, valuetype ItemClass/Level, int32, int32, valuetype BuildingInfo/ZoningMode, int32)
IL_0e8e: stloc.s 57

// CHANGED CODE
// buildingInfo = Singleton<BuildingManager>.instance.GetRandomBuildingInfoDis(ref Singleton<SimulationManager>.instance.m_randomizer, service, subService, level, width, length, zoningMode, style, district);
IL_00e4: call !0 class [ColossalManaged]ColossalFramework.Singleton`1<class ['Assembly-CSharp']BuildingManager>::get_instance()
IL_00e9: call !0 class [ColossalManaged]ColossalFramework.Singleton`1<class ['Assembly-CSharp']SimulationManager>::get_instance()
IL_00ee: ldflda valuetype [ColossalManaged]ColossalFramework.Math.Randomizer ['Assembly-CSharp']SimulationManager::m_randomizer
IL_00f3: ldloc.s 5
IL_00f5: ldloc.s 6
IL_00f7: ldloc.1
IL_00f8: ldloc.s 8
IL_00fa: ldloc.s 9
IL_00fc: ldloc.s 10
IL_00fe: ldloc.s 4
[new] IL_0100: ldloc.3
[changed] IL_0101: call class ['Assembly-CSharp']BuildingInfo DistrictHeight.BuildingManager_Extensions::GetRandomBuildingInfoDis(class ['Assembly-CSharp']BuildingManager, valuetype [ColossalManaged]ColossalFramework.Math.Randomizer&, valuetype ['Assembly-CSharp']ItemClass/Service, valuetype ['Assembly-CSharp']ItemClass/SubService, valuetype ['Assembly-CSharp']ItemClass/Level, int32, int32, valuetype ['Assembly-CSharp']BuildingInfo/ZoningMode, int32, uint8)
IL_0106: stloc.s 7

     */

    [HarmonyPatch(typeof(ZoneBlock), nameof(ZoneBlock.SimulationStep))]
    public static class ZoneBlock_SimulationStep_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            IEnumerator<CodeInstruction> ilcode = instructions.GetEnumerator();
            while (ilcode.MoveNext())
            {
                CodeInstruction instr = ilcode.Current;
                //if (instr.opcode == OpCodes.Callvirt && instr.operand == AccessTools.Method(typeof(BuildingWrapper), "OnCalculateSpawn"))
                if (instr.opcode == OpCodes.Callvirt && instr.operand == AccessTools.Method(typeof(BuildingManager), "GetRandomBuildingInfo"))
                {
                    // new and changed instructions
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 65); // ldloc.s 64, 230630 Adjust for latest patch - there is another variable used, so all stack ids are +1
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BuildingManager_Extensions), "GetRandomBuildingInfoDis"));
                    // skip old instructions
                    _ = ilcode.MoveNext();
                    // continue with the rest
                    instr = ilcode.Current;
                    //DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, "found OnCalculateSpawn");
                    //DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, instr.operand.ToString());
                }
                yield return instr;
            }
        }
    }

    /*
    // ORIGINAL IL CODE 
    // BuildingInfo randomBuildingInfo = Singleton<BuildingManager>.instance.GetRandomBuildingInfo(ref Singleton<SimulationManager>.instance.m_randomizer, m_info.m_class.m_service, subService, level, width, num3, m_info.m_zoningMode, style2);
	IL_056d: call !0 class [ColossalManaged]ColossalFramework.Singleton`1<class BuildingManager>::get_instance()
	IL_0572: call !0 class [ColossalManaged]ColossalFramework.Singleton`1<class SimulationManager>::get_instance()
	IL_0577: ldflda valuetype [ColossalManaged]ColossalFramework.Math.Randomizer SimulationManager::m_randomizer
	IL_057c: ldarg.0
	IL_057d: ldfld class BuildingInfo BuildingAI::m_info
	IL_0582: ldfld class ItemClass BuildingInfo::m_class
	IL_0587: ldfld valuetype ItemClass/Service ItemClass::m_service
	IL_058c: ldloc.s 16
	IL_058e: ldloc.s 17
	IL_0590: ldloc.s 18
	IL_0592: ldloc.s 19
	IL_0594: ldarg.0
	IL_0595: ldfld class BuildingInfo BuildingAI::m_info
	IL_059a: ldfld valuetype BuildingInfo/ZoningMode BuildingInfo::m_zoningMode
	IL_059f: ldloc.s 22
	IL_05a1: callvirt instance class BuildingInfo BuildingManager::GetRandomBuildingInfo(valuetype [ColossalManaged]ColossalFramework.Math.Randomizer&, valuetype ItemClass/Service, valuetype ItemClass/SubService, valuetype ItemClass/Level, int32, int32, valuetype BuildingInfo/ZoningMode, int32)
	IL_05a6: stloc.s 23
    */
    [HarmonyPatch(typeof(PrivateBuildingAI), nameof(PrivateBuildingAI.SimulationStep))]
    public static class PrivateBuildingAI_SimulationStep_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            IEnumerator<CodeInstruction> ilcode = instructions.GetEnumerator();
            while (ilcode.MoveNext())
            {
                CodeInstruction instr = ilcode.Current;
                if (instr.opcode == OpCodes.Callvirt && instr.operand == AccessTools.Method(typeof(BuildingManager), "GetRandomBuildingInfo"))
                {
                    // new and changed instructions
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 21); // put district on stack:ldloc.s 21
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BuildingManager_Extensions), "GetRandomBuildingInfoDis"));
                    // skip old instructions
                    _ = ilcode.MoveNext();
                    // continue with the rest
                    instr = ilcode.Current;
                    //DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, "found OnCalculateSpawn");
                    //DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, instr.operand.ToString());
                }
                yield return instr;
            }
        }
    }

} // namespace