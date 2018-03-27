using System;

namespace Scimesh.Base
{
	public abstract class Field
	{
		string name;
		int nComponents;
		int dim;

		public string Name { get { return name; } }

		public int NComponents { get { return nComponents; } }

		public int Dim { get { return Dim; } }

		public Field (string name, int nComponents, int dim)
		{
			this.name = name;
			this.nComponents = nComponents;
			this.dim = dim;
		}

		public abstract float?[] GetValue (float[] coordinates);
	}

	public class DiscreteField : Field
	{
		int nValues;
		float?[] values;
		float[] coordinates;

		public int NValues { get { return nValues; } }

		protected float?[] Values { get { return values; } }

		protected float[] Coordinates { get { return coordinates; } }

		public DiscreteField (string name, int nComponents, int dim, int nValues, float?[] values, float[] coordinates)
			: base (name, nComponents, dim)
		{
			if (nComponents * nValues != values.Length) {
				throw new ArgumentException ("nComponents * nValues != values.Length!");
			}
			this.nValues = nValues;
			this.values = values;
			this.coordinates = coordinates;
		}

		public void ResetValues ()
		{
			values = new float?[NComponents * nValues];
		}

		public virtual float?[] GetValue (int valueIndex)
		{
			float?[] value = new float?[NComponents];
			int startIndex = valueIndex * NComponents;
//			Array.Copy (values, startIndex, value, 0, value.Length);
			for (int i = 0; i < value.Length; i++) {
				value [i] = values [startIndex + i];
			}
			return value;
		}

		public virtual void SetValue (int valueIndex, float?[] value)
		{
			int startIndex = valueIndex * NComponents;
//			Array.Copy (value, 0, values, startIndex, value.Length);
			for (int i = 0; i < value.Length; i++) {
				values [startIndex + i] = value [i];
			}
		}

		public virtual float[] GetValueCoordinates (int valueIndex)
		{
			float[] cs = new float[Dim];
			int startIndex = valueIndex * Dim;
			Array.Copy (coordinates, startIndex, cs, 0, cs.Length);
			return cs;
		}

		public float?[] this [int valueIndex] {
			get { return GetValue (valueIndex); }
			set { SetValue (valueIndex, value); }
		}

		// TODO implement interpolation algorithm
		public override float?[] GetValue (float[] coordinates)
		{
			throw new NotImplementedException ();
		}
	}

	public class MeshPointField : DiscreteField
	{
		Mesh mesh;

		public Mesh Mesh { get { return mesh; } }

		public MeshPointField (string name, int nComponents, int nValues, float?[] values, Mesh mesh)
			: base (name, nComponents, mesh.MaxDim, nValues, values, new float[0])
		{
			if (nValues != mesh.points.Length) {
				throw new ArgumentException ("nValues != mesh.points.Length!");
			}
			this.mesh = mesh;
		}

		public override float[] GetValueCoordinates (int valueIndex)
		{
			return mesh.points [valueIndex].coordinates;
		}
	}

	public class MeshCellField : DiscreteField
	{
		Mesh mesh;

		public Mesh Mesh { get { return mesh; } }

		public MeshCellField (string name, int nComponents, int nValues, float?[] values, Mesh mesh)
			: base (name, nComponents, mesh.MaxDim, nValues, values, new float[0])
		{
			if (nValues != mesh.cells.Length) {
				throw new ArgumentException ("nValues != mesh.cells.Length!");
			}
			this.mesh = mesh;
		}

		public override float[] GetValueCoordinates (int valueIndex)
		{
			return mesh.CellCentroid (valueIndex);
		}
	}
}
