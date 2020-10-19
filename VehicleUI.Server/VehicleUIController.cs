using JetBrains.Annotations;
using NFive.SDK.Core.Diagnostics;
using NFive.SDK.Server.Communications;
using NFive.SDK.Server.Controllers;
using Coronaverse.VehicleUI.Shared;

namespace Coronaverse.VehicleUI.Server
{
	[PublicAPI]
	public class VehicleUIController : ConfigurableController<Configuration>
	{
		public VehicleUIController(ILogger logger, Configuration configuration, ICommunicationManager comms) : base(logger, configuration)
		{
			// Send configuration when requested
			comms.Event(VehicleUIEvents.Configuration).FromClients().OnRequest(e => e.Reply(this.Configuration));

			// Sync Indicator lights to all clients
			comms.Event(VehicleUIEvents.SyncIndicatorLights).FromClients().OnRequest<int>((e, status) =>
			{
				comms.Event(VehicleUIEvents.SyncIndicatorLights).ToClients().Emit(e.Client.Handle, status);
			});
		}
	}
}
