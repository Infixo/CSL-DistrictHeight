using CitiesHarmony.API;
using ICities;

namespace DistrictHeight
{
    public class DistrictHeightMod : IUserMod, ILoadingExtension
    {
        public string Name => "District Height";
        public string Description => "Controls height of buildings in the distric";

        public void OnEnabled()
        {
            HarmonyHelper.DoOnHarmonyReady(() => DistrictHeightPatcher.PatchAll());
        }

        public void OnDisabled()
        {
            if (HarmonyHelper.IsHarmonyInstalled) DistrictHeightPatcher.UnpatchAll();
        }

        // called when level loading begins
        public void OnCreated(ILoading loading)
        {
            //throw new System.NotImplementedException();
        }

        // called when level is loaded
        public void OnLevelLoaded(LoadMode mode)
        {
            if (DistrictHeightManager.Min == null || DistrictHeightManager.Max == null)
                DistrictHeightManager.ResetHeights();
        }

        // called when unloading begins
        public void OnLevelUnloading()
        {
            //throw new System.NotImplementedException();
        }

        // called when unloading finished
        public void OnReleased()
        {
            //throw new System.NotImplementedException();
        }

    }
}
