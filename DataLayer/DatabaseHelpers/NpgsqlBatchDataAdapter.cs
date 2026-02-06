namespace Ru.DataLayer.DatabaseHelpers
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Text;

    using Npgsql;
    //using System.Threading;
    //using System.Threading.Tasks;

    /// <summary>
    /// Represents the method that handles the <see cref="NpgsqlBatchDataAdapter.RowUpdated"/> events.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="NpgsqlRowUpdatedEventArgs"/> that contains the event data.</param>
    public delegate void NpgsqlRowUpdatedEventHandler(object sender, NpgsqlRowUpdatedEventArgs e);

    /// <summary>
    /// Represents the method that handles the <see cref="NpgsqlBatchDataAdapter.RowUpdating"/> events.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="NpgsqlRowUpdatingEventArgs"/> that contains the event data.</param>
    public delegate void NpgsqlRowUpdatingEventHandler(object sender, NpgsqlRowUpdatingEventArgs e);

    /// <summary>
    /// This class represents an adapter from many commands: select, update, insert and delete to fill a <see cref="System.Data.DataSet"/>.
    /// Модфикация стандартного класса NpgsqlDataAdapter с поддержкой UpdateBatchSize что бы выполнять команды пакетами
    /// </summary>
    [System.ComponentModel.DesignerCategory("")]
    public sealed class NpgsqlBatchDataAdapter : DbDataAdapter
    {
        /// <summary>
        /// Row updated event.
        /// </summary>
        public event NpgsqlRowUpdatedEventHandler RowUpdated;

        /// <summary>
        /// Row updating event.
        /// </summary>
        public event NpgsqlRowUpdatingEventHandler RowUpdating;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public NpgsqlBatchDataAdapter() {}

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="selectCommand"></param>
        public NpgsqlBatchDataAdapter(NpgsqlCommand selectCommand)
        {
            SelectCommand = selectCommand;
            UpdateBatchSize = 1;
        }

        public override int UpdateBatchSize { get; set; }

        private NpgsqlCommand _batchCommand;
        private int _batchCount;
        private Dictionary< string, List< NpgsqlParameter > > _batch;


        protected override void InitializeBatching()
        {
            _batch = new Dictionary< string, List< NpgsqlParameter > >(UpdateBatchSize);
        }

        protected override int AddToBatch( IDbCommand command )
        {
            _batchCount++;
            var commandText = command.CommandText;
            var paramList = new List<NpgsqlParameter>(command.Parameters.Count);

            foreach ( NpgsqlParameter parameter in ((NpgsqlCommand)command).Parameters )
            {
                var clone = parameter.Clone();
                    paramList.Add( clone );

                clone.ParameterName = $"{clone.ParameterName}_{_batchCount}";

                //замена первого подходящего параметра
                var index = commandText.IndexOf(parameter.ParameterName, StringComparison.Ordinal);
                if ( index > -1 )
                {
                    commandText = commandText.Substring( 0, index ) + clone.ParameterName
                                                                    + commandText.Substring( index + parameter.ParameterName.Length );
                }
            }
            _batch.Add( commandText, paramList  );
            return _batchCount;
        }

        protected override int ExecuteBatch()
        {
            using ( _batchCommand = new NpgsqlCommand() )
            {
                var stringBuilder = new StringBuilder();
                foreach ( var kvp in _batch )
                {
                    stringBuilder.AppendLine( kvp.Key + ';' );
                    foreach ( var parameter in kvp.Value )
                    {
                        _batchCommand.Parameters.Add( parameter );
                    }
                }

                _batchCommand.Connection = SelectCommand.Connection;
                _batchCommand.CommandTimeout = SelectCommand.CommandTimeout;
                _batchCommand.Transaction = SelectCommand.Transaction;
                _batchCommand.CommandText = stringBuilder.ToString();
                return _batchCommand.ExecuteNonQuery();
            }
        }

        protected override void TerminateBatching()
        {
            
        }

        protected override void ClearBatch()
        {
            _batch.Clear();
        }

        protected override bool GetBatchedRecordsAffected( int commandIdentifier, out int recordsAffected, out Exception error )
        {
            error = null;
            try
            {
                //recordsAffected = (int) _batchCommand.Statements[ commandIdentifier - 1 ].Rows;
                recordsAffected = 1;
            }
            catch ( Exception exception )
            {
                error = exception;
                recordsAffected = 0;
            }

            return true;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="selectCommandText"></param>
        /// <param name="selectConnection"></param>
        public NpgsqlBatchDataAdapter(string selectCommandText, NpgsqlConnection selectConnection)
            : this(new NpgsqlCommand(selectCommandText, selectConnection)) {}

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="selectCommandText"></param>
        /// <param name="selectConnectionString"></param>
        public NpgsqlBatchDataAdapter(string selectCommandText, string selectConnectionString)
            : this(selectCommandText, new NpgsqlConnection(selectConnectionString)) {}

        /// <summary>
        /// Create row updated event.
        /// </summary>
        protected override RowUpdatedEventArgs CreateRowUpdatedEvent(DataRow dataRow, IDbCommand command,
                                                                     System.Data.StatementType statementType,
                                                                     DataTableMapping tableMapping)
        {
            return new NpgsqlRowUpdatedEventArgs( dataRow, command, statementType, tableMapping );
        }

        /// <summary>
        /// Create row updating event.
        /// </summary>
        protected override RowUpdatingEventArgs CreateRowUpdatingEvent(DataRow dataRow, IDbCommand command,
                                                                       System.Data.StatementType statementType,
                                                                       DataTableMapping tableMapping)
        {
            return new NpgsqlRowUpdatingEventArgs( dataRow, command, statementType, tableMapping );
        }

        /// <summary>
        /// Raise the RowUpdated event.
        /// </summary>
        /// <param name="value"></param>
        protected override void OnRowUpdated(RowUpdatedEventArgs value)
        {
            //base.OnRowUpdated(value);
            var args = value as NpgsqlRowUpdatedEventArgs;
            if (args != null)
            {
                RowUpdated?.Invoke( this, args );
            }

            //if (RowUpdated != null && value is NpgsqlRowUpdatedEventArgs args)
            //    RowUpdated(this, args);
        }

        /// <summary>
        /// Raise the RowUpdating event.
        /// </summary>
        /// <param name="value"></param>
        protected override void OnRowUpdating(RowUpdatingEventArgs value)
        {
            var args = value as NpgsqlRowUpdatingEventArgs;
            if (args != null)
                RowUpdating?.Invoke(this, args);
        }

        /// <summary>
        /// Delete command.
        /// </summary>
        public new NpgsqlCommand DeleteCommand
        {
            get
            {
                return (NpgsqlCommand)base.DeleteCommand;
            }
            set
            {
                base.DeleteCommand = value;
            }
        }

        /// <summary>
        /// Select command.
        /// </summary>
        public new NpgsqlCommand SelectCommand
        {
            get
            {
                return (NpgsqlCommand)base.SelectCommand;
            }
            set
            {
                base.SelectCommand = value;
            }
        }

        /// <summary>
        /// Update command.
        /// </summary>
        public new NpgsqlCommand UpdateCommand
        {
            get
            {
                return (NpgsqlCommand)base.UpdateCommand;
            }
            set
            {
                base.UpdateCommand = value;
            }
        }

        /// <summary>
        /// Insert command.
        /// </summary>
        public new NpgsqlCommand InsertCommand
        {
            get
            {
                return (NpgsqlCommand)base.InsertCommand;
            }
            set
            {
                base.InsertCommand = value;
            }
        }

        // Temporary implementation, waiting for official support in System.Data via https://github.com/dotnet/runtime/issues/22109
        //internal async Task<int> Fill(DataTable dataTable, bool async, CancellationToken cancellationToken = default(CancellationToken))
        //{
        //    var command = SelectCommand;
        //    var activeConnection = command.Connection;
        //    var originalState = ConnectionState.Closed;

        //    try
        //    {
        //        originalState = activeConnection.State;
        //        if (ConnectionState.Closed == originalState)
        //            await activeConnection.OpenAsync(cancellationToken);

        //        var dataReader = await command.ExecuteReaderAsync(CommandBehavior.Default, cancellationToken);
        //        try
        //        {
        //            return await Fill(dataTable, dataReader, async, cancellationToken);
        //        }
        //        finally
        //        {
        //            if (async)
        //                await dataReader.DisposeAsync();
        //            else
        //                dataReader.Dispose();
        //        }
        //    }
        //    finally
        //    {
        //        if (ConnectionState.Closed == originalState)
        //            activeConnection.Close();
        //    }
        //}

        //async Task<int> Fill(DataTable dataTable, NpgsqlDataReader dataReader, bool async, CancellationToken cancellationToken = default(CancellationToken))
        //{
        //    dataTable.BeginLoadData();
        //    try
        //    {
        //        var rowsAdded = 0;
        //        var count = dataReader.FieldCount;
        //        var columnCollection = dataTable.Columns;
        //        for (var i = 0; i < count; ++i)
        //        {
        //            var fieldName = dataReader.GetName(i);
        //            if (!columnCollection.Contains(fieldName))
        //            {
        //                var fieldType = dataReader.GetFieldType(i);
        //                if ( fieldType != null )
        //                {
        //                    var dataColumn = new DataColumn(fieldName, fieldType);
        //                    columnCollection.Add(dataColumn);
        //                }
        //            }
        //        }

        //        var values = new object[count];

        //        while (async ? await dataReader.ReadAsync(cancellationToken) : dataReader.Read())
        //        {
        //            dataReader.GetValues(values);
        //            dataTable.LoadDataRow(values, true);
        //            rowsAdded++;
        //        }
        //        return rowsAdded;
        //    }
        //    finally
        //    {
        //        dataTable.EndLoadData();
        //    }
        //}
    }

#pragma warning disable 1591

    public class NpgsqlRowUpdatingEventArgs : RowUpdatingEventArgs
    {
        public NpgsqlRowUpdatingEventArgs(DataRow dataRow, IDbCommand command, System.Data.StatementType statementType,
                                          DataTableMapping tableMapping)
            : base(dataRow, command, statementType, tableMapping) {}
    }

    public class NpgsqlRowUpdatedEventArgs : RowUpdatedEventArgs
    {
        public NpgsqlRowUpdatedEventArgs(DataRow dataRow, IDbCommand command, System.Data.StatementType statementType,
                                         DataTableMapping tableMapping)
            : base(dataRow, command, statementType, tableMapping) {}
    }

#pragma warning restore 1591
}