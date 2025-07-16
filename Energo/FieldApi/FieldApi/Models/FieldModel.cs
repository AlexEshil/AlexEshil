// Models/FieldModel.cs
using System.Collections.Generic;

namespace FieldApi.Models
{
    public class LocationModel
    {
        public double[] Center { get; set; }
        public List<double[]> Polygon { get; set; }
    }

    public class FieldModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double Size { get; set; }
        public LocationModel Locations { get; set; }
    }
}
