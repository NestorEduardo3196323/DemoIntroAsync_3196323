using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;

namespace DemoIntroAsync_3196323
{

    public partial class Form1 : Form
    {
        HttpClient httpClient = new HttpClient();

        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            pictureBox1.Visible = true;

            var directorioActual = AppDomain.CurrentDomain.BaseDirectory;
            var destinoBaseSecuencial = Path.Combine(directorioActual, @"Imagenes\resultado-secuencial");
            var destinoBaseParalelo = Path.Combine(directorioActual, @"Imagenes\resultado-paralelo");
            PrepararEjecución(destinoBaseParalelo, destinoBaseSecuencial);

            Console.WriteLine("Inicio");
            List<Imagen> imagenes = ObtenerImagenes();

            // Parte secuencial
            var sw = new Stopwatch();

            sw.Start();

            foreach (var imagen in imagenes)
            {
                await ProcesarImagen(destinoBaseSecuencial, imagen);
            }

            Console.WriteLine("Secuencial - duración en segundos: {0}",
            sw.ElapsedMilliseconds / 1000.0);

            sw.Reset();

            sw.Start();

            var tareasEnumerable = imagenes.Select(async imagen =>
            {
                await ProcesarImagen(destinoBaseParalelo, imagen);
            });

            await Task.WhenAll(tareasEnumerable);

            Console.WriteLine("Paralelo - duración en segundos: {0}",
            sw.ElapsedMilliseconds / 1000.0);

            sw.Stop();

            pictureBox1.Visible = false;
        }

        private async Task ProcesarImagen(string directorio, Imagen imagen)
        {
            var respuesta = await httpClient.GetAsync(imagen.URL);
            var contenido = await respuesta.Content.ReadAsByteArrayAsync();

            Bitmap bitmap;
            using (var ms = new MemoryStream(contenido))
            {
                bitmap = new Bitmap(ms);
            }

            bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
            var destino = Path.Combine(directorio, imagen.Nombre);
            bitmap.Save(destino);
        }

        private static List<Imagen> ObtenerImagenes()
        {
            var imagenes = new List<Imagen>();

            for (int i = 0; i < 7; i++)
            {
                imagenes.Add(
                new Imagen()
                {
                    Nombre = $"Miku_foto1_{i}.jpg",
                    URL = "https://w.wallhaven.cc/full/2y/wallhaven-2yqzpg.jpg"
                });

                imagenes.Add(
                new Imagen()
                {
                    Nombre = $"Miku_foto2_{i}.jpg",
                    URL = "https://i.pinimg.com/originals/87/f7/03/87f703159e5fc2ec69a894e3ef4a817e.jpg"
                });

                imagenes.Add(
                new Imagen()
                {
                    Nombre = $"Miku_foto3_{i}.jpg",
                    URL = "https://images-wixmp-ed30a86b8c4ca887773594c2.wixmp.com/f/1bb5e3a6-1e89-4286-8e50-12463464feda/dfi40d5-9c0d897c-36ce-4747-9d0f-a7c83af6d1dd.png/v1/fill/w_1192,h_670,q_70,strp/te_amo_miku_by_amazingmiku7_dfi40d5-pre.jpg?token=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJ1cm46YXBwOjdlMGQxODg5ODIyNjQzNzNhNWYwZDQxNWVhMGQyNmUwIiwiaXNzIjoidXJuOmFwcDo3ZTBkMTg4OTgyMjY0MzczYTVmMGQ0MTVlYTBkMjZlMCIsIm9iaiI6W1t7ImhlaWdodCI6Ijw9NzIwIiwicGF0aCI6IlwvZlwvMWJiNWUzYTYtMWU4OS00Mjg2LThlNTAtMTI0NjM0NjRmZWRhXC9kZmk0MGQ1LTljMGQ4OTdjLTM2Y2UtNDc0Ny05ZDBmLWE3YzgzYWY2ZDFkZC5wbmciLCJ3aWR0aCI6Ijw9MTI4MCJ9XV0sImF1ZCI6WyJ1cm46c2VydmljZTppbWFnZS5vcGVyYXRpb25zIl19.RazB_EIzC4-_iDKf_8obEWLNKw75T1Sy_O5TKm-_oCk"
                });
            }

            return imagenes;
        }

        private void BorrarArchivos(string directorio)
        {
            var archivos = Directory.EnumerateFiles(directorio);
            foreach (var archivo in archivos)
            {
                System.IO.File.Delete(archivo); // Solucionado por completo
            }
        }

        private void PrepararEjecución(string destinoBaseParalelo, string destinoBaseSecuencial)
        {
            if (!Directory.Exists(destinoBaseParalelo))
            {
                Directory.CreateDirectory(destinoBaseParalelo);
            }

            if (!Directory.Exists(destinoBaseSecuencial))
            {
                Directory.CreateDirectory(destinoBaseSecuencial);
            }

            BorrarArchivos(destinoBaseSecuencial);
            BorrarArchivos(destinoBaseParalelo);
        }
    }
}