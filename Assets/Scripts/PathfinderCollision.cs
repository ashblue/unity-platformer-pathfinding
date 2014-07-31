using UnityEngine;
using System.Collections;

public class PathfinderCollision {
	PathfinderMap grid;

	public void Init (PathfinderMap gridTarget) {
		grid = gridTarget;
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
				if ((x >= thresholdX || y >= thresholdY) && !grid.Blocked(x, y)) count += 1;
			}
		}

		return count == (distance - 1) * 2 + 1;
	}
}
