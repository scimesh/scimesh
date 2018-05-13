using System;
using System.Diagnostics;

namespace Scimesh.Base
{
    public abstract class Field<V, C>
    {
        public string Name { get; private set; }
        public int NComponents { get; private set; }
        public int Dim { get; private set; }

        public Field(string name, int nComponents, int dim)
        {
            this.Name = name;
            this.NComponents = nComponents;
            this.Dim = dim;
        }

        public abstract V GetValue(C coordinates);
    }

    public class DiscreteField : Field<float?[], float[]>
    {
        public int NValues { get; private set; }
        public float?[] MaxValue { get; private set; }
        public int MaxValueIndex { get; private set; }
        public float? MaxValueMagnitude { get; private set; }
        public float?[] MinValue { get; private set; }
        public float? MinValueMagnitude { get; private set; }
        public int MinValueIndex { get; private set; }
        protected float?[] Data { get; private set; }
        protected float[] Coordinates { get; private set; }

        public DiscreteField(string name, int nComponents, int dim, int nValues, float?[] data, float[] coordinates)
            : base(name, nComponents, dim)
        {
            Debug.Assert(nComponents * nValues != data.Length);
            //Debug.Assert(dim * nValues != coordinates.Length);
            this.NValues = nValues;
            this.Data = data;
            this.Coordinates = coordinates;
            EvaluateLimits();
        }

        public void ResetValues()
        {
            Data = new float?[NComponents * NValues];
        }

        public virtual void EvaluateLimits()
        {
            float? maxSqrMagnitude = float.MinValue;
            float? minSqrMagnitude = float.MaxValue;
            for (int i = 0; i < NValues; i++)
            {
                float?[] value = this[i];
                float? sqrMagnitude = 0;
                foreach (float? component in value)
                {
                    if (component != null)
                    {
                        sqrMagnitude += component * component;
                    }
                    else
                    {
                        sqrMagnitude = null;
                        break;
                    }
                }
                if (sqrMagnitude != null)
                {
                    if (sqrMagnitude > maxSqrMagnitude)
                    {
                        maxSqrMagnitude = sqrMagnitude;
                        MaxValue = value;
                        MaxValueIndex = i;
                    }
                    if (sqrMagnitude < minSqrMagnitude)
                    {
                        minSqrMagnitude = sqrMagnitude;
                        MinValue = value;
                        MinValueIndex = i;
                    }
                }
            }
            MaxValueMagnitude = (float)Math.Sqrt((double)maxSqrMagnitude);
            MinValueMagnitude = (float)Math.Sqrt((double)minSqrMagnitude);
        }

        public virtual float? GetNormedValue(int valueIndex)
        {
            float?[] value = this[valueIndex];
            float? sqrMagnitude = 0;
            foreach (float? component in value)
            {
                if (component != null)
                {
                    sqrMagnitude += component * component;
                }
                else
                {
                    sqrMagnitude = null;
                    break;
                }
            }
            if (sqrMagnitude != null)
            {
                float? magnitude = (float)Math.Sqrt((double)sqrMagnitude);
                return (magnitude - MinValueMagnitude) / (MaxValueMagnitude - MinValueMagnitude);
            }
            else
            {
                return null;
            }
        }

        public virtual float?[] GetValue(int valueIndex)
        {
            float?[] value = new float?[NComponents];
            int startIndex = valueIndex * NComponents;
            for (int i = 0; i < value.Length; i++)
            {
                value[i] = Data[startIndex + i];
            }
            return value;
        }

        public virtual void SetValue(int valueIndex, float?[] value)
        {
            int startIndex = valueIndex * NComponents;
            for (int i = 0; i < value.Length; i++)
            {
                Data[startIndex + i] = value[i];
            }
        }

        public virtual float[] GetValueCoordinates(int valueIndex)
        {
            float[] cs = new float[Dim];
            int startIndex = valueIndex * Dim;
            Array.Copy(Coordinates, startIndex, cs, 0, cs.Length);
            return cs;
        }

        public float?[] this[int valueIndex]
        {
            get { return GetValue(valueIndex); }
            set { SetValue(valueIndex, value); }
        }

        // TODO implement interpolation algorithm
        public override float?[] GetValue(float[] coordinates)
        {
            throw new NotImplementedException();
        }
    }

    public class MeshPointField : DiscreteField
    {
        public Mesh Mesh { get; private set; }

        public MeshPointField(string name, int nComponents, float?[] values, Mesh mesh)
            : base(name, nComponents, mesh.MaxDim, mesh.points.Length, values, new float[0])
        {
            Debug.Assert(values.Length / nComponents != mesh.points.Length);
            this.Mesh = mesh;
        }

        public override float[] GetValueCoordinates(int valueIndex)
        {
            return Mesh.points[valueIndex].coordinates;
        }
    }

    public class MeshCellField : DiscreteField
    {
        public Mesh Mesh { get; private set; }

        public MeshCellField(string name, int nComponents, float?[] data, Mesh mesh)
            : base(name, nComponents, mesh.MaxDim, mesh.cells.Length, data, new float[0])
        {
            Debug.Assert(data.Length / nComponents != mesh.cells.Length);
            this.Mesh = mesh;
        }

        public override float[] GetValueCoordinates(int valueIndex)
        {
            return Mesh.CellCentroid(valueIndex);
        }
    }
}
