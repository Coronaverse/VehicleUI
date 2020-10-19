using JetBrains.Annotations;
using NFive.SDK.Server.Migrations;
using Coronaverse.VehicleUI.Server.Storage;

namespace Coronaverse.VehicleUI.Server.Migrations
{
	[UsedImplicitly]
	public sealed class Configuration : MigrationConfiguration<StorageContext> { }
}
