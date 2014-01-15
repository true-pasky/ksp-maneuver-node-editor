using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace amne
{
	public interface IDeltaFunction
	{
		double GetDelta(double value);
	}

	public class IdentityDeltaFunction : IDeltaFunction
	{

		public static IDeltaFunction Instance = new IdentityDeltaFunction();
		public double GetDelta(double x)
		{
			return x;
		}
	}
	public class PowerDeltaFunction : IDeltaFunction
	{

		public double A { get; private set; }
		public double B { get; private set; }

		public PowerDeltaFunction(double A, double B)
		{
			InitFromAB(A, B);
		}

		public PowerDeltaFunction(double x1, double y1, double x2, double y2)
		{
			InitFromTwoPoints(x1, y1, x2, y2);
		}

		private void InitFromAB(double A, double B)
		{
			this.A = A;
			this.B = B;
		}

		private void InitFromTwoPoints(double x1, double y1, double x2, double y2)
		{
			double B = Math.Log(y1 / y2) / (Math.Log(x1) - Math.Log(x2));
			double A = y1 / Math.Pow(x1, B);
			InitFromAB(A, B);
		}
		public double GetDelta(double x)
		{
			double sign = 1;
			if (x < 0)
			{
				x = -x;
				sign = -1;
			}
			return sign * A * Math.Pow(x, B);
		}
	}
}
