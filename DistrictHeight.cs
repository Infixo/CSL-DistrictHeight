using System;
using System.Reflection;
using System.Linq;
using ColossalFramework.IO;

namespace DistrictHeight
{
    public static class ExtensionsHelper
    {
        // The below code is from https://www.codeproject.com/Articles/80343/Accessing-private-members
        // and https://stackoverflow.com/questions/1548320/can-c-sharp-extension-methods-access-private-variables

        public static T GetPrivateField<T>(this object obj, string name)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            Type type = obj.GetType();
            FieldInfo field = type.GetField(name, flags);
            return (T)field.GetValue(obj);
        }

        public static T GetPrivateProperty<T>(this object obj, string name)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            Type type = obj.GetType();
            PropertyInfo field = type.GetProperty(name, flags);
            return (T)field.GetValue(obj, null);
        }

        public static void SetPrivateField(this object obj, string name, object value)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            Type type = obj.GetType();
            FieldInfo field = type.GetField(name, flags);
            field.SetValue(obj, value);
        }

        public static void SetPrivateProperty(this object obj, string name, object value)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            Type type = obj.GetType();
            PropertyInfo field = type.GetProperty(name, flags);
            field.SetValue(obj, value, null);
        }

        public static T CallPrivateMethod<T>(this object obj, string name, params object[] param)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            Type type = obj.GetType();
            MethodInfo method = type.GetMethod(name, flags);
            return (T)method.Invoke(obj, param);
        }
    }

    public static class BuildingInfo_Extensions
    {
        public static float GetHeight(this BuildingInfo instance)
        {
            // height - snippet from FindIt
            if (instance.m_generatedInfo?.m_heights != null)
                if (instance.m_generatedInfo.m_heights.Length > 0)
                    return instance.m_generatedInfo.m_heights.Max();
            return 0f;
        }
    }

    public static class DistrictHeightManager
    {
        private static float[] m_minHeights;
        private static float[] m_maxHeights;

        public static float[] Min { get { return m_minHeights; } set { m_minHeights = value; } }
        public static float[] Max { get { return m_maxHeights; } set { m_maxHeights = value; } }

        // Minimum: Sky: 60/70/80 High: 35/40/45 Med: 10/15/20
        public static readonly float[] MinList = new float[] { 0, 10, 15, 20, 35, 40, 45, 60, 70, 80 };
        // Maximum: Low 15/18/21 Medium 25/30/35/40 High 50/60/70/80/90/100
        public static readonly float[] MaxList = new float[] { 0, 100, 90, 80, 70, 60, 50, 40, 35, 30, 25, 21, 18, 15 };

        public static void ResetHeights()
        {
            m_minHeights = new float[DistrictManager.MAX_DISTRICT_COUNT];
            m_maxHeights = new float[DistrictManager.MAX_DISTRICT_COUNT];
            for (int i = 0; i < DistrictManager.MAX_DISTRICT_COUNT; i++)
            {
                m_minHeights[i] = 0f; m_maxHeights[i] = 0f;
            }
        }

    }

	public class DistrictHeightData : IDataContainer
	{
		private static readonly int VERSION = 1; // stored as Int8, so version should be 1..127

		public void Serialize(DataSerializer s)
		{
			s.WriteInt8(VERSION);

			// VERSION 1
			// maximum num of districts
			s.WriteInt16(DistrictManager.MAX_DISTRICT_COUNT);
			// data - min / max heights for each districts
			for (int i = 0; i < DistrictManager.MAX_DISTRICT_COUNT; i++)
			{
				s.WriteFloat(DistrictHeightManager.Min[i]);
				s.WriteFloat(DistrictHeightManager.Max[i]);
			}
		}

		public void Deserialize(DataSerializer s)
		{
			int version = s.ReadInt8();

			// VERSION 1
			if (version == 1)
			{
				int numDist = s.ReadInt16();
				// data - min heights
				float[] minBuf = new float[numDist];
				float[] maxBuf = new float[numDist];
				for (int i = 0; i < numDist; i++)
                {
					minBuf[i] = s.ReadFloat(); maxBuf[i] = s.ReadFloat();
                }
                DistrictHeightManager.Min = minBuf;
                DistrictHeightManager.Max = maxBuf;
			}
        }

		public void AfterDeserialize(DataSerializer s)
		{
		}
	}

} // namespace
