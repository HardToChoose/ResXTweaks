namespace ResXTweaks
{
    internal abstract class TableEntry
    {
        public abstract bool GetFieldValue(string fieldName, out object value);
    }
}
