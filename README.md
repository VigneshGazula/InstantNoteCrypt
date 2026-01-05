# 🔐 Instant Note Crypt

Instant Note Crypt is a lightweight, secure note-sharing web application inspired by platforms like CodedPad. It allows users to instantly create, store, and access notes using a unique code — without the need for accounts or authentication.

## 🚀 Features

- 🔑 **Code-Based Notes**
  - Each note is identified by a unique code.
  - Entering a new code creates an empty note.
  - Re-entering the same code retrieves the saved content.

- 📝 **Instant Editing**
  - Write, edit, and save notes in real time.
  - Notes are persistently stored and accessible anytime.

- 🔐 **Secondary Lock (Optional)**
  - Users can enable an additional **4-digit PIN** for security.
  - PIN verification is required before accessing protected notes.

- 🧭 **Separate Views**
  - Code entry screen
  - Note editor screen
  - PIN verification screen (if enabled)

- ⚡ **Simple & Fast**
  - No login or signup required.
  - Minimal UI focused on usability and speed.

## 🛠️ Tech Stack

- **Backend:** ASP.NET Core  
- **Frontend:** Razor Pages / MVC Views  
- **Database:** SQL Server  
- **ORM:** Entity Framework Core  

## 🗃️ How It Works

1. User enters a **code**
2. System checks the database:
   - If the code does not exist → a new empty note is created
   - If the code exists → the saved note is loaded
3. If a PIN is set:
   - User must verify the PIN
4. User edits the note
5. Changes are saved and linked to the same code

## 🔒 Security Notes

- The 4-digit PIN is currently stored in **plain text** (development stage).
- Planned security improvements:
  - PIN hashing and encryption
  - Secure access policies
  - Expiry-based notes

## 🚧 Future Enhancements

- 🔐 Encrypted PIN storage
- ⏳ Note expiration feature
- 👁️ Read-only or shareable links
- 🌙 Dark mode
- 📱 Responsive mobile UI
- 🕒 Version history for notes

## 🏗️ Setup & Installation

```bash
# Clone the repository
git clone https://github.com/your-username/instant-note-crypt.git

# Navigate to the project directory
cd instant-note-crypt

# Restore dependencies
dotnet restore

# Apply database migrations
dotnet ef database update

# Run the application
dotnet run
