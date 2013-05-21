using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace smsSurvery.Surveryer.ReportHelpers 
{
   public class RepDataColumn
   {
      public string id;
      public string label;
      public string type;

      public RepDataColumn(string iId, string iType, string iLabel = "")
      {
         id = iId;
         type = iType;
         label = iLabel;
      }

   }

   public class RepDataRowCell
   {
      public object v;
      public string f;

      public RepDataRowCell(object iV, string iF = "")
      {
         v = iV;
         f = iF;
      }

   }

   public class RepDataRow
   {
      /* 
          * the reason of using an IEnumerable structure and not a pair 
          * is because the json generated is a general json which fits 
          * a large number of charts, one of them with multiple dimensions. 
          * Also this structure is intended primarly to be displayed in tables with many columns.
       */
      public IEnumerable<RepDataRowCell> c;

      public RepDataRow(IEnumerable<RepDataRowCell> iC)
      {
         c = iC;
      }

   }

   public class ChartValue
   {
      public int value { get; set; }
      public string description { get; set; }
      public bool changed { get; set; }

      public ChartValue(int iValue, string iDescription, bool iChanged = false)
      {
         value = iValue;
         description = iDescription;
         changed = iChanged;
      }
   }

   public class RepChartData
   {
      public IEnumerable<RepDataColumn> cols;
      public IEnumerable<RepDataRow> rows;

      public RepChartData(IEnumerable<RepDataColumn> iCols, IEnumerable<RepDataRow> iRows)
      {
         cols = iCols;
         rows = iRows;
      }

   }    
}
