# 📝 Form Maker (Form Crafter)

A modern, accessible, drag-and-drop form builder designed for creating business and legal documents with ease. Built with **Blazor WebAssembly** and **.NET 8**.

![Form Maker Screenshot](https://raw.githubusercontent.com/kriss2012/form-crafter/main/FormMaker.Client/wwwroot/icon-512.png)

## 🚀 Live Demo
- **Frontend**: [https://kriss2012.github.io/form-crafter/](https://kriss2012.github.io/form-crafter/)
- **Backend API**: [https://formmaker-api.onrender.com/api/](https://formmaker-api.onrender.com/api/) (hosted on Render.com)

## ✨ Features
- **Drag & Drop Workspace**: Intuitive canvas for placing and arranging form elements.
- **Rich Element Library**: Text inputs, labels, checkboxes, images, date pickers, dropdowns, and even signature pads.
- **Accessibility First**: Designed with WCAG 2.1 AA guidelines in mind, featuring high contrast, large targets, and screen reader support.
- **Real-time Preview**: Switch between Desktop, Tablet, and Mobile views instantly.
- **Cloud Sync**: All forms are saved to a secure PostgreSQL database (via Render and Neon.tech).
- **Undo/Redo Support**: Full history management for all your design changes.

## 🛠️ Technology Stack
- **Frontend**: Blazor WebAssembly (.NET 8)
- **UI Framework**: MudBlazor
- **Backend**: Azure Functions (Isolated Worker)
- **Database**: PostgreSQL (via Neon.tech)
- **Infrastructure**: Docker, GitHub Actions, Render.com

## 💻 Local Development

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js](https://nodejs.org/) (for some optional build tools)
- Visual Studio 2022 or VS Code

### Running the App
1. Clone the repository:
   ```bash
   git clone https://github.com/kriss2012/form-crafter.git
   cd form-crafter
   ```
2. Run the API:
   ```bash
   cd FormMaker.Api
   dotnet run
   ```
3. Run the Client:
   ```bash
   cd ../FormMaker.Client
   dotnet run
   ```

## 📄 Documentation
- [Deployment Guide](DEPLOYMENT-RENDER.md) - How to host your own version.
- [Database Setup](DEPLOYMENT-RENDER.md#database-setup) - Configuring PostgreSQL.

## 🤝 Contributing
Contributions are welcome! Please feel free to submit a Pull Request.

## 📜 License
This project is licensed under the MIT License.
