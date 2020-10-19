using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NFive.SDK.Client.Commands;
using NFive.SDK.Client.Communications;
using NFive.SDK.Client.Events;
using NFive.SDK.Client.Interface;
using NFive.SDK.Client.Services;
using NFive.SDK.Core.Diagnostics;
using NFive.SDK.Core.Models.Player;
using Coronaverse.VehicleUI.Client.Overlays;
using Coronaverse.VehicleUI.Shared;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;
using System.Collections.Generic;

namespace Coronaverse.VehicleUI.Client
{
	[PublicAPI]
	public class VehicleUIService : Service
	{
		private Configuration config;
		private VehicleUIOverlay overlay;
		private List<int> vehicleCarClasses = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 17, 18, 19, 20 };
		private double currentSpeed = 0.0;
		private bool seatbeltIsOn = false;
		private Vector3 prevVelocity = new Vector3(0.0f, 0.0f, 0.0f);
		private bool cruiseControl = false;

		public enum IndicatorState
		{
			OFF = 0,
			LEFT = 1,
			RIGHT = 2,
			BOTH = 3
		}

		public VehicleUIService(ILogger logger, ITickManager ticks, ICommunicationManager comms, ICommandManager commands, IOverlayManager overlay, User user) : base(logger, ticks, comms, commands, overlay, user)
		{
			

		}

		public override async Task Started()
		{
			// Request server configuration
			this.config = await this.Comms.Event(VehicleUIEvents.Configuration).ToServer().Request<Configuration>();

			this.Logger.Debug($"From server config: {this.config.Example}");

			// Create overlay
			this.overlay = new VehicleUIOverlay(this.OverlayManager);

			// Attach a tick handler
			this.Ticks.On(OnTick);

			// Handle Car indicator syncing
			Comms.Event(VehicleUIEvents.SyncIndicatorLights).FromServer().OnRequest<int, int>((cb, id, status) =>
			{
				int vehicle = GetVehiclePedIsIn(GetPlayerPed(GetPlayerFromServerId(id)), false);
				bool leftLight = false, rightLight = false;
				switch (status)
				{
					case 1:
						leftLight = true;
						break;
					case 2:
						rightLight = true;
						break;
					case 3:
						leftLight = true;
						rightLight = true;
						break;
				}

				SetVehicleIndicatorLights(vehicle, 0, rightLight);
				SetVehicleIndicatorLights(vehicle, 1, leftLight);
			});
		}

		private void CreateKeybindings()
		{
			RegisterCommand("showvehicleinfo", new Action<int, List<object>, string>((source, args, raw) =>
			{
				int player = GetPlayerPed(-1);
				int vehicle = GetVehiclePedIsIn(player, false);
				int vehicleClass = GetVehicleClass(vehicle);
				Debug.WriteLine($"Player: ${player}\nVehicle: ${vehicle}\nvClass: ${vehicleClass}");
			}), false);
			// Seatbelt command
			RegisterCommand("+seatbelt", new Action<int, List<object>, string>((source, args, raw) =>
			{
				seatbeltIsOn = !seatbeltIsOn;
			}), false);
			RegisterCommand("-seatbelt", new Action<int, List<object>, string>((source, args, raw) =>
			{

			}), false);
			RegisterKeyMapping("+seatbelt", "Seatbelt", "keybaord", "k");

			RegisterCommand("+indicatorLeft", new Action<int, List<object>, string>((source, args, raw) =>
			{
				int localPlayer = GetPlayerPed(-1);
				int vehicle = GetVehiclePedIsIn(localPlayer, false);
				if (vehicle == 0)
					return;
				int vehicleClass = GetVehicleClass(vehicle);
				if (vehicleCarClasses.Contains(vehicleClass))
				{
					int lightState = GetVehicleIndicatorLights(vehicle);
					if (lightState == (int)IndicatorState.OFF)
					{
						Comms.Event(VehicleUIEvents.SyncIndicatorLights).ToServer().Emit((int) IndicatorState.LEFT);
					}
					else
					{
						Comms.Event(VehicleUIEvents.SyncIndicatorLights).ToServer().Emit((int)IndicatorState.OFF);
					}
				}
			}), false);

			RegisterCommand("-indicatorLeft", new Action<int, List<object>, string>((source, args, raw) =>
			{

			}), false);
			RegisterKeyMapping("+indicatorLeft", "Left Indicator Signal (in vehicle)", "keyboard", "LEFT");

			RegisterCommand("+indicatorRight", new Action<int, List<object>, string>((source, args, raw) =>
			{
				int localPlayer = GetPlayerPed(-1);
				int vehicle = GetVehiclePedIsIn(localPlayer, false);
				if (vehicle == 0)
					return;
				int vehicleClass = GetVehicleClass(vehicle);
				if (vehicleCarClasses.Contains(vehicleClass))
				{
					int lightState = GetVehicleIndicatorLights(vehicle);
					if (lightState == (int)IndicatorState.OFF)
					{
						Comms.Event(VehicleUIEvents.SyncIndicatorLights).ToServer().Emit((int)IndicatorState.RIGHT);
					}
					else
					{
						Comms.Event(VehicleUIEvents.SyncIndicatorLights).ToServer().Emit((int)IndicatorState.OFF);
					}
				}
			}), false);

			RegisterCommand("-indicatorRight", new Action<int, List<object>, string>((source, args, raw) =>
			{

			}), false);
			RegisterKeyMapping("+indicatorRight", "Right Indicator Signal (in vehicle)", "keyboard", "RIGHT");

			RegisterCommand("+indicatorBoth", new Action<int, List<object>, string>((source, args, raw) =>
			{
				int localPlayer = GetPlayerPed(-1);
				int vehicle = GetVehiclePedIsIn(localPlayer, false);
				if (vehicle == 0)
					return;
				int vehicleClass = GetVehicleClass(vehicle);
				if (vehicleCarClasses.Contains(vehicleClass))
				{
					int lightState = GetVehicleIndicatorLights(vehicle);
					if (lightState == (int)IndicatorState.OFF)
					{
						Comms.Event(VehicleUIEvents.SyncIndicatorLights).ToServer().Emit((int)IndicatorState.BOTH);
					}
					else
					{
						Comms.Event(VehicleUIEvents.SyncIndicatorLights).ToServer().Emit((int)IndicatorState.OFF);
					}
				}
			}), false);

			RegisterCommand("-indicatorBoth", new Action<int, List<object>, string>((source, args, raw) =>
			{

			}), false);
			RegisterKeyMapping("+indicatorBoth", "Both Indicator Signals (in vehicle)", "keyboard", "DOWN");
		}

		private async Task OnTick()
		{
			int localPlayer = GetPlayerPed(-1);
			int vehicle = GetVehiclePedIsIn(localPlayer, false);
			if (vehicle == 0)
			{
				return;
			}
			bool vehicleIsOn = GetIsVehicleEngineRunning(vehicle);
			VehicleNUIData info;

			if (IsPedInAnyVehicle(localPlayer, false) && vehicleIsOn)
			{
				int vehicleClass = GetVehicleClass(vehicle);
				bool meticUnits = ShouldUseMetricMeasurements();
				float vehicleSpeedRaw = GetEntitySpeed(vehicle);
				double vehicleSpeed;
				if (meticUnits)
				{
					vehicleSpeed = Math.Ceiling(Convert.ToDouble(vehicleSpeedRaw) * 3.6);
				}
				else
				{
					vehicleSpeed = Math.Ceiling(Convert.ToDouble(vehicleSpeedRaw) * 2.237);
				}

				double vehicleNailSpeed;
				if (vehicleSpeed > config.MaxSpeed)
				{
					vehicleNailSpeed = Math.Ceiling(280 - Math.Ceiling(Math.Ceiling(Convert.ToDouble(config.MaxSpeed) * 205) / config.MaxSpeed));
				}
				else
				{
					vehicleNailSpeed = Math.Ceiling(280 - Math.Ceiling(vehicleSpeed * 205) / config.MaxSpeed);
				}

				// Vehicle Fuel
				float vehicleFuel = GetVehicleFuelLevel(vehicle);

				// Vehicle Gear
				int vehicleGear = GetVehicleCurrentGear(vehicle);

				if ((vehicleSpeed == 0 && vehicleGear == 0) || (vehicleSpeed == 0 && vehicleGear == 1))
				{
					vehicleGear = 0;
				}
				else if (vehicleSpeed > 0 && vehicleGear == 0)
				{
					vehicleGear = -1;
				}

				// Vehicle Lights
				bool vehicleLightOn = true;
				bool vehicleHighBeamsOn = true;
				bool vehicleLightState = GetVehicleLightsState(vehicle, ref vehicleLightOn, ref vehicleHighBeamsOn);
				string vehicleLightString;
				if (vehicleLightOn && !vehicleHighBeamsOn)
				{
					vehicleLightString = "normal";
				}
				else if ((vehicleLightOn && vehicleHighBeamsOn) || (!vehicleLightOn && vehicleHighBeamsOn))
				{
					vehicleLightString = "high";
				}
				else
				{
					vehicleLightString = "off";
				}

				// Vehicle Siren
				bool vehicleSiren = IsVehicleSirenOn(vehicle);

				// Vehicle Seatbelt
				if (vehicleCarClasses.Contains(vehicleClass) && vehicleClass != 8)
				{
					double prevSpeed = currentSpeed;
					currentSpeed = vehicleSpeedRaw;

					SetPedConfigFlag(PlayerPedId(), 32, true);

					if (!seatbeltIsOn)
					{
						bool vehicleIsMovingFwd = GetEntitySpeedVector(vehicle, true).Y > 1.0;
						double vehAccel = (prevSpeed - currentSpeed) / GetFrameTime();
						if (vehicleIsMovingFwd && (prevSpeed > (config.EjectSpeed / 2.237)) && (vehAccel > (config.EjectAccel * 9.81)))
						{
							Vector3 position = GetEntityCoords(localPlayer, false);
							SetEntityCoords(localPlayer, position.X, position.Y, position.Z - 0.47f, true, true, true, false);
							SetEntityVelocity(localPlayer, prevVelocity.X, prevVelocity.Y, prevVelocity.Z);
							SetPedToRagdoll(localPlayer, 1000, 1000, 0, false, false, false);
						}
						else
						{
							prevVelocity = GetEntityVelocity(vehicle);
						}
					}
					else
					{
						DisableControlAction(0, 75, true); // Disable getting out of vehicle if seatbelt is on
					}
				}

				// Vehicle Indicator Lights
				int vehicleIndicator = GetVehicleIndicatorLights(vehicle);

				//Display Radar since we are in a vehicle
				DisplayRadar(true);

				info = new VehicleNUIData();
				info.status = true;
				info.speed = vehicleSpeed;
				info.nail = vehicleNailSpeed;
				info.gear = vehicleGear;
				info.fuel = vehicleFuel;
				info.lights = vehicleLightString;
				info.signals = vehicleIndicator;
				info.cruiser = cruiseControl;
				info.type = vehicleClass;
				info.siren = vehicleSiren;
				info.seatbelt = seatbeltIsOn;
				info.config = new VehicleNUIData.VehicleNUIInfoConfig();
				info.config.metricUnit = meticUnits;
				info.config.maxSpeed = config.MaxSpeed;
			}
			else
			{
				// We aren't in a vehicle, hide the speedometer and radar
				cruiseControl = false;
				seatbeltIsOn = false;
				info = new VehicleNUIData();
				info.status = false;
				DisplayRadar(false);
			}

			overlay.UpdateVehicle(info);
			await Delay(250);
		}
	}
}
