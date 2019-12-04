using System.Collections.Generic;

namespace UPFG
{
	public class Vehicle
	{
		public int Length { get; set; }

		public List<Stage> Stages { get; set; }

		public Vector3 UpVector { get; }
		public double LiftOffTime { get; set; }
	}

	public class Engine
	{
		public double ISP { get; set; }
		public double? Flow { get; set; }

		public double Thrust { get; set; }

		public Engine(double isp, double thrust, bool hasFlow = false, double flow = 0d)
		{
			ISP = isp;
			Thrust = thrust;

			if (hasFlow) Flow = flow;
		}

		public void Setup(double? throttle)
		{
			if (Flow == null)
			{
				Flow = Thrust / (ISP / Settings.G0) * throttle;
			}
		}
	}

	public class Stage
	{
		public double? Mode { get; set; }

		public List<Engine>? Engines { get; set; }
		public double? MassTotal { get; set; }
		public double? MassFuel { get; set; }
		public double? GLimit { get; set; }
		public double? MinThrottle { get; set; }
		public double? MaxT { get; set; }
		public double? MassDry { get; set; }

		public double? Throttle { get; set; }

		public Thrust CombinedEngines => UtilLibrary.GetThrust(this);

		public bool? ShutdownRequired { get; set; }

		public void SetWeights(double payload = 0d)
		{
			if (MassTotal != null && MassDry != null)
			{
				MassFuel = MassTotal - MassDry;
			} else if (MassFuel != null && MassTotal != null)
			{
				MassDry = MassTotal - MassFuel;
			} else if (MassFuel != null && MassDry != null)
			{
				MassTotal = MassFuel + MassDry;
			}

			if (payload != 0)
			{
				MassTotal += payload;
				MassDry += payload;
			}
		}

		public void SetParameters()
		{
			if (GLimit == null) GLimit = 0;
			if (MinThrottle == null)
			{
				MinThrottle = 0;
			} else if (MinThrottle > 1)
			{
				MinThrottle /= 100;
			}

			if (Throttle == null)
			{
				Throttle = 1;
			} else if (Throttle > 1)
			{
				Throttle /= 100;
			}

			Mode = 1;

			if (ShutdownRequired == null)
			{
				ShutdownRequired = false;
			}

			MaxT = MassFuel / CombinedEngines.Flow;
		}

	}

	public struct Thrust
	{
		public double? F { get; set; }
		public double? Flow { get; set; }
		public double? Isp { get; set; }

		public Thrust(double? f, double? flow, double? isp)
		{
			F = f;
			Flow = flow;
			Isp = isp;
		}
	}
}
