using NFive.SDK.Core.Controllers;

namespace Coronaverse.VehicleUI.Shared
{
	public class Configuration : ControllerConfiguration
	{
		public string Example { get; set; } = "Hello World";
		public double MaxSpeed { get; set; } = 240;
		public double EjectSpeed { get; set; } = 45.0;
		public double EjectAccel { get; set; } = 100.0;
	}
}
