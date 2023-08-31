using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Xml;
using NPOI.SS.Converter;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using NS_Education.Models.APIItems;
using NS_Education.Models.APIItems.Controller.PrintReport.Report11;
using NS_Education.Models.Entities;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;

namespace NS_Education.Controller.UsingHelper.PrintReportController
{
    /// <summary>
    /// 場地庫存狀況表的處理。
    /// </summary>
    public class Report11Controller : PublicClass,
        IPrintReport<Report11_Input_APIItem, Report11_Output_Row_APIItem>
    {
        /// <inheritdoc />
        public async Task<CommonResponseForPagedList<Report11_Output_Row_APIItem>> GetResultAsync(
            Report11_Input_APIItem input)
        {
            using (NsDbContext dbContext = new NsDbContext())
            {
                // 基於 Resver_Site，查詢範圍內的場地預約資料

                string tableName = dbContext.GetTableName<Resver_Site>();
                DateTime startTime = input.StartDate?.ParseDateTime().Date ?? SqlDateTime.MinValue.Value;
                DateTime endTime = input.EndDate?.ParseDateTime().Date ?? SqlDateTime.MaxValue.Value;

                var query = dbContext.Resver_Site
                    .AsNoTracking()
                    .Include(rs => rs.B_SiteData)
                    .Include(rs => rs.Resver_Head)
                    .Where(rs => !rs.DeleteFlag)
                    .Where(rs => !rs.Resver_Head.DeleteFlag)
                    .Where(rs => startTime <= rs.TargetDate && rs.TargetDate <= endTime)
                    .GroupJoin(dbContext.M_Resver_TimeSpan
                            .Include(rts => rts.D_TimeSpan)
                            .Where(rts => rts.TargetTable == tableName),
                        rs => rs.RSID,
                        rts => rts.TargetID,
                        (rs, rts) => new { rs, rts }
                    )
                    .SelectMany(e => e.rts.DefaultIfEmpty(), (e, rts) => new { e.rs, rts })
                    .AsQueryable();

                var results = await query
                    .OrderBy(e => e.rs.RSID)
                    .ToArrayAsync();

                var siteData = await dbContext.B_SiteData
                    .Include(sd => sd.B_Category)
                    .Where(sd => sd.ActiveFlag && !sd.DeleteFlag)
                    .OrderBy(sd => sd.BSID)
                    .ToArrayAsync();

                var timeSpans = await dbContext.D_TimeSpan
                    .Where(dts => dts.ActiveFlag && !dts.DeleteFlag)
                    .ToArrayAsync();

                Report11_Output_APIItem response = new Report11_Output_APIItem();
                response.SetByInput(input);

                // 欄位：
                // Type
                // SiteName
                // Time
                // yyyy-MM-dd
                // yyyy-MM-dd
                foreach (B_SiteData sd in siteData)
                {
                    foreach (D_TimeSpan dts in timeSpans)
                    {
                        Report11_Output_Row_APIItem newRow = new Report11_Output_Row_APIItem();
                        response.Items.Add(newRow);

                        newRow.Type = sd.B_Category.TitleC;
                        newRow.SiteName = sd.Title;
                        newRow.Time = dts.Title;

                        foreach (DateTime dt in startTime.Range(endTime))
                        {
                            newRow.DateToCustomer.Add(dt.Date,
                                results
                                    .Where(g => g.rs.TargetDate.Date == dt.Date)
                                    .Select(g => g.rs.Resver_Head.CustomerTitle)
                                    .FirstOrDefault());
                        }
                    }
                }

                response.UID = GetUid();
                response.Username = await GetUserNameByID(response.UID);
                response.AllItemCt = response.Items.Count;

                response.Items = response.Items.Skip(input.GetStartIndex()).Take(input.GetTakeRowCount()).ToList();

                return response;
            }
        }

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.PrintFlag)]
        public async Task<string> Get(Report11_Input_APIItem input)
        {
            CommonResponseForPagedList<Report11_Output_Row_APIItem> response = await GetResultAsync(input);

            response.Items = response.Items
                    .OrderBy(i => i.Type)
                    .ThenBy(i => i.SiteName)
                    .ThenBy(i => i.Type)
                    .ToList()
                ;
            IWorkbook book = new XSSFWorkbook();
            ISheet sheet = book.CreateSheet();

            // 1. 建立 Header

            IRow header = sheet.CreateRow(0);
            header.CreateCell(0).SetCellValue("場地類別");
            header.CreateCell(1).SetCellValue("場地名稱");
            header.CreateCell(2).SetCellValue("時段");

            var headerItem = response.Items.First();
            for (var i = 0; i < headerItem.DateToCustomer.Keys.Count; i++)
            {
                header.CreateCell(2 + i)
                    .SetCellValue(headerItem.DateToCustomer.Keys.ElementAt(i).ToFormattedStringDate());
            }

            // 2. 寫入資料

            foreach (Report11_Output_Row_APIItem item in response.Items)
            {
                IRow row = sheet.CreateRow(sheet.LastRowNum + 1);

                row.CreateCell(0).SetCellValue(item.Type);
                row.CreateCell(1).SetCellValue(item.SiteName);
                row.CreateCell(2).SetCellValue(item.Time);

                foreach (KeyValuePair<DateTime, string> kvp in item.DateToCustomer)
                {
                    row.CreateCell(row.LastCellNum).SetCellValue(kvp.Value ?? "");
                }
            }

            // 3. 前兩欄從上到下，相同內容的 cells 做 merge

            for (int c = 0; c <= 1; c++)
            {
                int thisRangeStart = 0;
                string lastCellContent = sheet.GetRow(0).GetCell(c).StringCellValue;

                // 小於等於，這樣才能保證最後一項也有 merge
                for (int r = 0; r <= sheet.PhysicalNumberOfRows; r++)
                {
                    IRow row = sheet.GetRow(r);
                    string thisCellContent = row?.GetCell(c)?.StringCellValue ?? "";

                    if (thisCellContent != lastCellContent)
                    {
                        CellRangeAddress range = new CellRangeAddress(thisRangeStart, r - 1, c, c);

                        if (range.NumberOfCells >= 2)
                            sheet.AddMergedRegion(range);

                        thisRangeStart = r;
                    }

                    lastCellContent = thisCellContent;
                }
            }

            // 完成, 調整所有欄位寬度

            for (var i = 0; i < header.Cells.Count; i++)
            {
                sheet.SetColumnWidth(i, header.GetCell(i)?.ToString().Length * 10 ?? 10);
            }

            XmlDocument xmlDocument = MakeXmlDocument(book);

            return xmlDocument.OuterXml;
        }

        private static XmlDocument MakeXmlDocument(IWorkbook book)
        {
            ExcelToHtmlConverter excelToHtmlConverter = new ExcelToHtmlConverter();
            excelToHtmlConverter.ProcessWorkbook(book);

            excelToHtmlConverter.Document.InnerXml =
                excelToHtmlConverter.Document.InnerXml.Insert(
                    excelToHtmlConverter.Document.InnerXml.IndexOf("<head>", 0,
                        StringComparison.InvariantCultureIgnoreCase) + 6,
                    @"<style>table, td, th{border:1px solid green;}th{background-color:green;color:white;}</style>"
                );

            return excelToHtmlConverter.Document;
        }
    }
}