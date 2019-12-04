namespace UPFG
{
	public class Target
	{
		public double Angle { get; set; }
		public Vector3 Normal { get; set; }
		public double Radius { get; set; }
		public double Velocity { get; set; }


		public Target(double angle, Vector3 normal, double radius, double velocity)
		{
			Angle = angle;
			Normal = normal;
			Radius = radius;
			Velocity = velocity;
		}
	}
}
