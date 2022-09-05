//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
using UnityEngine;
using ColossalFramework.UI;
using ColossalFramework.Plugins;
using HarmonyLib;

namespace DistrictHeight
{
    [HarmonyPatch(typeof(DistrictWorldInfoPanel))]
    public static class DistrictWorldInfoPanel_Patches
    {
        [HarmonyPostfix, HarmonyPatch("Start")]
        public static void DistrictWorldInfoPanel_Start_Postfix(DistrictWorldInfoPanel __instance)
        {
            DistrictHeightUI.Start(__instance);
        }

        [HarmonyPostfix, HarmonyPatch("OnSetTarget")]
        public static void DistrictWorldInfoPanel_OnSetTarget_Postfix(DistrictWorldInfoPanel __instance)
        {
            DistrictHeightUI.DistrictChanged(__instance);
        }

    }

    public static class DistrictHeightUI
    {
        //private static DistrictWorldInfoPanel m_districtPanel;
        private static UILabel m_districtHeightLabel;
        private static UIDropDown m_minHeightDropDown;
        private static UIDropDown m_maxHeightDropDown;
        private static UIButton m_refreshButton;
        private static bool m_disableEvents;
        private static byte m_districtID; // currently selected district

        public static void Start(DistrictWorldInfoPanel districtPanel)
        {
            //m_districtPanel = districtPanel;
            // add labels
            m_districtHeightLabel = districtPanel.component.AddUIComponent<UILabel>();
            m_districtHeightLabel.name = "DistrictHeightLabel";
            m_districtHeightLabel.text = "district height";
            //m_districtHeightLabel.textScale = 1.0f;
            m_districtHeightLabel.textAlignment = UIHorizontalAlignment.Center;
            m_districtHeightLabel.size = new Vector2(100, 20);
            m_districtHeightLabel.relativePosition = new Vector2(200, 100);
            m_districtHeightLabel.tooltip = "put some explanations here";

            // add dropdowns
            m_minHeightDropDown = districtPanel.component.AddUIComponent<UIDropDown>();
            m_maxHeightDropDown = districtPanel.component.AddUIComponent<UIDropDown>();
            //
            //
            //_minWorkLevelDropDown = UIDropDowns.AddLabelledDropDown(this, width - Margin - MenuWidth, 160f, Translations.Translate("ABLC_LVL_MIN"), 60f, accomodateLabel: false, tooltip: Translations.Translate("ABLC_CAT_WMN_TIP"));
            //_minWorkLevelDropDown.items = new string[] { "1", "2", "3" };

            //_maxWorkLevelDropDown = UIDropDowns.AddLabelledDropDown(this, width - Margin - MenuWidth, 190f, Translations.Translate("ABLC_LVL_MAX"), 60f, accomodateLabel: false, tooltip: Translations.Translate("ABLC_CAT_WMX_TIP"));
            //_maxWorkLevelDropDown.items = new string[] { "1", "2", "3" };

            // BUTTON
            m_refreshButton = districtPanel.component.AddUIComponent<UIButton>();
            // Size and position.

            m_refreshButton.size = new Vector2(50, 20);
            m_refreshButton.relativePosition = new Vector2(250, 150);
            // Appearance.
            m_refreshButton.normalBgSprite = "ButtonMenu";
            m_refreshButton.hoveredBgSprite = "ButtonMenuHovered";
            m_refreshButton.pressedBgSprite = "ButtonMenuPressed";
            m_refreshButton.disabledBgSprite = "ButtonMenuDisabled";
            m_refreshButton.disabledTextColor = new Color32(128, 128, 128, 255);
            m_refreshButton.canFocus = false;
            // Text.
            m_refreshButton.text = "REFRESH";
            m_refreshButton.tooltip = "refresh buildings";
            //m_refreshButton.textScale = 1.0f;
            //m_refreshButton.textPadding = new RectOffset(0, 0, 0, 0);
            m_refreshButton.textVerticalAlignment = UIVerticalAlignment.Middle;
            m_refreshButton.textHorizontalAlignment = UIHorizontalAlignment.Center;
            // hook event handlers
            //m_refreshButton.eventClicked += ClearBuildings;

            // Add event handlers.
            m_minHeightDropDown.eventSelectedIndexChanged += (control, index) =>
            {
                // Don't do anything if events are disabled.
                if (!m_disableEvents)
                {
                    // Set minimum level of building in dictionary.
                    //UpdateMinLevel((byte)index);

                    // If the minimum level is now greater than the maximum level, increase the maximum to match the minimum.
                    //if (index > m_maxLevelDropDown.selectedIndex)
                    //{
                      //  m_maxLevelDropDown.selectedIndex = index;
                    //}
                }
            };

            m_maxHeightDropDown.eventSelectedIndexChanged += (control, index) =>
            {
                // Don't do anything if events are disabled.
                if (!m_disableEvents)
                {
                    // Update maximum level.
                    //UpdateMaxLevel((byte)index);

                    // If the maximum level is now less than the minimum level, reduce the minimum to match the maximum.
                    //if (index < m_minLevelDropDown.selectedIndex)
                    //{
                        //m_minLevelDropDown.selectedIndex = index;
                    //}
                }
            };

            m_refreshButton.eventClick += (control, clickEvent) =>
            {
                // Local references for SimulationManager action.
                //ushort buildingID = m_targetID;
                //byte targetLevel = _upgradeLevel;
                //Logging.KeyMessage("upgrading building to level ", _upgradeLevel);
                //Singleton<SimulationManager>.instance.AddAction(() =>
                //{
                //    LevelUtils.ForceLevel(m_targetID, targetLevel);
                //});

                // Check to see if we should increase this buildings maximum level.
                //if (Buildings.GetMaxLevel(m_targetID) < _upgradeLevel)
                //{
                //    m_maxLevelDropDown.selectedIndex = _upgradeLevel;
                //}
                ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
                panel.SetMessage("DistrictHeightMod", $"Refreshing District ID {m_districtID}", false);
            };


        }

        /// <summary>
        /// Called when the selected district has changed.
        /// </summary>
        public static void DistrictChanged(DistrictWorldInfoPanel districtPanel)
        {
            //GetCurrentInstanceID
            // Disable events while we make changes to avoid triggering event handler.
            m_disableEvents = true;

            // Update selected district ID.
            m_districtID = WorldInfoPanel.GetCurrentInstanceID().District;
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, $"Selected district is {m_districtID}");

            // create dialog panel
            //ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
            //panel.SetMessage("DistrictHeightMod", $"District ID is {m_districtID}.", false);

            // Set name.
            //m_nameLabel.text = Singleton<DistrictManager>.instance.GetDistrictName(m_districtID);

            // Set min and max levels.
            //m_minHeightDropDown.selectedIndex = Districts.GetDistrictMin(m_districtID, true);
            //m_maxHeightDropDown.selectedIndex = Districts.GetDistrictMax(m_districtID, true);

            // All done: re-enable events.
            m_disableEvents = false;
        }
    }

} // namespace
