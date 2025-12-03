using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;

namespace CG.Test.Editor.FrontEnd.Models
{
    public class SchemaNumberType(double minimum, double maximum) : SchemaTypeBase
    {
		public double Minimum { get; } = minimum;
        public double Maximum { get; } = maximum;

        public override bool IsConvertibleFrom(SchemaTypeBase sourceType) => sourceType is SchemaIntegerType || sourceType is SchemaNumberType;
    }
}
