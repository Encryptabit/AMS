using System;
using System.Linq;
using Xceed.Words.NET;

Console.WriteLine(typeof(DocX).Assembly.FullName);
var docxType = typeof(DocX);
foreach(var method in docxType.GetMethods().Where(m => m.Name.Contains("Page", StringComparison.OrdinalIgnoreCase)))
{
    Console.WriteLine(method);
}

var documentType = typeof(DocX).Assembly.GetType("Xceed.Document.NET.Document");
if(documentType != null)
{
    Console.WriteLine($"Document methods containing PageCount: {documentType.GetMethods().Any(m => m.Name.Contains("PageCount"))}");
}
