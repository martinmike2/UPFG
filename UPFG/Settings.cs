namespace UPFG
{
	public class Settings
	{
		public double PitchOverTimeLimit { get; set; }

		public double UpfgConvergenceDelay { get; set; }

		public double UpfgFinalizationTime { get; set; }

		public double StagingKillRotTime { get; set; }

		public double UpfgConvergenceCriterion { get; set; }

		public double UpfgGoodsSolutionCriterion { get; set; }

		public static double G0 { get; set; }

		public static Vector3 CurrentNode { get; set; }

		public static int UpfgStage { get; set; }

		public UPFGTarget UpfgTarget { get; set; }

		public double TimeToOrbitIntercept { get; set; }

		public double LiftoffTime { get; set; }
		public double SteeringRoll { get; set; }


		public Settings()
		{
			G0 = 9.8067;
			PitchOverTimeLimit = 20;
			UpfgConvergenceDelay = 5;
			UpfgFinalizationTime = 5;
			StagingKillRotTime = 5;
			UpfgConvergenceCriterion = 0.1;
			UpfgGoodsSolutionCriterion = 15;
		}
	}
}
