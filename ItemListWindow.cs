

using System;
using System.Collections.Generic;
using Stolen;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;
using UniverseLib.UI.Models;

namespace EasyCheats
{
	public class ItemListWindow
	{
		private const int ItemButtonCount = 11;

		public readonly GameObject WindowGameobject;
		public bool Active => WindowGameobject.active;


		private readonly ButtonRef[] _itemButtons = new ButtonRef[ItemButtonCount];
		private readonly InputFieldRef _quantityField;


		public ItemListWindow()
		{
            // Create canvas go + add canvas/canvasscaler components
            GameObject canvasGo = new GameObject("CanvasItemList");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.referencePixelsPerUnit = 100f;
            canvas.sortingOrder = 999;
            CanvasScaler canvasScaler = canvasGo.AddComponent<CanvasScaler>();
            canvasScaler.referenceResolution = new Vector2(400f, 400f);
            canvasScaler.HandleScaleWithScreenSize();
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();


			// create ItemListWindow, child of canvas gameobject
            WindowGameobject = new GameObject("ItemListWindow");
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
            Text text = UIFactory.CreateLabel(titleBar, "TitleBar", "Add item to inventory", TextAnchor.UpperLeft);
            UIFactory.SetLayoutElement(text.gameObject, minWidth:250, minHeight:25, null, flexibleHeight:0);

            GameObject vLayout;
			UIFactory.CreatePanel("ItemButtonsVLayout", contentHolder, out vLayout, bgColor:Color.black);
            UIFactory.SetLayoutGroup<VerticalLayoutGroup>(vLayout, true, false, true, true);
            UIFactory.SetLayoutElement(vLayout, 200, +60);


            // create inputfield
            InputFieldRef inputFieldRef = UIFactory.CreateInputField(vLayout, "frames", "Enter a Item's name or ID");
            UIFactory.SetLayoutElement(inputFieldRef.UIRoot, minHeight: 25, minWidth: 70, flexibleWidth: 1, flexibleHeight: 0);
            inputFieldRef.OnValueChanged += OnFilterItemsSearch;
			inputFieldRef.Component.onValueChanged.AddListener((Action<string>)OnFilterItemsSearch);
            inputFieldRef.Component.onEndEdit.AddListener((Action<string>)OnFilterItemsSearch);

			_quantityField = UIFactory.CreateInputField(vLayout, "frames", "Enter the desired quantity");
			UIFactory.SetLayoutElement(inputFieldRef.UIRoot, minHeight: 25, minWidth: 70, flexibleWidth: 1, flexibleHeight: 0);

			// create buttons
            for (int i = 0; i < ItemButtonCount; ++i)
            {
                _itemButtons[i] = UIFactory.CreateButton(vLayout, $"button{i}", "????");
				_itemButtons[i].ButtonText.alignment = TextAnchor.MiddleLeft;
                UIFactory.SetLayoutElement(_itemButtons[i].GameObject, minHeight: 25, minWidth: 70);
            }
			OnFilterItemsSearch("");	// refresh displayed Items


            DragPanel dragPanel = new DragPanel(titleBar.GetComponent<RectTransform>(), namedfRect);
            // dragPanel.OnFinishDrag += OnFinishDrag;
		}

		public void SetActive(bool active)
		{
			WindowGameobject.SetActive(active);
		}

        private void OnFilterItemsSearch(string query)
        {
			List<int> matches = new List<int>();

			query = query.ToLower();
			for (int i = 0; i < Data.allItemsInOneArray.Length; ++i)
			{
				string ItemName = Data.allItemsInOneArray[i];
				if (query == i.ToString() || ItemName.ToLower().Contains(query))
				{
					matches.Add(i);
				}
			}


			for (int i = 0; i < ItemButtonCount; ++i)
			{
				ButtonRef btn = _itemButtons[i];

				// clear all subscribed
				btn.OnClick = null;
				
				// set Item data if possible
				if (i < matches.Count)
				{
					if (i == ItemButtonCount - 1 && matches.Count > ItemButtonCount)
					{
						btn.ButtonText.text = $" Too many items to display: {matches.Count - ItemButtonCount - 1} more not shown";
						btn.Enabled = true;
					}
					else
					{
						int ItemId = matches[i];
						
						string ItemButtonText = $" [{ItemId:D3}] {Data.allItemsInOneArray[ItemId]}";
						btn.ButtonText.text = ItemButtonText;
						btn.OnClick += () => OnClickedItemButton(ItemId);
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

        private void OnClickedItemButton(int id)
        {
			string quantityText = _quantityField.Text.Trim(' ');
			int quantity = 1;

			if (string.IsNullOrWhiteSpace(quantityText) || int.TryParse(_quantityField.Text, out quantity))
			{
            	datCalc.datAddItem(id, quantity);
			}
			else
			{
				_quantityField.Text = "";
			}
        }
	}
}