﻿using Azure.Data.Tables;
using MvcCoreSasStorage.Helpers;
using MvcCoreSasStorage.Models;
using System.Xml.Linq;

namespace MvcCoreSasStorage.Services
{
    public class ServiceStorageTables
    {
        private TableClient tableAlumnos;
        private HelperPathProvider helper;
        private XDocument documentAlumnos;
        private string pathAlumnos;

        public ServiceStorageTables(TableServiceClient tableService, HelperPathProvider helper)
        {
            this.tableAlumnos = tableService.GetTableClient("alumnos");
            Task.Run(async () =>
            {
                await this.tableAlumnos.CreateIfNotExistsAsync();
            });
            this.helper = helper;
            this.pathAlumnos = this.helper.MapPath("alumnos_tables.xml", Folders.Documents);
            documentAlumnos = XDocument.Load(this.pathAlumnos);

        }

        public async Task LoadXML()
        {
            var consulta = from datos in documentAlumnos.Descendants("alumno")
                           select datos;
            foreach (XElement tag in consulta)
            {
                await this.CreateAlumnoAsync(int.Parse(tag.Element("idalumno").Value), tag.Element("nombre").Value, tag.Element("apellidos").Value, int.Parse(tag.Element("nota").Value), tag.Element("curso").Value);
            }
        }

        public async Task CreateAlumnoAsync(int id, string nombre, string apellidos, int nota, string curso)
        {
            Alumno alumno = new Alumno();
            alumno.IdAlumno = id;
            alumno.Nombre = nombre;
            alumno.Apellidos = apellidos;
            alumno.Nota = nota;
            alumno.Curso = curso;
            await this.tableAlumnos.AddEntityAsync<Alumno>(alumno);
        }

        /*public async Task<Cliente> FindClienteAsync(string partitionKey, string rowKey)
        {
            Cliente cliente = await this.tableClient.GetEntityAsync<Cliente>(partitionKey, rowKey);
            return cliente;
        }

        public async Task DeleteClienteAsync(string partitionKey, string rowKey)
        {
            await this.tableClient.DeleteEntityAsync(partitionKey, rowKey);
        }*/

        public async Task<List<Alumno>> GetAlumnosAsync()
        {
            List<Alumno> alumnos = new List<Alumno>();
            var query = this.tableAlumnos.QueryAsync<Alumno>(filter: "");
            await foreach (Alumno item in query)
            {
                alumnos.Add(item);
            }
            return alumnos;
        }

        public async Task<List<Alumno>> GetAlumnosCursoAsync(string curso)
        {
            var query = this.tableAlumnos.Query<Alumno>(x => x.Curso == curso);
            return query.ToList();
        }
    }
}