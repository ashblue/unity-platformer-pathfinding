using UnityEngine;
using System.Collections;

public class PathfinderLink {
	public PathfinderTile target;
	public int weight; // Weight the tile to be less desirable for A*
	public int distance; // How far out is this tile, mainly used for checking max jump distance
}
