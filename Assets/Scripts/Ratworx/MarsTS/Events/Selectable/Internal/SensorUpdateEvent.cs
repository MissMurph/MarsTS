using System;

namespace Ratworx.MarsTS.Events.Selectable.Internal {

	public class SensorUpdateEvent<T> : AbstractEvent {

		public Type SensorType { get; private set; }
		//Returns true of added to inRange list, returns false if removed
		public bool Detected { get; private set; }
		public T Target { get; private set; }

		public SensorUpdateEvent(T _unit, bool _detected) : base("sensorUpdate") {
			SensorType = typeof(T);
			Target = _unit;
			Detected = _detected;
		}
	}
}