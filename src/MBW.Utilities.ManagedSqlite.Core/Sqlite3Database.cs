﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using MBW.Utilities.ManagedSqlite.Core.Internal;
using MBW.Utilities.ManagedSqlite.Core.Internal.Objects.Headers;
using MBW.Utilities.ManagedSqlite.Core.Tables;

namespace MBW.Utilities.ManagedSqlite.Core;

public class Sqlite3Database
{
    private readonly ReaderBase _reader;
    private readonly DatabaseHeader _header;
    private readonly Dictionary<string, Sqlite3Table> _tables;

    public Sqlite3Database(Stream file)
    {
        _reader = new ReaderBase(file);
        _header = new DatabaseHeader(_reader);

        // Database Size in pages adjustment
        // https://www.sqlite.org/fileformat.html#in_header_database_size
        _reader.ApplySqliteDatabaseHeader(_header);

        // Fake the schema for the sqlite_master table
        Sqlite3Table masterTable = new Sqlite3Table(_reader, 1, "sqlite_master", "sqlite_master");

        _tables = new Dictionary<string, Sqlite3Table>();
        foreach (Sqlite3Row row in masterTable.EnumerateRows())
        {
            string? type = row.TryGetOrdinal(0, out object? objType) ? (string)objType : null;

            //We only store tables
            if (type is not "table")
                continue;

            string? name = row.TryGetOrdinal(1, out object? objName) ? (string)objName : null;
            string? tableName = row.TryGetOrdinal(2, out object? objTableName) ? (string)objTableName : null;
            uint rootPage = row.TryGetOrdinal(3, out object? objRootPage) ? (uint)(long)objRootPage : 0u;

            _tables.Add(name, new Sqlite3Table(_reader, rootPage, name, tableName));
        }
    }

    public bool TryGetTable(string name, [NotNullWhen(true)]out Sqlite3Table? table) => _tables.TryGetValue(name, out table);

    public IEnumerable<Sqlite3Table> Tables => _tables.Values;
}