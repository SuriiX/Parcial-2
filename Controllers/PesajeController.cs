using Parcial__2.Models;
using Parcial__2.Clases;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace Servicios_Jue.Controllers
{
    [RoutePrefix("api/pesaje")]
    public class PesajeController : ApiController
    {
            private clsOpePesaje servicioPesaje = new clsOpePesaje();
        [HttpPost]
        [Route("registrar")]
        public IHttpActionResult RegistrarPesaje([FromBody] Pesaje pesaje)
        {
            if (pesaje == null || string.IsNullOrEmpty(pesaje.PlacaCamion))
            {
                return BadRequest("Datos de pesaje inválidos.");
            }

            servicioPesaje.pesaje = pesaje;  // Asigna solo el pesaje

            string resultado = servicioPesaje.InsertarPesajeYCamion();
            return Ok(resultado);
        }

        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult ConsultarPesaje(int id)
        {
            Pesaje pesaje = servicioPesaje.Consultar(id);
            if (pesaje == null)
            {
                return NotFound();
            }
            return Ok(pesaje);
        }

        [HttpGet]
        [Route("placa/{placa}")]
        public IHttpActionResult ConsultarPesajesPorPlaca(string placa)
        {
            List<Pesaje> pesajes = servicioPesaje.ConsultarPorPlaca(placa);
            return Ok(pesajes);
        }


        [HttpDelete]
        [Route("{id:int}")]
        public IHttpActionResult EliminarPesaje(int id)
        {
            string resultado = servicioPesaje.Eliminar(id);
            if (resultado.StartsWith("Error"))
            {
                return NotFound();
            }
            return Ok(resultado);
        }
    }
}