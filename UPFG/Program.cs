using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json;

namespace UPFG
{
    class Program
    {
        static void Main(string[] args)
        {
	        var package = LoadMission();

	        Settings settings = new Settings();
	        Mission mission = package.Mission;
	        Controls controls = package.Controls;
	        Vehicle vehicle = package.Vehicle;


            var currentTime = (double)DateTime.Now.ToUniversalTime().ToTimestamp().Seconds;

            mission = UtilLibrary.MissionSetup(mission, new Controls());

            settings.UpfgTarget = UtilLibrary.TargetSetup(mission);
            settings.TimeToOrbitIntercept = UtilLibrary.OrbitInterceptTime(mission, mission.Direction.Value);
            settings.LiftoffTime = currentTime + settings.TimeToOrbitIntercept - controls.LaunchTimeAdvance;

            if (settings.TimeToOrbitIntercept < controls.LaunchTimeAdvance)
            {
	            settings.LiftoffTime += UtilLibrary.Vessel.Orbit.Body.RotationalPeriod;
            }

            if (mission.LaunchAzimuth == null)
            {
	            mission.LaunchAzimuth = UtilLibrary.LaunchAzimuth(mission, settings.UpfgTarget);
            }

            if (controls.InitialRoll != null)
            {
	            settings.SteeringRoll = controls.InitialRoll.Value;
            }

            UtilLibrary.SetVehicle(vehicle, mission, controls);

        }

        private static Package LoadMission()
        {
	        string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"missions/test.json");
	        string[] iStr = File.ReadAllLines(path);
	        string str = "";

	        foreach (var s in iStr)
	        {
		        str += s;
	        }

	        return JsonConvert.DeserializeObject<Package>(str);
        }
    }
}
