using System;
//using System.Collections.Generic;
using System.Reflection;
using System.Linq;
//using System.Text;
//using ColossalFramework;
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
} // namespace
