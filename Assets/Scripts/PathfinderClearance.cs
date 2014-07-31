using UnityEngine;
using System.Collections;

// Loops through all existing tiles and updates the clearance values
public class PathfinderClearance {
	PathfinderMap pathData;
	PathfinderCollision gridCollision;

	public PathfinderClearance (PathfinderMap data, PathfinderCollision collision) {
		pathData = data;
		gridCollision = collision;
		MapUpdateClearance();
	}

	// Loops through all tiles and sets a clearance value
	// @NOTE Collision data must be updated on tiles in order for this to work
	public void MapUpdateClearance () {
		for (int y = 0, lY = pathData.GetHeightInTiles(), lX = pathData.GetWidthInTiles(); y < lY; y++) {
			for (int x = 0; x < lX; x++) {
				pathData.GetTile(x, y).clearance = RecursiveClearance(x, y, 1);
			}
		}
	}

	// Clearance value checking to continually expand the searched clearance area
	int RecursiveClearance (int xStart, int yStart, int distance) {
		if (gridCollision.IsEdgeOpen(xStart, yStart, distance))
			return RecursiveClearance(xStart, yStart, distance + 1);

//		// New distance failed, return the previous value
		return distance - 1;
	}
}
