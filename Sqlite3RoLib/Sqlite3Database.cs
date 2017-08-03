﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sqlite3RoLib.Objects;
using Sqlite3RoLib.Objects.Headers;
using Sqlite3RoLib.Tables;

namespace Sqlite3RoLib
{
    public class Sqlite3Database : IDisposable
    {
        private readonly Sqlite3Settings _settings;
        private readonly ReaderBase _reader;

        private uint _sizeInPages;

        public DatabaseHeader Header { get; private set; }
        private Sqlite3MasterTable _masterTable;

        public Sqlite3Database(Stream file, Sqlite3Settings settings = null)
        {
            _settings = settings ?? new Sqlite3Settings();
            _reader = new ReaderBase(file);

            Initialize();
            InitializeMasterTable();
        }

        private void Initialize()
        {
            Header = DatabaseHeader.Parse(_reader);

            // Database Size in pages adjustment
            // https://www.sqlite.org/fileformat.html#in_header_database_size

            uint expectedPages = (uint)(_reader.Length / Header.PageSize);

            // TODO: Warn on mismatch
            _sizeInPages = Math.Max(expectedPages, Header.DatabaseSizeInPages);

            _reader.ApplySqliteDatabaseHeader(Header);
        }

        private void InitializeMasterTable()
        {
            // Parse table on Page 1, the sqlite_master table
            BTreePage rootBtree = BTreePage.Parse(_reader, 1);

            Sqlite3Table table = new Sqlite3Table(_reader, rootBtree);
            _masterTable = new Sqlite3MasterTable(table);
        }

        public Sqlite3Table GetTable(string name)
        {
            IEnumerable<Sqlite3SchemaRow> tables = GetTables();

            foreach (Sqlite3SchemaRow table in tables)
            {
                if (table.TableName == name && table.Type == "table")
                {
                    // Found it
                    BTreePage root = BTreePage.Parse(_reader, table.RootPage);
                    Sqlite3Table tbl = new Sqlite3Table(_reader, root);
                    return tbl;
                }
            }

            throw new Exception("Unable to find table named " + name);
        }

        public IEnumerable<Sqlite3SchemaRow> GetTables() => _masterTable.Tables;

        public void Dispose()
        {
            _reader?.Dispose();
        }
    }
}
