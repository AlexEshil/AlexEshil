// Utils/GeoUtils.cs
using System;
using System.Collections.Generic;

namespace FieldApi.Services
{
    public static class GeoUtils
    {
        // Площадь многоугольника в метрах 
        public static double PolygonArea(List<double[]> polygon)
        {
            // Переводим lat/lng в метры 
            double R = 6378137; // радиус земли в метрах
            double area = 0;
            int n = polygon.Count;
            for (int i = 0; i < n; i++)
            {
                var p1 = polygon[i];
                var p2 = polygon[(i + 1) % n];
                area += DegToRad(p2[1] - p1[1]) * (2 + Math.Sin(DegToRad(p1[0])) + Math.Sin(DegToRad(p2[0])));
            }
            area = area * R * R / 2.0;
            return Math.Abs(area); // в метрах^2
        }

        public static double HaversineDistance(double[] from, double[] to)
        {
            double R = 6371000;
            double lat1 = DegToRad(from[0]);
            double lon1 = DegToRad(from[1]);
            double lat2 = DegToRad(to[0]);
            double lon2 = DegToRad(to[1]);
            double dlat = lat2 - lat1;
            double dlon = lon2 - lon1;

            double a = Math.Pow(Math.Sin(dlat / 2), 2) +
                       Math.Cos(lat1) * Math.Cos(lat2) *
                       Math.Pow(Math.Sin(dlon / 2), 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        public static bool PointInPolygon(double[] point, List<double[]> polygon)
        {
            int n = polygon.Count;
            bool inside = false;
            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                if (((polygon[i][0] > point[0]) != (polygon[j][0] > point[0])) &&
                    (point[1] < (polygon[j][1] - polygon[i][1]) * (point[0] - polygon[i][0]) / (polygon[j][0] - polygon[i][0]) + polygon[i][1]))
                {
                    inside = !inside;
                }
            }
            return inside;
        }

        private static double DegToRad(double deg) => deg * Math.PI / 180.0;
    }
}
