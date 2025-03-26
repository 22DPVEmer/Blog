// API Endpoints
const API_ENDPOINTS = {
    PERMISSIONS: {
        PROCESS: '/Permissions/Process'
    },
    ARTICLE_VOTES: {
        VOTE: '/api/articlevotes/vote'
    }
};

// Export for use in other files
if (typeof module !== 'undefined' && module.exports) {
    module.exports = API_ENDPOINTS;
} 