

using System;
using System.Collections.Generic;
using Stolen;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;
using UniverseLib.UI.Models;

namespace EasyCheats
{
	public class DemonListWindow
	{
		private const int DemonButtonCount = 12;

		public readonly GameObject WindowGameobject;
		public bool Active => WindowGameobject.active;


		private readonly ButtonRef[] _demonButtons = new ButtonRef[DemonButtonCount];


		public DemonListWindow()
		{
            // Create canvas go + add canvas/canvasscaler components
            GameObject canvasGo = new GameObject("CanvasDemonList");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.referencePixelsPerUnit = 100f;
            canvas.sortingOrder = 999;
            CanvasScaler canvasScaler = canvasGo.AddComponent<CanvasScaler>();
            canvasScaler.referenceResolution = new Vector2(400f, 400f);
            canvasScaler.HandleScaleWithScreenSize();
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();


			// create demonlistwindow, child of canvas gameobject
            WindowGameobject = new GameObject("demonListWindow");
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
            Text text = UIFactory.CreateLabel(titleBar, "TitleBar", "Add demon to stock", TextAnchor.UpperLeft);
            UIFactory.SetLayoutElement(text.gameObject, minWidth:250, minHeight:25, null, flexibleHeight:0);

            GameObject vLayout;
			UIFactory.CreatePanel("demonButtonsVLayout", contentHolder, out vLayout, bgColor:Color.black);
            UIFactory.SetLayoutGroup<VerticalLayoutGroup>(vLayout, true, false, true, true);
            UIFactory.SetLayoutElement(vLayout, 200, +60);


            // create inputfield
            InputFieldRef inputFieldRef = UIFactory.CreateInputField(vLayout, "frames", "Enter a demon's name or ID");
            UIFactory.SetLayoutElement(inputFieldRef.UIRoot, minHeight: 25, minWidth: 70, flexibleWidth: 1, flexibleHeight: 0);
            inputFieldRef.OnValueChanged += OnFilterDemonsSearch;
			inputFieldRef.Component.onValueChanged.AddListener((Action<string>)OnFilterDemonsSearch);
            inputFieldRef.Component.onEndEdit.AddListener((Action<string>)OnFilterDemonsSearch);

			// create buttons
            for (int i = 0; i < DemonButtonCount; ++i)
            {
                _demonButtons[i] = UIFactory.CreateButton(vLayout, $"button{i}", "????");
				_demonButtons[i].ButtonText.alignment = TextAnchor.MiddleLeft;
                UIFactory.SetLayoutElement(_demonButtons[i].GameObject, minHeight: 25, minWidth: 70);
            }
			OnFilterDemonsSearch("");	// refresh displayed demons


            DragPanel dragPanel = new DragPanel(titleBar.GetComponent<RectTransform>(), namedfRect);
            // dragPanel.OnFinishDrag += OnFinishDrag;
		}

		public void SetActive(bool active)
		{
			WindowGameobject.SetActive(active);
		}

        private void OnFilterDemonsSearch(string query)
        {
			List<int> matches = new List<int>();

			query = query.ToLower();
			for (int i = 0; i < Data.allUnitsInOneArray.Length; ++i)
			{
				string demonName = Data.allUnitsInOneArray[i];
				if (query == i.ToString() || demonName.ToLower().Contains(query))
				{
					matches.Add(i);
				}
			}


			for (int i = 0; i < DemonButtonCount; ++i)
			{
				ButtonRef btn = _demonButtons[i];

				// clear all subscribed
				btn.OnClick = null;
				
				// set demon data if possible
				if (i < matches.Count)
				{
					if (i == DemonButtonCount - 1 && matches.Count > DemonButtonCount)
					{
						btn.ButtonText.text = $" Too many demons to display: {matches.Count - DemonButtonCount - 1} more not shown";
						btn.Enabled = true;
					}
					else
					{
						int demonId = matches[i];
						
						string demonButtonText = $" [{demonId:D3}] {Data.allUnitsInOneArray[demonId]}";
						btn.ButtonText.text = demonButtonText;
						btn.OnClick += () => OnClickedDemonButton(demonId);
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

        private void OnClickedDemonButton(int id)
        {
            datCalc.datAddDevil(id, 0);
        }
	}
}