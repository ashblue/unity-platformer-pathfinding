using UnityEngine;
using System.Collections;

public class PathfinderCollision {
	PathfinderMap grid;

	public void UpdateCollision (PathfinderMap map, LayerMask whatIsCollision) {
		// Populate collision data
		for (int y = 0, l = map.GetHeightInTiles(); y < l; y++) {
			for (int x = 0, lX = map.GetWidthInTiles(); x < lX; x++) {
				PathfinderTile tile = map.GetTile(x, y);

				float offset = map.tileSize / 2;
				Vector3 pos = tile.transform.position;
				Vector2 topLeft = new Vector2(pos.x - offset, pos.y - offset);
				Vector2 bottomRight = new Vector2(pos.x + offset, pos.y + offset);

				bool overlap = Physics2D.OverlapArea(topLeft, bottomRight, whatIsCollision);
				if (overlap) tile.SetCost("closed");
			}
		}
	}
}
