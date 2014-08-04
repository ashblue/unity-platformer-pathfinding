using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AStarTest : MonoBehaviour {
	PathfinderTile startTile;
	public int startX;
	public int startY;

	PathfinderTile endTile;
	public int endX;
	public int endY;
	
	Pathfinder.AStar pathfinder;
	PathfinderMap map;

	public Transform debugTile;

	void Start () {
		pathfinder = new Pathfinder.AStar();
		map = GetComponent<PathfinderMap>();

		GetTiles();

		pathfinder.SetMap(map);
	}

	void GetTiles () {
		// Convert start and end tile positions to a real tiles
		startTile = map.GetTile(startX, startY);
		endTile = map.GetTile(endX, endY);
	}

	void OnGUI () {
		// Visually mark start tile
		Vector3 startPos = Camera.main.WorldToScreenPoint(startTile.transform.position);
		GUI.Label(new Rect(startPos.x, Screen.height - startPos.y - 10, 30, 20), "X"); 

		// Visually mark end tile
		Vector3 endPos = Camera.main.WorldToScreenPoint(endTile.transform.position);
		GUI.Label(new Rect(endPos.x, Screen.height - endPos.y - 10, 30, 20), "Y"); 

		// Button should find the path
		if (GUI.Button(new Rect(10, 10, 70, 30), "Find Path")) {
			GetTiles();

			GameObject[] debugTiles = GameObject.FindGameObjectsWithTag("DebugTile");
			for (int i = 0, l = debugTiles.Length; i < l; i++) {
				Destroy(debugTiles[i]);
			}

			List<Pathfinder.Step> path = pathfinder.FindPath(startTile, endTile);

			GameObject newTile;
			for (int i = 0, l = path.Count; i < l; i++) {
				newTile = Instantiate(debugTile, path[i].tile.transform.position, Quaternion.identity) as GameObject;
			}
		}
	}
}
