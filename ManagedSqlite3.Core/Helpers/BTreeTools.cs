﻿using System;
using System.Collections.Generic;
using ManagedSqlite3.Core.Objects;

namespace ManagedSqlite3.Core.Helpers
{
    internal static class BTreeTools
    {
        public static byte[] ReadCellData(ReaderBase reader, BTreeCellData data)
        {
            reader.SeekPage(data.Page, data.CellOffset);
            reader.SkipVarInt();
            reader.SkipVarInt();

            // Read data
            if (data.Cell.FirstOverflowPage > 0)
                throw new NotImplementedException("We don't support overflow yet");

            byte[] bytes = reader.Read(data.Cell.DataSizeInCell);

            return bytes;
        }

        public static IEnumerable<BTreeCellData> WalkTableBTree(BTreePage node)
        {
            BTreeInteriorTablePage asInterior = node as BTreeInteriorTablePage;

            if (asInterior != null)
                return WalkTableBTree(asInterior);

            BTreeLeafTablePage asLeaf = node as BTreeLeafTablePage;

            if (asLeaf != null)
                return WalkTableBTree(asLeaf);

            throw new ArgumentException("Did not receive a compatible BTreePage", nameof(node));
        }

        private static IEnumerable<BTreeCellData> WalkTableBTree(BTreeInteriorTablePage interior)
        {
            // Walk sub-pages and yield their data
            foreach (BTreeInteriorTablePage.Cell cell in interior.Cells)
            {
                BTreePage subPage = BTreePage.Parse(interior.Reader, cell.LeftPagePointer);

                foreach (BTreeCellData data in WalkTableBTree(subPage))
                    yield return data;
            }

            if (interior.Header.RightMostPointer > 0)
            {
                // Process sibling page
                BTreePage subPage = BTreePage.Parse(interior.Reader, interior.Header.RightMostPointer);

                foreach (BTreeCellData data in WalkTableBTree(subPage))
                    yield return data;
            }
        }

        private static IEnumerable<BTreeCellData> WalkTableBTree(BTreeLeafTablePage leaf)
        {
            // Walk cells and yield their data
            for (var i = 0; i < leaf.Cells.Length; i++)
            {
                BTreeLeafTablePage.Cell cell = leaf.Cells[i];

                BTreeCellData res = new BTreeCellData();

                res.Cell = cell;
                res.CellOffset = leaf.CellOffsets[i];
                res.Page = leaf.Page;

                yield return res;
            }
        }
    }
}