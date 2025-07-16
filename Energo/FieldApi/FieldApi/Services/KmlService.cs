// Services/KmlService.cs
using FieldApi.Models;
using System.Xml.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FieldApi.Services
{
    public class KmlService
    {
        private readonly string _fieldsPath;
        private readonly string _centroidsPath;

        private List<FieldModel> _fieldsCache;

        public KmlService()
        {
            // лежат файлы
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            _fieldsPath = Path.Combine(baseDir, "KmlFiles", "fields.kml");
            _centroidsPath = Path.Combine(baseDir, "KmlFiles", "centroids.kml");
            _fieldsCache = null;
        }

        public List<FieldModel> GetAllFields()
        {
            if (_fieldsCache != null) return _fieldsCache;

            var centroids = ParseCentroids();
            var fields = new List<FieldModel>();

            var kml = XDocument.Load(_fieldsPath);
            XNamespace ns = "http://www.opengis.net/kml/2.2";

            var placemarks = kml.Descendants(ns + "Placemark");

            foreach (var placemark in placemarks)
            {
                var id = placemark.Element(ns + "name")?.Value ?? "NoId";
                var name = id; 

                var coordsStr = placemark.Descendants(ns + "coordinates").FirstOrDefault()?.Value.Trim();
                var coords = ParseCoordinates(coordsStr);
                if (coords.Count == 0) continue;

                var center = centroids.ContainsKey(id) ? centroids[id] : GetPolygonCentroid(coords);
                var size = GeoUtils.PolygonArea(coords);

                fields.Add(new FieldModel
                {
                    Id = id,
                    Name = name,
                    Size = size,
                    Locations = new LocationModel
                    {
                        Center = center,
                        Polygon = coords
                    }
                });
            }

            _fieldsCache = fields;
            return fields;
        }

        private Dictionary<string, double[]> ParseCentroids()
        {
            var dict = new Dictionary<string, double[]>();
            var kml = XDocument.Load(_centroidsPath);
            XNamespace ns = "http://www.opengis.net/kml/2.2";
            var placemarks = kml.Descendants(ns + "Placemark");

            foreach (var placemark in placemarks)
            {
                var id = placemark.Element(ns + "name")?.Value ?? "";
                var coordsStr = placemark.Descendants(ns + "coordinates").FirstOrDefault()?.Value.Trim();
                var coords = ParseCoordinates(coordsStr);
                if (coords.Count > 0)
                    dict[id] = coords[0];
            }
            return dict;
        }

        private List<double[]> ParseCoordinates(string coordsStr)
        {
            var list = new List<double[]>();
            if (string.IsNullOrEmpty(coordsStr)) return list;
            var pairs = coordsStr.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var pair in pairs)
            {
                var parts = pair.Split(',');
                if (parts.Length >= 2 &&
                    double.TryParse(parts[1], System.Globalization.CultureInfo.InvariantCulture, out double lat) &&
                    double.TryParse(parts[0], System.Globalization.CultureInfo.InvariantCulture, out double lng))
                {
                    list.Add(new[] { lat, lng });
                }
            }
            return list;
        }

        private double[] GetPolygonCentroid(List<double[]> coords)
        {
            double lat = 0, lng = 0;
            foreach (var p in coords)
            {
                lat += p[0];
                lng += p[1];
            }
            return new[] { lat / coords.Count, lng / coords.Count };
        }
    }
}
