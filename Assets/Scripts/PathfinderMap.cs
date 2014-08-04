using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// @TODO Feels like this should be in a namespace
public class PathfinderMap : MonoBehaviour {
	// Evaluated data from the bounding box
	GameObject boundingBox;
	Vector3 boxPos;
	float boxWidth;
	float boxHeight;

	PathfinderClearance pathClearance = new PathfinderClearance();
	PathfinderCollision pathCollision = new PathfinderCollision();

	public float tileSize = 1;
	public PathfinderTile[,] grid;
	public GameObject tilePrefab;
	public LayerMask whatIsCollision;
	public LayerMask whatIsJumpPoint;
	public float jumpDistance = 4; // Max jump distance from a ledge

	public bool debug = false; // Allows for live debug mode, not recommended in production mode (expendsive)
	public bool debugLines = true; // Show debug lines
	public float debugRefresh = 1;
	float debugRefreshCount = 0;

	void Awake () {
		Init();
	}

	// @TODO Compartmentalise these components into separate files
	void Init () {
		// Get the required size data
		boundingBox = GameObject.Find("BoundingBox");
		boxWidth = boundingBox.collider2D.bounds.size.x;
		boxHeight = boundingBox.collider2D.bounds.size.y;
		
		float boxX = boundingBox.collider2D.bounds.center.x - (boundingBox.collider2D.bounds.size.x / 2);
		float boxY = boundingBox.collider2D.bounds.center.y + (boundingBox.collider2D.bounds.size.y / 2);
		boxPos = new Vector3(boxX, boxY, 0);
		
		// Generate a grid of tiles
		grid = new PathfinderTile[Mathf.RoundToInt(boxHeight / tileSize), Mathf.RoundToInt(boxWidth / tileSize)];
		for (int y = 0, l = GetHeightInTiles(); y < l; y++) {
			for (int x = 0, lX = GetWidthInTiles(); x < lX; x++) {
				GameObject newTile = (GameObject)Instantiate(tilePrefab);
				grid[y, x] = newTile.GetComponent<PathfinderTile>();
				newTile.transform.parent = transform;
				grid[y, x].Init(x, y, tileSize, boxPos, debugLines);
			}
		}
		
		// Run clearance and collision setup here since collision data is ready
		pathCollision.UpdateCollision(this, whatIsCollision);
		pathClearance.UpdateClearance(this);
		
		// Find all valid pathways for gravity based pathfinding
		// @TODO Traversing and creating the links could be much more modular
		List<PathfinderTile> ledges = new List<PathfinderTile>();
		List<PathfinderTile> corners = new List<PathfinderTile>();
		for (int y = 0, lY = GetHeightInTiles(); y < lY; y++) {
			for (int x = 0, lX = GetWidthInTiles(); x < lX; x++) {
				PathfinderTile tile = GetTile(x, y);
				if (tile.cost.weight == 1 && Blocked(x, y + 1) && y + 1 != lY) {	// If the bottom ledge is blocked or bottom of y axis
					tile.SetLedge(true);
					ledges.Add(tile);
					
					if (!Blocked(x - 1, y + 1) || !Blocked(x + 1, y + 1)) 
						corners.Add(tile);
				}
			}
		}
		
		// For each ledge, add a link to all ledge neighbors
		for (int i = 0, l = ledges.Count; i < l; i++) {
			int x = ledges[i].x;
			int y = ledges[i].y;
			
			// Left neighbor check
			if (!Blocked(x - 1, y)) {
				PathfinderTile tile = GetTile(x - 1, y);
				if (tile.ledge) ledges[i].AddLink(tile, 0, 1);
			}
			
			// Right neighbor check
			if (!Blocked(x + 1, y)) {
				PathfinderTile tile = GetTile(x + 1, y);
				if (tile.ledge) ledges[i].AddLink(tile, 0, 1);
			}
		}
		
		// Find ledge corner fall points
		// Raycast at specific angle to check for clear fall point
		// If valid create a one way link
		List<PlatformLedgePoint> ledgePoints = new List<PlatformLedgePoint>();
		for (int i = 0, l = corners.Count; i < l; i++) {
			// Discover the direction the tile is facing
			int direction = Blocked(corners[i].x - 1, corners[i].y + 1) ? 1 : -1;
			
			// Step over the facing direction 1 tile
			PathfinderTile overhang = GetTile(corners[i].x + direction, corners[i].y);
			
			// Shoot a raycast straight down to the end of the boundary to look for a fall
			RaycastHit2D hit = Physics2D.Raycast(
				overhang.transform.position, 
				-Vector2.up, 
				(GetHeightInTiles() - overhang.y) * tileSize,
				whatIsCollision);
			
			// If we hit something add a fall link at the hit target
			if (hit.collider) {
				PathfinderTile tileTarget = GetTileByPos(hit.point);
				int distance = (int)Mathf.Floor(Vector3.Distance(corners[i].transform.position, tileTarget.transform.position));
				corners[i].AddLink(tileTarget, 1, distance, "fall");
			}
			
			// Find corner runoff point
			hit = Physics2D.Raycast(
				overhang.transform.position,
				new Vector2(0.2f * direction, -0.5f),
				(GetHeightInTiles() - overhang.y) * tileSize,
				whatIsCollision
				);
			
			// If valid create a 2 way link
			if (hit.collider) {
				PathfinderTile tileTarget = GetTileByPos(hit.point);
				int distance = (int)Mathf.Floor(Vector3.Distance(corners[i].transform.position, tileTarget.transform.position));
				corners[i].AddLink(tileTarget, 1, distance, "runoff");
				tileTarget.AddLink(corners[i], 1, distance, "runoff"); // @TODO Should probably be runoff jump
			}

			// Creata a ledge point for evaluating corner jumps
			Transform targetTrans = GetTile((int)corners[i].x, (int)corners[i].y).transform;
			GameObject pointObj = (GameObject)Instantiate(Resources.Load("PlatformLedge"));
			PlatformLedgePoint point = pointObj.GetComponent<PlatformLedgePoint>();
			pointObj.transform.position = targetTrans.position;
			point.x = (int)corners[i].x;
			point.y = (int)corners[i].y;
			point.direction = direction;
			ledgePoints.Add(point);

		}

		// Loop through all ledge corners
		for (int i = 0, l = ledgePoints.Count; i < l; i++) {
			// @TODO Swap for a circle cast
			RaycastHit2D[] jumpPoints = Physics2D.BoxCastAll(
				ledgePoints[i].transform.position,
				new Vector2(1, jumpDistance),
				0,
				new Vector2(jumpDistance * ledgePoints[i].direction, 0), // Shift down slightly to activate the collision test
				jumpDistance,
				whatIsJumpPoint);

			for (int j = 0, jL = jumpPoints.Length; j < jL; j++) {
				int distance = (int)Mathf.Floor(Vector3.Distance(ledgePoints[i].transform.position, jumpPoints[j].transform.position));

				RaycastHit2D hit = Physics2D.Raycast(
					ledgePoints[i].transform.position,
//					(ledgePoints[i].transform.position - jumpPoints[j].transform.position).normalized,
					(jumpPoints[j].transform.position - ledgePoints[i].transform.position).normalized,
					Vector3.Distance(ledgePoints[i].transform.position, jumpPoints[j].transform.position),
					whatIsCollision);

				if (hit.collider) {
					continue;
				}

				PathfinderTile tile = GetTile(ledgePoints[i].x, ledgePoints[i].y);
				PlatformLedgePoint tileTarget = jumpPoints[j].collider.GetComponent<PlatformLedgePoint>();
				tile.AddLink(GetTile(tileTarget.x, tileTarget.y), 2, distance, "jump");
			}
		}

		// Cleanup leftover ledge corner colliders
		for (int i = 0, l = ledgePoints.Count; i < l; i++) {
			Destroy(ledgePoints[i].gameObject);
		}
	}

	// Reset pathfinder back to ground zero
	void Reset () {
		foreach (Transform child in transform) {
			Destroy(child.gameObject);
		}
	}

	void FixedUpdate () {
		// If debug mode is active, update the grid every 60 seconds
		if (!debug) return;
		
	    if (debugRefreshCount > debugRefresh * 60) {
			Reset();
			Init();
			debugRefreshCount = 0;
		}

		debugRefreshCount += 1;
	}

	// Turn real world coordinates into a tile postiion
	public PathfinderTile GetTileByPos (Vector3 pos) {
		float x = Mathf.Floor(Mathf.Abs(boxPos.x - pos.x) / tileSize);
		float y = Mathf.Floor(Mathf.Abs(boxPos.y - pos.y) / tileSize);

		return GetTile((int)x, (int)y);
	}

	public int GetWidthInTiles () {
		return grid.GetLength(1);
	}

	public int GetHeightInTiles () {
		return grid.GetLength(0);
	}

	public bool OutOfBounds (int x, int y) {
		return x < 0 || x >= GetWidthInTiles() ||
			y < 0 || y >= GetHeightInTiles();
	}

	public bool Blocked (int x, int y) {
		if (OutOfBounds(x, y)) return true;
		if (GetTile(x, y).cost.weight == 0) return true;
		
		return false;
	}

	public PathfinderTile GetTile (int x, int y) {
		return grid[y, x];
	}

	/**
	 * Returns the bottom right edge of a square from a specific point. Note: All squares are drawn from the top
	 * left to bottom right
	 * @param xStart Bottom right start point of the square's edge
	 * @param yStart Bottom left start point of the square's edge
	 * @param distance
	 */
	public bool IsEdgeOpen (int xStart, int yStart, int distance) {
		int count = 0;
		
		// @TODO A bit messy, a cleaner way of doing this?
		for (int y = yStart, lY = yStart + distance, thresholdY = lY - 1; y < lY; y++) {
			for (int x = xStart, lX = xStart + distance, thresholdX = lX - 1; x < lX; x++) {
				if ((x >= thresholdX || y >= thresholdY) && !Blocked(x, y)) count += 1;
			}
		}
		
		return count == (distance - 1) * 2 + 1;
	}

	public List<PathfinderTile> GetNeighbors (int x, int y) {
		List<PathfinderTile> neighbors = new List<PathfinderTile>();
		
		// Check left, right, top, bottom
		if (!Blocked(x + 1, y)) neighbors.Add(GetTile(x + 1, y));
		if (!Blocked(x - 1, y)) neighbors.Add(GetTile(x - 1, y));
		if (!Blocked(x, y + 1)) neighbors.Add(GetTile(x, y + 1));
		if (!Blocked(x, y - 1)) neighbors.Add(GetTile(x, y - 1));
		
		return neighbors;
	}
}
