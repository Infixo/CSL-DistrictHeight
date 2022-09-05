        /// <summary>
        /// Adds event handler to show/hide building panel as appropriate (in line with ZonedBuildingWorldInfoPanel).
        /// </summary>
        internal static void Hook()
        {
            // Get building info panel instance.
            UIComponent buildingInfoPanel = UIView.library.Get<ZonedBuildingWorldInfoPanel>(typeof(ZonedBuildingWorldInfoPanel).Name)?.component;
            if (buildingInfoPanel == null)
            {
                Logging.Error("couldn't hook building info panel");
            }
            else
            {
                // Toggle button and/or panel visibility when game building info panel visibility changes.
                buildingInfoPanel.eventVisibilityChanged += (control, isVisible) =>
                {
                    // Create / destroy our panel as and when the info panel is shown or hidden.
                    if (isVisible)
                    {
                        if (ModSettings.ShowPanel)
                        {
                            Create();
                        }
                    }
                    else
                    {
                        Close();
                    }
                };
            }
        }

   /// <summary>
    /// Harmony Postfix patches to update ABLC panels when WorldInfoPanel target changes.
    /// </summary>
    [HarmonyPatch]
    internal static class WorldInfoPanelPatches
    {
        /// <summary>
        /// Harmony Postfix patch to update ABLC district info panel when district selection changes.
        /// </summary>
        [HarmonyPatch(typeof(DistrictWorldInfoPanel), "OnSetTarget")]
        [HarmonyPostfix]
        public static void DistrictPostfix()
        {
            DistrictPanelManager.Panel?.DistrictChanged();
        }

        /// <summary>
        /// Harmony Postfix patch to update ABLC building info panel when building selection changes.
        /// </summary>
        [HarmonyPatch(typeof(ZonedBuildingWorldInfoPanel), "OnSetTarget")]
        [HarmonyPostfix]
        public static void BuildingPostfix()
        {
            BuildingPanelManager.TargetChanged();
        }
    }
	

     /// <summary>
        /// Handles a change in target building from the WorldInfoPanel.
        /// Sets the panel button state according to whether or not this building is 'levellable' and communicates changes to the ABLC panel.
        /// </summary>
        internal static void TargetChanged()
        {
            // Get current WorldInfoPanel building instance and determine maximum building level.
            if (LevelUtils.GetMaxLevel(WorldInfoPanel.GetCurrentInstanceID().Building) == 1)
            {
                // Only one building level - not a 'levellable' building, so disable the ABLC button and update the tooltip accordingly.
                s_panelButton.Disable();
                s_panelButton.tooltip = Translations.Translate("ABLC_BUT_DIS");
            }
            else
            {
                // Multiple levels available - enable the ABLC button and update the tooltip accordingly.
                s_panelButton.Enable();
                s_panelButton.tooltip = Translations.Translate("ABLC_NAME");
            }

            // Communicate target change to the panel (if it's currently instantiated).
            Panel?.BuildingChanged();
        }

        public void Start()
        {
            try
            {
                _zonedBuildingWorldInfoPanel = GameObject.Find("(Library) ZonedBuildingWorldInfoPanel").GetComponent<ZonedBuildingWorldInfoPanel>();
                _makeHistoricalPanel = _zonedBuildingWorldInfoPanel.Find("MakeHistoricalPanel").GetComponent<UIPanel>();

                CreateUI();
            }
            catch (Exception e)
            {
                Debug.Log("[Show It!] ModManager:Start -> Exception: " + e.Message);
            }
        }

        private void CreateUI()
        {
            try
            {
                _indicatorsPanel = UIUtils.CreatePanel(_zonedBuildingWorldInfoPanel.component, "ShowItIndicatorsPanel");
                _indicatorsPanel.backgroundSprite = "SubcategoriesPanel";
                _indicatorsPanel.opacity = 0.90f;

                _indicatorsCheckBox = UIUtils.CreateCheckBox(_makeHistoricalPanel, "ShowItIndicatorsCheckBox", "Indicators", ModConfig.Instance.ShowIndicators);
                _indicatorsCheckBox.width = 110f;
                _indicatorsCheckBox.label.textColor = new Color32(185, 221, 254, 255);
                _indicatorsCheckBox.label.textScale = 0.8125f;
                _indicatorsCheckBox.tooltip = "Indicators will show how well serviced the building is and what problems might prevent the building from leveling up.";
                _indicatorsCheckBox.AlignTo(_makeHistoricalPanel, UIAlignAnchor.TopLeft);
                _indicatorsCheckBox.relativePosition = new Vector3(_makeHistoricalPanel.width - _indicatorsCheckBox.width, 6f);
                _indicatorsCheckBox.eventCheckChanged += (component, value) =>
                {
                    _indicatorsPanel.isVisible = value;
                    ModConfig.Instance.ShowIndicators = value;
                    ModConfig.Instance.Save();
                };

                _header = UIUtils.CreateLabel(_indicatorsPanel, "ShowItIndicatorsPanelHeader", "Indicators");
                _header.font = UIUtils.GetUIFont("OpenSans-Regular");
                _header.textAlignment = UIHorizontalAlignment.Center;

			}
		}

        /// <summary>
        /// Adds an ABLC button to a building info panel to open the ABLC panel for that building.
        /// The button will be added to the right of the panel with a small margin from the panel edge, at the relative Y position specified.
        /// </summary>
        internal static void AddInfoPanelButton()
        {
            const float PanelButtonSize = 36f;

            BuildingWorldInfoPanel infoPanel = UIView.library.Get<ZonedBuildingWorldInfoPanel>(typeof(ZonedBuildingWorldInfoPanel).Name);
            s_panelButton = infoPanel.component.AddUIComponent<UIButton>();

            // Basic button setup.
            s_panelButton.atlas = UITextures.LoadQuadSpriteAtlas("ablc_buttons");
            s_panelButton.size = new Vector2(PanelButtonSize, PanelButtonSize);
            s_panelButton.normalFgSprite = "normal";
            s_panelButton.focusedFgSprite = "hovered";
            s_panelButton.hoveredFgSprite = "hovered";
            s_panelButton.pressedFgSprite = "pressed";
            s_panelButton.disabledFgSprite = "disabled";
            s_panelButton.name = "ABLCbutton";
            s_panelButton.tooltip = Translations.Translate("ABLC_NAME");

            // Find ProblemsPanel relative position to position button.
            // We'll use 40f as a default relative Y in case something doesn't work.
            UIComponent problemsPanel;
            float relativeY = 40f;

            // Player info panels have wrappers, zoned ones don't.
            UIComponent wrapper = infoPanel.Find("Wrapper");
            if (wrapper == null)
            {
                problemsPanel = infoPanel.Find("ProblemsPanel");
            }
            else
            {
                problemsPanel = wrapper.Find("ProblemsPanel");
            }

            try
            {
                // Position button vertically in the middle of the problems panel.  If wrapper panel exists, we need to add its offset as well.
                relativeY = (wrapper == null ? 0 : wrapper.relativePosition.y) + problemsPanel.relativePosition.y + ((problemsPanel.height - PanelButtonSize) / 2f);
            }
            catch
            {
                // Don't really care; just use default relative Y.
                Logging.Message("couldn't find ProblemsPanel relative position");
            }

            // Set position.
            s_panelButton.AlignTo(infoPanel.component, UIAlignAnchor.TopLeft);
            s_panelButton.relativePosition += new Vector3(infoPanel.component.width - 62f - PanelButtonSize, relativeY, 0f);

            // Event handler.
            s_panelButton.eventClick += (control, clickEvent) =>
            {
                // Toggle panel visibility.
                if (s_gameObject == null)
                {
                    Create();
                }
                else
                {
                    Close();
                }

                // Manually unfocus control, otherwise it can stay focused until next UI event (looks untidy).
                control.Unfocus();
            };
        }
    }


    [HarmonyPatch(typeof(ZonedBuildingWorldInfoPanel), "UpdateBindings")]
    public static class ZonedBuildingWorldInfoPanelPatch
    {
        // Visitor label reference.
        private static UILabel s_visitLabel;

        /// <summary>
        /// Harmony Postfix patch to ZonedBuildingWorldInfoPanel.UpdateBindings to display visitor counts for commercial buildings.
        /// </summary>
        public static void Postfix()
        {
            // Currently selected building.
            ushort building = WorldInfoPanel.GetCurrentInstanceID().Building;

            // Create visit label if it isn't already set up.
            if (s_visitLabel == null)
            {
                // Get info panel.
                ZonedBuildingWorldInfoPanel infoPanel = UIView.library.Get<ZonedBuildingWorldInfoPanel>(typeof(ZonedBuildingWorldInfoPanel).Name);

                // Add current visitor count label.
                s_visitLabel = UILabels.AddLabel(infoPanel.component, 65f, 280f, Translations.Translate("RPR_INF_VIS"), textScale: 0.75f);
                s_visitLabel.textColor = new Color32(185, 221, 254, 255);
                s_visitLabel.font = Resources.FindObjectsOfTypeAll<UIFont>().FirstOrDefault((UIFont f) => f.name == "OpenSans-Regular");

                // Position under existing Highly Educated workers count row in line with total workplace count label.
                UIComponent situationLabel = infoPanel.Find("WorkSituation");
                UIComponent workerLabel = infoPanel.Find("HighlyEducatedWorkers");
                if (situationLabel != null && workerLabel != null)
                {
                    s_visitLabel.absolutePosition = new Vector2(situationLabel.absolutePosition.x, workerLabel.absolutePosition.y + 25f);
                }
                else
                {
                    Logging.Error("couldn't find ZonedBuildingWorldInfoPanel components");
                }
            }

            // Local references.
            Building[] buildingBuffer = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
            BuildingInfo buildingInfo = buildingBuffer[building].Info;

            // Is this a commercial building?
            CommercialBuildingAI commercialAI = buildingInfo.GetAI() as CommercialBuildingAI;
            if (commercialAI == null)
            {
                // Not a commercial building - hide the label.
                s_visitLabel.Hide();
            }
            else
            {
                // Commercial building - show the label.
                s_visitLabel.Show();

                // Get current visitor count.
                int aliveCount = 0, totalCount = 0;
                Citizen.BehaviourData behaviour = default;
                GetVisitBehaviour(commercialAI, building, ref buildingBuffer[building], ref behaviour, ref aliveCount, ref totalCount);

                // Display visitor count.
                s_visitLabel.text = totalCount.ToString() + " / " + commercialAI.CalculateVisitplaceCount((ItemClass.Level)buildingBuffer[building].m_level, new ColossalFramework.Math.Randomizer(building), buildingBuffer[building].Width, buildingBuffer[building].Length).ToString() + ' ' + Translations.Translate("RPR_INF_VIS");
            }
        }
	}
