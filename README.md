# Document Comparison System

A professional PDF document comparison system with AI-powered insights using Azure OpenAI. This application provides intelligent document analysis with a modern, clean interface.

![Document Comparison System](https://img.shields.io/badge/Status-Active-green)
![.NET 9](https://img.shields.io/badge/.NET-9.0-blue)
![React](https://img.shields.io/badge/React-18.3.0-blue)
![TypeScript](https://img.shields.io/badge/TypeScript-4.9.5-blue)

## 🚀 Features

### Core Functionality
- **PDF Document Comparison**: Upload and compare two PDF documents
- **Intelligent Text Extraction**: Advanced PDF text extraction using iText7
- **Side-by-Side Analysis**: Detailed segment-by-segment comparison view
- **Page-Aware Diff**: Maintains page number information for precise comparison
- **Professional UI**: Clean, corporate design without distracting elements

### AI-Powered Insights
- **Azure OpenAI Integration**: Intelligent document analysis with GPT
- **Comprehensive Summaries**: AI-generated overviews of document changes
- **Key Changes Detection**: Automated identification of significant modifications
- **Smart Recommendations**: AI-powered suggestions for document review
- **Impact Assessment**: Analysis of change significance and business impact

### Advanced Features
- **Real-time Comparison**: Fast document processing and analysis
- **Export Functionality**: Generate detailed PDF reports of comparisons
- **Severity Classification**: Categorize changes by importance (Low/Medium/High)
- **Page Navigation**: Jump between pages in the detailed analysis view
- **Professional Styling**: Clean, corporate interface design

## 🏗️ Architecture

### Backend (.NET 9)
- **ASP.NET Core Web API**: RESTful API with minimal endpoints
- **iText7**: Professional PDF text extraction and manipulation
- **Azure OpenAI**: GPT-powered document analysis
- **DiffPlex**: Advanced text comparison algorithms
- **QuestPDF**: PDF report generation

### Frontend (React + TypeScript)
- **React 18**: Modern React with hooks and functional components
- **TypeScript**: Type-safe development
- **Framer Motion**: Smooth animations and transitions
- **Professional Design**: Clean, corporate styling

## 📋 Prerequisites

- **.NET 9.0 SDK** or later
- **Node.js 16+** and npm
- **Azure OpenAI Account** (for AI features)
- **Git** for version control

## 🛠️ Installation

### 1. Clone the Repository
```bash
git clone https://github.com/yourusername/document-comparison-system.git
cd document-comparison-system
```

### 2. Backend Setup
```bash
cd backend
dotnet restore
dotnet build
```

### 3. Frontend Setup
```bash
cd frontend
npm install
```

### 4. Environment Configuration

**Important**: Never commit your actual API keys to version control.

1. Copy the environment template:
```bash
cp .env.example .env
```

2. Edit the `.env` file with your actual Azure OpenAI credentials:
```
AzureOpenAI__Endpoint=https://your-resource-name.openai.azure.com/
AzureOpenAI__ApiKey=your-actual-api-key-here
AzureOpenAI__DeploymentName=gpt-4
```

Alternatively, you can set these as system environment variables or update `appsettings.json` (but never commit real keys):

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "AzureOpenAI": {
    "Endpoint": "your-azure-openai-endpoint",
    "ApiKey": "your-api-key",
    "DeploymentName": "your-deployment-name"
  }
}
```

## 🚀 Running the Application

### Start Backend
```bash
cd backend
dotnet run
```
The API will be available at `http://localhost:5000`

### Start Frontend
```bash
cd frontend
npm start
```
The application will be available at `http://localhost:3000`

## 📖 Usage

1. **Upload Documents**: Select two PDF documents using the upload interface
2. **Compare**: Click "Compare Documents" to start the analysis
3. **Review Results**: View similarity scores, AI insights, and key differences
4. **Detailed Analysis**: Click "Detailed Analysis" for segment-by-segment comparison
5. **Export Report**: Generate and download a comprehensive PDF report

## 🔧 API Endpoints

### Document Comparison
```http
POST /compare
Content-Type: multipart/form-data

Form Data:
- pdf1: PDF file
- pdf2: PDF file
```

### Export Report
```http
POST /export
Content-Type: application/json

{
  "summary": "string",
  "similarityScore": 0.85,
  "diffSegments": [...],
  "aiInsights": {...}
}
```

### Health Check
```http
GET /health
```

## 🎨 Professional Styling

The application features a clean, corporate design with:
- Professional color scheme (blues, grays, whites)
- Clean typography using system fonts
- Consistent spacing and layout
- Accessible contrast ratios
- Responsive design for all screen sizes

## 🤖 AI Integration

The system integrates with Azure OpenAI to provide:
- **Document Summaries**: Overview of changes between versions
- **Key Changes**: Highlighted important modifications
- **Recommendations**: Suggestions for document review process
- **Impact Analysis**: Assessment of change significance

## 🧪 Testing

### Backend Tests
```bash
cd backend
dotnet test
```

### Frontend Tests
```bash
cd frontend
npm test
```

## 📦 Building for Production

### Backend
```bash
cd backend
dotnet publish -c Release -o out
```

### Frontend
```bash
cd frontend
npm run build
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- **iText7** for professional PDF processing
- **Azure OpenAI** for intelligent document analysis
- **DiffPlex** for advanced text comparison
- **React & TypeScript** for modern frontend development
- **Framer Motion** for smooth animations

## 📞 Support

For support, email your-email@example.com or create an issue in this repository.

---

**Built with ❤️ for professional document analysis**

---

Need a deeper dive? See the detailed step-by-step walkthrough:

- docs/PROJECT_OVERVIEW.md