﻿using System.Diagnostics;

namespace ManagedSqlite3.Core.Objects
{
    [DebuggerDisplay("Page {Page}, Size {Cell.DataSizeInCell} / {Cell.DataSize}")]
    internal class BTreeCellData
    {
        public uint Page { get; set; }

        public BTreeLeafTablePage.Cell Cell { get; set; }

        public ushort CellOffset { get; set; }
    }
}