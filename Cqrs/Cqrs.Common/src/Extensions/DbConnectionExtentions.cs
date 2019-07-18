using System;
using System.Collections.Generic;
using System.Linq;

using Cqrs;


namespace System.Data
{
    public static class DbConnectionExtentions
    {
        private static void AttachParameters(IDbCommand command, IEnumerable<IDbDataParameter> commandParameters)
        {
            if (commandParameters.IsEmpty())
                return;

            foreach (IDataParameter parame in commandParameters) {
                if (parame == null)
                    continue;

                if ((parame.Direction == ParameterDirection.InputOutput || parame.Direction == ParameterDirection.Input) && (parame.Value == null)) {
                    parame.Value = DBNull.Value;
                }
                command.Parameters.Add(parame);
            }
        }
        private static IDbCommand CreateCommandByCommandType(IDbConnection connection, IDbTransaction transaction, CommandType commandType, string commandText, IEnumerable<IDbDataParameter> commandParameters)
        {
            IDbCommand command = connection.CreateCommand();
            command.CommandType = commandType;
            command.CommandText = commandText;
            command.Connection = connection;
            if (transaction != null) {
                command.Transaction = transaction;
            }

            if (commandParameters != null) {
                AttachParameters(command, commandParameters);
            }

            return command;
        }

        public static int ExecuteNonQuery(this IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            return connection.ExecuteNonQuery(commandType, commandText, (IEnumerable<IDbDataParameter>)commandParameters);
        }
        public static int ExecuteNonQuery(this IDbConnection connection, CommandType commandType, string commandText, IEnumerable<IDbDataParameter> commandParameters)
        {
            Ensure.NotNullOrWhiteSpace(commandText, "commandText");

            using (IDbCommand command = CreateCommandByCommandType(connection, null, commandType, commandText, commandParameters)) {
                try {
                    return command.ExecuteNonQuery();
                }
                catch (Exception ex) {
                    throw ex;
                }
                finally {
                    command.Parameters.Clear();
                }
            }
        }

        public static int ExecuteNonQuery(this IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            return transaction.ExecuteNonQuery(commandType, commandText, (IEnumerable<IDbDataParameter>)commandParameters);
        }

        public static int ExecuteNonQuery(this IDbTransaction transaction, CommandType commandType, string commandText, IEnumerable<IDbDataParameter> commandParameters)
        {
            Ensure.NotNullOrWhiteSpace(commandText, "commandText");

            using (IDbCommand command = CreateCommandByCommandType(transaction.Connection, transaction, commandType, commandText, commandParameters)) {
                try {
                    return command.ExecuteNonQuery();
                }
                catch (Exception ex) {
                    throw ex;
                }
                finally {
                    command.Parameters.Clear();
                }
            }
        }

        public static IDataReader ExecuteReader(this IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            return connection.ExecuteReader(commandType, commandText, (IEnumerable<IDbDataParameter>)commandParameters);
        }

        public static IDataReader ExecuteReader(this IDbConnection connection, CommandType commandType, string commandText, IEnumerable<IDbDataParameter> commandParameters)
        {            
            Ensure.NotNullOrWhiteSpace(commandText, "commandText");

            using (IDbCommand command = CreateCommandByCommandType(connection, null, commandType, commandText, commandParameters)) {
                try {
                    return command.ExecuteReader();
                }
                catch (Exception ex) {
                    throw ex;
                }
                finally {
                    bool canClear = true;
                    foreach (IDataParameter commandParameter in command.Parameters) {
                        if (commandParameter.Direction != ParameterDirection.Input)
                            canClear = false;
                    }
                    if (canClear) {
                        command.Parameters.Clear();
                    }
                }
            }
        }

        public static IDataReader ExecuteReader(this IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            return transaction.ExecuteReader(commandType, commandText, (IEnumerable<IDbDataParameter>)commandParameters);
        }

        public static IDataReader ExecuteReader(this IDbTransaction transaction, CommandType commandType, string commandText, IEnumerable<IDbDataParameter> commandParameters)
        {            
            Ensure.NotNullOrWhiteSpace(commandText, "commandText");

            using (IDbCommand command = CreateCommandByCommandType(transaction.Connection, transaction, commandType, commandText, commandParameters)) {
                try {
                    return command.ExecuteReader();
                }
                catch (Exception ex) {
                    throw ex;
                }
                finally {
                    bool canClear = true;
                    foreach (IDataParameter commandParameter in command.Parameters) {
                        if (commandParameter.Direction != ParameterDirection.Input)
                            canClear = false;
                    }
                    if (canClear) {
                        command.Parameters.Clear();
                    }
                }
            }
        }

        public static object ExecuteScalar(this IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            return connection.ExecuteScalar(commandType, commandText, (IEnumerable<IDbDataParameter>)commandParameters);
        }

        public static object ExecuteScalar(this IDbConnection connection, CommandType commandType, string commandText, IEnumerable<IDbDataParameter> commandParameters)
        {            
            Ensure.NotNullOrWhiteSpace(commandText, "commandText");

            using (IDbCommand command = CreateCommandByCommandType(connection, null, commandType, commandText, commandParameters)) {
                try {
                    return command.ExecuteScalar();
                }
                catch (Exception ex) {
                    throw ex;
                }
                finally {
                    command.Parameters.Clear();
                }
            }
        }

        public static object ExecuteScalar(this IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            return transaction.ExecuteScalar(commandType, commandText, (IEnumerable<IDbDataParameter>)commandParameters);
        }

        public static object ExecuteScalar(this IDbTransaction transaction, CommandType commandType, string commandText, IEnumerable<IDbDataParameter> commandParameters)
        {
            Ensure.NotNullOrWhiteSpace(commandText, "commandText");

            using (IDbCommand command = CreateCommandByCommandType(transaction.Connection, transaction, commandType, commandText, commandParameters)) {
                try {
                    return command.ExecuteScalar();
                }
                catch (Exception ex) {
                    throw ex;
                }
                finally {
                    command.Parameters.Clear();
                }
            }
        }

    }
}
