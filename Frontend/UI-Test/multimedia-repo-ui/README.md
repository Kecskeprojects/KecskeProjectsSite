# Multimedia Repository Web UI

A dark-themed web interface for multimedia servicing and file repository management.

## Features

- Dark color palette with darker blue accents
- File upload with drag-and-drop support
- File repository listing
- Integrated media player for videos, audio, and images
- Local storage for file persistence (demo purposes)

## Usage

1. Open `index.html` in a web browser.
2. Upload files using the upload area (drag and drop or click to select).
3. View uploaded files in the repository section.
4. Click "Play/View" to display media in the player.

## Color Palette

- Primary Background: #121212
- Secondary Background: #1e1e1e
- Accent Blue: #1a237e
- Light Text: #ffffff
- Muted Text: #b0b0b0

## Technologies

- HTML5
- CSS3
- JavaScript (ES6)

## Limitations

This is a frontend-only demo. Files are stored temporarily using localStorage and File API object URLs. For a production file repository, a backend server is required.

## Troubleshooting

- Ensure JavaScript is enabled in your browser.
- For large files, browser limitations may apply.
- Files are not actually stored on a server; refresh may clear object URLs.
