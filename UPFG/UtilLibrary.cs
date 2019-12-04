using System;
using System.Collections.Generic;
using System.Net;
using System.Numerics;
using Google.Protobuf.WellKnownTypes;
using KRPC.Client;
using KRPC.Client.Services.SpaceCenter;
using KRPC.Client.Services.KRPC;
using Service = KRPC.Client.Services.KRPC.Service;

namespace UPFG
{
	public class UtilLibrary
	{
		private static Connection _conn;
		private static Service _krpc;
		public static Service KRPC
		{
			get
			{
				return Connection.KRPC();
			}
			set { }
		}

		public static Connection Connection
		{
			get
			{
				if (_conn == null)
				{
					_conn = new Connection(
						name: "UPFG",
						address: IPAddress.Parse("127.0.0.1"),
						rpcPort: 50000,
						streamPort: 50001
					);
				}

				return _conn;
			}
			set { }
		}

		public static KRPC.Client.Services.SpaceCenter.Service SpaceCenter
		{
			get => Connection.SpaceCenter();
		}

		public static Vessel Vessel
		{
			get => SpaceCenter.ActiveVessel;
		}


		#region INTERNAL FUNCTIONS

		public static Vector3 SolarPrimeVector(ReferenceFrame referenceFrame)
		{
			var sun = SpaceCenter.Bodies["Sun"];
			var secondsPerDegree = sun.RotationalPeriod / 360;
			var rotationOffset = (SpaceCenter.UT / secondsPerDegree) % 360;
			var sunPosition = new Vector3(sun.Position(referenceFrame));
			var sunPosition2 = new Vector3(sun.SurfacePosition(0, 0 - rotationOffset, referenceFrame));sun.SurfacePosition(0, 0 - rotationOffset, referenceFrame);
			var primeVector = sunPosition2 - sunPosition;

			return primeVector.Normalize();
		}

		private static Vector3 Rodrigues(Vector3 inVector, Vector3 axis, double angle)
		{
			Vector3 outVector = inVector * Math.Cos(angle);
			axis = axis.Normalize();

			outVector += axis.CrossProduct(inVector) * Math.Sin(angle);
			outVector += axis * axis.DotProduct(inVector) * (1 - Math.Cos(angle));

			return outVector;

		}

		private static Vector3 AimAndRoll(Vector3 aimVec, double rollAngle, Vehicle vehicle)
		{
			Vector3 rollVector = Rodrigues(vehicle.UpVector, aimVec, -rollAngle);
			Quaternion lookRotation = Direction.LookRotation(aimVec, rollVector);
			Vector3 rollVec = new Vector3(lookRotation.X, lookRotation.Y, lookRotation.Z);
			double rollScalar = lookRotation.W;

			Vector3 first = 2.0 * Vector3.DotProduct(rollVec, aimVec) * rollVec;
			Vector3 second = rollScalar * rollScalar - Vector3.DotProduct(rollVec, rollVec) * aimVec;
			Vector3 third = 2.0 * rollScalar * Vector3.CrossProduct(rollVec, aimVec);

			Vector3 vPrime = first + second + third;

			return vPrime;
		}

		// KSP -> MATLAB -> KSP vector conversion
		private static Vector3 vecYZ(Vector3 input)
		{
			return new Vector3(input.X, input.Z, input.Y);
		}

		public static Thrust GetThrust(Stage stage)
		{
			var n = stage.Engines.Count;
			double? F = 0d;
			double? dm = 0d;

			double? isp = 0d;

			for (int i = 0; i < n; i++)
			{
				isp = stage.Engines[i].ISP;
				var dm_ = stage.Engines[i].Flow;
				dm += dm_;
				F += isp * dm_ * Settings.G0;
			}

			isp = F / (dm * Settings.G0);

			return new Thrust(F,dm, isp);
		}

		private static double ConstAccBurnTime(Stage stage)
		{
			var engineData = GetThrust(stage);
			var isp = engineData.Isp;
			var baseFlow = engineData.Flow;
			var mass = stage.MassTotal;
			var fuel = stage.MassFuel;
			var gLim = stage.GLimit;
			var tMin = stage.MinThrottle;

			var maxBurnTime = isp / gLim * Math.Log((mass / (mass - fuel)).Value);

			// If no throttling limit - we will always be able to throttle a bit more down.
			// With no possible constraints to violate, we can just return this theoretical time

			if (tMin == 0)
			{
				return maxBurnTime.Value;
			}

			var violationTime = -isp / gLim * Math.Log(tMin.Value);

			// If this time is lower than the time we want to burn, we need to act
			var constThrustTime = 0d;

			if (violationTime < maxBurnTime)
			{
				// calculate mass of fuel burned until violationTime
				var burnedFuel = mass * (1 - Math.Pow(Math.E, (-gLim / isp * violationTime).Value));
				// then, time it will take to burn the rest on constant minimum throttle
				constThrustTime = ((fuel - burnedFuel) / (baseFlow * tMin)).Value;
			}

			return (maxBurnTime + constThrustTime).Value;

		}

		#endregion

		#region TARGETING FUNCTIONS

		public static Mission MissionSetup(Mission mission, Controls controls)
		{

			if (mission.Altitude < mission.Periapsis || mission.Altitude > mission.Apoapsis || mission.Altitude == null)
			{
				mission.Altitude = mission.Periapsis;
			}

			if (mission.Direction == null)
			{
				mission.Direction = MissionDirection.nearest;
			}

			if (mission.Inclination == null)
			{
				mission.Inclination = Vessel.Orbit.Inclination;
			}

			while (mission.Inclination <= -180)
			{
				mission.Inclination += 360;
			}

			while (mission.Inclination >= 180)
			{
				mission.Inclination -= 360;
			}

			if (mission.LAN != null)
			{
				while (mission.LAN <= 0)
				{
					mission.LAN += 360;
				}

				if (mission.LAN > 360)
				{
					mission.LAN %= 360;
				}
			}
			else
			{
				if (mission.Direction == MissionDirection.nearest)
				{
					mission.Direction = MissionDirection.north;
				}

				var currentNode = NodeVector(mission.Inclination.Value, mission.Direction.Value);
				var currentLan = Vector3.Angle(
					Vector3.YAxis,
					SolarPrimeVector(Vessel.Orbit.Body.ReferenceFrame)
				);

				var cx = Vector3.CrossProduct(currentNode, SolarPrimeVector(Vessel.Orbit.Body.ReferenceFrame));
				if (Vector3.DotProduct(Vector3.YAxis, cx) < 0)
				{
					currentLan = 360 - currentLan;
				}

				mission.LAN = currentLan + (controls.LaunchTimeAdvance + 30) / Vessel.Orbit.Body.RotationalPeriod * 360;
			}

			return mission;
		}

		public static UPFGTarget TargetSetup(Mission mission)
		{
			var pe = mission.Periapsis * 1000 + Vessel.Orbit.Body.EquatorialRadius;
			var ap = mission.Apoapsis * 1000 + Vessel.Orbit.Body.EquatorialRadius;
			var targetAltitude = mission.Altitude.Value * 100 + Vessel.Orbit.Body.EquatorialRadius;
			var sma = (pe + ap) / 2;
			var vpe = Math.Sqrt(Vessel.Orbit.Body.GravitationalParameter * (2 / pe - 1 / sma));
			var srm = pe * vpe;
			var targetVelocity = Math.Sqrt(Vessel.Orbit.Body.GravitationalParameter * (2 / targetAltitude - 1 / sma));

			var aa = targetVelocity * targetAltitude;
			var ab = Math.Min(-1 , Math.Max(1, srm / aa));
			var flightPathAngle = Math.Acos(ab);


			return new UPFGTarget(targetAltitude, targetVelocity, flightPathAngle, Vector3.Zero);
		}

		public static Vector3 NodeVector(double inc, MissionDirection dir)
		{
			var b = Math.Tan(90 - inc) * Math.Tan(Vessel.Flight().Latitude);
			b = Math.Asin(Math.Min(Math.Max(-1, b), 1));

			var longitudeVector = Vector3.ProjectOnPlane(
				Vector3.YAxis,
				-(new Vector3(Vessel.Position(Vessel.Orbit.Body.ReferenceFrame)).NormalizeOrDefault())
					);

			if (dir == MissionDirection.north)
			{
				return Rodrigues(longitudeVector, Vector3.YAxis, b);
			} else if (dir == MissionDirection.south)
			{
				return Rodrigues(longitudeVector, Vector3.YAxis, 180 - b);
			}
			else
			{
				return NodeVector(inc, MissionDirection.north);
			}

		}

		public static double OrbitInterceptTime(Mission mission, MissionDirection direction)
		{
			var targetInc = mission.Inclination.Value;
			var targetLan = mission.LAN.Value;

			if (direction == MissionDirection.nearest)
			{
				var timeToNortherly = OrbitInterceptTime(mission, MissionDirection.north);
				var timeToSouthernly = OrbitInterceptTime(mission, MissionDirection.south);

				if (timeToSouthernly < timeToNortherly)
				{
					mission.Direction = MissionDirection.south;
					return timeToSouthernly;
				}
				else
				{
					mission.Direction = MissionDirection.north;
					return timeToNortherly;
				}
			}
			else
			{
				Settings.CurrentNode = NodeVector(targetInc, direction);
				var targetNode = Rodrigues(SolarPrimeVector(Vessel.Orbit.Body.ReferenceFrame), Vector3.YAxis,
					-targetLan);
				var nodeDelta = Vector3.Angle(Settings.CurrentNode, targetNode);
				var deltaDir =
					Vector3.DotProduct(Vector3.YAxis, Vector3.CrossProduct(targetNode, Settings.CurrentNode));

				if (deltaDir < 0)
				{
					nodeDelta = 360 - nodeDelta;
				}

				var deltaTime = Vessel.Orbit.Body.RotationalPeriod * nodeDelta / 360;

				return deltaTime;
			}
		}

		public static double LaunchAzimuth(Mission mission, UPFGTarget upfgTarget)
		{
			var targetInc = mission.Inclination.Value;
			var targetVel = upfgTarget.Velocity;
			var siteLat = Vessel.Flight().Latitude;

			var bInertial = Math.Cos(targetInc) / Math.Cos(siteLat);

			bInertial = Math.Min(Math.Max(-1, bInertial), 1);
			bInertial = Math.Asin(bInertial);

			var vOrbit = targetVel * Math.Cos(upfgTarget.Angle);
			var vBody = (2 * Math.PI * Vessel.Orbit.Body.EquatorialRadius / Vessel.Orbit.Body.RotationalPeriod) *
			            Math.Cos(siteLat);
			var vRotX = vOrbit * Math.Sin(bInertial) - vBody;
			var vRotY = vOrbit * Math.Cos(bInertial);
			var azimuth = Math.Atan2(vRotY, vRotX);

			if (mission.Direction == MissionDirection.north)
			{
				return 90 - azimuth;
			} else if (mission.Direction == MissionDirection.south)
			{
				return 90 + azimuth;
			}
			else
			{
				return 90 - azimuth;
			}

		}




		#endregion

		#region SETUP FUNCTIONS
		public static UPFG SetupUPFG(Mission mission, UPFGState upfgState, UPFGTarget upfgTarget)
		{
			var curR = upfgState.Radius;
			var curV = upfgState.Velocity;

			upfgTarget.Normal = TargetNormal(mission.Inclination.Value, mission.LAN.Value);

			var desR = Rodrigues(curR, -upfgTarget.Normal, 20).Normalize() * upfgTarget.Radius;
			var tgoV = upfgTarget.Velocity * Vector3.CrossProduct(-upfgTarget.Normal, desR).Normalize();

			return new UPFG()
			{
				Cser = new CserResult(Vector3.Zero,  Vector3.Zero, 0d, null),
				RBias = Vector3.Zero,
				Rd = desR,
				RGrav = -Vessel.Orbit.Body.GravitationalParameter / 2 * curR / Math.Pow(curR.Magnitude, 3),
				Tb = 0d,
				Time = upfgState.Time,
				TGo = 0d,
				V = curV,
				VGo = tgoV
			};
		}

		public static State AcquireState(Vehicle vessel)
		{
			var vel = new Vector3(Vessel.Velocity(Vessel.OrbitalReferenceFrame));
			return new State()
			{
				Time = (double)DateTime.Now.ToTimestamp().Seconds - vessel.LiftOffTime,
				Mass = Vessel.Mass * 1000,
				Radius = vecYZ(new Vector3(Vessel.Position(Vessel.Orbit.Body.ReferenceFrame))),
				Velocity = vecYZ(new Vector3(Vessel.Velocity(Vessel.OrbitalReferenceFrame)))
			};
		}

		public static Vector3 TargetNormal(double targetInc, double targetLan)
		{
			var highPoint = Rodrigues(SolarPrimeVector(Vessel.Orbit.Body.ReferenceFrame), Vector3.YAxis,
				90 - targetLan);
			var rotAxis = new Vector3(-highPoint.Z, highPoint.Y, highPoint.X);
			var normalVec = Rodrigues(highPoint, rotAxis, 90 - targetInc);

			return -vecYZ(normalVec);
		}
		#endregion

		#region HELPER FUNCTIONS

		#endregion

		public static void SetVehicle(Vehicle vehicle, Mission mission, Controls controls)
		{
			foreach (var stage in vehicle.Stages)
			{
				stage.SetWeights();
				stage.SetParameters();

				foreach (var engine in stage.Engines)
				{
					engine.Setup(stage.Throttle);
				}
			}
		}
	}

	public struct Direction
	{
		public double Pitch { get; set; }
		public double Yaw { get; set; }
		public double Roll { get; set; }
		public Vector3 Forward { get; set; }
		public Vector3 Top { get; set; }
		public Vector3 Right { get; set; }

		public Direction Inverse
		{
			get => new  Direction();
		}

		public Direction(double pitch, double yaw, double roll, Vector3 forward, Vector3 top, Vector3 right)
		{
			Pitch = pitch;
			Yaw = yaw;
			Roll = roll;
			Forward = forward;
			Top = top;
			Right = right;
		}

		public static Quaternion LookRotation(Vector3 forward, Vector3 up)
		{
			forward = forward.Normalize();

			Vector3 vector1 = Vector3.Normalize(forward);
			Vector3 vector2 = Vector3.Normalize(Vector3.CrossProduct(up, vector1));
			Vector3 vector3 = Vector3.CrossProduct(vector1, vector2);

			var m00 = vector2.X;
			var m01 = vector2.Y;
			var m02 = vector2.Z;
			var m10 = vector3.X;
			var m11 = vector3.Y;
			var m12 = vector3.Z;
			var m20 = vector1.X;
			var m21 = vector1.Y;
			var m22 = vector1.Z;

			double num8 = (m00 + m11) + m22;
			var quaternion = new Quaternion();

			if ((float)num8 > 0f)
			{
				var num = (float)Math.Sqrt(num8 + 1d);
				quaternion.W = num * 0.5f;
				num = 0.5f / num;
				quaternion.X = (float) ((m12 - m21) * num);
				quaternion.Y = (float) ((m20 - m02) * num);
				quaternion.Z = (float) ((m01 - m10) * num);
				return quaternion;
			}

			if ((m00 >= m11) && (m00 >= m22))
			{
				var num7 = (float) Math.Sqrt(((1f + m00) - m11) - m22);
				var num4 = 0.5f / num7;
				quaternion.X = 0.5f * num7;
				quaternion.Y = (float) ((m01 + m10) * num4);
				quaternion.Z = (float) ((m02 + m20) * num4);
				quaternion.W = (float) ((m12 - m21) * num4);
				return quaternion;
			}

			if (m11 > m22)
			{
				var num6 = (float) Math.Sqrt(((1f + m11) - m00) - m22);
				var num3 = 0.5f / num6;
				quaternion.X = (float) ((m10 + m01) * num3);
				quaternion.Y = 0.5f * num6;
				quaternion.Z = (float) ((m21 + m12) * num3);
				quaternion.W = (float) ((m21 - m02) * num3);
				return quaternion;
			}

			var num5 = (float) Math.Sqrt(((1f + m22) - m00) - m11);
			var num2 = 0.5f / num5;
			quaternion.X = (float) ((m20 + m02) * num2);
			quaternion.Y = (float) ((m21 + m12) * num2);
			quaternion.Z = 0.5f * num5;
			quaternion.W = (float) ((m01 - m10) * num2);
			return quaternion;
		}
	}
}
