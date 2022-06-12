using Content.Server.Light.EntitySystems;
using Content.Shared.Light.Component;

namespace Content.Server.Light.Components
{
    /// <summary>
    ///     Component that represents an emergency light, it has an internal battery that charges when the power is on.
    /// </summary>
    [RegisterComponent, Access(typeof(EmergencyLightSystem))]
    public sealed class EmergencyLightComponent : SharedEmergencyLightComponent
    {
        [ViewVariables]
        public EmergencyLightState State;

        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("wattage")]
        public float Wattage = 5;

        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("chargingWattage")]
        public float ChargingWattage = 60;

        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("chargingEfficiency")]
        public float ChargingEfficiency = 0.85f;

        public Dictionary<EmergencyLightState, string> BatteryStateText = new()
        {
            { EmergencyLightState.Full, "emergency-light-component-light-state-full" },
            { EmergencyLightState.Empty, "emergency-light-component-light-state-empty" },
            { EmergencyLightState.Charging, "emergency-light-component-light-state-charging" },
            { EmergencyLightState.On, "emergency-light-component-light-state-on" }
        };
    }

    public enum EmergencyLightState : byte
    {
        Charging,
        Full,
        Empty,
        On
    }

    public sealed class EmergencyLightEvent : EntityEventArgs
    {
        public EmergencyLightComponent Component { get; }

        public EmergencyLightState State { get; }

        public EmergencyLightEvent(EmergencyLightComponent component, EmergencyLightState state)
        {
            Component = component;
            State = state;
        }
    }
}