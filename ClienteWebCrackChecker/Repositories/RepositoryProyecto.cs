using ClienteWebCrackChecker.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGetCrackChecker.Helpers;
using NuGetCrackChecker.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ClienteWebCrackChecker.Repositories
{
    public class RepositoryProyecto
    {
        private String url;
        private MediaTypeWithQualityHeaderValue header;
        public RepositoryProyecto()
        {
            //this.url = "https://localhost:44367/";
            this.url = "https://apicrackchecker.azurewebsites.net/";
            this.header = new MediaTypeWithQualityHeaderValue("application/json");
        }

        public async Task<String> GetToken(String user, String password)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                LoginModel login = new LoginModel();
                login.Username = user;
                login.Password = password;
                String json = JsonConvert.SerializeObject(login);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                String request = "Auth/Login";
                HttpResponseMessage response = await client.PostAsync(request, content);
                if (response.IsSuccessStatusCode)
                {
                    String data = await response.Content.ReadAsStringAsync();
                    JObject jobject = JObject.Parse(data);
                    String token = jobject.GetValue("response").ToString();
                    return token;
                }
                else
                {
                    return null;
                }
            }
        }
        private async Task<T> CallApi<T>(String request)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                HttpResponseMessage response = await client.GetAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    T data = await response.Content.ReadAsAsync<T>();
                    return (T)Convert.ChangeType(data, typeof(T));
                }
                else
                {
                    return default(T);
                }
            }
        }

        private async Task<T> CallApi<T>(String request, String token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.GetAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    T data = await response.Content.ReadAsAsync<T>();
                    return (T)Convert.ChangeType(data, typeof(T));
                }
                else
                {
                    return default(T);
                }
            }
        }

        public async Task<Usuario> PerfilUsuarioAsync(String token)
        {
           Usuario u = await this.CallApi<Usuario>("api/usuarios/perfil", token);
           return u;
        }

        public async Task<Usuario> BuscarUsuarioAsync(int id)
        {
            Usuario u = await this.CallApi<Usuario>("api/usuarios/" + id);
            return u;
        }

        public async Task<Usuario> BuscarUsuarioAsync(String email)
        {
            Usuario u = await this.CallApi<Usuario>("api/usuarios/buscarusuarioemail/" + email);
            return u;
        }

        public async Task<List<Juego>> GetJuegosAsync()
        {
            List<Juego> juegos = await this.CallApi<List<Juego>>("api/juegos");
            return juegos;
        }

        public async Task<Juego> BuscarJuegoIdAsync(String id)
        {
            Juego j = await this.CallApi<Juego>("api/juegos/" + id);
            return j;
        }

        public async Task<Juego> BuscarJuegoSlugAsync(String slug)
        {
            Juego j = await this.CallApi<Juego>("api/juegos/buscarjuegoslug/" + slug);
            return j;
        }

        public async Task<List<Juego>> BuscarJuegosAsync(String busqueda)
        {
            List<Juego> juegos = await this.CallApi<List<Juego>>("api/juegos/buscarjuegos/" + busqueda);
            return juegos;
        }

        public async Task<List<Juego>> PaginarJuegosAsync(int posicion)
        {
            List<Juego> juegos = await this.CallApi<List<Juego>>("api/juegos/paginarjuegos/" + posicion);
            return juegos;
        }
        public async Task EditarJuegoAsync(String id, DateTime fechaCrack, DateTime fechaLanz, String token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                Juego j = await this.BuscarJuegoIdAsync(id);
                j.FechaCrack = fechaCrack;
                j.FechaLanzamiento = fechaLanz.ToString("yyyy-MM-dd");
                String json = JsonConvert.SerializeObject(j);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PutAsync("api/juegos/editar", content);
            }
        }

        public async Task<List<Usuario>> GetUsuariosAsync(String token)
        {
            List<Usuario> users = await this.CallApi<List<Usuario>>("api/usuarios", token);
            return users;
        }

        public async Task EditarUsuarioAdminAsync(int id, String role, int activo, String token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                Usuario u = await this.BuscarUsuarioAsync(id);
                u.Role = role;
                u.Activo = activo;
                String json = JsonConvert.SerializeObject(u);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PutAsync("api/usuarios/editarusuarioadmin", content);
            }
        }

        public async Task DarBajaCuentaAsync(int id, String token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                StringContent content = new StringContent(String.Empty, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PutAsync("api/usuarios/darbaja/" + id, content);
            }
        }

        public async Task<List<String>> GetIdJuegoListaAsync(int id, String token)
        {
            List<String> deseados = await this.CallApi<List<String>>("api/usuarios/listadeseados/" + id, token);
            return deseados;
        }

        public async Task AnyadirJuegoListaDeseadosAsync(String idjuego, int iduser, String token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                StringContent content = new StringContent(String.Empty, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync("api/juegos/insertarjuegolista/" + idjuego + "/" + iduser, content);
            }
        }

        public async Task EliminarJuegoListaDeseadosAsync(String idjuego, int iduser, String token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.DeleteAsync("api/juegos/eliminarjuegolista/" + idjuego + "/" + iduser);
            }
        }

        public async Task<String> ComprobarDisponibilidadAsync(String email, String username)
        {
            String mensaje = await this.CallApi<String>("api/usuarios/comprobardisponibilidad/" + email + "/" + username);
            return mensaje;
        }

        public async Task<int> RegistrarUsuarioAsync(String email, byte[] password, String salt, String username)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                Usuario u = new Usuario()
                {
                    Email = email,
                    Password = password,
                    Salt = salt,
                    Username = username
                };
                String json = JsonConvert.SerializeObject(u);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync("api/usuarios/registrar", content);
                if (response.IsSuccessStatusCode)
                {
                    return int.Parse(await response.Content.ReadAsStringAsync());
                }
                else
                {
                    return 0;
                }
            }
        }

        public async Task<String> EnviarEmailActivacionAsync(String email, int id)
        {
            String msj = await this.CallApi<String>("api/manage/enviaremailactivacion/" + id + "/" + email);
            return msj;
        }

        public async Task<String> EnviarEmailRecuperacionAsync(String email)
        {
            String msj = await this.CallApi<String>("api/manage/enviaremailrecuperacion/" + email);
            return msj;
        }

        public async Task ActivarCuentaAsync(int id)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                StringContent content = new StringContent(String.Empty, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PutAsync("api/manage/activar/"+id, content);
            }
        }

        public async Task EditarUsuarioAsync(int id, String email, String role, String nombre, String apellidos, DateTime fechaNac, int telefono, String username, String archivo, String token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                Usuario u = await this.BuscarUsuarioAsync(id);
                u.Email = email;
                u.Role = role;
                u.Nombre = nombre;
                u.Apellidos = apellidos;
                u.FechaNacimiento = fechaNac;
                u.Username = username;
                u.Foto = archivo;
                u.Telefono = telefono;
                String data = JsonConvert.SerializeObject(u);
                StringContent content = new StringContent(data, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PutAsync("api/usuarios/editar", content);
            }
        }

        public async Task<String> SubirImagenAsync(IFormFile archivo, String token)
        {
            using (HttpClient client = new HttpClient())
            {
                String fichero = "";
                client.BaseAddress = new Uri(this.url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                if (archivo.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        archivo.CopyTo(ms);
                        var fileBytes = ms.ToArray();
                        fichero = Convert.ToBase64String(fileBytes);
                    }
                }
                FileData imagen = new FileData()
                {
                    Archivo = fichero,
                    Nombre = archivo.FileName
                };
                String data = JsonConvert.SerializeObject(imagen);
                StringContent content = new StringContent(data, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync("api/storage/subirimagen", content);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    return String.Empty;
                }
            }
        }

        public async Task CambiarPasswordAsync(int id, String password, String token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                Usuario u = await this.BuscarUsuarioAsync(id);
                byte[] pass = CypherHelper.CifradoHashSHA256(password + u.Salt);
                u.Password = pass;
                String data = JsonConvert.SerializeObject(u);
                StringContent content = new StringContent(data, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PutAsync("api/usuarios/cambiarpass", content);
            }
        }
        public async Task EliminarUsuarioAsync(int id, String token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.DeleteAsync("api/manage/eliminarusuario/" + id);
            }
        }
    }
}
