﻿using System.Text;

namespace Heir.Runtime
{
    public sealed class ObjectValue(IEnumerable<KeyValuePair<object, object?>> pairs) : Dictionary<object, object?>(pairs)
    {
        public string ToString(int indent = 0)
        {
            var result = new StringBuilder("{");
            if (Count > 0)
            {
                result.AppendLine();
                indent++;
            }
            
            foreach (var property in this)
            {
                result.Append(string.Join("", Enumerable.Repeat("  ", indent)));
                result.Append('[');
                result.Append(property.Key.ToString());
                result.Append("]: ");
                result.Append(property.Value is ObjectValue objectValue ? objectValue.ToString(indent + 1) : property.Value?.ToString() ?? "none");
                if (this.ToList().IndexOf(property) != Count - 1)
                    result.AppendLine(",");
            }

            if (Count > 0)
            {
                result.AppendLine();
                indent--;
            }
            result.Append('}');
            return result.ToString();
        }
    }
}
