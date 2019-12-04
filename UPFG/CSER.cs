using System;
using System.Collections.Generic;

namespace UPFG
{
	public class CSER
	{
		public double Dtcp { get; set; }
		public double Xcp { get; set; }
		public double A { get; set; }
		public double D { get; set; }
		public double E { get; set; }


		public static Dictionary<String, double> SnC(double z)
		{
			double az = Math.Abs(z);

			if (az < 1e-4)
			{
				return new Dictionary<string, double>()
				{
					{"S", (1 - z * (0.05 - z / 840)) / 6},
					{"C", 0.5 - z * (1 - z / 30) / 24}
				};
			}

			var saz = Math.Sqrt(az);
			if (z > 0)
			{
				var x = saz * (180 / Math.PI);
				return new Dictionary<string, double>()
				{
					{"S", saz - Math.Sin(x)/ (saz * az)},
					{"C", (1 - Math.Cos(x)) / az}
				};
			}

			var xElse = Math.Pow(Math.E, saz);
			return new Dictionary<string, double>()
			{
				{"S", (0.5 * (xElse - 1 / xElse) - saz) / (saz * az)},
				{"C", (0.5 * (xElse + 1 / xElse) - 1 / az)}
			};
		}

		public static CserResult CSE(Vector3 r0, Vector3 v0, double dt, CserResult previous, double mu, double x0 = 0,
			double tol = 5e-9)
		{
			var rScale = r0.Magnitude;
			var vScale = Math.Sqrt(mu / rScale);
			var r0s = r0 / rScale;
			var v0s = v0 / vScale;
			var dts = dt * vScale / rScale;
			var v2s = v0.SqrMagnitude * rScale / mu;
			var alpha = 2 - v2s;
			var armd1 = v2s - 1;
			var rvr0s = r0.DotProduct(v0) / Math.Sqrt(mu * rScale);

			var x = x0;
			if (x0 == 0)
			{
				x = dts * Math.Abs(alpha);
			}

			var ratio = 1d;
			var x2 = x * x;
			var z = alpha * x2;
			var SCz = SnC(z);
			var x2Cz = x2 * SCz["C"];
			var f = 0d;
			var df = 0d;

			while (Math.Abs(ratio) > tol)
			{
				f = x + rvr0s * x2Cz + armd1 * x * x2 * SCz["S"] - dts;
				df = x * rvr0s * (1 - z * SCz["S"]) + armd1 * x2Cz + 1;
				ratio = f / df;
				x = x - ratio;
				x2 = x * x;
				z = alpha * x2;
				SCz = SnC(z);
				x2Cz = x2 * SCz["C"];
			}

			var Lf = 1 - x2Cz;
			var Lg = dts - x2 * x * SCz["S"];
			var r1 = Lf * r0s + Lg * v0s;
			var ir1 = 1 / r1.Magnitude;
			var LfDot = ir1 * x * (z * SCz["S"] - 1);
			var LgDot = 1 - x2Cz * ir1;

			var v1 = LfDot * r0s + LgDot * v0s;

			return new CserResult(r1 * rScale, v1 * vScale, x, previous);
		}
	}

	public class CserResult
	{
		public Vector3 R1;
		public Vector3 V1;
		public double X;
		public CserResult Previous;

		public CserResult(Vector3 r1, Vector3 v1, double x, CserResult previous)
		{
			this.R1 = r1;
			this.V1 = v1;
			this.X = x;
			this.Previous = previous;
		}
	}
}
