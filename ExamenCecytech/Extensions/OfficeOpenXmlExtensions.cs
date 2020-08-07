using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamenCecytech.Extensions
{
    public class OfficeOpenXmlExtensions
    {
        public static void CrearHojaConLista<T>(ref ExcelPackage package, IEnumerable<T> lista, string nombreHoja)
        {
            var ws = package.Workbook.Worksheets.Add(nombreHoja);

            var propsT = typeof(T).GetProperties();
            for (int i = 0; i < propsT.Count(); i++)
            {
                ws.Cells[1, i + 1].Value = propsT[i].Name;
            }

            int r = 2;
            foreach (var item in lista)
            {
                for (int i = 0; i < propsT.Count(); i++)
                {
                    ws.Cells[r, i + 1].Value = item.GetType().GetProperty(propsT[i].Name).GetValue(item);
                }
                r++;
            }
        }
    }
}
