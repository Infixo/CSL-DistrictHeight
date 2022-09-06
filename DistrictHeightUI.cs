using System;
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

    /*
     * The design is very simple:
     * a) label with generic info
     * b) button to apply heights to all buildings at once
     * c) minimum height dropdown
     * d) maximum height dropdown
     * */
    public static class DistrictHeightUI
    {
        //private static readonly Color32 COLOR_WHITE = new Color32(255, 255, 255, 255);
        //private static readonly Color32 COLOR_BLACK = new Color32(0, 0, 0, 0);
        private static readonly Color32 COLOR_GREY = new Color32(170, 170, 170, 255);
        private static readonly Color32 COLOR_GREEN = new Color32(206, 248, 0, 255);
        private static readonly Color32 COLOR_NORMAL = new Color32(185, 221, 254, 255);
        private static readonly float SCALE_SMALL  = 0.6250f; // 50/80
        private static readonly float SCALE_MEDIUM = 0.8125f; // 65/80
        private static readonly int ITEM_HEIGHT = 16;
        //public static DistrictWorldInfoPanel m_districtPanel;
        private static readonly float POSX = 250.0f;
        private static readonly float POSY = 320.0f;
        public static UILabel m_minLabel;
        public static UILabel m_maxLabel;
        //public static UILabel m_mmmLabel;
        public static UIPanel m_minPanel;
        public static UIPanel m_maxPanel;
        public static UIDropDown m_minDropdown;
        public static UIDropDown m_maxDropdown;
        public static UILabel m_heightLabel;
        public static UIButton m_applyButton;
        //private static bool m_disableEvents;
        private static byte m_districtID; // currently selected district

        public static void Start(DistrictWorldInfoPanel districtPanel)
        {
            //m_districtPanel = districtPanel;

            // move existing elements to make some space
            // UIPanel zonePanel = districtPanel.Find<UIPanel>("ZonePanel");
            // for some reason this panel canot be moved... :(
            // so I will move RICO bars separately
            void moveSpriteToLeft(UISprite sprite)
            {
                sprite.relativePosition = new Vector2(0f, sprite.relativePosition.y);
            }
            moveSpriteToLeft(districtPanel.Find<UISprite>("ResidentialThumb"));
            moveSpriteToLeft(districtPanel.Find<UISprite>("CommercialThumb"));
            moveSpriteToLeft(districtPanel.Find<UISprite>("IndustrialThumb"));
            moveSpriteToLeft(districtPanel.Find<UISprite>("OfficeThumb"));
            /*
            UISprite resSprite = districtPanel.Find<UISprite>("ResidentialThumb");
            resSprite.relativePosition = new Vector2(0f, resSprite.relativePosition.y);
            UISprite comSprite = districtPanel.Find<UISprite>("CommercialThumb");
            comSprite.relativePosition = new Vector2(0f, comSprite.relativePosition.y);
            UISprite indSprite = districtPanel.Find<UISprite>("IndustrialThumb");
            indSprite.relativePosition = new Vector2(0f, indSprite.relativePosition.y);
            UISprite offSprite = districtPanel.Find<UISprite>("OfficeThumb");
            offSprite.relativePosition = new Vector2(0f, offSprite.relativePosition.y);
            */
            UIRadialChart zoneChart = districtPanel.Find<UIRadialChart>("ZoneChart");
            zoneChart.relativePosition = new Vector2(250f, 30f); // this is working...
            zoneChart.height = 60;
            zoneChart.width = 60;

            // LABEL district height
            m_heightLabel = districtPanel.component.AddUIComponent<UILabel>();
            m_heightLabel.name = "HeightLabel";
            m_heightLabel.text = "Height [m]";
            m_heightLabel.textScale = SCALE_SMALL;
            m_heightLabel.relativePosition = new Vector2(POSX, POSY-15f);
            m_heightLabel.textColor = COLOR_GREEN;
            m_heightLabel.tooltip = "Controls the minimum and maximum height (in meters) of buildings that will spawn in the district";

            // DROPDOWN min
            m_minPanel = districtPanel.component.AttachUIComponent(UITemplateManager.GetAsGameObject("OptionsDropdownTemplate")) as UIPanel;
            m_minPanel.relativePosition = new Vector2(POSX, POSY);
            m_minPanel.height = 50; // default is 72 and it covers Style dropdown
            //m_minPanel.autoLayout = false; // turning off allows to place label anywhere
            m_minDropdown = m_minPanel.Find<UIDropDown>("Dropdown");
            m_minDropdown.name = "HeightMinDropdown";
            m_minDropdown.autoSize = false;
            m_minDropdown.size = new Vector2(65f, 20f);
            m_minDropdown.textScale = SCALE_MEDIUM;
            m_minDropdown.textColor = COLOR_NORMAL;
            m_minDropdown.disabledTextColor = COLOR_GREY;
            m_minDropdown.itemHeight = ITEM_HEIGHT;
            m_minDropdown.textFieldPadding = new RectOffset(5, 0, 4, 0); // default is l=14, so big gap
            m_minDropdown.itemPadding = new RectOffset(5, 5, 2, 0); // default is l=14
            // LABEL min
            m_minLabel = m_minPanel.Find<UILabel>("Label");
            //m_minLabel = districtPanel.component.AddUIComponent<UILabel>();
            m_minLabel.name = "HeightMinLabel";
            m_minLabel.text = "Min";
            m_minLabel.textScale = SCALE_SMALL;
            //m_minLabel.relativePosition = new Vector2(POSX + 0f, POSY + 5f); // autoLayout must be turned off first
            m_minLabel.textColor = COLOR_GREEN;

            // PANEL max
            m_maxPanel = districtPanel.component.AttachUIComponent(UITemplateManager.GetAsGameObject("OptionsDropdownTemplate")) as UIPanel;
            m_maxPanel.relativePosition = new Vector2(POSX + 70f, POSY);
            m_maxPanel.height = 50;
            // DROPDOWN max
            //m_maxPanel.autoLayout = false;
            m_maxDropdown = m_maxPanel.Find<UIDropDown>("Dropdown");
            m_maxDropdown.name = "HeightMaxDropdown";
            m_maxDropdown.autoSize = false;
            m_maxDropdown.size = m_minDropdown.size;
            m_maxDropdown.textScale = SCALE_MEDIUM;
            m_maxDropdown.textColor = COLOR_NORMAL;
            m_maxDropdown.disabledTextColor = COLOR_GREY;
            m_maxDropdown.itemHeight = ITEM_HEIGHT;
            m_maxDropdown.textFieldPadding = m_minDropdown.textFieldPadding;
            m_maxDropdown.itemPadding = m_minDropdown.itemPadding;
            // LABEL max
            m_maxLabel = m_maxPanel.Find<UILabel>("Label");
            //m_maxLabel = districtPanel.component.AddUIComponent<UILabel>();
            m_maxLabel.name = "HeightMaxLabel";
            m_maxLabel.text = "Max";
            m_maxLabel.textScale = SCALE_SMALL;
            //m_maxLabel.relativePosition = new Vector2(POSX + 75f, POSY + 5f);
            m_maxLabel.textColor = COLOR_GREEN;

            // dropdowns initialization
            m_minDropdown.items = new string[DistrictHeightManager.MinList.Length];
            for (int i = 0; i < DistrictHeightManager.MinList.Length; i++)
                m_minDropdown.items[i] = DistrictHeightManager.MinList[i].ToString("F");
            m_maxDropdown.items = new string[DistrictHeightManager.MaxList.Length];
            for (int i = 0; i < DistrictHeightManager.MaxList.Length; i++)
                m_maxDropdown.items[i] = DistrictHeightManager.MaxList[i].ToString("F");
            m_maxDropdown.items[0] = "--"; // this means 999
            m_minDropdown.selectedIndex = 0;
            m_maxDropdown.selectedIndex = 0;
            m_minDropdown.listHeight = m_minDropdown.items.Length * ITEM_HEIGHT + 9;
            m_maxDropdown.listHeight = m_maxDropdown.items.Length * ITEM_HEIGHT + 9;

            // BUTTON
            m_applyButton = districtPanel.component.AddUIComponent<UIButton>();
            m_applyButton.name = "HeightApplyButton";
            m_applyButton.relativePosition = new Vector2(POSX+85f, POSY-23f);
            m_applyButton.size = new Vector2(50f, 20f);
            m_applyButton.normalBgSprite = "ButtonMenu";
            m_applyButton.hoveredBgSprite = "ButtonMenuHovered";
            m_applyButton.pressedBgSprite = "ButtonMenuPressed";
            m_applyButton.disabledBgSprite = "ButtonMenuDisabled";
            m_applyButton.disabledTextColor = new Color32(128, 128, 128, 255);
            m_applyButton.canFocus = false;
            m_applyButton.text = "Apply";
            m_applyButton.tooltip = "refresh buildings";
            m_applyButton.textScale = SCALE_SMALL;
            m_applyButton.textVerticalAlignment = UIVerticalAlignment.Middle;
            m_applyButton.textHorizontalAlignment = UIHorizontalAlignment.Center;

            // event handlers
            m_minDropdown.eventSelectedIndexChanged += (UIComponent control, int index) =>
            {
                float minH = DistrictHeightManager.MinList[index];
                DistrictHeightManager.Min[m_districtID] = minH;
                //MessageBox($"Minimum height for district {m_districtID} set to {minH}");
            };

            m_maxDropdown.eventSelectedIndexChanged += (UIComponent control, int index) =>
            {
                float maxH = DistrictHeightManager.MaxList[index];
                DistrictHeightManager.Max[m_districtID] = maxH;
                //MessageBox($"Maximum height for district {m_districtID} set to {maxH}");
            };

            m_applyButton.eventClick += (control, clickEvent) =>
            {
                // Local references for SimulationManager action.
                //ushort buildingID = m_targetID;
                //byte targetLevel = _upgradeLevel;
                //Logging.KeyMessage("upgrading building to level ", _upgradeLevel);
                //Singleton<SimulationManager>.instance.AddAction(() =>
                //{
                //    LevelUtils.ForceLevel(m_targetID, targetLevel);
                //});
                ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
                panel.SetMessage("District Height", $"District ID is {m_districtID}. Min = {DistrictHeightManager.Min[m_districtID]}, Max = {DistrictHeightManager.Max[m_districtID]}", false);
            };


        }

        /// <summary>
        /// Displays a message box
        /// </summary>
        /// <param name="text">Text to be shown</param>
        //public static void MessageBox(string text)
        //{
            //ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
            //panel.SetMessage("District Height Mod", text, false);
        //}

        /// <summary>
        /// Called when the selected district has changed.
        /// </summary>
        public static void DistrictChanged(DistrictWorldInfoPanel districtPanel)
        {
            // Update selected district ID.
            m_districtID = WorldInfoPanel.GetCurrentInstanceID().District;

            // minimum height
            float minH = DistrictHeightManager.Min[m_districtID];
            if (minH == 0f)
                m_minDropdown.selectedIndex = 0;
            else
            {
                int idx = Array.IndexOf(DistrictHeightManager.MinList, minH);
                if (idx >= 0)
                    m_minDropdown.selectedIndex = idx;
                else
                {
                    Debug.Log("element min not found, resetting");
                    m_minDropdown.selectedIndex = 0;
                }
            }

            // maximum height
            float maxH = DistrictHeightManager.Max[m_districtID];
            if (maxH == 0f)
                m_maxDropdown.selectedIndex = 0;
            else
            {
                int idx = Array.IndexOf(DistrictHeightManager.MaxList, maxH);
                if (idx >= 0)
                    m_maxDropdown.selectedIndex = idx;
                else
                {
                    Debug.Log("element max not found, resetting");
                    m_maxDropdown.selectedIndex = 0;
                }
            }

            DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, $"Selected district is {m_districtID}. Min height is {minH}, max height is {maxH}");
        }
    }

} // namespace
