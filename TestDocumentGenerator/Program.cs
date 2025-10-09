using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace TestDocumentGenerator;

class Program
{
    static void Main(string[] args)
    {
        // Set QuestPDF license and enable debugging
        QuestPDF.Settings.License = LicenseType.Community;
        QuestPDF.Settings.EnableDebugging = true;
        
        // Generate Version 1
        GenerateVersion1();
        
        // Generate Version 2
        GenerateVersion2();
        
        Console.WriteLine("Detailed test PDFs generated successfully!");
        Console.WriteLine("- SoftwareSpec_v1.pdf (Multi-page detailed specification)");
        Console.WriteLine("- SoftwareSpec_v2.pdf (Enhanced multi-page specification)");
    }

    static void GenerateVersion1()
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                page.Header()
                    .Height(80)
                    .Background(Colors.Grey.Lighten3)
                    .Padding(15)
                    .Row(row =>
                    {
                        row.RelativeItem().Column(column =>
                        {
                            column.Item().Text("Software Requirements Specification")
                                .SemiBold().FontSize(18).FontColor(Colors.Blue.Darken2);
                            column.Item().Text("Customer Management System v1.0")
                                .FontSize(12).FontColor(Colors.Grey.Darken1);
                        });
                        
                        row.ConstantItem(80).AlignRight().Column(column =>
                        {
                            column.Item().Text("Version: 1.0").FontSize(9);
                            column.Item().Text("Date: Jan 15, 2024").FontSize(9);
                            column.Item().Text("Status: Final").FontSize(9).FontColor(Colors.Green.Darken1);
                        });
                    });

                page.Content().Column(column =>
                {
                    // Cover Page
                    column.Item().AlignCenter().PaddingTop(100).Column(coverColumn =>
                    {
                        coverColumn.Item().Text("SOFTWARE REQUIREMENTS SPECIFICATION")
                            .Bold().FontSize(24).FontColor(Colors.Blue.Darken2);
                        coverColumn.Item().PaddingTop(20).Text("Customer Management System")
                            .FontSize(20).FontColor(Colors.Grey.Darken1);
                        coverColumn.Item().PaddingTop(10).Text("Version 1.0")
                            .FontSize(16);
                        coverColumn.Item().PaddingTop(50).Text("January 15, 2024")
                            .FontSize(14);
                    });

                    // Table of Contents
                    column.Item().PageBreak();
                    column.Item().Text("Table of Contents").Bold().FontSize(16).FontColor(Colors.Blue.Darken1);
                    column.Item().PaddingTop(10).Column(tocColumn =>
                    {
                        tocColumn.Item().Text("1. Executive Summary ......................................................... 3").FontSize(10);
                        tocColumn.Item().Text("2. Introduction ................................................................. 4").FontSize(10);
                        tocColumn.Item().Text("3. Project Scope and Objectives .......................................... 5").FontSize(10);
                        tocColumn.Item().Text("4. Stakeholder Analysis ..................................................... 6").FontSize(10);
                        tocColumn.Item().Text("5. Functional Requirements ................................................ 7").FontSize(10);
                        tocColumn.Item().Text("6. Non-Functional Requirements .......................................... 9").FontSize(10);
                        tocColumn.Item().Text("7. Technical Architecture .................................................. 10").FontSize(10);
                        tocColumn.Item().Text("8. Security Requirements ................................................... 11").FontSize(10);
                    });

                    // Executive Summary
                    column.Item().PageBreak();
                    AddSection(column, "1. Executive Summary", @"
The Customer Management System (CMS) represents a strategic initiative to modernize and centralize customer data management processes across the organization. This comprehensive software solution will replace existing disparate systems and manual processes with a unified, web-based platform designed to enhance operational efficiency and customer service delivery.

The primary objective of this system is to provide a robust, scalable platform for managing customer relationships, tracking interactions, and generating actionable business intelligence through comprehensive reporting capabilities.

Key benefits of the proposed system include:
• Centralized customer data repository with 360-degree customer view
• Automated business processes reducing manual intervention by 60%
• Enhanced data accuracy and consistency across all customer touchpoints
• Improved reporting and analytics capabilities for strategic planning
• Scalable architecture supporting future business growth
• Compliance with industry standards and regulatory requirements

The project scope encompasses the design, development, testing, and deployment of a complete customer management solution, including data migration from legacy systems, user training, and ongoing support infrastructure. The estimated implementation timeline is 8 months with a total project budget of $450,000.");

                    // Introduction
                    column.Item().PageBreak();
                    AddSection(column, "2. Introduction", @"
This Software Requirements Specification (SRS) document provides a comprehensive overview of the Customer Management System requirements, serving as the primary reference for all stakeholders throughout the project lifecycle. The document has been developed in accordance with IEEE 830-1998 standards and follows industry best practices for requirements documentation.

2.1 Purpose and Scope
The Customer Management System is designed to address critical business challenges related to customer data fragmentation, inefficient manual processes, and limited reporting capabilities. The system will provide a centralized platform for managing all aspects of customer relationships, from initial contact through ongoing service delivery.

2.2 Document Conventions
This document uses the following conventions:
• SHALL indicates mandatory requirements
• SHOULD indicates recommended features
• MAY indicates optional capabilities
• Risk levels: Critical, High, Medium, Low

2.3 Intended Audience
This document is intended for:
• Business stakeholders and executive sponsors
• Project managers and technical leads
• Software development teams
• Quality assurance and testing teams
• System administrators and IT operations
• End users and customer service representatives");

                    // Project Scope and Objectives
                    column.Item().PageBreak();
                    AddSection(column, "3. Project Scope and Objectives", @"
3.1 Business Objectives
The Customer Management System project aligns with the organization's strategic goals of digital transformation, operational excellence, and customer-centric service delivery. The primary business objectives include:

• Improve customer satisfaction scores by 25% through enhanced service delivery
• Reduce customer data processing time by 70% through automation
• Increase revenue per customer by 15% through better relationship management
• Achieve 99.5% data accuracy across all customer records
• Establish foundation for future digital initiatives and integrations

3.2 Functional Scope
The system will encompass the following functional areas:

Customer Data Management:
• Complete customer profile management with extensible data fields
• Advanced search and filtering capabilities across all customer attributes
• Customer relationship hierarchy management for corporate accounts
• Automated data validation and cleansing processes
• Integration with external data sources for enhanced profiles

User Management and Security:
• Role-based access control with granular permissions
• Multi-level approval workflows for sensitive operations
• Comprehensive audit trails for all system activities
• Single sign-on integration with existing Active Directory
• Password policy enforcement and multi-factor authentication

3.3 Technical Scope
The technical implementation will include:
• Modern web-based user interface with responsive design
• RESTful API architecture for system integrations
• Comprehensive data backup and disaster recovery procedures
• Performance monitoring and alerting systems
• Automated deployment and configuration management");

                    // Stakeholder Analysis
                    column.Item().PageBreak();
                    AddSection(column, "4. Stakeholder Analysis", @"
4.1 Primary Stakeholders

Executive Sponsors:
• CEO: Strategic oversight and ultimate accountability for project success
• CTO: Technical governance and architecture approval authority
• VP Customer Service: Primary business sponsor and requirements owner

Business Users:
• Customer Service Representatives (25 users): Daily system users for customer interactions
• Sales Team (15 users): Customer prospecting and relationship management
• Marketing Team (8 users): Campaign management and customer segmentation
• Management Team (12 users): Reporting and analytics consumers

Technical Team:
• IT Director: Infrastructure and security requirements ownership
• Database Administrator: Data architecture and performance optimization
• Network Administrator: System integration and connectivity requirements
• Help Desk Team: User support and training coordination

4.2 Success Criteria
Project success will be measured against the following criteria:
• User adoption rate exceeding 95% within 3 months of deployment
• Customer data accuracy improvement from 78% to 99.5%
• Average customer service response time reduced by 60%
• System availability meeting 99.5% uptime target
• Total cost of ownership reduction of 30% over 3 years");

                    // Functional Requirements
                    column.Item().PageBreak();
                    AddSection(column, "5. Functional Requirements", @"
5.1 User Authentication and Authorization

FR-001: User Login System
The system SHALL provide secure user authentication through username and password credentials. The authentication process SHALL include:
• Minimum password complexity requirements
• Account lockout after 3 consecutive failed attempts
• Password expiration every 90 days with 7-day advance notification
• Session timeout after 30 minutes of inactivity
• Audit logging of all authentication attempts

FR-002: Role-Based Access Control
The system SHALL implement role-based access control with the following predefined roles:
• Administrator: Full system access including user management and configuration
• Manager: Read/write access to all customer data with reporting capabilities
• User: Limited read/write access to assigned customer accounts only
• Read-Only: View-only access for reporting and inquiry purposes

5.2 Customer Data Management

FR-003: Customer Profile Management
The system SHALL provide comprehensive customer profile management including:
• Contact information (name, address, phone, email, website)
• Company details (industry, size, annual revenue, key contacts)
• Service history and interaction tracking
• Custom fields for industry-specific data requirements
• Document attachment capabilities for contracts and correspondence");

                    // Non-Functional Requirements
                    column.Item().PageBreak();
                    AddSection(column, "6. Non-Functional Requirements", @"
6.1 Performance Requirements

NFR-001: Response Time
The system SHALL meet the following response time requirements:
• Page load time: Maximum 3 seconds for standard operations
• Search queries: Maximum 5 seconds for complex searches
• Report generation: Maximum 30 seconds for standard reports
• Data export: Maximum 2 minutes for large datasets
• System startup: Maximum 60 seconds for application initialization

NFR-002: Throughput
The system SHALL support the following throughput requirements:
• Concurrent users: Minimum 100 simultaneous active users
• Transaction volume: 10,000 transactions per hour during peak usage
• Database queries: 500 queries per second sustained performance
• Batch processing: 50,000 records per hour for bulk operations

6.2 Security Requirements

NFR-003: Data Protection
The system SHALL implement enterprise-grade security measures including:
• Data encryption at rest using AES-256 encryption
• Data encryption in transit using TLS 1.3 protocol
• Role-based access control with principle of least privilege
• Comprehensive audit logging with tamper-proof storage
• Regular security vulnerability assessments and penetration testing");

                    // Technical Architecture
                    column.Item().PageBreak();
                    AddSection(column, "7. Technical Architecture", @"
7.1 System Architecture Overview
The Customer Management System will be implemented using a modern three-tier architecture consisting of presentation, business logic, and data tiers. This architecture provides scalability, maintainability, and security while supporting future enhancements and integrations.

7.2 Technology Stack

Presentation Tier:
• Web-based user interface using HTML5, CSS3, and JavaScript
• Responsive design framework (Bootstrap 4.x) for cross-device compatibility
• Modern web browser support (Chrome, Firefox, Safari, Edge)
• Progressive Web App (PWA) capabilities for mobile access

Application Tier:
• Microsoft .NET Framework 4.8 for server-side processing
• ASP.NET MVC architecture for web application framework
• Entity Framework 6.x for object-relational mapping
• RESTful web services for API-based integrations

Data Tier:
• Microsoft SQL Server 2019 Standard Edition as primary database
• SQL Server Reporting Services (SSRS) for report generation
• SQL Server Integration Services (SSIS) for data migration
• Full-text search capabilities for advanced search functionality");
                });

                page.Footer()
                    .Height(30)
                    .Background(Colors.Grey.Lighten4)
                    .Padding(10)
                    .Row(row =>
                    {
                        row.RelativeItem().Text("Customer Management System v1.0 - Requirements Specification").FontSize(8);
                        row.ConstantItem(50).AlignRight().Text(text =>
                        {
                            text.CurrentPageNumber().FontSize(8);
                            text.Span(" of ").FontSize(8);
                            text.TotalPages().FontSize(8);
                        });
                    });
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
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                page.Header()
                    .Height(80)
                    .Background(Colors.Blue.Lighten4)
                    .Padding(15)
                    .Row(row =>
                    {
                        row.RelativeItem().Column(column =>
                        {
                            column.Item().Text("Software Requirements Specification")
                                .SemiBold().FontSize(18).FontColor(Colors.Blue.Darken3);
                            column.Item().Text("Enhanced Customer Management System v2.1")
                                .FontSize(12).FontColor(Colors.Blue.Darken1);
                        });
                        
                        row.ConstantItem(80).AlignRight().Column(column =>
                        {
                            column.Item().Text("Version: 2.1").FontSize(9);
                            column.Item().Text("Date: Oct 15, 2024").FontSize(9);
                            column.Item().Text("Status: Active").FontSize(9).FontColor(Colors.Green.Darken2);
                        });
                    });

                page.Content().Column(column =>
                {
                    // Cover Page
                    column.Item().AlignCenter().PaddingTop(100).Column(coverColumn =>
                    {
                        coverColumn.Item().Text("SOFTWARE REQUIREMENTS SPECIFICATION")
                            .Bold().FontSize(24).FontColor(Colors.Blue.Darken3);
                        coverColumn.Item().PaddingTop(20).Text("Enhanced Customer Management System")
                            .FontSize(20).FontColor(Colors.Blue.Darken1);
                        coverColumn.Item().PaddingTop(10).Text("Version 2.1")
                            .FontSize(16);
                        coverColumn.Item().PaddingTop(50).Text("October 15, 2024")
                            .FontSize(14);
                    });

                    // Enhanced Table of Contents
                    column.Item().PageBreak();
                    column.Item().Text("Table of Contents").Bold().FontSize(16).FontColor(Colors.Blue.Darken2);
                    column.Item().PaddingTop(10).Column(tocColumn =>
                    {
                        tocColumn.Item().Text("1. Executive Summary ......................................................... 3").FontSize(10);
                        tocColumn.Item().Text("2. Introduction ................................................................. 4").FontSize(10);
                        tocColumn.Item().Text("3. Enhanced Project Scope and Objectives ............................. 5").FontSize(10);
                        tocColumn.Item().Text("4. Advanced Stakeholder Analysis ......................................... 6").FontSize(10);
                        tocColumn.Item().Text("5. Enhanced Functional Requirements ................................... 7").FontSize(10);
                        tocColumn.Item().Text("6. Advanced Non-Functional Requirements ............................ 10").FontSize(10);
                        tocColumn.Item().Text("7. Modern Technical Architecture ........................................ 11").FontSize(10);
                        tocColumn.Item().Text("8. Enhanced Security Framework ........................................ 12").FontSize(10);
                        tocColumn.Item().Text("9. AI and Analytics Features .............................................. 13").FontSize(10);
                        tocColumn.Item().Text("10. Cloud Infrastructure and DevOps ................................... 14").FontSize(10);
                    });

                    // Enhanced Executive Summary
                    column.Item().PageBreak();
                    AddSection(column, "1. Executive Summary", @"
The Enhanced Customer Management System (ECMS) v2.1 represents a significant evolution from the original CMS, incorporating advanced technologies, artificial intelligence capabilities, and modern architectural patterns to deliver a world-class customer experience platform. This comprehensive upgrade addresses emerging business needs, technological advances, and lessons learned from the initial system implementation.

Strategic Vision and Business Impact:
The ECMS v2.1 is designed to be the cornerstone of our digital transformation initiative, providing not just customer data management, but a complete customer experience ecosystem. The system incorporates predictive analytics, machine learning algorithms, and real-time personalization capabilities to drive business growth and customer satisfaction.

Key transformational improvements include:
• AI-powered customer insights and predictive analytics reducing churn by 40%
• Omnichannel integration providing seamless customer experience across all touchpoints
• Advanced automation reducing manual processes by 85% and operational costs by 50%
• Real-time personalization engine increasing customer engagement by 60%
• Microservices architecture enabling rapid feature deployment and third-party integrations
• Cloud-native design ensuring 99.99% availability with global scalability
• Advanced security framework with zero-trust architecture and compliance automation

The project encompasses a complete platform modernization with an estimated ROI of 300% over three years, supported by a $1.2M investment in cutting-edge technology and expert resources. The implementation timeline is 12 months with phased rollouts to minimize business disruption while maximizing value realization.");

                    // Enhanced Introduction
                    column.Item().PageBreak();
                    AddSection(column, "2. Introduction", @"
2.1 Document Evolution and Version History
This Software Requirements Specification v2.1 represents a comprehensive enhancement of the original CMS requirements, incorporating two years of operational experience, user feedback, and technological advancement. The document reflects our organization's maturity in customer relationship management and commitment to innovation leadership.

Version History:
• v1.0 (January 2024): Initial requirements for basic customer management
• v1.5 (June 2024): Minor enhancements based on user feedback
• v2.0 (August 2024): Major revision incorporating AI and analytics capabilities
• v2.1 (October 2024): Current version with enhanced security and compliance features

2.2 Business Context and Market Drivers
The enhanced system responds to rapidly evolving market conditions including:
• Increased customer expectations for personalized, omnichannel experiences
• Regulatory compliance requirements (GDPR, CCPA, SOX) demanding automated governance
• Competitive pressure requiring faster innovation cycles and time-to-market
• Digital transformation initiatives across all business units
• Remote workforce requirements necessitating cloud-native, mobile-first solutions

2.3 Technology Modernization Imperatives
The current technology landscape demands:
• Cloud-native architecture for scalability and cost optimization
• API-first design enabling ecosystem integration and innovation
• AI and machine learning capabilities for competitive advantage
• Real-time processing for immediate response to customer needs
• Advanced analytics for data-driven decision making");

                    // Enhanced Project Scope
                    column.Item().PageBreak();
                    AddSection(column, "3. Enhanced Project Scope and Objectives", @"
3.1 Strategic Business Objectives
The Enhanced Customer Management System v2.1 aligns with enterprise-wide digital transformation goals and positions the organization as an industry leader in customer experience innovation. The strategic objectives include:

Customer Experience Excellence:
• Achieve Net Promoter Score (NPS) of 70+ through personalized customer experiences
• Reduce average customer resolution time from 48 hours to 4 hours
• Increase customer lifetime value by 45% through predictive analytics and personalization
• Establish omnichannel consistency across all customer touchpoints
• Implement proactive customer service using predictive issue identification

Operational Excellence and Efficiency:
• Automate 85% of routine customer service tasks using AI and workflow automation
• Reduce operational costs by 50% through intelligent process optimization
• Achieve 99.99% system availability with cloud-native, resilient architecture
• Implement zero-downtime deployments enabling continuous feature delivery
• Establish real-time monitoring and self-healing system capabilities

3.2 Enhanced Functional Scope

Advanced Customer Intelligence:
• 360-degree customer view with real-time data synchronization across all systems
• Predictive customer lifetime value calculations using advanced algorithms
• Automated customer segmentation using machine learning clustering techniques
• Real-time sentiment analysis from social media, reviews, and communication channels
• Behavioral analytics predicting customer needs and preferences

3.3 Technical Architecture Evolution
The technical scope encompasses a complete modernization including:
• Microservices architecture with containerized deployment using Docker and Kubernetes
• Multi-region cloud deployment with automatic failover and disaster recovery
• Serverless computing for variable workloads and cost optimization
• Infrastructure as Code (IaC) for consistent, repeatable deployments");

                    // Enhanced Functional Requirements
                    column.Item().PageBreak();
                    AddSection(column, "5. Enhanced Functional Requirements", @"
5.1 Advanced User Authentication and Authorization

FR-001: Multi-Factor Authentication System
The system SHALL implement enterprise-grade multi-factor authentication including:
• Support for hardware security keys (FIDO2/WebAuthn standard)
• Time-based one-time passwords (TOTP) via authenticator applications
• SMS and email-based verification codes with rate limiting
• Biometric authentication for mobile devices (fingerprint, face recognition)
• Risk-based authentication with adaptive security policies
• Single sign-on (SSO) integration with SAML 2.0 and OAuth 2.0 providers

FR-002: Advanced Role-Based Access Control
The system SHALL provide granular, context-aware access control including:
• Dynamic role assignment based on user attributes and organizational hierarchy
• Time-based access permissions with automatic expiration
• Location-based access restrictions with geofencing capabilities
• Device-based access control with trusted device registration
• Fine-grained permissions at field and record level

5.2 Intelligent Customer Data Management

FR-003: AI-Powered Customer Profiling
The system SHALL provide advanced customer profiling capabilities including:
• Machine learning-based customer segmentation with automatic updates
• Predictive customer lifetime value calculations using historical data and behavior patterns
• Real-time customer sentiment analysis from all interaction channels
• Automated data enrichment from external sources
• Duplicate detection and merging using fuzzy matching algorithms

FR-004: Advanced Search and Discovery
The system SHALL provide intelligent search capabilities including:
• Natural language query processing with intent recognition
• Semantic search using vector embeddings and similarity matching
• Auto-complete and suggestion engine with personalized recommendations
• Visual search capabilities for image and document content");

                    // Advanced Non-Functional Requirements
                    column.Item().PageBreak();
                    AddSection(column, "6. Advanced Non-Functional Requirements", @"
6.1 Performance and Scalability Requirements

NFR-001: High-Performance Computing
The system SHALL meet enhanced performance requirements including:
• Sub-second response times for 95% of user interactions
• Support for 500+ concurrent users with linear scalability to 5,000+ users
• Processing capacity of 100,000+ transactions per hour during peak loads
• Real-time data processing with maximum 100ms latency for critical operations
• Auto-scaling capabilities responding to demand fluctuations within 30 seconds

NFR-002: Enterprise-Grade Availability
The system SHALL provide mission-critical availability including:
• 99.99% uptime target with maximum 52 minutes annual downtime
• Multi-region deployment with automatic failover capabilities
• Zero-downtime deployment processes for continuous feature delivery
• Disaster recovery with Recovery Time Objective (RTO) of 15 minutes

6.2 Advanced Security Framework

NFR-003: Zero-Trust Security Architecture
The system SHALL implement comprehensive security measures including:
• End-to-end encryption using AES-256 for data at rest and TLS 1.3 for data in transit
• Zero-trust network architecture with microsegmentation and identity verification
• Advanced threat detection using machine learning and behavioral analytics
• Automated security scanning and vulnerability management
• Data loss prevention (DLP) with content inspection and policy enforcement

NFR-004: Privacy and Compliance
The system SHALL ensure comprehensive privacy protection including:
• GDPR compliance with automated data subject rights management
• CCPA compliance with consumer privacy request processing
• Data residency controls with geographic data sovereignty
• Privacy by design principles embedded in all system components");
                });

                page.Footer()
                    .Height(30)
                    .Background(Colors.Blue.Lighten4)
                    .Padding(10)
                    .Row(row =>
                    {
                        row.RelativeItem().Text("Enhanced Customer Management System v2.1 - Requirements Specification").FontSize(8);
                        row.ConstantItem(50).AlignRight().Text(text =>
                        {
                            text.CurrentPageNumber().FontSize(8);
                            text.Span(" of ").FontSize(8);
                            text.TotalPages().FontSize(8);
                        });
                    });
            });
        });

        document.GeneratePdf("SoftwareSpec_v2.pdf");
    }

    static void AddSection(ColumnDescriptor column, string title, string content)
    {
        column.Item().PaddingTop(15).Column(sectionColumn =>
        {
            sectionColumn.Item().Text(title).Bold().FontSize(12).FontColor(Colors.Blue.Darken1);
            sectionColumn.Item().PaddingTop(8).Text(content.Trim()).FontSize(10).LineHeight(1.3f).Justify();
        });
    }
}