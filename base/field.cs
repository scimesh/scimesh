using System;
using System.Diagnostics;

namespace Scimesh.Base
{
    [Serializable]
    public abstract class Field<V, C>
    {
        public string name;
        public string Name { get { return name; } set { name = value; } }
        public int nComponents;
        public int NComponents { get { return nComponents; } set { nComponents = value; } }
        public int dim;
        public int Dim { get { return dim; } set { dim = value; } }

        public Field(string name, int nComponents, int dim)
        {
            Name = name;
            NComponents = nComponents;
            Dim = dim;
        }

        public abstract V GetValue(C coordinates);
    }

    [Serializable]
    public class DiscreteField : Field<float[], float[]>
    {
        public int nValues;
        public int NValues { get { return nValues; } set { nValues = value; } }
        public float[] maxValue;
        public float[] MaxValue { get { return maxValue; } set { maxValue = value; } }
        public int maxValueIndex;
        public int MaxValueIndex { get { return maxValueIndex; } set { maxValueIndex = value; } }
        public float maxValueMagnitude;
        public float MaxValueMagnitude { get { return maxValueMagnitude; } set { maxValueMagnitude = value; } }
        public float[] minValue;
        public float[] MinValue { get { return minValue; } set { minValue = value; } }
        public float minValueMagnitude;
        public float MinValueMagnitude { get { return minValueMagnitude; } set { minValueMagnitude = value; } }
        public int minValueIndex;
        public int MinValueIndex { get { return minValueIndex; } set { minValueIndex = value; } }
        public float[] data;
        public float[] Data { get { return data; } set { data = value; } }
        public float[] coordinates;
        public float[] Coordinates { get { return coordinates; } set { coordinates = value; } }

        public DiscreteField(string name, int nComponents, int dim, int nValues, float[] data, float[] coordinates)
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
            Data = new float[NComponents * NValues];
        }

        public virtual void EvaluateLimits()
        {
            float maxSqrMagnitude = float.MinValue;
            float minSqrMagnitude = float.MaxValue;
            for (int i = 0; i < NValues; i++)
            {
                float[] value = this[i];
                float sqrMagnitude = 0;
                foreach (float component in value)
                {
                    sqrMagnitude += component * component;
                }
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
            MaxValueMagnitude = (float)Math.Sqrt(maxSqrMagnitude);
            MinValueMagnitude = (float)Math.Sqrt(minSqrMagnitude);
        }

        public virtual float GetNormedValue(int valueIndex)
        {
            if (MaxValueMagnitude - MinValueMagnitude == 0)  // For uniform field
            {
                return 0;
            }
            float[] value = this[valueIndex];
            float sqrMagnitude = 0;
            foreach (float component in value)
            {
                sqrMagnitude += component * component;
            }
            float magnitude = (float)Math.Sqrt((double)sqrMagnitude);
            return (magnitude - MinValueMagnitude) / (MaxValueMagnitude - MinValueMagnitude);
        }

        public virtual float[] GetValue(int valueIndex)
        {
            float[] value = new float[NComponents];
            int startIndex = valueIndex * NComponents;
            for (int i = 0; i < value.Length; i++)
            {
                value[i] = Data[startIndex + i];
            }
            return value;
        }

        public virtual void SetValue(int valueIndex, float[] value)
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

        public float[] this[int valueIndex]
        {
            get { return GetValue(valueIndex); }
            set { SetValue(valueIndex, value); }
        }

        // TODO implement interpolation algorithm
        public override float[] GetValue(float[] coordinates)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            string str = "";
            str += string.Format("Type: {0} \n", this.GetType().Name);
            str += string.Format("Name: {0} \n", Name);
            str += string.Format("Dim: {0} \n", Dim);
            str += string.Format("NComponents: {0} \n", NComponents);
            str += string.Format("NValues: {0} \n", NValues);
            str += string.Format("Data Length: {0} \n", Data.Length);
            str += string.Format("Coordinates Length: {0} \n", Coordinates.Length);
            // Max Value
            str += string.Format("MaxValueIndex: {0}\t", MaxValueIndex);
            str += "MaxValue: [";
            for (int i = 0; i < NComponents; i++)
            {
                str += string.Format("{0} ", MaxValue[i]);
            }
            str += string.Format("]\tMaxValueMagnitude: {0}\n", MaxValueMagnitude);
            // Min Value
            str += string.Format("MinValueIndex: {0}\t", MinValueIndex);
            str += "MinValue: [";
            for (int i = 0; i < NComponents; i++)
            {
                str += string.Format("{0} ", MinValue[i]);
            }
            str += string.Format("]\tMinValueMagnitude: {0}\n", MinValueMagnitude);
            // Table
            str += "Index\t";
            for (int i = 0; i < NComponents; i++)
            {
                str += string.Format("Component {0}\t", i + 1);
            }
            str += "\n";
            for (int i = 0; i < NValues; i++)
            {
                string valueStr = "";
                valueStr += string.Format("{0}\t", i);
                float[] value = GetValue(i);
                foreach (float? c in value)
                {
                    valueStr += string.Format("{0}\t", c);
                }
                str += string.Format("{0}\n", valueStr);
            }
            return str;
        }
    }

    public class DiscreteFieldNullable : Field<float?[], float[]>
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

        public DiscreteFieldNullable(string name, int nComponents, int dim, int nValues, float?[] data, float[] coordinates)
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
            if (MaxValueMagnitude - MinValueMagnitude == 0)  // For uniform field
            {
                return 0;
            }
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

        public override string ToString()
        {
            string str = "";
            str += string.Format("Type: {0} \n", this.GetType().Name);
            str += string.Format("Name: {0} \n", Name);
            str += string.Format("Dim: {0} \n", Dim);
            str += string.Format("NComponents: {0} \n", NComponents);
            str += string.Format("NValues: {0} \n", NValues);
            str += string.Format("Data Length: {0} \n", Data.Length);
            str += string.Format("Coordinates Length: {0} \n", Coordinates.Length);
            // Max Value
            str += string.Format("MaxValueIndex: {0}\t", MaxValueIndex);
            str += "MaxValue: [";
            for (int i = 0; i < NComponents; i++)
            {
                str += string.Format("{0} ", MaxValue[i]);
            }
            str += string.Format("]\tMaxValueMagnitude: {0}\n", MaxValueMagnitude);
            // Min Value
            str += string.Format("MinValueIndex: {0}\t", MinValueIndex);
            str += "MinValue: [";
            for (int i = 0; i < NComponents; i++)
            {
                str += string.Format("{0} ", MinValue[i]);
            }
            str += string.Format("]\tMinValueMagnitude: {0}\n", MinValueMagnitude);
            // Table
            str += "Index\t";
            for (int i = 0; i < NComponents; i++)
            {
                str += string.Format("Component {0}\t", i + 1);
            }
            str += "\n";
            for (int i = 0; i < NValues; i++)
            {
                string valueStr = "";
                valueStr += string.Format("{0}\t", i);
                float?[] value = GetValue(i);
                foreach (float? c in value)
                {
                    valueStr += string.Format("{0}\t", c);
                }
                str += string.Format("{0}\n", valueStr);
            }
            return str;
        }
    }

    [Serializable]
    public class MeshPointField : DiscreteField
    {
        [NonSerialized]
        public Mesh mesh;
        public Mesh Mesh { get { return mesh; } set { mesh = value; } }

        public MeshPointField(string name, int nComponents, float[] data, Mesh mesh)
            : base(name, nComponents, mesh.MaxDim, mesh.points.Length, data, new float[0])
        {
            Debug.Assert(data.Length / nComponents != mesh.points.Length);
            Mesh = mesh;
        }

        public override float[] GetValueCoordinates(int valueIndex)
        {
            return Mesh.points[valueIndex].coordinates;
        }
    }

    [Serializable]
    public class MeshCellField : DiscreteField
    {
        public Mesh mesh;
        public Mesh Mesh { get { return mesh; } set { mesh = value; } }

        public MeshCellField(string name, int nComponents, float[] data, Mesh mesh)
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

    public class MeshPointFieldNullable : DiscreteFieldNullable
    {
        public Mesh Mesh { get; private set; }

        public MeshPointFieldNullable(string name, int nComponents, float?[] data, Mesh mesh)
            : base(name, nComponents, mesh.MaxDim, mesh.points.Length, data, new float[0])
        {
            Debug.Assert(data.Length / nComponents != mesh.points.Length);
            Mesh = mesh;
        }

        public override float[] GetValueCoordinates(int valueIndex)
        {
            return Mesh.points[valueIndex].coordinates;
        }
    }

    public class MeshCellFieldNullable : DiscreteFieldNullable
    {
        public Mesh Mesh { get; private set; }

        public MeshCellFieldNullable(string name, int nComponents, float?[] data, Mesh mesh)
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
