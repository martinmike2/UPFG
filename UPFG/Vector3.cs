using System;
using System.Numerics;

namespace UPFG
{
	public struct Vector3
	{
		public static readonly Vector3 Origin = new Vector3(0, 0, 0);
		public static readonly Vector3 XAxis = new Vector3(1, 0, 0);
		public static readonly Vector3 YAxis = new Vector3(0, 1, 0);
		public static readonly Vector3 ZAxis = new Vector3(0, 0, 1);

		private readonly double x;

		private readonly double y;

		private readonly double z;

		public double X
		{
			get { return this.x; }
		}

		public double Y
		{
			get { return this.y; }
		}

		public double Z
		{
			get { return this.z; }
		}

		public double[] Array
		{
			get { return new double[] {x, y, z}; }
		}

		public double this[int index]
		{
			get
			{
				switch (index)
				{
					case 0:
					{
						return X;
					}
					case 1:
					{
						return Y;
					}
					case 2:
					{
						return Z;
					}
					default: throw new ArgumentException(THREE_COMPONENTS, "index");
				}
			}
		}

		public static readonly Vector3 MinValue =
			new Vector3(Double.MinValue, Double.MinValue, Double.MinValue);

		public static readonly Vector3 MaxValue =
			new Vector3(Double.MaxValue, Double.MaxValue, Double.MaxValue);

		public static readonly Vector3 Epsilon =
			new Vector3(Double.Epsilon, Double.Epsilon, Double.Epsilon);

		public static readonly Vector3 Zero =
			Origin;

		public static readonly Vector3 NaN =
			new Vector3(double.NaN, double.NaN, double.NaN);

		public Vector3(double x, double y, double z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public Vector3(System.Tuple<double, double, double> tuple)
		{
			this.x = tuple.Item1;
			this.y = tuple.Item2;
			this.z = tuple.Item3;
		}

		public Vector3(double[] xyz)
		{
			if (xyz.Length == 3)
			{
				this.x = xyz[0];
				this.y = xyz[1];
				this.z = xyz[2];
			}
			else
			{
				throw new ArgumentException(THREE_COMPONENTS);
			}
		}

		public Vector3(Vector3 v1)
		{
			this.x = v1.X;
			this.y = v1.Y;
			this.z = v1.Z;
		}

		private const string THREE_COMPONENTS =
			"Array must contain exactly three components, (x,y,z)";


		public double Magnitude
		{
			get { return Math.Sqrt(SumComponentSqrs()); }
		}

		public double SqrMagnitude
		{
			get
			{
				var mag = this.Magnitude;
				return mag * mag;
			}
		}

		public static Vector3 operator +(Vector3 v1, Vector3 v2)
		{
			return new Vector3(
				v1.X + v2.X,
				v1.Y + v2.Y,
				v1.Z + v2.Z);
		}

		public static Vector3 operator +(float scalar, Vector3 v1)
		{
			return new Vector3(
				v1.X + scalar,
				v1.Y + scalar,
				v1.Z + scalar
				);
		}

		public static Vector3 operator +(Vector3 v1, double scalar)
		{
			return new Vector3(
				v1.X + scalar,
				v1.Y + scalar,
				v1.Z + scalar
				);
		}

		public static Vector3 operator -(Vector3 v1, Vector3 v2)
		{
			return new Vector3(
				v1.X - v2.X,
				v1.Y - v2.Y,
				v1.Z - v2.Z);
		}

		public static Vector3 operator -(double scalar, Vector3 v1)
		{
			return new Vector3(
				v1.X - scalar,
				v1.Y - scalar,
				v1.Z - scalar
				);
		}

		public static Vector3 operator -(Vector3 v1, double scalar)
		{
			return scalar - v1;
		}

		public static Vector3 operator -(Vector3 v1)
		{
			return new Vector3(
				-v1.X,
				-v1.Y,
				-v1.Z);
		}

		public static Vector3 operator +(Vector3 v1)
		{
			return new Vector3(
				+v1.X,
				+v1.Y,
				+v1.Z);
		}

		public static bool operator <(Vector3 v1, Vector3 v2)
		{
			return v1.SumComponentSqrs() < v2.SumComponentSqrs();
		}

		public static bool operator <=(Vector3 v1, Vector3 v2)
		{
			return v1.SumComponentSqrs() <= v2.SumComponentSqrs();
		}

		public static bool operator >(Vector3 v1, Vector3 v2)
		{
			return v1.SumComponentSqrs() > v2.SumComponentSqrs();
		}

		public static bool operator >=(Vector3 v1, Vector3 v2)
		{
			return v1.SumComponentSqrs() >= v2.SumComponentSqrs();
		}

		public static bool operator ==(Vector3 v1, Vector3 v2)
		{
			return
				v1.X == v2.X &&
				v1.Y == v2.Y &&
				v1.Z == v2.Z;
		}

		public static bool operator !=(Vector3 v1, Vector3 v2)
		{
			return !(v1 == v2);
		}

		public override bool Equals(object other)
		{
			// Check object other is a Vector3 object
			if (other is Vector3)
			{
				// Convert object to Vector3
				Vector3 otherVector = (Vector3) other;

				// Check for equality
				return otherVector.Equals(this);
			}
			else
			{
				return false;
			}
		}

		public bool Equals(Vector3 other)
		{
			return
				this.X.Equals(other.X) &&
				this.Y.Equals(other.Y) &&
				this.Z.Equals(other.Z);
		}

		public bool Equals(object other, double tolerance)
		{
			if (other is Vector3)
			{
				return this.Equals((Vector3) other, tolerance);
			}

			return false;
		}

		public bool Equals(Vector3 other, double tolerance)
		{
			return
				AlmostEqualsWithAbsTolerance(this.X, other.X, tolerance) &&
				AlmostEqualsWithAbsTolerance(this.Y, other.Y, tolerance) &&
				AlmostEqualsWithAbsTolerance(this.Z, other.Z, tolerance);
		}

		public static bool AlmostEqualsWithAbsTolerance(double a, double b, double maxAbsoluteError)
		{
			double diff = Math.Abs(a - b);

			if (a.Equals(b))
			{
				// shortcut, handles infinities
				return true;
			}

			return diff <= maxAbsoluteError;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = this.x.GetHashCode();
				hashCode = (hashCode * 397) ^ this.y.GetHashCode();
				hashCode = (hashCode * 397) ^ this.z.GetHashCode();
				return hashCode;
			}
		}

		public int CompareTo(object other)
		{
			if (other is Vector3)
			{
				return this.CompareTo((Vector3) other);
			}

			// Error condition: other is not a Vector3 object
			throw new ArgumentException(
				NON_VECTOR_COMPARISON + "\n" +
				ARGUMENT_TYPE + other.GetType().ToString(),
				"other");
		}

		public int CompareTo(Vector3 other)
		{
			if (this < other)
			{
				return -1;
			}
			else if (this > other)
			{
				return 1;
			}

			return 0;
		}

		private const string NON_VECTOR_COMPARISON =
			"Cannot compare a Vector3 to a non-Vector3";

		private const string ARGUMENT_TYPE =
			"The argument provided is a type of ";

		public int CompareTo(object other, double tolerance)
		{
			if (other is Vector3)
			{
				return this.CompareTo((Vector3) other, tolerance);
			}

			// Error condition: other is not a Vector3 object
			throw new ArgumentException(
				NON_VECTOR_COMPARISON + "\n" +
				ARGUMENT_TYPE + other.GetType().ToString(),
				"other");
		}

		public int CompareTo(Vector3 other, double tolerance)
		{
			var bothInfinite =
				double.IsInfinity(this.SumComponentSqrs()) &&
				double.IsInfinity(other.SumComponentSqrs());

			if (this.Equals(other, tolerance) || bothInfinite)
			{
				return 0;
			}

			if (this < other)
			{
				return -1;
			}

			return 1;
		}

		public static Vector3 operator *(Vector3 v1, double s2)
		{
			return
				new Vector3
				(
					v1.X * s2,
					v1.Y * s2,
					v1.Z * s2
				);
		}

		public static Vector3 operator *(double s1, Vector3 v2)
		{
			return v2 * s1;
		}

		public static Vector3 CrossProduct(Vector3 v1, Vector3 v2)
		{
			return
				new Vector3
				(
					v1.Y * v2.Z - v1.Z * v2.Y,
					v1.Z * v2.X - v1.X * v2.Z,
					v1.X * v2.Y - v1.Y * v2.X
				);
		}

		public Vector3 CrossProduct(Vector3 other)
		{
			return CrossProduct(this, other);
		}

		public static double DotProduct(Vector3 v1, Vector3 v2)
		{
			return
			(
				v1.X * v2.X +
				v1.Y * v2.Y +
				v1.Z * v2.Z
			);
		}

		public double DotProduct(Vector3 other)
		{
			return DotProduct(this, other);
		}

		public static Vector3 operator /(Vector3 v1, double s2)
		{
			return
			(
				new Vector3
				(
					v1.X / s2,
					v1.Y / s2,
					v1.Z / s2
				)
			);
		}

		public static bool IsUnitVector(Vector3 v1)
		{
			return v1.Magnitude == 1;
		}

		public bool IsUnitVector()
		{
			return IsUnitVector(this);
		}

		public bool IsUnitVector(double tolerance)
		{
			return IsUnitVector(this, tolerance);
		}

		public static bool IsUnitVector(Vector3 v1, double tolerance)
		{
			return AlmostEqualsWithAbsTolerance(v1.Magnitude, 1, tolerance);
		}

		public static Vector3 Normalize(Vector3 v1)
		{
			var magnitude = v1.Magnitude;

			// Check that we are not attempting to normalize a vector of magnitude 0
			if (magnitude == 0)
			{
				throw new NormalizeVectorException(NORMALIZE_0);
			}

			// Check that we are not attempting to normalize a vector of magnitude NaN
			if (double.IsNaN(magnitude))
			{
				throw new NormalizeVectorException(NORMALIZE_NaN);
			}

			// Special Cases
			if (double.IsInfinity(v1.Magnitude))
			{
				var x =
					v1.X == 0 ? 0 :
					v1.X == -0 ? -0 :
					double.IsPositiveInfinity(v1.X) ? 1 :
					double.IsNegativeInfinity(v1.X) ? -1 :
					double.NaN;
				var y =
					v1.Y == 0 ? 0 :
					v1.Y == -0 ? -0 :
					double.IsPositiveInfinity(v1.Y) ? 1 :
					double.IsNegativeInfinity(v1.Y) ? -1 :
					double.NaN;
				var z =
					v1.Z == 0 ? 0 :
					v1.Z == -0 ? -0 :
					double.IsPositiveInfinity(v1.Z) ? 1 :
					double.IsNegativeInfinity(v1.Z) ? -1 :
					double.NaN;

				var result = new Vector3(x, y, z);

				// If this wasnt' a special case throw an exception
				if (result.IsNaN())
				{
					throw new NormalizeVectorException(NORMALIZE_Inf);
				}

				// If this was a special case return the special case result
				return result;
			}

			// Run the normalization as usual
			return NormalizeOrNaN(v1);
		}


		public Vector3 Normalize()
		{
			return Normalize(this);
		}


		private static Vector3 NormalizeOrNaN(Vector3 v1)
		{
			// find the inverse of the vectors magnitude
			double inverse = 1 / v1.Magnitude;

			return new Vector3(
				// multiply each component by the inverse of the magnitude
				v1.X * inverse,
				v1.Y * inverse,
				v1.Z * inverse);
		}

		private const string NORMALIZE_Inf =
			"Cannot normalize a vector when it's magnitude is Inf";


		private const string NORMALIZE_0 =
			"Cannot normalize a vector when it's magnitude is zero";

		private const string NORMALIZE_NaN =
			"Cannot normalize a vector when it's magnitude is NaN";

		public static Vector3 NormalizeOrDefault(Vector3 v1)
		{
			/* Check that we are not attempting to normalize a vector of magnitude 1;
			   if we are then return v(0,0,0) */
			if (v1.Magnitude == 0)
			{
				return Origin;
			}

			/* Check that we are not attempting to normalize a vector with NaN components;
			   if we are then return v(NaN,NaN,NaN) */
			if (v1.IsNaN())
			{
				return NaN;
			}

			// Special Cases
			if (double.IsInfinity(v1.Magnitude))
			{
				var x =
					v1.X == 0 ? 0 :
					v1.X == -0 ? -0 :
					double.IsPositiveInfinity(v1.X) ? 1 :
					double.IsNegativeInfinity(v1.X) ? -1 :
					double.NaN;
				var y =
					v1.Y == 0 ? 0 :
					v1.Y == -0 ? -0 :
					double.IsPositiveInfinity(v1.Y) ? 1 :
					double.IsNegativeInfinity(v1.Y) ? -1 :
					double.NaN;

				var z =
					v1.Z == 0 ? 0 :
					v1.Z == -0 ? -0 :
					double.IsPositiveInfinity(v1.Z) ? 1 :
					double.IsNegativeInfinity(v1.Z) ? -1 :
					double.NaN;

				var result = new Vector3(x, y, z);

				// If this was a special case return the special case result otherwise return NaN
				return result.IsNaN() ? NaN : result;
			}

			// Run the normalization as usual
			return NormalizeOrNaN(v1);
		}

		public Vector3 NormalizeOrDefault()
		{
			return NormalizeOrDefault(this);
		}

		public static Double Abs(Vector3 v1)
		{
			return v1.Magnitude;
		}

		public double Abs()
		{
			return this.Magnitude;
		}

		public static double Angle(Vector3 v1, Vector3 v2)
		{
			if (v1 == v2)
			{
				return 0;
			}

			return
				Math.Acos(
					Math.Min(1.0f, NormalizeOrDefault(v1).DotProduct(NormalizeOrDefault(v2))));
		}

		public double Angle(Vector3 other)
		{
			return Angle(this, other);
		}

		public static bool IsBackFace(Vector3 normal, Vector3 lineOfSight)
		{
			return normal.DotProduct(lineOfSight) < 0;
		}

		public bool IsBackFace(Vector3 lineOfSight)
		{
			return IsBackFace(this, lineOfSight);
		}

		public static double Distance(Vector3 v1, Vector3 v2)
		{
			return
				Math.Sqrt
				(
					(v1.X - v2.X) * (v1.X - v2.X) +
					(v1.Y - v2.Y) * (v1.Y - v2.Y) +
					(v1.Z - v2.Z) * (v1.Z - v2.Z)
				);
		}

		public double Distance(Vector3 other)
		{
			return Distance(this, other);
		}

		public static Vector3 Max(Vector3 v1, Vector3 v2)
		{
			return v1 >= v2 ? v1 : v2;
		}

		public Vector3 Max(Vector3 other)
		{
			return Max(this, other);
		}

		public static Vector3 Min(Vector3 v1, Vector3 v2)
		{
			return v1 <= v2 ? v1 : v2;
		}

		public Vector3 Min(Vector3 other)
		{
			return Min(this, other);
		}

		public static double MixedProduct(Vector3 v1, Vector3 v2, Vector3 v3)
		{
			return DotProduct(CrossProduct(v1, v2), v3);
		}

		public double MixedProduct(Vector3 other_v1, Vector3 other_v2)
		{
			return DotProduct(CrossProduct(this, other_v1), other_v2);
		}

		public static bool IsPerpendicular(Vector3 v1, Vector3 v2)
		{
			// Use normalization of special cases to handle special cases of IsPerpendicular
			v1 = NormalizeSpecialCasesOrOrigional(v1);
			v2 = NormalizeSpecialCasesOrOrigional(v2);

			// If either vector is vector(0,0,0) the vectors are not perpendicular
			if (v1 == Zero || v2 == Zero)
			{
				return false;
			}

			// Is perpendicular
			return v1.DotProduct(v2).Equals(0);
		}

		public bool IsPerpendicular(Vector3 other)
		{
			return IsPerpendicular(this, other);
		}

// Helpers
		private static Vector3 NormalizeSpecialCasesOrOrigional(Vector3 v1)
		{
			if (double.IsInfinity(v1.Magnitude))
			{
				var x =
					v1.X == 0 ? 0 :
					v1.X == -0 ? -0 :
					double.IsPositiveInfinity(v1.X) ? 1 :
					double.IsNegativeInfinity(v1.X) ? -1 :
					double.NaN;
				var y =
					v1.Y == 0 ? 0 :
					v1.Y == -0 ? -0 :
					double.IsPositiveInfinity(v1.Y) ? 1 :
					double.IsNegativeInfinity(v1.Y) ? -1 :
					double.NaN;
				var z =
					v1.Z == 0 ? 0 :
					v1.Z == -0 ? -0 :
					double.IsPositiveInfinity(v1.Z) ? 1 :
					double.IsNegativeInfinity(v1.Z) ? -1 :
					double.NaN;

				return new Vector3(x, y, z);
			}

			return v1;
		}

		public static Vector3 ProjectOnPlane(Vector3 from, Vector3 to)
		{
			var sqrMag = DotProduct(to, to);
			var dot = DotProduct(from, to);
			return new Vector3(
				from.X - to.X * dot / sqrMag,
				from.Y - to.Y * dot / sqrMag,
				from.Z - to.Z * dot / sqrMag
				);
		}

		public static Vector3 Projection(Vector3 v1, Vector3 v2)
		{
			return new Vector3(v2 * (v1.DotProduct(v2) / Math.Pow(v2.Magnitude, 2)));
		}

		public Vector3 Projection(Vector3 direction)
		{
			return Projection(this, direction);
		}

		public static Vector3 Rejection(Vector3 v1, Vector3 v2)
		{
			return v1 - v1.Projection(v2);
		}

		public Vector3 Rejection(Vector3 direction)
		{
			return Rejection(this, direction);
		}

		public Vector3 Reflection(Vector3 reflector)
		{
			this = Vector3.Reflection(this, reflector);
			return this;
		}

		public static Vector3 Reflection(Vector3 v1, Vector3 v2)
		{
			// if v2 has a right angle to vector, return -vector and stop
			if (Math.Abs(Math.Abs(v1.Angle(v2)) - Math.PI / 2) < Double.Epsilon)
			{
				return -v1;
			}

			Vector3 retval = new Vector3(2 * v1.Projection(v2) - v1);
			return retval.Scale(v1.Magnitude);
		}

		public static Vector3 RotateX(Vector3 v1, double rad)
		{
			double x = v1.X;
			double y = (v1.Y * Math.Cos(rad)) - (v1.Z * Math.Sin(rad));
			double z = (v1.Y * Math.Sin(rad)) + (v1.Z * Math.Cos(rad));
			return new Vector3(x, y, z);
		}

		public Vector3 RotateX(double rad)
		{
			return RotateX(this, rad);
		}

		public static Vector3 Pitch(Vector3 v1, double rad)
		{
			return RotateX(v1, rad);
		}

		public Vector3 Pitch(double rad)
		{
			return Pitch(this, rad);
		}

		public static Vector3 RotateY(Vector3 v1, double rad)
		{
			double x = (v1.Z * Math.Sin(rad)) + (v1.X * Math.Cos(rad));
			double y = v1.Y;
			double z = (v1.Z * Math.Cos(rad)) - (v1.X * Math.Sin(rad));
			return new Vector3(x, y, z);
		}

		public Vector3 RotateY(double rad)
		{
			return RotateY(this, rad);
		}

		public static Vector3 Yaw(Vector3 v1, double rad)
		{
			return RotateY(v1, rad);
		}

		public Vector3 Yaw(double rad)
		{
			return Yaw(this, rad);
		}

		public static Vector3 RotateZ(Vector3 v1, double rad)
		{
			double x = (v1.X * Math.Cos(rad)) - (v1.Y * Math.Sin(rad));
			double y = (v1.X * Math.Sin(rad)) + (v1.Y * Math.Cos(rad));
			double z = v1.Z;
			return new Vector3(x, y, z);
		}

		public Vector3 RotateZ(double rad)
		{
			return RotateZ(this, rad);
		}

		public static Vector3 Roll(Vector3 v1, double rad)
		{
			return RotateZ(v1, rad);
		}

		public Vector3 Roll(double rad)
		{
			return Roll(this, rad);
		}

		public static Vector3 RotateX(Vector3 v1, double yOff, double zOff, double rad)
		{
			double x = v1.X;
			double y =
				(v1.Y * Math.Cos(rad)) - (v1.Z * Math.Sin(rad)) +
				(yOff * (1 - Math.Cos(rad)) + zOff * Math.Sin(rad));
			double z =
				(v1.Y * Math.Sin(rad)) + (v1.Z * Math.Cos(rad)) +
				(zOff * (1 - Math.Cos(rad)) - yOff * Math.Sin(rad));
			return new Vector3(x, y, z);
		}

		public Vector3 RotateX(double yOff, double zOff, double rad)
		{
			return RotateX(this, yOff, zOff, rad);
		}

		public static Vector3 RotateY(Vector3 v1, double xOff, double zOff, double rad)
		{
			double x =
				(v1.Z * Math.Sin(rad)) + (v1.X * Math.Cos(rad)) +
				(xOff * (1 - Math.Cos(rad)) - zOff * Math.Sin(rad));
			double y = v1.Y;
			double z =
				(v1.Z * Math.Cos(rad)) - (v1.X * Math.Sin(rad)) +
				(zOff * (1 - Math.Cos(rad)) + xOff * Math.Sin(rad));
			return new Vector3(x, y, z);
		}

		public Vector3 RotateY(double xOff, double zOff, double rad)
		{
			return RotateY(this, xOff, zOff, rad);
		}

		public static Vector3 RotateZ(Vector3 v1, double xOff, double yOff, double rad)
		{
			double x =
				(v1.X * Math.Cos(rad)) - (v1.Y * Math.Sin(rad)) +
				(xOff * (1 - Math.Cos(rad)) + yOff * Math.Sin(rad));
			double y =
				(v1.X * Math.Sin(rad)) + (v1.Y * Math.Cos(rad)) +
				(yOff * (1 - Math.Cos(rad)) - xOff * Math.Sin(rad));
			double z = v1.Z;
			return new Vector3(x, y, z);
		}

		public Vector3 RotateZ(double xOff, double yOff, double rad)
		{
			return RotateZ(this, xOff, yOff, rad);
		}

		public static Vector3 Round(Vector3 v1)
		{
			return new Vector3(Math.Round(v1.X), Math.Round(v1.Y), Math.Round(v1.Z));
		}

		public static Vector3 Round(Vector3 v1, MidpointRounding mode)
		{
			return new Vector3(
				Math.Round(v1.X, mode),
				Math.Round(v1.Y, mode),
				Math.Round(v1.Z, mode));
		}

		public Vector3 Round()
		{
			return new Vector3(Math.Round(this.X), Math.Round(this.Y), Math.Round(this.Z));
		}

		public Vector3 Round(MidpointRounding mode)
		{
			return new Vector3(
				Math.Round(this.X, mode),
				Math.Round(this.Y, mode),
				Math.Round(this.Z, mode));
		}

		public static Vector3 Round(Vector3 v1, int digits)
		{
			return new Vector3(
				Math.Round(v1.X, digits),
				Math.Round(v1.Y, digits),
				Math.Round(v1.Z, digits));
		}

		public static Vector3 Round(Vector3 v1, int digits, MidpointRounding mode)
		{
			return new Vector3(
				Math.Round(v1.X, digits, mode),
				Math.Round(v1.Y, digits, mode),
				Math.Round(v1.Z, digits, mode));
		}

		public Vector3 Round(int digits)
		{
			return new Vector3(
				Math.Round(this.X, digits),
				Math.Round(this.Y, digits),
				Math.Round(this.Z, digits));
		}

		public Vector3 Round(int digits, MidpointRounding mode)
		{
			return new Vector3(
				Math.Round(this.X, digits, mode),
				Math.Round(this.Y, digits, mode),
				Math.Round(this.Z, digits, mode));
		}

		public static Vector3 Scale(Vector3 vector, double magnitude)
		{
			if (magnitude < 0)
			{
				throw new ArgumentOutOfRangeException("magnitude", magnitude, NEGATIVE_MAGNITUDE);
			}

			if (vector == new Vector3(0, 0, 0))
			{
				throw new ArgumentException(ORIGIN_VECTOR_MAGNITUDE, "vector");
			}

			return vector * (magnitude / vector.Magnitude);
		}

		public Vector3 Scale(double magnitude)
		{
			return Vector3.Scale(this, magnitude);
		}

		private const string NEGATIVE_MAGNITUDE =
			"The magnitude of a Vector3 must be a positive value, (i.e. greater than 0)";

		private const string ORIGIN_VECTOR_MAGNITUDE =
			"Cannot change the magnitude of Vector3(0,0,0)";

		public static double SumComponents(Vector3 v1)
		{
			return (v1.X + v1.Y + v1.Z);
		}

		public double SumComponents()
		{
			return SumComponents(this);
		}

		public static Vector3 PowComponents(Vector3 v1, double power)
		{
			return
				new Vector3
				(
					Math.Pow(v1.X, power),
					Math.Pow(v1.Y, power),
					Math.Pow(v1.Z, power)
				);
		}

		public void PowComponents(double power)
		{
			this = PowComponents(this, power);
		}

		public static Vector3 SqrtComponents(Vector3 v1)
		{
			return
			(
				new Vector3
				(
					Math.Sqrt(v1.X),
					Math.Sqrt(v1.Y),
					Math.Sqrt(v1.Z)
				)
			);
		}

		public void SqrtComponents()
		{
			this = SqrtComponents(this);
		}

		public static Vector3 SqrComponents(Vector3 v1)
		{
			return
			(
				new Vector3
				(
					v1.X * v1.X,
					v1.Y * v1.Y,
					v1.Z * v1.Z
				)
			);
		}

		public void SqrComponents()
		{
			this = SqrtComponents(this);
		}

		public static double SumComponentSqrs(Vector3 v1)
		{
			Vector3 v2 = SqrComponents(v1);
			return v2.SumComponents();
		}

		public double SumComponentSqrs()
		{
			return SumComponentSqrs(this);
		}

		public static bool IsNaN(Vector3 v1)
		{
			return double.IsNaN(v1.X) || double.IsNaN(v1.Y) || double.IsNaN(v1.Z);
		}

		public bool IsNaN()
		{
			return IsNaN(this);
		}

		public string ToVerbString()
		{
			string output = null;

			if (IsUnitVector())
			{
				output += UNIT_VECTOR;
			}
			else
			{
				output += POSITIONAL_VECTOR;
			}

			output += string.Format("( x={0}, y={1}, z={2})", X, Y, Z);
			output += MAGNITUDE + Magnitude;
			return output;
		}

		private const string UNIT_VECTOR =
			"Unit vector composing of ";

		private const string POSITIONAL_VECTOR =
			"Positional vector composing of ";

		private const string MAGNITUDE =
			" of magnitude ";

		public string ToString(string format, IFormatProvider formatProvider)
		{
			// If no format is passed
			if (format == null || format == "")
				return String.Format("({0}, {1}, {2})", X, Y, Z);

			char firstChar = format[0];
			string remainder = null;

			if (format.Length > 1)
				remainder = format.Substring(1);

			switch (firstChar)
			{
				case 'x':
					return X.ToString(remainder, formatProvider);
				case 'y':
					return Y.ToString(remainder, formatProvider);
				case 'z':
					return Z.ToString(remainder, formatProvider);
				default:
					return
						String.Format
						(
							"({0}, {1}, {2})",
							X.ToString(format, formatProvider),
							Y.ToString(format, formatProvider),
							Z.ToString(format, formatProvider)
						);
			}
		}

		public override string ToString()
		{
			return ToString(null, null);
		}


		public class NormalizeVectorException : Exception
		{
			public NormalizeVectorException(object normalizeInf)
			{
				throw new NotImplementedException();
			}
		}
	}
}
