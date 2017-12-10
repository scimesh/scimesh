using System;

namespace Scimesh.Base
{
	public abstract class Field
	{
		public string name;
		public int nComponents;
		public int dim;

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
		public int nValues;
		public float?[] values;
		public float[] coordinates;

		public DiscreteField (string name, int nComponents, int dim,
		                      int nValues,
		                      float?[] values,
		                      float[] coordinates
		) : base (name, nComponents, dim)
		{
			this.nValues = nValues;
			this.coordinates = coordinates;
		}

		public virtual float?[] GetValue (int valueIndex)
		{
			float?[] value = new float?[nComponents];
			if (valueIndex < nValues) {
				for (int i = 0; i < value.Length; i++) {
					value [i] = values [valueIndex * nComponents + i];
				}
			} else {
				for (int i = 0; i < value.Length; i++) {
					value [i] = null;
				}
			}
			return value;
		}

		public virtual float[] GetValueCoordinates (int valueIndex)
		{
			float[] coordinates = new float[dim];
			for (int i = 0; i < coordinates.Length; i++) {
				coordinates [i] = coordinates [valueIndex * dim + i];
			}
			return coordinates;
		}

		// TODO implement interpolation algorithm
		public override float?[] GetValue (float[] coordinates)
		{
			throw new NotImplementedException ();
		}
	}

	// TODO scalar min/max/norm to vector min/max algorithm (possibly vector length)
	public class DiscreteSpacetimeField : DiscreteField
	{
		public int nTimeValues;
		public int nSpaceValues;
		public float[] times;
		public float? minValue;
		public float? maxValue;

		public DiscreteSpacetimeField (string name, int nComponents, int dim,
		                               int nSpaceValues, 
		                               int nTimeValues,
		                               float?[] values,
		                               float[] coordinates,
		                               float[] times
		) : base (name, nComponents, dim, nSpaceValues * nTimeValues, values, coordinates)
		{
			this.nSpaceValues = nSpaceValues;
			this.nTimeValues = nTimeValues;
			this.times = times;
			EvaluateMaxValue ();
			EvaluateMinValue ();
		}

		public virtual float?[] GetValue (int spaceValueIndex, int timeValueIndex)
		{
			float?[] value = new float?[nComponents];
			int startIndex = timeValueIndex * nSpaceValues * nComponents +
			                 spaceValueIndex * nComponents;
			for (int i = 0; i < value.Length; i++) {
				value [i] = values [startIndex + i];
			}
			return value;
		}

		public virtual float?[] GetTimeValues (int timeValueIndex)
		{
			float?[] values = new float?[nSpaceValues * nComponents];
			int startIndex = timeValueIndex * nSpaceValues * nComponents;
			for (int i = 0; i < values.Length; i++) {
				values [i] = values [startIndex + i];
			}
			return values;
		}

		public void EvaluateMaxValue ()
		{
			maxValue = null;
			for (int i = 0; i < values.Length; i++) {
				if (!maxValue.HasValue) {
					maxValue = values [i];
				}
				if (maxValue < values [i]) {
					maxValue = values [i];
				}
			}
		}

		public void EvaluateMinValue ()
		{
			minValue = null;
			for (int i = 0; i < values.Length; i++) {
				if (!minValue.HasValue) {
					minValue = values [i];
				}
				if (minValue > values [i]) {
					minValue = values [i];
				}
			}
		}

		public float?[] GetNormedData ()
		{
			float? deltaValue = maxValue - minValue;
			float?[] normedData = new float?[values.Length];
			for (int i = 0; i < values.Length; i++) {
				normedData [i] = (values [i] - minValue) / deltaValue;
			}
			return normedData;
		}

		public override float[] GetValueCoordinates (int valueIndex)
		{
			if (valueIndex > coordinates.Length) {
				while (valueIndex > coordinates.Length) {
					valueIndex -= coordinates.Length;
				}
			}
			float[] cs = new float[dim];
			for (int i = 0; i < cs.Length; i++) {
				cs [i] = cs [valueIndex];
			}
			return cs;
		}
	}

	public class PointField : DiscreteSpacetimeField
	{
		public Mesh mesh;

		public PointField (string name, int nComponents, int dim,
		                                int nTimeValues,
		                                float?[] values,
		                                float[] times,
		                                Mesh mesh
		) : base (name, nComponents, dim,
			         mesh.points.Length, nTimeValues, values, new float[0], times)
		{
			this.mesh = mesh;
		}

		public override float[] GetValueCoordinates (int valueIndex)
		{
			if (valueIndex > mesh.points.Length) {
				while (valueIndex > mesh.points.Length) {
					valueIndex -= mesh.points.Length;
				}
			}
			return mesh.points [valueIndex].coordinates;
		}
	}

	public class CellField : DiscreteSpacetimeField
	{
		public Mesh mesh;

		public CellField (string name, int nComponents, int dim,
		                               int nTimeValues,
		                               float?[] values,
		                               float[] times,
		                               Mesh mesh
		) : base (name, nComponents, dim, 
			         mesh.cells.Length, nTimeValues, values, new float[0], times)
		{
			this.mesh = mesh;
		}

		public override float[] GetValueCoordinates (int valueIndex)
		{
			if (valueIndex > mesh.cells.Length) {
				while (valueIndex > mesh.cells.Length) {
					valueIndex -= mesh.cells.Length;
				}
			}
			return mesh.CellCentroid (valueIndex);
		}
	}
}
