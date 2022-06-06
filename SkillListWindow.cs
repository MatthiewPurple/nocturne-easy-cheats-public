

using System;
using System.Collections.Generic;
using newdata_H;
using Stolen;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;
using UniverseLib.UI.Models;

namespace EasyCheats
{
	public class SkillListWindow
	{
		private const int SkillButtonCount = 12;

		public readonly GameObject WindowGameobject;
		public bool Active => WindowGameobject.active;
		public datUnitWork_t WorkingUnit;


		private readonly ButtonRef[] _skillButtons = new ButtonRef[SkillButtonCount];



		public SkillListWindow()
		{
            // Create canvas go + add canvas/canvasscaler components
            GameObject canvasGo = new GameObject("CanvasSkillList");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.referencePixelsPerUnit = 100f;
            canvas.sortingOrder = 999;
            CanvasScaler canvasScaler = canvasGo.AddComponent<CanvasScaler>();
            canvasScaler.referenceResolution = new Vector2(400f, 400f);
            canvasScaler.HandleScaleWithScreenSize();
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();


			// create SkillListWindow, child of canvas gameobject
            WindowGameobject = new GameObject("SkillListWindow");
            WindowGameobject.transform.SetParent(canvasGo.transform, worldPositionStays: false);
            WindowGameobject.layer = 5;
            RectTransform rectTransform = WindowGameobject.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(400f, 400f);
            WindowGameobject.transform.SetAsFirstSibling();

            GameObject contentHolder;
            GameObject namedf = UIFactory.CreatePanel("namedf", WindowGameobject, out contentHolder, bgColor:Color.black);
            RectTransform namedfRect = namedf.GetComponent<RectTransform>();
            UIFactory.SetLayoutGroup<VerticalLayoutGroup>(namedf, false, false, true, true, 0, 2, 2, 2, 2, TextAnchor.UpperLeft);
            
            UIFactory.SetLayoutGroup<VerticalLayoutGroup>(contentHolder, false, false, true, true, 2, 2, 2, 2, 2, TextAnchor.UpperLeft);
            //forceExpandWidth:false
            GameObject titleBar = UIFactory.CreateHorizontalGroup(contentHolder, "TitleBar", forceExpandWidth: true, forceExpandHeight: true, childControlWidth: true, childControlHeight: true, 2, new Vector4(2f, 2f, 2f, 2f), new Color(0.06f, 0.06f, 0.06f));
            UIFactory.SetLayoutElement(titleBar, null, null, flexibleWidth:1, flexibleHeight:0, preferredWidth:250);
            Text text = UIFactory.CreateLabel(titleBar, "TitleBar", "Add skill to demon", TextAnchor.UpperLeft);
            UIFactory.SetLayoutElement(text.gameObject, minWidth:250, minHeight:25, null, flexibleHeight:0);

            GameObject vLayout;
			UIFactory.CreatePanel("SkillButtonsVLayout", contentHolder, out vLayout, bgColor:Color.black);
            UIFactory.SetLayoutGroup<VerticalLayoutGroup>(vLayout, true, false, true, true);
            UIFactory.SetLayoutElement(vLayout, 200, +60);


            // create inputfield
            InputFieldRef inputFieldRef = UIFactory.CreateInputField(vLayout, "frames", "Enter a skill's name or ID");
            UIFactory.SetLayoutElement(inputFieldRef.UIRoot, minHeight: 25, minWidth: 70, flexibleWidth: 1, flexibleHeight: 0);
            inputFieldRef.OnValueChanged += OnFilterSkillsSearch;
			inputFieldRef.Component.onValueChanged.AddListener((Action<string>)OnFilterSkillsSearch);
            inputFieldRef.Component.onEndEdit.AddListener((Action<string>)OnFilterSkillsSearch);

			// create buttons
            for (int i = 0; i < SkillButtonCount; ++i)
            {
                _skillButtons[i] = UIFactory.CreateButton(vLayout, $"button{i}", "????");
				_skillButtons[i].ButtonText.alignment = TextAnchor.MiddleLeft;
                UIFactory.SetLayoutElement(_skillButtons[i].GameObject, minHeight: 25, minWidth: 70);
            }
			OnFilterSkillsSearch("");	// refresh displayed Skills


            DragPanel dragPanel = new DragPanel(titleBar.GetComponent<RectTransform>(), namedfRect);
            // dragPanel.OnFinishDrag += OnFinishDrag;
		}

		public void SetActive(bool active)
		{
			WindowGameobject.SetActive(active);
		}

        private void OnFilterSkillsSearch(string query)
        {
			List<int> matches = new List<int>();

			query = query.ToLower();
			for (int i = 0; i < Data.allSkillsInOneArray.Length; ++i)
			{
				string SkillName = Data.allSkillsInOneArray[i];
				if (query == i.ToString() || SkillName.ToLower().Contains(query))
				{
					matches.Add(i);
				}
			}


			for (int i = 0; i < SkillButtonCount; ++i)
			{
				ButtonRef btn = _skillButtons[i];

				// clear all subscribed
				btn.OnClick = null;
				
				// set Skill data if possible
				if (i < matches.Count)
				{
					if (i == SkillButtonCount - 1 && matches.Count > SkillButtonCount)
					{
						btn.ButtonText.text = $" Too many Skills to display: {matches.Count - SkillButtonCount - 1} more not shown";
						btn.Enabled = true;
					}
					else
					{
						int SkillId = matches[i];
						
						string SkillButtonText = $" [{SkillId:D3}] {Data.allSkillsInOneArray[SkillId]}";
						btn.ButtonText.text = SkillButtonText;
						btn.OnClick += () => OnClickedSkillButton(SkillId);
						btn.Enabled = true;
					}
				}
				else
				{
					btn.ButtonText.text = "";
					btn.Enabled = false;
				}
			}
        }

        private void OnClickedSkillButton(int id)
        {
            fclCombineCalcCore.cmbAddSkill((ushort)id, WorkingUnit);
        }
	}
}