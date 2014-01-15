using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace amne
{
	public class DynSlidersDrawer
	{
		int nextSliderIndex;
		int activeSliderIndex;
		double activeSliderValue;
		DateTime activeLastTime;

		enum State
		{
			NoActive,
			WillHasActive,
			HasActive,
			DisabledUntilMouseUp,
		}
		State state;

		public DynSlidersDrawer()
		{
			Reset();
		}

		public void Reset()
		{
			state = State.NoActive;
		}
		public void BeginDraw()
		{
			nextSliderIndex = 0;

			if (Input.GetMouseButtonUp(0))
			{
				state = State.NoActive;
			}

			if (state == State.NoActive)
			{
			}
			else if (state == State.WillHasActive)
			{
				state = State.HasActive;
			}
			else if (state == State.HasActive)
			{
			}
			else if (state == State.DisabledUntilMouseUp)
			{

			}
			else
			{
				Utils.Wtf("bad state " + state.ToString());
			}
		}
		public double DrawDynSlider(double value, IDeltaFunction deltaFunction = null, params GUILayoutOption[] options)
		{
			if (deltaFunction == null)
			{
				deltaFunction = IdentityDeltaFunction.Instance;
			}
			if (state == State.NoActive)
			{
				var newSliderValue = drawSlider(0, options);
				if (newSliderValue != 0)
				{
					activeSliderIndex = nextSliderIndex;
					activeLastTime = DateTime.UtcNow;
					activeSliderValue = newSliderValue;
					state = State.WillHasActive;
					value += getDelta(newSliderValue);
				}
			}
			else if (state == State.WillHasActive)
			{
				drawZeroSlider();
			}
			else if (state == State.HasActive)
			{
				if (activeSliderIndex == nextSliderIndex)
				{
					var newSliderValue = drawSlider(activeSliderValue, options);
					var now = DateTime.UtcNow;
					var timeDelta = (now - activeLastTime).TotalSeconds;
					var prev = deltaFunction.GetDelta(activeSliderValue);
					var cur = deltaFunction.GetDelta(newSliderValue);
					value += getDelta(prev, cur, timeDelta);
					activeLastTime = now;
					activeSliderValue = newSliderValue;
				}
				else
				{
					drawZeroSlider();
				}
			}
			else if (state == State.DisabledUntilMouseUp)
			{
				drawDisabledSlider();
			}
			else
			{
				Utils.Wtf("bad state " + state.ToString());
			}
			nextSliderIndex++;
			return value;
		}

		double getDelta(double prev, double cur, double timeDelta)
		{
			return (prev + cur) / 2 * timeDelta;
		}
		double getDelta(double t)
		{
			return 0;
		}

		double drawSlider(double value, params GUILayoutOption[] options)
		{
			return (double)GUILayout.HorizontalSlider((float)value, -1, 1, options);
		}
		void drawZeroSlider(params GUILayoutOption[] options)
		{
			var newSliderValue = drawSlider(0, options);
			if (newSliderValue != 0)
			{
				Utils.Wtf(string.Format("slider at index {0} returned {1} instead of 0", nextSliderIndex, newSliderValue));
			}
		}

		void drawDisabledSlider(params GUILayoutOption[] options)
		{
			var enabled = GUI.enabled;
			GUI.enabled = false;
			drawSlider(0, options);
			GUI.enabled = enabled;
		}
		public void DisableUntilMouseUp()
		{
			state = State.DisabledUntilMouseUp;
		}
	}
}
