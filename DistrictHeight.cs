using System;
//using System.Collections.Generic;
using System.Reflection;
using System.Linq;
//using System.Text;
//using ColossalFramework;
using ColossalFramework.IO;
//using UnityEngine;

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
		private static float[] m_minHeights; // = new float[DistrictManager.MAX_DISTRICT_COUNT];
		private static float[] m_maxHeights; // = new float[DistrictManager.MAX_DISTRICT_COUNT];

        public static float[] Min => m_minHeights;
        public static float[] Max => m_maxHeights;

        public static readonly float[] MinList = new float[] { 0, 10, 20, 30, 40, 50, 60, 80, 100 };
        public static readonly float[] MaxList = new float[] { 0, 150, 120, 100, 80, 70, 60, 50, 40, 30, 20, 10 };

        public static void ResetHeights()
        {
			m_minHeights = new float[DistrictManager.MAX_DISTRICT_COUNT];
			m_maxHeights = new float[DistrictManager.MAX_DISTRICT_COUNT];
			for (int i = 0; i < DistrictManager.MAX_DISTRICT_COUNT; i++)
            {
				m_minHeights[i] = 0f; m_maxHeights[i] = 0f;
            }
        }

		public class Data : IDataContainer
		{
			private static string PREFIX = "DistrictHeight";
			private static int VERSION = 1; // stored as Int8, so version should be 1..127

			public void Serialize(DataSerializer s)
			{
				// header
				s.WriteUniqueString(PREFIX);
				s.WriteInt8(VERSION);

				// VERSION 1
				// maximum num of districts
				s.WriteInt16(DistrictManager.MAX_DISTRICT_COUNT);
				// data - min / max heights for each districts
				for (int i = 0; i < DistrictManager.MAX_DISTRICT_COUNT; i++)
				{
					s.WriteFloat(m_minHeights[i]);
					s.WriteFloat(m_maxHeights[i]);
				}
			}

			public void Deserialize(DataSerializer s)
			{
				string prefix;
				// header
				try
				{
					prefix = s.ReadUniqueString();
				}
				catch
				{
					// this assumes it is a load of am old save
					ResetHeights();
					return;
				}
				if (prefix != PREFIX)
					// some error reading savefile - should NOT happen
					throw new Exception($"DistrictHeight: cannot read data from savefile, wrong prefix {prefix}");

				int version = s.ReadInt8();

				// VERSION 1
				if (version >= 1)
				{
					int numDist = s.ReadInt16();
					// data - min heights
					float[] minBuf = new float[numDist];
					float[] maxBuf = new float[numDist];
					for (int i = 0; i < numDist; i++)
                    {
						minBuf[i] = s.ReadFloat(); maxBuf[i] = s.ReadFloat();
                    }
					m_minHeights = minBuf;
					m_maxHeights = maxBuf;
				}
            }

			public void AfterDeserialize(DataSerializer s)
			{
				// Log. all ok
			}
		}

	}

} // namespace
