using Microsoft.AspNetCore.Mvc;
using FieldApi.Models;
using FieldApi.Services;
using System.Collections.Generic;
using System.Linq;

namespace FieldApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FieldsController : ControllerBase
    {
        private readonly KmlService _kml;

        public FieldsController()
        {
            _kml = new KmlService();
        }

        // 1. Получение всех полей
        [HttpGet]
        public ActionResult<IEnumerable<FieldModel>> GetAllFields()
        {
            return Ok(_kml.GetAllFields());
        }

        // 2. Площадь по id
        [HttpGet("{id}/size")]
        public ActionResult<double> GetFieldSize(string id)
        {
            var field = _kml.GetAllFields().FirstOrDefault(f => f.Id == id);
            if (field == null) return NotFound();
            return Ok(field.Size);
        }

        // 3. Расстояние от центра поля до точки
        [HttpPost("{id}/distance")]
        public ActionResult<double> GetDistanceToPoint(string id, [FromBody] PointModel point)
        {
            var field = _kml.GetAllFields().FirstOrDefault(f => f.Id == id);
            if (field == null) return NotFound();
            var dist = GeoUtils.HaversineDistance(field.Locations.Center, new double[] { point.Lat, point.Lng });
            return Ok(dist);
        }

        // 4. Принадлежит ли точка полю
        [HttpPost("contains")]
        public ActionResult<object> CheckPointInField([FromBody] PointModel point)
        {
            var fields = _kml.GetAllFields();
            var pt = new double[] { point.Lat, point.Lng };
            foreach (var f in fields)
            {
                if (GeoUtils.PointInPolygon(pt, f.Locations.Polygon))
                {
                    return Ok(new { id = f.Id, name = f.Name });
                }
            }
            return Ok(false);
        }
    }
}
