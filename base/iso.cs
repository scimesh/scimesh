using System.Collections.Generic;

namespace Scimesh.Base
{
	public class Isocells
	{
		public float?[] scalarField;
		public float isovalue;
		public Mesh mesh;
		public int[] visibleCells;

		public Isocells (float?[] scalarField, float isovalue, Mesh mesh)
		{
			this.scalarField = scalarField;
			this.isovalue = isovalue;
			this.mesh = mesh;
			this.visibleCells = new int[0];
		}

		// TODO Performance for big meshes?
		// TODO Comparasion with null value
		public void UpdateCellsVisibility ()
		{
			// Comparaison array
			bool?[] comparaison = new bool?[scalarField.Length];
			for (int i = 0; i < scalarField.Length; i++) {
				if (scalarField [i] != null) {
					comparaison [i] = isovalue > scalarField [i];
				}
			}
			List<int> newVisibleCells = new List<int> ();
			for (int i = 0; i < mesh.cells.Length; i++) {
				bool isVisible = false;
				int[] cellPointsIndices = mesh.cells [i].pointsIndices;
				for (int j = 1; j < cellPointsIndices.Length; j++) {
					if (comparaison [cellPointsIndices [j]] != null) {
						if (comparaison [cellPointsIndices [0]] != comparaison [cellPointsIndices [j]]) {
							isVisible = true;
						}
					} else {
						isVisible = false;
						break;
					}
				}
				if (isVisible) {
					newVisibleCells.Add (i);
				}
			}
			visibleCells = newVisibleCells.ToArray ();
		}
	}
}

