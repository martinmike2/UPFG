using System;

namespace UPFG
{
	public struct Mission
	{
		public double? Altitude { get; set; }
		public double Periapsis { get; set; }
		public double Apoapsis { get; set; }
		public double? Inclination { get; set; }
		public double? LAN { get; set; }
		public MissionDirection? Direction { get; set; }
		public double? LaunchAzimuth { get; set; }

		public double? Payload { get; set; }
	}


	public enum MissionDirection
	{
		north,
		south,
		nearest
	}
}
