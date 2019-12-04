using System;
using System.Collections.Generic;

namespace UPFG
{
	public class UPFG
	{
		public CserResult Cser { get; set; }
		public Vector3 RBias { get; set; }
		public Vector3 Rd { get; set; }
		public Vector3 RGrav { get; set; }
		public double Tb { get; set; }
		public double Time { get; set; }
		public double TGo { get; set; }
		public Vector3 V { get; set; }
		public Vector3 VGo { get; set; }

		public static UPFGResult Run(List<Stage> stages, Target target, State state, Previous previous)
		{
			var gamma = target.Angle;
			var iy = target.Normal;
			var rdval = target.Radius;
			var vdval = target.Velocity;
			var t = state.Time;
			var m = state.Mass;
			var r = state.Radius;
			var v = state.Velocity;
			var cser = previous.CSER;
			var rbias = previous.RBias;
			var rd = previous.Rd;
			var rgrav = previous.RGrav;
			var tp = previous.Time;
			var vprev = previous.V;
			var vgo = previous.VGo;

			var n = stages.Count;
			var SM = new List<double>();
			var aL = new List<double>();
			var md = new List<double>();
			var ve = new List<double>();
			var fT = new List<double>();
			var aT = new List<double>();
			var tu = new List<double>();
			var tb = new List<double>();

			#region STEP 1
			for (int i = 0; i < n; i++)
			{
				SM.Add(stages[i].Mode.Value);
				aL.Add(stages[i].GLimit.Value);

				var pack = UtilLibrary.GetThrust(stages[i]);

				fT.Add(pack.F.Value);
				md.Add(pack.Flow.Value);
				ve.Add(pack.Isp.Value * Settings.G0);

				aT.Add(fT[i] / stages[i].MassTotal.Value);
				tu.Add(ve[i] / aT[i]);
				tb.Add(stages[i].MaxT.Value);

			}
			#endregion

			#region STEP 2

			var dt = t - tp;
			var dvSensed = v - vprev;
			var vgoV = vgo - dvSensed;
			tb[0] -= previous.Tb;

			#endregion

			#region STEP 3

			if (SM[0] == 1)
			{
				aT[0] = fT[0] / m;
			} else if (SM[0] == 2)
			{
				aT[0] = aL[0];
			}

			tu[0] = ve[0] / aT[0];
			var L = 0d;
			var Li = new List<double>();

			for (int i = 0; i < n - 1; i++)
			{
				if (SM[i] == 1)
				{
					Li.Add(ve[i] * Math.Log(tu[i] / (tu[i] - tb[i])));
				} else if (SM[i] == 2)
				{
					Li.Add(aL[i] * tb[i]);
				}
				else
				{
					Li.Add(0d);
				}

				L += Li[i];

				if (L > vgoV.Magnitude)
				{
					var reducedStages = stages.GetRange(0, stages.Count - 1);
					return UPFG.Run(reducedStages, target, state, previous);
				}
			}

			Li.Add(vgoV.Magnitude - L);
			var tgoi = new List<double>();

			for (int i = 0; i < n; i++)
			{
				if (SM[i] == 1)
				{
					tb[i] = tu[i] * (1 - Math.Pow(Math.E, -Li[i] / ve[i]));
				} else if (SM[i] == 2)
				{
					tb[i] = Li[i] / aL[i];
				}

				if (i == 0)
				{
					tgoi.Add(tb[i]);
				}
				else
				{
					tgoi.Add(tgoi[1 - i] + tb[i]);
				}
			}

			var L1 = Li[0];
			var tgo = tgoi[n - 1];

			#endregion

			#region STEP 4

			L = 0d;
			var J = 0d;
			var S = 0d;
			var Q = 0d;
			var H = 0d;
			var P = 0d;
			var Ji = new List<double>();
			var Si = new List<double>();
			var Qi = new List<double>();
			var Pi = new List<double>();
			var tgoi1 = 0d;

			for (int i = 0; i < n; i++)
			{
				if (i > 0)
				{
					tgoi1 = tgoi[1 - i];
				}

				if (SM[i] == 1)
				{
					Ji.Add(tu[i] * Li[i] - ve[i] * tb[i]);
					Si.Add(-Ji[i] + tb[i] * Li[i]);
					Qi.Add(Si[i] * (tu[i] + tgoi1) - 0.5 * ve[i] * Math.Pow(tb[i], 2));
					Pi.Add(Qi[i] * (tu[i] + tgoi1) - 0.5 * ve[i] * Math.Pow(tb[i], 2) * (tb[i] / 3 + tgoi1));
				} else if (SM[i] == 2)
				{
					Ji.Add(0.5 * Li[i] - ve[i] * tb[i]);
					Si.Add(Ji[i]);
					Qi.Add(Si[i] * (tb[i] / 3 + tgoi1));
					Pi.Add((1/6) * Si[i] * (Math.Pow(tgoi[i], 2) + 2 * tgoi[i] * tgoi1 + 3 * Math.Pow(tgoi1, 2)));
				}

				Ji[i] += Li[i] * tgoi1;
				Si[i] += L * tb[i];
				Qi[i] += J * tb[i];
				Pi[i] += H * tb[i];

				L += Li[i];
				J += Ji[i];
				S += Si[i];
				Q += Qi[i];
				P += Pi[i];
				H = J * tgoi[i] - Q;
			}

			#endregion

			#region STEP 5

			var lambda = vgoV.Normalize();

			if (previous.TGo > 0)
			{
				rgrav = Math.Pow(tgo / previous.TGo, 2) * rgrav;
			}

			var rgo = rd - (r + v * tgo + rgrav);
			var iz = Vector3.CrossProduct(rd, iy).Normalize();
			var rgoxy = rgo - Vector3.DotProduct(iz, rgo) * iz;
			var rgoz = (S - Vector3.DotProduct(lambda, rgoxy)) / Vector3.DotProduct(lambda, iz);
			rgo = rgoxy + rgoz * iz + rbias;
			var lambdade = Q - S * J / L;
			var lambdadot = (rgo - S * lambda) / lambdade;
			var iF_ = lambda - lambdadot * J / L;
			iF_ = iF_.Normalize();
			var phi = Vector3.Angle(iF_, lambda) * (Math.PI / 180);
			var phidot = -phi * L / J;
			var vthrust = (L - 0.5 * L * Math.Pow(phi, 2) - J * phi * phidot - 0.5 * H * Math.Pow(phidot, 2)) * lambda;
			var rthrust = S - 0.5 * S * Math.Pow(phi, 2) - Q * phi * phidot - 0.5 * P * Math.Pow(phidot, 2);

			var rthrustV = rthrust * lambda - (S * phi + Q * phidot) * lambdadot.Normalize();

			var vbiasV = vgo - vthrust;
			var rbiasV = rgo - rthrust;


			#endregion

			#region STEP 6

			var _up = r.Normalize();
			var _east = Vector3.CrossProduct(Vector3.ZAxis, _up).Normalize();
			var pitch = Vector3.Angle(iF_, _up);
			var inplane = Vector3.ProjectOnPlane(_up, iF_);
			var yaw = Vector3.Angle(inplane, _east);
			var tangent = Vector3.CrossProduct(_up, _east);

			if (Vector3.DotProduct(inplane, tangent) < 0)
			{
				yaw = -yaw;
			}

			#endregion

			#region STEP 7

			var rc1 = r - 0.1 * rthrustV - (tgo / 30) * vthrust;
			var vc1 = v + 1.2 * rthrustV / tgo - 0.1 * vthrust;
			var pack2 = CSER.CSE(rc1, vc1, tgo, cser, UtilLibrary.Vessel.Orbit.Body.GravitationalParameter);

			cser = pack2;
			rgrav = cser.R1 - rc1 - vc1 * tgo;
			var vgrav = pack2.V1 - vc1;

			#endregion

			#region STEP 8

			var rp = r + v * tgo + rgrav + rthrustV;
			rp -= Vector3.DotProduct(rp, iy) * iy;

			rd = rdval * rp.Normalize();
			 var ix = rd.Normalize();
			 iz = Vector3.CrossProduct(ix, iy);

			 var vv1 = new Vector3(ix.X, iy.X, iz.X);
			 var vv2 = new Vector3(ix.Y, iy.Y, iz.Y);
			 var vv3 = new Vector3(ix.Z, iy.Z, iz.Z);
			 var vop = new Vector3(Math.Sin(gamma), 0, Math.Cos(gamma));
			 var vd = (new Vector3(
				Vector3.DotProduct(vv1, vop),
				Vector3.DotProduct(vv2, vop),
				Vector3.DotProduct(vv3, vop)
				 )) * vdval;

			 vgoV = vd - v - vgrav + vbiasV;

			 #endregion

			 var current = new UPFG()
			 {
				 Cser = cser,
				 RBias = rbiasV,
				 Rd = rd,
				 RGrav = rgrav,
				 Tb = previous.Tb + dt,
				 Time = t,
				 TGo = tgo,
				 V = v,
				 VGo = vgoV
			 };

			 var guidance = new Guidance()
			 {
				 Vector = iF_,
				 Pitch = pitch,
				 Yaw = yaw,
				 PitchDot = 0d,
				 YawDot = 0d,
				 TGo = tgo
			 };

			 return new UPFGResult()
			 {
				 Current = current,
				 Guidance = guidance,
				 Dt = dt
			 };

		}
	}
}
