using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace amne
{
	public class Utils
	{
		const double eps = 1e-9;
		public static bool Eq(double a, double b)
		{
			return a == b || Math.Abs(b - a) < eps;
		}

		public static void Wtf(string message = null)
		{
			if (message != null)
			{
				message = "wtf: " + message;
			}
			else
			{
				message = "wtf";
			}
			throw new Exception(message);
		}
		public static PatchedConicSolver GetSolver()
		{
			var fetch = FlightGlobals.fetch;
			if (fetch == null)
			{
				return null;
			}

			var activeVessel = FlightGlobals.ActiveVessel;
			if (activeVessel == null)
			{
				return null;
			}

			var solver = activeVessel.patchedConicSolver;
			if (solver == null)
			{
				return null;
			}

			return solver;
		}

		public static ManeuverNode GetManeuverNode(int index)
		{
			var nodes = GetManeuverNodes();			
			if (nodes != null && index < nodes.Count)
			{
				return nodes[index];
			}
			return null;
		}
		public static List<ManeuverNode> GetManeuverNodes()
		{
			var solver = GetSolver();
			if (solver == null)
			{
				return null;
			}

			var maneuverNodes = solver.maneuverNodes;
			if (maneuverNodes == null)
			{
				return null;
			}

			return maneuverNodes;
		}

		public static double GetTime()
		{
			try
			{
				return Planetarium.GetUniversalTime();
			}
			catch (Exception)
			{
				return 0;
			}
		}

		public static Rect MakeRect(Vector2 pos, Vector2 size)
		{
			return new Rect(pos.x, pos.y, size.x, size.y);
		}

		public static Vector2 GetPos(Rect rect)
		{
			return new Vector2(rect.xMin, rect.yMin);
		}
		public static Vector2 GetSize(Rect rect)
		{
			return new Vector2(rect.width, rect.height);
		}


		public static string FormatTime(double time)
		{
			if (time < 0)
			{
				return "+ " + FormatPositiveTime(-time);
			}
			else
			{
				return "- " + FormatPositiveTime(time);
			}
		}
		public static string FormatPositiveTime(double time)
		{
			var milliseconds = time % 1;
			string res = (milliseconds * 1000).ToString("000.0") + "ms";						
			var rest = (long)Math.Floor(time);

			if (rest == 0)
			{
				return res;
			}

			var seconds = rest % 60;
			res = seconds.ToString("00") + "s, " + res;
			rest /= 60;
			if (rest == 0)
			{
				return res;
			}

			var minutes = rest % 60;
			res = minutes.ToString("00") + "m, " + res;
			rest /= 60;
			if (rest == 0)
			{
				return res;
			}

			var hours = rest % 24;
			res = hours.ToString("00") + "h, " + res;
			rest /= 24;
			if (rest == 0)
			{
				return res;
			}

			var days = rest % 365;
			res = days.ToString() + "d, " + res;
			rest /= 365;

			if (rest == 0)
			{
				return res;
			}

			res = rest.ToString() + "y, " + res;
			return res;
		}
	}
}
