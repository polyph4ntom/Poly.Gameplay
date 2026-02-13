using Poly.Settings;
using UnityEngine;

namespace Poly.Path
{
	[System.Serializable]
	public class FPolyPathSubsystemSettings
	{
		[field: SerializeField]
		public float SpawnCooldown { get; private set; }
		
		[field: SerializeField]
		public float MovementSpeed { get; private set; }
		
		[field: SerializeField]
		public float MinDistanceBetween { get; private set; }
	}

	[CreateAssetMenu(menuName = "Polyphantom/Settings/Path Subsystem Settings")]
	public class OPolyPathSubsystemSettingsAsset : OPolyDevSettingsAsset<FPolyPathSubsystemSettings> { }
}