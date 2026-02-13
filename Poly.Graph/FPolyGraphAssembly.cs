using System.Collections.Generic;
using Arc.Main;
using Poly.Common;
using Poly.Settings;
using UnityEngine;

namespace Poly.Graph
{
	public static class FPolyGraphAssembly
	{
		public static readonly Dictionary<System.Type, Color> portColorsMapping = new()
		{
			{ typeof(bool), FPolyColor.Maroon },
			{ typeof(byte), FPolyColor.SherpaBlue },
			{ typeof(int), FPolyColor.SeaGreen },
			{ typeof(float), FPolyColor.YellowGreen },
			{ typeof(string), FPolyColor.Magenta },
			{ typeof(Vector2), FPolyColor.Gold },
			{ typeof(Vector3), FPolyColor.Gold },
			{ typeof(Quaternion), FPolyColor.CornflowerBlue },
			{ typeof(Transform), FPolyColor.Orange},
			{ typeof(GameObject), FPolyColor.Yellow }
		};
		
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
		public static void LoadAssembly()
		{
			FPolyDevSettingsDatabase.Register<PolyGraphSettings>("SETTINGS_Graph");
		}
	}
}