using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AStarLinkTest : MonoBehaviour {
	Pathfinder.AStarLinks pathfinder;
	PathfinderMap map;
	List<Pathfinder.Step> path;
	PlatformerCharacter2D moveScript;
	JumpTween jumpScript;

	string moveType;
	Pathfinder.Step tileCurrent; // @TODO Rename step current
	Pathfinder.Step tileGoal; // @TODO Rename step goal

	int direction; // Movement direction for the player

	// Delay between finding a new path to the target
	float updateDelay = 1;
	float delay = 1;

	void Awake () {
		moveScript = GetComponent<PlatformerCharacter2D>();
		jumpScript = GetComponent<JumpTween>();
	}

	void Start () {
		// Initialize AStar Pathfinder
		pathfinder = new Pathfinder.AStarLinks();
		pathfinder.SetMap(GameObject.Find("PathFinder").GetComponent<PathfinderMap>());
		map = GameObject.Find("PathFinder").GetComponent<PathfinderMap>();
	}

	void Update () {
		if (tileCurrent != null) Move();

		if (Input.GetMouseButtonDown(0)) {
			DiscoverPath();
		}
	}

	void DiscoverPath () {
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
		}

		// Grab the first tile for traversal
		NextTile();
	}

	// Has the intended destination been reached?
	// @TODO Consider testing range instead of direction (are they on the tile)
	bool IsDestination () {
		float dif = transform.position.x - tileCurrent.tile.transform.position.x;

		// Have we passed it? Then get a new tile
		if ((direction == 1 && dif > 0) || (direction == -1 && dif < 0)) {
			return true;
		}

		return false;
	}

	void Move () {
		// Move towards the target tile
		if (moveType == "ground") {
			rigidbody2D.velocity = new Vector2(moveScript.maxSpeed * direction, rigidbody2D.velocity.y);
			
			if (IsDestination())
				NextTile();

		} else if (moveType == "fall") {
			// Move forward if we haven't passed the fall point, otherwise the player will undershoot the ledge
			if (!IsDestination()) {
				rigidbody2D.velocity = new Vector2(moveScript.maxSpeed * direction, rigidbody2D.velocity.y);

			// Player is grounded and very close to goal tile, must have landed at proper location
			} else if (moveScript.grounded && Vector3.Distance(transform.position, tileCurrent.tile.transform.position) < 1) {
				NextTile();

			// Must be falling, stop forward movement immediately
			} else {
				rigidbody2D.velocity = new Vector2(0.2f, rigidbody2D.velocity.y);
			}
		
		} else if (!rigidbody2D.isKinematic) { // Treat all other tile types as jump
			jumpScript.JumpTo(tileCurrent.tile.transform.position, "NextTile");
		}


//		if (moveType == "ground") {
			// Move in direction from current tile to goal

//		} else if (moveType == "fall") {
			// Move forward until above the fall, tile
			// Then descend until touching it
//		} else if (moveType == "runoff" || moveType == "jump") {
			// jump to target point
//		}

		// If we are at the goal, get the next move tile
	}

	// @TODO Add a method to lerp down the character's movement so it doesn't suddenly stop

	void NextTile () {
		// if tiles are available get the next tile
//		if (path == null || path.Count == 0 || tileCurrent != null) return;

		if (path.Count == 0) {
			tileCurrent = null;
			rigidbody2D.velocity = Vector2.zero;
			return;
		}

		// @TODO Move to an array list for pop methods
		// Get the next tile
		tileCurrent = path[path.Count - 1];
		path.RemoveAt(path.Count - 1);
		moveType = tileCurrent.linkType;

		// Only compare against the player if we are out of tiles (prevents accidental movement overlap)
		if (path.Count > 0) {
			direction = tileCurrent.tile.transform.position.x - path[path.Count - 1].tile.transform.position.x > 0 ? -1 : 1;
		} else {
			direction = transform.position.x - tileCurrent.tile.transform.position.x > 0 ? -1 : 1;
		}

		// Remove coloring from tile for debug purposes
		tileCurrent.tile.SetColor(Color.yellow);
	}

	RaycastHit2D LedgeCheck (Vector3 pos) {
		RaycastHit2D ledgeHit = Physics2D.Raycast(
			pos,
			-Vector2.up,
			10,
			map.whatIsCollision);
		
		return ledgeHit;
	}
}
