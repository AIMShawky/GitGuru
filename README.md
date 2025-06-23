# GitGuru

A C# command-line tool to automate cherry-picking multiple Git revisions safely and efficiently.

## 🚀 Features

- Reads a predefined list of Git commit hashes.
- Resets the working directory using `git reset --hard`.
- Pulls the latest changes with `git pull`.
- Cherry-picks each revision one by one:
  - If a conflict occurs, the cherry-pick is aborted and the commit is added to an error list.
  - If successful, the changes are immediately pushed using `git push`.
- At the end, a summary is printed showing successful and failed cherry-picks.

---

## 🛠 Requirements

- [.NET SDK](https://dotnet.microsoft.com/download) (.NET 8 or newer)
- Git must be installed and available in the system's PATH

---

## 📦 Setup & Build

1. Clone or download this repository.
2. Navigate to the folder containing the project.

```bash
cd GitGuru
