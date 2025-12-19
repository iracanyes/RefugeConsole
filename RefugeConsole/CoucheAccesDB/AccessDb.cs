using Microsoft.Extensions.Logging;
using Npgsql;
using RefugeConsole.ClassesMetiers.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace RefugeConsole.CoucheAccesDB
{
    internal class AccessDb
    {
        private static readonly ILogger MyLogger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger(nameof(AccessDb));
        protected readonly NpgsqlConnection SqlConn;

        public AccessDb() {
            try
            {
                SqlConn = new NpgsqlConnection(Environment.GetEnvironmentVariable("REFUGE_DB_CONNECTION_STRING"));
                SqlConn.Open();

            }
            catch (Exception ex)
            {
                MyLogger.LogError("Error while connecting to database. Reason : {0}", ex.Message);
                throw new AccessDbException(ex.Message, "Error while connecting to database");
            }
        }
    }
}
