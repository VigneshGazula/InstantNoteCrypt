🔐 Instant Note Crypt

Instant Note Crypt is a lightweight, secure note-sharing web application inspired by platforms like CodedPad. It allows users to instantly create, store, and access notes using a unique code — without the need for accounts or authentication.

🚀 Features

🔑 Code-Based Access

Each note is identified by a unique code.

Entering a new code creates a fresh empty note.

Re-entering an existing code retrieves the stored content.

📝 Instant Note Editing

Write, update, and save text seamlessly.

Content is persisted and accessible anytime using the same code.

🔐 Secondary Lock (Optional)

Users can secure their note with an additional 4-digit PIN.

PIN protection adds an extra layer of privacy.

🧭 Separate Views

Dedicated views for:

Code entry

Note editing

PIN verification (if enabled)

⚡ Minimal & Fast

No login, no signup.

Clean UI focused on speed and simplicity.

🛠️ Tech Stack

Backend: ASP.NET Core

Frontend: Razor Pages / MVC Views

Database: SQL Server

ORM: Entity Framework Core

🗃️ Project Workflow

User enters a code

System checks if the code exists:

❌ Not found → Creates a new empty note

✅ Found → Loads the existing note

If PIN is enabled:

User must enter the correct PIN to proceed

User can write or update the note

Content is saved and mapped to the same code

🔒 Security Notes

Currently, the 4-digit PIN is stored in plain text (development phase).

Future enhancements include:

PIN hashing & encryption

Expiry-based notes

Read-only sharing links

🧩 Future Enhancements

🔐 Encrypted PIN storage

⏳ Auto-expiring notes

👁️ Read-only / shareable links

🌙 Dark mode

📱 Mobile-friendly UI

📜 Version history for notes

🏗️ Setup & Installation
# Clone the repository
git clone https://github.com/your-username/instant-note-crypt.git

# Navigate to the project directory
cd instant-note-crypt

# Restore dependencies
dotnet restore

# Apply migrations
dotnet ef database update

# Run the application
dotnet run

📄 License

This project is open-source and available under the MIT License.

🤝 Contribution

Contributions are welcome!
Feel free to fork the repository, open issues, or submit pull requests.

✨ Author

Vignesh Gazula
ASP.NET Core Developer | Problem Solver | Builder
