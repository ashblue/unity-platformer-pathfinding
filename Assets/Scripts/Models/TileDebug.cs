using UnityEngine;
using System.Collections;

// Visual debugging tile
namespace Pathfinder {
	public class TileDebug : MonoBehaviour {
		public Color color;
		public Texture2D texture;
		Rect rect;
		GUIStyle style;
		
		void Awake () {
			Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
			rect = new Rect(screenPos.x - 10, Screen.height - screenPos.y - 10, 20, 20);
		}
		
		void OnGUI () {
			GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill, true);
		}
	}
}
