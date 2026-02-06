// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MigSharpExtensions.cs" company="ООО Рубиус">
//   Все права защищены (с) 2010-2015
// </copyright>
// <summary>
//   Вспомогательные методля для работы MigSharp
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Ru.DataLayer.Catalog.Migrations
{
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;

    using MigSharp;

    /// <summary>
    /// Вспомогательные методля для работы MigSharp
    /// </summary>
    public static class MigSharpExtensions
    {
        /// <summary>
        /// Разделитель запросов
        /// </summary>
        private const string QuerySplitter = "\nGO|GO\n";

        /// <summary>
        /// The execute embedded.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        /// <param name="createDbStructureSql">
        /// The create db structure sql.
        /// </param>
        public static void ExecuteEmbedded(this IDatabase db, string createDbStructureSql)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var embeddedResources = assembly.GetManifestResourceNames();
            var resourceName = embeddedResources.FirstOrDefault(x => x.EndsWith(createDbStructureSql));

            string script = null;
            if (resourceName != null)
            {
                using (var resourceStream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (resourceStream != null)
                    {
                        var streamReader = new StreamReader(resourceStream);
                        script = streamReader.ReadToEnd();
                    }
                }
            }

            SpliteAndExecute(db, script);
        }

        /// <summary>
        /// Разделяет sql-скрипт на отдельные запросы по пустым строкам и выполняет их
        /// </summary>
        /// <param name="db">
        /// Информатция о базе данных
        /// </param>
        /// <param name="script">
        /// Содеражние sql-скрипта
        /// </param>
        private static void SpliteAndExecute(IDatabase db, string script)
        {
            var queries = Regex.Split(script, QuerySplitter, RegexOptions.IgnoreCase).Where(x => x != null && !string.IsNullOrEmpty(x.Trim()));

            foreach (var query in queries)
            {
                db.Execute(query);
            }
        }
    }
}