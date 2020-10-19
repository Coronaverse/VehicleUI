using System.Data.Entity;
using NFive.SDK.Core.Models.Player;
using NFive.SDK.Server.Storage;

namespace Coronaverse.VehicleUI.Server.Storage
{
	public class StorageContext : EFContext<StorageContext>
	{
		public DbSet<User> Users { get; set; }
	}
}
