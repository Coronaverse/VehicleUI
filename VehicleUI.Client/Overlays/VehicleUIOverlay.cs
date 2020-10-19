using Newtonsoft.Json;
using NFive.SDK.Client.Interface;

namespace Coronaverse.VehicleUI.Client.Overlays
{
	public class VehicleUIOverlay : Overlay
	{
		public VehicleUIOverlay(IOverlayManager manager) : base(manager) { }

		public void UpdateVehicle(VehicleNUIData data)
		{
			Emit("updateVehicle", JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
		}
	}
}
