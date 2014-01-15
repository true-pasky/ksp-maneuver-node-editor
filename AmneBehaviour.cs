using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MuMech;
using System.Diagnostics;


namespace amne
{
	public abstract class AmneBehaviour : MonoBehaviour
	{
		protected abstract string StartupName { get; }
		protected abstract KSPAddon.Startup Startup { get; }
		protected abstract bool InFlight { get; }

		Vector2 scroll = Vector2.zero;
		Vector2 mainWindowPos = new Vector2(100, 100);
		Vector2 mainWindowSize = new Vector2(400, 400);
		Vector2 mainWindowSizeAfterLayout = Vector2.zero;

		bool nullSkin = true;

		IDeltaFunction timeDeltaFunction = new PowerDeltaFunction(0.1, 0.1, 1, 100000);
		IDeltaFunction speedDeltaFunction = new PowerDeltaFunction(0.1, 0.1, 1, 1000);

		public void Awake()
		{
			//DontDestroyOnLoad(this);			
			CancelInvoke();
			DebugInitNodes();
		}

		[Conditional("DEBUG")]
		void DebugInitNodes()
		{
			if (!InFlight)
			{
				state.Nodes.Add(new NodeState(1, 2, 3, 123));
				state.Nodes.Add(new NodeState(-10, -20, -30, 92345));
				state.Nodes.Add(new NodeState(-10, -20, -300000, 9345));
				state.Nodes.Add(new NodeState(-10, -20, -300000, 9345));
				state.Nodes.Add(new NodeState(-10, -20, -300000, 9345));
			}
		}

		int onguiid = 0;
		int constructorid = 0;
		bool inconstructor = false;
		void log(params object[] args)
		{
			var newargs = new object[] { Event.current.type, inconstructor ? "constructor" : "ongui", onguiid, constructorid };
			var n = newargs.Length;
			Array.Resize(ref newargs, n + args.Length);
			for (var i = 0; i < args.Length; i++)
			{
				newargs[n + i] = args[i];
			}
			_log(newargs);
		}
		void _log(params object[] args)
		{
			var s = "";
			foreach (var item in args)
			{
				if (s.Length > 0)
				{
					s += " ";
				}
				if (item == null)
				{
					s += "null";
				}
				else
				{
					s += item.ToString();
				}
			}
			print("! " + s);
		}
		public void OnGUI()
		{
			inconstructor = false;
			onguiid++;

			SetSkin();

			DrawMainWindow();
		}

		string GetMainWindowTitle()
		{
			if (wr.Resizing)
			{
				return string.Format("[{0:0}x{1:0}]", mainWindowSize.x, mainWindowSize.y);
			}
			else if (lastToolTip.Length > 0)
			{
				return lastToolTip;
			}
			else
			{
				return "Maneuver Nodes";
			}
		}

		void DrawMainWindow()
		{
			Vector2 size;
			if (Event.current.type == EventType.Repaint)
			{
				size = mainWindowSizeAfterLayout;
			}
			else
			{
				if (Enabled)
				{
					size = mainWindowSize;
				}
				else
				{
					size = new Vector2(1, 1);
				}
			}

			var title = GetMainWindowTitle();
			var style = new GUIStyle(GUI.skin.window);
			var rect = Utils.MakeRect(mainWindowPos, size);

			var newRect = GUILayout.Window(1337111, rect, MainWindowConstructor, title, style);

			mainWindowPos = Utils.GetPos(newRect);
			var newSize = Utils.GetSize(newRect);

			mainWindowSizeAfterLayout = newSize;
			if (updSize)
			{
				updSize = false;
				mainWindowSize = mainWindowSizeAfterLayout;
			}
		}

		void SetSkin()
		{
			if (nullSkin)
			{
				GUI.skin = null;
			}
			else
			{
				GUI.skin = HighLogic.Skin;
			}
		}

		NodesState stateFromGame = new NodesState();
		NodesState latestStateFromGame = new NodesState();
		NodesState state = new NodesState();
		bool editingState = false;
		DynSlidersDrawer sd = new DynSlidersDrawer();
		WindowResizer wr = new WindowResizer();

		bool _enabled = false;
		bool _newEnabled = false;
		bool newEnabled
		{
			get { return _newEnabled; }
			set
			{
				if (_newEnabled != value)
				{
					_newEnabled = value;
				}
			}
		}
		bool Enabled
		{
			get { return _enabled; }
			set
			{
				if (_enabled != value)
				{
					_enabled = value;
				}
			}
		}

		void OnNodesEditingStart()
		{
			if (!InFlight)
			{
				return;
			}
			if (editingState)
			{
				if (latestStateFromGame.InitFromState())
				{
					if (latestStateFromGame.equals(stateFromGame))
					{

					}
					else
					{
						stateFromGame.InitFromState();
						state.InitFromState();
						editingState = false;
					}
				}
				else
				{
					editingState = false;
				}
			}
			else
			{
				if (latestStateFromGame.InitFromState())
				{
					stateFromGame.InitFromState();
					state.InitFromState();
					editingState = true;
				}
				else
				{
					editingState = false;
				}
			}
		}
		void OnNodesEditingEnd()
		{
			if (!InFlight)
			{
				return;
			}

			if (editingState)
			{
				if (Event.current.type == EventType.repaint)
				{

					if (state.Equals(stateFromGame))
					{
						return;
					}

					//FixFirstNode();
					//FixReorderedNodes();

					state.ExportToState();
					editingState = false;
				}
			}
		}

		void FixFirstNode()
		{
			if (state.Nodes.Count == 0)
			{
				return;
			}
			var ut = Utils.GetTime();
			if (state.Nodes[0].Time <= ut)
			{
				var maneuverNode = Utils.GetManeuverNode(0);
				if (maneuverNode == null)
				{
					return;
				}

				var orbit = maneuverNode.patch;
				if (orbit == null)
				{
					return;
				}

				var period = orbit.period;
				state.Nodes[0].Time += period;
			}
		}

		void FixReorderedNodes()
		{
			if (state.Nodes.Count < 2)
			{
				return;
			}

			bool swapped = false;
			for (var i = 1; i < state.Nodes.Count; i++)
			{
				if (state.Nodes[i].Time < state.Nodes[i - 1].Time)
				{
					swapped = true;
					break;
				}
			}
			if (!swapped)
			{
				return;
			}

			state.Nodes.Sort((NodeState a, NodeState b) =>
			{
				return a.Time.CompareTo(b.Time);
			});
			sd.DisableUntilMouseUp();
		}

		bool updSize = false;
		string lastToolTip = "";
		public void MainWindowConstructor(int id)
		{
			inconstructor = true;
			constructorid++;


			//Enabled = GUI.Toggle(new Rect(0, 0, 16, 16), Enabled, "");

			var enableButtonRect = new Rect(2, 2, 16, 16);

			if (Enabled)
			{
				if (GUI.Button(enableButtonRect, new GUIContent("", "Disable")))
				{
					Enabled = false;
				}
				OnNodesEditingStart();
				GUILayout.BeginHorizontal();
				//Enabled = GUILayout.Toggle(Enabled, "");
				//nullSkin = GUILayout.Toggle(nullSkin, new GUIContent("", "Change skin"));
				GUILayout.EndHorizontal();

				//GUILayout.BeginHorizontal();
				//var p = GUI.enabled;
				//GUI.enabled = false;
				//GUILayout.Button(new GUIContent("undo", "Undo"));
				//GUILayout.Button(new GUIContent("redo", "Redo"));
				//GUI.enabled = true;
				//GUILayout.EndHorizontal();

				DrawState();

				OnNodesEditingEnd();
				var resizing = wr.Resizing;
				mainWindowSize = wr.ResizeWindow(mainWindowPos, mainWindowSize, mainWindowSizeAfterLayout);
				if (resizing && wr.Resizing != resizing)
				{
					updSize = true;
				}
			}
			else
			{
				if (GUI.Button(enableButtonRect, new GUIContent("", "enable")))
				{
					Enabled = true;
				}
			}

			if (Event.current.type == EventType.Repaint)
			{
				lastToolTip = GUI.tooltip;
			}

			GUI.DragWindow();
		}

		void DrawState()
		{
			scroll = GUILayout.BeginScrollView(scroll);
			sd.BeginDraw();
			int deleteIndex = -1;
			var prevTime = Utils.GetTime();
			double totalDeltaV = 0;
			for (var i = 0; i < state.Nodes.Count; i++)
			{
				var node = state.Nodes[i];

				GUILayout.BeginVertical("box");

				node.Time = DrawTime(node.Time, prevTime, i, ref deleteIndex);
				prevTime = node.Time;
				node.Prograde = DrawNodeComponent(node.Prograde, "Prograde");
				node.Normal = DrawNodeComponent(node.Normal, "Normal");
				node.RadialOut = DrawNodeComponent(node.RadialOut, "Radial out");

				var deltaV = node.DeltaV;
				totalDeltaV += deltaV;
				GUILayout.Label("Δv:\t" + deltaV.ToString("0.00") + " m/s", GUILayout.ExpandWidth(false));

				GUILayout.EndVertical();
			}

			if (GUILayout.Button(new GUIContent("+", "Add new maneuver node")))
			{
				state.AddNode(60);
				scroll.y = Mathf.Infinity;
			}

			if (deleteIndex != -1)
			{
				state.Nodes.RemoveAt(deleteIndex);
			}

			GUILayout.EndScrollView();

			GUILayout.Label("total Δv:\t" + totalDeltaV.ToString("0.00"));
		}

		double DrawTime(double value, double prevValue, int index, ref int deleteIndex)
		{
			GUILayout.BeginHorizontal();

			GUILayout.Label(string.Format("Node {0}: {1}", index + 1, Utils.FormatTime(value - prevValue)));

			GUILayout.FlexibleSpace();

			if (GUILayout.Button(new GUIContent("x", string.Format("delete node {0}", index + 1)), GUILayout.ExpandWidth(false)))
			{
				deleteIndex = index;
			}

			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			if (GUILayout.Button(new GUIContent("Pe", "Set node time to next periapsis"), GUILayout.ExpandWidth(false)))
			{
				try
				{
					var o = Utils.GetManeuverNodes()[index].patch;
					value = o.NextPeriapsisTime(value - o.period / 2);
				}
				catch (Exception) { }
			}

			if (GUILayout.Button(new GUIContent("Ap", "Set node time to next apoapsis"), GUILayout.ExpandWidth(false)))
			{
				try
				{
					var o = Utils.GetManeuverNodes()[index].patch;
					if (o.eccentricity < 1) value = o.NextApoapsisTime(value - o.period / 2);
				}
				catch (Exception) { }
			}

			value = DoubleTextField(value);

			bool setZero = GUILayout.Button("0", GUILayout.ExpandWidth(false));

			value = sd.DrawDynSlider(value, timeDeltaFunction, GUILayout.MinWidth(40), GUILayout.ExpandWidth(true));
			if (setZero)
			{
				value = 0;
			}

			GUILayout.EndHorizontal();

			return value;
		}

		double DoubleTextField(double value, string format = "0.000", float width = 60)
		{
			var originalStringValue = value.ToString(format);
			string newStringValue = GUILayout.TextField(originalStringValue, GUILayout.Width(width));
			if (originalStringValue != newStringValue)
			{
				try
				{
					value = double.Parse(newStringValue);
				}
				catch (FormatException) { }
			}
			return value;
		}
		double DrawNodeComponent(double value, string label)
		{
			GUILayout.BeginHorizontal();

			GUILayout.Label(label + ":", GUILayout.ExpandWidth(false), GUILayout.Width(70));

			value = DoubleTextField(value);

			bool setZero = GUILayout.Button("0", GUILayout.ExpandWidth(false));

			value = sd.DrawDynSlider(value, speedDeltaFunction, GUILayout.MinWidth(40));
			if (setZero)
			{
				value = 0;
			}

			GUILayout.EndHorizontal();
			return value;
		}
	}
}
