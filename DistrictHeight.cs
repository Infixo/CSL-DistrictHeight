//using System;
//using System.Collections.Generic;
using System.Linq;
//using System.Text;
//using ColossalFramework;
//using UnityEngine;

namespace DistrictHeight
{
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
