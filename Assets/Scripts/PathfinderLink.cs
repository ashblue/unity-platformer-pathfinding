using UnityEngine;
using System.Collections;

public class PathfinderLink {
	public PathfinderTile target;
	public string type; // What kind of link is this? ground, fall, runoff, jump
	public int weight; // Weight the tile to be less desirable for A*
	public int distance; // How far out is this tile, mainly used for checking max jump distance
}
