using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AStarLinkTest : MonoBehaviour {
	Pathfinder.AStarLinks pathfinder;
	PathfinderMap map;
	List<Pathfinder.Step> path;

	float updateDelay = 1;
	float delay = 1;

	Rect mousePos = new Rect(10, Screen.width - 50, 20, 30);
	int mouseX;
	int mouseY;

	void Start () {
		// Initialize AStar Pathfinder
		pathfinder = new Pathfinder.AStarLinks();
		pathfinder.SetMap(GameObject.Find("PathFinder").GetComponent<PathfinderMap>());
		map = GameObject.Find("PathFinder").GetComponent<PathfinderMap>();
	}

	void Update () {
		// Get the mouse position every 1 second
		delay -= Time.deltaTime;
		if (delay > 0) return;
		delay = updateDelay;

		// Use mouse position to discover a ledge
		Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		RaycastHit2D ledgeHit = LedgeCheck(mousePos);
		if (!ledgeHit.collider) return;

		// Get the current target tile
		PathfinderTile targetTile = map.GetTileByPos(ledgeHit.point);
		if (!targetTile.ledge) return;

		// Get the player's start location
		ledgeHit = LedgeCheck(transform.position);
		if (!ledgeHit.collider) return;

		// @TODO Check to make sure player is grounded
		// Verify the player is on a valid ledge
		PathfinderTile originTile = map.GetTileByPos(ledgeHit.point);
		if (originTile.ledge == null) return;

		// If there is existing path data clear it
		if (path != null) {
			for (int i = 0, l = path.Count; i < l; i++) {
				path[i].tile.SetColor(Color.yellow);
			}
		}

		// Execute find path
		// Visually display the path result
		path = pathfinder.FindPath(originTile, targetTile);
		if (path == null) return;
		for (int i = 0, l = path.Count; i < l; i++) {
			path[i].tile.SetColor(Color.cyan);
			Debug.Log(path[i].linkType);
		}
	}

//	void OnGUI () {
//		GUI.Label(mousePos, mouseX + " " + mouseY);
//	}

	RaycastHit2D LedgeCheck (Vector3 pos) {
		RaycastHit2D ledgeHit = Physics2D.Raycast(
			pos,
			-Vector2.up,
			10,
			map.whatIsCollision);
		
		return ledgeHit;
	}
}
