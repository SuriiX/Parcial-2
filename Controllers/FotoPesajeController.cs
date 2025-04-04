using Parcial__2.Clases;
using Parcial__2.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Parcial__2.Controllers
{
    [RoutePrefix("api/FotoPesaje")]
    public class FotoPesajeController : ApiController
    {
        private readonly clsOpeFotoPesaje _opeFotoPesaje = new clsOpeFotoPesaje();
        private readonly DBExamenEntities ExaEnt = new DBExamenEntities();

        [HttpGet]
        [Route("ConsultarDetallePorPlaca/{placa}")]
        public HttpResponseMessage ConsultarDetallePorPlaca(string placa)
        {
            var resultados = (from c in ExaEnt.Camions
                              join p in ExaEnt.Pesajes on c.Placa equals p.PlacaCamion
                              join fp in ExaEnt.FotoPesajes on p.id equals fp.idPesaje
                              where c.Placa == placa
                              select new
                              {
                                  c.Placa,
                                  c.NumeroEjes,
                                  c.Marca,
                                  p.FechaPesaje,
                                  p.Peso,
                                  fp.ImagenVehiculo,
                                  p.id // Necesitamos el ID del pesaje para agrupar las imágenes
                              }).ToList();

            if (!resultados.Any())
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, $"No se encontraron detalles para la placa: {placa}");
            }

            // Agrupamos los resultados por ID de pesaje para obtener las imágenes por cada pesaje
            var detallesPorPesaje = resultados
                .GroupBy(r => r.id)
                .Select(g => new PesajeDetalleDto
                {
                    Placa = g.First().Placa,
                    NumeroEjes = g.First().NumeroEjes,
                    Marca = g.First().Marca,
                    FechaPesaje = g.First().FechaPesaje,
                    PesoObtenido = g.First().Peso,
                    NombresImagenes = g.Select(r => r.ImagenVehiculo).ToList()
                })
                .ToList();

            return Request.CreateResponse(HttpStatusCode.OK, detallesPorPesaje);
        }


        [HttpGet]
        [Route("ListarArchivos")]
        public HttpResponseMessage ListarArchivos()
        {
            string rutaBase = System.Web.Hosting.HostingEnvironment.MapPath("~/Archivos");
            if (rutaBase == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "No se pudo resolver la ruta del directorio.");
            }

            try
            {
                if (Directory.Exists(rutaBase))
                {
                    string[] archivos = Directory.GetFiles(rutaBase);
                    List<string> nombresDeArchivos = archivos.Select(Path.GetFileName).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, nombresDeArchivos);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "El directorio de archivos no existe.");
                }
            }
            catch (System.Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, $"Error al listar los archivos: {ex.Message}");
            }
        }
        [HttpPost]
        [Route("{idPesaje}")]
        public async Task<HttpResponseMessage> CargarFotoPesaje(int idPesaje)
        {
            _opeFotoPesaje.request = Request;
            return await _opeFotoPesaje.GrabarFotoPesaje(idPesaje, false);
        }

        [HttpPut]
        [Route("{idPesaje}")]
        public async Task<HttpResponseMessage> ActualizarFotoPesaje(int idPesaje)
        {
            _opeFotoPesaje.request = Request;
            return await _opeFotoPesaje.GrabarFotoPesaje(idPesaje, true);
        }

        [HttpGet]
        [Route("ConsultarArchivo")]
        public HttpResponseMessage ConsultarArchivo([FromUri] string NombreImagen)
        {
            try
            {
                _opeFotoPesaje.request = Request;
                return _opeFotoPesaje.DescargarArchivo(NombreImagen);
            }
            catch (Exception ex)
            {
                var response = Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Error en la solicitud: " + ex.Message);
                return response;
            }
        }

        [HttpDelete]
        [Route("EliminarFoto")] 
        public HttpResponseMessage EliminarFoto([FromUri] int idPesaje, [FromUri] string nombreArchivo) 
        {
            _opeFotoPesaje.request = Request;
            return _opeFotoPesaje.EliminarFotoPesaje(idPesaje, nombreArchivo);
        }

    }
}