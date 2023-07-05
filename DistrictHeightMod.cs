using System.IO;
using ColossalFramework.IO;
using ICities;
using CitiesHarmony.API;

namespace DistrictHeight
{
    public class DistrictHeightMod : IUserMod, ILoadingExtension
    {
        public string Name => "District Height 0.8";
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

    public class DistrictHeightSerializer : ISerializableDataExtension
    {
        public ISerializableData SerializableDataManager;
        private const string DATA_ID = "DistrictHeight"; // unique data id
        private const uint DATA_VERSION = 0; // current data version

        public void OnCreated(ISerializableData serializableData)
        {
            SerializableDataManager = serializableData;
        }

        public void OnReleased()
        {
        }

        public void OnSaveData()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, DATA_VERSION, new DistrictHeightData()); // serialize district tables
                SerializableDataManager.SaveData(DATA_ID, stream.ToArray()); // write to savegame
            }
        }

        public void OnLoadData()
        {
            byte[] data = SerializableDataManager.LoadData(DATA_ID); // read data from savegame

            // Check to see if anything was read.
            if (data != null && data.Length != 0)
            {
                // data was read, we can deserialise
                using (MemoryStream stream = new MemoryStream(data))
                {
                    DataSerializer.Deserialize<DistrictHeightData>(stream, DataSerializer.Mode.Memory); // deserialize district tables
                }
            }
            else
            {
                // no data read, initialise empty data
                DistrictHeightManager.ResetHeights();
            }
        }
    }

} // namespace
