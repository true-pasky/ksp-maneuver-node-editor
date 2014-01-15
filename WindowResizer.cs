using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace amne
{
	public class WindowResizer
	{		
		bool resizing = false;
		Vector2 startSize;
		Vector2 startMouse;

		public Vector2 ActiveSize = new Vector2(13, 13);
		public bool Resizing
		{
			get { return resizing; }
			private set
			{
				Debug.Log(resizing.ToString() + " " + startSize.ToString() + " " + startMouse.ToString());
				resizing = value;
			}
		}
		public WindowResizer()
		{

		}

		public Vector2 ResizeWindow(Vector2 pos, Vector2 size, Vector2 lastSize)
		{
			Vector2 mouse = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
			var rect = Utils.MakeRect(pos + lastSize - ActiveSize, ActiveSize);
			//Debug.Log("!" + pos.ToString() + " " + size.ToString() + " " + lastSize.ToString() + " " + mouse.ToString() + " " + rect.ToString());
			if (resizing)
			{
				if (Input.GetMouseButtonUp(0))
				{
					resizing = false;
				}
				else
				{
					if (Event.current.type == EventType.Layout)
					{
						size.x = Math.Max(size.x, startSize.x + (mouse.x - startMouse.x));
						size.y = Math.Max(size.y, startSize.y + (mouse.y - startMouse.y));
					}
					else
					{
						size.x = startSize.x + (mouse.x - startMouse.x);
						size.y = startSize.y + (mouse.y - startMouse.y);
					}
				}
			}
			else
			{
				if (Event.current.type == EventType.MouseDown)
				{
					if (rect.Contains(mouse))
					{
						startMouse = mouse;
						startSize = size;
						resizing = true;
						Event.current.Use();
					}
				}
			}
			return size;
		}
	}
}
