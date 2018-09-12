﻿using System;
using MBW.Utilities.ManagedSqlite.Core.Tables;

namespace MBW.Utilities.ManagedSqlite.Sql
{
    public static class SqlExtensions
    {
        public static SqlTableDefinition GetTableDefinition(this Sqlite3SchemaRow schema)
        {
            SqlTableDefinition def = SqlCache.GetOrAdd(schema);

            return def;
        }

        public static SqlTableDefinition GetTableDefinition(this Sqlite3Table table)
        {
            return table.SchemaDefinition.GetTableDefinition();
        }

        public static bool TryGetValueByName(this Sqlite3Row row, string columnName, out object value)
        {
            value = null;

            SqlTableDefinition tableDefinition = row.Table.GetTableDefinition();

            if (!tableDefinition.TryGetColumn(columnName, out SqlTableColumn column))
                return false;

            if (row.ColumnData.Length <= column.Ordinal)
                return false;

            // Special case for row-id
            if (tableDefinition.RowIdColumn != null && columnName.Equals(tableDefinition.RowIdColumn.Name, StringComparison.OrdinalIgnoreCase))
            {
                // Return the row-id
                value = row.RowId;
                return true;
            }

            value = row.ColumnData[column.Ordinal];
            return true;
        }
    }
}