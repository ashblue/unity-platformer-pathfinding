using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// @TODO Rename PathfinderMap
// @TODO Feels like this should be in a namespace
public class PathfinderGrid : MonoBehaviour {
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
	
	void Awake () {
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
				grid[y, x].Init(x, y, tileSize, boxPos);
			}
		}
	}

	void Start () {
		// Populate collision data
		for (int y = 0, l = GetHeightInTiles(); y < l; y++) {
			for (int x = 0, lX = GetWidthInTiles(); x < lX; x++) {
				GetTile(x, y).UpdateCollision(whatIsCollision);
			}
		}

		// Run clearanc and collision setup here since collision data is ready
		pathCollision.Init(this);
		pathClearance.Init(this, pathCollision);

		// Find all valid pathways for gravity based pathfinding
		List<PathfinderTile> ledges = new List<PathfinderTile>();
		List<PathfinderTile> corners = new List<PathfinderTile>();
		for (int y = 0, lY = GetHeightInTiles(); y < lY; y++) {
			for (int x = 0, lX = GetWidthInTiles(); x < lX; x++) {
				PathfinderTile tile = GetTile(x, y);
				if (tile.cost == 1 && Blocked(x, y + 1) && y + 1 != lY) {	// If the bottom ledge is blocked or bottom of y axis
					tile.SetLedge(true);
					ledges.Add(tile);

					if (!Blocked(x - 1, y + 1) || !Blocked(x + 1, y + 1)) 
						corners.Add(tile);
				}
			}
		}

		// For each ledge, add a link to all ledge neighbors
		for (int i = 0, l = ledges.Count; i < l; i++) {
			int x = (int)ledges[i].xy.x;
			int y = (int)ledges[i].xy.y;

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
		for (int i = 0, l = corners.Count; i < l; i++) {
			// Discover the direction the tile is facing
			int direction = Blocked((int)corners[i].xy.x - 1, (int)corners[i].xy.y + 1) ? 1 : -1;

			// Step over the facing direction 1 tile
			PathfinderTile overhang = GetTile((int)corners[i].xy.x + direction, (int)corners[i].xy.y);

			// Shoot a raycast straight down to the end of the boundary
			RaycastHit2D hit = Physics2D.Raycast(
				overhang.transform.position, 
				-Vector2.up, 
				(GetHeightInTiles() - overhang.xy.y) * tileSize,
				whatIsCollision);

			// If we hit something add a link at the collision location
			if (hit.collider) {
				// Translate hit position into x y coordinates
				// Get the tile at the hit position
				// Record a one way link and attach it
			}

			// Find corner runoff points
			// Also raycast
			// If valid create a 2 way link
		}

		// From all ledge corners calculate jump parabola and link if successful
		// Check for clearance by fudging an extra jump arc above to simulate clearance
		// Needs to be a real physics jump arc
		// Raycast along the jump arc for collision tests
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
		if (GetTile(x, y).cost == 0) return true;
		
		return false;
	}

	public PathfinderTile GetTile (int x, int y) {
		return grid[y, x];
	}
}
