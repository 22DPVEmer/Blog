// Add total size tracking
import { Constants } from "./constants.js";

let featuredImageSize = 0; // For featured image only
let contentImagesSize = 0; // Track content images size separately
let contentImageMap = new Map(); // Track each content image by URL and size
const MAX_TOTAL_SIZE = Constants.LIMITS.MAX_TOTAL_SIZE;
const MAX_CONTENT_IMAGES_SIZE = Constants.LIMITS.MAX_CONTENT_IMAGES_SIZE;
let contentSizeExceeded = false;
let contentEmpty = false; // Set default to false to not block initial render

// Update featured image size display
function updateSizeDisplay() {
  const sizeMB = (featuredImageSize / 1024 / 1024).toFixed(2);
  const percentage = ((featuredImageSize / MAX_TOTAL_SIZE) * 100).toFixed(1);

  document.getElementById("currentSize").textContent = sizeMB;
  const progressBar = document.getElementById("sizeProgress");
  progressBar.style.width = percentage + "%";
  progressBar.setAttribute("aria-valuenow", percentage);
  progressBar.textContent = percentage + "%";

  // Update progress bar color based on usage
  if (percentage > 90) {
    progressBar.classList.remove("bg-success", "bg-warning");
    progressBar.classList.add("bg-danger");
  } else if (percentage > 70) {
    progressBar.classList.remove("bg-success", "bg-danger");
    progressBar.classList.add("bg-warning");
  } else {
    progressBar.classList.remove("bg-warning", "bg-danger");
    progressBar.classList.add("bg-success");
  }
  
  updateSubmitButtonState();
}

// Check content image size and update validation state
function updateContentImageSizeStatus() {
  // Calculate total content image size
  let totalContentSize = 0;
  contentImageMap.forEach((size) => {
    totalContentSize += size;
  });
  
  contentImagesSize = totalContentSize;
  
  // Update exceeded flag
  if (contentImagesSize > MAX_CONTENT_IMAGES_SIZE) {
    contentSizeExceeded = true;
    showError(Constants.MESSAGES.CONTENT_IMAGES_LIMIT_EXCEEDED);
  } else {
    contentSizeExceeded = false;
  }
  
  // Debug info
  console.log(`Content images total size: ${(contentImagesSize / 1024 / 1024).toFixed(2)}MB`);
  console.log(`Number of tracked images: ${contentImageMap.size}`);
  
  updateSubmitButtonState();
}

// Function to update submit button state
function updateSubmitButtonState() {
  const submitButton = document.getElementById("submitButton");
  
  if (contentSizeExceeded) { // Removed contentEmpty check
    submitButton.disabled = true;
    if (contentSizeExceeded) {
      showError(Constants.MESSAGES.CONTENT_IMAGES_LIMIT_EXCEEDED);
    }
    submitButton.classList.add("disabled");
  } else {
    submitButton.disabled = false;
    submitButton.classList.remove("disabled");
  }
}

// Initialize drag and drop functionality
const dragDropZone = document.getElementById("dragDropZone");
const fileInput = document.getElementById("FeaturedImage");
const imagePreview = document.getElementById("imagePreview");

["dragenter", "dragover", "dragleave", "drop"].forEach((eventName) => {
  dragDropZone.addEventListener(eventName, preventDefaults, false);
});

function preventDefaults(e) {
  e.preventDefault();
  e.stopPropagation();
}

["dragenter", "dragover"].forEach((eventName) => {
  dragDropZone.addEventListener(eventName, highlight, false);
});

["dragleave", "drop"].forEach((eventName) => {
  dragDropZone.addEventListener(eventName, unhighlight, false);
});

function highlight(e) {
  dragDropZone.classList.add("dragover");
}

function unhighlight(e) {
  dragDropZone.classList.remove("dragover");
}

dragDropZone.addEventListener("drop", handleDrop, false);
dragDropZone.addEventListener("click", () => fileInput.click());

function handleDrop(e) {
  const dt = e.dataTransfer;
  const file = dt.files[0];
  handleFile(file);
}

fileInput.addEventListener("change", function (e) {
  handleFile(e.target.files[0]);
});

function handleFile(file) {
  if (!file) return;

  if (file.size > Constants.LIMITS.MAX_IMAGE_SIZE) {
    showError(Constants.MESSAGES.IMAGE_SIZE_LIMIT);
    return;
  }

  if (!Constants.IMAGE_TYPES.includes(file.type)) {
    showError(Constants.MESSAGES.INVALID_IMAGE_TYPE);
    return;
  }

  // Update featured image size only
  featuredImageSize = file.size;
  updateSizeDisplay();

  const reader = new FileReader();
  reader.onload = function (e) {
    imagePreview.src = e.target.result;
    imagePreview.style.display = "block";
    dragDropZone.style.display = "none";
    document.querySelector(".image-actions").style.display = "flex";
  };
  reader.readAsDataURL(file);

  // Update the file input
  const dT = new DataTransfer();
  dT.items.add(file);
  fileInput.files = dT.files;
}

function showError(message) {
  const errorDiv = document.getElementById("errorMessages");
  errorDiv.textContent = message;
  errorDiv.style.display = "block";
  setTimeout(() => {
    errorDiv.style.display = "none";
  }, 3000);
}

// Summary character count
const summaryTextarea = document.getElementById("Intro");
const charCount = document.getElementById("introCharCount");

summaryTextarea.addEventListener("input", function () {
  const count = this.value.length;
  charCount.textContent = count;

  if (count > Constants.LIMITS.INTRO_MAX_LENGTH - 10) {
    charCount.style.color = "#dc3545";
  } else if (count > Constants.LIMITS.INTRO_MAX_LENGTH - 20) {
    charCount.style.color = "#ffc107";
  } else {
    charCount.style.color = "#6c757d";
  }
});

// Initialize TinyMCE
tinymce.init({
  selector: "#Content",
  plugins:
    "anchor autolink charmap codesample emoticons image link lists media searchreplace table visualblocks wordcount checklist mediaembed casechange export formatpainter pageembed permanentpen footnotes advtemplate advtable advcode editimage tableofcontents powerpaste tinymcespellchecker autocorrect typography inlinecss",
  toolbar:
    "undo redo | blocks fontfamily fontsize | bold italic underline strikethrough | link image media table | align lineheight | checklist numlist bullist indent outdent | emoticons charmap | removeformat",
  height: 500,
  menubar: true,
  image_advtab: true,
  image_caption: true,
  image_dimensions: false,
  image_class_list: [
    { title: "Responsive", value: "img-fluid" },
    { title: "Full Width", value: "img-fluid w-100" },
    { title: "Centered", value: "img-fluid mx-auto d-block" },
  ],
  file_picker_types: "image",
  images_upload_handler: async function (blobInfo, progress) {
    return new Promise((resolve, reject) => {
      const file = blobInfo.blob();
      
      // Generate a temporary ID for this upload
      const tempId = "temp_" + Date.now();
      
      // Check if adding this image would exceed the limit
      const potentialTotalSize = contentImagesSize + file.size;
      if (potentialTotalSize > MAX_CONTENT_IMAGES_SIZE) {
        contentSizeExceeded = true;
        updateSubmitButtonState();
        showError(Constants.MESSAGES.CONTENT_IMAGES_LIMIT_EXCEEDED);
        reject(Constants.MESSAGES.CONTENT_IMAGES_LIMIT_EXCEEDED);
        return;
      }

      if (file.size > Constants.LIMITS.MAX_IMAGE_SIZE) {
        reject(Constants.MESSAGES.IMAGE_SIZE_LIMIT);
        return;
      }

      if (!Constants.IMAGE_TYPES.includes(file.type)) {
        reject(Constants.MESSAGES.INVALID_IMAGE_TYPE);
        return;
      }

      const formData = new FormData();
      formData.append("file", file);

      const xhr = new XMLHttpRequest();
      xhr.open("POST", "/Articles/UploadImage");

      const token = document.querySelector(
        'input[name="__RequestVerificationToken"]'
      ).value;
      xhr.setRequestHeader("RequestVerificationToken", token);

      xhr.upload.onprogress = (e) => {
        const percent = (e.loaded / e.total * 100).toFixed(1);
        progress(percent);
      };
      
      xhr.onerror = function() {
        reject('Image upload failed due to a network error.');
      };

      xhr.onload = function () {
        if (xhr.status === 200) {
          const response = JSON.parse(xhr.responseText);
          const imageUrl = response.imageUrl;
          
          // Add this image to our tracking map with its size
          contentImageMap.set(imageUrl, file.size);
          
          // Update content image size tracking
          updateContentImageSizeStatus();
          
          resolve(imageUrl);
        } else {
          try {
            const response = JSON.parse(xhr.responseText);
            reject(response.message || "HTTP Error: " + xhr.status);
          } catch {
            reject("HTTP Error: " + xhr.status);
          }
        }
      };

      xhr.send(formData);
    });
  },
  paste_data_images: true,
  automatic_uploads: true,
  images_reuse_filename: false,
  setup: function (editor) {
    editor.on("init", function() {
      // Register a post processor to track and count images after init
      if (editor.getContent()) {
        scanContentForImages(editor);
      }
    });
    
    editor.on("change", function () {
      editor.save();
      
      // Update tracking but don't block the editor
      scanContentForImages(editor);
    });

    editor.on("paste", function (e) {
      const items = (e.clipboardData || e.originalEvent.clipboardData).items;
      for (let i = 0; i < items.length; i++) {
        if (items[i].type.indexOf("image") !== -1) {
          const file = items[i].getAsFile();
          
          // Check if adding this image would exceed the content image limit
          const potentialTotalSize = contentImagesSize + file.size;
          if (potentialTotalSize > MAX_CONTENT_IMAGES_SIZE) {
            contentSizeExceeded = true;
            updateSubmitButtonState();
            editor.notificationManager.open({
              text: Constants.MESSAGES.CONTENT_IMAGES_LIMIT_EXCEEDED,
              type: "error",
            });
            showError(Constants.MESSAGES.CONTENT_IMAGES_LIMIT_EXCEEDED);
            e.preventDefault();
            return;
          }
          
          if (file.size > Constants.LIMITS.MAX_IMAGE_SIZE) {
            editor.notificationManager.open({
              text: Constants.MESSAGES.IMAGE_SIZE_LIMIT,
              type: "error",
            });
            e.preventDefault();
            return;
          }
          
          // Will be tracked in the upload handler
        }
      }
    });
    
    // Track when images are removed
    editor.on("SetContent", function() {
      // After content is set, scan for images
      scanContentForImages(editor);
    });
    
    // This catches both adding and removing content including images
    editor.on("NodeChange", function(e) {
      // Slight delay to ensure the DOM has updated
      setTimeout(() => {
        scanContentForImages(editor);
      }, 100);
    });
  },
});

// Scan the editor content for images and update tracking
function scanContentForImages(editor) {
  // Get all images in the editor
  const images = editor.dom.select('img');
  
  // Create a set of current image URLs
  const currentImageUrls = new Set();
  
  // Add to the set any images found now
  images.forEach(img => {
    const src = img.getAttribute('src');
    if (src) {
      currentImageUrls.add(src);
      
      // If we don't have this image's size yet, estimate it
      if (!contentImageMap.has(src) && src.startsWith('data:image')) {
        // For data URLs, estimate size based on length
        const estimatedSize = Math.ceil(src.length * 0.75); // Base64 is ~33% larger than binary
        contentImageMap.set(src, estimatedSize);
        console.log(`Estimated size for data URL image: ${Math.round(estimatedSize/1024)}KB`);
      } else if (!contentImageMap.has(src)) {
        // For URLs we don't know yet, set a default size and try to fetch it
        contentImageMap.set(src, 1024 * 200); // Default 200KB for unknown images
        
        // Try to fetch the image to get its actual size
        if (src.startsWith('http')) {
          fetch(src, { method: 'HEAD' })
            .then(response => {
              if (response.ok) {
                const size = parseInt(response.headers.get('content-length') || '0');
                if (size > 0) {
                  contentImageMap.set(src, size);
                  updateContentImageSizeStatus();
                }
              }
            })
            .catch(err => console.warn('Could not get image size', err));
        }
      }
    }
  });
  
  // Remove images from our map that are no longer in the editor
  const imagesToRemove = [];
  contentImageMap.forEach((size, url) => {
    if (!currentImageUrls.has(url)) {
      imagesToRemove.push(url);
    }
  });
  
  imagesToRemove.forEach(url => {
    contentImageMap.delete(url);
  });
  
  // Update content image size status
  updateContentImageSizeStatus();
}

// Form submission
document.getElementById("createArticleForm").addEventListener("submit", async function (e) {
  e.preventDefault();
  
  // Check for any content image size issues before submission
  updateContentImageSizeStatus();
  
  // Reset validation state
  resetValidationState();
  
  // Validate Content specifically
  const contentEditor = tinymce.get("Content");
  
  // Check for empty content on submission but don't block editor initialization
  if (!contentEditor || !contentEditor.getContent().trim()) {
    showError(Constants.MESSAGES.CONTENT_REQUIRED);
    return;
  }
  
  // Double-check content images size before submission
  if (contentSizeExceeded) {
    showError(Constants.MESSAGES.CONTENT_IMAGES_LIMIT_EXCEEDED);
    return;
  }
  
  // Continue with form submission if validation passes
  submitForm();
});

// Initialize when DOM is loaded
document.addEventListener("DOMContentLoaded", function () {
  // Initialize content validation on load
  setTimeout(() => {
    const contentEditor = tinymce.get("Content");
    if (contentEditor) {
      // Just scan for images without checking content empty status
      scanContentForImages(contentEditor);
    }
  }, 1000); // Small delay to ensure TinyMCE is fully loaded

  // If in edit mode and there's an initial image, update the size tracking
  if (window.isEditMode && window.initialFeaturedImage) {
    // Show the image preview and actions
    imagePreview.style.display = "block";
    dragDropZone.style.display = "none";
    document.querySelector(".image-actions").style.display = "flex";

    // Fetch the image size and update total size
    if (window.initialFeaturedImage.startsWith("http")) {
      fetch(window.initialFeaturedImage)
        .then((response) => {
          if (response.ok) {
            const size = parseInt(
              response.headers.get("content-length") || "0"
            );
            featuredImageSize = size;
            updateSizeDisplay();
          }
        })
        .catch((error) => {
          console.warn("Could not fetch image size:", error);
          // Set a default size for local images
          featuredImageSize = 1024 * 1024; // 1MB default
          updateSizeDisplay();
        });
    } else {
      // For local images, set a default size
      featuredImageSize = 1024 * 1024; // 1MB default
      updateSizeDisplay();
    }
  }

  // Add remove image button functionality
  document
    .getElementById("removeImageBtn")
    .addEventListener("click", function () {
      document.getElementById("dragDropZone").style.display = "block";
      document.getElementById("imagePreview").style.display = "none";
      document.querySelector(".image-actions").style.display = "none";
      document.getElementById("FeaturedImage").value = "";
      imagePreview.src = "#";
      featuredImageSize = 0; // Reset featured image size
      updateSizeDisplay();
    });

  // Update change image button functionality
  document
    .getElementById("changeImageBtn")
    .addEventListener("click", function () {
      document.getElementById("dragDropZone").style.display = "block";
      document.getElementById("imagePreview").style.display = "none";
      document.querySelector(".image-actions").style.display = "none";
    });
});

// Reset validation state for all inputs
function resetValidationState() {
  const form = document.getElementById("createArticleForm");
  const inputs = form.querySelectorAll("input, textarea");
  
  inputs.forEach(input => {
    input.classList.remove("is-invalid");
    const feedback = input.nextElementSibling;
    if (feedback && feedback.classList.contains("invalid-feedback")) {
      feedback.textContent = "";
    }
  });
}

// Submit the form data
async function submitForm() {
  const form = document.getElementById("createArticleForm");
  const formData = new FormData(form);
  
  // Add the content from TinyMCE
  const contentEditor = tinymce.get("Content");
  formData.set("Content", contentEditor.getContent());
  
  const submitButton = document.getElementById("submitButton");
  const errorDiv = document.getElementById("errorMessages");
  const successDiv = document.getElementById("successMessage");
  const featuredImageFile = document.getElementById("FeaturedImage").files[0];
  
  try {
    submitButton.disabled = true;
    errorDiv.style.display = "none";
    successDiv.style.display = "none";
    
    // Handle featured image if present
    if (featuredImageFile) {
      formData.set("FeaturedImageFile", featuredImageFile);
    }
    
    // Get article ID if in edit mode
    const articleId = document.getElementById("articleId")?.value;
    const endpoint = window.isEditMode
      ? `/Articles/Edit/${articleId}`
      : "/Articles/Create";
    
    const response = await fetch(endpoint, {
      method: "POST",
      headers: {
        "RequestVerificationToken": document.querySelector('input[name="__RequestVerificationToken"]').value,
        "Accept": "application/json"
      },
      body: formData
    });
    
    if (response.ok) {
      // Show success message
      successDiv.textContent = window.isEditMode
        ? Constants.MESSAGES.ARTICLE_UPDATED
        : Constants.MESSAGES.ARTICLE_CREATED;
      successDiv.style.display = "block";
      
      // Redirect to the articles list after a delay
      setTimeout(() => {
        window.location.href = "/Articles";
      }, 1500);
    } else {
      let errorMessage = Constants.MESSAGES.PROCESS_ERROR;
      
      try {
        // Check if response is JSON before parsing
        const contentType = response.headers.get("content-type");
        if (contentType && contentType.includes("application/json")) {
          const responseData = await response.json();
          if (response.status === 400 && responseData.errors) {
            // Show validation errors
            for (const field in responseData.errors) {
              const inputField = document.getElementById(field);
              if (inputField) {
                inputField.classList.add("is-invalid");
                const feedback = inputField.nextElementSibling;
                if (feedback && feedback.classList.contains("invalid-feedback")) {
                  feedback.textContent = responseData.errors[field][0];
                }
              }
            }
            
            errorMessage = Constants.MESSAGES.VALIDATION_ERROR;
          } else if (responseData.message) {
            errorMessage = responseData.message;
          }
        } else {
          errorMessage = await response.text();
        }
      } catch (jsonError) {
        console.warn("Failed to parse error response:", jsonError);
      }
      
      errorDiv.textContent = errorMessage;
      errorDiv.style.display = "block";
    }
  } catch (error) {
    console.error("Error processing article:", error);
    errorDiv.textContent = Constants.MESSAGES.UNEXPECTED_ERROR;
    errorDiv.style.display = "block";
  } finally {
    submitButton.disabled = false;
    updateSubmitButtonState();
  }
}
