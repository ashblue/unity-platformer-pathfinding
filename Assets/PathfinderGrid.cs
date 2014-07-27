using UnityEngine;
using System.Collections;

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
				GetTile(x, y).UpdateCollision();
			}
		}
		
		pathCollision.Init(this);
		pathClearance.Init(this, pathCollision);

		// Find all valid pathways for gravity based pathfinding
		for (int y = 0, lY = GetHeightInTiles(); y < lY; y++) {
			for (int x = 0, lX = GetWidthInTiles(); x < lX; x++) {
				PathfinderTile tile = GetTile(x, y);
				if (tile.cost == 1 && Blocked(x, y + 1) && y + 1 != lY) 	// If the bottom ledge is blocked or bottom of y axis
					tile.SetLedge(true);
					// @TODO Validate if a corner and not just a normal ledge
			}
		}

		// For each ledge add a link to all ledge neighbors

		// Find ledge corner fall points

		// Find ledge corner runoff points

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
