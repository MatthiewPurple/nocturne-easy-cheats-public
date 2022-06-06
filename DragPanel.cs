
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Stolen
{
	public class DragPanel
	{
		private enum MouseState
		{
			Down,
			Held,
			NotPressed
		}

		[Flags]
		public enum ResizeTypes : ulong
		{
			NONE = 0uL,
			Top = 1uL,
			Left = 2uL,
			Right = 4uL,
			Bottom = 8uL,
			TopLeft = 3uL,
			TopRight = 5uL,
			BottomLeft = 0xAuL,
			BottomRight = 0xCuL
		}

		internal static List<DragPanel> Instances = new List<DragPanel>();

		private static bool handledInstanceThisFrame;

		public static GameObject s_resizeCursorObj;

		internal static bool wasAnyDragging;

		private readonly RectTransform canvasTransform;

		private Vector2 m_lastDragPosition;

		private const int RESIZE_THICKNESS = 10;

		private ResizeTypes m_currentResizeType = ResizeTypes.NONE;

		private Vector2 m_lastResizePos;

		private ResizeTypes m_lastResizeHoverType;

		private Rect m_totalResizeRect;

		private readonly Dictionary<ResizeTypes, Rect> m_resizeMask = new Dictionary<ResizeTypes, Rect>
		{
			{
				ResizeTypes.Top,
				default(Rect)
			},
			{
				ResizeTypes.Left,
				default(Rect)
			},
			{
				ResizeTypes.Right,
				default(Rect)
			},
			{
				ResizeTypes.Bottom,
				default(Rect)
			}
		};

		private const int DBL_THICKESS = 20;

		public static bool Resizing { get; private set; }

		public static bool ResizePrompting => (bool)s_resizeCursorObj && s_resizeCursorObj.activeSelf;

		public bool AllowDragAndResize => true;

		public RectTransform Panel { get; set; }

		public RectTransform DragableArea { get; set; }

		public bool WasDragging { get; set; }

		private bool WasResizing { get; set; }

		private bool WasHoveringResize => s_resizeCursorObj.activeInHierarchy;

		public event Action<RectTransform> OnFinishResize;

		public event Action<RectTransform> OnFinishDrag;

		internal static void ForceEnd()
		{
			s_resizeCursorObj.SetActive(value: false);
			wasAnyDragging = false;
			Resizing = false;
		}

		public static void UpdateInstances()
		{
			if (!s_resizeCursorObj)
			{
				CreateCursorUI();
			}
			MouseState mouseState = ((!Input.GetMouseButtonDown(0)) ? (Input.GetMouseButton(0) ? MouseState.Held : MouseState.NotPressed) : MouseState.Down);
			Vector3 mousePosition = Input.mousePosition;
			handledInstanceThisFrame = false;
			foreach (DragPanel instance in Instances)
			{
				instance.Update(mouseState, mousePosition);
			}
			if (wasAnyDragging && mouseState == MouseState.NotPressed)
			{
				wasAnyDragging = false;
			}
		}

		public DragPanel(RectTransform dragArea, RectTransform panelToDrag)
		{
			DragableArea = dragArea;
			Panel = panelToDrag;
			if (!canvasTransform)
			{
				canvasTransform = Panel.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
			}
			Instances.Add(this);
			UpdateResizeCache();
		}

		public void Destroy()
		{
			if ((bool)s_resizeCursorObj)
			{
				UnityEngine.Object.Destroy(s_resizeCursorObj);
			}
		}

		private void Update(MouseState state, Vector3 rawMousePos)
		{
			if (DragableArea == null)
			{
				return;
			}
			Vector3 point = DragableArea.InverseTransformPoint(rawMousePos);
			bool flag = DragableArea.rect.Contains(point);
			switch (state)
			{
			case MouseState.Down:
				if (flag)
				{
					if (AllowDragAndResize)
					{
						OnBeginDrag();
					}
					handledInstanceThisFrame = true;
				}
				break;
			case MouseState.Held:
				if (WasDragging)
				{
					OnDrag();
					handledInstanceThisFrame = true;
				}
				else if (WasResizing)
				{
					OnResize();
					handledInstanceThisFrame = true;
				}
				break;
			case MouseState.NotPressed:
				if (WasDragging)
				{
					OnEndDrag();
				}
				break;
			}
		}

		public void OnBeginDrag()
		{
			wasAnyDragging = true;
			WasDragging = true;
			m_lastDragPosition = Input.mousePosition;
		}

		public void OnDrag()
		{
			Vector3 mousePosition = Input.mousePosition;
			Vector2 vector = (Vector2)mousePosition - m_lastDragPosition;
			m_lastDragPosition = mousePosition;
			Panel.localPosition += (Vector3)vector;
		}

		public void OnEndDrag()
		{
			WasDragging = false;
			this.OnFinishDrag?.Invoke(Panel);
		}

		private void UpdateResizeCache()
		{
			m_totalResizeRect = new Rect(Panel.rect.x - 10f + 1f, Panel.rect.y - 10f + 1f, Panel.rect.width + 20f - 2f, Panel.rect.height + 20f - 2f);
			if (AllowDragAndResize)
			{
				m_resizeMask[ResizeTypes.Bottom] = new Rect(m_totalResizeRect.x, m_totalResizeRect.y, m_totalResizeRect.width, 10f);
				m_resizeMask[ResizeTypes.Left] = new Rect(m_totalResizeRect.x, m_totalResizeRect.y, 10f, m_totalResizeRect.height);
				m_resizeMask[ResizeTypes.Top] = new Rect(m_totalResizeRect.x, Panel.rect.y + Panel.rect.height - 2f, m_totalResizeRect.width, 10f);
				m_resizeMask[ResizeTypes.Right] = new Rect(m_totalResizeRect.x + Panel.rect.width + 10f - 2f, m_totalResizeRect.y, 10f, m_totalResizeRect.height);
			}
		}

		private bool MouseInResizeArea(Vector2 mousePos)
		{
			return m_totalResizeRect.Contains(mousePos);
		}

		private ResizeTypes GetResizeType(Vector2 mousePos)
		{
			int num = 0;
			num |= (m_resizeMask[ResizeTypes.Top].Contains(mousePos) ? 1 : 0);
			num |= 8 * (m_resizeMask[ResizeTypes.Bottom].Contains(mousePos) ? 1 : 0);
			num |= 2 * (m_resizeMask[ResizeTypes.Left].Contains(mousePos) ? 1 : 0);
			num |= 4 * (m_resizeMask[ResizeTypes.Right].Contains(mousePos) ? 1 : 0);
			return (ResizeTypes)num;
		}

		public void OnHoverResize(ResizeTypes resizeType)
		{
			if (WasHoveringResize && m_lastResizeHoverType == resizeType)
			{
				return;
			}
			m_lastResizeHoverType = resizeType;
			s_resizeCursorObj.SetActive(value: true);
			s_resizeCursorObj.transform.SetAsLastSibling();
			float z = 0f;
			ResizeTypes num = resizeType - 1;
			if (num <= ResizeTypes.Right)
			{
				switch (num)
				{
				case ResizeTypes.Right:
					goto IL_009d;
				case ResizeTypes.NONE:
					goto IL_00a5;
				case ResizeTypes.Left:
					goto IL_00ad;
				case ResizeTypes.Top:
				case ResizeTypes.TopLeft:
					goto IL_00b5;
				}
			}
			ResizeTypes num2 = resizeType - 8;
			if (num2 > ResizeTypes.Right)
			{
				goto IL_00b5;
			}
			switch (num2)
			{
			case ResizeTypes.Left:
				break;
			case ResizeTypes.NONE:
				goto IL_00a5;
			case ResizeTypes.Right:
				goto IL_00ad;
			default:
				goto IL_00b5;
			}
			goto IL_009d;
			IL_00ad:
			z = 135f;
			goto IL_00b5;
			IL_00b5:
			Quaternion rotation = s_resizeCursorObj.transform.rotation;
			rotation.eulerAngles = new Vector3(0f, 0f, z);
			s_resizeCursorObj.transform.rotation = rotation;
			UpdateHoverImagePos();
			return;
			IL_009d:
			z = 45f;
			goto IL_00b5;
			IL_00a5:
			z = 90f;
			goto IL_00b5;
		}

		private void UpdateHoverImagePos()
		{
			s_resizeCursorObj.transform.localPosition = canvasTransform.InverseTransformPoint(Input.mousePosition);
		}

		public void OnHoverResizeEnd()
		{
			s_resizeCursorObj.SetActive(value: false);
		}

		public void OnBeginResize(ResizeTypes resizeType)
		{
			m_currentResizeType = resizeType;
			m_lastResizePos = Input.mousePosition;
			WasResizing = true;
			Resizing = true;
		}

		public void OnResize()
		{
			Vector3 mousePosition = Input.mousePosition;
			Vector2 vector = m_lastResizePos - (Vector2)mousePosition;
			if (!((Vector2)mousePosition == m_lastResizePos) && !(mousePosition.x < 0f) && !(mousePosition.y < 0f) && !(mousePosition.x > (float)Screen.width) && !(mousePosition.y > (float)Screen.height))
			{
				m_lastResizePos = mousePosition;
				float num = (float)((decimal)vector.x / (decimal)Screen.width);
				float num2 = (float)((decimal)vector.y / (decimal)Screen.height);
				Vector2 anchorMin = Panel.anchorMin;
				Vector2 anchorMax = Panel.anchorMax;
				if (m_currentResizeType.HasFlag(ResizeTypes.Left))
				{
					anchorMin.x -= num;
				}
				else if (m_currentResizeType.HasFlag(ResizeTypes.Right))
				{
					anchorMax.x -= num;
				}
				if (m_currentResizeType.HasFlag(ResizeTypes.Top))
				{
					anchorMax.y -= num2;
				}
				else if (m_currentResizeType.HasFlag(ResizeTypes.Bottom))
				{
					anchorMin.y -= num2;
				}
				Vector2 anchorMin2 = Panel.anchorMin;
				Vector2 anchorMax2 = Panel.anchorMax;
				Panel.anchorMin = new Vector2(anchorMin.x, anchorMin.y);
				Panel.anchorMax = new Vector2(anchorMax.x, anchorMax.y);
			}
		}

		public void OnEndResize()
		{
			WasResizing = false;
			Resizing = false;
			try
			{
				OnHoverResizeEnd();
			}
			catch
			{
			}
			UpdateResizeCache();
			this.OnFinishResize?.Invoke(Panel);
		}

		internal static void CreateCursorUI()
		{
		}
	}
}