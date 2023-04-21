using Azure.Data.Tables;
using MvcCoreSasStorage.Helpers;
using MvcCoreSasStorage.Models;
using System.Net;
using System;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;

namespace MvcCoreSasStorage.Services
{
    public class ServiceStorageTables
    {
        private TableClient tableAlumnos;
        private HelperPathProvider helper;
        private XDocument documentAlumnos;
        private string pathAlumnos;
        private bool isXmlLoaded = false;
        private string UrlApi;

        public ServiceStorageTables(TableServiceClient tableService, HelperPathProvider helper, IConfiguration configuration)
        {
            this.UrlApi = configuration.GetValue<string>("ApiUrls:ApiTableTokens");
            this.helper = helper;
            this.pathAlumnos = this.helper.MapPath("alumnos_tables.xml", Folders.Documents);
            this.tableAlumnos = tableService.GetTableClient("alumnos");
            Task.Run(async () =>
            {
                await this.tableAlumnos.CreateIfNotExistsAsync();
            });

            if (!isXmlLoaded)
            {
                documentAlumnos = XDocument.Load(this.pathAlumnos);
                Task.Run(async () =>
                {
                    await LoadXML();
                });
                isXmlLoaded = true;
            }
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
            return alumnos.OrderBy(x => x.IdAlumno).ToList();
        }

        public async Task<List<Alumno>> GetAlumnosCursoAsync(string curso)
        {
            var query = this.tableAlumnos.Query<Alumno>(x => x.Curso == curso);
            return query.ToList();
        }

        public async Task<List<Alumno>> GetAlumnosAsync(string token)
        {
            Uri uriToken = new Uri(token);
            this.tableAlumnos = new TableClient(uriToken);
            List<Alumno> alumnos = new List<Alumno>();
            var consulta = this.tableAlumnos.QueryAsync<Alumno>
                (filter: "");
            await foreach (Alumno al in consulta)
            {
                alumnos.Add(al);
            }
            return alumnos;
        }

        public async Task<string> GetTokenAsync(string curso)
        {
            using (WebClient client = new WebClient())
            {
                string request = "/api/tabletoken/generatetoken/" + curso;
                client.Headers["content-type"] = "application/json";
                Uri uri = new Uri(this.UrlApi + request);
                string data = await client.DownloadStringTaskAsync(uri);
                JObject objetoJSON = JObject.Parse(data);
                string token = objetoJSON.GetValue("token").ToString();
                return token;
            }
        }
    }
}
