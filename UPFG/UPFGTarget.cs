namespace UPFG
{
	public class UPFGTarget
	{
		public double Radius { get; set; }
		public double Velocity { get; set; }
		public double Angle { get; set; }
		public Vector3 Normal { get; set; }

		public UPFGTarget(double radius, double velocity, double angle, Vector3 normal)
		{
			Radius = radius;
			Velocity = velocity;
			Angle = angle;
			Normal = normal;
		}
	}
}
