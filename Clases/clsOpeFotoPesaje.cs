using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using Parcial__2.Models;

namespace Parcial__2.Clases
{
    public class clsOpeFotoPesaje
    {
        private readonly DBExamenEntities ExaEnt = new DBExamenEntities();
        private List<string> ArchivosGuardados;
        public HttpRequestMessage request { get; set; }

        public async Task<HttpResponseMessage> GrabarFotoPesaje(int idPesaje, bool actualizar)
        {
            if (request == null || !request.Content.IsMimeMultipartContent())
            {
                if (request == null) return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError) { ReasonPhrase = "Request object not initialized." };
                return request.CreateErrorResponse(System.Net.HttpStatusCode.UnsupportedMediaType, "La solicitud no es de tipo multipart/form-data.");
            }

            string root = HttpContext.Current.Server.MapPath("~/Archivos");
            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }

            var provider = new MultipartFormDataStreamProvider(root);
            try
            {
                bool algunArchivoExiste = false;
                ArchivosGuardados = new List<string>();
                await request.Content.ReadAsMultipartAsync(provider);
                if (provider.FileData.Count == 0)
                {
                    return request.CreateErrorResponse(System.Net.HttpStatusCode.BadRequest, "No se envió ningún archivo para procesar.");
                }
                foreach (MultipartFileData file in provider.FileData)
                {
                    string tempFilePath = file.LocalFileName;
                    string originalFileName = file.Headers.ContentDisposition.FileName;
                    if (originalFileName.StartsWith("\"") && originalFileName.EndsWith("\""))
                    {
                        originalFileName = originalFileName.Trim('"');
                    }
                    if (originalFileName.Contains(@"/") || originalFileName.Contains(@"\"))
                    {
                        originalFileName = Path.GetFileName(originalFileName);
                    }
                    originalFileName = Path.GetFileName(originalFileName);
                    string finalFilePath = Path.Combine(root, originalFileName);
                    if (File.Exists(finalFilePath))
                    {
                        if (actualizar)
                        {
                            File.Delete(finalFilePath);
                            File.Move(tempFilePath, finalFilePath);
                            ArchivosGuardados.Add(originalFileName);
                        }
                        else
                        {
                            File.Delete(tempFilePath);
                            algunArchivoExiste = true;
                        }
                    }
                    else
                    {
                        if (actualizar)
                        {
                            File.Delete(tempFilePath);
                            return request.CreateErrorResponse(System.Net.HttpStatusCode.NotFound, $"El archivo '{originalFileName}' no existe y no se puede actualizar. Use el método de creación.");
                        }
                        else
                        {
                            File.Move(tempFilePath, finalFilePath);
                            ArchivosGuardados.Add(originalFileName);
                            algunArchivoExiste = false;
                        }
                    }
                }
                if (ArchivosGuardados.Count > 0)
                {
                    string RptaBD = ProcesarBDFotoPesaje(idPesaje, actualizar);

                    if (actualizar)
                    {
                        return request.CreateResponse(System.Net.HttpStatusCode.OK, $"Archivos actualizados correctamente. {RptaBD}");
                    }
                    else
                    {
                        if (algunArchivoExiste)
                        {
                            return request.CreateResponse(System.Net.HttpStatusCode.OK, $"Se procesaron los archivos. Algunos archivos nuevos se guardaron ({ArchivosGuardados.Count} archivos). Otros ya existían y no fueron modificados. {RptaBD}");
                        }
                        else
                        {
                            return request.CreateResponse(System.Net.HttpStatusCode.OK, $"Archivos creados correctamente ({ArchivosGuardados.Count} archivos). {RptaBD}");
                        }
                    }
                }
                else if (algunArchivoExiste && !actualizar)
                {
                    return request.CreateErrorResponse(System.Net.HttpStatusCode.Conflict, "Todos los archivos enviados ya existen en el servidor.");
                }
                else if (actualizar)
                {
                    return request.CreateErrorResponse(System.Net.HttpStatusCode.BadRequest, "No se pudo actualizar ningún archivo. Verifique los nombres.");
                }
                else
                {
                    return request.CreateErrorResponse(System.Net.HttpStatusCode.InternalServerError, "Ocurrió una situación inesperada al procesar los archivos.");
                }
            }
            catch (Exception ex)
            {
                foreach (var fileData in provider.FileData)
                {
                    if (File.Exists(fileData.LocalFileName))
                    {
                        File.Delete(fileData.LocalFileName);
                    }
                }
                return request.CreateErrorResponse(System.Net.HttpStatusCode.InternalServerError, $"Error al procesar archivos: {ex.Message}");
            }
        }

        public HttpResponseMessage DescargarArchivo(string Imagen)
        {
            try
            {
                string rutaBase = HttpContext.Current.Server.MapPath("~/Archivos");
                string archivoRuta = Path.Combine(rutaBase, Imagen);
                System.Diagnostics.Debug.WriteLine($"Intentando descargar el archivo en la ruta: {archivoRuta}");

                if (File.Exists(archivoRuta))
                {
                    HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                    var stream = new FileStream(archivoRuta, FileMode.Open, FileAccess.Read);
                    response.Content = new StreamContent(stream);
                    response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                    {
                        FileName = Imagen
                    };
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    return response;
                }
                else
                {
                    return request.CreateErrorResponse(HttpStatusCode.NotFound, "No se encontró el archivo.");
                }
            }
            catch (Exception ex)
            {
                return request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        public HttpResponseMessage EliminarFotoPesaje(int idPesaje, string nombreArchivo)
        {
            if (request == null)
                return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError) { ReasonPhrase = "Request object not initialized." };

            if (string.IsNullOrWhiteSpace(nombreArchivo) || nombreArchivo.Contains("..") || nombreArchivo.Contains("/") || nombreArchivo.Contains("\\"))
            {
                return request.CreateErrorResponse(System.Net.HttpStatusCode.BadRequest, "Nombre de archivo no válido.");
            }

            string rutaBase = HttpContext.Current.Server.MapPath("~/Archivos");
            if (rutaBase == null)
            {
                return request.CreateErrorResponse(System.Net.HttpStatusCode.InternalServerError, "No se pudo resolver la ruta del directorio.");
            }

            string rutaCompletaArchivo = Path.Combine(rutaBase, nombreArchivo);
            System.Diagnostics.Debug.WriteLine($"Intentando eliminar el archivo en la ruta: {rutaCompletaArchivo}");

            try
            {
                if (File.Exists(rutaCompletaArchivo))
                {
                    File.Delete(rutaCompletaArchivo);

                    var fotoEnBD = ExaEnt.FotoPesajes.FirstOrDefault(fp => fp.idPesaje == idPesaje && fp.ImagenVehiculo == nombreArchivo);
                    if (fotoEnBD != null)
                    {
                        ExaEnt.FotoPesajes.Remove(fotoEnBD);
                        ExaEnt.SaveChanges();
                        return request.CreateResponse(System.Net.HttpStatusCode.OK, $"Archivo '{nombreArchivo}' eliminado correctamente.");
                    }
                    else
                    {
                        return request.CreateResponse(System.Net.HttpStatusCode.OK, $"Archivo '{nombreArchivo}' eliminado del sistema de archivos, pero no se encontró registro en la base de datos.");
                    }
                }
                else
                {
                    return request.CreateErrorResponse(System.Net.HttpStatusCode.NotFound, $"No se encontró el archivo '{nombreArchivo}'.");
                }
            }
            catch (Exception ex)
            {
                return request.CreateErrorResponse(System.Net.HttpStatusCode.InternalServerError, $"Error al eliminar el archivo '{nombreArchivo}': {ex.Message}");
            }
        }

        private string ProcesarBDFotoPesaje(int idPesaje, bool actualizar)
        {
            if (ArchivosGuardados == null || ArchivosGuardados.Count == 0)
            {
                return "No hay información de archivos para guardar en la base de datos.";
            }

            try
            {
                if (actualizar)
                {
                    var fotosAntiguas = ExaEnt.FotoPesajes.Where(fp => fp.idPesaje == idPesaje).ToList();
                    if (fotosAntiguas.Any())
                    {
                        ExaEnt.FotoPesajes.RemoveRange(fotosAntiguas);
                    }
                }

                foreach (string nombreArchivo in ArchivosGuardados)
                {
                    FotoPesaje nuevaFoto = new FotoPesaje
                    {
                        idPesaje = idPesaje,
                        ImagenVehiculo = nombreArchivo,
                    };
                    ExaEnt.FotoPesajes.Add(nuevaFoto);

                }

                int cambios = ExaEnt.SaveChanges();
                return $"Se registraron {cambios} referencias de fotos en la base de datos.";
            }
            catch (Exception ex)
            {
                return $"Error al guardar información en la base de datos: {ex.Message}";
            }
        }
    }
}