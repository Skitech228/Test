namespace ANG24.Sys.Infrastructure.Helpers
{
    public abstract class Filter<T> where T : IComparable
    {
        #region abstract
        protected T[] array;
        protected byte index = 0;
        public byte MaxArray { get; }
        public abstract T AVG { get; set; }
        public Filter(byte max = 3) => array = new T[MaxArray = max];
        public T GetAVG(T value)
        {
            AVG = value;
            return AVG;
        }
        #endregion
        public static Filter<T> NewFilter(byte max = 3)
        {
            if (typeof(T).Equals(typeof(int))) return new IntFilter(max) as Filter<T>;

            return new DoubleFilter(max) as Filter<T>;
        }
    }
    public class DoubleFilter : Filter<double>
    {
        public DoubleFilter(byte AVG) : base(AVG) { }
        public override double AVG
        {
            set
            {
                array[index++] = value;
                if (index >= array.Length) index = 0;
            }
            get => array.Aggregate((a, b) => a + b) / array.Length;
        }
    }
    public class IntFilter : Filter<int>
    {
        public IntFilter(byte AVG) : base(AVG) { }
        public override int AVG
        {
            set
            {
                array[index++] = value;
                if (index >= array.Length) index = 0;
            }
            get => array.Aggregate((a, b) => a + b) / array.Length;
        }
    }
}
