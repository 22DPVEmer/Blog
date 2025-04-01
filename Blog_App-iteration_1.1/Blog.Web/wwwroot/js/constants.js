export const Constants = {
    MESSAGES: {
        SIZE_LIMIT_EXCEEDED: 'Featured image would exceed the size limit of 5MB',
        IMAGE_SIZE_LIMIT: 'Featured image size must not exceed 5MB',
        INVALID_IMAGE_TYPE: 'Only JPG, PNG, GIF, and WEBP images are allowed',
        ARTICLE_CREATED: 'Article created successfully!',
        ARTICLE_UPDATED: 'Article updated successfully!',
        PROCESS_ERROR: 'An error occurred while processing the article.',
        VALIDATION_ERROR: 'Validation errors:',
        UNEXPECTED_ERROR: 'An unexpected error occurred. Please try again.',
        CONTENT_REQUIRED: 'Content is required. Please enter article content.',
        CONTENT_IMAGES_LIMIT_EXCEEDED: 'Too many images. The total size of images in content must not exceed 5MB'
    },
    LIMITS: {
        MAX_TOTAL_SIZE: 5 * 1024 * 1024, // 5MB for featured image
        MAX_IMAGE_SIZE: 5 * 1024 * 1024, // 5MB
        MAX_CONTENT_IMAGES_SIZE: 5 * 1024 * 1024, // 5MB
        INTRO_MAX_LENGTH: 200
    },
    IMAGE_TYPES: ['image/jpeg', 'image/png', 'image/gif', 'image/webp']
};