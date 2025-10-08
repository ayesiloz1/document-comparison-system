using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

// Set license for QuestPDF
QuestPDF.Settings.License = LicenseType.Community;

// Create first document with original content
Document.Create(container =>
{
    container.Page(page =>
    {
        page.Size(PageSizes.A4);
        page.Margin(2, Unit.Centimetre);
        page.DefaultTextStyle(x => x.FontSize(12));

        page.Header()
            .Text("Software Requirements Specification - Version 1.0")
            .FontSize(16)
            .Bold()
            .AlignCenter();

        page.Content()
            .Column(column =>
            {
                column.Item()
                    .Text("1. Introduction")
                    .FontSize(14)
                    .Bold()
                    .PaddingBottom(10);

                column.Item()
                    .Text("This document outlines the requirements for a document management system. The system should provide users with the ability to upload, store, and organize documents efficiently.")
                    .PaddingBottom(15);

                column.Item()
                    .Text("2. System Requirements")
                    .FontSize(14)
                    .Bold()
                    .PaddingBottom(10);

                column.Item()
                    .Text("2.1 Functional Requirements:")
                    .Bold()
                    .PaddingBottom(5);

                column.Item()
                    .Text("- User authentication and authorization")
                    .PaddingLeft(20)
                    .PaddingBottom(3);

                column.Item()
                    .Text("- Document upload functionality")
                    .PaddingLeft(20)
                    .PaddingBottom(3);

                column.Item()
                    .Text("- Document categorization")
                    .PaddingLeft(20)
                    .PaddingBottom(3);

                column.Item()
                    .Text("- Search functionality")
                    .PaddingLeft(20)
                    .PaddingBottom(15);

                column.Item()
                    .Text("2.2 Non-Functional Requirements:")
                    .Bold()
                    .PaddingBottom(5);

                column.Item()
                    .Text("- System should support up to 1000 concurrent users")
                    .PaddingLeft(20)
                    .PaddingBottom(3);

                column.Item()
                    .Text("- Response time should be under 2 seconds")
                    .PaddingLeft(20)
                    .PaddingBottom(3);

                column.Item()
                    .Text("- 99.9% uptime availability")
                    .PaddingLeft(20)
                    .PaddingBottom(15);

                column.Item()
                    .Text("3. Technical Specifications")
                    .FontSize(14)
                    .Bold()
                    .PaddingBottom(10);

                column.Item()
                    .Text("The system will be built using modern web technologies including React for the frontend and .NET Core for the backend API. The database will be PostgreSQL for reliable data storage.")
                    .PaddingBottom(15);

                column.Item()
                    .Text("4. Security Requirements")
                    .FontSize(14)
                    .Bold()
                    .PaddingBottom(10);

                column.Item()
                    .Text("All data transmission must be encrypted using HTTPS. User passwords must be hashed using bcrypt. Access to sensitive documents requires multi-factor authentication.");
            });

        page.Footer()
            .AlignCenter()
            .Text(x =>
            {
                x.Span("Page ");
                x.CurrentPageNumber();
                x.Span(" of ");
                x.TotalPages();
            });
    });
})
.GeneratePdf("TestDocument_v1.pdf");

// Create second document with updated content
Document.Create(container =>
{
    container.Page(page =>
    {
        page.Size(PageSizes.A4);
        page.Margin(2, Unit.Centimetre);
        page.DefaultTextStyle(x => x.FontSize(12));

        page.Header()
            .Text("Software Requirements Specification - Version 2.0")
            .FontSize(16)
            .Bold()
            .AlignCenter();

        page.Content()
            .Column(column =>
            {
                column.Item()
                    .Text("1. Introduction")
                    .FontSize(14)
                    .Bold()
                    .PaddingBottom(10);

                column.Item()
                    .Text("This document outlines the requirements for an advanced document management system with AI capabilities. The system should provide users with the ability to upload, store, organize, and intelligently analyze documents.")
                    .PaddingBottom(15);

                column.Item()
                    .Text("2. System Requirements")
                    .FontSize(14)
                    .Bold()
                    .PaddingBottom(10);

                column.Item()
                    .Text("2.1 Functional Requirements:")
                    .Bold()
                    .PaddingBottom(5);

                column.Item()
                    .Text("- User authentication and authorization")
                    .PaddingLeft(20)
                    .PaddingBottom(3);

                column.Item()
                    .Text("- Document upload functionality with drag-and-drop support")
                    .PaddingLeft(20)
                    .PaddingBottom(3);

                column.Item()
                    .Text("- Advanced document categorization with AI tagging")
                    .PaddingLeft(20)
                    .PaddingBottom(3);

                column.Item()
                    .Text("- Intelligent search functionality with semantic search")
                    .PaddingLeft(20)
                    .PaddingBottom(3);

                column.Item()
                    .Text("- Document comparison and diff analysis")
                    .PaddingLeft(20)
                    .PaddingBottom(3);

                column.Item()
                    .Text("- Real-time collaboration features")
                    .PaddingLeft(20)
                    .PaddingBottom(15);

                column.Item()
                    .Text("2.2 Non-Functional Requirements:")
                    .Bold()
                    .PaddingBottom(5);

                column.Item()
                    .Text("- System should support up to 5000 concurrent users")
                    .PaddingLeft(20)
                    .PaddingBottom(3);

                column.Item()
                    .Text("- Response time should be under 1 second")
                    .PaddingLeft(20)
                    .PaddingBottom(3);

                column.Item()
                    .Text("- 99.95% uptime availability")
                    .PaddingLeft(20)
                    .PaddingBottom(3);

                column.Item()
                    .Text("- GDPR and SOC2 compliance")
                    .PaddingLeft(20)
                    .PaddingBottom(15);

                column.Item()
                    .Text("3. Technical Specifications")
                    .FontSize(14)
                    .Bold()
                    .PaddingBottom(10);

                column.Item()
                    .Text("The system will be built using modern web technologies including React 18 with TypeScript for the frontend and .NET 9 for the backend API. The database will be PostgreSQL with Redis for caching. AI features will be powered by Azure OpenAI services.")
                    .PaddingBottom(15);

                column.Item()
                    .Text("4. Security Requirements")
                    .FontSize(14)
                    .Bold()
                    .PaddingBottom(10);

                column.Item()
                    .Text("All data transmission must be encrypted using HTTPS with TLS 1.3. User passwords must be hashed using Argon2. Access to sensitive documents requires multi-factor authentication with biometric options. All API endpoints must implement rate limiting and CSRF protection.");

                column.Item()
                    .Text("5. New AI Features")
                    .FontSize(14)
                    .Bold()
                    .PaddingBottom(10)
                    .PaddingTop(15);

                column.Item()
                    .Text("The system will include advanced AI capabilities for document analysis, automatic summarization, and intelligent content suggestions. Machine learning models will continuously improve based on user interactions and feedback.");
            });

        page.Footer()
            .AlignCenter()
            .Text(x =>
            {
                x.Span("Page ");
                x.CurrentPageNumber();
                x.Span(" of ");
                x.TotalPages();
            });
    });
})
.GeneratePdf("TestDocument_v2.pdf");

Console.WriteLine("Test documents created successfully!");
Console.WriteLine("- TestDocument_v1.pdf (Original version)");
Console.WriteLine("- TestDocument_v2.pdf (Updated version with AI features)");
Console.WriteLine("\nKey differences:");
Console.WriteLine("✓ Enhanced introduction with AI capabilities");
Console.WriteLine("✓ New functional requirements (drag-drop, AI tagging, semantic search, document comparison, collaboration)");
Console.WriteLine("✓ Improved non-functional requirements (5x user capacity, better performance, compliance)");
Console.WriteLine("✓ Updated technical stack (React 18, .NET 9, Redis, Azure OpenAI)");
Console.WriteLine("✓ Enhanced security (TLS 1.3, Argon2, biometrics, rate limiting)");
Console.WriteLine("✓ Brand new AI Features section");