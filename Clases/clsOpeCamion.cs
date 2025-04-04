using Parcial__2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Parcial__2.Clases
{
    public class clsOpeCamion
    {

        private readonly DBExamenEntities ExaEnt = new DBExamenEntities();
        public Camion Cami { get; set; }


    }
}
public class PesajeDetalleDto
{
    public string Placa { get; set; }
    public int NumeroEjes { get; set; }
    public string Marca { get; set; }
    public DateTime FechaPesaje { get; set; }
    public float PesoObtenido { get; set; }
    public List<string> NombresImagenes { get; set; }
}
public class PesajeCompletoDTO
{
    public Pesaje Pesaje { get; set; }
    public Camion Camion { get; set; }
}