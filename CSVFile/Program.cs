﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CSVFile.Models;
using CSVFile.Models.Utilities;
using System.Xml.Serialization;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;


using DocumentFormat.OpenXml.ExtendedProperties;
using System.Drawing;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;
using System.Data.OleDb;

namespace CSVFile
{
    class Program
    {






        static void Main(string[] args)
        {
            Student myrecord = new Student { StudentId = "200429013", FirstName = "Priyanka", LastName = "Garg" };

            List<string> directories = FTP.GetDirectory(Constants.FTP.BaseUrl);
            List<Student> students = new List<Student>();

            foreach (var directory in directories)
            {
                Student student = new Student() { AbsoluteUrl = Constants.FTP.BaseUrl };
                student.FromDirectory(directory);

                //Console.WriteLine(student);
                string infoFilePath = student.FullPathUrl + "/" + Constants.Locations.InfoFile;

                bool fileExists = FTP.FileExists(infoFilePath);
                if (fileExists == true)
                {
                    string csvPath = $@"/Users/priyankagarg/Desktop/Student Data/{directory}.csv";

                    // FTP.DownloadFile(infoFilePath, csvPath);
                    byte[] bytes = FTP.DownloadFileBytes(infoFilePath);
                    string csvData = Encoding.Default.GetString(bytes);

                    string[] csvlines = csvData.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

                    if (csvlines.Length != 2)
                    {
                        Console.WriteLine("Error in CSV format");
                    }
                    else
                    {
                        student.FromCSV(csvlines[1]);
                        Console.WriteLine("  \t Age of Student is: {0} ", student.age);


                    }

                    Console.WriteLine("Found info file:");
                    student.MyRecord = "yes";

                }
                else
                {
                    Console.WriteLine("Could not find info file:");
                    student.MyRecord = "No";
                }

                Console.WriteLine("\t" + infoFilePath);

                string imageFilePath = student.FullPathUrl + "/" + Constants.Locations.ImageFile;

                bool imageFileExists = FTP.FileExists(imageFilePath);

                if (imageFileExists == true)
                {

                    Console.WriteLine("Found image file:");

                }
                else
                {
                    Console.WriteLine("Could not find image file:");

                }

                Console.WriteLine("\t" + imageFilePath);

                students.Add(student);
                Console.WriteLine(directory);

                Console.WriteLine(" \t Count of student is: {0}", students.Count);
                Console.WriteLine("  \t Age of Student is: {0} ", student.age);
                Console.WriteLine("  \t Record of Student is: {0} ", student.MyRecord);

            }

            Student me = students.SingleOrDefault(x => x.StudentId == myrecord.StudentId);
            Student meUsingFind = students.Find(x => x.StudentId == myrecord.StudentId);

            var avgage = students.Average(x => x.age);
            var minage = students.Min(x => x.age);
            var maxage = students.Max(x => x.age);


            Console.WriteLine("  \n\t Name Searched With Query: {0} ", meUsingFind);
            Console.WriteLine("  \t Average of Student age is: {0} ", avgage);
            Console.WriteLine("  \t Minimum of Student age is: {0} ", minage);
            Console.WriteLine("  \t Maximum of Student age is: {0} ", maxage);

            //save to csv

            string studentsCSVPath = $"{Constants.Locations.DataFolder}//students.csv";
            //Establish a file stream to collect data from the response
            using (StreamWriter fs = new StreamWriter(studentsCSVPath))
            {
                foreach (var student in students)
                {
                    fs.WriteLine(student.ToCSV());
                }

            }


            string studentsWordPath = $"{Constants.Locations.DataFolder}//students.docx";
            //string studentsImagePath1 = $"{Constants.Locations.ImagesFolder}//images.jpg";

            //Create Word sheet and fetch data from FTP
            // Create a document by supplying the filepath. 
            using (WordprocessingDocument wordDocument =
                WordprocessingDocument.Create(studentsWordPath, WordprocessingDocumentType.Document))
            {



                // Add a main document part. 
                MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();


                // Create the document structure and add some text.
                mainPart.Document = new Document();
                Body body = mainPart.Document.AppendChild(new Body());
                Paragraph para = body.AppendChild(new Paragraph());




                Run run = para.AppendChild(new Run());


                //using (FileStream stream = new FileStream(studentsImagePath, FileMode.Open))
                //{
                //    imagePart.FeedData(stream);
                //}

                //Word.AddImageToBody(wordDocument, mainPart.GetIdOfPart(imagePart));


                foreach (var student in students)
                {


                    run.AppendChild(new Text("My name is :  "));
                    //run.AppendChild(new Text(student.ToString()));
                    run.AppendChild(new Text(student.FirstName.ToString()));
                    run.AppendChild(new Text("  ,  "));

                    run.AppendChild(new Text("My Student id is: "));


                    run.AppendChild(new Text(student.StudentId.ToString()));
                    run.AppendChild(new Text("  ,  "));



                    run.AppendChild(new Break() { Type = BreakValues.Page });








                }




            }







            string studentsjsonPath = $"{Constants.Locations.DataFolder}//students.json";
            //Establish a file stream to collect data from the response
            using (StreamWriter fs = new StreamWriter(studentsjsonPath))
            {
                foreach (var student in students)
                {
                    string Student = Newtonsoft.Json.JsonConvert.SerializeObject(student);
                    fs.WriteLine(Student.ToString());
                    //Console.WriteLine(jStudent);
                }
            }


            //Create Excel sheet and fetch data from FTP
            string studentsExcelPath = $"{Constants.Locations.DataFolder}//students.xlsx";


            //using (StreamWriter fs = new StreamWriter(studentsxmlPath))
            //{
            SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.
        Create(studentsExcelPath, SpreadsheetDocumentType.Workbook);

            // Add a WorkbookPart to the document.
            WorkbookPart workbookpart = spreadsheetDocument.AddWorkbookPart();
            workbookpart.Workbook = new DocumentFormat.OpenXml.Spreadsheet.Workbook();

            // Add a WorksheetPart to the WorkbookPart.
            WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new DocumentFormat.OpenXml.Spreadsheet.Worksheet(new DocumentFormat.OpenXml.Spreadsheet.SheetData());

            // Add Sheets to the Workbook.
            DocumentFormat.OpenXml.Spreadsheet.Sheets sheets = spreadsheetDocument.WorkbookPart.Workbook.
                AppendChild<DocumentFormat.OpenXml.Spreadsheet.Sheets>(new DocumentFormat.OpenXml.Spreadsheet.Sheets());

            // Append a new worksheet and associate it with the workbook.
            DocumentFormat.OpenXml.Spreadsheet.Sheet sheet = new DocumentFormat.OpenXml.Spreadsheet.Sheet()
            {
                Id = spreadsheetDocument.WorkbookPart.
                GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = "mySheet"
            };
            DocumentFormat.OpenXml.Spreadsheet.SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.SheetData>();
            var excelRows = sheetData.Descendants<DocumentFormat.OpenXml.Spreadsheet.Row>().ToList();
            //var excelcolumns = sheetData.Descendants<DocumentFormat.OpenXml.Spreadsheet.Column>().ToList();
            int rowindex = 1;
            //int columnindex = 1;
            foreach (var student in students)
            {

                DocumentFormat.OpenXml.Spreadsheet.Row row = new DocumentFormat.OpenXml.Spreadsheet.Row();
                DocumentFormat.OpenXml.Spreadsheet.Columns cs = new DocumentFormat.OpenXml.Spreadsheet.Columns();
                row.RowIndex = (UInt32)rowindex;

                DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell()
                {

                    DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String,
                    CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(student.FirstName.ToString())
                    //CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(student.FirstName.ToString())


                };
                DocumentFormat.OpenXml.Spreadsheet.Cell cell1 = new DocumentFormat.OpenXml.Spreadsheet.Cell()
                {

                    DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String,
                    CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(student.LastName.ToString())
                    //CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(student.FirstName.ToString())


                };
                DocumentFormat.OpenXml.Spreadsheet.Cell cell2 = new DocumentFormat.OpenXml.Spreadsheet.Cell()
                {

                    DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String,
                    CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(student.StudentId.ToString())
                    //CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(student.FirstName.ToString())


                };
                DocumentFormat.OpenXml.Spreadsheet.Cell cell3 = new DocumentFormat.OpenXml.Spreadsheet.Cell()
                {

                    DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String,
                    CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(Convert.ToString(student.MyRecord.ToString()))
                    //CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(student.FirstName.ToString())


                };

                DocumentFormat.OpenXml.Spreadsheet.Cell cell4 = new DocumentFormat.OpenXml.Spreadsheet.Cell()
                {

                    DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String,
                    CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(student.age.ToString())
                    //CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(student.FirstName.ToString())


                };
                DocumentFormat.OpenXml.Spreadsheet.Cell cell5 = new DocumentFormat.OpenXml.Spreadsheet.Cell()
                {


                    DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String,
                    CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(Convert.ToString(student.DateOfBirthDT.ToString()))



                };
                DocumentFormat.OpenXml.Spreadsheet.Cell cell6 = new DocumentFormat.OpenXml.Spreadsheet.Cell()
                {


                    DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String,
                    CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(Convert.ToString(Guid.NewGuid().ToString()))



                };


                row.Append(cell);
                row.Append(cell1);
                row.Append(cell2);
                row.Append(cell3);
                row.Append(cell4);
                row.Append(cell5);
                row.Append(cell6);



                sheetData.Append(row);



                //how to write the data in cell
                rowindex++;
            }

            sheets.Append(sheet);

            workbookpart.Workbook.Save();

            // Close the document.
            spreadsheetDocument.Close();






            string studentsxmlPath = $"{Constants.Locations.DataFolder}//students.xml";
            //Establish a file stream to collect data from the response
            using (StreamWriter fs = new StreamWriter(studentsxmlPath))
            {
                XmlSerializer x = new XmlSerializer(students.GetType());
                x.Serialize(fs, students);
                Console.WriteLine();
            }

            //  4.Upload the files to My FTP
            foreach (var student in students)
            {

                FTP.UploadFile(studentsCSVPath, Constants.FTP.BaseUrl + "/200429013 Priyanka Garg/students.csv");
                FTP.UploadFile(studentsjsonPath, Constants.FTP.BaseUrl + "/200429013 Priyanka Garg/students.json");
                FTP.UploadFile(studentsxmlPath, Constants.FTP.BaseUrl + "/200429013 Priyanka Garg/students.xml");
                FTP.UploadFile(studentsxmlPath, Constants.FTP.BaseUrl + "/200429013 Priyanka Garg/students.word");
                FTP.UploadFile(studentsxmlPath, Constants.FTP.BaseUrl + "/200429013 Priyanka Garg/students.xlsx");

            }


            return;


        }


    }
}
