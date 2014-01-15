using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace amne
{
	public class NodeState
	{
		double prograde;
		double normal;
		double radialOut;
		double time;

		public double Prograde { get { return prograde; } set { prograde = value; } }
		public double Normal { get { return normal; } set { normal = value; } }
		public double RadialOut { get { return radialOut; } set { radialOut = value; } }
		public double Time { get { return time; } set { time = value; } }

		public double X { get { return RadialOut; } set { RadialOut = value; } }
		public double Y { get { return Normal; } set { Normal = value; } }
		public double Z { get { return Prograde; } set { Prograde = value; } }
		public double Retrograde { get { return -Prograde; } set { Prograde = -value; } }
		public double AntiNormal { get { return -Normal; } set { Normal = value; } }
		public double RadialIn { get { return -RadialOut; } set { RadialOut = -value; } }

		public double DeltaV { get { return Math.Sqrt(prograde * prograde + normal * normal + radialOut * radialOut); } }

		public NodeState() { }
		public NodeState(ManeuverNode maneuverNode)
		{
			InitFromManeuverNode(maneuverNode);
		}

		public NodeState(double x, double y, double z, double time)
		{
			InitFromValues(x, y, z, time);
		}

		public void InitFromManeuverNode(ManeuverNode maneuverNode)
		{
			var deltaV = maneuverNode.DeltaV;
			X = deltaV.x;
			Y = deltaV.y;
			Z = deltaV.z;
			Time = maneuverNode.UT;
		}


		public void InitFromValues(double x, double y, double z, double time)
		{
			X = x;
			Y = y;
			Z = z;
			Time = time;
		}

		public void UpdateManeuverNode(ManeuverNode maneuverNode)
		{
			var deltaV = maneuverNode.DeltaV;
			deltaV.x = X;
			deltaV.y = Y;
			deltaV.z = Z;
			maneuverNode.OnGizmoUpdated(deltaV, Time);
			//maneuverNode.OnGizmoUpdated(new Vector3d(X, Y, Z), Time);
		}

		public bool equals(ManeuverNode maneuverNode)
		{
			var deltaV = maneuverNode.DeltaV;
			return Eq(X, deltaV.x) && Eq(Y, deltaV.y) && Eq(Z, deltaV.z) && Eq(Time, maneuverNode.UT);
		}

		public bool equals(NodeState b)
		{
			return Eq(X, b.X) && Eq(Y, b.Y) && Eq(Z, b.Z) && Eq(Time, b.Time);
		}

		bool Eq(double a, double b)
		{
			return Utils.Eq(a, b);
		}

		public string ToDebugString()
		{
			return Time.ToString() + " " + Prograde.ToString() + " " + AntiNormal.ToString() + " " + RadialOut.ToString();
		}

		public override string ToString()
		{
			return ToDebugString();
		}


	}
	public class NodesState
	{
		List<NodeState> nodes = new List<NodeState>();
		public List<NodeState> Nodes { get { return nodes; } }

		void InitFromManeuverNodes(List<ManeuverNode> maneuverNodes)
		{
			nodes.Clear();
			for (var i = 0; i < maneuverNodes.Count; i++)
			{
				var maneuverNode = maneuverNodes[i];
				nodes.Add(new NodeState(maneuverNode));
			}
		}
		public bool InitFromState()
		{
			var maneuverNodes = Utils.GetManeuverNodes();
			if (maneuverNodes == null)
			{
				return false;
			}
			InitFromManeuverNodes(maneuverNodes);
			return true;
		}

		public string ToDebugString()
		{
			string res = "";
			for (var i = 0; i < nodes.Count; i++)
			{
				res += nodes[i].ToDebugString() + "\n";
			}
			return res;
		}

		public override string ToString()
		{
			return ToDebugString();
		}

		public bool equals(NodesState s)
		{
			if (nodes.Count != s.nodes.Count)
			{
				return false;
			}
			for (int i = 0; i < nodes.Count; i++)
			{
				if (!nodes[i].equals(s.nodes[i]))
				{
					return false;
				}
			}
			return true;
		}

		public bool ExportToState()
		{
			var solver = Utils.GetSolver();
			if (solver == null)
			{
				return false;
			}

			var maneuverNodes = solver.maneuverNodes;
			if (maneuverNodes == null)
			{
				return false;
			}

			int n = Math.Min(nodes.Count, maneuverNodes.Count);
			for (int i = 0; i < n; i++)
			{
				var maneuverNode = maneuverNodes[i];
				var node = nodes[i];

				if (!node.equals(maneuverNode))
				{
					node.UpdateManeuverNode(maneuverNode);
				}
			}

			if (n < nodes.Count)
			{
				for (int i = n; i < nodes.Count; i++)
				{
					var node = nodes[i];
					var maneuverNode = solver.AddManeuverNode(node.Time);
					node.UpdateManeuverNode(maneuverNode);
				}
			}
			else if (n < maneuverNodes.Count)
			{
				for (int i = maneuverNodes.Count - 1; i >= n; i--)
				{
					solver.RemoveManeuverNode(maneuverNodes[i]);
				}
			}

			return true;
		}

		public double GetLastTime()
		{
			if (nodes.Count > 0)
			{
				return nodes[nodes.Count - 1].Time;
			}
			else
			{
				return Utils.GetTime();
			}
		}

		public void AddNode(double timeAfterLast = 60)
		{
			nodes.Add(new NodeState(0, 0, 0, GetLastTime() + timeAfterLast));
		}
	}
}
