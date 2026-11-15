using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace GestionDeFicheros2
{
    public class Program
    {
        // Rutas por defecto (relativas, como recomienda el temario)
        static string rutaCsv = "./NetflixOriginals.csv";
        static string rutaXml = "./NetflixOriginals.xml";
        // Ruta del fichero de log
        static string rutaLog = "./NetflixOriginals_log.txt";

        public static void Main(string[] args)
        {
            int opcion;
            do
            {
                Console.WriteLine("\n---- MENÚ ----");
                Console.WriteLine("1_Exportar todo el documento (CSV → XML).");
                Console.WriteLine("2_Exportar por género.");
                Console.WriteLine("3_Exportar por año (y superiores).");
                Console.WriteLine("4_Exportar por lenguaje.");
                Console.WriteLine("5_Exportar XML → CSV (todo).");
                Console.WriteLine("6_Exportar XML → CSV por género.");
                Console.WriteLine("7_Exportar XML → CSV por año.");
                Console.WriteLine("8_Exportar XML → CSV por lenguaje.");
                Console.WriteLine("9_Cambiar rutas de ficheros.");
                Console.WriteLine("0_Salir.");
                Console.Write("Opción: ");
                opcion = int.Parse(Console.ReadLine());

                switch (opcion)
                {
                    case 1: ExportarTodo(); break;
                    case 2: ExportarPorGenero(); break;
                    case 3: ExportarPorAnio(); break;
                    case 4: ExportarPorLenguaje(); break;
                    case 5: XmlACsvTodo(); break;
                    case 6: XmlACsvPorGenero(); break;
                    case 7: XmlACsvPorAnio(); break;
                    case 8: XmlACsvPorLenguaje(); break;
                    case 9: CambiarRutas(); break;
                    case 0: Console.WriteLine("Cerrando aplicación..."); break;
                    default: Console.WriteLine("Opción no válida."); break;
                }
            } while (opcion != 0);
        }

        // ============================================================
        //  UTILIDAD DE LOG
        // ============================================================

        /// <summary>
        /// Escribe una línea en el fichero de log, añadiendo fecha y hora.
        /// Usa StreamWriter en modo Append (true) para no sobrescribir.
        /// </summary>
        static void EscribirLog(string mensaje)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(rutaLog, true))
                {
                    sw.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {mensaje}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error escribiendo log: {ex.Message}");
            }
        }

        // ============================================================
        //  PARTE 1: CSV → XML
        // ============================================================

        public static void ExportarTodo()
        {
            EscribirLog("==================================================");
            EscribirLog("INICIO PROCESO DE EXPORTACIÓN");
            EscribirLog("Formato Origen: CSV -> Formato Destino: XML");
            EscribirLog($"Fichero Origen : {Path.GetFullPath(rutaCsv)}");
            EscribirLog($"Fichero Destino: {Path.GetFullPath(rutaXml)}");

            try
            {
                string[] lineas = File.ReadAllLines(rutaCsv);
                EscribirXml(rutaXml, lineas, 1, lineas.Length - 1);

                EscribirLog("PROCESO FINALIZADO CORRECTAMENTE");
                EscribirLog($"Fichero generado en: {Path.GetFullPath(rutaXml)}");
                EscribirLog("==================================================\n");

                Console.WriteLine($"Exportado todo a {rutaXml}");
            }
            catch (Exception ex)
            {
                EscribirLog($"ERROR durante la exportación: {ex.Message}");
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public static void ExportarPorGenero()
        {
            EscribirLog("==================================================");
            EscribirLog("INICIO PROCESO DE EXPORTACIÓN (CSV -> XML) POR GÉNERO");
            EscribirLog($"Fichero Origen: {Path.GetFullPath(rutaCsv)}");

            try
            {
                string[] lineas = File.ReadAllLines(rutaCsv);

                // Primera pasada: sacar géneros únicos (ignorando cabecera)
                HashSet<string> generos = new HashSet<string>();
                for (int i = 1; i < lineas.Length; i++)
                {
                    string[] campos = lineas[i].Split(',');
                    if (campos.Length > 1)
                        generos.Add(campos[1].Trim());
                }

                Console.WriteLine("Géneros disponibles:");
                foreach (var g in generos)
                    Console.WriteLine($" - {g}");

                Console.Write("Introduce el género a exportar: ");
                string generoElegido = Console.ReadLine().Trim();

                EscribirLog($"Filtro aplicado -> Género: '{generoElegido}'");

                // Segunda pasada: filtrar
                List<string> filtradas = new List<string>();
                for (int i = 1; i < lineas.Length; i++)
                {
                    string[] campos = lineas[i].Split(',');
                    if (campos.Length > 1 && campos[1].Trim().Equals(generoElegido, StringComparison.OrdinalIgnoreCase))
                        filtradas.Add(lineas[i]);
                }

                if (filtradas.Count == 0)
                {
                    EscribirLog($"No se encontraron películas con género '{generoElegido}'. Proceso cancelado.");
                    Console.WriteLine("No se encontraron películas con ese género.");
                    return;
                }

                string salida = $"./NetflixOriginals_{generoElegido.Replace(" ", "_")}.xml";
                EscribirLog($"Fichero Destino: {Path.GetFullPath(salida)}");

                string[] nuevasLineas = new string[filtradas.Count + 1];
                nuevasLineas[0] = lineas[0]; // cabecera
                filtradas.CopyTo(nuevasLineas, 1);

                EscribirXml(salida, nuevasLineas, 1, nuevasLineas.Length - 1);

                EscribirLog("PROCESO FINALIZADO CORRECTAMENTE");
                EscribirLog($"Fichero generado en: {Path.GetFullPath(salida)}");
                EscribirLog("==================================================\n");

                Console.WriteLine($"Exportadas {filtradas.Count} películas a {salida}");
            }
            catch (Exception ex)
            {
                EscribirLog($"ERROR durante la exportación: {ex.Message}");
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public static void ExportarPorAnio()
        {
            EscribirLog("==================================================");
            EscribirLog("INICIO PROCESO DE EXPORTACIÓN (CSV -> XML) POR AÑO");
            EscribirLog($"Fichero Origen: {Path.GetFullPath(rutaCsv)}");

            try
            {
                string[] lineas = File.ReadAllLines(rutaCsv);

                Console.Write("Introduce el año (se exportarán ese año y superiores): ");
                int anio = int.Parse(Console.ReadLine());

                EscribirLog($"Filtro aplicado -> Año >= {anio}");

                List<string> filtradas = new List<string>();
                for (int i = 1; i < lineas.Length; i++)
                {
                    string[] campos = lineas[i].Split(',');
                    if (campos.Length > 2)
                    {
                        string fecha = campos[2].Trim();
                        int anioPeli;
                        if (int.TryParse(fecha.Substring(0, 4), out anioPeli) && anioPeli >= anio)
                            filtradas.Add(lineas[i]);
                    }
                }

                if (filtradas.Count == 0)
                {
                    EscribirLog($"No se encontraron películas del año {anio} o posteriores. Proceso cancelado.");
                    Console.WriteLine("No se encontraron películas de ese año o posteriores.");
                    return;
                }

                string salida = $"./NetflixOriginals_desde_{anio}.xml";
                EscribirLog($"Fichero Destino: {Path.GetFullPath(salida)}");

                string[] nuevasLineas = new string[filtradas.Count + 1];
                nuevasLineas[0] = lineas[0];
                filtradas.CopyTo(nuevasLineas, 1);

                EscribirXml(salida, nuevasLineas, 1, nuevasLineas.Length - 1);

                EscribirLog("PROCESO FINALIZADO CORRECTAMENTE");
                EscribirLog($"Fichero generado en: {Path.GetFullPath(salida)}");
                EscribirLog("==================================================\n");

                Console.WriteLine($"Exportadas {filtradas.Count} películas a {salida}");
            }
            catch (Exception ex)
            {
                EscribirLog($"ERROR durante la exportación: {ex.Message}");
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public static void ExportarPorLenguaje()
        {
            EscribirLog("==================================================");
            EscribirLog("INICIO PROCESO DE EXPORTACIÓN (CSV -> XML) POR LENGUAJE");
            EscribirLog($"Fichero Origen: {Path.GetFullPath(rutaCsv)}");

            try
            {
                string[] lineas = File.ReadAllLines(rutaCsv);

                HashSet<string> idiomas = new HashSet<string>();
                for (int i = 1; i < lineas.Length; i++)
                {
                    string[] campos = lineas[i].Split(',');
                    if (campos.Length > 5)
                        idiomas.Add(campos[5].Trim());
                }

                Console.WriteLine("Idiomas disponibles:");
                foreach (var idi in idiomas)
                    Console.WriteLine($" - {idi}");

                Console.Write("Introduce el idioma a exportar: ");
                string idiomaElegido = Console.ReadLine().Trim();

                EscribirLog($"Filtro aplicado -> Idioma: '{idiomaElegido}'");

                List<string> filtradas = new List<string>();
                for (int i = 1; i < lineas.Length; i++)
                {
                    string[] campos = lineas[i].Split(',');
                    if (campos.Length > 5 && campos[5].Trim().Equals(idiomaElegido, StringComparison.OrdinalIgnoreCase))
                        filtradas.Add(lineas[i]);
                }

                if (filtradas.Count == 0)
                {
                    EscribirLog($"No se encontraron películas en idioma '{idiomaElegido}'. Proceso cancelado.");
                    Console.WriteLine("No se encontraron películas en ese idioma.");
                    return;
                }

                string salida = $"./NetflixOriginals_{idiomaElegido.Replace(" ", "_")}.xml";
                EscribirLog($"Fichero Destino: {Path.GetFullPath(salida)}");

                string[] nuevasLineas = new string[filtradas.Count + 1];
                nuevasLineas[0] = lineas[0];
                filtradas.CopyTo(nuevasLineas, 1);

                EscribirXml(salida, nuevasLineas, 1, nuevasLineas.Length - 1);

                EscribirLog("PROCESO FINALIZADO CORRECTAMENTE");
                EscribirLog($"Fichero generado en: {Path.GetFullPath(salida)}");
                EscribirLog("==================================================\n");

                Console.WriteLine($"Exportadas {filtradas.Count} películas a {salida}");
            }
            catch (Exception ex)
            {
                EscribirLog($"ERROR durante la exportación: {ex.Message}");
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Escribe un XML a partir de las líneas del CSV (desde 'desde' hasta 'hasta').
        /// Registra en el log cada película procesada.
        /// </summary>
        private static void EscribirXml(string rutaSalida, string[] lineas, int desde, int hasta)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.Encoding = new UTF8Encoding(false);

            int contador = 0;

            using (XmlWriter xmlWriter = XmlWriter.Create(rutaSalida, settings))
            {
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("Peliculas");

                for (int i = desde; i <= hasta; i++)
                {
                    string[] campos = lineas[i].Split(',');
                    if (campos.Length < 6) continue; // línea mal formada

                    // LOG DEL ELEMENTO: datos que lleva
                    EscribirLog($"Procesando Película -> Título: '{campos[0].Trim()}', " +
                                $"Género: '{campos[1].Trim()}', " +
                                $"Premiere: '{campos[2].Trim()}', " +
                                $"Runtime: '{campos[3].Trim()}', " +
                                $"IMDB: '{campos[4].Trim()}', " +
                                $"Idioma: '{campos[5].Trim()}'");
                    contador++;

                    xmlWriter.WriteStartElement("Pelicula");

                    xmlWriter.WriteElementString("Title",       campos[0].Trim());
                    xmlWriter.WriteElementString("Genre",       campos[1].Trim());
                    xmlWriter.WriteElementString("Premiere",    campos[2].Trim());
                    xmlWriter.WriteElementString("Runtime",     campos[3].Trim());
                    xmlWriter.WriteElementString("IMDB_Score",  campos[4].Trim());
                    xmlWriter.WriteElementString("Language",    campos[5].Trim());

                    xmlWriter.WriteEndElement(); // Pelicula
                }

                xmlWriter.WriteEndElement(); // Peliculas
                xmlWriter.WriteEndDocument();
            }

            EscribirLog($"Total de elementos escritos en el XML: {contador}");
        }

        // ============================================================
        //  PARTE 2: XML → CSV
        // ============================================================

        public static void XmlACsvTodo()
        {
            EscribirLog("==================================================");
            EscribirLog("INICIO PROCESO DE EXPORTACIÓN (XML -> CSV) COMPLETO");
            EscribirLog($"Fichero Origen: {Path.GetFullPath(rutaXml)}");

            try
            {
                string salida = "./NetflixOriginals_exportado.csv";
                EscribirLog($"Fichero Destino: {Path.GetFullPath(salida)}");

                string[] lineas = LeerXmlComoLineasCsv(rutaXml, null, null, null);
                File.WriteAllLines(salida, lineas);

                EscribirLog("PROCESO FINALIZADO CORRECTAMENTE");
                EscribirLog($"Fichero generado en: {Path.GetFullPath(salida)}");
                EscribirLog("==================================================\n");

                Console.WriteLine("Exportado XML → CSV completo.");
            }
            catch (Exception ex)
            {
                EscribirLog($"ERROR durante la exportación: {ex.Message}");
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public static void XmlACsvPorGenero()
        {
            EscribirLog("==================================================");
            EscribirLog("INICIO PROCESO DE EXPORTACIÓN (XML -> CSV) POR GÉNERO");
            EscribirLog($"Fichero Origen: {Path.GetFullPath(rutaXml)}");

            try
            {
                string[] lineas = LeerXmlComoLineasCsv(rutaXml, "Genre", null, null);
                string genero = PreguntarFiltro(lineas, "Genre", 1);
                if (genero == null)
                {
                    EscribirLog("Género no válido. Proceso cancelado.");
                    return;
                }

                EscribirLog($"Filtro aplicado -> Género: '{genero}'");

                string[] filtradas = FiltrarPorColumna(lineas, 1, genero);
                string salida = $"./NetflixOriginals_XML2CSV_{genero.Replace(" ", "_")}.csv";
                EscribirLog($"Fichero Destino: {Path.GetFullPath(salida)}");

                File.WriteAllLines(salida, filtradas);

                EscribirLog("PROCESO FINALIZADO CORRECTAMENTE");
                EscribirLog($"Fichero generado en: {Path.GetFullPath(salida)}");
                EscribirLog("==================================================\n");

                Console.WriteLine($"Exportadas {filtradas.Length - 1} películas a {salida}");
            }
            catch (Exception ex)
            {
                EscribirLog($"ERROR durante la exportación: {ex.Message}");
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public static void XmlACsvPorAnio()
        {
            EscribirLog("==================================================");
            EscribirLog("INICIO PROCESO DE EXPORTACIÓN (XML -> CSV) POR AÑO");
            EscribirLog($"Fichero Origen: {Path.GetFullPath(rutaXml)}");

            try
            {
                string[] lineas = LeerXmlComoLineasCsv(rutaXml, "Premiere", null, null);
                Console.Write("Introduce el año (se exportarán ese año y superiores): ");
                int anio = int.Parse(Console.ReadLine());

                EscribirLog($"Filtro aplicado -> Año >= {anio}");

                List<string> filtradas = new List<string> { lineas[0] };
                for (int i = 1; i < lineas.Length; i++)
                {
                    string[] campos = lineas[i].Split(',');
                    if (campos.Length > 2)
                    {
                        int anioPeli;
                        if (int.TryParse(campos[2].Substring(0, 4), out anioPeli) && anioPeli >= anio)
                            filtradas.Add(lineas[i]);
                    }
                }

                string salida = $"./NetflixOriginals_XML2CSV_desde_{anio}.csv";
                EscribirLog($"Fichero Destino: {Path.GetFullPath(salida)}");

                File.WriteAllLines(salida, filtradas);

                EscribirLog("PROCESO FINALIZADO CORRECTAMENTE");
                EscribirLog($"Fichero generado en: {Path.GetFullPath(salida)}");
                EscribirLog("==================================================\n");

                Console.WriteLine($"Exportadas {filtradas.Count - 1} películas a {salida}");
            }
            catch (Exception ex)
            {
                EscribirLog($"ERROR durante la exportación: {ex.Message}");
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public static void XmlACsvPorLenguaje()
        {
            EscribirLog("==================================================");
            EscribirLog("INICIO PROCESO DE EXPORTACIÓN (XML -> CSV) POR LENGUAJE");
            EscribirLog($"Fichero Origen: {Path.GetFullPath(rutaXml)}");

            try
            {
                string[] lineas = LeerXmlComoLineasCsv(rutaXml, "Language", null, null);
                string idioma = PreguntarFiltro(lineas, "Language", 5);
                if (idioma == null)
                {
                    EscribirLog("Idioma no válido. Proceso cancelado.");
                    return;
                }

                EscribirLog($"Filtro aplicado -> Idioma: '{idioma}'");

                string[] filtradas = FiltrarPorColumna(lineas, 5, idioma);
                string salida = $"./NetflixOriginals_XML2CSV_{idioma.Replace(" ", "_")}.csv";
                EscribirLog($"Fichero Destino: {Path.GetFullPath(salida)}");

                File.WriteAllLines(salida, filtradas);

                EscribirLog("PROCESO FINALIZADO CORRECTAMENTE");
                EscribirLog($"Fichero generado en: {Path.GetFullPath(salida)}");
                EscribirLog("==================================================\n");

                Console.WriteLine($"Exportadas {filtradas.Length - 1} películas a {salida}");
            }
            catch (Exception ex)
            {
                EscribirLog($"ERROR durante la exportación: {ex.Message}");
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Lee el XML y lo convierte en un array de líneas CSV (con cabecera).
        /// Registra en el log cada película leída.
        /// </summary>
        private static string[] LeerXmlComoLineasCsv(string rutaXml, string colFiltro, string valFiltro, object _)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(rutaXml);

            List<string> lineas = new List<string>();
            lineas.Add("Title,Genre,Premiere,Runtime,IMDB_Score,Language"); // cabecera

            XmlNodeList peliculas = doc.SelectNodes("//Peliculas/Pelicula");
            foreach (XmlNode p in peliculas)
            {
                string title    = p.SelectSingleNode("Title")?.InnerText    ?? "";
                string genre    = p.SelectSingleNode("Genre")?.InnerText    ?? "";
                string premiere = p.SelectSingleNode("Premiere")?.InnerText ?? "";
                string runtime  = p.SelectSingleNode("Runtime")?.InnerText  ?? "";
                string score    = p.SelectSingleNode("IMDB_Score")?.InnerText ?? "";
                string language = p.SelectSingleNode("Language")?.InnerText ?? "";

                // LOG DEL ELEMENTO leído del XML
                EscribirLog($"Leyendo Película del XML -> Título: '{title}', " +
                            $"Género: '{genre}', " +
                            $"Premiere: '{premiere}', " +
                            $"Runtime: '{runtime}', " +
                            $"IMDB: '{score}', " +
                            $"Idioma: '{language}'");

                lineas.Add($"{title},{genre},{premiere},{runtime},{score},{language}");
            }

            EscribirLog($"Total de elementos leídos del XML: {peliculas.Count}");

            return lineas.ToArray();
        }

        private static string PreguntarFiltro(string[] lineas, string nombreColumna, int indiceColumna)
        {
            HashSet<string> valores = new HashSet<string>();
            for (int i = 1; i < lineas.Length; i++)
            {
                string[] campos = lineas[i].Split(',');
                if (campos.Length > indiceColumna)
                    valores.Add(campos[indiceColumna].Trim());
            }

            Console.WriteLine($"{nombreColumna} disponibles:");
            foreach (var v in valores)
                Console.WriteLine($" - {v}");

            Console.Write($"Introduce {nombreColumna}: ");
            string elegido = Console.ReadLine().Trim();

            if (!valores.Contains(elegido))
            {
                Console.WriteLine("Valor no encontrado.");
                return null;
            }
            return elegido;
        }

        private static string[] FiltrarPorColumna(string[] lineas, int indice, string valor)
        {
            List<string> resultado = new List<string> { lineas[0] };
            for (int i = 1; i < lineas.Length; i++)
            {
                string[] campos = lineas[i].Split(',');
                if (campos.Length > indice && campos[indice].Trim().Equals(valor, StringComparison.OrdinalIgnoreCase))
                    resultado.Add(lineas[i]);
            }
            return resultado.ToArray();
        }

        // ============================================================
        //  CAMBIO DE RUTAS
        // ============================================================

        public static void CambiarRutas()
        {
            Console.Write($"Ruta CSV actual: {rutaCsv}\nNueva ruta CSV (Enter para mantener): ");
            string nuevaCsv = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(nuevaCsv)) rutaCsv = nuevaCsv;

            Console.Write($"Ruta XML actual: {rutaXml}\nNueva ruta XML (Enter para mantener): ");
            string nuevaXml = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(nuevaXml)) rutaXml = nuevaXml;

            Console.WriteLine("Rutas actualizadas.");

            EscribirLog("==================================================");
            EscribirLog("CAMBIO DE RUTAS");
            EscribirLog($"Nueva ruta CSV: {Path.GetFullPath(rutaCsv)}");
            EscribirLog($"Nueva ruta XML: {Path.GetFullPath(rutaXml)}");
            EscribirLog("==================================================\n");
        }
    }
}