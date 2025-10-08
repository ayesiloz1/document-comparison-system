using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace TestDocumentGenerator;

class Program
{
    static void Main(string[] args)
    {
        // Set QuestPDF license
        QuestPDF.Settings.License = LicenseType.Community;
        
        // Generate Version 1
        GenerateVersion1();
        
        // Generate Version 2
        GenerateVersion2();
        
        Console.WriteLine("Test PDFs generated successfully!");
        Console.WriteLine("- SoftwareSpec_v1.pdf");
        Console.WriteLine("- SoftwareSpec_v2.pdf");
    }

    static void GenerateVersion1()
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(12));

                page.Header()
                    .Text("Software Requirements Specification v1.0")
                    .SemiBold().FontSize(18).FontColor(Colors.Blue.Medium);

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(column =>
                    {
                        column.Spacing(20);

                        column.Item().Text("1. Introduction").Bold().FontSize(14);
                        column.Item().Text("This document outlines the requirements for the Customer Management System. The system will allow users to manage customer information and generate basic reports.");

                        column.Item().Text("2. Functional Requirements").Bold().FontSize(14);
                        column.Item().Text("2.1 User Authentication").Bold();
                        column.Item().Text("- Users must log in with username and password");
                        column.Item().Text("- System supports basic user roles");
                        
                        column.Item().Text("2.2 Customer Management").Bold();
                        column.Item().Text("- Add new customers");
                        column.Item().Text("- Edit existing customer information");
                        column.Item().Text("- Delete customers");
                        column.Item().Text("- Search customers by name");

                        column.Item().Text("2.3 Reporting").Bold();
                        column.Item().Text("- Generate customer list report");
                        column.Item().Text("- Export reports to PDF format");

                        column.Item().Text("3. Non-Functional Requirements").Bold().FontSize(14);
                        column.Item().Text("- System must support up to 100 concurrent users");
                        column.Item().Text("- Response time should be under 2 seconds");
                        column.Item().Text("- System availability: 99% uptime");

                        column.Item().Text("4. Technical Requirements").Bold().FontSize(14);
                        column.Item().Text("- Web-based application");
                        column.Item().Text("- SQL Server database");
                        column.Item().Text("- .NET Framework 4.8");
                    });

                page.Footer()
                    .AlignCenter()
                    .Text("Page 1 of 1 | Document Version 1.0 | Created: January 2024");
            });
        });

        document.GeneratePdf("SoftwareSpec_v1.pdf");
    }

    static void GenerateVersion2()
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(12));

                page.Header()
                    .Text("Software Requirements Specification v2.1")
                    .SemiBold().FontSize(18).FontColor(Colors.Blue.Medium);

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(column =>
                    {
                        column.Spacing(20);

                        column.Item().Text("1. Introduction").Bold().FontSize(14);
                        column.Item().Text("This document outlines the requirements for the Enhanced Customer Management System. The system will allow users to manage customer information, handle orders, and generate comprehensive reports with analytics.");

                        column.Item().Text("2. Functional Requirements").Bold().FontSize(14);
                        column.Item().Text("2.1 User Authentication & Authorization").Bold();
                        column.Item().Text("- Users must log in with username and password");
                        column.Item().Text("- Multi-factor authentication support");
                        column.Item().Text("- Advanced role-based access control (Admin, Manager, User)");
                        column.Item().Text("- Password complexity requirements");
                        
                        column.Item().Text("2.2 Customer Management").Bold();
                        column.Item().Text("- Add new customers with extended profile fields");
                        column.Item().Text("- Edit existing customer information");
                        column.Item().Text("- Soft delete customers (archive instead of permanent delete)");
                        column.Item().Text("- Advanced search: name, email, phone, company");
                        column.Item().Text("- Customer categorization and tagging");

                        column.Item().Text("2.3 Order Management").Bold();
                        column.Item().Text("- Create and manage customer orders");
                        column.Item().Text("- Order status tracking");
                        column.Item().Text("- Invoice generation");

                        column.Item().Text("2.4 Advanced Reporting & Analytics").Bold();
                        column.Item().Text("- Customer demographics report");
                        column.Item().Text("- Sales analytics dashboard");
                        column.Item().Text("- Custom report builder");
                        column.Item().Text("- Export to PDF, Excel, and CSV formats");
                        column.Item().Text("- Scheduled report delivery via email");

                        column.Item().Text("3. Non-Functional Requirements").Bold().FontSize(14);
                        column.Item().Text("- System must support up to 500 concurrent users");
                        column.Item().Text("- Response time should be under 1 second for standard operations");
                        column.Item().Text("- System availability: 99.9% uptime with load balancing");
                        column.Item().Text("- Data backup every 4 hours");
                        column.Item().Text("- GDPR compliance for data protection");

                        column.Item().Text("4. Technical Requirements").Bold().FontSize(14);
                        column.Item().Text("- Modern web application with responsive design");
                        column.Item().Text("- PostgreSQL database cluster");
                        column.Item().Text("- .NET 8 with microservices architecture");
                        column.Item().Text("- Redis cache for performance optimization");
                        column.Item().Text("- Docker containerization");
                        column.Item().Text("- API-first design with REST endpoints");
                    });

                page.Footer()
                    .AlignCenter()
                    .Text("Page 1 of 1 | Document Version 2.1 | Updated: October 2024");
            });
        });

        document.GeneratePdf("SoftwareSpec_v2.pdf");
    }
}