// Add total size tracking
import { Constants } from './constants.js';

let totalSize = 0;
const MAX_TOTAL_SIZE = Constants.LIMITS.MAX_TOTAL_SIZE;

function updateSizeDisplay() {
    const sizeMB = (totalSize / 1024 / 1024).toFixed(2);
    const percentage = (totalSize / MAX_TOTAL_SIZE * 100).toFixed(1);
    
    document.getElementById('currentSize').textContent = sizeMB;
    const progressBar = document.getElementById('sizeProgress');
    progressBar.style.width = percentage + '%';
    progressBar.setAttribute('aria-valuenow', percentage);
    progressBar.textContent = percentage + '%';
    
    // Update progress bar color based on usage
    if (percentage > 90) {
        progressBar.classList.remove('bg-success', 'bg-warning');
        progressBar.classList.add('bg-danger');
    } else if (percentage > 70) {
        progressBar.classList.remove('bg-success', 'bg-danger');
        progressBar.classList.add('bg-warning');
    } else {
        progressBar.classList.remove('bg-warning', 'bg-danger');
        progressBar.classList.add('bg-success');
    }
}

// Initialize drag and drop functionality
const dragDropZone = document.getElementById('dragDropZone');
const fileInput = document.getElementById('FeaturedImage');
const imagePreview = document.getElementById('imagePreview');

['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
    dragDropZone.addEventListener(eventName, preventDefaults, false);
});

function preventDefaults(e) {
    e.preventDefault();
    e.stopPropagation();
}

['dragenter', 'dragover'].forEach(eventName => {
    dragDropZone.addEventListener(eventName, highlight, false);
});

['dragleave', 'drop'].forEach(eventName => {
    dragDropZone.addEventListener(eventName, unhighlight, false);
});

function highlight(e) {
    dragDropZone.classList.add('dragover');
}

function unhighlight(e) {
    dragDropZone.classList.remove('dragover');
}

dragDropZone.addEventListener('drop', handleDrop, false);
dragDropZone.addEventListener('click', () => fileInput.click());

function handleDrop(e) {
    const dt = e.dataTransfer;
    const file = dt.files[0];
    handleFile(file);
}

fileInput.addEventListener('change', function(e) {
    handleFile(e.target.files[0]);
});

function handleFile(file) {
    if (!file) return;

    if (totalSize + file.size > MAX_TOTAL_SIZE) {
        showError(Constants.MESSAGES.SIZE_LIMIT_EXCEEDED);
        return;
    }

    if (file.size > Constants.LIMITS.MAX_IMAGE_SIZE) {
        showError(Constants.MESSAGES.IMAGE_SIZE_LIMIT);
        return;
    }

    if (!Constants.IMAGE_TYPES.includes(file.type)) {
        showError(Constants.MESSAGES.INVALID_IMAGE_TYPE);
        return;
    }

    // Update total size and preview
    totalSize += file.size;
    updateSizeDisplay();
    
    const reader = new FileReader();
    reader.onload = function(e) {
        imagePreview.src = e.target.result;
        imagePreview.style.display = 'block';
        dragDropZone.style.display = 'none';
        document.querySelector('.image-actions').style.display = 'flex';
    }
    reader.readAsDataURL(file);
    
    // Update the file input
    const dT = new DataTransfer();
    dT.items.add(file);
    fileInput.files = dT.files;
}

function showError(message) {
    const errorDiv = document.getElementById('errorMessages');
    errorDiv.textContent = message;
    errorDiv.style.display = 'block';
    setTimeout(() => {
        errorDiv.style.display = 'none';
    }, 3000);
}

// Summary character count
const summaryTextarea = document.getElementById('Intro');
const charCount = document.getElementById('introCharCount');

summaryTextarea.addEventListener('input', function() {
    const count = this.value.length;
    charCount.textContent = count;
    
    if (count > Constants.LIMITS.INTRO_MAX_LENGTH - 10) {
        charCount.style.color = '#dc3545';
    } else if (count > Constants.LIMITS.INTRO_MAX_LENGTH - 20) {
        charCount.style.color = '#ffc107';
    } else {
        charCount.style.color = '#6c757d';
    }
});

// Initialize TinyMCE
tinymce.init({
    selector: '#Content',
    plugins: 'anchor autolink charmap codesample emoticons image link lists media searchreplace table visualblocks wordcount checklist mediaembed casechange export formatpainter pageembed permanentpen footnotes advtemplate advtable advcode editimage tableofcontents powerpaste tinymcespellchecker autocorrect typography inlinecss',
    toolbar: 'undo redo | blocks fontfamily fontsize | bold italic underline strikethrough | link image media table | align lineheight | checklist numlist bullist indent outdent | emoticons charmap | removeformat',
    height: 500,
    menubar: true,
    image_advtab: true,
    image_caption: true,
    image_dimensions: false,
    image_class_list: [
        { title: 'Responsive', value: 'img-fluid' },
        { title: 'Full Width', value: 'img-fluid w-100' },
        { title: 'Centered', value: 'img-fluid mx-auto d-block' }
    ],
    file_picker_types: 'image',
    images_upload_handler: async function (blobInfo, progress) {
        return new Promise((resolve, reject) => {
            const file = blobInfo.blob();
            
            if (totalSize + file.size > MAX_TOTAL_SIZE) {
                reject(Constants.MESSAGES.SIZE_LIMIT_EXCEEDED);
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
            formData.append('file', file);
            
            const xhr = new XMLHttpRequest();
            xhr.open('POST', '/Articles/UploadImage');
            
            const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
            xhr.setRequestHeader('RequestVerificationToken', token);
            
            xhr.upload.onprogress = (e) => {
                progress(e.loaded / e.total * 100);
            };
            
            xhr.onload = function() {
                if (xhr.status === 200) {
                    const response = JSON.parse(xhr.responseText);
                    totalSize = response.currentSize;
                    updateSizeDisplay();
                    resolve(response.imageUrl);
                } else {
                    try {
                        const response = JSON.parse(xhr.responseText);
                        reject(response.message || 'HTTP Error: ' + xhr.status);
                    } catch {
                        reject('HTTP Error: ' + xhr.status);
                    }
                }
            };
            
            xhr.onerror = function() {
                reject('Image upload failed due to a network error.');
            };
            
            xhr.send(formData);
        });
    },
    paste_data_images: true,
    automatic_uploads: true,
    images_reuse_filename: false,
    setup: function (editor) {
        editor.on('change', function () {
            editor.save();
        });

        editor.on('paste', function(e) {
            const items = (e.clipboardData || e.originalEvent.clipboardData).items;
            for (let i = 0; i < items.length; i++) {
                if (items[i].type.indexOf('image') !== -1) {
                    const file = items[i].getAsFile();
                    
                    if (totalSize + file.size > MAX_TOTAL_SIZE) {
                        editor.notificationManager.open({
                            text: `Adding this image would exceed the total size limit of 10MB (Current: ${(totalSize / 1024 / 1024).toFixed(2)}MB)`,
                            type: 'error'
                        });
                        return;
                    }

                    const maxSize = 5 * 1024 * 1024;
                    if (file.size > maxSize) {
                        editor.notificationManager.open({
                            text: 'Pasted image size must not exceed 5MB',
                            type: 'error'
                        });
                        return;
                    }

                    const allowedTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp'];
                    if (!allowedTypes.includes(file.type)) {
                        editor.notificationManager.open({
                            text: 'Only JPG, PNG, GIF, and WEBP images are allowed',
                            type: 'error'
                        });
                        return;
                    }

                    const loadingId = 'loading-' + Date.now();
                    editor.insertContent(`<img id="${loadingId}" src="data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7" class="loading-placeholder" />`);
                    
                    const formData = new FormData();
                    formData.append('file', file);
                    
                    fetch('/Articles/UploadImage', {
                        method: 'POST',
                        headers: {
                            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                        },
                        body: formData
                    })
                    .then(response => response.json())
                    .then(data => {
                        totalSize = data.currentSize;
                        updateSizeDisplay();
                        
                        const img = editor.dom.get(loadingId);
                        if (img) {
                            editor.dom.setAttrib(img, 'src', data.imageUrl);
                            editor.dom.setAttrib(img, 'class', 'img-fluid');
                            editor.dom.setAttrib(img, 'id', '');
                        }
                    })
                    .catch(error => {
                        const img = editor.dom.get(loadingId);
                        if (img) {
                            editor.dom.remove(img);
                        }
                        editor.notificationManager.open({
                            text: error.message || 'Failed to upload image',
                            type: 'error'
                        });
                    });
                }
            }
        });
    }
});

// Form submission
const formElement = document.getElementById('createArticleForm') || document.getElementById('editArticleForm');
// In the DOMContentLoaded event handler
document.addEventListener('DOMContentLoaded', function() {
    const isEditMode = window.isEditMode || false;
    
    if (isEditMode && window.initialFeaturedImage) {
        // Remove the fetch call and set default size
        imagePreview.style.display = 'block';
        dragDropZone.style.display = 'none';
        document.querySelector('.image-actions').style.display = 'flex';
        totalSize = 1024 * 1024; // Set default 1MB for existing images
        updateSizeDisplay();
    }
});

// In the form submission handler
if (formElement) {
    formElement.addEventListener('submit', async function(e) {
        e.preventDefault();
        
        const submitButton = document.getElementById('submitButton');
        const errorDiv = document.getElementById('errorMessages');
        const successDiv = document.getElementById('successMessage');
        const editor = tinymce.get('Content');
        const featuredImageFile = document.getElementById('FeaturedImage').files[0];

        try {
            submitButton.disabled = true;
            errorDiv.style.display = 'none';
            successDiv.style.display = 'none';

            const formData = new FormData(formElement);
            formData.set('Content', editor.getContent());
            
            if (featuredImageFile) {
                formData.set('FeaturedImageFile', featuredImageFile);
            }

            const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
            const articleId = document.getElementById('articleId')?.value; // Add a hidden input for article ID in your form
            const endpoint = window.isEditMode ? `/Articles/Edit/${articleId}` : '/Articles/Create';

            const response = await fetch(endpoint, {
                method: 'POST',
                headers: {
                    'RequestVerificationToken': token,
                    'Accept': 'application/json'
                },
                body: formData
            });

            if (response.ok) {
                successDiv.textContent = window.isEditMode ? 'Article updated successfully!' : 'Article created successfully!';
                successDiv.style.display = 'block';
                setTimeout(() => {
                    window.location.href = '/Articles';
                }, 1000);
            } else {
                let errorMessage = 'An error occurred while processing the article.';
                try {
                    // Check if response is JSON before parsing
                    const contentType = response.headers.get('content-type');
                    if (contentType && contentType.includes('application/json')) {
                        const responseData = await response.json();
                        if (response.status === 400 && responseData.errors) {
                            errorMessage = 'Validation errors:';
                            Object.keys(responseData.errors).forEach(key => {
                                errorMessage += `\n${key}: ${responseData.errors[key].join(', ')}`;
                            });
                        } else if (responseData.message) {
                            errorMessage = responseData.message;
                        }
                    } else {
                        errorMessage = await response.text();
                    }
                } catch (jsonError) {
                    console.warn('Failed to parse error response:', jsonError);
                }
                errorDiv.textContent = errorMessage;
                errorDiv.style.display = 'block';
            }
        } catch (error) {
            console.error('Error processing article:', error);
            errorDiv.textContent = 'An unexpected error occurred. Please try again.';
            errorDiv.style.display = 'block';
        } finally {
            submitButton.disabled = false;
        }
    });
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    const isEditMode = window.isEditMode || false;
    
    // If in edit mode and there's an initial image, update the size tracking
    if (isEditMode && window.initialFeaturedImage) {
        // Show the image preview and actions
        imagePreview.style.display = 'block';
        dragDropZone.style.display = 'none';
        document.querySelector('.image-actions').style.display = 'flex';
        
        // Fetch the image size and update total size
        if (window.initialFeaturedImage.startsWith('http')) {
            fetch(window.initialFeaturedImage)
                .then(response => {
                    if (response.ok) {
                        const size = parseInt(response.headers.get('content-length') || '0');
                        totalSize = size;
                        updateSizeDisplay();
                    }
                })
                .catch(error => {
                    console.warn('Could not fetch image size:', error);
                    // Set a default size for local images
                    totalSize = 1024 * 1024; // 1MB default
                    updateSizeDisplay();
                });
        } else {
            // For local images, set a default size
            totalSize = 1024 * 1024; // 1MB default
            updateSizeDisplay();
        }
    }

    // Add remove image button functionality
    document.getElementById('removeImageBtn').addEventListener('click', function() {
        document.getElementById('dragDropZone').style.display = 'block';
        document.getElementById('imagePreview').style.display = 'none';
        document.querySelector('.image-actions').style.display = 'none';
        document.getElementById('FeaturedImage').value = '';
        imagePreview.src = '#';
        totalSize = 0;
        updateSizeDisplay();
    });

    // Update change image button functionality
    document.getElementById('changeImageBtn').addEventListener('click', function() {
        document.getElementById('dragDropZone').style.display = 'block';
        document.getElementById('imagePreview').style.display = 'none';
        document.querySelector('.image-actions').style.display = 'none';
    });
});