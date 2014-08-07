using UnityEngine;
using System.Collections;

namespace Pathfinder {
	public class Step {
		public int h; // Heuristic
		public string direction = "none";
		public PathfinderTile tile;
		public int totalSteps;
		public int totalCost;
		public Pathfinder.Step parent;
		public string linkType;

		public Step (PathfinderTile begin, PathfinderTile end, int steps, Step parentStep = null) {
			tile = begin;
			direction = getDirection(begin, end);

			h = distanceM(begin, end);
			totalSteps = steps;
			totalCost = steps + h;

			parent = parentStep;
		}

		// Euclidean distance
		static float distanceE (PathfinderTile current, PathfinderTile target) {
			int dx = target.x - current.x, dy = target.y - current.y;
			return Mathf.Sqrt((dx * dx) + (dy * dy));
		} 

		// Manhattan distance
		static int distanceM (PathfinderTile current, PathfinderTile target) {
			int dx = Mathf.Abs(target.x - current.x), dy = Mathf.Abs(target.y - current.y);
			return dx + dy;
		}

		// Discover the specific direction of a step
		static string getDirection (PathfinderTile current, PathfinderTile target) {
			string dir = "none";

			if (current.x > target.x) {
				dir = "west";
			} else if (current.x < target.x) {
				dir = "east";
			} else if (current.y > target.y) {
				dir = "north";
			} else if (current.y < target.y) {
				dir = "south";
			}

			return dir;
		}
	}
}
