﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models;
using Models.ViewModels;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using OfficeOpenXml;
using UploadExcel.Models;

namespace UploadExcel.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment webHostEnvironment;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            this.webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost("import")]
        public async Task<HttpResponse<List<User>>> Import(IFormFile file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length <= 0)
            {
                return HttpResponse<List<User>>.GetResult(-1, "File is empty.");
            }

            if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return HttpResponse<List<User>>.GetResult(-1, "Not Support file extension");
            }

            var list = new List<User>();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream, cancellationToken);

                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    var rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        list.Add(new User
                        {
                            Name = worksheet.Cells[row, 1].Value.ToString().Trim(),
                            Email = worksheet.Cells[row, 2].Value.ToString().Trim(),
                            Age = int.Parse(worksheet.Cells[row, 3].Value.ToString().Trim()),

                        });
                    }
                }
            }

            // add list to db ..  
            // here just read and return  

            return HttpResponse<List<User>>.GetResult(0, "OK", list);
        }

        [HttpPost("import-sync")]
        public HttpResponse<List<User>> Import(IFormFile file)
        {
            if (file == null || file.Length <= 0)
            {
                return HttpResponse<List<User>>.GetResult(-1, "File is empty.");
            }

            if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return HttpResponse<List<User>>.GetResult(-1, "Not Support file extension");
            }

            var list = new List<User>();
            // If you use EPPlus in a noncommercial context
            // according to the Polyform Noncommercial license:
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var stream = new MemoryStream())
            {
                file.CopyToAsync(stream);

                using (var package = new ExcelPackage(stream))
                {

                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    var rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        list.Add(new User
                        {
                            Name = worksheet.Cells[row, 1].Value.ToString().Trim(),
                            Email = worksheet.Cells[row, 2].Value.ToString().Trim(),
                            Age = int.Parse(worksheet.Cells[row, 3].Value.ToString().Trim()),

                        });
                    }
                }
            }

            // add list to db ..  
            // here just read and return  

            return HttpResponse<List<User>>.GetResult(0, "OK", list);
        }


        [HttpGet("export")]
        public async Task<HttpResponse<string>> Export(CancellationToken cancellationToken)
        {
            string folder = webHostEnvironment.WebRootPath;
            string excelName = $"UserList-{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.xlsx";
            string downloadUrl = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, excelName);
            FileInfo file = new FileInfo(Path.Combine(folder, excelName));
            if (file.Exists)
            {
                file.Delete();
                file = new FileInfo(Path.Combine(folder, excelName));
            }

            // query data from database  
            await Task.Yield();

            var list = new List<User>()
            {
                new User { Name = "catcher",Email="catcher@me.com",  Age = 18 },
                new User { Name = "james",Email="james@me.com" , Age = 20 },
            };

            using (var package = new ExcelPackage(file))
            {
                var workSheet = package.Workbook.Worksheets.Add("Sheet1");
                workSheet.Cells.LoadFromCollection(list, true);
                package.Save();
            }

            return HttpResponse<string>.GetResult(0, "OK", downloadUrl);
        }

        [HttpGet("download")]
        public async Task<IActionResult> ExportV2(CancellationToken cancellationToken)
        {
            // query data from database  
            await Task.Yield();
            var list = new List<User>()
            {
                new User { Name = "catcher",Email="catcher@me.com",  Age = 18 },
                new User { Name = "james",Email="james@me.com" , Age = 20 },
            };
            var stream = new MemoryStream();

            using (var package = new ExcelPackage(stream))
            {
                var workSheet = package.Workbook.Worksheets.Add("Sheet1");
                workSheet.Cells.LoadFromCollection(list, true);
                package.Save();
            }
            stream.Position = 0;
            string excelName = $"UserList-{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.xlsx";

            //return File(stream, "application/octet-stream", excelName);  
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
        }

        [HttpPost("import-sync-npoi")]
        public HttpResponse<List<User>> ImportNpoi(IFormFile file)
        {
            if (file == null || file.Length <= 0)
            {
                return HttpResponse<List<User>>.GetResult(-1, "File is empty.");
            }

            string fileExtension = Path.GetExtension(file.FileName);
            if (!(fileExtension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase)
                || fileExtension.Equals(".xls", StringComparison.OrdinalIgnoreCase)))
            {
                return HttpResponse<List<User>>.GetResult(-1, "Not Support file extension");
            }

            var list = new List<User>();

            using (var stream = new MemoryStream())
            {
                ISheet sheet;

                file.CopyToAsync(stream);
                stream.Position = 0;
                if (fileExtension == ".xls") // excel 97-2000 formats
                {
                    HSSFWorkbook hssfWorkbook = new HSSFWorkbook(stream);
                    sheet = hssfWorkbook.GetSheetAt(0);
                }
                else // excel 2007 format
                {
                    XSSFWorkbook xssfWorkbook = new XSSFWorkbook(stream);
                    sheet = xssfWorkbook.GetSheetAt(0);
                }

                IRow headerRow = sheet.GetRow(0); // Row 0 is the header containing all columns
                int cellCount = headerRow.LastCellNum; // note that the cell number is the size, RowNum is the index
                if (cellCount != 3)
                {

                    return HttpResponse<List<User>>.GetResult(-1, "File should have 3 columns");
                }

                for (int i = sheet.FirstRowNum + 1; i <= sheet.LastRowNum; ++i)
                {
                    IRow row = sheet.GetRow(i);
                    if (row == null) continue;

                    list.Add(new User
                    {
                        Name = row.GetCell(0).ToString().Trim(),
                        Email = row.GetCell(1).ToString().Trim(),
                        Age = int.Parse(row.GetCell(2).ToString().Trim()),

                    });
                }

            }

            // add list to db ..  
            // here just read and return  

            return HttpResponse<List<User>>.GetResult(0, "OK", list);
        }
    }
}
