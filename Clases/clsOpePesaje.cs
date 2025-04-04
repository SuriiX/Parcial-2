using Parcial__2.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;

namespace Parcial__2.Clases
{
    public class clsOpePesaje
    {
        private DBExamenEntities dbExamen = new DBExamenEntities();
        public Pesaje pesaje { get; set; }
        public Camion camion { get; set; }

        public string InsertarPesajeYCamion()
        {
            try
            {
                if (pesaje == null || string.IsNullOrEmpty(pesaje.PlacaCamion))
                {
                    return "Error: Los datos del pesaje son inválidos.";
                }
                Camion camExistente = dbExamen.Camions.FirstOrDefault(c => c.Placa == pesaje.PlacaCamion);
                if (camExistente == null)
                {
                    Camion nuevoCamion = new Camion
                    {
                        Placa = pesaje.PlacaCamion,
                        Marca = "Desconocida",
                        NumeroEjes = 2
                    };

                    dbExamen.Camions.Add(nuevoCamion);
                    dbExamen.SaveChanges();
                }
                dbExamen.Pesajes.Add(pesaje);
                dbExamen.SaveChanges();
                return "Pesaje registrado correctamente";
            }
            catch (Exception ex)
            {
                return "Error al registrar el pesaje: " + ex.Message;
            }
        }



        public List<Pesaje> ConsultarTodos()
        {
            return dbExamen.Pesajes
                .OrderBy(p => p.FechaPesaje)
                .ToList();
        }

        public Pesaje Consultar(int id)
        {
            return dbExamen.Pesajes.FirstOrDefault(p => p.id == id);
        }

        public string Eliminar(int id)
        {
            try
            {
                Pesaje pesajeExistente = Consultar(id);
                if (pesajeExistente == null)
                {
                    return "El pesaje con el ID ingresado no existe, por lo tanto no se puede eliminar";
                }

                dbExamen.Pesajes.Remove(pesajeExistente);
                dbExamen.SaveChanges();
                return "Pesaje eliminado correctamente";
            }
            catch (Exception ex)
            {
                return "Error al eliminar el pesaje: " + ex.Message;
            }
        }

        public List<Pesaje> ConsultarPorPlaca(string placa)
        {
            return dbExamen.Pesajes
                .Where(p => p.PlacaCamion == placa)
                .OrderByDescending(p => p.FechaPesaje)
                .ToList();
        }
    }
}