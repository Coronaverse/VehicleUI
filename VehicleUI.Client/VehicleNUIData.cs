using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coronaverse.VehicleUI.Client
{
	public class VehicleNUIData
	{
		public bool status { get; set; }
		public double? speed { get; set; }
		public double? nail { get; set; }
		public int? gear { get; set; }
		public float? fuel { get; set; }
		[DefaultValue("")]
		public string lights { get; set; }
		public int? signals { get; set; }
		public bool? cruiser { get; set; }
		public int? type { get; set; }
		public bool? siren { get; set; }
		public bool? seatbelt { get; set; }

		public VehicleNUIInfoConfig config { get; set; }

		public class VehicleNUIInfoConfig
		{
			public double maxSpeed { get; set; }
			public bool metricUnit { get; set; }
		}
	}
}
