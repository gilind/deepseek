// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlCeConnectionStringHelper.cs" company="ООО Рубиус">
//   Все права защищены (с) 2010-2015
// </copyright>
// <summary>
//   Вспомогательный класс для работы со строкой подключения к Sql Server CE
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Ru.DataLayer.Catalog
{
    using System;
    using System.ComponentModel;
    using System.Data.Common;

    /// <summary>
    /// Вспомогательный класс для работы со строкой подключения к Sql Server CE
    /// </summary>
    public class SqlCeConnectionStringHelper : DbConnectionStringBuilder
    {
        /// <summary>
        /// Название параметра DataSource
        /// </summary>
        const string DataSourceString = "Data Source";

        /// <summary>
        /// Название параметра Password
        /// </summary>
        const string PasswordString = "Password";

        /// <summary>
        /// Название параметра Max Database Size
        /// </summary>
        const string MaxDatabaseSizeString = "Max Database Size";

        private string _dataSource;

        private string _password;

        private string _maxDatabaseSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlCeConnectionStringHelper"/> class.
        /// </summary>
        public SqlCeConnectionStringHelper()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlCeConnectionStringHelper"/> class.
        /// </summary>
        /// <param name="connectionString">
        /// The connection string.
        /// </param>
        public SqlCeConnectionStringHelper(string connectionString)
        {
            if (!string.IsNullOrEmpty(connectionString))
            {
                var parametersStrings = connectionString.Split(';');

                foreach (var parameterString in parametersStrings)
                {
                    var parameterWords = parameterString.Split('=');

                    if (parameterWords.Length == 2)
                    {
                        string keyword = parameterWords[0];
                        object value = parameterWords[1];
                        this[keyword] = value;
                    }
                }
            }
            
            BrowsableConnectionString = false;
        }

        [DisplayName("Data Source")]
        [RefreshProperties(RefreshProperties.All)]
        public string DataSource
        {
            get
            {
                return _dataSource;
            }

            set
            {
                SetValue(DataSourceString, value);
                _dataSource = value;
            }
        }

        [DisplayName("Password")]
        [RefreshProperties(RefreshProperties.All)]
        public string Password
        {
            get
            {
                return _password;
            }

            set
            {
                SetValue(PasswordString, value);
                _password = value;
            }
        }

        [DisplayName("MaxDatabaseSize")]
        [RefreshProperties(RefreshProperties.All)]
        public string MaxDatabaseSize
        {
            get
            {
                return _maxDatabaseSize;
            }

            set
            {
                SetValue(MaxDatabaseSizeString, value);
                _maxDatabaseSize = value;
            }
        }

        private void SetValue(string keyword, string value)
        {
            CheckArgumentNull(value, keyword);
            base[keyword] = value;
        }

        internal static void CheckArgumentNull(object value, string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        public override object this[string keyword]
        {
            get
            {
                if (keyword == DataSourceString)
                {
                    return DataSource;
                }

                if (keyword == PasswordString)
                {
                    return Password;
                }

                if (keyword == MaxDatabaseSizeString)
                {
                    return MaxDatabaseSize;
                }

                return string.Empty;
            }

            set 
            {
                if (keyword == DataSourceString)
                {
                    DataSource = value as string;
                }

                if (keyword == PasswordString)
                {
                    Password = value as string;
                }

                if (keyword == MaxDatabaseSizeString)
                {
                    MaxDatabaseSize = value as string;
                }
            }
        }
    }
}