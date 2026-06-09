// Multimedia Repository App
document.addEventListener("DOMContentLoaded", function () {
  const fileInput = document.getElementById("file-input");
  const uploadBtn = document.getElementById("upload-btn");
  const uploadModalBtn = document.getElementById("upload-modal-btn");
  const fileList = document.getElementById("file-list");
  const videoPlayer = document.getElementById("video-player");
  const audioPlayer = document.getElementById("audio-player");
  const imageViewer = document.getElementById("image-viewer");

  const uploadModal = document.getElementById("upload-modal");
  const mediaModal = document.getElementById("media-modal");
  const closeButtons = document.querySelectorAll(".close");

  let uploadedFiles = JSON.parse(localStorage.getItem("uploadedFiles")) || [];

  // Add example files if none exist
  if (uploadedFiles.length === 0) {
    uploadedFiles = [
      {
        name: "sample_video.mp4",
        type: "video/mp4",
        url: "https://www.w3schools.com/html/mov_bbb.mp4",
      },
      {
        name: "sample_audio.mp3",
        type: "audio/mpeg",
        url: "https://www.w3schools.com/html/horse.mp3",
      },
      {
        name: "sample_image.jpg",
        type: "image/jpeg",
        url: "https://www.w3schools.com/html/img_girl.jpg",
      },
    ];
    localStorage.setItem("uploadedFiles", JSON.stringify(uploadedFiles));
  }

  // Load existing files
  displayFiles();

  // Open upload modal
  uploadModalBtn.addEventListener("click", function () {
    uploadModal.style.display = "block";
  });

  // Close modals
  closeButtons.forEach((btn) => {
    btn.addEventListener("click", function () {
      uploadModal.style.display = "none";
      mediaModal.style.display = "none";
    });
  });

  // Close modal when clicking outside
  window.addEventListener("click", function (event) {
    if (event.target === uploadModal) {
      uploadModal.style.display = "none";
    }
    if (event.target === mediaModal) {
      mediaModal.style.display = "none";
    }
  });

  // Upload button
  uploadBtn.addEventListener("click", function () {
    const files = fileInput.files;
    if (files.length > 0) {
      for (let file of files) {
        uploadedFiles.push({
          name: file.name,
          type: file.type,
          url: URL.createObjectURL(file),
        });
      }
      localStorage.setItem("uploadedFiles", JSON.stringify(uploadedFiles));
      displayFiles();
      fileInput.value = "";
      uploadModal.style.display = "none";
    }
  });

  // Drag and drop in modal
  const uploadArea = document.querySelector(".upload-area");
  uploadArea.addEventListener("dragover", function (e) {
    e.preventDefault();
    uploadArea.style.borderColor = "#1a237e";
  });
  uploadArea.addEventListener("dragleave", function (e) {
    e.preventDefault();
    uploadArea.style.borderColor = "#333333";
  });
  uploadArea.addEventListener("drop", function (e) {
    e.preventDefault();
    uploadArea.style.borderColor = "#333333";
    const files = e.dataTransfer.files;
    for (let file of files) {
      uploadedFiles.push({
        name: file.name,
        type: file.type,
        url: URL.createObjectURL(file),
      });
    }
    localStorage.setItem("uploadedFiles", JSON.stringify(uploadedFiles));
    displayFiles();
    uploadModal.style.display = "none";
  });

  function displayFiles() {
    fileList.innerHTML = "";
    uploadedFiles.forEach((file, index) => {
      const fileItem = document.createElement("div");
      fileItem.className = "file-item";
      fileItem.innerHTML = `
                <p>${file.name}</p>
                <button onclick="playMedia(${index})">Play/View</button>
                <button onclick="deleteFile(${index})">Delete</button>
            `;
      fileList.appendChild(fileItem);
    });
  }

  window.playMedia = function (index) {
    const file = uploadedFiles[index];
    if (file.type.startsWith("video/")) {
      videoPlayer.src = file.url;
      videoPlayer.style.display = "block";
      audioPlayer.style.display = "none";
      imageViewer.style.display = "none";
    } else if (file.type.startsWith("audio/")) {
      audioPlayer.src = file.url;
      audioPlayer.style.display = "block";
      videoPlayer.style.display = "none";
      imageViewer.style.display = "none";
    } else if (file.type.startsWith("image/")) {
      imageViewer.src = file.url;
      imageViewer.style.display = "block";
      videoPlayer.style.display = "none";
      audioPlayer.style.display = "none";
    }
    mediaModal.style.display = "block";
  };

  window.deleteFile = function (index) {
    uploadedFiles.splice(index, 1);
    localStorage.setItem("uploadedFiles", JSON.stringify(uploadedFiles));
    displayFiles();
  };
});
